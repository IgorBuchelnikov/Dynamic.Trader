using System;
using System.Windows.Input;
using ObservableComputations;
using Trader.Domain.Infrastucture;

namespace Trader.Client.Views
{
	public class SearchHints : AbstractNotifyPropertyChanged, IDisposable
	{
		private string _searchText = String.Empty;
		private readonly Computing<string> _searchTextToApply;
		private readonly OcConsumer _consumer = new OcConsumer();

		public SearchHints(UserInputThrottlingOcDispatcher userInputThrottlingOcDispatcher, WpfOcDispatcher wpfOcDispatcher)
		{
			_searchTextToApply =
				new Computing<string>(() => 
					new Computing<string>(() => SearchText)
					.ScalarDispatching(userInputThrottlingOcDispatcher)
					.Value ?? string.Empty)
				.For(_consumer);

			_searchTextToApply.PreValueChanged += (sender, args) => 
				wpfOcDispatcher.IsPaused = true;

			_searchTextToApply.PostValueChanged += (sender, args) =>
			{
				wpfOcDispatcher.IsPaused = false;
				wpfOcDispatcher.Invoke(() => CommandManager.InvalidateRequerySuggested(), 0, null, null);
			};
		}

		public string SearchText
		{
			get => _searchText;
			set => SetAndRaise(ref _searchText, value);
		}

		public IReadScalar<string> SearchTextToApply => _searchTextToApply;

		public void Dispose()
		{
			_consumer.Dispose();
		}
	}
}