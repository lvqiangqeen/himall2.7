using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.IServices;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Model;
using Himall.Core;
using Himall.Web.Models;
using Himall.Core.Helper;
using System.IO;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class ShopBonusController : BaseSellerController
    {
        private IShopBonusService _bonusService;
        private IMarketService _iMarketService;
        public ShopBonusController(IShopBonusService iShopBonusService, IMarketService iMarketService)
        {
            this._bonusService = iShopBonusService;
            _iMarketService = iMarketService;
        }

        public ActionResult Management()
        {
            var settings = _iMarketService.GetServiceSetting(MarketType.RandomlyBonus);
            if (settings == null)
            {
                return View("UnSet");
            }

            var market = this._bonusService.GetShopBonusService(CurrentSellerManager.ShopId);

            if (market == null || market != null && ((Himall.Model.ActiveMarketServiceInfo)market).MarketServiceRecordInfo.Max(a => a.EndTime) < DateTime.Now.Date)
            {
                return RedirectToAction("BuyService");
            }
            ViewBag.Market = market;
            return View();
        }

        public ActionResult Add()
        {
            return View();
        }

        public ActionResult Edit(long id)
        {
            var obj = this._bonusService.Get(id);
            ShopBonusModel model = new ShopBonusModel(obj);
            return View(model);
        }

        public ActionResult Detail(long id)
        {
            ViewBag.Id = id;
            return View();
        }

        public ActionResult UnConfig()
        {
            return View();
        }

        public ActionResult UnSet()
        {
            return View();
        }

        [HttpPost]
        public ActionResult List(string name, int state, int page = 1, int rows = 20)
        {
            var result = this._bonusService.Get(CurrentSellerManager.ShopId, name.Trim(), state, page, rows);
            List<ShopBonusModel> datas = new List<ShopBonusModel>();
            foreach (var m in result.Models)
            {
                datas.Add(new ShopBonusModel(m));
            }
            DataGridModel<ShopBonusModel> model = new DataGridModel<ShopBonusModel>
            {
                rows = datas,
                total = result.Total
            };
            return Json(model);
        }

        [HttpPost]
        public ActionResult DetailList(long id, int page = 1, int rows = 20)
        {
            var result = this._bonusService.GetDetail(id, page, rows);

            var datas = result.Models.ToList().Select(p => new ShopBonusReceiveModel()
            {
                OpenId = p.OpenId,
                Price = (decimal)p.Price,
                ReceiveTime = p.ReceiveTime == null ? "" : ((DateTime)p.ReceiveTime).ToString("yyyy-MM-dd"),
                StateStr = p.State.ToDescription(),
                UsedTime = p.UsedTime == null ? "" : ((DateTime)p.UsedTime).ToString("yyyy-MM-dd"),
                UsedOrderId = p.UsedOrderId.ToString()
            }).ToList();
            DataGridModel<ShopBonusReceiveModel> model = new DataGridModel<ShopBonusReceiveModel>
            {
                rows = datas,
                total = result.Total
            };
            return Json(model);
        }

        [HttpPost]
        public ActionResult IsOverDate(string bend, string end)
        {
            bool result = this._bonusService.IsOverDate(DateTime.Parse(bend), DateTime.Parse(end), CurrentSellerManager.ShopId);
            return Json(result);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Add(ShopBonusModel model)
        {
            string image = model.ShareImg;
            string imageName = Guid.NewGuid().ToString() + ".png";
            string moveDir = "/Storage/Shop/Bonus/";
            if (image.Contains("/temp/"))
            {
                var source = image.Substring(image.LastIndexOf("/temp"));
                Core.HimallIO.CopyFile(source, moveDir + imageName,true);
                model.ShareImg = "/Storage/Shop/Bonus/" + imageName;
            }
            else
            {
                model.ShareImg = "";
            }
            var market = this._bonusService.GetShopBonusService(CurrentSellerManager.ShopId);
            var time = ((Himall.Model.ActiveMarketServiceInfo)market).MarketServiceRecordInfo.Max(a => a.EndTime);
            if (model.BonusDateEnd > time || model.DateEnd > time)
            {
                throw new HimallException("随机红包截止日期不允许超过您购买的服务到期时间。");
            }
            if(model.Count<1 || model.Count>1000)
            {
                throw new HimallException("红包个数请控制在1-1000个！");
            }
            this._bonusService.Add(model, CurrentSellerManager.ShopId);
            return RedirectToAction("Management");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditData(ShopBonusModel model)
        {
            var market = this._bonusService.GetShopBonusService(CurrentSellerManager.ShopId);
            var time = ((Himall.Model.ActiveMarketServiceInfo)market).MarketServiceRecordInfo.Max(a => a.EndTime);
            if (model.BonusDateEnd > time || model.DateEnd > time)
            {
                throw new HimallException("随机红包截止日期不允许超过您购买的服务到期时间。");
            }
            if (model.Count < 1 || model.Count > 1000)
            {
                throw new HimallException("红包个数请控制在1-1000个！");
            }
            string image = model.ShareImg;
            string imageName = Guid.NewGuid().ToString() + ".png";
            string moveDir = "/Storage/Shop/Bonus/";
            if (image.Contains("/temp/"))
            {
                var source = image.Substring(image.LastIndexOf("/temp"));
                Core.HimallIO.CopyFile(source, moveDir + imageName);
                model.ShareImg = "/Storage/Shop/Bonus/" + imageName;
            }
            else if (image.Contains("/Storage/"))
            {
                model.ShareImg = image.Substring(image.LastIndexOf("/Storage/"));
            }
            else
            {
                model.ShareImg = "";
            }
            this._bonusService.Update(model);
            return RedirectToAction("Management");
        }

        [HttpPost]
        public ActionResult IsAdd()
        {
            return Json(this._bonusService.IsAdd(CurrentSellerManager.ShopId));
        }

        [HttpPost]
        public JsonResult Invalid(long id)
        {
            this._bonusService.Invalid(id);
            return Json(true);
        }

        #region 服务费用设置
        public ActionResult BuyService()
        {
            Models.ShopBonusBuyServiceViewModel model = new ShopBonusBuyServiceViewModel();
            model.Market = this._bonusService.GetShopBonusService(CurrentSellerManager.ShopId);
            var active = _iMarketService.GetServiceSetting(MarketType.RandomlyBonus);
            model.IsNo = true;
            string endDate = null;
            var now = DateTime.Now.Date;
            if (model.Market != null && model.Market.MarketServiceRecordInfo.Max(item => item.EndTime) < now)
            {
                endDate = "您的随机红包服务已经过期，您可以续费。";
            }
            else if (model.Market != null && model.Market.MarketServiceRecordInfo.Max(item => item.EndTime) >= now)
            {
                var maxtime = model.Market.MarketServiceRecordInfo.Max(item => item.EndTime);
                endDate = string.Format("{0} 年 {1} 月 {2} 日", maxtime.Year, maxtime.Month, maxtime.Day);
            }
            else if (active == null)
            {
                model.IsNo = false;
                return View(model);
            }
            model.EndDate = endDate;
            model.Price = active.Price;
            return View(model);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult BuyService(int month)
        {
            Result result = new Result();
            var service = _iMarketService;
            service.OrderMarketService(month, CurrentSellerManager.ShopId, MarketType.RandomlyBonus);
            result.success = true;
            result.msg = "购买服务成功";
            return Json(result);
        }
        #endregion
    }
}