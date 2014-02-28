using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XSocketsClient;
using XSocketsClient.Common.Event.Interface;

namespace StockTickerClient
{
    public class StockListViewModel
    {
        private readonly SynchronizationContext _context;
        public ObservableCollection<Stock> Stocks { get; private set; }

        private readonly XSocketClient _client;
        
        public StockListViewModel()
        {
            _context = SynchronizationContext.Current;

            Stocks = new ObservableCollection<Stock>();

            //_client = new XSocketClient("ws://localhost:15820/Stock", "*");
            _client = new XSocketClient("wss://localhost:44305/Stock", "*");
            
        }

        public async void Start()
        {
            await _client.Open();
            Debug.WriteLine("Connection Open");
            await _client.Bind("allStocks", OnAllStocks);
            await _client.Bind("tick", OnTick); 
            Debug.WriteLine("Listening for ticks");
        }

        private void OnAllStocks(ITextArgs args)
        {
            Debug.WriteLine("OnAllStocks");
            var stocks = JsonConvert.DeserializeObject<IList<Stock>>(args.data);

            RunOnContext(() =>
            {
                foreach (var stock in stocks)
                    AddOrUpdateStock(stock);
            });
        }

        private void OnTick(ITextArgs args)
        {
            Debug.WriteLine("Tick");

            var stock = JsonConvert.DeserializeObject<Stock>(args.data);

            RunOnContext(() => AddOrUpdateStock(stock));
        }

        private void AddOrUpdateStock(Stock stock)
        {

            var i = Stocks.IndexOf(stock);
            if (i >= 0)
            {
                var s = Stocks[i];

                s.Change = stock.Change;
                s.DayClose = stock.DayClose;
                s.DayHigh = stock.DayHigh;
                s.DayLow = stock.DayLow;
                s.DayOpen = stock.DayOpen;
                s.LastChange = stock.LastChange;
                s.PercentChange = stock.PercentChange;
                s.Price = stock.Price;
            }
            else
            {
                Stocks.Add(stock);
            }
        }

        private void RunOnContext(Action action)
        {
            if (_context != null)
            {
                _context.Post(_ => action(), null);
            }
            else
            {
                action();
            }
        }
    }
}
