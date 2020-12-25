using System;

namespace Trader.Domain.Model
{
	public class MarketData
	{
		public MarketData(string instrument, decimal bid, decimal offer)
		{
			Instrument = instrument;
			Bid = bid;
			Offer = offer;
		}

		public string Instrument { get; }
		public decimal Bid { get; }
		public decimal Offer { get; }

		public static MarketData operator +(MarketData left, decimal pipsValue)
		{
			var bid = left.Bid + pipsValue;
			var offer = left.Offer + pipsValue;
			return new MarketData(left.Instrument, bid, offer);
		}

		public static MarketData operator -(MarketData left, decimal pipsValue)
		{
			var bid = left.Bid - pipsValue;
			var offer = left.Offer - pipsValue;
			return new MarketData(left.Instrument, bid, offer);
		}

		public static bool operator >=(MarketData left, MarketData right)
		{
			return left.Bid >= right.Bid;
		}

		public static bool operator <=(MarketData left, MarketData right)
		{
			return left.Bid <= right.Bid;
		}

		public static bool operator >(MarketData left, MarketData right)
		{
			return left.Bid > right.Bid;
		}

		public static bool operator <(MarketData left, MarketData right)
		{
			return left.Bid < right.Bid;
		}

		public static bool operator ==(MarketData left, MarketData right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(MarketData left, MarketData right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return $"{Instrument}, {Bid}/{Offer}";
		}
	}
}