using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Disposables;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Model;
using Trader.Domain.Services;

namespace Trader.Client.Views
{
	public class VisibleRowsViewer : AbstractNotifyPropertyChanged, IDisposable
	{
		private readonly IDisposable _cleanUp;
		private readonly ObservableCollection<TradeProxy> _data;

		public VisibleRowsViewer(ITradeService tradeService, OcDispatcher backgroundOcDispatcher, WpfOcDispatcher wpfOcDispatcher)
		{
			Consumer consumer = new Consumer();
			_data = tradeService.All
				.Ordering(t => t.Timestamp, ListSortDirection.Descending)
				.Selecting(trade => new TradeProxy(trade))
				.CollectionDisposing()
				.CollectionDispatching(wpfOcDispatcher, backgroundOcDispatcher, new DispatcherPriorities(1, 0))
				.For(consumer);

			_cleanUp = new CompositeDisposable(consumer);
		}

		public ObservableCollection<TradeProxy> Data => _data;

		public void Dispose()
		{
			_cleanUp.Dispose();
		}
	}
}