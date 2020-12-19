using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using ObservableComputations;
using TradeExample.Annotations;
using Trader.Domain.Infrastucture;
using Trader.Domain.Model;

namespace Trader.Domain.Services
{
	public class NearToMarketService : INearToMarketService
	{
		private readonly ITradeService _tradeService;

		public NearToMarketService([NotNull] ITradeService tradeService)
		{
			_tradeService = tradeService ?? throw new ArgumentNullException(nameof(tradeService));
		}

		public ObservableCollection<Trade> Query(Expression<Func<decimal>> percentFromMarket)
		{
			if (percentFromMarket == null) throw new ArgumentNullException(nameof(percentFromMarket));

			return _tradeService.Live.Filtering(ConstructPredicate(percentFromMarket));
		}

		private Expression<Func<Trade, bool>> ConstructPredicate(Expression<Func<decimal>> percentFromMarket)
		{
		var tradeParameter = Expression.Parameter(typeof(Trade));
		var tradePercentFromMarket = Expression.Property(tradeParameter, nameof(Trade.PercentFromMarket));
		var tradePercentFromMarketValue = Expression.Property(tradePercentFromMarket, nameof(Computing<decimal>.Value));
		var mathAbsMethodInfo = typeof(Math).GetMethod(nameof(Math.Abs), new[] {typeof(decimal)});
		var absTradePercentFromMarketValue = Expression.Call(null, mathAbsMethodInfo, tradePercentFromMarketValue);

		return Expression.Lambda<Func<Trade, bool>>(
		Expression.LessThanOrEqual(absTradePercentFromMarketValue, percentFromMarket.Body), tradeParameter);
		}
	}
}