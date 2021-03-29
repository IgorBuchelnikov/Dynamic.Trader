using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Threading;
using ObservableComputations;
using Trader.Domain.Model;

namespace Trader.Domain.Services
{
	public class TradePriceUpdateJob : IDisposable
	{
		private readonly OcConsumer _consumer = new OcConsumer();
		private readonly  OcDispatcher _backgroundOcDispatcher;

		public TradePriceUpdateJob(ITradeService tradeService, IMarketDataService marketDataService, Dispatcher dispatcher, OcDispatcher backgroundOcDispatcher)
		{
			_backgroundOcDispatcher = backgroundOcDispatcher;

			tradeService.All
			.Grouping(t => t.CurrencyPair)
			.CollectionProcessing(
				(newTradeGroups, @this) =>
				{
					IDisposable[] disposables = new IDisposable[newTradeGroups.Length];
					for (var index = 0; index < newTradeGroups.Length; index++)
					{
						var tradesGroup = newTradeGroups[index];
						IReadScalar<MarketData> observableMarketData =
							marketDataService.Get(tradesGroup.Key);

						OcConsumer consumer1 = new OcConsumer();

						//DataHasChanged
						tradesGroup.CollectionProcessing(
							(newTrades, this1) =>
							{
								Trade[] newTradesCopy = newTrades.ToArray();
							  
								dispatcher.Invoke(() =>
								{
									foreach (Trade trade in newTradesCopy)
										trade.MarketPrice = observableMarketData.Value.Bid;
								}, DispatcherPriority.Background);
								
							})
						.For(consumer1);

						observableMarketData.Binding((newMarketData, _) =>
						{
							decimal bid = observableMarketData.Value.Bid;
							Trade[] tradesGroupCopy = tradesGroup.ToArray();
							dispatcher.InvokeAsync(() =>
							{
								tradesGroupCopy.ForEach(trade =>
									trade.MarketPrice = bid);
							}, DispatcherPriority.Background);
						}).For(consumer1);

						disposables[index] = consumer1;
					}

					return disposables;
				})
			.CollectionDisposing()
			.For(_consumer);
		}

		public void Dispose()
		{
			_backgroundOcDispatcher.Invoke(() =>
				_consumer.Dispose());
		}
	}
}
