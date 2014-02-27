using System.Collections.Generic;
using System.Linq;
using StockTickerSample.Model;
using XSockets.Core.XSocket;
using XSockets.Core.XSocket.Helpers;

namespace StockTickerSample.Controllers
{
    public class StockController : XSocketController
    {
        //We have state :) Each user can listen to stocks of choice
        public List<string> MyStocks { get; set; }

        public StockController()
        {
            //By default, listen to all stocks
            MyStocks = StockTicker.Stocks.Values.Select(p => p.Symbol).ToList();

            this.OnOpen += StockController_OnOpen;
        }

        void StockController_OnOpen(object sender, XSockets.Core.Common.Socket.Event.Arguments.OnClientConnectArgs e)
        {
            //Send to available stocks to the client when he/she´s connected. No need to ask for them.
            this.Send(StockTicker.Stocks.Values, "allStocks");
        }

        /// <summary>
        /// Do a conditional send to only clients listening for the actual stock
        /// </summary>
        /// <param name="stock"></param>
        public void Tick(Stock stock)
        {
            //Send only to client having this stock in their list
            this.SendTo(p => p.MyStocks.Contains(stock.Symbol), stock, "tick");
        }
    }
}
