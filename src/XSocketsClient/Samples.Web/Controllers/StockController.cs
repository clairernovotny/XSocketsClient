using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XSockets.MVC;

namespace Samples.Web.Controllers
{
    public class StockController : XSocketsControllerBase<StockTickerSample.Controllers.StockController>
    {
        //
        // GET: /Stock/
        public ActionResult Index()
        {
            return HttpNotFound("This endpoint is only available via WebSockets");
        }
	}
}