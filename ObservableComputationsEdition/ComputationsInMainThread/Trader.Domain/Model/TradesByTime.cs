using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ObservableComputations;
using TradeExample.Annotations;

namespace Trader.Domain.Model
{
    public class TradesByTime : IEquatable<TradesByTime>
    {
        private readonly ObservableCollection<TradeProxy> _data;

        public TradesByTime([NotNull] Group<Trade, TimePeriod> @group,
            Consumer consumer)
        {
            Period = group?.Key ?? throw new ArgumentNullException(nameof(group));

            _data = group
	            .Ordering(t => t.Timestamp, ListSortDirection.Descending)
                .Selecting(trade => new TradeProxy(trade))
                .CollectionDisposing()
                .For(consumer);
        }


        public TimePeriod Period { get; }

        public string Description
        {
            get
            {
                switch (Period)
                {
                    case TimePeriod.LastMinute:
                        return "Last Minute";
                    case TimePeriod.LastHour:
                        return "Last Hour";
                        ;
                    case TimePeriod.Older:
                        return "Old";
                    default:
                        return "Unknown";
                }
            }
        }

        public ObservableCollection<TradeProxy> Data => _data;

        #region Equality

        public bool Equals(TradesByTime other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Period == other.Period;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TradesByTime) obj);
        }

        public override int GetHashCode()
        {
            return (int) Period;
        }

        public static bool operator ==(TradesByTime left, TradesByTime right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TradesByTime left, TradesByTime right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}