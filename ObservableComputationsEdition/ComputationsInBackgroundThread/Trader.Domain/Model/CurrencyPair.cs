using System;

namespace Trader.Domain.Model
{
	public class CurrencyPair
	{
		public CurrencyPair(string code, decimal startingPrice, int decimalPlaces, decimal tickFrequency, int defaultSpread = 8)
		{
			Code = code;
			InitialPrice = startingPrice;
			DecimalPlaces = decimalPlaces;
			TickFrequency = tickFrequency;
			DefaultSpread = defaultSpread;
			PipSize = (decimal)Math.Pow(10, -decimalPlaces);
		}

		public string Code { get; }
		public decimal InitialPrice { get; }
		public int DecimalPlaces { get; }
		public decimal TickFrequency { get; }
		public decimal PipSize { get; }
		public int DefaultSpread { get; }

		public override string ToString()
		{
			return $"Code: {Code}, DecimalPlaces: {DecimalPlaces}";
		}
	}
}