using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Model;
using Trader.Domain.Services;

namespace Trader.Client.Views
{
    public class NearToMarketViewer : AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly Consumer _consumer = new Consumer();
        private readonly ObservableCollection<TradeProxy> _data;
        private double _nearToMarketPercent = 0.05D;

        public NearToMarketViewer(INearToMarketService nearToMarketService)
        {
	        _data = nearToMarketService.Query(() => (decimal) NearToMarketPercent)
		        .Ordering(t => t.Timestamp, ListSortDirection.Descending)
		        .Selecting(t => new TradeProxy(t))
                .CollectionDisposing()
                .For(_consumer);
        }

        public double NearToMarketPercent
        {
            get => _nearToMarketPercent;
            set => SetAndRaise(ref _nearToMarketPercent, value);
        }

        public ObservableCollection<TradeProxy> Data => _data;

        public void Dispose()
        {
            _consumer.Dispose();
        }
    }
}