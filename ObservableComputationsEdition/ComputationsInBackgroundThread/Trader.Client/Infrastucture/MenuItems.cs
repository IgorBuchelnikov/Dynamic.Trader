using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ObservableComputations;
using Trader.Client.Views;
using Trader.Domain.Infrastucture;

namespace Trader.Client.Infrastucture
{
	public class MenuItems : AbstractNotifyPropertyChanged, IDisposable
	{
		private readonly ILogger _logger;
		private readonly IObjectProvider _objectProvider;
		private readonly ISubject<ViewContainer> _viewCreatedSubject = new Subject<ViewContainer>();
		private readonly Consumer _consumer = new Consumer();

		private bool _showLinks=false;
		private IEnumerable<MenuItem> _items;


		public MenuItems(ILogger logger, IObjectProvider objectProvider)
		{
			_logger = logger;
			_objectProvider = objectProvider;
			
			Items = new List<MenuItem>
			{
				new MenuItem("Live Trades",
					"A basic example, illustrating how to connect to a stream, inject a user filter and bind.",
					() => Open<LiveTradesViewer>("Live Trades"),new []
						{
							new Link("Service","TradeService.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/Trader.Domain/Services/TradeService.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/ObservableComputationsEdition/ComputationsInBackgroundThread/Trader.Domain/Services/TradeService.cs"), 
							new Link("View Model","LiveTradesViewer.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/Trader.Client/Views/LiveTradesViewer.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/ObservableComputationsEdition/ComputationsInBackgroundThread/Trader.Client/Views/LiveTradesViewer.cs"), 
							new Link("Blog","Ui Integration", "http://dynamic-data.org/2014/11/24/trading-example-part-3-integrate-with-ui/", null), 
						}),
				
				new MenuItem("Near to Market",
					 "Dynamic filtering of calculated values.",
					 () => Open<NearToMarketViewer>("Near to Market"),new []
						{
							new Link("Service","NearToMarketService.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/Trader.Domain/Services/NearToMarketService.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/ObservableComputationsEdition/ComputationsInBackgroundThread/Trader.Domain/Services/NearToMarketService.cs"),
							new Link("Rates Updater","TradePriceUpdateJob.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/Trader.Domain/Services/TradePriceUpdateJob.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/ObservableComputationsEdition/ComputationsInBackgroundThread/Trader.Domain/Services/TradePriceUpdateJob.cs"), 
							new Link("View Model", "NearToMarketViewer.cs","https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/Trader.Client/Views/NearToMarketViewer.cs","https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/ObservableComputationsEdition/ComputationsInBackgroundThread/Trader.Client/Views/NearToMarketViewer.cs"), 
							new Link("Blog i","Manage Market Data", "http://dynamic-data.org/2014/11/22/trading-example-part-2-manage-market-data/", null),
							new Link("Blog ii","Filter on calculated values", "http://dynamic-data.org/2014/12/21/trading-example-part-4-filter-on-calculated-values/", null),
							
						}),

				new MenuItem("Trades By %",  
					   "Group trades by a constantly changing calculated value. With automatic regrouping.",
						() => Open<TradesByPercentViewer>("Trades By % Diff"),new []
							{
								new Link("View Model", "TradesByPercentViewer.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/Trader.Client/Views/TradesByPercentViewer.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/ObservableComputationsEdition/ComputationsInBackgroundThread/Trader.Client/Views/TradesByPercentViewer.cs"), 
								new Link("Group Model","TradesByPercentDiff.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/Trader.Domain/Model/TradesByPercentDiff.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/ObservableComputationsEdition/ComputationsInBackgroundThread/Trader.Domain/Model/TradesByPercentDiff.cs"),
							}),

				new MenuItem("Trades By hh:mm",   
					   "Group items by time with automatic regrouping as time passes",
						() => Open<TradesByTimeViewer>("Trades By hh:mm"),new []
						{
							new Link("View Model","TradesByTimeViewer.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/Trader.Client/Views/TradesByTimeViewer.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/ObservableComputationsEdition/ComputationsInBackgroundThread/Trader.Client/Views/TradesByTimeViewer.cs"), 
							new Link("Group Model","TradesByTime.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/Trader.Domain/Model/TradesByTime.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/ObservableComputationsEdition/ComputationsInBackgroundThread/Trader.Domain/Model/TradesByTime.cs"),
						}),
				
				new MenuItem("Recent Trades",   
					"Operator which only includes trades which have changed in the last minute.",
					() => Open<RecentTradesViewer>("Recent Trades"),new []
					{
						new Link("View Model", "RecentTradesViewer.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/Trader.Client/Views/RecentTradesViewer.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/ObservableComputationsEdition/ComputationsInBackgroundThread/Trader.Client/Views/RecentTradesViewer.cs"), 
					}),

					
				new MenuItem("Trading Positions",   
					   "Calculate overall position for each currency pair and aggregate totals",
						() => Open<PositionsViewer>("Trading Positions"),new []
					{
						new Link("View Model", "PositionsViewer.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/Trader.Client/Views/PositionsViewer.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/ObservableComputationsEdition/ComputationsInBackgroundThread/Trader.Client/Views/PositionsViewer.cs"), 
						new Link("Group Model", "CurrencyPairPosition.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/Trader.Domain/Model/CurrencyPairPosition.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/ObservableComputationsEdition/ComputationsInBackgroundThread/Trader.Domain/Model/CurrencyPairPosition.cs"), 
					}),


				  new MenuItem("Paged Data",
					"An advanced example of how to page data",
					() => Open<PagedDataViewer>("Paged Data"),new []
						{
							new Link("View Model","PagedDataViewer.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/Trader.Client/Views/PagedDataViewer.cs", "https://github.com/IgorBuchelnikov/Dynamic.Trader/blob/master/ObservableComputationsEdition/ComputationsInBackgroundThread/Trader.Client/Views/PagedDataViewer.cs"),
							new Link("Blog","Sort Filter and Page Data", " http://dynamic-data.org/2015/04/22/dynamically-sort-filter-and-page-data/", null),
						}),
			};
		}

		private void Open<T>(string title)
		{

			_logger.Debug("Opening '{0}'", title);
		   
			var content = _objectProvider.Get<T>();
			_viewCreatedSubject.OnNext(new ViewContainer(title, content));
			_logger.Debug("--Opened '{0}'", title);
		}

		public IEnumerable<MenuItem> Items
		{
			get => _items;
			set => SetAndRaise(ref _items, value);
		}

		public bool ShowLinks
		{
			get => _showLinks;
			set => SetAndRaise(ref _showLinks, value);
		}
		
		public IObservable<ViewContainer> ItemCreated => _viewCreatedSubject.AsObservable();

		public void Dispose()
		{
			_consumer.Dispose();
			_viewCreatedSubject.OnCompleted();
		}
	}
}