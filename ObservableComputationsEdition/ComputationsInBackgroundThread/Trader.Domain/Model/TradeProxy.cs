using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Trader.Domain.Infrastucture;

namespace Trader.Domain.Model
{
    public class TradeProxy : AbstractNotifyPropertyChanged, IDisposable//, IEquatable<TradeProxy>
    {
        private readonly IDisposable _cleanUp;
        private readonly long _id;
        private readonly Trade _trade;
        private decimal _marketPrice;
        private decimal _pcFromMarketPrice;
        private bool _recent;

        public Trade Trade => _trade;

        public TradeProxy(Trade trade)
        {
            _id = trade.Id;
            _trade = trade;

            var isRecent = DateTime.Now.Subtract(trade.Timestamp).TotalSeconds < 2;
            var recentIndicator = Disposable.Empty;

            if (isRecent)
            {
                Recent = true;
                recentIndicator = Observable.Timer(TimeSpan.FromSeconds(2))
                    .Subscribe(_ => Recent = false);
            }


            _cleanUp = Disposable.Create(() =>
            {
                recentIndicator.Dispose();
            });
        }

        public bool Recent
        {
            get => _recent;
            set => SetAndRaise(ref _recent, value);
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }

        //#region Equaility Members

        //public bool Equals(TradeProxy other)
        //{
        //    if (ReferenceEquals(null, other)) return false;
        //    if (ReferenceEquals(this, other)) return true;
        //    return _id == other._id;
        //}

        //public override bool Equals(object obj)
        //{
        //    if (ReferenceEquals(null, obj)) return false;
        //    if (ReferenceEquals(this, obj)) return true;
        //    if (obj.GetType() != GetType()) return false;
        //    return Equals((TradeProxy) obj);
        //}

        //public override int GetHashCode()
        //{
        //    return _id.GetHashCode();
        //}

        //public static bool operator ==(TradeProxy left, TradeProxy right)
        //{
        //    return Equals(left, right);
        //}

        //public static bool operator !=(TradeProxy left, TradeProxy right)
        //{
        //    return !Equals(left, right);
        //}

        //#endregion
    }
}