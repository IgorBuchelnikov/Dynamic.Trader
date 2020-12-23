using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Model;
using Trader.Domain.Services;

namespace Trader.Client.Views
{
	public class TradesByTimeViewer : AbstractNotifyPropertyChanged, IDisposable
	{
		private readonly Consumer _consumer = new Consumer();
		private readonly ObservableCollection<TradesByTime> _data;

		public TradesByTimeViewer(ITradeService tradeService, OcDispatcher backgroundOcDispatcher, WpfOcDispatcher wpfOcDispatcher)
		{
			_data = tradeService.All
				.Grouping(trade => trade.Age)
				.Selecting(group => new TradesByTime(group, _consumer, backgroundOcDispatcher, wpfOcDispatcher))
				.Ordering(t => t.Period)
				.CollectionDispatching(wpfOcDispatcher, backgroundOcDispatcher, new DispatcherPriorities(1, 0))			   
				.For(_consumer);
		}

		public ObservableCollection<TradesByTime> Data => _data;

		public void Dispose()
		{
			_consumer.Dispose();
		}

	}
}