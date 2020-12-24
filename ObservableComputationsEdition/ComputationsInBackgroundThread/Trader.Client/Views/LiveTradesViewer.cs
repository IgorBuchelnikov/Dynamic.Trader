using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Disposables;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Model;
using Trader.Domain.Services;

namespace Trader.Client.Views
{
	public class LiveTradesViewer : AbstractNotifyPropertyChanged, IDisposable
	{
		private bool _paused;
		private Consumer _consumer = new Consumer();

		public LiveTradesViewer(ITradeService tradeService, SearchHints searchHints, OcDispatcher backgroundOcDispatcher, WpfOcDispatcher wpfOcDispatcher)
		{
			SearchHints = searchHints;

			Data = tradeService.Live
				.CollectionPausing(new Computing<bool>(() => Paused).ScalarDispatching(backgroundOcDispatcher, wpfOcDispatcher))
				.Filtering(t =>	
					t.CurrencyPair.Contains(SearchHints.SearchTextToApply.Value, StringComparison.OrdinalIgnoreCase)
					|| t.Customer.Contains(SearchHints.SearchTextToApply.Value, StringComparison.OrdinalIgnoreCase))
				.Ordering(t => t.Timestamp, ListSortDirection.Descending)
				.Selecting(t => new TradeProxy(t))
				.CollectionDisposing()
				.CollectionDispatching(wpfOcDispatcher, backgroundOcDispatcher, new DispatcherPriorities(1, 0))
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