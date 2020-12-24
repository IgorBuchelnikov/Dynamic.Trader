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
		private readonly Consumer _consumer = new Consumer();

		public TradesByTimeViewer(ITradeService tradeService)
		{
			Data = tradeService.All
				.Grouping(trade => trade.Age)
				.Selecting(group => new TradesByTime(group, _consumer))
				.Ordering(t => t.Period)
				.For(_consumer);
		}

		public ObservableCollection<TradesByTime> Data { get; }

		public void Dispose()
		{
			_consumer.Dispose();
		}
	}
}