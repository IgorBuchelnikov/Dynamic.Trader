using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading;
using System.Windows.Threading;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Model;

namespace Trader.Domain.Services
{
	public class TradePriceUpdateJob : IDisposable
	{
		private IDisposable _cleanUp;

		public TradePriceUpdateJob(ITradeService tradeService, IMarketDataService marketDataService)
		{
			Consumer consumer = new Consumer();

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
			.For(consumer);

			_cleanUp = new CompositeDisposable(consumer);
		}

		public void Dispose()
		{
			_cleanUp.Dispose();
		}
	}
}
