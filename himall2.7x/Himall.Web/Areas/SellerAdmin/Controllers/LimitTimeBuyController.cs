using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Himall.Web.App_Code.Common;
using Himall.CommonModel;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class LimitTimeBuyController : BaseSellerController
    {
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private IMarketService _iMarketService;
        private IOrderService _iOrderService;
        private IShopService _iShopService;
        private IProductService _iProductService;
        private IFightGroupService _iFightGroupService;

        public LimitTimeBuyController(ILimitTimeBuyService iLimitTimeBuyService,
            IMarketService iMarketService,
            IOrderService iOrderService,
            IShopService iShopService,
            IProductService iProductService,
            IFightGroupService iFightGroupService
            )
        {
            _iLimitTimeBuyService = iLimitTimeBuyService;
            _iOrderService = iOrderService;
            _iShopService = iShopService;
            _iProductService = iProductService;
            _iMarketService = iMarketService;
            _iFightGroupService = iFightGroupService;
        }
        public ActionResult Management()
        {
            var settings = _iLimitTimeBuyService.GetServiceSetting();
            if (settings == null)
                return View("Nosetting");

            bool expired = false;
            ActiveMarketServiceInfo market = _iLimitTimeBuyService.GetMarketService(CurrentSellerManager.ShopId);
            if (market == null || market != null && ((Himall.Model.ActiveMarketServiceInfo)market).MarketServiceRecordInfo.Max(a => a.EndTime) < DateTime.Now.Date)
            {
                expired = true;
            }
            ViewBag.IsExpired = expired;
            return View();
        }

        public ActionResult BuyService()
        {
            var market = _iLimitTimeBuyService.GetMarketService(CurrentSellerManager.ShopId);
            ViewBag.Market = market;
            string endDate = null;
            if (market != null && market.MarketServiceRecordInfo.Max(m => m.EndTime) < DateTime.Now)
            {
                endDate = "您的限时购服务已经过期，您可以续费。";
            }
            else if (market != null && market.MarketServiceRecordInfo.Max(m => m.EndTime) > DateTime.Now)
            {
                var date = market.MarketServiceRecordInfo.Max(m => m.EndTime);
                endDate = string.Format("{0} 年 {1} 月 {2} 日", date.Year, date.Month, date.Day);
            }
            bool expired = false;
            if (market == null || market != null && ((Himall.Model.ActiveMarketServiceInfo)market).MarketServiceRecordInfo.Max(a => a.EndTime) < DateTime.Now.Date)
            {
                expired = true;
            }
            ViewBag.IsExpired = expired;
            ViewBag.EndDate = endDate;
            ViewBag.Price = _iLimitTimeBuyService.GetServiceSetting().Price;
            return View();
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult GetItemList(int page, int rows, int? status, DateTime? StartDate, DateTime? EndDate, string ProductName)
        {
            var service = _iLimitTimeBuyService;
            if (!status.HasValue)
            {
                status = 0;
            }
            var result = service.GetAll(CurrentSellerManager.ShopId, status, ProductName, StartDate, EndDate, page, rows);
            var list = new List<FlashSaleModel>();
            var models = result.Models.ToList();
            foreach (var i in models)
            {
                //if (i.EndDate < DateTime.Now)
                //{
                //    i.Status = FlashSaleInfo.FlashSaleStatus.Ended;
                //}
                if (i.Status != FlashSaleInfo.FlashSaleStatus.WaitForAuditing && i.Status != FlashSaleInfo.FlashSaleStatus.AuditFailed && i.BeginDate > DateTime.Now && i.EndDate < DateTime.Now)
                {
                    i.Status = FlashSaleInfo.FlashSaleStatus.Ongoing;
                }
                else if (i.Status != FlashSaleInfo.FlashSaleStatus.WaitForAuditing && i.Status != FlashSaleInfo.FlashSaleStatus.AuditFailed && i.BeginDate > DateTime.Now)
                {
                    i.Status = FlashSaleInfo.FlashSaleStatus.NotBegin;
                }
                list.Add(new FlashSaleModel
                {
                    Id = i.Id,
                    BeginDate = i.BeginDate.ToString("yyyy-MM-dd HH:mm"),
                    EndDate = i.EndDate.ToString("yyyy-MM-dd HH:mm"),
                    ProductId = i.ProductId,
                    SaleCount = i.SaleCount,
                    ProductName = i.Himall_Products.ProductName,
                    StatusNum = (int)i.Status,
                    StatusStr = i.Status.ToDescription(),
                    LimitCountOfThePeople = i.LimitCountOfThePeople,
                    IsStarted = (i.BeginDate > DateTime.Now)
                });
            }
            var model = new { rows = list, total = result.Total };
            return Json(model);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult BuyService(int month)
        {
            Result result = new Result();
            try
            {
                var service = _iMarketService;
                service.OrderMarketService(month, CurrentSellerManager.ShopId, MarketType.LimitTimeBuy);
                result.success = true;
                result.msg = "购买服务成功";
            }
            catch (HimallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("购买服务出错", ex);
                result.msg = "购买服务出错！";
            }
            return Json(result);
        }

        public ActionResult EditLimitItem(long id)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            var cateArray = _iLimitTimeBuyService.GetServiceCategories();
            foreach (var cate in cateArray)
            {
                items.Add(new SelectListItem { Selected = false, Text = cate, Value = cate });
            }
            var i = _iLimitTimeBuyService.GetLimitTimeMarketItem(id);
            //状态修正
            // if (i.EndTime>DateTime.Now) i.AuditStatus = Himall.Model.LimitTimeMarketInfo.LimitTimeMarketAuditStatus.Ended;

            items.FirstOrDefault(c => c.Text.Equals(i.CategoryName)).Selected = true;
            ViewBag.Cate = items;
            var model = new LimitTimeMarketModel
            {
                Title = i.Title,
                CategoryName = i.CategoryName,
                StartTime = i.StartTime.ToString("yyyy-MM-dd HH:mm"),
                EndTime = i.EndTime.ToString("yyyy-MM-dd HH:mm"),
                ProductId = i.ProductId,
                MaxSaleCount = i.MaxSaleCount,
                ProductName = i.ProductName,
                ProductPrice = _iProductService.GetProduct(i.ProductId).MinSalePrice,
                AuditStatus = i.AuditStatus.ToDescription(),
                CancelReson = i.CancelReson,
                Price = i.Price,
                Stock = i.Stock
            };
            return View(model);
        }

        [HttpPost]
        public JsonResult EditLimitItem(string Title, string ProductName, long ProductId, decimal Price, string CategoryName,
            DateTime StartTime, DateTime EndTime, int Stock, int MaxSaleCount, long Id)
        {
            Result result = new Result();
            try
            {
                var info = _iLimitTimeBuyService.GetLimitTimeMarketItem(Id);
                info.Title = Title;
                info.ProductId = ProductId;
                info.ProductName = ProductName;
                info.CategoryName = CategoryName;
                info.StartTime = StartTime;
                info.EndTime = EndTime;
                info.Price = Price;
                info.Stock = Stock;
                info.MaxSaleCount = MaxSaleCount;
                _iLimitTimeBuyService.UpdateLimitTimeItem(info);
                result.success = true;
                result.msg = "修改限时购成功";
            }
            catch (HimallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("修改限时购出错", ex);
                result.msg = "修改限时购出错！";
            }
            return Json(result);
        }

        [HttpGet]
        public ActionResult AddLimitItem()
        {
            LimitTimeMarketModel model = null;
            List<SelectListItem> items = new List<SelectListItem>();
            var cateArray = _iLimitTimeBuyService.GetServiceCategories();
            foreach (var cate in cateArray)
            {
                items.Add(new SelectListItem { Selected = false, Text = cate, Value = cate });
            }
            ViewBag.Cate = items;
            ViewBag.EndTime = _iLimitTimeBuyService.GetMarketService(CurrentSellerManager.ShopId).MarketServiceRecordInfo.Max(m => m.EndTime).ToString("yyyy-MM-dd");
            return View(model);
        }

        [HttpPost]
        public ActionResult GetDetailInfo(long productId)
        {
            var result = _iLimitTimeBuyService.GetDetailInfo(productId);
            result.ProductImg = Himall.Core.HimallIO.GetProductSizeImage(result.ProductImg, 1, (int)ImageSize.Size_50); ;
            return Json(result);
        }

        [HttpPost]
        public JsonResult AddLimitItem(string Title, string ProductName, long ProductId, decimal Price, string CategoryName,
            DateTime StartTime, DateTime EndTime, int MaxSaleCount, int Stock = 0)
        {
            Result result = new Result();
            try
            {
                var average = _iOrderService.GetRecentMonthAveragePrice(CurrentSellerManager.ShopId, ProductId);
                var pro = _iProductService.GetProduct(ProductId);
                LimitTimeMarketInfo info = new LimitTimeMarketInfo
                {
                    AuditStatus = LimitTimeMarketInfo.LimitTimeMarketAuditStatus.WaitForAuditing,
                    Title = Title,
                    ProductId = ProductId,
                    ProductName = ProductName,
                    CancelReson = "",
                    CategoryName = CategoryName,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    MaxSaleCount = MaxSaleCount,
                    Price = Price,
                    SaleCount = 0,
                    ShopId = CurrentSellerManager.ShopId,
                    ShopName = _iShopService.GetShop(CurrentSellerManager.ShopId).ShopName,
                    Stock = Stock,
                    AuditTime = StartTime,
                    RecentMonthPrice = average,
                    ImagePath = pro.ImagePath,
                    ProductAd = pro.ShortDescription,
                    MinPrice = pro.MinSalePrice
                };
                _iLimitTimeBuyService.AddLimitTimeItem(info);
                result.success = true;
                result.msg = "添加限时购成功";
            }
            catch (HimallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("添加限时购出错", ex);
                result.msg = "添加限时购出错！";
            }
            return Json(result);
        }


        public ActionResult Detail(long id)
        {
            var result = _iLimitTimeBuyService.Get(id);
            return View(result);
        }

        public JsonResult DeleteItem(long id)
        {
            Result result = new Result();
            _iLimitTimeBuyService.Delete(id,CurrentShop.Id);
            result.success = true;
            result.msg = "删除成功！";
            return Json(result);
        }


        public ActionResult Add()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            var cateArray = _iLimitTimeBuyService.GetServiceCategories();
            foreach (var cate in cateArray)
            {
                items.Add(new SelectListItem { Selected = false, Text = cate, Value = cate });
            }
            ViewBag.Cate = items;
            return View();
        }

        [HttpPost]
        public ActionResult Get(long id)
        {
            return Json(_iLimitTimeBuyService.Get(id));
        }

        [HttpPost]
        public ActionResult IsAdd(long productId)
        {
            bool result = _iLimitTimeBuyService.IsAdd(productId);

            if (result)
            {
                //拼团活动
                result = _iFightGroupService.ProductCanJoinActive(productId);
            }
            return Json(result);
        }

        [HttpPost]
        public ActionResult IsEdit(long productId, long id)
        {
            return Json(_iLimitTimeBuyService.IsEdit(productId, id));
        }

        public ActionResult Edit(long id)
        {
            var result = _iLimitTimeBuyService.Get(id);
            if (result.Status == FlashSaleInfo.FlashSaleStatus.Ongoing)
            {
                DateTime enddate = DateTime.Parse(result.EndDate);
                DateTime start = DateTime.Parse(result.BeginDate);
                if (start < DateTime.Now && enddate > DateTime.Now)
                {
                    throw new HimallException("进行中的活动不可编辑");
                }
            }

            List<SelectListItem> items = new List<SelectListItem>();
            var cateArray = _iLimitTimeBuyService.GetServiceCategories();
            foreach (var cate in cateArray)
            {
                if (cate == result.CategoryName)
                {
                    items.Add(new SelectListItem { Selected = true, Text = cate, Value = cate });
                }
                else
                {
                    items.Add(new SelectListItem { Selected = false, Text = cate, Value = cate });
                }

            }
            ViewBag.DataStr = JsonConvert.SerializeObject(result);
            ViewBag.Cate = items;
            return View(result);
        }

        public ActionResult EditFS(string fsmodel)
        {
            try
            {
                FlashSaleModel model = (FlashSaleModel)JsonConvert.DeserializeObject(fsmodel, typeof(FlashSaleModel));
                if (Convert.ToDateTime(model.BeginDate) > Convert.ToDateTime(model.EndDate))
                {
                    return Json(new Result { msg = "开始时间不能大于结束时间！", success = false });
                }
                model.ShopId = CurrentSellerManager.ShopId;
                _iLimitTimeBuyService.UpdateFlashSale(model);
                foreach (var d in model.Details)
                {
                    LimitOrderHelper.ModifyLimitStock(d.SkuId, d.Stock, DateTime.Parse(model.EndDate));
                }
                return Json(new Result { msg = "添加活动成功！", success = true });
            }
            catch (Exception ex)
            {
                return Json(new Result { msg = ex.Message, success = false });
            }
        }

        [HttpPost]
        public ActionResult AddFS(string fsmodel)
        {
            try
            {
                FlashSaleModel model = (FlashSaleModel)JsonConvert.DeserializeObject(fsmodel, typeof(FlashSaleModel));

                if (Convert.ToDateTime(model.BeginDate) > Convert.ToDateTime(model.EndDate))
                {
                    return Json(new Result { msg = "开始时间不能大于结束时间！", success = false });
                }
                if (!_iFightGroupService.ProductCanJoinActive(model.ProductId))
                {
                    return Json(new Result { msg = "该商品已参与拼团或其他营销活动，请重新选择！", success = false });
                }
                model.ShopId = CurrentSellerManager.ShopId;
                _iLimitTimeBuyService.AddFlashSale(model);
                foreach (var d in model.Details)
                {
                    LimitOrderHelper.AddLimitStock(d.SkuId, d.Stock, DateTime.Parse(model.EndDate));
                }
                return Json(new Result { msg = "添加活动成功！", success = true });
            }
            catch (Exception ex)
            {
                return Json(new Result { msg =ex.Message, success = false });
            }

        }
    }
}