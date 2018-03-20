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
    public class LimitTimeBuyController : BaseAdminController
    {
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private IMarketService _iMarketService;
        private ISlideAdsService _iSlideAdsService;

        public LimitTimeBuyController(ILimitTimeBuyService iLimitTimeBuyService, IMarketService iMarketService, ISlideAdsService iSlideAdsService)
        {
            _iLimitTimeBuyService = iLimitTimeBuyService;
            _iMarketService = iMarketService;
            _iSlideAdsService = iSlideAdsService;
        }

        #region 活动列表

        // GET: Admin/LimitTimeBuy
        public ActionResult Management()
        {
            return View();
        }

        public ActionResult Audit(long id)
        {
            var result = _iLimitTimeBuyService.Get(id);
            ViewBag.IsAudit = true;
            return View(result);
        }

        public ActionResult Detail(long id)
        {
            var result = _iLimitTimeBuyService.Get(id);
            ViewBag.IsAudit = false;
            return View(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult List(int? status, int page, int rows, string shopName, string title)
        {
            if (status == null)
            {
                status = 0;
            }

            ObsoletePageModel<FlashSaleInfo> result = _iLimitTimeBuyService.GetAll((int)status, shopName, title, page, rows);
            IEnumerable<FlashSaleModel> market = result.Models.ToArray().Select(item =>

           {
               var m = new FlashSaleModel();
               m.Id = item.Id;
               m.Title = item.Title;
               m.BeginDate = item.BeginDate.ToString("yyyy-MM-dd");
               m.EndDate = item.EndDate.ToString("yyyy-MM-dd");
               m.ShopName = item.Himall_Shops.ShopName;
               m.ProductName = item.Himall_Products.ProductName;
               m.ProductId = item.ProductId;
               //StatusStr = item.EndDate < DateTime.Now ?FlashSaleInfo.FlashSaleStatus.Ended.ToDescription() : item.Status.ToDescription() ,
               m.StatusStr = item.Status.ToDescription();
               if (item.Status != FlashSaleInfo.FlashSaleStatus.WaitForAuditing && item.Status != FlashSaleInfo.FlashSaleStatus.AuditFailed && item.BeginDate > DateTime.Now && item.EndDate < DateTime.Now)
               {
                   m.StatusStr = "进行中";
               }
               else if (item.Status != FlashSaleInfo.FlashSaleStatus.WaitForAuditing && item.Status != FlashSaleInfo.FlashSaleStatus.AuditFailed && item.BeginDate > DateTime.Now)
               {
                   m.StatusStr = "未开始";
               }
               m.SaleCount = item.SaleCount;
               return m;
           });




            DataGridModel<FlashSaleModel> dataGrid = new DataGridModel<FlashSaleModel>() { rows = market, total = result.Total };
            return Json(dataGrid);
        }


        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="auditState">审核状态</param>
        /// <param name="message">理由</param>
        /// <returns></returns>
        [UnAuthorize]
        [OperationLog(Message = "审核商品状态")]
        [HttpPost]
        public JsonResult AuditItem(long id)
        {

            Result result = new Result();
            try
            {
                _iLimitTimeBuyService.Pass(id);
                Cache.Remove(CacheKeyCollection.CACHE_LIMITPRODUCTS);
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
        /// 拒绝
        /// </summary>
        [UnAuthorize]
        [OperationLog(Message = "拒绝商品状态")]
        [HttpPost]
        public JsonResult RefuseItem(long id)
        {

            Result result = new Result();
            try
            {
                _iLimitTimeBuyService.Refuse(id);
                result.success = true;
                result.msg = "成功拒绝！";
            }
            catch (HimallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("拒绝出错", ex);
                result.msg = "拒绝出错！";
            }
            return Json(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult CancelItem(long id)
        {
            Result result = new Result();
            try
            {
                _iLimitTimeBuyService.Cancel(id);
                result.success = true;
                result.msg = "取消成功！";
            }
            catch (HimallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("取消出错", ex);
                result.msg = "取消出错！";
            }
            return Json(result);
        }



        public ActionResult SetSlide()
        {
            return View();
        }

        #endregion

        #region 轮播图

        public JsonResult GetSlideJson()
        {
            var data = _iSlideAdsService.GetSlidAds(0, SlideAdInfo.SlideAdType.PlatformLimitTime);
            IEnumerable<HandSlideModel> slide = data.ToArray().Select(item => new HandSlideModel()
            {
                Id = item.Id,
                Pic = Core.HimallIO.GetImagePath(item.ImageUrl),
                URL = item.Url,
                Index = item.DisplaySequence
            });

            DataGridModel<HandSlideModel> dataGrid = new DataGridModel<HandSlideModel>() { rows = slide, total = slide.Count() };
            return Json(dataGrid);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult AddSlideAd(string pic, string url)
        {
            var slide = new SlideAdInfo()
            {
                ImageUrl = pic,
                Url = url,
                ShopId = 0,
                DisplaySequence = 0,
                TypeId = SlideAdInfo.SlideAdType.PlatformLimitTime
            };
            if (!string.IsNullOrWhiteSpace(pic))
            {
                if (pic.Contains("/temp/"))
                {
                    string source = pic.Substring(pic.LastIndexOf("/temp/"));
                    string dest = @"/Storage/Plat/ImageAd/";
                    pic = dest + Path.GetFileName(pic);
                    Core.HimallIO.CopyFile(source, pic, true);
                }
                else if (pic.Contains("/Storage/"))
                {
                    pic = pic.Substring(pic.LastIndexOf("/Storage/"));
                }

                slide.ImageUrl = pic;
            }
            _iSlideAdsService.AddSlidAd(slide);
            return Json(new { successful = true }, JsonRequestBehavior.AllowGet);
        }

        [UnAuthorize]
        public JsonResult DeleteSlide(long Id)
        {
            _iSlideAdsService.DeleteSlidAd(0, Id);
            return Json(new { successful = true }, JsonRequestBehavior.AllowGet);
        }

        [UnAuthorize]
        public JsonResult EditSlideAd(long id, string pic, string url)
        {
            var slide = _iSlideAdsService.GetSlidAd(0, id);

            if (!string.IsNullOrWhiteSpace(pic) && (!slide.ImageUrl.Equals(pic)))
            {
                if (pic.Contains("/temp/"))
                {
                    string source = pic.Substring(pic.LastIndexOf("/temp/"));
                    string dest = @"/Storage/Plat/ImageAd/";
                    pic = dest + Path.GetFileName(pic);
                    Core.HimallIO.CopyFile(source, pic, true);
                }
                else if (pic.Contains("/Storage/"))
                {
                    pic = pic.Substring(pic.LastIndexOf("/Storage/"));
                }
            }

            _iSlideAdsService.UpdateSlidAd(new SlideAdInfo
            {
                Id = id,
                ImageUrl = pic,
                Url = url
            });
            return Json(new { successful = true }, JsonRequestBehavior.AllowGet);
        }

        [UnAuthorize]
        [HttpPost]
        public ActionResult AdjustSlideIndex(long id, int direction)
        {
            _iSlideAdsService.AdjustSlidAdIndex(0, id, direction == 1, SlideAdInfo.SlideAdType.PlatformLimitTime);
            return Json(new { successful = true }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 服务购买列表

        public ActionResult BoughtList()
        {
            return View();
        }

        [UnAuthorize]
        public JsonResult GetBoughtJson(string shopName, int page, int rows)
        {
            var queryModel = new MarketBoughtQuery()
            {
                PageSize = rows,
                PageNo = page,
                ShopName = shopName,
                MarketType = MarketType.LimitTimeBuy
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

        #region 活动商品分类

        public ActionResult MarketCategory()
        {
            return View();
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult GetMarketCategoryJson()
        {
            var service = _iLimitTimeBuyService;
            var cate = service.GetServiceCategories();
            var list = from i in cate
                       select new { Name = i, Id = 0 };
            return Json(new { rows = list, total = list.Count() });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult AddMarketCategory(string name)
        {
            Result result = new Result();
            try
            {
                var service = _iLimitTimeBuyService;
                service.AddServiceCategory(name.Replace(",", "").Replace("，", ""));
                result.success = true;
                result.msg = "添加分类成功！";
            }
            catch (HimallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("添加分类出错", ex);
                result.msg = "添加分类出错！";
            }
            return Json(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult DeleteMarketCategory(string name)
        {
            Result result = new Result();
            try
            {
                var service = _iLimitTimeBuyService;
                service.DeleteServiceCategory(name);
                result.success = true;
                result.msg = "删除分类成功！";
            }
            catch (HimallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("删除分类出错", ex);
                result.msg = "删除分类出错！";
            }
            return Json(result);
        }
        #endregion

        #region 服务费用设置

        public ActionResult ServiceSetting()
        {
            LimitTimeBuySettingModel model = _iLimitTimeBuyService.GetServiceSetting();
            return View(model);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SaveServiceSetting(decimal Price, int ReviceDays = 0)
        {
            Result result = new Result();
            try
            {
                var model = new LimitTimeBuySettingModel { Price = Price, ReviceDays = ReviceDays };
                _iLimitTimeBuyService.UpdateServiceSetting(model);
                result.success = true;
                result.msg = "保存成功！";
            }
            catch (HimallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("保存出错", ex);
                result.msg = "保存出错！";
            }
            return Json(result);
        }
        #endregion

        #region  活动参数

        public ActionResult ConfigSetting()
        {
            return View(_iLimitTimeBuyService.GetConfig());
        }

        public ActionResult SetConfig(FlashSaleConfigModel data)
        {
            _iLimitTimeBuyService.UpdateConfig(data);
            Result result = new Result { success = true };
            return Json(result);
        }

        #endregion
    }
}