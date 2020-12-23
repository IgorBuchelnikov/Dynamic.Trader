using System.Collections.ObjectModel;
using System.ComponentModel;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Model;
using Trader.Domain.Services;

namespace Trader.Client.Views
{
	public class VisibleRowsViewer : AbstractNotifyPropertyChanged, IDisposable
	{
		private readonly Consumer _consumer = new Consumer();
		private readonly ObservableCollection<TradeProxy> _data;

		public VisibleRowsViewer(ITradeService tradeService)
		{
			_data = tradeService.All
				.Ordering(t => t.Timestamp, ListSortDirection.Descending)
				.Selecting(trade => new TradeProxy(trade))
				.CollectionDisposing()
				.For(_consumer);
		}

		public ObservableCollection<TradeProxy> Data => _data;
		
		public void Dispose()
		{
			_consumer.Dispose();
		}
	}
}