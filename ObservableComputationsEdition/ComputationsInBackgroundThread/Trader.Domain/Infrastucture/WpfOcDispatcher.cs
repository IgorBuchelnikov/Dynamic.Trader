using System;
using System.Collections.Generic;
using System.Windows.Threading;
using ObservableComputations;

namespace Trader.Domain.Infrastucture
{
	public class WpfOcDispatcher : IOcDispatcher
	{
		private Dispatcher _dispatcher;

		public List<Action> _deferredActions = new List<Action>();

		private bool _isPaused;

		public bool IsPaused
		{
			get => _isPaused;
			set
			{
				if (_isPaused && !value)
				{
					_dispatcher.Invoke(() =>
					{
						foreach (Action deferredAction in _deferredActions)
						{
							deferredAction();
						}
					}, DispatcherPriority.Send);

					_deferredActions.Clear();
				}

				_isPaused = value;
			}
		}

		public WpfOcDispatcher(Dispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}

		#region Implementation of IDispatcher

		public void Invoke(Action action, int priority = 0, object parameter = null, object context = null)
		{
			if (_isPaused)
			{
				_deferredActions.Add(action);
				return;
			}

			if (_dispatcher.CheckAccess())
				action();
			else
				_dispatcher.Invoke(action, DispatcherPriority.Send);
		}

		#endregion
	}
}
