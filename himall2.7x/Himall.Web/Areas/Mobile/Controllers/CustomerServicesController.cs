using Himall.Application;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class CustomerServicesController : BaseMobileTemplatesController
    {
        // GET: Mobile/CustomerServices
        public ActionResult PlatCustomerServices()
        {
            var services = CustomerServiceApplication.GetPlatformCustomerService(true, true);
            var meiqia = CustomerServiceApplication.GetPlatformCustomerService(true, false).FirstOrDefault(p => p.Tool == Model.CustomerServiceInfo.ServiceTool.MeiQia);
            if (meiqia != null)
                services.Insert(0, meiqia);
            return View(services);
        }

        public ActionResult ShopCustomerServices(long shopId)
        {
            var customerServices = CustomerServiceApplication.GetMobileCustomerService(shopId);
            var meiqia = CustomerServiceApplication.GetPreSaleByShopId(shopId).FirstOrDefault(p => p.Tool == CustomerServiceInfo.ServiceTool.MeiQia);
            if (meiqia != null)
                customerServices.Insert(0, meiqia);
            //ViewBag.CustomerServices = customerServices;
            return View("PlatCustomerServices",customerServices);
        }
    }
}