using Himall.IServices;
using Himall.Web.Areas.Admin.Helper;
using Himall.Web.Framework;
using System;
using System.Configuration;
using System.Web.Mvc;
using Himall.Core.Helper;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class LoginController : BaseController
    {

        private IManagerService _iManagerService;

        public LoginController(IManagerService iManagerService)
        {
            _iManagerService = iManagerService; // 第三方容器.构造<T>();
        }

        /// <summary>
        /// 同一用户名无需验证的的尝试登录次数
        /// </summary>
        const int TIMES_WITHOUT_CHECKCODE = 3;

        // GET: SellerAdmin/Login
        public ActionResult Index()
        {
            var t = ConfigurationManager.AppSettings["IsInstalled"];
            if (!(null == t || bool.Parse(t)))
            {
                return RedirectToAction("Agreement", "Installer", new { area = "Web" });
            }
            return View();
        }



        [HttpPost]
        public JsonResult Login(string username, string password, string checkCode)
        {
            string msg = "", host = System.Web.HttpContext.Current.Request.Url.Host;
            try
            {
                //检查输入合法性
                CheckInput(username, password);

                //检查验证码
                CheckCheckCode(username, checkCode);

                var manager = _iManagerService.Login(username, password, true);
                if (manager == null)
                {
                    throw new Himall.Core.HimallException("用户名和密码不匹配");
                }
                ClearErrorTimes(username);//清除输入错误记录次数

                //日龙修改
                //去除将过期店铺变为不可用店铺的功能
                //ServiceHelper.Create<IManagerService>().UpdateShopStatus();

				base.SetAdminLoginCookie(manager.Id);

				return Json(new { success = true });
            }
            catch (Himall.Core.HimallException ex)
            {
                int errorTimes = SetErrorTimes(username);
                return Json(new { success = false, msg = ex.Message, errorTimes = errorTimes, minTimesWithoutCheckCode = TIMES_WITHOUT_CHECKCODE });
            }
            catch (Exception ex)
            {
                int errorTimes = SetErrorTimes(username);
                Exception innerEx = GerInnerException(ex);
                string showerrmsg = "未知错误";
                if (innerEx is Himall.Core.HimallException)
                {
                    showerrmsg = innerEx.Message;
                }
                else
                {
                    Core.Log.Error("用户" + username + "登录时发生异常", ex);
                }
                return Json(new { success = false, msg = showerrmsg, errorTimes = errorTimes, minTimesWithoutCheckCode = TIMES_WITHOUT_CHECKCODE });
            }

        }

        [HttpPost]
        public JsonResult GetErrorLoginTimes(string username)
        {
            var errorTimes = GetErrorTimes(username);
            return Json(new { errorTimes = errorTimes });
        }


        [HttpPost]
        public JsonResult CheckCode(string checkCode)
        {
            try
            {
                string systemCheckCode = Session["checkCode"] as string;
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

        void CheckInput(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new Himall.Core.HimallException("请填写用户名");

            if (string.IsNullOrWhiteSpace(password))
                throw new Himall.Core.HimallException("请填写密码");

        }

        void CheckCheckCode(string username, string checkCode)
        {
            var errorTimes = GetErrorTimes(username);
            if (errorTimes >= TIMES_WITHOUT_CHECKCODE)
            {
                if (string.IsNullOrWhiteSpace(checkCode))
                    throw new Himall.Core.HimallException("30分钟内登录错误3次以上需要提供验证码");

                string systemCheckCode = Session["checkCode"] as string;
                if (systemCheckCode.ToLower() != checkCode.ToLower())
                    throw new Himall.Core.HimallException("验证码错误");

                //生成随机验证码，强制使验证码过期（一次提交必须更改验证码）
                Session["checkCode"] = Guid.NewGuid().ToString();
            }
        }


        public ActionResult GetCheckCode()
        {
            string code;
            var image = Core.Helper.ImageHelper.GenerateCheckCode(out code);
            Session["checkCode"] = code;
            return File(image.ToArray(), "image/png");
        }


        /// <summary>
        /// 获取指定用户名在30分钟内的错误登录次数
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        int GetErrorTimes(string username)
        {
            var timesObject = Core.Cache.Get(CacheKeyCollection.ManagerLoginError(username));
            var times = timesObject == null ? 0 : int.Parse(timesObject.ToString());
            return times;
        }

        void ClearErrorTimes(string username)
        {
            Core.Cache.Remove(CacheKeyCollection.ManagerLoginError(username));
        }

        /// <summary>
        /// 设置错误登录次数
        /// </summary>
        /// <param name="username"></param>
        /// <returns>返回最新的错误登录次数</returns>
        int SetErrorTimes(string username)
        {
            var times = GetErrorTimes(username) + 1;
            Core.Cache.Insert(CacheKeyCollection.ManagerLoginError(username), times, DateTime.Now.AddMinutes(30.0));//写入缓存
            return times;
        }

		public ActionResult Logout()
		{
			WebHelper.DeleteCookie(CookieKeysCollection.PLATFORM_MANAGER);

			return RedirectToAction("index");
		}
    }
}