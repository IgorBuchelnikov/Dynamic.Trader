using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Trader.Domain.Infrastucture
{
	public class RecurringAction : IDisposable
	{
		public Action Action { get; }
		public Func<TimeSpan> IntervalFunc { get; }
		public  bool IsAlive  { get; private set; }
		public  bool IsDisposed  { get; private set; }
		private ManualResetEventSlim _mresInterval = new ManualResetEventSlim(false);
		private ManualResetEventSlim _mresStop = new ManualResetEventSlim(false);

		private static List<RecurringAction> _notDisposedInstances = new List<RecurringAction>();
		private  static object _notDisposedInstancesLock = new object();
		private string _instantiatingStackTrace;

		public static bool AllInstancesIsDisposed 
		{
			get
			{
				lock (_notDisposedInstancesLock)
					return !_notDisposedInstances.Any();
			}
		}

		public RecurringAction(Action action, Func<TimeSpan> intervalFunc)
		{
			_instantiatingStackTrace = Environment.StackTrace;
			IsAlive = true;
			Action = action;
			IntervalFunc = intervalFunc;

			lock (_notDisposedInstancesLock)
			{
				_notDisposedInstances.Add(this);
			}

			Thread tradeEmitterThread = new Thread(() =>
			{
				Stopwatch stopwatch = new Stopwatch();
				while (IsAlive)
				{
					stopwatch.Restart();
					Action();
					stopwatch.Stop();

					TimeSpan interval = IntervalFunc();
					//interval =
					//	stopwatch.Elapsed < interval
					//		? interval - stopwatch.Elapsed
					//		: TimeSpan.Zero;

					_mresInterval.Wait(interval);

				}

				IsDisposed = true;
				_mresStop.Set();
			});

			tradeEmitterThread.Start();
		}


		public void Dispose()
		{
			IsAlive = false;
			_mresInterval.Set();

			Thread disposeThread = new Thread(() =>
			{
				_mresStop.Wait();
				_mresInterval.Dispose();
				_mresStop.Dispose();

				lock (_notDisposedInstancesLock)
				{
					_notDisposedInstances.Remove(this);
				}
			});

			disposeThread.Name = "RecurringAction dispose";
			disposeThread.IsBackground = true;
			disposeThread.Start();

		}

	}
}
