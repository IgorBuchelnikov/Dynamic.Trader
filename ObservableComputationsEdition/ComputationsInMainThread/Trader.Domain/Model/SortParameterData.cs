using System.Collections.ObjectModel;
using System.ComponentModel;
using ObservableComputations;
using Trader.Domain.Infrastucture;

namespace Trader.Domain.Model
{
	public class SortParameterData : AbstractNotifyPropertyChanged
	{
		private SortContainer _selectedItem;

		public SortParameterData(ObservableCollection<TradeProxy> sourceData, OcConsumer consumer)
		{
			SortItems = new []
			{
				new SortContainer("Customer, Currency Pair", 
					sourceData
					.Ordering(l => l.Trade.Customer)
					.ThenOrdering(p => p.Trade.CurrencyPair)
					.ThenOrdering(p => p.Trade.Id)
					.For(consumer)),

				new SortContainer("Currency Pair, Amount", 
					sourceData
					.Ordering(l => l.Trade.CurrencyPair)
					.ThenOrdering(p => p.Trade.Amount, ListSortDirection.Descending)
					.ThenOrdering(p => p.Trade.Id)
					.For(consumer)),


				new SortContainer("Recently Changed", 
					sourceData
					.Ordering(l => l.Trade.Timestamp, ListSortDirection.Descending)
					.ThenOrdering(p => p.Trade.Customer)
					.ThenOrdering(p => p.Trade.Id)
					.For(consumer))
			};

			SelectedItem = SortItems[2];
		}

		public SortContainer SelectedItem
		{
			get => _selectedItem;
			set => SetAndRaise(ref _selectedItem, value);
		}

		public SortContainer[] SortItems { get; }
	}
}