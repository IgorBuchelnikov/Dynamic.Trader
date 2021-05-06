using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Model;
using Trader.Domain.Services;

namespace Trader.Client.Views
{
	public class LiveTradesViewer : AbstractNotifyPropertyChanged, IDisposable
	{
		private bool _paused;
		private readonly OcConsumer _consumer = new OcConsumer();

		public LiveTradesViewer(ITradeService tradeService, SearchHints searchHints, OcDispatcher backgroundOcDispatcher, WpfOcDispatcher wpfOcDispatcher)
		{
			SearchHints = searchHints;

			Data = tradeService.Live
				.CollectionPausing(new Computing<bool>(() => Paused).ScalarDispatching(backgroundOcDispatcher, wpfOcDispatcher))
				.Filtering(t =>	
					t.CurrencyPair.Contains(SearchHints.SearchTextThrottled.Value, StringComparison.OrdinalIgnoreCase)
					|| t.Customer.Contains(SearchHints.SearchTextThrottled.Value, StringComparison.OrdinalIgnoreCase))
				.Ordering(t => t.Timestamp, ListSortDirection.Descending)
				.Selecting(t => new TradeProxy(t))
				.CollectionDisposing()
				.CollectionDispatching(wpfOcDispatcher, backgroundOcDispatcher, 0, 1)
				.For(_consumer);
		}

		public ObservableCollection<TradeProxy> Data { get; }

		public SearchHints SearchHints { get; }

		public bool Paused
		{
			get => _paused;
			set => SetAndRaise(ref _paused, value);
		}

		public void Dispose()
		{
			_consumer.Dispose();
			SearchHints.Dispose();
		}
	}
}