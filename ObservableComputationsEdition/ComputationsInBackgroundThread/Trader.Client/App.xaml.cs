using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ObservableComputations;
using StructureMap;
using Trader.Client.Infrastucture;
using Trader.Domain.Infrastucture;
using Trader.Domain.Services;

namespace Trader.Client
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private Container _container;
		private IContainer _containerOcDispatcher;

		/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Application" /> class.</summary>
		/// <exception cref="T:System.InvalidOperationException">More than one instance of the <see cref="T:System.Windows.Application" /> class is created per <see cref="T:System.AppDomain" />.</exception>
		public App()
		{
			ObservableComputations.OcConfiguration.SaveInstantiationStackTrace = false;
			ObservableComputations.OcConfiguration.EventUnsubscriberThreadsCount = 4;
			AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

			ShutdownMode = ShutdownMode.OnLastWindowClose;
		}

		private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			Console.WriteLine(args.LoadedAssembly);
		}

		/// <summary>Raises the <see cref="E:System.Windows.Application.Startup" /> event.</summary>
		/// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
		protected override void OnStartup(StartupEventArgs e)
		{
			_container = new Container(x => x.AddRegistry<AppRegistry>());
			var factory = _container.GetInstance<WindowFactory>();
			var window = factory.Create(true);
			_container.Configure(x => x.For<Dispatcher>().Add(window.Dispatcher));

			_container.Configure(x => x.For<WpfOcDispatcher>().Add(new WpfOcDispatcher(window.Dispatcher)));

			_containerOcDispatcher =  _container.GetNestedContainer();
			OcDispatcher backgroundOcDispatcher = new OcDispatcher(2);
			_containerOcDispatcher.Configure(x => x.For<OcDispatcher>().Add(backgroundOcDispatcher));
			_container.Configure(x => x.For<OcDispatcher>().Add(() => backgroundOcDispatcher));

			_container.Configure(x => x.For<UserInputThrottlingOcDispatcher>().Add(
				new UserInputThrottlingOcDispatcher(backgroundOcDispatcher)));

			//configure dependency resolver for RxUI / Splat
			//var resolver = new ReactiveUIDependencyResolver(_container);
			//resolver.Register(() => new LogEntryView(), typeof(IViewFor<LogEntryViewer>));
			//Locator.Current = resolver;
			//RxApp.SupportsRangeNotifications = false;

			//run start up jobs


			_container.GetInstance<TradePriceUpdateJob>();
			_container.GetInstance<TradeAgeUpdateJob>();

			window.Show();
			base.OnStartup(e);
		}

		private void App_OnExit(object sender, ExitEventArgs e)
		{
			OcDispatcher backgroundOcDispatcher = _container.GetInstance<OcDispatcher>();

			_container.Dispose();

			while (!RecurringAction.AllInstancesIsDisposed)
				Dispatcher.Invoke(() => { }, DispatcherPriority.Background);

			while (backgroundOcDispatcher.GetQueueCount(0) + backgroundOcDispatcher.GetQueueCount(1) > 0)
				Thread.Sleep(10);				
			
			_containerOcDispatcher.Dispose();
		}
	}
}
