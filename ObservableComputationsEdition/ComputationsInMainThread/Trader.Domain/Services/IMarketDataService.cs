using System;
using ObservableComputations;
using Trader.Domain.Model;

namespace Trader.Domain.Services
{
    public interface IMarketDataService : IDisposable
    {
        IReadScalar<MarketData> Get(string currencyPair);
    }
}