using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Core;
using Himall.DTO;

using Himall.Web.Framework;
using Himall.CommonModel;
using Himall.Application;

namespace Himall.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// 满减
    /// </summary>
    public class FullDiscountController : BaseAdminController
    {
        #region 服务购买列表

        public ActionResult BoughtList()
        {
            return View();
        }

        [UnAuthorize]
        public JsonResult GetBoughtList(string shopName, int page, int rows)
        {
            var data = FullDiscountApplication.GetMarketServiceBuyList(shopName, page, rows);
            return Json(new { rows = data.Models.ToList(), total = data.Total });
        }
        #endregion

        #region 服务费用设置

        public ActionResult ServiceSetting()
        {
            decimal model = FullDiscountApplication.GetMarketServicePrice();
            return View(model);
        }

        [HttpPost]
        public JsonResult ServiceSetting(decimal Price)
        {
            Result result = new Result();
            if(Price<0)
            {
                result.success = false;
                result.msg = "错误的服务价格！";
                return Json(result);
            }
            FullDiscountApplication.SetMarketServicePrice(Price);
            result.success = true;
            result.msg = "保存成功！";
            return Json(result);
        }
        #endregion
    }
}