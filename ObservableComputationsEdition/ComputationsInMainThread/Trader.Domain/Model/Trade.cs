using System;
using ObservableComputations;
using Trader.Domain.Infrastucture;

namespace Trader.Domain.Model
{
	public class Trade: AbstractNotifyPropertyChanged, IDisposable
	{
		private decimal _marketPrice;
		private TradeStatus _status;
		private TimePeriod _age;
		private bool _expired;
		private readonly Consumer _consumer = new Consumer();

		public long Id { get; }
		public string CurrencyPair { get; }
		public string Customer { get; }
		public decimal TradePrice { get; }

		public decimal MarketPrice
		{
			get => _marketPrice;
			set => SetAndRaise(ref _marketPrice, value);
		}

		public Computing<decimal> PercentFromMarket { get; }

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
			PercentFromMarket = new Computing<decimal>(() => 
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

		public void Dispose()
		{
			_consumer.Dispose();
		}
	}
}