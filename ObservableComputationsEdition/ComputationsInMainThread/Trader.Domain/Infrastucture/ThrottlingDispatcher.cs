using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Threading;
using ObservableComputations;

namespace Trader.Domain.Infrastucture
{
	public class ThrottlingDispatcher : IOcDispatcher, IDisposable
	{
		public Dispatcher Dispatcher => _dispatcher;
		public TimeSpan TimeSpan => _timeSpan;

		Subject<Action> _actions;
		private IDisposable _cleanUp;
		private readonly Dispatcher _dispatcher;
		private readonly TimeSpan _timeSpan;

		public ThrottlingDispatcher(TimeSpan timeSpan, Dispatcher dispatcher)
		{
			_timeSpan = timeSpan;
			_dispatcher = dispatcher;

			_actions = new Subject<Action>();
			_cleanUp = _actions.Throttle(timeSpan).Subscribe(action =>
			{
				_dispatcher.Invoke(action);

			});
		}

		#region Implementation of IDispatcher

		public void Invoke(Action action, int priority, object parameter, object context)
		{
			_actions.OnNext(action);
		}

		#endregion

		#region Implementation of IDisposable

		public void Dispose()
		{
			_cleanUp.Dispose();
		}

		#endregion
	}

	public class UserInputThrottlingOcDispatcher : ThrottlingDispatcher
	{
		public UserInputThrottlingOcDispatcher(Dispatcher dispatcher) : base(TimeSpan.FromMilliseconds(250), dispatcher)
		{
		}
	}
}
