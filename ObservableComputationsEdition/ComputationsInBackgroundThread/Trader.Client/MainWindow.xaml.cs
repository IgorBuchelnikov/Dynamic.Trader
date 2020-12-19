using System;
using System.ComponentModel;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using Trader.Domain.Infrastucture;

namespace Trader.Client
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void MainWindow_OnClosing(object sender, CancelEventArgs e)
		{
			App.Container.Dispose();

			while (!RecurringAction.AllInstancesIsDisposed)
				Dispatcher.Invoke(() => { }, DispatcherPriority.Background);

		}
	}
}
