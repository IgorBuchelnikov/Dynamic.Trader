using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ObservableComputations;
using TradeExample.Annotations;
using Trader.Domain.Infrastucture;

namespace Trader.Domain.Model
{
	public class TradesByPercentDiff
	{
		private readonly Group<Trade, int> _group;

		public TradesByPercentDiff([NotNull] Group<Trade, int> group,
			Consumer consumer, OcDispatcher backgroundOcDispatcher, WpfOcDispatcher wpfOcDispatcher)
		{
			_group = group ?? throw new ArgumentNullException(nameof(group));
			PercentBand = group.Key;

			Data = group
			.Ordering(t => t.Timestamp, ListSortDirection.Descending)
			.Selecting(trade => new TradeProxy(trade))
				.CollectionDisposing()
				.CollectionDispatching(wpfOcDispatcher, backgroundOcDispatcher, new DispatcherPriorities(1, 0))
				.For(consumer);
		}

		public int PercentBand { get; }

		public int PercentBandUpperBound => _group.Key + 1;

		public ObservableCollection<TradeProxy> Data { get; }
	}
}