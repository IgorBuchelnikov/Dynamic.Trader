﻿using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using ObservableComputations;
using Trader.Domain.Model;

namespace Trader.Domain.Services
{
	public class TradePriceUpdateJob : IDisposable
	{
		private Consumer _consumer = new Consumer();

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

						Consumer consumer1 = new Consumer();

						//DataHasChanged
						tradesGroup.CollectionProcessing(
							(newTrades, @this1) =>
							{
								foreach (Trade trade in newTrades)
									trade.MarketPrice = observableMarketData.Value.Bid;
								
							})
						.For(consumer1);

						Binding<MarketData> binding = 
							observableMarketData.Binding(newMarketData =>
							{
								decimal bid = observableMarketData.Value.Bid;

								tradesGroup.ForEach(trade =>																
									trade.MarketPrice = bid);
						 
							}); 

						disposables[index] = new CompositeDisposable(consumer1, binding);
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
