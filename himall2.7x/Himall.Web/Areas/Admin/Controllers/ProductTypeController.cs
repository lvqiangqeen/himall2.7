using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.Admin.Models.Product;
using Himall.Web.Framework;

using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Himall.Application;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class ProductTypeController : BaseAdminController
    {
        private ITypeService _iTypeService;
        private IOperationLogService _iOperationLogService;
        private IBrandService _iBrandService;
        public ProductTypeController(ITypeService iTypeService,
        IOperationLogService iOperationLogService,
        IBrandService iBrandService)
        {
            _iTypeService = iTypeService;
            _iOperationLogService = iOperationLogService;
            _iBrandService = iBrandService;
        }
        public ActionResult Management()
        {
            return View();
        }

        [HttpPost]
        [OperationLog("删除平台类目","id")]
        public JsonResult DeleteType(long Id)
        {
            Result result = new Result();
            try
            {
				TypeApplication.DeleteType(Id);
               // ServiceHelper.Create<IOperationLogService>().AddPlatformOperationLog(
               //new LogInfo
               //{
               //    Date = DateTime.Now,
               //    Description = "删除平台类目，Id=" + Id,
               //    IPAddress = Request.UserHostAddress,
               //    PageUrl = "/ProductType/DeleteTyp",
               //    UserName = CurrentManager.UserName,
               //    ShopId = 0

               //});
                result.success = true;
            }
            catch (HimallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("删除平台类目失败", ex);
                result.msg = "删除平台类目失败";
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult DataGridJson(string searchKeyWord, int page, int rows)
        {
            if (!string.IsNullOrWhiteSpace(searchKeyWord))
            {
                searchKeyWord = searchKeyWord.Trim();
            }
            var typePage = _iTypeService.GetTypes(searchKeyWord, page, rows);
            var data = typePage.Models.Select(item => new ProductTypeInfo
            {

                Id = item.Id,
                Name = item.Name

            });
            DataGridModel<ProductTypeInfo> dataGrid = new DataGridModel<ProductTypeInfo>() { rows = data, total = typePage.Total };
            return Json(dataGrid, JsonRequestBehavior.AllowGet);
        }

        private void TransformAttrs(ProductTypeInfo model)
        {
            foreach (var item in model.AttributeInfo)
            {
                StringBuilder value = new StringBuilder();
                foreach (var v in item.AttributeValueInfo.Select(c => c.Value))
                {
                    value.Append(v);
                    value.Append(',');
                };
                item.AttrValue = value.ToString().TrimEnd(',');
            }
        }

        private void TransformSpec(ProductTypeInfo model)
        {
            if (model.IsSupportColor)
            {
                StringBuilder value = new StringBuilder();
                foreach (var item in model.SpecificationValueInfo.Where(s => s.Specification == SpecificationType.Color && s.TypeId.Equals(model.Id)))
                {
                    value.Append(item.Value);
                    value.Append(',');
                }
                model.ColorValue = value.ToString().TrimEnd(',');
            }
            if (model.IsSupportSize)
            {
                StringBuilder value = new StringBuilder();
                foreach (var item in model.SpecificationValueInfo.Where(s => s.Specification == SpecificationType.Size && s.TypeId.Equals(model.Id)))
                {
                    value.Append(item.Value);
                    value.Append(',');
                }
                model.SizeValue = value.ToString().TrimEnd(',');
            }
            if (model.IsSupportVersion)
            {
                StringBuilder value = new StringBuilder();
                foreach (var item in model.SpecificationValueInfo.Where(s => s.Specification == SpecificationType.Version && s.TypeId.Equals(model.Id)))
                {
                    value.Append(item.Value);
                    value.Append(',');
                }
                model.VersionValue = value.ToString().TrimEnd(',');
            }
        }

        public JsonResult GetBrandsAjax(long id)
        {
            var brands = _iBrandService.GetBrands("");
            var model = id == 0 ? null : _iTypeService.GetType(id);
            var data = new List<BrandViewModel>();
            foreach (var brand in brands)
            {
                data.Add(new Models.Product.BrandViewModel
                {
                    id = brand.Id,
                    isChecked = null == model ? false : model.TypeBrandInfo.Any(b => b.BrandId.Equals(brand.Id)),
                    value = brand.Name
                });
            }
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(long id = 0)
        {
            var brands = _iBrandService.GetBrands("");
            ViewBag.Brands = brands.ToList();

            var newInfo = new ProductTypeInfo(true); 
            if (id == 0)
                return View(newInfo);
            else
            {
                var model = _iTypeService.GetType(id);
				if (model == null)
					return HttpNotFound();

                if (string.IsNullOrWhiteSpace(model.SizeValue) && string.IsNullOrWhiteSpace(model.ColorValue) && string.IsNullOrWhiteSpace(model.VersionValue))
                {
                    model.SizeValue = newInfo.SizeValue;
                    model.ColorValue = newInfo.ColorValue;
                    model.VersionValue = newInfo.VersionValue;
                }
                    
                TransformAttrs(model);
                TransformSpec(model);
                return View(model);
            }
        }

        [HttpPost]
        [OperationLog("修改平台类目","id")]
        public ActionResult SaveModel(TypeInfoModel type)
        {
            if (0 != type.Id)
            {
                _iTypeService.UpdateType(type);
            }
            else if (0 == type.Id)
            {
                _iTypeService.AddType(type);
            }
           // ServiceHelper.Create<IOperationLogService>().AddPlatformOperationLog(
           //new LogInfo
           //{
           //    Date = DateTime.Now,
           //    Description = "修改平台类目，Id=" + type.Id,
           //    IPAddress = Request.UserHostAddress,
           //    PageUrl = "/ProductType/SaveModel",
           //    UserName = CurrentManager.UserName,
           //    ShopId = 0

           //});
            return RedirectToAction("Management");
        }


    }
}