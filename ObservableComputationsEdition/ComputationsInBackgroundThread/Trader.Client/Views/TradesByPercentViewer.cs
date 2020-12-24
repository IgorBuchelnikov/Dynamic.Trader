using System;
using System.Collections.ObjectModel;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Services;

namespace Trader.Client.Views
{
	public class TradesByPercentViewer : AbstractNotifyPropertyChanged, IDisposable
	{
		private readonly Consumer _consumer = new Consumer();

		public TradesByPercentViewer(INearToMarketService nearToMarketService, OcDispatcher backgroundOcDispatcher, WpfOcDispatcher wpfOcDispatcher)
		{
			Data = 
				nearToMarketService.Query(() => 4)
				.Grouping(trade => (int) Math.Truncate(Math.Abs(trade.PercentFromMarket.Value)))
				.Selecting(group => new Domain.Model.TradesByPercentDiff(group, _consumer, backgroundOcDispatcher, wpfOcDispatcher))
				.Ordering(tbpd => tbpd.PercentBand)
				.CollectionDispatching(wpfOcDispatcher, backgroundOcDispatcher, new DispatcherPriorities(1, 0))
				.For(_consumer);
		}

		public ObservableCollection<Domain.Model.TradesByPercentDiff> Data { get; }

		public void Dispose()
		{
			_consumer.Dispose();
		}
	}
}