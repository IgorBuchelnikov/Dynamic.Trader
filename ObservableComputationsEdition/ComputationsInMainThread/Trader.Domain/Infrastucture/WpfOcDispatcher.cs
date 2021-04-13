using System;
using System.Windows.Threading;
using ObservableComputations;

namespace Trader.Domain.Infrastucture
{
	public class WpfOcDispatcher : IOcDispatcher
	{
		private readonly Dispatcher _dispatcher;

		public WpfOcDispatcher(Dispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}

		#region Implementation of IOcDispatcher

		public void Invoke(Action action, int priority, object parameter, object context)
		{
			if (_dispatcher.CheckAccess())
				action();
			else
				_dispatcher.Invoke(action);
		}

		#endregion
	}
}
