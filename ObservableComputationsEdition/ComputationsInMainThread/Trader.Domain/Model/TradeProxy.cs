using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Trader.Domain.Infrastucture;

namespace Trader.Domain.Model
{
	public class TradeProxy : AbstractNotifyPropertyChanged, IDisposable//, IEquatable<TradeProxy>
	{
		private readonly IDisposable _cleanUp;
		private readonly long _id;
		private bool _recent;

		public Trade Trade { get; }

		public TradeProxy(Trade trade)
		{
			_id = trade.Id;
			Trade = trade;

			var isRecent = DateTime.Now.Subtract(trade.Timestamp).TotalSeconds < 2;
			var recentIndicator = Disposable.Empty;

			if (isRecent)
			{
				Recent = true;
				recentIndicator = Observable.Timer(TimeSpan.FromSeconds(2))
					.Subscribe(_ => Recent = false);
			}


			_cleanUp = Disposable.Create(() =>
			{
				recentIndicator.Dispose();
			});
		}

		public bool Recent
		{
			get => _recent;
			set => SetAndRaise(ref _recent, value);
		}

		public void Dispose()
		{
			_cleanUp.Dispose();
		}
	}
}