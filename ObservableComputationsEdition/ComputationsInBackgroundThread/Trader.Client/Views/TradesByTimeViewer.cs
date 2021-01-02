using System;
using System.Collections.ObjectModel;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Model;
using Trader.Domain.Services;

namespace Trader.Client.Views
{
	public class TradesByTimeViewer : AbstractNotifyPropertyChanged, IDisposable
	{
		private readonly OcConsumer _consumer = new OcConsumer();

		public TradesByTimeViewer(ITradeService tradeService, OcDispatcher backgroundOcDispatcher, WpfOcDispatcher wpfOcDispatcher)
		{
			Data = tradeService.All
				.Grouping(trade => trade.Age)
				.Selecting(group => new TradesByTime(group, _consumer, backgroundOcDispatcher, wpfOcDispatcher))
				.Ordering(t => t.Period)
				.CollectionDispatching(wpfOcDispatcher, backgroundOcDispatcher, 0, 1)			   
				.For(_consumer);
		}

		public ObservableCollection<TradesByTime> Data { get; }

		public void Dispose()
		{
			_consumer.Dispose();
		}
	}
}