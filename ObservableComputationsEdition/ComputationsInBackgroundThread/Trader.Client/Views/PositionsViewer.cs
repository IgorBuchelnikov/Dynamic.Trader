using System;
using System.Collections.ObjectModel;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Model;
using Trader.Domain.Services;

namespace Trader.Client.Views
{
	public class PositionsViewer : IDisposable
	{
		private readonly OcConsumer _consumer = new OcConsumer();

		public PositionsViewer(ITradeService tradeService, OcDispatcher backgroundOcDispatcher, WpfOcDispatcher wpfOcDispatcher)
		{
			Data = tradeService.Live
				.Grouping(trade => trade.CurrencyPair)
				.Selecting(group => new CurrencyPairPosition(group, _consumer))
				.Ordering(p => p.CurrencyPair)
				.CollectionDispatching(wpfOcDispatcher, backgroundOcDispatcher, 0, 1)
				.For(_consumer);
		}

		public ObservableCollection<CurrencyPairPosition> Data { get; }

		public void Dispose()
		{
			_consumer.Dispose();
		}
	}
}
