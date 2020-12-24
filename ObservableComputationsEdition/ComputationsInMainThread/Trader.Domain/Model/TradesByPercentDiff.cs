using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ObservableComputations;
using TradeExample.Annotations;

namespace Trader.Domain.Model
{
	public class TradesByPercentDiff : IEquatable<TradesByPercentDiff>
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

		#region Equality

		public bool Equals(TradesByPercentDiff other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return PercentBand == other.PercentBand;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((TradesByPercentDiff) obj);
		}

		public override int GetHashCode()
		{
			return PercentBand;
		}

		public static bool operator ==(TradesByPercentDiff left, TradesByPercentDiff right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(TradesByPercentDiff left, TradesByPercentDiff right)
		{
			return !Equals(left, right);
		}

		#endregion
	}
}