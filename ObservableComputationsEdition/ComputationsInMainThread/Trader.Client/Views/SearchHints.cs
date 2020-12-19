using System;
using ObservableComputations;
using Trader.Domain.Infrastucture;

namespace Trader.Client.Views
{
	public class SearchHints : AbstractNotifyPropertyChanged, IDisposable
	{
		private string _searchText = String.Empty;
		private Computing<string> _searchTextToApply;
		private readonly Consumer _consumer = new Consumer();

		public SearchHints(UserInputThrottlingOcDispatcher userInputThrottlingOcDispatcher)
		{
			_searchTextToApply =
				new Computing<string>(() => 
					new Computing<string>(() => SearchText)
					.ScalarDispatching(userInputThrottlingOcDispatcher)
					.Value ?? string.Empty)
				.For(_consumer);
		}

		public string SearchText
		{
			get => _searchText;
			set => SetAndRaise(ref _searchText, value);
		}

		public IReadScalar<string> SearchTextToApply
		{
			get => _searchTextToApply;
		}

		public void Dispose()
		{
			_consumer.Dispose();
		}
	}
}