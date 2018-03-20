using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.Admin.Models.Product;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class ProductController : BaseAdminController
    {
        private ISiteSettingService _iSiteSettingService;
        private IProductService _iProductService;
        private IBrandService _iBrandService;
        private ICategoryService _iCategoryService;
        private IShopService _iShopService;
        private ISearchProductService _iSearchProductService;
        public ProductController(
            IProductService iProductService,
            ISiteSettingService iSiteSettingService,
            IBrandService iBrandService,
            ICategoryService iCategoryService,
            IShopService iShopService,
            ISearchProductService iSearchProductService
            )
        {
            _iSiteSettingService = iSiteSettingService;
            _iProductService = iProductService;
            _iBrandService = iBrandService;
            _iCategoryService = iCategoryService;
            _iShopService = iShopService;
            _iSearchProductService = iSearchProductService;
        }
        // GET: Admin/Products
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Management()
        {
            return View();
        }

        [HttpPost]
        public JsonResult List(long? categoryId, string brandName, string productCode, int? auditStatus, string ids, int page, int rows, string keyWords, string shopName, int? saleStatus)
        {
            var queryModel = new ProductQuery()
            {
                PageSize = rows,
                PageNo = page,
                BrandNameKeyword = brandName,
                KeyWords = keyWords,
                CategoryId = categoryId,
                Ids = string.IsNullOrWhiteSpace(ids) ? null : ids.Split(',').Select(item => long.Parse(item)),
                ShopName = shopName,
                ProductCode = productCode,
                NotIncludedInDraft = true
            };
            if (auditStatus.HasValue)
            {
                queryModel.AuditStatus = new Model.ProductInfo.ProductAuditStatus[] { (Model.ProductInfo.ProductAuditStatus)auditStatus };
                if (auditStatus == (int)Model.ProductInfo.ProductAuditStatus.WaitForAuditing)
                    queryModel.SaleStatus = Model.ProductInfo.ProductSaleStatus.OnSale;
            }
            if (saleStatus.HasValue)
                queryModel.SaleStatus = (Himall.Model.ProductInfo.ProductSaleStatus)saleStatus;

            ObsoletePageModel<Himall.Model.ProductInfo> productEntities = _iProductService.GetProducts(queryModel);
            ICategoryService productCategoryService = _iCategoryService;
            IShopService shopService = _iShopService;
            IBrandService brandService = _iBrandService;
            IEnumerable<ProductModel> products = productEntities.Models.ToArray().Select(item => new ProductModel()
            {
                name = item.ProductName,
                brandName = item.BrandId == 0 ? "" : brandService.GetBrand(item.BrandId) == null ? "" : brandService.GetBrand(item.BrandId).Name,
                categoryName = productCategoryService.GetCategory(item.CategoryId) == null ? "" : productCategoryService.GetCategory(item.CategoryId).Name,
                id = item.Id,
                imgUrl = item.GetImage(ImageSize.Size_50),
                price = item.MinSalePrice,
                state = item.ShowProductState,
                auditStatus = (int)item.AuditStatus,
                url = "",
                auditReason = item.ProductDescriptionInfo != null ? item.ProductDescriptionInfo.AuditReason : "",
                shopName = shopService.GetShopBasicInfo(item.ShopId) == null ? "" : shopService.GetShopBasicInfo(item.ShopId).ShopName,
                saleStatus = (int)item.SaleStatus,
                productCode = item.ProductCode,
                saleCounts = item.SaleCounts,
                AddedDate = item.AddedDate.ToString("yyyy-MM-dd")
            });
            DataGridModel<ProductModel> dataGrid = new DataGridModel<ProductModel>() { rows = products, total = productEntities.Total };
            return Json(dataGrid);
        }

        public ActionResult ExportToExcel(long? categoryId = null, string brandName = "", string productCode = "", int? auditStatus = null, string ids = "", string keyWords = "", string shopName = "", int? saleStatus = null)
        {
            var queryModel = new ProductQuery()
            {
                PageSize = int.MaxValue,
                PageNo = 1,
                BrandNameKeyword = brandName,
                KeyWords = keyWords,
                CategoryId = categoryId,
                Ids = string.IsNullOrWhiteSpace(ids) ? null : ids.Split(',').Select(item => long.Parse(item)),
                ShopName = shopName,
                ProductCode = productCode,
                NotIncludedInDraft = true
            };
            if (auditStatus.HasValue)
            {
                queryModel.AuditStatus = new Model.ProductInfo.ProductAuditStatus[] { (Model.ProductInfo.ProductAuditStatus)auditStatus };
                if (auditStatus == (int)Model.ProductInfo.ProductAuditStatus.WaitForAuditing)
                    queryModel.SaleStatus = Model.ProductInfo.ProductSaleStatus.OnSale;
            }
            if (saleStatus.HasValue)
                queryModel.SaleStatus = (Himall.Model.ProductInfo.ProductSaleStatus)saleStatus;


            ObsoletePageModel<Himall.Model.ProductInfo> productEntities = _iProductService.GetProducts(queryModel);
            ICategoryService productCategoryService = _iCategoryService;
            IShopService shopService = _iShopService;
            IBrandService brandService = _iBrandService;
            IEnumerable<ProductModel> products = productEntities.Models.ToArray().Select(item => new ProductModel()
            {
                name = item.ProductName,
                brandName = item.BrandId == 0 ? "" : brandService.GetBrand(item.BrandId) == null ? "" : brandService.GetBrand(item.BrandId).Name,
                categoryName = productCategoryService.GetCategory(item.CategoryId) == null ? "" : productCategoryService.GetCategory(item.CategoryId).Name,
                id = item.Id,
                imgUrl = item.GetImage(ImageSize.Size_50),
                price = item.MinSalePrice,
                state = item.ShowProductState,
                auditStatus = (int)item.AuditStatus,
                url = "",
                auditReason = item.ProductDescriptionInfo != null ? item.ProductDescriptionInfo.AuditReason : "",
                shopName = shopService.GetShopBasicInfo(item.ShopId) == null ? "" : shopService.GetShopBasicInfo(item.ShopId).ShopName,
                saleStatus = (int)item.SaleStatus,
                productCode = item.ProductCode
            });

            #region 构建Excel文档
            ViewData.Model = products;
            string viewHtml = RenderPartialViewToString(this, "ExportProductinfo");
            return File(System.Text.Encoding.UTF8.GetBytes(viewHtml), "application/ms-excel", string.Format("平台商品信息_{0}.xls", DateTime.Now.ToString("yyyy-MM-dd")));
            #endregion
        }

        [NonAction]
        protected string RenderPartialViewToString(Controller controller, string partialViewName)
        {
            IView view = ViewEngines.Engines.FindPartialView(controller.ControllerContext, partialViewName).View;
            using (StringWriter writer = new StringWriter())
            {
                ViewContext viewContext = new ViewContext(controller.ControllerContext, view, controller.ViewData, controller.TempData, writer);
                viewContext.View.Render(viewContext, writer);
                return writer.ToString();
            }
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="auditState">审核状态</param>
        /// <param name="message">理由</param>
        /// <returns></returns>
        [OperationLog(Message = "审核商品状态")]
        [HttpPost]
        public JsonResult Audit(long productId, int auditState, string message)
        {
            Result result = new Result();
            try
            {
                Model.ProductInfo.ProductAuditStatus status = (Model.ProductInfo.ProductAuditStatus)auditState;
                _iProductService.AuditProduct(productId, status, message);
                #region 修改搜索商品状态
                _iSearchProductService.UpdateSearchStatusByProduct(productId);
                #endregion
                if (status != ProductInfo.ProductAuditStatus.Audited)
                {
                    //处理门店
                    ShopBranchApplication.UnSaleProduct(productId);
                }
                result.success = true;
                result.msg = "审核成功！";
            }
            catch (HimallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("审核出错", ex);
                result.msg = "审核出错！";
            }
            return Json(result);
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="auditState">审核状态</param>
        /// <param name="message">理由</param>
        /// <returns></returns>
        [OperationLog(Message = "批量审核商品状态")]
        [HttpPost]
        public JsonResult BatchAudit(string productIds, int auditState, string message)
        {
            var productIdsArr = productIds.Split(',').Select(item => long.Parse(item));

            Result result = new Result();
            try
            {
                Model.ProductInfo.ProductAuditStatus status = (Model.ProductInfo.ProductAuditStatus)auditState;
                _iProductService.AuditProducts(productIdsArr, status, message);
                _iSearchProductService.UpdateSearchStatusByProducts(productIdsArr.ToList());
                if (status != ProductInfo.ProductAuditStatus.Audited)
                {
                    foreach (var item in productIdsArr)
                    {
                        //处理门店
                        ShopBranchApplication.UnSaleProduct(item);
                    }
                }
                result.success = true;
                result.msg = "审核成功！";
            }
            catch (HimallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("审核出错", ex);
                result.msg = "审核出错！";
            }
            return Json(result);
        }
        [HttpPost]
        public JsonResult GetProductAuditOnOff()
        {
            var sitesetting = _iSiteSettingService.GetSiteSettings().ProdutAuditOnOff;
            return Json(new { value = sitesetting });
        }
        [HttpPost]
        public JsonResult SaveProductAuditOnOff(int value)
        {
            _iSiteSettingService.SaveSetting("ProdutAuditOnOff", value);
            return Json(new { success = true });
        }
    }
}