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
        private readonly IDisposable _cleanUp;
        private readonly ObservableCollection<TradeProxy> _data;
        private readonly ILogger _logger;

        public RecentTradesViewer(ILogger logger, ITradeService tradeService, OcDispatcher backgroundOcDispatcher, WpfOcDispatcher wpfOcDispatcher)
        {
            _logger = logger;
			DateTime initialDateTime = DateTime.Now;
            Consumer consumer = new Consumer();
			_data = tradeService.All
				.Filtering(t => !t.Expired && t.Timestamp > initialDateTime)
				.Ordering(t => t.Timestamp, ListSortDirection.Descending)
				.Selecting(t => new TradeProxy(t))
                .CollectionDisposing()
                .CollectionDispatching(wpfOcDispatcher, backgroundOcDispatcher, new DispatcherPriorities(1, 0))
                .For(consumer);

            _cleanUp = consumer;
        }

        public ObservableCollection<TradeProxy> Data => _data;

        public void Dispose()
        {
            _cleanUp.Dispose();
        }

    }
}