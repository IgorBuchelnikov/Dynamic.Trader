using System;
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
		public static Container Container;

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
			Container = new Container(x => x.AddRegistry<AppRegistry>());
			var factory = Container.GetInstance<WindowFactory>();
			var window = factory.Create(true);
			Container.Configure(x => x.For<Dispatcher>().Add(window.Dispatcher));

			Container.Configure(x => x.For<WpfOcDispatcher>().Add(new WpfOcDispatcher(window.Dispatcher)));

			OcDispatcher backgroundOcDispatcher = new OcDispatcher(2);
			Container.Configure(x => x.For<OcDispatcher>().Add(backgroundOcDispatcher));

			Container.Configure(x => x.For<UserInputThrottlingOcDispatcher>().Add(
				new UserInputThrottlingOcDispatcher(backgroundOcDispatcher)));

			//configure dependency resolver for RxUI / Splat
			//var resolver = new ReactiveUIDependencyResolver(Container);
			//resolver.Register(() => new LogEntryView(), typeof(IViewFor<LogEntryViewer>));
			//Locator.Current = resolver;
			//RxApp.SupportsRangeNotifications = false;

			//run start up jobs


			Container.GetInstance<TradePriceUpdateJob>();
			Container.GetInstance<TradeAgeUpdateJob>();

			window.Show();
			base.OnStartup(e);
		}
	}
}
