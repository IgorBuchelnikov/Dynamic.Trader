using System;
using System.Collections.Generic;
using System.Linq;
using Trader.Domain.Model;

namespace Trader.Domain.Services
{
	public class TradeGenerator
	{
		private readonly Random _random = new Random();
		private readonly IStaticData _staticData;
		private readonly IMarketDataService _marketDataService;
		private int _counter;

		public TradeGenerator(IStaticData staticData, IMarketDataService marketDataService)
		{
			_staticData = staticData;
			_marketDataService = marketDataService;
		}

		public IEnumerable<Trade> Generate(int numberToGenerate, bool initialLoad = false)
		{
			Trade NewTrade()
			{
				var id = _counter++;
				var bank = _staticData.Customers[_random.Next(0, _staticData.Customers.Length)];
				var pair = _staticData.CurrencyPairs[_random.Next(0, _staticData.CurrencyPairs.Length)];
				var amount = (_random.Next(1, 2000) / 2) * (10 ^ _random.Next(1, 5));
				var buySell = _random.NextBoolean() ? BuyOrSell.Buy : BuyOrSell.Sell;

				if (initialLoad)
				{
					var status = _random.NextDouble() > 0.5 ? TradeStatus.Live : TradeStatus.Closed;
					var seconds = _random.Next(1, 60 * 60 * 24);
					var time = DateTime.Now.AddSeconds(-seconds);
					return new Trade(id, bank, pair.Code, status, buySell, GenerateRandomPrice(pair, buySell), amount, timeStamp: time);
				}

				return new Trade(id, bank, pair.Code, TradeStatus.Live, buySell, GenerateRandomPrice(pair, buySell), amount);
			}

			IEnumerable<Trade> result = Enumerable.Range(1, numberToGenerate).Select(_ => NewTrade()).ToArray();
			return result;
		}


		private decimal GenerateRandomPrice(CurrencyPair currencyPair, BuyOrSell buyOrSell)
		{
			var price = _marketDataService.Get(currencyPair.Code).Value?.Bid ?? currencyPair.InitialPrice;

			//generate percent price 1-100 pips away from the inital market
			var pipsFromMarket = _random.Next(1, 100);
			var adjustment = Math.Round(pipsFromMarket * currencyPair.PipSize, currencyPair.DecimalPlaces);
			decimal generateRandomPrice = buyOrSell == BuyOrSell.Sell ? price + adjustment : price - adjustment;

			return generateRandomPrice;
		}
	}
}