using System;
using ObservableComputations;
using Trader.Domain.Infrastucture;

namespace Trader.Domain.Model
{
	public class CurrencyPairPosition: AbstractNotifyPropertyChanged
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
	}
}