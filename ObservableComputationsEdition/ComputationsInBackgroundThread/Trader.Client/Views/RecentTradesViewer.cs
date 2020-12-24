using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Model;
using Trader.Domain.Services;
using Dispatcher = System.Windows.Threading.Dispatcher;

namespace Trader.Client.Views
{
	public class RecentTradesViewer : AbstractNotifyPropertyChanged, IDisposable
	{
		private readonly Consumer _consumer = new Consumer();

		public RecentTradesViewer(ITradeService tradeService, OcDispatcher backgroundOcDispatcher, WpfOcDispatcher wpfOcDispatcher)
		{
			DateTime initialDateTime = DateTime.Now;

			Data = tradeService.All
				.Filtering(t => !t.Expired && t.Timestamp > initialDateTime)
				.Ordering(t => t.Timestamp, ListSortDirection.Descending)
				.Selecting(t => new TradeProxy(t))
				.CollectionDisposing()
				.CollectionDispatching(wpfOcDispatcher, backgroundOcDispatcher, new DispatcherPriorities(1, 0))
				.For(_consumer);
		}

		public ObservableCollection<TradeProxy> Data { get; }

		public void Dispose()
		{
			_consumer.Dispose();
		}

	}
}