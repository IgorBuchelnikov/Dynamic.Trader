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
        private readonly IDisposable _cleanUp;
        private readonly ObservableCollection<TradesByTime> _data;

        public TradesByTimeViewer(ITradeService tradeService)
        {
            Consumer consumer = new Consumer();
            _data = tradeService.All
                .Grouping(trade => trade.Age)
                .Selecting(group => new TradesByTime(group, consumer))
                .Ordering(t => t.Period)
                .For(consumer);

            _cleanUp = new CompositeDisposable(consumer);
        }

        public ObservableCollection<TradesByTime> Data => _data;

        public void Dispose()
        {
            _cleanUp.Dispose();
        }

    }
}