using System.Collections.ObjectModel;
using Trader.Domain.Model;

namespace Trader.Domain.Services
{
	public interface ITradeService
	{
		ObservableCollection<Trade> All { get; }
		ObservableCollection<Trade> Live { get; }
	}
}