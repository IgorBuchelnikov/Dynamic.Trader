using System.Collections.ObjectModel;

namespace Trader.Domain.Model
{
	public sealed class SortContainer
	{
		/// <summary>
		///	 Initializes a new instance of the <see cref="T:System.Object" /> class.
		/// </summary>
		public SortContainer(string description, ObservableCollection<TradeProxy> sortedData)
		{
			Description = description;
			SortedData = sortedData;
		}

		public ObservableCollection<TradeProxy> SortedData { get; }
		public string Description { get; }
	}
}