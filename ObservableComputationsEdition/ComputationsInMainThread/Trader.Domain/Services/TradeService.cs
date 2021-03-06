using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Windows.Threading;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Model;

namespace Trader.Domain.Services
{
	public class TradeService : ITradeService, IDisposable
	{
		private readonly ILogger _logger;
		private readonly TradeGenerator _tradeGenerator;
		private readonly Dispatcher _dispatcher;
		private readonly IDisposable _cleanup;
		private readonly OcConsumer _consumer = new OcConsumer();

		public TradeService(ILogger logger, TradeGenerator tradeGenerator, WpfOcDispatcher wpfOcDispatcher, Dispatcher dispatcher)
		{
			_logger = logger;
			_tradeGenerator = tradeGenerator;
			_dispatcher = dispatcher;

			All = new ObservableCollection<Trade>(_tradeGenerator.Generate(5_000, true));
			Live = All.Filtering(t => t.Status == TradeStatus.Live).For(_consumer);

			var random = new Random();
			TimeSpan RandomInterval() => TimeSpan.FromMilliseconds(random.Next(2500, 5000));

			// create a random number of trades at a random interval
			RecurringAction tradeEmitter = new RecurringAction(() =>
			{
				var number = random.Next(1, 5);
				var trades = _tradeGenerator.Generate(number);

				foreach (Trade trade in trades)
					_dispatcher.Invoke(() => All.Add(trade), DispatcherPriority.Background);
			}, RandomInterval);

			List<Trade> closedTrades = new List<Trade>();
			//close a random number of trades at a random interval
			RecurringAction tradeCloser = new RecurringAction(() =>
			{
				var number = random.Next(1, 2);
				for (int i = 1; i <= number; i++)
					_dispatcher.Invoke(() =>
					{
						Trade trade = All[random.Next(0, All.Count - 1)];
						trade.Status = TradeStatus.Closed;
						trade.CloseTimestamp = DateTime.Now;
						closedTrades.Add(trade);
					}, DispatcherPriority.Background);
			}, RandomInterval);


			//expire closed items from the cache to avoid unbounded data
			RecurringAction tradeRemover = new RecurringAction(() =>
			{
				_dispatcher.Invoke(() =>
				{
					for (var index = closedTrades.Count - 1; index >= 0; index--)
					{
						Trade closedTrade = closedTrades[index];
						if ((DateTime.Now - closedTrade.CloseTimestamp).Minutes >= 1)
						{
							All.Remove(closedTrade);
							closedTrades.RemoveAt(index);
						}
					}
				}, DispatcherPriority.Background);
			}, () => TimeSpan.FromMinutes(1));

			//log changes
			OcDispatcher logOcDispatcher = new OcDispatcher();
			OcDispatcher logPropertyChangedOcDispatcher = new OcDispatcher();
			LogChanges(wpfOcDispatcher, logOcDispatcher, logPropertyChangedOcDispatcher);

			_cleanup = new CompositeDisposable(
				_consumer, tradeEmitter, tradeCloser, logOcDispatcher, logPropertyChangedOcDispatcher, tradeRemover);
		}

		private void LogChanges(WpfOcDispatcher wpfOcDispatcher, OcDispatcher logOcDispatcher, OcDispatcher logPropertyChangedOcDispatcher)
		{
			const string messageTemplate = "{0} {1} {2} ({4}). Status = {3}";

			void log(Trade trade)
			{
				_logger.Info(
					string.Format(messageTemplate,
						trade.BuyOrSell,
						trade.Amount,
						trade.CurrencyPair,
						trade.Status,
						trade.Customer));
			}

			All.CollectionDispatching(logOcDispatcher, wpfOcDispatcher)
				.CollectionItemProcessing(
					(newTrade, processing) =>
					{
						if (!processing.ActivationInProgress)
								log(newTrade);
					}).For(_consumer);
		}

		public ObservableCollection<Trade> All { get; }

		public ObservableCollection<Trade> Live { get; }

		public void Dispose()
		{
			_cleanup.Dispose();
		}
	}
}