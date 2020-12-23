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

		public TradesByTimeViewer(ITradeService tradeService)
		{
			_data = tradeService.All
				.Grouping(trade => trade.Age)
				.Selecting(group => new TradesByTime(group, _consumer))
				.Ordering(t => t.Period)
				.For(_consumer);
		}

		public ObservableCollection<TradesByTime> Data => _data;

		public void Dispose()
		{
			_consumer.Dispose();
		}

	}
}