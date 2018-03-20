using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.IServices;
using Himall.Model;

namespace Himall.Web.Controllers
{
    /// <summary>
    /// 直通精灵
    /// </summary>
    public class ZTJLController : Controller
    {
        IZTJLService _iZTJLService;
        public ZTJLController( IZTJLService iZTJLService )
        {
            _iZTJLService = iZTJLService;
        }

        [HttpPost]
        public ActionResult ImportOrder()
        {
            string a = Request.Form[ "a" ];
            string b = Request.Form[ "b" ];
            string c = Request.Form[ "c" ];
            ZTJLOrderModel model = new ZTJLOrderModel();
            try
            {
                _iZTJLService.ImportOrder( model );
                return Json( new { result = true } );
            }
            catch
            {
                return Json( new { result = false } );
            }

        }
    }
}