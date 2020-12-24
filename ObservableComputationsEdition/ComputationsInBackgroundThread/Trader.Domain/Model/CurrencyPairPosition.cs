using System;
using ObservableComputations;
using Trader.Domain.Infrastucture;

namespace Trader.Domain.Model
{
	public class CurrencyPairPosition: AbstractNotifyPropertyChanged, IEquatable<CurrencyPairPosition>
	{
		private TradesPosition _position;

		public CurrencyPairPosition(Group<Trade, string> tradesByCurrencyPair, Consumer consumer)
		{
			CurrencyPair = tradesByCurrencyPair.Key;
			_position = new TradesPosition(
				tradesByCurrencyPair
				.Filtering(trade => trade.BuyOrSell == BuyOrSell.Buy)
				.Selecting(trade => trade.Amount)
				.Summarizing()
				.For(consumer),

				tradesByCurrencyPair
				.Filtering(trade => trade.BuyOrSell == BuyOrSell.Sell)
				.Selecting(trade => trade.Amount)
				.Summarizing()
				.For(consumer),

				new Computing<int>(() => tradesByCurrencyPair.Count)
				.For(consumer),

				consumer);
		}

		public TradesPosition Position
		{
			get => _position;
			set => SetAndRaise(ref  _position, value);
		}

		public string CurrencyPair { get; }

		#region Equality Members

		public bool Equals(CurrencyPairPosition other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(CurrencyPair, other.CurrencyPair);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((CurrencyPairPosition) obj);
		}

		public override int GetHashCode()
		{
			return (CurrencyPair != null ? CurrencyPair.GetHashCode() : 0);
		}

		public static bool operator ==(CurrencyPairPosition left, CurrencyPairPosition right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(CurrencyPairPosition left, CurrencyPairPosition right)
		{
			return !Equals(left, right);
		}

		#endregion
	}
}