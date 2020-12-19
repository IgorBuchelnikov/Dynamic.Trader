using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ObservableComputations;

namespace Trader.Domain.Infrastucture
{
	public class ThrottlingDispatcher : IOcDispatcher, IDisposable
	{
		public IOcDispatcher DestinationOcDispatcher => _destinationOcDispatcher;
		public TimeSpan TimeSpan => _timeSpan;

		Subject<Action> _actions;
		private IDisposable _cleanUp;
		private readonly IOcDispatcher _destinationOcDispatcher;
		private readonly TimeSpan _timeSpan;

		public ThrottlingDispatcher(TimeSpan timeSpan, IOcDispatcher destinationOcDispatcher)
		{
			_timeSpan = timeSpan;
			_destinationOcDispatcher = destinationOcDispatcher;

			_actions = new Subject<Action>();
			_cleanUp = _actions.Throttle(timeSpan).Subscribe(action =>
			{
				_destinationOcDispatcher.Invoke(action, 0, null, this);

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
		public UserInputThrottlingOcDispatcher(IOcDispatcher destinationOcDispatcher) : base(TimeSpan.FromMilliseconds(250), destinationOcDispatcher)
		{
		}
	}
}
