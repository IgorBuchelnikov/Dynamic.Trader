using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ObservableComputations;
using TradeExample.Annotations;

namespace Trader.Domain.Model
{
	public class TradesByTime 
	{
		public TradesByTime([NotNull] Group<Trade, TimePeriod> @group,
			OcConsumer consumer)
		{
			Period = group?.Key ?? throw new ArgumentNullException(nameof(group));

			Data = group
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

		public ObservableCollection<TradeProxy> Data { get; }
	}
}