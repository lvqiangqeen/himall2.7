using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System.Web.Mvc;
using System;
using Himall.Core;
using Himall.Core.Plugins.Message;
using System.Threading.Tasks;
using Himall.Web.Areas.Web.Models;
using Himall.Application;
using Himall.Core.Helper;

namespace Himall.Web.Areas.Web.Controllers
{
    public class RegisterController : BaseController
    {
        private IMemberService _iMemberService;
        private IBonusService _iBonusService;
        private IMessageService _iMessageService;
        private IManagerService _iManagerService;
        private ISystemAgreementService _iSystemAgreementService;
        public RegisterController(
            IMemberService iMemberService,
            IBonusService iBonusService,
            IMessageService iMessageService,
            ISystemAgreementService iSystemAgreementService,
            IManagerService iManagerService
            )
        {
            _iMemberService = iMemberService;
            _iBonusService = iBonusService;
            _iMessageService = iMessageService;
            _iSystemAgreementService = iSystemAgreementService;
            _iManagerService = iManagerService;
        }

        // GET: Web/Register
        public ActionResult Index(long id = 0)
        {
            ViewBag.SiteName = CurrentSiteSetting.SiteName;
            ViewBag.Logo = CurrentSiteSetting.Logo;
            RegisterIndexPageModel model = new RegisterIndexPageModel();
            model.MobileVerifOpen = CurrentSiteSetting.MobileVerifOpen;
            model.EmailVerifOpen = CurrentSiteSetting.EmailVerifOpen;
            model.RegisterEmailRequired = CurrentSiteSetting.RegisterEmailRequired;
            model.RegisterType = (Himall.Model.SiteSettingsInfo.RegisterTypes)CurrentSiteSetting.RegisterType;
            model.Introducer = id;
            return View(model);
        }

        const string CHECK_CODE_KEY = "regist_CheckCode";

        [HttpPost]
        public JsonResult RegisterUser(string username, string password, string mobile, string email,string checkCode, long introducer = 0)
		{
			var siteset = CurrentSiteSetting;
			if (siteset.RegisterEmailRequired)
			{
				if (string.IsNullOrWhiteSpace(email))
				{
					return Json(new { success = false, msg = "错误的电子邮箱地址" });
				}
			}
			if (siteset.MobileVerifOpen)
			{
				if (string.IsNullOrWhiteSpace(mobile))
				{
					return Json(new { success = false, msg = "错误的手机号码" });
				}
			}

            if (StringHelper.GetStringLength(username)> CommonModel.CommonConst.MEMBERNAME_LENGTH)
            {
                var unicodeChar = CommonModel.CommonConst.MEMBERNAME_LENGTH / 2;

                return Json(new { success = false, msg = "用户名最大长度为" + CommonModel.CommonConst.MEMBERNAME_LENGTH + "位," + unicodeChar + "个中文字符" });
            }

			var cacheCheckCode = Session[CHECK_CODE_KEY] as string;
			if (cacheCheckCode==null||string.IsNullOrEmpty(checkCode)||checkCode.ToLower()!=cacheCheckCode.ToLower())
			{
				return Json(new { success = false, msg = "验证码错误" });
			}

			var member = _iMemberService.Register(username, password, mobile, email, introducer);
			if (member != null)
			{
				//自动登录
				_iMemberService.Login(username, password);

				base.SetUserLoginCookie(member.Id);

				Session.Remove(CHECK_CODE_KEY);
				if (!string.IsNullOrEmpty(mobile))
					Core.Cache.Remove(CacheKeyCollection.MemberPluginCheck(mobile, "Himall.Plugin.Message.SMS"));
			}
			//TODO:ZJT  在用户注册的时候，检查此用户是否存在OpenId是否存在红包，存在则添加到用户预存款里
			_iBonusService.DepositToRegister(member.Id);
			//用户注册的时候，检查是否开启注册领取优惠券活动，存在自动添加到用户预存款里
			int num = CouponApplication.RegisterSendCoupon(member.Id, member.UserName);

			return Json(new { success = true, memberId = member.Id, num = num });
		}

        [ValidateInput(false)]
        public ActionResult GetCheckCode()
        {
            string code;
            var image = Core.Helper.ImageHelper.GenerateCheckCode(out code);
            Session[CHECK_CODE_KEY] = code;
            return File(image.ToArray(), "image/png");
        }

        [HttpPost]
        public JsonResult CheckCheckCode(string checkCode)
        {
			var cache = Session[CHECK_CODE_KEY] as string;
			bool result = cache != null && checkCode.ToLower() == cache.ToLower();
            return Json(new { success = true, result = result });
        }

        [HttpPost]
        public JsonResult CheckUserName(string username)
        {
            bool result = _iMemberService.CheckMemberExist(username);
            return Json(new { success = true, result = result });
        }

        [HttpPost]
        public JsonResult CheckMobile(string mobile)
        {
            bool result = _iMemberService.CheckMobileExist(mobile);
            return Json(new { success = true, result = result });
        }

        [HttpPost]
        public JsonResult CheckEmail(string email)
        {
            bool result = _iMemberService.CheckEmailExist(email);
            return Json(new { success = true, result = result });
        }

        [HttpPost]
        public JsonResult SendCode(string pluginId, string destination)
        {
            _iMemberService.CheckContactInfoHasBeenUsed(pluginId, destination);
            var timeout = CacheKeyCollection.MemberPluginCheckTime(destination, pluginId);
            if (Core.Cache.Get(timeout) != null)
            {
                return Json(new Result() { success = false, msg = "120秒内只允许请求一次，请稍后重试!" });
            }
            var checkCode = new Random().Next(10000, 99999);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            if (pluginId.ToLower().Contains("email"))
            {
                cacheTimeout = DateTime.Now.AddHours(24);
            }
            Log.Debug(destination + "：" + checkCode);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(destination, pluginId), checkCode, cacheTimeout);
            var user = new MessageUserInfo() { UserName = destination, SiteName = CurrentSiteSetting.SiteName, CheckCode = checkCode.ToString() };
            _iMessageService.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheckTime(destination, pluginId), "0", DateTime.Now.AddSeconds(120));
            return Json(new Result() { success = true, msg = "发送成功" });
        }

        [HttpPost]
        public JsonResult CheckCode(string pluginId, string code, string destination)
        {
            var cache = CacheKeyCollection.MemberPluginCheck(destination, pluginId);
            var cacheCode = Core.Cache.Get(cache);
            if (cacheCode != null && cacheCode.ToString() == code)
            {
                return Json(new Result() { success = true, msg = "验证正确" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "验证码不正确或者已经超时" });
            }
        }

        [HttpPost]
        public JsonResult CheckManagerUser(string username)
        {
            bool result = _iManagerService.CheckUserNameExist(username);
            return Json(new { success = true, result });
        }

        [HttpGet]
        public ActionResult RegBusiness()
        {
            ViewBag.Logo = CurrentSiteSetting.Logo;
            return View();
        }
        public ActionResult RegisterAgreement()
        {
            ViewBag.Logo = CurrentSiteSetting.Logo;
            AgreementInfo model = _iSystemAgreementService.GetAgreement(0);

            return View(model);
        }
    }
}