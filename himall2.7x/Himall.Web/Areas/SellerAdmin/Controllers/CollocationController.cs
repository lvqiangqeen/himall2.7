using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class CollocationController : BaseSellerController
    {
        private IMarketService _iMarketService;
        private MarketSettingInfo settings;
        private IProductService _iProductService;
        private ICollocationService _iCollocationService;

        public CollocationController(IMarketService iMarketService, ICollocationService iCollocationService, IProductService iProductService)
        {
            _iMarketService = iMarketService;
            settings = _iMarketService.GetServiceSetting(MarketType.Collocation);
            _iProductService = iProductService;
            _iCollocationService = iCollocationService;
        }

        [HttpPost]
        public JsonResult Cancel(long Id)
        {
            var shopId = CurrentSellerManager.ShopId;
            _iCollocationService.CancelCollocation(Id, shopId);
            return Json(new Result() { success = true, msg = "操作成功！" });
        }

        [HttpPost]

        public JsonResult GetProductSKU(long productId)
        {
            var skus = _iProductService.GetSKUs(productId).ToList();
            ProductSkuModel m = new ProductSkuModel();
            m.productId = productId;
            m.SKUs = skus.Select(a => new SKUModel()
            {
                Id = a.Id,
                SalePrice = a.SalePrice,
                Size = a.Size,
                Stock = a.Stock,
                Version = a.Version,
                Color = a.Color,
                Sku = a.Sku,
                AutoId = a.AutoId,
                ProductId = a.ProductId
            }).ToList();
            // var model=skus.Select(a=>new{a.ProductId,a.SalePrice,a.Size,a.Sku,a.Stock,a.Version});
            return Json(m);
        }
        public JsonResult GetProductsSku(string productIds)
        {
            var pros = productIds.Split(',').Select(a => long.Parse(a));
            var skus = _iProductService.GetSKUs(pros);
            var groupSkus = skus.GroupBy(a => a.ProductId);
            List<ProductSkuModel> model = new List<ProductSkuModel>();
            foreach (var s in groupSkus)
            {
                ProductSkuModel m = new ProductSkuModel();
                m.productId = s.Key;
                m.SKUs = s.Select(a => new SKUModel()
                {
                    Id = a.Id,
                    SalePrice = a.SalePrice,
                    Size = a.Size,
                    Stock = a.Stock,
                    Version = a.Version,
                    Color = a.Color,
                    Sku = a.Sku,
                    AutoId = a.AutoId,
                    ProductId = a.ProductId
                }).ToList();
                model.Add(m);
            }
            return Json(model);
        }

        public ActionResult Management()
        {
            if (settings == null)
                return View("Nosetting");
            SetExpire();
            return View();
        }
        public ActionResult BuyService()
        {
            if (settings == null)
                return View("Nosetting");
            SetExpire();
            return View(settings);
        }


        [HttpPost]
        public JsonResult List(int page, int rows, string collName)
        {
            var service = _iCollocationService;
            var result = service.GetCollocationList(new CollocationQuery { Title = collName, ShopId = CurrentSellerManager.ShopId, PageSize = rows, PageNo = page });
            var list = result.Models.ToList().Select(
                item => new
                {
                    Id = item.Id,
                    StartTime = item.StartTime.ToString("yyyy/MM/dd"),
                    EndTime = item.EndTime.ToString("yyyy/MM/dd"),
                    Title = item.Title,
                    ShopName = item.ShopName,
                    ProductId = item.ProductId,
                    Status = item.Status
                }
                );
            var model = new { rows = list, total = result.Total };
            return Json(model);
        }

        private void SetExpire()
        {
            var now = DateTime.Now.Date;
            var model = _iMarketService.GetMarketService(CurrentSellerManager.ShopId, MarketType.Collocation);
            bool expire = false;
            if (model == null)
            {
                ViewBag.IsBuy = false;
                expire = true;
            }
            else
            {
                ViewBag.IsBuy = true;
                var endTime = model.MarketServiceRecordInfo.Max(item => item.EndTime);
                if (endTime > now)
                {

                    ViewBag.EndDateInfo = endTime.ToString("yyyy年MM月dd日");
                }
                else
                {
                    expire = true;
                    ViewBag.EndDateInfo = "您的优惠券服务已经过期，您可以续费。";
                }
            }
            ViewBag.Expire = expire;
        }


        [HttpPost]
        public JsonResult BuyService(int month)
        {
            Result result = new Result();
            var service = _iMarketService;
            service.OrderMarketService(month, CurrentSellerManager.ShopId, MarketType.Collocation);
            result.success = true;
            result.msg = "购买服务成功";
            return Json(result);
        }
        public ActionResult Add()
        {

            ViewBag.EndTime = _iMarketService.GetMarketService(CurrentSellerManager.ShopId, MarketType.Collocation).MarketServiceRecordInfo.Max(item => item.EndTime).ToString("yyyy-MM-dd");
            return View();
        }

        public ActionResult Edit(long id)
        {
            var model = _iCollocationService.GetCollocation(id);
            if (model.ShopId != CurrentSellerManager.ShopId)
            {
                RedirectToAction("Management");
            }
            CollocationDataModel m = new CollocationDataModel();
            m.CreateTime = model.CreateTime.Value;
            m.EndTime = model.EndTime;
            m.ShopId = model.ShopId;
            m.ShortDesc = model.ShortDesc;
            m.Title = model.Title;
            m.StartTime = model.StartTime;
            m.Id = model.Id;
            m.CollocationPoruducts = model.Himall_CollocationPoruducts.Select(a =>
                {
                    var Sku = a.Himall_Products.SKUInfo.ToList(); ;
                    return new CollocationPoruductModel()
                   {
                       Id = a.Id,
                       ColloId = a.ColloId,
                       DisplaySequence = a.DisplaySequence.Value,
                       IsMain = a.IsMain,
                       ProductId = a.ProductId,
                       ProductName = a.Himall_Products.ProductName,
                       ImagePath = a.Himall_Products.ImagePath,
                       CollocationSkus = a.Himall_CollocationSkus.Select(b =>
                           {
                               var cosku = Sku.FirstOrDefault(t => t.Id == b.SkuID);
                               return new CollocationSkus()
                               {
                                   Id = b.Id,
                                   Price = b.Price,
                                   SkuID = b.SkuID,
                                   SKUName = cosku.Color + " " + cosku.Size + " " + cosku.Version,
                                   SkuPirce = b.SkuPirce.Value,
                                   ColloProductId = b.ColloProductId,
                                   ProductId = b.ProductId
                               };
                           }).ToList()
                   };

                }).OrderBy(a => a.DisplaySequence).ToList();
            //var s = new Newtonsoft.Json.JsonSerializerSettings();
            //s.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            //var t = Newtonsoft.Json.JsonConvert.SerializeObject(m, s);
            //ViewBag.ColloJson = t;
            ViewBag.EndTime = _iMarketService.GetMarketService(CurrentSellerManager.ShopId, MarketType.Collocation).MarketServiceRecordInfo.Max(item => item.EndTime).ToString("yyyy-MM-dd");
            return View(m);
        }


        [HttpPost]

        public ActionResult EditCollocation(string collocationjson)
        {
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<CollocationDataModel>(collocationjson);
            if (model == null)
            {
                throw new HimallException("组合购参数错误！");
            }
            if (model.Title.Trim().Length <= 0)
            {
                throw new HimallException("组合购的标题不能为空！");
            }
            if (model.ShortDesc.Length>=500)
            {
                throw new HimallException("组合描述不能超过500字！");
            }

            Himall.Model.CollocationInfo m = new CollocationInfo();
            m.EndTime = model.EndTime;
            m.ShopId = CurrentSellerManager.ShopId;
            m.ShortDesc = model.ShortDesc;
            m.Title = model.Title;
            m.StartTime = model.StartTime;
            m.Id = model.Id;
            m.Himall_CollocationPoruducts = model.CollocationPoruducts.Select(a => new Himall.Model.CollocationPoruductInfo
            {
                Id = a.Id,
                ColloId = a.ColloId,
                DisplaySequence = a.DisplaySequence,
                IsMain = a.IsMain,
                ProductId = a.ProductId,
                Himall_CollocationSkus = a.CollocationSkus.Select(b =>
                   new Himall.Model.CollocationSkuInfo()
                   {
                       Id = b.Id,
                       Price = b.Price,
                       SkuID = b.SkuID,
                       SkuPirce = b.SkuPirce,
                       ColloProductId = b.ColloProductId,
                       ProductId = b.ProductId
                   }).ToArray()
            }).ToArray();
            UpdateModel<CollocationInfo>(m);
            _iCollocationService.EditCollocation(m);
            return Json(new Result() { success = true, msg = "修改成功！" });
        }



        [HttpPost]
        public ActionResult AddCollocation(string collocationjson)
        {
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<CollocationDataModel>(collocationjson);
            if (model == null)
            {
                throw new HimallException("添加组合购参数错误！");
            }
            if (model.Title.Trim().Length <= 0)
            {
                throw new HimallException("组合购的标题不能为空！");
            }
            if (model.ShortDesc.Length >= 500)
            {
                throw new HimallException("组合描述不能超过500字！");
            }
            Himall.Model.CollocationInfo m = new CollocationInfo();
            m.CreateTime = DateTime.Now;
            m.EndTime = model.EndTime;
            m.ShopId = CurrentSellerManager.ShopId;
            m.ShortDesc = model.ShortDesc;
            m.Title = model.Title;
            m.StartTime = model.StartTime;
            m.Id = model.Id;
            m.Himall_CollocationPoruducts = model.CollocationPoruducts.Select(a => new Himall.Model.CollocationPoruductInfo
            {
                Id = a.Id,
                ColloId = a.ColloId,
                DisplaySequence = a.DisplaySequence,
                IsMain = a.IsMain,
                ProductId = a.ProductId,
                Himall_CollocationSkus = a.CollocationSkus.Select(b =>
                   new Himall.Model.CollocationSkuInfo()
                   {
                       Id = b.Id,
                       Price = b.Price,
                       SkuID = b.SkuID,
                       SkuPirce = b.SkuPirce,
                       ColloProductId = b.ColloProductId,
                       ProductId = b.ProductId
                   }).ToArray()
            }).ToArray();

            _iCollocationService.AddCollocation(m);
            return Json(new Result() { success = true, msg = "添加成功！" });
        }
    }
}