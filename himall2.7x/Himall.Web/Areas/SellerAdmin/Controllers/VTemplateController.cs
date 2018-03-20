using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.CommonModel;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class VTemplateController : BaseSellerController
    {
        private IVShopService _iVShopService;
        private ISlideAdsService _iSlideAdsService;
        private INavigationService _iNavigationService;
        private ICouponService _iCouponService;
        private ITemplateSettingsService _iTemplateSettingsService;

        public VTemplateController(IVShopService iVShopService,
            ISlideAdsService iSlideAdsService,
            INavigationService iNavigationService,
            ICouponService iCouponService,
            ITemplateSettingsService iTemplateSettingsService
            )
        {
            _iVShopService = iVShopService;
            _iSlideAdsService = iSlideAdsService;
            _iNavigationService = iNavigationService;
            _iCouponService = iCouponService;
            _iTemplateSettingsService = iTemplateSettingsService;
        }
        
        /// <summary>
        /// 模板编辑
        /// </summary>
        /// <param name="tName"></param>
        /// <returns></returns>
        public ActionResult EditTemplate(int client = 2, string tName = "t1")
        {
            VTemplateEditModel model = new VTemplateEditModel();
            model.Name = tName;
            model.ClientType = (VTemplateClientTypes)client;
            model.IsShowPrvPage = true;
            switch (model.ClientType)
            {
                case VTemplateClientTypes.SellerWapSpecial:
                    model.IsShowTitle = true;
                    model.IsShowTags = true;
                    model.IsShowPrvPage = false;
                    model.IsShowIcon = true;
                    break;
            }
            long shopid = CurrentSellerManager.ShopId;
            model.ShopId = shopid;
            var tmpobj = _iVShopService.GetVShopByShopId(shopid);
            if (tmpobj == null)
            {
                throw new Himall.Core.HimallException("未开通微店");
            }
            long vshopid = tmpobj.Id;
            model.VShopId = vshopid;
            return View(model);
        }        
        /// <summary>
        /// 微商城首页模板
        /// </summary>
        /// <returns></returns>
        public ActionResult VHomepage()
        {
            //Models.VshopHomeSiteViewModel model = new Models.VshopHomeSiteViewModel();
            //未开通微店就进不去首页设置 
            //VShopInfo vshop = 
            //model.VShop = vshop;
            //model.ShopId = CurrentSellerManager.ShopId;
            //model.SlideImage = _iSlideAdsService.GetSlidAds(CurrentSellerManager.ShopId, SlideAdInfo.SlideAdType.VShopHome).ToList();
            //model.Banner = _iNavigationService.GetSellerNavigations(CurrentSellerManager.ShopId, PlatformType.WeiXin).ToList();
            var vshop = _iVShopService.GetVShopByShopId(CurrentSellerManager.ShopId);
            if (vshop == null)
            {
                //throw new Himall.Core.HimallException("未开通微店");
            }
            ViewBag.IsOpenVShop = vshop != null;
            ViewBag.VShopId = vshop == null ? 0 : vshop.Id;
            ViewBag.ShopId = CurrentSellerManager.ShopId;
            string crrentTemplateName = "t1";
            var curr = _iTemplateSettingsService.GetCurrentTemplate(CurrentSellerManager.ShopId);
            if (null != curr)
            {
                crrentTemplateName = curr.CurrentTemplateName;
            }

            var helper = new GalleryHelper();
            var themes = helper.LoadThemes(CurrentSellerManager.ShopId);
            var CurTemplateObj = themes.FirstOrDefault(t => t.ThemeName.Equals(crrentTemplateName.ToLower()));
            if (CurTemplateObj == null)
            {
                CurTemplateObj = themes.FirstOrDefault(t => t.ThemeName.Equals("t1"));
            }
            if (CurTemplateObj == null)
            {
                throw new HimallException("错误的模板：" + crrentTemplateName);
            }
            ViewBag.CurrentTemplate = CurTemplateObj;
            ViewBag.CurUrl = Request.Url.Scheme + "://" + Request.Url.Authority;
            return View(themes.Where(t => t.ThemeName != crrentTemplateName.ToLower()).ToList());
        }
        /// <summary>
        /// 设置微商城首页模板
        /// </summary>
        /// <param name="tName"></param>
        /// <returns></returns>
        public JsonResult UpdateCurrentTemplate(string tName)
        {
            if (string.IsNullOrWhiteSpace(tName))
                return Json(new { success = false, msg = "模板名称不能为空" });
            _iTemplateSettingsService.SetCurrentTemplate(tName, CurrentSellerManager.ShopId);
            return Json(new { success = true, msg = "模板启用成功" });
        }

        public JsonResult UpdateTemplateName(string tName, string newName)
        {
            if (string.IsNullOrWhiteSpace(tName))
                return Json(new { success = false, msg = "模板名称不能为空" });
            new GalleryHelper().UpdateTemplateName(tName, newName, CurrentSellerManager.ShopId);
            return Json(new { success = true, msg = "模板名称修改成功" });
        }

    }
}