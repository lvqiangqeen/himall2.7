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
    public class RegisterController : BaseMobileTemplatesController
    {
        const string CHECK_CODE_KEY = "checkCode";

        private IMemberService _iMemberService;
        private ISiteSettingService _iSiteSettingService;
        private IMemberInviteService _iMemberInviteService;
        private IMemberIntegralService _iMemberIntegralService;
        private IBonusService _iBonusService;
        private IMessageService _iMessageService;
        private IMemberIntegralConversionFactoryService _iMemberIntegralConversionFactoryService;
        private SiteSettingsInfo _siteSetting = null;
        public RegisterController(
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
            _iMemberService = iMemberService;
            _iSiteSettingService = iSiteSettingService;
            _iMemberInviteService = iMemberInviteService;
            _iMemberIntegralService = iMemberIntegralService;
            _iMemberIntegralConversionFactoryService = iMemberIntegralConversionFactoryService;
            _iBonusService = iBonusService;
            this._siteSetting = _iSiteSettingService.GetSiteSettings();
        }
        // GET: Mobile/Register
        public ActionResult Index(long id = 0, string openid = "")
        {
            ViewBag.Introducer = id;
            if (id > 0)
            {
                if (string.IsNullOrWhiteSpace(openid))
                {
                    string webRoot = Request.Url.Scheme + "://" + HttpContext.Request.Url.Host + (HttpContext.Request.Url.Port == 80 ? "" : (":" + HttpContext.Request.Url.Port.ToString()));
                    string url = webRoot + "/m-" + PlatformType + "/Register/InviteRegist?id=" + id;
                    return Redirect("/m-" + PlatformType.ToString() + "/WXApi/WXAuthorize?returnUrl=" + url);
                    //return Redirect(url);
                }
            }
            var setting = _iSiteSettingService.GetSiteSettings();
            var type = setting.RegisterType;
            ViewBag.EmailVerifOpen = setting.EmailVerifOpen;
            if (type == (int)Himall.Model.SiteSettingsInfo.RegisterTypes.Mobile)
            {
                return View("MobileReg");
            }
            return View();
        }
		public ActionResult InviteRegist(long id = 0, string openId = "", string unionid="", string serviceProvider = "")
        {
            ViewBag.Introducer = id;
            var memberInfo = _iMemberService.GetMemberByUnionId(unionid);
            var settings = _iSiteSettingService.GetSiteSettings();
            var inviteRule = _iMemberInviteService.GetInviteRule();
            var model = _iMemberIntegralService.GetIntegralChangeRule();
            var perMoney = model == null ? 0 : model.IntegralPerMoney;
            ViewBag.WXLogo = settings.WXLogo;
            string money;
            if (inviteRule.InviteIntegral.HasValue && perMoney > 0)
            {
                money = (Convert.ToDouble(inviteRule.InviteIntegral.Value) / perMoney).ToString("f1");
            }
            else
            {
                money = "0.0";
            }


            int isRegist = 0;
            if (memberInfo != null)
            {
                isRegist = 1;
            }
            ViewBag.Money = money;
            ViewBag.IsRegist = isRegist;
            ViewBag.RegisterType = settings.RegisterType;
            return View(inviteRule);
        }
        [HttpPost]
        public JsonResult Index(string serviceProvider, string openId, string username, string password, string checkCode, string mobilecheckCode,
            string headimgurl, long introducer = 0, string unionid = null, string sex = null,
            string city = null, string province = null, string country = null, string nickName = null, string email = "", string emailcheckCode = "")
        {
            var mobilepluginId = "Himall.Plugin.Message.SMS";
            var emailpluginId = "Himall.Plugin.Message.Email";
            string systemCheckCode = Session[CHECK_CODE_KEY] as string;
            if (systemCheckCode.ToLower() != checkCode.ToLower())
                throw new Core.HimallException("验证码错误");

            if (Core.Helper.ValidateHelper.IsMobile(username))
            {
                var cache = CacheKeyCollection.MemberPluginCheck(username, mobilepluginId);
                var cacheCode = Core.Cache.Get(cache);

                if (string.IsNullOrEmpty(mobilecheckCode) || mobilecheckCode.ToLower() != cacheCode.ToString().ToLower())
                {
                    throw new Core.HimallException("手机验证码错误");
                }
            }

            if (!string.IsNullOrEmpty(email) && Core.Helper.ValidateHelper.IsMobile(email))
            {
                var cache = CacheKeyCollection.MemberPluginCheck(username, emailpluginId);
                var cacheCode = Core.Cache.Get(cache);

                if (string.IsNullOrEmpty(emailcheckCode) || emailcheckCode.ToLower() != cacheCode.ToString().ToLower())
                {
                    throw new Core.HimallException("手机验证码错误");
                }
            }

            headimgurl = System.Web.HttpUtility.UrlDecode(headimgurl);
            nickName = System.Web.HttpUtility.UrlDecode(nickName);
            province = System.Web.HttpUtility.UrlDecode(province);
            city = System.Web.HttpUtility.UrlDecode(city);
            UserMemberInfo member;
            var mobile = "";
            if (Core.Helper.ValidateHelper.IsMobile(username))
                mobile = username;
            if (!string.IsNullOrWhiteSpace(serviceProvider) && !string.IsNullOrWhiteSpace(openId))
            {
                OAuthUserModel userModel = new OAuthUserModel
                {
                    UserName = username,
                    Password = password,
                    LoginProvider = serviceProvider,
                    OpenId = openId,
                    Headimgurl = headimgurl,
                    Sex = sex,
                    NickName = nickName,
                    Email = email,
                    UnionId = unionid,
                    introducer = introducer,
                    Province = province,
                    City = city
                };
                member = _iMemberService.Register(userModel);
            }
            else
                member = _iMemberService.Register(username, password, mobile, email, introducer);
            if (member != null)
            {
                Session.Remove(CHECK_CODE_KEY);
                MessageHelper helper = new MessageHelper();
                helper.ClearErrorTimes(member.UserName);
                if (!string.IsNullOrEmpty(email))
                {
                    helper.ClearErrorTimes(member.Email);
                }
            }
            //TODO:ZJT  在用户注册的时候，检查此用户是否存在OpenId是否存在红包，存在则添加到用户预存款里
            _iBonusService.DepositToRegister(member.Id);
            //用户注册的时候，检查是否开启注册领取优惠券活动，存在自动添加到用户预存款里
            int num = CouponApplication.RegisterSendCoupon(member.Id, member.UserName);

			base.SetUserLoginCookie(member.Id);
			Application.MemberApplication.UpdateLastLoginDate(member.Id);
            return Json(new { success = true, memberId = member.Id, num = num });
        }

        [HttpPost]
        public JsonResult InviteRegist(string serviceProvider, string openId, string username, string password, string nickName, string headimgurl, long introducer, string sex, string city = null, string province = null, string unionid = null, string mobile = null)
        {

            headimgurl = System.Web.HttpUtility.UrlDecode(headimgurl);
            nickName = System.Web.HttpUtility.UrlDecode(nickName);
            username = System.Web.HttpUtility.UrlDecode(username);
            province = System.Web.HttpUtility.UrlDecode(province);
            city = System.Web.HttpUtility.UrlDecode(city);
            UserMemberInfo member;
            if (string.IsNullOrWhiteSpace(username))
                username = mobile;
            if (!string.IsNullOrWhiteSpace(serviceProvider) && !string.IsNullOrWhiteSpace(openId))
                member = _iMemberService.Register(username, password, serviceProvider, openId, sex, headimgurl, introducer, nickName, city, province, unionid);
            else
                member = _iMemberService.Register(username, password, mobile, "", introducer);

            //TODO:ZJT  在用户注册的时候，检查此用户是否存在OpenId是否存在红包，存在则添加到用户预存款里
            _iBonusService.DepositToRegister(member.Id);
            //用户注册的时候，检查是否开启注册领取优惠券活动，存在自动添加到用户预存款里
            int num = CouponApplication.RegisterSendCoupon(member.Id, member.UserName);

			base.SetUserLoginCookie(member.Id);
			Application.MemberApplication.UpdateLastLoginDate(member.Id);
            return Json(new { success = true, memberId = member.Id, num = num });
        }


        [HttpPost]
        public JsonResult Skip(string serviceProvider, string openId, string nickName, string realName, string headimgurl, MemberOpenIdInfo.AppIdTypeEnum appidtype = MemberOpenIdInfo.AppIdTypeEnum.Normal, string unionid = null, string sex = null, string city = null, string province = null)
        {
            int num = 0;
            string username = DateTime.Now.ToString("yyMMddHHmmssffffff");   //TODO:DZY[150916]未使用，在方法内会重新生成
            nickName = System.Web.HttpUtility.UrlDecode(nickName);
            realName = System.Web.HttpUtility.UrlDecode(realName);
            headimgurl = System.Web.HttpUtility.UrlDecode(headimgurl);
            province = System.Web.HttpUtility.UrlDecode(province);
            city = System.Web.HttpUtility.UrlDecode(city);
            UserMemberInfo memberInfo = _iMemberService.GetMemberByUnionId(openId);
            if (memberInfo == null)
            {
				memberInfo = _iMemberService.QuickRegister(username, realName, nickName, serviceProvider, openId, unionid, sex, headimgurl, appidtype, null, city, province);
                //TODO:ZJT  在用户注册的时候，检查此用户是否存在OpenId是否存在红包，存在则添加到用户预存款里
                _iBonusService.DepositToRegister(memberInfo.Id);
                //用户注册的时候，检查是否开启注册领取优惠券活动，存在自动添加到用户预存款里
                num = CouponApplication.RegisterSendCoupon(memberInfo.Id, memberInfo.UserName);
			}

			base.SetUserLoginCookie(memberInfo.Id);
			Application.MemberApplication.UpdateLastLoginDate(memberInfo.Id);
            WebHelper.SetCookie(CookieKeysCollection.HIMALL_ACTIVELOGOUT, "0", DateTime.MaxValue);
            return Json(new { success = true, num = num });
        }

        [HttpPost]
        public JsonResult CheckCode(string checkCode)
        {
            try
            {
                string systemCheckCode = Session[CHECK_CODE_KEY] as string;
                bool result = systemCheckCode.ToLower() == checkCode.ToLower();
                return Json(new { success = result });
            }
            catch (Himall.Core.HimallException ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
            catch (Exception ex)
            {
                Core.Log.Error("检验验证码时发生异常", ex);
                return Json(new { success = false, msg = "未知错误" });
            }
        }

        public ActionResult GetCheckCode()
        {
            string code;
            var image = Core.Helper.ImageHelper.GenerateCheckCode(out code);
            Session[CHECK_CODE_KEY] = code;
            return File(image.ToArray(), "image/png");
        }


        [HttpPost]
        public JsonResult SendMobileCode(string pluginId, string destination)
        {
            _iMemberService.CheckContactInfoHasBeenUsed(pluginId, destination);
            MessageHelper helper = new MessageHelper();
            var timeout = CacheKeyCollection.MemberPluginCheckTime(destination, pluginId);
            if (Core.Cache.Get(timeout) != null)
            {
                return Json(new Result() { success = false, msg = "120秒内只允许请求一次，请稍后重试!" });
            }
            var num = helper.GetErrorTimes(destination);
            if (num > 5)
            {
                return Json(new Result() { success = false, msg = "你发送的次数超过限制，请15分钟后再试！" });
            }
            var checkCode = new Random().Next(10000, 99999);
            Log.Info(destination + "：" + checkCode);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(destination, pluginId), checkCode, cacheTimeout);
            var user = new Himall.Core.Plugins.Message.MessageUserInfo() { UserName = destination, SiteName = CurrentSiteSetting.SiteName, CheckCode = checkCode.ToString() };
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheckTime(destination, pluginId), "0", DateTime.Now.AddSeconds(120));
            _iMessageService.SendMessageCode(destination, pluginId, user);
            helper.SetErrorTimes(destination);
            return Json(new Result() { success = true, msg = "发送成功" });
        }

        [HttpPost]
        public JsonResult SendEmailCode(string pluginId, string destination)
        {
            _iMemberService.CheckContactInfoHasBeenUsed(pluginId, destination);
            MessageHelper helper = new MessageHelper();
            var timeout = CacheKeyCollection.MemberPluginCheckTime(destination, pluginId);
            if (Core.Cache.Get(timeout) != null)
            {
                return Json(new Result() { success = false, msg = "120秒内只允许请求一次，请稍后重试!" });
            }
            var num = helper.GetErrorTimes(destination);
            if (num > 5)
            {
                return Json(new Result() { success = false, msg = "你发送的次数超过限制，请15分钟后再试！" });
            }
            var checkCode = new Random().Next(10000, 99999);
            Log.Info(destination + "：" + checkCode);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(destination, pluginId), checkCode, cacheTimeout);
            var user = new Himall.Core.Plugins.Message.MessageUserInfo() { UserName = destination, SiteName = CurrentSiteSetting.SiteName, CheckCode = checkCode.ToString() };
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheckTime(destination, pluginId), "0", DateTime.Now.AddSeconds(120));
            _iMessageService.SendMessageCode(destination, pluginId, user);
            helper.SetErrorTimes(destination);
            return Json(new Result() { success = true, msg = "发送成功" });
        }


        [HttpPost]
        public JsonResult CheckEmailCode(string pluginId, string code, string destination)
        {
            var cache = CacheKeyCollection.MemberPluginCheck(destination, pluginId);
            var cacheCode = Core.Cache.Get(cache);
            if (cacheCode != null && cacheCode.ToString() == code)
            {
                return Json(new Result() { success = true, msg = "验证正确" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "邮箱验证码不正确或者已经超时" });
            }
        }


        [HttpPost]
        public JsonResult CheckMobileCode(string pluginId, string code, string destination)
        {
            var cache = CacheKeyCollection.MemberPluginCheck(destination, pluginId);
            var cacheCode = Core.Cache.Get(cache);
            if (cacheCode != null && cacheCode.ToString() == code)
            {
                return Json(new Result() { success = true, msg = "验证正确" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "手机验证码不正确或者已经超时" });
            }
        }

    }
}