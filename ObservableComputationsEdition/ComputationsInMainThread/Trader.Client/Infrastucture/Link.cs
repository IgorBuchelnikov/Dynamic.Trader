namespace Trader.Client.Infrastucture
{
	public class Link
	{
		public Link(string text,string display, string urlDynamicData, string urlObservableComputations)
		{
			Text = text;
			Display = display;
			UrlDynamicData = urlDynamicData;
			UrlObservableComputations = urlObservableComputations;
		}

		public string Text { get; }

		public string UrlDynamicData { get; }

		public string UrlObservableComputations { get; }

		public string Display { get; }

	}
}