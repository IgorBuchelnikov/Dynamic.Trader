using System;
using ObservableComputations;

namespace Trader.Domain.Model
{
	public class TradesPosition
	{
		private readonly IReadScalar<int> _count;
		
		public TradesPosition(IReadScalar<decimal> buy, IReadScalar<decimal> sell, IReadScalar<int> count,
			Consumer consumer)
		{
			Buy = buy;
			Sell = sell;
			_count = count;
			Position = new Computing<decimal>(() => Buy.Value - Sell.Value).For(consumer);
			_countText = new Computing<string>(() => "Order".Pluralise(_count.Value)).For(consumer);
			_negative = new Computing<bool>(() => Position.Value < 0).For(consumer);
		}

		private IReadScalar<bool> _negative;
		public IReadScalar<bool> Negative => _negative;


		public IReadScalar<decimal> Position { get; }
		public IReadScalar<decimal> Buy { get; }
		public IReadScalar<decimal> Sell { get; }

		public IReadScalar<string> _countText;
		public IReadScalar<string> CountText => _countText;
	}
}