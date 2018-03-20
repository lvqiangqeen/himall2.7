using Himall.Core;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.Web;
using Himall.Web.Framework;
using System;
using System.Web.Mvc;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class LoginController : BaseMobileTemplatesController
    {
        IMemberService _iMemberService;
        IMessageService _iMessageService;
        IManagerService _iManagerService;

        public LoginController(IMemberService iMemberService, IMessageService iMessageService, IManagerService iManagerService)
        {

            _iMemberService = iMemberService;
            _iMessageService = iMessageService;
            _iManagerService = iManagerService;
        }
        // GET: Mobile/Login
        public ActionResult Entrance(string returnUrl, string openId, string serviceProvider, string nickName, string headimgurl, string realName, string unionid = null)
        {

            return View(CurrentSiteSetting);
        }
        public ActionResult ForgotPassword()
        {

            return View();
        }

        [HttpPost]
        public JsonResult BindUser(string username, string password, string headimgurl, string serviceProvider, string openId, Model.MemberOpenIdInfo.AppIdTypeEnum appidtype = MemberOpenIdInfo.AppIdTypeEnum.Normal, string unionid = null, string sex = null, string city = null, string province = null, string country = null, string nickName = null)
        {
            var service = _iMemberService;
            var member = service.Login(username, password);
            if (member == null)
                throw new Himall.Core.HimallException("用户名和密码不匹配");

            //Log.Debug("BindUser unionid=" + (string.IsNullOrWhiteSpace(unionid) ? "null" : unionid));
            headimgurl = System.Web.HttpUtility.UrlDecode(headimgurl);
            nickName = System.Web.HttpUtility.UrlDecode(nickName);
            city = System.Web.HttpUtility.UrlDecode(city);
            province = System.Web.HttpUtility.UrlDecode(province);
            OAuthUserModel model = new OAuthUserModel()
            {
                AppIdType = appidtype,
                UserId = member.Id,
                LoginProvider = serviceProvider,
                OpenId = openId,
                Headimgurl = headimgurl,
                UnionId = unionid,
                Sex = sex,
                NickName = nickName,
                City = city,
                Province = province
            };
            service.BindMember(model);

			base.SetUserLoginCookie(member.Id);
            WebHelper.SetCookie(CookieKeysCollection.HIMALL_ACTIVELOGOUT, "0", DateTime.MaxValue);
            SellerLoginIn(username, password);
            return Json(new { success = true });
        }



        [HttpPost]
        public JsonResult Index(string username, string password)
        {
            try
            {
                //检查输入合法性
                CheckInput(username, password);

                var member = _iMemberService.Login(username, password);
                if (member == null)
                {
                    throw new LoginException("用户名和密码不匹配", LoginException.ErrorTypes.PasswordError);
                }

				if (PlatformType == Core.PlatformType.WeiXin)
					base.SetUserLoginCookie(member.Id);
				else
					base.SetUserLoginCookie(member.Id, DateTime.MaxValue);

                WebHelper.SetCookie(CookieKeysCollection.HIMALL_ACTIVELOGOUT, "0", DateTime.MaxValue);
                SellerLoginIn(username, password);
                return Json(new { success = true, memberId = member.Id });
            }
            catch (LoginException ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
            catch (HimallException ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
            catch (Exception ex)
            {
                Core.Log.Error("用户" + username + "登录时发生异常", ex);
                return Json(new { success = false, msg = "未知错误" });
            }
        }

        void CheckInput(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new LoginException("请填写用户名", LoginException.ErrorTypes.UsernameError);

            if (string.IsNullOrWhiteSpace(password))
                throw new LoginException("请填写密码", LoginException.ErrorTypes.PasswordError);

        }
		[HttpGet]
		public JsonResult CheckLogin()
		{
			var userId = base.UserId;
			if (userId != 0)
			{
				//_iMemberService.DeleteMemberOpenId(userid, string.Empty);
				return Json(new { success = true }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = false }, JsonRequestBehavior.AllowGet);
		}

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="checkCode"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckUserName(string contact, string checkCode)
        {
            var service = _iMemberService;
            string systemCheckCode = Session["checkCode"] as string;
            if (systemCheckCode.ToLower() != checkCode.ToLower())
                throw new Core.HimallException("验证码错误");
            var userMenberInfo = service.GetMemberByContactInfo(contact);
            if (userMenberInfo == null)
            {
                throw new Core.HimallException("该手机号或邮箱未绑定账号");
            }

            #region 发送验证码
            var pluginId = "";
            if (Core.Helper.ValidateHelper.IsMobile(contact))
            {
                pluginId = "Himall.Plugin.Message.SMS";
            }
            if (!string.IsNullOrEmpty(contact) && Core.Helper.ValidateHelper.IsEmail(contact))
            {
                pluginId = "Himall.Plugin.Message.Email";
            }

            var timeout = CacheKeyCollection.MemberPluginCheckTime(contact, pluginId);
            if (Core.Cache.Get(timeout) != null)
            {
                return Json(new { success = false, msg = "60秒内只允许请求一次，请稍后重试!", url = "FillCode", contact = contact });
            }
            var Code = new Random().Next(10000, 99999);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            if (pluginId.ToLower().Contains("email"))
            {
                cacheTimeout = DateTime.Now.AddHours(24);
            }
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(contact, pluginId), Code, cacheTimeout);
            var user = new Himall.Core.Plugins.Message.MessageUserInfo() { UserName = contact, SiteName = CurrentSiteSetting.SiteName, CheckCode = Code.ToString() };
            _iMessageService.SendMessageCode(contact, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheckTime(contact, pluginId), "0", DateTime.Now.AddSeconds(55));

            #endregion

            return Json(new { success = true, contact = contact, url = "FillCode" });
        }


        //public ActionResult FillCode(string contact)
        //{
        //    ViewBag.Contact = contact;

        //    return View();
        //}

        /// <summary>
        /// 重新获取验证码
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public JsonResult SendCode(string contact)
        {
            var pluginId = "";
            if (Core.Helper.ValidateHelper.IsMobile(contact))
            {
                pluginId = "Himall.Plugin.Message.SMS";
            }
            if (!string.IsNullOrEmpty(contact) && Core.Helper.ValidateHelper.IsEmail(contact))
            {
                pluginId = "Himall.Plugin.Message.Email";
            }

            var timeout = CacheKeyCollection.MemberPluginCheckTime(contact, pluginId);
            if (Core.Cache.Get(timeout) != null)
            {
                return Json(new { success = false, msg = "60秒内只允许请求一次，请稍后重试!" });
            }
            var checkCode = new Random().Next(10000, 99999);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            if (pluginId.ToLower().Contains("email"))
            {
                cacheTimeout = DateTime.Now.AddHours(24);
            }
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(contact, pluginId), checkCode, cacheTimeout);
            var user = new Himall.Core.Plugins.Message.MessageUserInfo() { UserName = contact, SiteName = CurrentSiteSetting.SiteName, CheckCode = checkCode.ToString() };
            _iMessageService.SendMessageCode(contact, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheckTime(contact, pluginId), "0", DateTime.Now.AddSeconds(55));

            return Json(new { success = true });
        }

        /// <summary>
        /// 验证验证码
        /// </summary>
        /// <param name="code"></param>
        /// <param name="contact"></param>
        /// <returns></returns>
        public JsonResult CheckCode(string code, string contact)
        {
            var pluginId = "";
            if (Core.Helper.ValidateHelper.IsMobile(contact))
            {
                pluginId = "Himall.Plugin.Message.SMS";
            }
            if (!string.IsNullOrEmpty(contact) && Core.Helper.ValidateHelper.IsEmail(contact))
            {
                pluginId = "Himall.Plugin.Message.Email";
            }
            ViewBag.Contact = contact;
            var cache = CacheKeyCollection.MemberPluginCheck(contact, pluginId);
            var cacheCode = Core.Cache.Get(cache);
            if (cacheCode != null && cacheCode.ToString() == code)
            {
                var FdCache = CacheKeyCollection.MemberFindPwd(contact);
                Core.Cache.Insert(FdCache, contact, DateTime.Now.AddMinutes(10));

                return Json(new { success = true, msg = "验证正确", url = "ResetPwd" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "验证码输入错误或者已经超时" });
            }
        }

        /// <summary>
        /// 修改密码页
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public ActionResult ResetPwd(string contact)
        {
            //判断是否通过验证
            var FdCache = CacheKeyCollection.MemberFindPwd(contact);
            if (Core.Cache.Get(FdCache) == null)
            {
                Response.Redirect("ForgotPassword");
            }
            ViewBag.Contact = contact;
            return View();
        }
        public ActionResult GoResetResult()
        {
            return View();
        }
        public JsonResult ModPwd(string contact, string password, string repeatPassword)
        {
            var userMenberInfo = _iMemberService.GetMemberByContactInfo(contact);
            if (userMenberInfo == null)
            {
                throw new Core.HimallException("该手机号或邮箱未绑定账号");
            }
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(repeatPassword))
            {
                return Json(new Result() { success = false, msg = "密码不能为空！" });
            }
            if (!password.Trim().Equals(repeatPassword.Trim()))
            {
                return Json(new Result() { success = false, msg = "两次密码不一致！" });
            }
            _iMemberService.ChangePassword(userMenberInfo.Id, password);

            return Json(new { success = true, msg = "密码修改成功！", url = "GoResetResult" });
        }
        private ManagerInfo SellerLoginIn(string username, string password, bool keep = false)
        {
            var seller = _iManagerService.Login(username, password);
            if (seller == null)
            {
                return null;
            }

            if (keep)
            {
                base.SetSellerAdminLoginCookie(seller.Id, DateTime.Now.AddDays(7));
            }
            else
            {
                base.SetSellerAdminLoginCookie(seller.Id, DateTime.MaxValue);
            }
            return seller;
        }
    }
}