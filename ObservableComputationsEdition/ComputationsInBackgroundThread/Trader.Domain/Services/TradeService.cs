using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Disposables;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Model;

namespace Trader.Domain.Services
{
	public class TradeService : ITradeService, IDisposable
	{
		private readonly ILogger _logger;
		private readonly TradeGenerator _tradeGenerator;
		private readonly OcDispatcher _backgroundOcDispatcher;
		private readonly IDisposable _cleanup;
		private readonly Consumer _consumer = new Consumer();

		public TradeService(ILogger logger, TradeGenerator tradeGenerator, OcDispatcher backgroundOcDispatcher)
		{
			_logger = logger;
			_tradeGenerator = tradeGenerator;
			_backgroundOcDispatcher = backgroundOcDispatcher;

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
					_backgroundOcDispatcher.Invoke(() => All.Add(trade));
			}, RandomInterval);

			List<Trade> closedTrades = new List<Trade>();
			//close a random number of trades at a random interval
			RecurringAction tradeCloser = new RecurringAction(() =>
			{
				var number = random.Next(1, 2);
				for (int i = 1; i <= number; i++)
				_backgroundOcDispatcher.Invoke(() =>
				{
					Trade trade = All[random.Next(0, All.Count - 1)];
					trade.Status = TradeStatus.Closed;
					trade.CloseTimestamp = DateTime.Now;
					closedTrades.Add(trade);
				});
			}, RandomInterval);


			//expire closed items from the cache to avoid unbounded data
			RecurringAction tradeRemover = new RecurringAction(() =>
			{
				_backgroundOcDispatcher.Invoke(() =>
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
				});
			}, () => TimeSpan.FromMinutes(1));



			//log changes
			OcDispatcher logPropertyChangedOcDispatcher = new OcDispatcher();
			LogChanges(logPropertyChangedOcDispatcher);

			_cleanup = new CompositeDisposable(
				_consumer, tradeEmitter, tradeCloser, logPropertyChangedOcDispatcher, tradeRemover);
		}

		private void LogChanges(OcDispatcher logPropertyChangedOcDispatcher)
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

			All
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

						//void tradeOnPropertyChanged(object sender, PropertyChangedEventArgs args) => 
						//	logPropertyChangedOcDispatcher.BeginInvoke(() => log(trade));

						//trade.PropertyChanged += tradeOnPropertyChanged;
						//tradeOnPropertyChangedHandlers[index] = tradeOnPropertyChanged;
					}

					return tradeOnPropertyChangedHandlers;
				},
				(oldTrades, _, tradeOnPropertyChangedHandlers) =>
				{
					//for (var index = 0; index < tradeOnPropertyChangedHandlers.Length; index++)
					//{
					//	Trade trade = oldTrades[index];
					//	if (trade != null) 
					//		trade.PropertyChanged -= tradeOnPropertyChangedHandlers[index];
					//}
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