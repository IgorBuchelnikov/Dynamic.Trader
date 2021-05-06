using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ObservableComputations;
using Trader.Client.Infrastucture;
using Trader.Domain.Infrastucture;
using Trader.Domain.Model;
using Trader.Domain.Services;

namespace Trader.Client.Views
{
	public class PagedDataViewer : AbstractNotifyPropertyChanged, IDisposable
	{
		private readonly Command _nextPageCommand;
		private readonly Command _previousPageCommand;

		public ICommand NextPageCommand => _nextPageCommand;
		public ICommand PreviousPageCommand => _previousPageCommand;

		private readonly OcConsumer _consumer = new OcConsumer();

		public PagedDataViewer(ITradeService tradeService, SearchHints searchHints)
		{
			SearchHints = searchHints;

			SortParameters = new SortParameterData(
				tradeService.Live
				.Selecting(t => new TradeProxy(t))
				.CollectionDisposing(), 
				_consumer);

			AllData =
				new Computing<ObservableCollection<TradeProxy>>(
					 () => SortParameters.SelectedItem.SortedData)
				.Filtering(t =>	
					t.Trade.CurrencyPair.Contains(SearchHints.SearchTextThrottled.Value, StringComparison.OrdinalIgnoreCase)
					|| t.Trade.Customer.Contains(SearchHints.SearchTextThrottled.Value, StringComparison.OrdinalIgnoreCase));

			Data = AllData.Paging(25, 1).For(_consumer);

			_nextPageCommand = new Command(() => Data.CurrentPage = Data.CurrentPage + 1, () => Data.CurrentPage < Data.PageCount);
			_previousPageCommand = new Command(() => Data.CurrentPage = Data.CurrentPage - 1, () => Data.CurrentPage > 1);
		}

		public SearchHints SearchHints { get; }

		public ObservableCollection<TradeProxy> AllData { get; }

		public Paging<TradeProxy> Data { get; }

		public SortParameterData SortParameters { get; }

		public void Dispose()
		{
			_consumer.Dispose();
			SearchHints.Dispose();
		}
	}
}