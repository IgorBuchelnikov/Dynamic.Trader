using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using ObservableComputations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Model;
using Dispatcher = System.Windows.Threading.Dispatcher;

namespace Trader.Domain.Services
{
	public class MarketDataService : IMarketDataService
	{
		private readonly Dictionary<string, IReadScalar<MarketData>> _marketDataObservables = new Dictionary<string, IReadScalar<MarketData>>();
		private readonly Dispatcher _dispatcher;

		public MarketDataService(IStaticData staticData, Dispatcher dispatcher)
		{
			_dispatcher = dispatcher;

			foreach (var item in staticData.CurrencyPairs)
			{
				_marketDataObservables[item.Code] = CreateObservableMarketData(item);
			}
		}

		private IReadScalar<MarketData> CreateObservableMarketData(CurrencyPair currencyPair)
		{
			return new MarketDataObsevable(currencyPair, _dispatcher);
		}

		public IReadScalar<MarketData> Get(string currencyPair)
		{
			if (currencyPair == null) throw new ArgumentNullException(nameof(currencyPair));
			if (_marketDataObservables.TryGetValue(currencyPair, out var marketDataObservable))
				return marketDataObservable;

			throw new Exception(currencyPair + " is an unknown currency pair");			
		}

		private class MarketDataObsevable : AbstractNotifyPropertyChanged, IReadScalar<MarketData>, IDisposable
		{
			private MarketData _value;
			private RecurringAction _recurringAction;

			public MarketDataObsevable(CurrencyPair currencyPair, Dispatcher dispatcher)
			{
				var spread = currencyPair.DefaultSpread;
				var midRate = currencyPair.InitialPrice;
				var bid = midRate - (spread * currencyPair.PipSize);
				var offer = midRate + (spread * currencyPair.PipSize);
				var initial = new MarketData(currencyPair.Code, bid, offer);

				var currentPrice = initial;

				Value = currentPrice;

				var random = new Random();

				//for a given period, move prices by up to 5 pips
				_recurringAction = new RecurringAction(() =>
				{
					int pips = random.Next(1, 5);
					//move up or down between 1 and 5 pips
					var adjustment = Math.Round(pips * currencyPair.PipSize, currencyPair.DecimalPlaces);
					MarketData marketData = random.NextDouble() > 0.5
						? currentPrice + adjustment
						: currentPrice - adjustment;

					dispatcher.Invoke(() =>									   
						Value = marketData, DispatcherPriority.Background);
				}, () => TimeSpan.FromSeconds(1 / (double)currencyPair.TickFrequency));
			}

			public MarketData Value
			{
				get => _value;
				set => SetAndRaise(ref _value, value);
			}

			public void Dispose()
			{
				_recurringAction.Dispose();
			}

		}

		public void Dispose()
		{
			foreach (MarketDataObsevable marketDataObsevable 
				in _marketDataObservables.Values.Cast<MarketDataObsevable>())
				marketDataObsevable.Dispose();
		}

	}
}