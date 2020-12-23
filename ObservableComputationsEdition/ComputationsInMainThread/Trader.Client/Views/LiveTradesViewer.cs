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
		private readonly ObservableCollection<TradeProxy> _data;
		private bool _paused;
		private IDisposable _cleanup;

		public LiveTradesViewer(ITradeService tradeService, SearchHints searchHints, OcDispatcher backgroundOcDispatcher, WpfOcDispatcher wpfOcDispatcher)
		{
			Consumer consumer = new Consumer();
			SearchHints = searchHints;

			_data = tradeService.Live
				.CollectionPausing(() => Paused)
				.Filtering(t =>
					t.CurrencyPair.Contains(SearchHints.SearchTextToApply.Value, StringComparison.OrdinalIgnoreCase)
					|| t.Customer.Contains(SearchHints.SearchTextToApply.Value, StringComparison.OrdinalIgnoreCase))
				.Ordering(t => t.Timestamp, ListSortDirection.Descending)
				.Selecting(t => new TradeProxy(t))
				.CollectionDisposing()
				.For(_consumer);

			_cleanup = new CompositeDisposable(searchHints, consumer);
		}

		public ObservableCollection<TradeProxy> Data => _data;

		public SearchHints SearchHints { get; }

		public bool Paused
		{
			get => _paused;
			set => SetAndRaise(ref _paused, value);
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			_cleanup.Dispose();
		}

		#endregion
	}
}
