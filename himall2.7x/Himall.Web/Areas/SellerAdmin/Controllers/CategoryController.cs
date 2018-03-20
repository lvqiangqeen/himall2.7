using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class CategoryController : BaseSellerController
    {
        private ICategoryService _iCategoryService;
        private IShopCategoryService _iShopCategoryService;
        public CategoryController(ICategoryService iCategoryService, IShopCategoryService iShopCategoryService)
        {
            _iCategoryService = iCategoryService;
            _iShopCategoryService = iShopCategoryService;
        }

        // GET: SellerAdmin/Category
        public ActionResult Management()
        {
            var ICategory = _iShopCategoryService;
            var firstLevel = ICategory.GetMainCategory(CurrentSellerManager.ShopId);
            List<ShopCategoryModel> list = new List<ShopCategoryModel>();
            foreach (var item in firstLevel)
            {
                list.Add(new ShopCategoryModel(item));
            }
            return View(list);
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult GetEffectCategory(long categoryId)
        {
            var cate = _iCategoryService.GetCategory(categoryId);
            string names = _iCategoryService.GetEffectCategoryName(CurrentSellerManager.ShopId, cate.TypeId);
            return Json(new { json = names }, JsonRequestBehavior.AllowGet);
        }

        [UnAuthorize]
        public JsonResult GetCategoryDrop(long id = 0)
        {
            List<SelectListItem> cateList = new List<SelectListItem>{ new SelectListItem
                {
                    Selected = id==0,
                    Text ="请选择...",
                    Value = "0"
                }
            };
            var cateMain = _iShopCategoryService.GetMainCategory(CurrentSellerManager.ShopId);
            foreach (var item in cateMain)
            {
                cateList.Add(new SelectListItem
                {
                    Selected = id == item.Id,
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }
            return Json(new { successful = true, category = cateList }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ShopOperationLog("创建店铺分类", "pid,name")]
        public JsonResult CreateCategory(string name, long pId)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 12)
                throw new Exception("分类名称长度不能多于12位");



            var cate = new Model.ShopCategoryInfo
            {
                Name = name,
                ParentCategoryId = pId,
                IsShow = true,
                DisplaySequence = _iShopCategoryService.GetCategoryByParentId(pId).Count() + 1,
                ShopId = CurrentSellerManager.ShopId
            };
            _iShopCategoryService.AddCategory(cate);
           // ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(
           //new LogInfo
           //{
           //    Date = DateTime.Now,
           //    Description = "创建店铺分类，父Id=" + pId,
           //    IPAddress = Request.UserHostAddress,
           //    PageUrl = "/Category/CreateCategory",
           //    UserName = CurrentSellerManager.UserName,
           //    ShopId = CurrentSellerManager.ShopId
           //});
            return Json(new { successful = true }, JsonRequestBehavior.AllowGet);
        }

        [ShopOperationLog("修改店铺分类名称", "id,name")]
        public JsonResult UpdateName(string name, long id)
        {
            _iShopCategoryService.UpdateCategoryName(id, name);

            return Json(new { Successful = true }, JsonRequestBehavior.AllowGet);
        }

        [UnAuthorize]
        public JsonResult UpdateOrder(long order, long id)
        {
            _iShopCategoryService.UpdateCategoryDisplaySequence(id, order);
            return Json(new { Successful = true }, JsonRequestBehavior.AllowGet);
        }

        [UnAuthorize]
        public ActionResult GetCategoryByParentId(int id)
        {
            List<ShopCategoryModel> list = new List<ShopCategoryModel>();
            var categoryList = _iShopCategoryService.GetCategoryByParentId(id);
            foreach (var item in categoryList)
            {
                list.Add(new ShopCategoryModel(item));
            }
            return Json(new { Successful = true, Category = list }, JsonRequestBehavior.AllowGet);
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult GetCategory(long? key = null, int? level = -1)
        {
            if (level == -1)
                key = 0;

            if (key.HasValue)
            {
                var categories = _iShopCategoryService.GetCategoryByParentId(key.Value, CurrentSellerManager.ShopId);
                var cateoriesPair = categories.Select(item => new KeyValuePair<long, string>(item.Id, item.Name));
                return Json(cateoriesPair);
            }
            else
                return Json(new object[] { });
        }


        [UnAuthorize]
        [HttpPost]
        public JsonResult GetSystemCategory(long? key = null, int? level = -1)
        {
            if (level == -1)
                key = 0;

            if (key.HasValue)
            {
                var categories = _iCategoryService.GetCategoryByParentId(key.Value);
                var cateoriesPair = categories.Select(item => new KeyValuePair<long, string>(item.Id, item.Name));
                return Json(cateoriesPair);
            }
            else
                return Json(new object[] { });
        }

        [HttpPost]
        [ShopOperationLog("删除店铺分类", "id")]
        public JsonResult DeleteCategoryById(long id)
        {
            _iShopCategoryService.DeleteCategory(id, CurrentSellerManager.ShopId);
           // ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(
           //new LogInfo
           //{
           //    Date = DateTime.Now,
           //    Description = "删除店铺分类，Id=" + id ,
           //    IPAddress = Request.UserHostAddress,
           //    PageUrl = "/Category/DeleteCategoryById",
           //    UserName = CurrentSellerManager.UserName,
           //    ShopId = CurrentSellerManager.ShopId
           //});
            return Json(new { Successful = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult BatchDeleteCategory(string Ids)
        {
            int id;
            foreach (var idStr in Ids.Split('|'))
            {
                if (string.IsNullOrWhiteSpace(idStr)) continue;
                if (int.TryParse(idStr, out id))
                {
                    _iShopCategoryService.DeleteCategory(id, CurrentSellerManager.ShopId);
                }
            }
            return Json(new { Successful = true }, JsonRequestBehavior.AllowGet);
        }
    }
}