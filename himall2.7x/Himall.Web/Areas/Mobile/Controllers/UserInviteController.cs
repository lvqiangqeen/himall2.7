using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Model;
using System.IO;
using Senparc.Weixin.MP.Helpers;
using Himall.Core;
using Senparc.Weixin.MP.CommonAPIs;
using Himall.Web.App_Code.Common;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class UserInviteController : BaseMobileMemberController
    {
        private SiteSettingsInfo _siteSetting = null;
        private ISiteSettingService _iSiteSettingService;
        private IMemberInviteService _iMemberInviteService;
        private IMemberIntegralService _iMemberIntegralService;
        public UserInviteController(ISiteSettingService iSiteSettingService,
            IMemberInviteService iMemberInviteService,
            IMemberIntegralService iMemberIntegralService
            )
        {
            _iMemberInviteService = iMemberInviteService;
            _iMemberIntegralService = iMemberIntegralService;
            _iSiteSettingService = iSiteSettingService;
            this._siteSetting = _iSiteSettingService.GetSiteSettings();
        }
        public ActionResult Index()
        {
            var userId = CurrentUser.Id;
            var model = _iMemberInviteService.GetMemberInviteInfo(userId);
            var rule = _iMemberInviteService.GetInviteRule();
            var Integral = _iMemberIntegralService.GetIntegralChangeRule() ;
            if (Integral != null && Integral.IntegralPerMoney > 0)
            {
                ViewBag.IntergralMoney = (rule.InviteIntegral.Value / Integral.IntegralPerMoney).ToString("f2");
            }
            string host = Request.Url.Host;
            host += Request.Url.Port != 80 ? ":" + Request.Url.Port.ToString() : "";
            model.InviteLink = String.Format("http://{0}/Register/index/{1}", host, userId);
            //rule.ShareIcon = string.Format("http://{0}{1}", host, rule.ShareIcon);
            rule.ShareIcon = !string.IsNullOrWhiteSpace(rule.ShareIcon) ? HimallIO.GetRomoteImagePath(rule.ShareIcon) : "";
            var map = Core.Helper.QRCodeHelper.Create(model.InviteLink);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            //  将图片内存流转成base64,图片以DataURI形式显示  
            string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
            ms.Dispose();
            model.QR = strUrl;
            ViewBag.WeiXin = PlatformType == PlatformType.WeiXin;
            var m = new Tuple<UserInviteModel, InviteRuleInfo, UserMemberInfo>(model, rule, CurrentUser);
            return View(m);
        }
    }
}