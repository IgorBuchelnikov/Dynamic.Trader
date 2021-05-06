using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Services;

namespace Trader.Client.Views
{
	public class SearchHints : AbstractNotifyPropertyChanged, IDisposable
	{
		private string _searchText = "";
		private readonly ScalarDispatching<string> _searchTextThrottled;
		private readonly ObservableCollection<string> _hints;
		private readonly OcConsumer _consumer = new OcConsumer();

		public SearchHints(ITradeService tradeService, UserInputThrottlingOcDispatcher throttling, OcDispatcher backgroundOcDispatcher, WpfOcDispatcher wpfOcDispatcher)
		{
			_searchTextThrottled =
				new Computing<string>(() => SearchText)
					.ScalarDispatching(throttling)
					.SetDefaultValue("")
					.For(_consumer);

			_searchTextThrottled.PreValueChanged += (sender, args) => 
				wpfOcDispatcher.IsPaused = true;

			_searchTextThrottled.PostValueChanged += (sender, args) =>
			{
				wpfOcDispatcher.IsPaused = false;
				wpfOcDispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
			};

			_hints = 
				tradeService.Live.Selecting(t => t.CurrencyPair).Distincting()
				.Concatenating(
					tradeService.Live.Selecting(t => t.Customer).Distincting())
				.Filtering(str =>
					str.Contains(_searchTextThrottled.Value, StringComparison.OrdinalIgnoreCase)
					|| str.Contains(_searchTextThrottled.Value, StringComparison.OrdinalIgnoreCase))
				.Ordering(s => s)
				.CollectionDispatching(wpfOcDispatcher, backgroundOcDispatcher)
				.For(_consumer);
		}

		public string SearchText
		{
			get => _searchText;
			set => SetAndRaise(ref _searchText, value);
		}

		public IReadScalar<string> SearchTextThrottled => _searchTextThrottled;

		public ObservableCollection<string> Hints => _hints;

		public void Dispose()
		{
			_consumer.Dispose();
		}
	}
}