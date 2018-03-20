using Himall.Core;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Himall.Web;

using System.IO;
using Himall.Web.App_Code.Common;
using Himall.Application;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class RegisterActivityController : BaseMobileTemplatesController
    {
        const string CHECK_CODE_KEY = "checkCode";

        private ISiteSettingService _iSiteSettingService;
        private IMemberInviteService _iMemberInviteService;
        private IMemberIntegralService _iMemberIntegralService;
        private IMessageService _iMessageService;
        private SiteSettingsInfo _siteSetting = null;
        public RegisterActivityController(
            IMemberService iMemberService,
            ISiteSettingService iSiteSettingService,
            IMemberInviteService iMemberInviteService,
            IMemberIntegralService iMemberIntegralService,
            IMemberIntegralConversionFactoryService iMemberIntegralConversionFactoryService,
            IBonusService iBonusService,
            IMessageService iMessageService
            )
        {
            _iMessageService = iMessageService;
            _iSiteSettingService = iSiteSettingService;
            _iMemberInviteService = iMemberInviteService;
            _iMemberIntegralService = iMemberIntegralService;
            this._siteSetting = _iSiteSettingService.GetSiteSettings();
        }

        #region 注册有礼
        /// <summary>
        /// 注册有礼
        /// </summary>
        /// <returns></returns>
        public ActionResult Gift()
        {
            if (!IsMobileTerminal)
            {
                Response.Redirect("/RegisterActivity/Gift");
            }
            var model = CouponApplication.GetCouponSendByRegister();
            if (model != null && model.Status.Equals(Himall.CommonModel.CouponSendByRegisterStatus.Open) && model.total > 0) { }
            else
                Response.Redirect("end");
            return View(model);
        }

        /// <summary>
        /// 老用户
        /// </summary>
        /// <returns></returns>
        public ActionResult Share()
		{
			var userId = base.UserId;
            if (userId != 0)
            {
                var model = _iMemberInviteService.GetMemberInviteInfo(userId);
                var rule = _iMemberInviteService.GetInviteRule();
                var Integral = _iMemberIntegralService.GetIntegralChangeRule();
                if (Integral != null && Integral.IntegralPerMoney > 0)
                {
                    ViewBag.IntergralMoney = (rule.InviteIntegral.Value / Integral.IntegralPerMoney).ToString("f2");
                }
                string host = Request.Url.Host;
                host += Request.Url.Port != 80 ? ":" + Request.Url.Port.ToString() : "";
                model.InviteLink = String.Format("http://{0}/Register/index/{1}", host, userId);
                //rule.ShareIcon = string.Format("http://{0}{1}", host, rule.ShareIcon);
                //var map = Core.Helper.QRCodeHelper.Create(model.InviteLink);
                //MemoryStream ms = new MemoryStream();
                //map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                ////  将图片内存流转成base64,图片以DataURI形式显示  
                //string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
                //ms.Dispose();
                //model.QR = strUrl;
                var m = new Tuple<UserInviteModel, InviteRuleInfo, UserMemberInfo>(model, rule, CurrentUser);

                return View(m);
            }
            else
            {
                Response.Redirect("/m-Wap/Login/Entrance?returnUrl=" + HttpUtility.UrlEncode(Request.Url.ToString()));
                return View();
            }
        }

        /// <summary>
        /// 活动结束
        /// </summary>
        /// <returns></returns>
        public ActionResult End()
        {
            return View();
        }
        #endregion
    }
}