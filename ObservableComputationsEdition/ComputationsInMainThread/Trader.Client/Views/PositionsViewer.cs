using System;
using System.Collections.ObjectModel;
using ObservableComputations;
using Trader.Domain.Model;
using Trader.Domain.Services;

namespace Trader.Client.Views
{
	public class PositionsViewer : IDisposable
	{
		private readonly Consumer _consumer = new Consumer();

		public PositionsViewer(ITradeService tradeService)
		{
			Data = tradeService.Live
				.Grouping(trade => trade.CurrencyPair)
				.Selecting(group => new CurrencyPairPosition(group, _consumer))
				.Ordering(p => p.CurrencyPair)
				.For(_consumer);
		}

		public ObservableCollection<CurrencyPairPosition> Data { get; }

		public void Dispose()
		{
			_consumer.Dispose();
		}
	}
}
