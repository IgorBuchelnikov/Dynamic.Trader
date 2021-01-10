using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Model;

namespace Trader.Domain.Services
{
	public class TradeAgeUpdateJob : IDisposable
	{
		private readonly RecurringAction _recurringAction;

		public TradeAgeUpdateJob(OcDispatcher backgroundOcDispatcher, Dispatcher dispatcher, ITradeService tradeService)
		{
			_recurringAction = new RecurringAction(() =>
			{
				Trade[] trades = null;

				backgroundOcDispatcher.Invoke(() => 
					trades = tradeService.All.ToArray());

				if (trades != null)
					foreach (IEnumerable<Trade> tradesSlice in trades.Split(300))
					{
						backgroundOcDispatcher.Invoke(() =>
						{
							foreach (Trade trade in tradesSlice)
							{
								TimeSpan diff = DateTime.Now.Subtract(trade.Timestamp);

								TimePeriod age =
									diff.TotalSeconds <= 60 ? TimePeriod.LastMinute
									: diff.TotalMinutes <= 60 ? TimePeriod.LastHour
									: TimePeriod.Older;

								trade.Age = age;

								if (diff.TotalSeconds > 30)
									trade.Expired = true;								
							}
						});  
					}
			}, () => TimeSpan.FromSeconds(1));
		}

		public void Dispose()
		{
			_recurringAction.Dispose();
		}

	}
}
