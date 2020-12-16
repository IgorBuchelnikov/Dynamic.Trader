using System;
using ObservableComputations;
using Trader.Domain.Infrastucture;

namespace Trader.Domain.Model
{
    public class Trade: AbstractNotifyPropertyChanged, IDisposable, IEquatable<Trade>
    {
        private decimal _marketPrice;
        private Computing<decimal> _percentFromMarket;
        private TradeStatus _status;
        private TimePeriod _age;
        private bool _expired;
        private Consumer _consumer = new Consumer();

        public long Id { get; }
        public string CurrencyPair { get; }
        public string Customer { get; }
        public decimal TradePrice { get; }

        public decimal MarketPrice
        {
	        get => _marketPrice;
	        set => SetAndRaise(ref _marketPrice, value);
        }

        public Computing<decimal> PercentFromMarket => _percentFromMarket;

        public decimal Amount { get; }
        public BuyOrSell BuyOrSell { get; }

        public TradeStatus Status
        {
	        get => _status;
	        set => SetAndRaise(ref _status, value);
        }

        public DateTime Timestamp { get; }

        public TimePeriod Age
        {
            get => _age;
            set => SetAndRaise(ref _age, value);
        }

        public bool Expired
        {
            get => _expired;
            set => SetAndRaise(ref _expired, value);
        }

        public DateTime CloseTimestamp { get; set; }

        private Trade()
        {
	        _percentFromMarket = new Computing<decimal>(() => 
		        MarketPrice != 0 
			        ? Math.Round(((TradePrice - MarketPrice) / MarketPrice) * 100, 4)
			        : 0)
                .For(_consumer);
        }

        public Trade(Trade trade, TradeStatus status) : this()
        {
            Id = trade.Id;
            Customer = trade.Customer;
            CurrencyPair = trade.CurrencyPair;
            _status = status;
            MarketPrice = trade.MarketPrice;
            TradePrice = trade.TradePrice;
            Amount = trade.Amount;
            Timestamp = DateTime.Now;
            BuyOrSell = trade.BuyOrSell;
        }

        public Trade(long id, string customer, string currencyPair, TradeStatus status, BuyOrSell buyOrSell, decimal tradePrice, decimal amount, decimal marketPrice = 777, DateTime? timeStamp = null) : this()
        {
            Id = id;
            Customer = customer;
            CurrencyPair = currencyPair;
            _status = status;
            MarketPrice = marketPrice;
            TradePrice = tradePrice;
            Amount = amount;
            BuyOrSell = buyOrSell;
            Timestamp =timeStamp ?? DateTime.Now;
        }

        #region Equality Members

        public bool Equals(Trade other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Trade) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(Trade left, Trade right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Trade left, Trade right)
        {
            return !Equals(left, right);
        }

        #endregion

        public void Dispose()
        {
            _consumer.Dispose();
        }
    }
}