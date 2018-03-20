using System.Web.Mvc;
using Himall.Web.Framework;
using Himall.IServices;
using System.Linq;
using Himall.Web.Areas.Mobile.Models;
using System.IO;
using System.Text;
using Himall.Application;
using Himall.CommonModel;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class HomeController : BaseMobileTemplatesController
    {
        IDistributionService _iDistributionService;
        private ITemplateSettingsService _iTemplateSettingsService;
        private ISiteSettingService _iSiteSettingService;
        private ICustomerService _iCustomerService;

        public HomeController(IDistributionService iDistributionService, ITemplateSettingsService iTemplateSettingsService, ISiteSettingService iSiteSettingService, ICustomerService iCustomerService)
        {
            _iDistributionService = iDistributionService;
            _iTemplateSettingsService = iTemplateSettingsService;
            _iSiteSettingService = iSiteSettingService;
            _iCustomerService = iCustomerService;
        }

        [OutputCache(Duration = ConstValues.PAGE_CACHE_DURATION)]
        // GET: Mobile/Home
        public ActionResult Index(int ispv = 0, string tn = "")
        {
            //Model.SlideAdInfo.SlideAdType slideAdType ;
            //switch(PlatformType)
            //{
            //    case Core.PlatformType.WeiXin:
            //        slideAdType = Model.SlideAdInfo.SlideAdType.WeixinHome;
            //        break;
            //    default :
            //        slideAdType = Model.SlideAdInfo.SlideAdType.WeixinHome;//暂时都调用微信端数据
            //        break;
            //}
            string crrentTemplateName = "t1";
            var curr = _iTemplateSettingsService.GetCurrentTemplate(0);
            if (null != curr)
            {
                crrentTemplateName = curr.CurrentTemplateName;
            }
            if (ispv == 1)
            {
                if (!string.IsNullOrWhiteSpace(tn))
                {
                    crrentTemplateName = tn;
                }
            }
            Core.Log.Debug("crrentTemplateName=" + crrentTemplateName);
            ViewBag.Title = CurrentSiteSetting.SiteName + "首页";
            ViewBag.FootIndex = 0;

            var services = CustomerServiceApplication.GetPlatformCustomerService(true, true);
			var meiqia=CustomerServiceApplication.GetPlatformCustomerService(true, false).FirstOrDefault(p => p.Tool == Model.CustomerServiceInfo.ServiceTool.MeiQia);
			if (meiqia != null)
				services.Insert(0, meiqia);
            ViewBag.CustomerServices = services;

            return View(string.Format("~/Areas/Admin/Templates/vshop/{0}/Skin-HomePage.cshtml", crrentTemplateName));
        }

        public JsonResult LoadProducts(int page, int pageSize)
        {
            var homeProducts = ServiceHelper.Create<IMobileHomeProductsService>().GetMobileHomePageProducts(0, Core.PlatformType.WeiXin).OrderBy(item => item.Sequence).ThenByDescending(o => o.Id).Skip((page - 1) * pageSize).Take(pageSize);
            var model = homeProducts.ToArray().Select(item => new
            {
                name = item.Himall_Products.ProductName,
                id = item.ProductId,
                image = item.Himall_Products.GetImage(ImageSize.Size_350),
                price = item.Himall_Products.MinSalePrice,
                marketPrice = item.Himall_Products.MarketPrice
            });
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadProductsFromCache(int page)
        {
            var html =TemplateSettingsApplication.GetGoodTagFromCache(page);
            return Json(new { htmlTag = html }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult About()
        {
            return View();
        }

        /// <summary>
        /// 招募计划
        /// </summary>
        /// <returns></returns>
        public ActionResult RecruitPlan()
        {
            var model = _iDistributionService.GetRecruitmentPlan();
            return View(model);
        }

        public ActionResult DownLoadApp()
        {
            if (PlatformType == Core.PlatformType.WeiXin)
                return RedirectToAction("WeiXinDownLoad");
            if (visitorTerminalInfo.OperaSystem == EnumVisitorOperaSystem.Android)
                return RedirectToAction("AndriodDownLoad");
            if (visitorTerminalInfo.OperaSystem == EnumVisitorOperaSystem.IOS)
                return RedirectToAction("IOSDownLoad");
            return View();
        }


       public ActionResult WeiXinDownLoad()
        {
            if (PlatformType == Core.PlatformType.WeiXin)
                return View();
            if (visitorTerminalInfo.OperaSystem == EnumVisitorOperaSystem.Android)
                return RedirectToAction("AndriodDownLoad");
            if (visitorTerminalInfo.OperaSystem == EnumVisitorOperaSystem.IOS)
                return RedirectToAction("IOSDownLoad");
            return View();
        }


        public ActionResult AndriodDownLoad()
        {
            var DownLoadApk = _iSiteSettingService.GetSiteSettings().AndriodDownLoad;
            if (!string.IsNullOrEmpty(DownLoadApk))
            {
                ViewBag.DownLoadApk = DownLoadApk;
                return View();
            }
            return RedirectToAction("DownLoadError");
        }

        public ActionResult IOSDownLoad()
        {
            var DownLoadApk = _iSiteSettingService.GetSiteSettings().IOSDownLoad;
            if (!string.IsNullOrEmpty(DownLoadApk))
            {
                return Redirect(DownLoadApk);
            }
            return RedirectToAction("DownLoadError");
        }

        public ActionResult DownLoadError()
        {
            return View();
        }

        /// <summary>
        /// 获取分享内容
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetShare(string url)
        {
            var shareArgs = Himall.Application.WXApiApplication.GetWeiXinShareArgs(url);
            var siteSetting = SiteSettingApplication.GetSiteSettings();
            var shareTitle = string.Empty;
            if (siteSetting != null && !string.IsNullOrEmpty(siteSetting.SiteName))
            {
                shareTitle = siteSetting.SiteName;
            }
            var result = new
            {
                AppId = shareArgs.AppId,
                Timestamp = shareArgs.Timestamp,
                NonceStr = shareArgs.NonceStr,
                Signature = shareArgs.Signature,
                ShareIcon = Himall.Core.HimallIO.GetRomoteImagePath(SiteSettingApplication.GetSiteSettings().WXLogo),
                ShareTitle = shareTitle
            };

            return Json(result, true);
        }
        /// <summary>
        /// 获取模板节点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetTemplateItem(string id, string tn = "")
        {
            string result = "";
            if (string.IsNullOrWhiteSpace(tn))
            {
                tn = "t1";
                var curr = _iTemplateSettingsService.GetCurrentTemplate(0);
                if (null != curr)
                {
                    tn = curr.CurrentTemplateName;
                }
            }
            result = VTemplateHelper.GetTemplateItemById(id, tn, VTemplateClientTypes.WapIndex);
            return result;
        }
    }
}
