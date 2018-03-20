using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Web.App_Code.Common;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class DistributionMarketController : BaseAdminController
    {
        IDistributionService _iDistributionService;
        public DistributionMarketController(IDistributionService iDistributionService)
        {

            _iDistributionService = iDistributionService;
        }

        public ActionResult Management()
        {
            #region 二维码
            string sharePath = Url.Action("Index", "DistributionMarket", new { Area = "Mobile" });
            string fullPath = ""; string strUrl = "";
            WeiXinHelper.CreateQCode(sharePath, out fullPath, out strUrl);

            ViewBag.ShopQUrl = fullPath;
            ViewBag.ShopQCode = strUrl;

            #endregion
            return View();
        }

        [HttpPost]
        public JsonResult List(int page, int rows, string productName, string shopName, int? status)
        {
            if (!string.IsNullOrEmpty(productName))
            {
                productName = productName.Trim();
            }
            if (!string.IsNullOrEmpty(shopName))
            {
                shopName = shopName.Trim();
            }
            DistributionQuery query = new DistributionQuery();
            query.PageNo = page;
            query.PageSize = rows;
            query.ProductName = productName;
            query.ShopName = shopName;
            if (status.HasValue && status.Value != -1)
                query.Status = (Himall.Model.ProductBrokerageInfo.ProductBrokerageStatus)status.Value;
            var m = _iDistributionService.GetDistributionlist(query);
            var model = m.Models.ToList();
            var dataGrid = new { rows = model, total = m.Total };
            return Json(dataGrid);
        }


        [HttpPost]
        public JsonResult UpdateDistrituionOrder(long pid, int sort = 0)
        {
            _iDistributionService.UpdateProductsDistributionOrder(pid, sort);
            return Json(new Result() { success = true, msg = "更新成功" });
        }


        // GET: Admin/Distribution
        public ActionResult Index()
        {
            ProformanceQuery query = new ProformanceQuery();
            query.PageNo = 1;
            query.PageSize = 9;
            var model = _iDistributionService.GetPerformanceList(query);

            return View(model);
        }


        public ActionResult Share()
        {
            var m = _iDistributionService.GetDistributionShare();
            return View(m);
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveShareSetting(Model.DistributionShareSetting model)
        {
            if (!string.IsNullOrEmpty(model.ProShareTitle)&& model.ProShareTitle.Length > 200)
            {
                return Json(new Result() { success = false, msg = "商品分享标题超过长度" });
            }
            if (!string.IsNullOrEmpty(model.ShopShareTitle)&&model.ShopShareTitle.Length > 200)
            {
                return Json(new Result() { success = false, msg = "店铺标题分享超过长度" });
            }
            if (!string.IsNullOrEmpty(model.DisShareTitle) && model.DisShareTitle.Length > 200)
            {
                return Json(new Result() { success = false, msg = "分销市场分享超过长度" });
            }
            if (!string.IsNullOrEmpty(model.RecruitShareTitle) && model.RecruitShareTitle.Length > 200)
            {
                return Json(new Result() { success = false, msg = "招募分享超过长度" });
            }
            if (!string.IsNullOrEmpty(model.ShopShareDesc) && model.ShopShareDesc.Length > 200)
            {
                return Json(new Result() { success = false, msg = "店铺分享描述过长" });
            }
            if (!string.IsNullOrEmpty(model.ProShareDesc) && model.ProShareDesc.Length > 200)
            {
                return Json(new Result() { success = false, msg = "商品分享描述过长" });
            }
            if (!string.IsNullOrEmpty(model.DisShareDesc) && model.DisShareDesc.Length > 200)
            {
                return Json(new Result() { success = false, msg = "分销市场分享描述过长" });
            }
            if (!string.IsNullOrEmpty(model.RecruitShareDesc) && model.RecruitShareDesc.Length > 200)
            {
                return Json(new Result() { success = false, msg = "招募分享描述过长" });
            }
            model.ShopShareDesc = FilterShareString(model.ShopShareDesc);
            model.ProShareDesc = FilterShareString(model.ProShareDesc);
            model.DisShareDesc = FilterShareString(model.DisShareDesc);
            model.RecruitShareDesc = FilterShareString(model.RecruitShareDesc);
            _iDistributionService.UpdateDistributionShare(model);
            return Json(new Result() { success = true, msg = "保存成功" });
        }
        private string FilterShareString(string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                str = str.Replace("\r","");
                str = str.Replace("\n", "");
            }
            return str;
        }

        public ActionResult Setting()
        {
            var m = _iDistributionService.GetDistributionSetting();
           if(!string.IsNullOrWhiteSpace(m.DisBanner))
               m.DisBanner = Core.HimallIO.GetRomoteImagePath(m.DisBanner);
            return View(m);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveSetting(Model.DistributorSettingInfo model)
        {
            //model.Enable = true;
            _iDistributionService.UpdateDistributorSetting(model);
            return Json(new Result() { success = true, msg = "保存成功" });
        }
    }
}