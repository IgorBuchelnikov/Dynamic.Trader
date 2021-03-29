using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using ObservableComputations;
using Trader.Domain.Model;

namespace Trader.Domain.Services
{
	public class TradePriceUpdateJob : IDisposable
	{
		private readonly OcConsumer _consumer = new OcConsumer();

		public TradePriceUpdateJob(ITradeService tradeService, IMarketDataService marketDataService)
		{
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
							(newTrades, @this1) =>
							{
								foreach (Trade trade in newTrades)
									trade.MarketPrice = observableMarketData.Value.Bid;
								
							})
						.For(consumer1);

						observableMarketData.Binding((newMarketData, _) =>
						{
							decimal bid = observableMarketData.Value.Bid;

							tradesGroup.ForEach(trade =>																
								trade.MarketPrice = bid);
					 
						}).For(consumer1); 

						disposables[index] = new CompositeDisposable(consumer1);
					}

					return disposables;
				})
			.CollectionDisposing()
			.For(_consumer);
		}

		public void Dispose()
		{
			_consumer.Dispose();
		}
	}
}
