using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using TradeExample.Annotations;
using Trader.Domain.Model;

namespace Trader.Domain.Services
{
	public interface INearToMarketService
	{
		ObservableCollection<Trade> Query([NotNull] Expression<Func<decimal>> percentFromMarket);
	}
}