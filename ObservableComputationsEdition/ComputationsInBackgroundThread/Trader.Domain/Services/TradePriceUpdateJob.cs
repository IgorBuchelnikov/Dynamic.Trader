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
			.CollectionItemProcessing(
				(newTradeGroup, _) =>
				{
					IReadScalar<MarketData> observableMarketData =
						marketDataService.Get(newTradeGroup.Key);

					OcConsumer consumer1 = new OcConsumer();

					//DataHasChanged
					newTradeGroup.CollectionItemsProcessing(
						(newTrades, __) =>
						{
							Trade[] newTradesCopy = newTrades.ToArray();
						  
							dispatcher.Invoke(() =>
							{
								foreach (Trade trade in newTradesCopy)
									trade.MarketPrice = observableMarketData.Value.Bid;
							}, DispatcherPriority.Background);
							
						})
					.For(consumer1);

					observableMarketData.Binding((newMarketData, __) =>
					{
						decimal bid = observableMarketData.Value.Bid;
						Trade[] tradesGroupCopy = newTradeGroup.ToArray();
						dispatcher.Invoke(() =>
						{
							tradesGroupCopy.ForEach(trade =>
								trade.MarketPrice = bid);
						}, DispatcherPriority.Background);
					}).For(consumer1);

					return consumer1;
				})
			.CollectionDisposing()
			.For(_consumer);
		}

		public void Dispose()
		{
			_backgroundOcDispatcher.InvokeAsync(() =>
				_consumer.Dispose());
		}
	}
}
