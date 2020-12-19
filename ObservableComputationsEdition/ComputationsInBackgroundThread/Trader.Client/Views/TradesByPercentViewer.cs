using System;
using System.Collections.ObjectModel;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Services;

namespace Trader.Client.Views
{
	public class TradesByPercentViewer : AbstractNotifyPropertyChanged, IDisposable
	{
		private readonly IDisposable _cleanUp;
		private readonly ObservableCollection<Domain.Model.TradesByPercentDiff> _data;

		public TradesByPercentViewer(INearToMarketService nearToMarketService, ILogger logger, OcDispatcher backgroundOcDispatcher, WpfOcDispatcher wpfOcDispatcher)
		{
			Consumer consumer = new Consumer();
			_data = nearToMarketService.Query(() => 4)
				.Grouping(trade => (int) Math.Truncate(Math.Abs(trade.PercentFromMarket.Value)))
				.Selecting(group => new Domain.Model.TradesByPercentDiff(group, logger, consumer, backgroundOcDispatcher, wpfOcDispatcher))
				.Ordering(tbpd => tbpd.PercentBand)
				.CollectionDispatching(wpfOcDispatcher, backgroundOcDispatcher, new DispatcherPriorities(1, 0))
				.For(consumer);

			_cleanUp = consumer;
		}

		public ObservableCollection<Domain.Model.TradesByPercentDiff> Data => _data;

		public void Dispose()
		{
			_cleanUp.Dispose();
		}
	}
}