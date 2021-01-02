using System;
using System.Collections.ObjectModel;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Services;

namespace Trader.Client.Views
{
	public class TradesByPercentViewer : AbstractNotifyPropertyChanged, IDisposable
	{
		private readonly OcConsumer _consumer = new OcConsumer();

		public TradesByPercentViewer(INearToMarketService nearToMarketService, OcDispatcher backgroundOcDispatcher, WpfOcDispatcher wpfOcDispatcher)
		{
			Data = 
				nearToMarketService.Query(() => 4)
				.Grouping(trade => (int) Math.Truncate(Math.Abs(trade.PercentFromMarket.Value)))
				.Selecting(group => new Domain.Model.TradesByPercentDiff(group, _consumer, backgroundOcDispatcher, wpfOcDispatcher))
				.Ordering(tbpd => tbpd.PercentBand)
				.CollectionDispatching(wpfOcDispatcher, backgroundOcDispatcher, 0, 1)
				.For(_consumer);
		}

		public ObservableCollection<Domain.Model.TradesByPercentDiff> Data { get; }

		public void Dispose()
		{
			_consumer.Dispose();
		}
	}
}