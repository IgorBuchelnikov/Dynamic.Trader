using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ObservableComputations;
using TradeExample.Annotations;

namespace Trader.Domain.Model
{
	public class TradesByPercentDiff
	{
		private readonly Group<Trade, int> _group;

		public TradesByPercentDiff([NotNull] Group<Trade, int> @group, Consumer consumer)
		{
			_group = @group ?? throw new ArgumentNullException(nameof(@group));
			PercentBand = group.Key;

			Data = group
				.Ordering(t => t.Timestamp, ListSortDirection.Descending)
				.Selecting(trade => new TradeProxy(trade))
				.CollectionDisposing()
				.For(consumer);
		}

		public int PercentBand { get; }

		public int PercentBandUpperBound => _group.Key + 1;

		public ObservableCollection<TradeProxy> Data { get; }
	}
}