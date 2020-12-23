using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Dragablz;
using ObservableComputations;
using Trader.Domain.Infrastucture;

namespace Trader.Client.Infrastucture
{
	public class WindowViewModel: AbstractNotifyPropertyChanged, IDisposable
	{
		private readonly IObjectProvider _objectProvider;
		private readonly Command _showMenuCommand;
		private readonly IDisposable _cleanUp;
		private ViewContainer _selected;
		private readonly Consumer _consumer = new Consumer();

		public ICommand MemoryCollectCommand { get; } = new Command(() =>
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		});


		public WindowViewModel(IObjectProvider objectProvider, IWindowFactory windowFactory)
		{
			_objectProvider = objectProvider;
			InterTabClient = new InterTabClient(windowFactory);
			_showMenuCommand =  new Command(ShowMenu,()=> Selected!=null && !(Selected.Content is MenuItems));
			ShowInGitHubCommand = new Command(()=>   Process.Start( new ProcessStartInfo
			{
				FileName = "cmd",
				Arguments = "/c start https://github.com/IgorBuchelnikov/ObservableComputations"
			}));

			Views
				.Filtering(vc => vc.Content is MenuItems)
				.Selecting(vc => (MenuItems) vc.Content)
				.CollectionProcessing(
					(newMenuItems, _) =>
					{
						IDisposable[] subscriptions = new IDisposable[newMenuItems.Length];
						for (int index = 0; index < newMenuItems.Length; index++)
							subscriptions[index] =
								newMenuItems[index].ItemCreated.Subscribe(
									item =>
									{
										Views.Add(item);
										Selected = item;
									});

						return subscriptions;
					},
					(oldMenuItems, _, subscriptions) =>
					{
						foreach (IDisposable subscription in subscriptions)
							subscription.Dispose();
					})
				.For(_consumer);
		}
		
		public void ShowMenu()
		{
			var existing = Views.FirstOrDefault(vc => vc.Content is MenuItems);
			if (existing == null)
			{
				var newmenu = _objectProvider.Get<MenuItems>();
				var newItem = new ViewContainer("Menu", newmenu);
				Views.Add(newItem);
				Selected = newItem;
			}
			else
			{
				Selected = existing;
			}
		}


		public ItemActionCallback ClosingTabItemHandler => ClosingTabItemHandlerImpl;

		private void ClosingTabItemHandlerImpl(ItemActionCallbackArgs<TabablzControl> args)
		{
			var container = (ViewContainer)args.DragablzItem.DataContext;//.DataContext;
			if (container.Equals(Selected))
			{
				Selected = Views.FirstOrDefault(vc => vc != container);
			}
			var disposable = container.Content as IDisposable;
			disposable?.Dispose();
		}

		public ObservableCollection<ViewContainer> Views { get; } = new ObservableCollection<ViewContainer>();

		public ViewContainer Selected
		{
			get => _selected;
			set => SetAndRaise(ref _selected, value);
		}

		public IInterTabClient InterTabClient { get; }

		public ICommand ShowMenuCommand => _showMenuCommand;

		public Command ShowInGitHubCommand { get; }

		public void Dispose()
		{
			_consumer.Dispose();
			foreach (var disposable in  Views.Select(vc=>vc.Content).OfType<IDisposable>())
				disposable.Dispose();
		}
	}
}
