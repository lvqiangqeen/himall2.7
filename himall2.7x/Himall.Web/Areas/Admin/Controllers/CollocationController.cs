using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Himall.Core;
using System.Web;
using System.Web.Mvc;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.IServices;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Models;
using Himall.Core.Helper;
using System.IO;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class CollocationController : BaseAdminController
    {
        IMarketService _iMarketService;
        public CollocationController(IMarketService iMarketService)
        {
            _iMarketService = iMarketService;
        }

        #region 活动列表
        public ActionResult Management()
        {
           var  model = _iMarketService.GetServiceSetting(MarketType.Collocation);
            //if(model==null)
            //{
            //    return RedirectToAction("ServiceSetting");
            //}
            return View();
        }
        #endregion


        #region 服务购买列表
        public JsonResult List(string shopName, int page, int rows)
        {
            var queryModel = new MarketBoughtQuery()
            {
                PageSize = rows,
                PageNo = page,
                ShopName = shopName,
                MarketType = MarketType.Collocation
            };
            ObsoletePageModel<MarketServiceRecordInfo> marketEntities = _iMarketService.GetBoughtShopList(queryModel);

            var market = marketEntities.Models.OrderByDescending(m => m.MarketServiceId).ThenByDescending(m => m.EndTime).ToArray().Select(item => new
            {
                Id = item.Id,
                StartDate = item.StartTime.ToString("yyyy-MM-dd"),
                EndDate = item.EndTime.ToString("yyyy-MM-dd"),
                ShopName = item.ActiveMarketServiceInfo.ShopName
            });

            return Json(new { rows = market, total = marketEntities.Total });
        }
        #endregion

        #region 服务费用设置

        public ActionResult ServiceSetting()
        {
            MarketSettingInfo model = _iMarketService.GetServiceSetting(MarketType.Collocation);
            return View(model);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SaveServiceSetting(decimal Price)
        {
            Result result = new Result();
            var model = new MarketSettingInfo { Price = Price, TypeId = MarketType.Collocation };
            _iMarketService.AddOrUpdateServiceSetting(model);
            result.success = true;
            result.msg = "保存成功！";
            return Json(result);
        }
        #endregion


    }
}