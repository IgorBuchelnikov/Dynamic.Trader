using System.Windows.Controls;

namespace Trader.Client.Views
{
	/// <summary>
	/// Interaction logic for PagedDataView.xaml
	/// </summary>
	public partial class PagedDataView : UserControl
	{
		public PagedDataView()
		{
			InitializeComponent();
			//this.Loaded += (s, a) =>
			//{
			//	Thread t = new Thread(() =>
			//	{
			//		for (int i = 0; i < 100; i++)
			//		{
			//			var i1 = i;
			//			Thread.Sleep(1000);
			//			Dispatcher.Invoke(() =>
			//			{
			//				SortCombo.SelectedIndex = i1 % 3;
			//			});
			//		}
			//	});

			//	t.Start();
			//};
		}
	}
}
