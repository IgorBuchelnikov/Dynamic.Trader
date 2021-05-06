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
			.CollectionItemProcessing(
				(newTradeGroup, _) =>
				{
					IReadScalar<MarketData> observableMarketData =
							marketDataService.Get(newTradeGroup.Key);

					OcConsumer consumer1 = new OcConsumer();

					//DataHasChanged
					newTradeGroup.CollectionItemProcessing(
						(newTrade, __) =>
							newTrade.MarketPrice = observableMarketData.Value.Bid)
					.For(consumer1);

					observableMarketData.Binding((newMarketData, __) =>
					{
						decimal bid = observableMarketData.Value.Bid;

						newTradeGroup.ForEach(trade =>																
							trade.MarketPrice = bid);
				 
					}).For(consumer1); 

					return consumer1;
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
