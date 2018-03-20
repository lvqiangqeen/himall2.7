using Himall.Core;
using Himall.Core.Plugins;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Areas.Admin.Models.Product;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class BussinessCategoryController : BaseSellerController
    {
        private IShopService _iShopService;
        private ICategoryService _iCategoryService;
        public BussinessCategoryController(IShopService iShopService, ICategoryService iCategoryService)
        {
            _iShopService = iShopService;
            _iCategoryService = iCategoryService;
        }
        public ActionResult Management()
        {
            return View();
        }

        public ActionResult ApplyList()
        {
            return View();
        }

        public JsonResult GetApplyList(int page, int rows)
        {
            BussinessCateApplyQuery query = new BussinessCateApplyQuery();
            query.PageNo = page;
            query.PageSize = rows;
            query.shopId = CurrentSellerManager.ShopId;
            var model = _iShopService.GetBusinessCateApplyList(query);
            var cate = model.Models.ToList().Select(a => new { Id = a.Id, ShopName = a.ShopName, ApplyDate = a.ApplyDate.ToString("yyyy-MM-dd HH:mm"), AuditedStatus = a.AuditedStatus.ToDescription() });
            var p = new { rows = cate.ToList(), total = model.Total };
            return Json(p);
        }

        public ActionResult ApplyDetail(long Id)
        {
            var model = _iShopService.GetBusinessCategoriesApplyInfo(Id);
            return View(model);
        }


        public ActionResult List(int page, int rows)
        {
            var model = _iShopService.GetBusinessCategory(CurrentSellerManager.ShopId);
            var result = model.OrderBy(a => a.Id).Skip((page - 1) * rows).Take(rows);

            var cate = result.ToList().Select(a => new { a.Id, a.CommisRate, a.CategoryName });
            var p = new { rows = cate.ToList(), total = model.Count() };
            return Json(p);
        }

        public ActionResult Apply()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetCategories(long? key = null, int? level = -1)
        {
            var categories = _iCategoryService.GetValidBusinessCategoryByParentId(key.GetValueOrDefault());
            var models = categories.Select(item => new KeyValuePair<long, string>(item.Id, item.Name));
            return Json(models);
        }

        public JsonResult GetBussinessCate(long id)
        {
            var categories = _iShopService.GetThirdBusinessCategory(id, CurrentSellerManager.ShopId);
            var t = categories.Select(item => new { id = item.Id, rate = item.Rate, path = item.Path });
            return Json(t, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ApplyBussinessCate(string categoryIds)
        {
            List<long> arr = new List<long>();
            var ids= Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(categoryIds, arr);
            _iShopService.ApplyShopBusinessCate(CurrentSellerManager.ShopId, ids);
            return Json(new Result() { success = true, msg = "申请成功" });
        }
    }
}