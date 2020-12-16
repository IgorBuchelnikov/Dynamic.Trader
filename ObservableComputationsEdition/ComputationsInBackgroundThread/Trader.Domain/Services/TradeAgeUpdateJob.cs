using System;
using System.Linq;
using System.Windows.Threading;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Model;

namespace Trader.Domain.Services
{
    public class TradeAgeUpdateJob : IDisposable
    {
        private RecurringAction _recurringAction;

        public TradeAgeUpdateJob(OcDispatcher backgroundOcDispatcher, Dispatcher dispatcher, ITradeService tradeService)
        {
            _recurringAction = new RecurringAction(() =>
            {
                Trade[] trades = null;

                backgroundOcDispatcher.Invoke(() => 
                    trades = tradeService.All.ToArray());

                foreach (Trade trade in trades)
                {
                    TimeSpan diff = DateTime.Now.Subtract(trade.Timestamp);

                    TimePeriod age = 
                        diff.TotalSeconds <= 60 ?
                            TimePeriod.LastMinute
                            : diff.TotalMinutes <= 60 ?
                                TimePeriod.LastHour
                                : TimePeriod.Older;

                    backgroundOcDispatcher.Invoke(() => 
                        dispatcher.Invoke(() => 
                            { trade.Age = age; }, 
                            DispatcherPriority.Background));

                    if (diff.TotalSeconds > 30)
                        backgroundOcDispatcher.Invoke(() => 
                            dispatcher.Invoke(() => 
                                { trade.Expired = true; }, 
                                DispatcherPriority.Background));
                }
            }, () => TimeSpan.FromSeconds(1));
        }

        public void Dispose()
        {
            _recurringAction.Dispose();
        }

    }
}
