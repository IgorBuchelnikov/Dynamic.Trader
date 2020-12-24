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
		private readonly ObservableCollection<Trade> _all;
		private readonly ObservableCollection<Trade> _live;
		private readonly IDisposable _cleanup;
		private readonly Consumer _consumer = new Consumer();
		private bool _disposed;

		public TradeService(ILogger logger, TradeGenerator tradeGenerator, WpfOcDispatcher wpfOcDispatcher, Dispatcher dispatcher)
		{
			_logger = logger;
			_tradeGenerator = tradeGenerator;
			_dispatcher = dispatcher;

			_all = new ObservableCollection<Trade>(_tradeGenerator.Generate(5_000, true));
			_live = _all.Filtering(t => t.Status == TradeStatus.Live).For(_consumer);

			var random = new Random();
			TimeSpan RandomInterval() => TimeSpan.FromMilliseconds(random.Next(2500, 5000));

			// create a random number of trades at a random interval
			RecurringAction tradeEmitter = new RecurringAction(() =>
			{
				var number = random.Next(1, 5);
				var trades = _tradeGenerator.Generate(number);

				foreach (Trade trade in trades)
					_dispatcher.Invoke(() => _all.Add(trade), DispatcherPriority.Background);
			}, RandomInterval);

			List<Trade> closedTrades = new List<Trade>();
			//close a random number of trades at a random interval
			RecurringAction tradeCloser = new RecurringAction(() =>
			{
				var number = random.Next(1, 2);
				for (int i = 1; i <= number; i++)
					_dispatcher.Invoke(() =>
					{
						Trade trade = _all[random.Next(0, _all.Count - 1)];
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
							_all.Remove(closedTrade);
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
				.CollectionProcessing(
				(newTrades, processing) =>
				{
					PropertyChangedEventHandler[] tradeOnPropertyChangedHandlers =
						new PropertyChangedEventHandler[newTrades.Length];
					for (var index = 0; index < newTrades.Length; index++)
					{
						Trade trade = newTrades[index];

						if (!processing.Initializing)
							log(trade);

						PropertyChangedEventHandler tradeOnPropertyChanged =
							(sender, args) => logPropertyChangedOcDispatcher.BeginInvoke(() => log(trade));
						trade.PropertyChanged += tradeOnPropertyChanged;
						tradeOnPropertyChangedHandlers[index] = tradeOnPropertyChanged;
					}

					return tradeOnPropertyChangedHandlers;
				},
				(oldTrades, _, tradeOnPropertyChangedHandlers) =>
				{
					for (var index = 0; index < tradeOnPropertyChangedHandlers.Length; index++)
					{
						Trade trade = oldTrades[index];
						if (trade != null)
							trade.PropertyChanged -= tradeOnPropertyChangedHandlers[index];
					}
				}).For(_consumer);
		}

		public ObservableCollection<Trade> All => _all;
		public ObservableCollection<Trade> Live => _live;

		public void Dispose()
		{
			_cleanup.Dispose();
		}
	}
}