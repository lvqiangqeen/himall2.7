using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Core;
using Himall.Core.Helper;
using Himall.Web.Areas.Web.Models;
using Himall.Core.Plugins.Message;
using Himall.Application;
using Himall.DTO;

namespace Himall.Web.Areas.Web.Controllers
{
    public class UserInfoController : BaseMemberController
    {
        private IMessageService _iMessageService;
        private IMemberService _iMemberService;

        public UserInfoController(IMessageService iMessageService, IMemberService iMemberService)
        {
            _iMemberService = iMemberService;
            _iMessageService = iMessageService;
        }
        // GET: Web/UserInfo
        public ActionResult Index()
        {
            var model = MemberApplication.GetMembers(CurrentUser.Id);
            var messagePlugins = PluginsManagement.GetPlugins<IMessagePlugin>();
            var sms = PluginsManagement.GetPlugins<ISMSPlugin>();
            var smsInfo = sms.Select(item => new PluginsInfo
            {
                ShortName = item.Biz.ShortName,
                PluginId = item.PluginInfo.PluginId,
                Enable = item.PluginInfo.Enable,
                IsSettingsValid = item.Biz.IsSettingsValid,
                IsBind = !string.IsNullOrEmpty(_iMessageService.GetDestination(CurrentUser.Id, item.PluginInfo.PluginId, Himall.Model.MemberContactsInfo.UserTypes.General))
            }).FirstOrDefault();
            var email = PluginsManagement.GetPlugins<IEmailPlugin>();
            var emailInfo = email.Select(item => new PluginsInfo
            {
                ShortName = item.Biz.ShortName,
                PluginId = item.PluginInfo.PluginId,
                Enable = item.PluginInfo.Enable,
                IsSettingsValid = item.Biz.IsSettingsValid,
                IsBind = !string.IsNullOrEmpty(_iMessageService.GetDestination(CurrentUser.Id, item.PluginInfo.PluginId, Himall.Model.MemberContactsInfo.UserTypes.General))
            }).FirstOrDefault();


            ViewBag.BindSMS = smsInfo;
            ViewBag.BindEmail = emailInfo;
            return View(model);
        }

        [HttpPost]
        public JsonResult GetCurrentUserInfo()
        {
            var memberInfo = CurrentUser;
            string name = string.IsNullOrWhiteSpace(memberInfo.Nick) ? memberInfo.UserName : memberInfo.Nick;
            return Json(new { success = true, name = name });
        }

        public JsonResult UpdateUserInfo(MemberUpdate model)
        {
            if (!model.BirthDay.HasValue && !CurrentUser.BirthDay.HasValue)
            {
                return Json(new Result() { success = false, msg = "生日必须填写" });
            }
            if(string.IsNullOrWhiteSpace(model.CellPhone)||string.IsNullOrWhiteSpace(CurrentUser.CellPhone))
            {
                return Json(new Result() { success = false, msg = "请先绑定手机号码" });
            }
            if (string.IsNullOrWhiteSpace(model.RealName))
            {
                return Json(new Result() { success = false, msg = "用户姓名必须填写" });
            }
            model.Id = CurrentUser.Id;
            MemberApplication.UpdateMemberInfo(model);
            return Json(new Result() { success = true, msg = "修改成功" });
        }

        public ActionResult ReBind(string pluginId)
        {
            var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>(pluginId);
            ViewBag.ShortName = messagePlugin.Biz.ShortName;
            ViewBag.id = pluginId;
            ViewBag.ContactInfo = _iMessageService.GetDestination(CurrentUser.Id, pluginId, MemberContactsInfo.UserTypes.General);
            return View();
        }

        [HttpPost]
        public ActionResult CheckCode(string pluginId, string code, string destination)
        {
            var cache = CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId+destination); //带上发送目标防止修改
            var cacheCode = Core.Cache.Get(cache);
			if (cacheCode == null)
				return Json(new Result { success=false,msg="验证码已经超时" });

            var member = CurrentUser;
            if (cacheCode.ToString() == code)
            {
                Core.Cache.Remove(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId+destination));
                Core.Cache.Insert("Rebind" + member.Id, "step2", DateTime.Now.AddMinutes(30));
                return Json(new { success = true, msg = "验证正确", key = member.Id });
            }
            else
            {
				return Json(new Result() { success = false, msg = "验证码不正确" });
            }
        }


        [HttpPost]  //验证第二步需要修改信息了
        public ActionResult CheckCodeStep2(string pluginId, string code, string destination)
        {
            var cache = CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId + destination);
            var cacheCode = Core.Cache.Get(cache);
            var member = CurrentUser;
            var mark = "";
            if (cacheCode != null && cacheCode.ToString() == code)
            {
                var service = _iMessageService;
                if (service.GetMemberContactsInfo(pluginId, destination, MemberContactsInfo.UserTypes.General) != null)
                {
                    return Json(new Result() { success = false, msg = destination + "已经绑定过了！" });
                }
                if (pluginId.ToLower().Contains("email"))
                {
                    member.Email = destination;
                    mark = "邮箱";
                }
                else if (pluginId.ToLower().Contains("sms"))
                {
                    member.CellPhone = destination;
                    mark = "手机";
                }
                _iMemberService.UpdateMember(member);
                service.UpdateMemberContacts(new Model.MemberContactsInfo() { Contact = destination, ServiceProvider = pluginId, UserId = CurrentUser.Id, UserType = MemberContactsInfo.UserTypes.General });
                Core.Cache.Remove(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId + destination));
                Core.Cache.Remove(CacheKeyCollection.Member(CurrentUser.Id));//移除用户缓存
                Core.Cache.Remove("Rebind" + CurrentUser.Id);
                return Json(new Result() { success = true, msg = "验证正确" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "验证码不正确或者已经超时" });
            }
        }

        [HttpPost]
        public ActionResult SendCode(string pluginId, string destination)
        {
            var timeout = CacheKeyCollection.MemberPluginReBindTime(CurrentUser.UserName, pluginId); //验证码超时时间
            if (Core.Cache.Get(timeout) != null)
            {
                return Json(new Result() { success = false, msg = "120秒内只允许请求一次，请稍后重试！" });
            }
            var checkCode = new Random().Next(10000, 99999);
			//TODO yx 短信验证码超时时间需改成可配置，并且短信模板需添加超时时间变量
            var cacheTimeout = DateTime.Now.AddMinutes(2);
#if DEBUG
            //Log.Debug("[USC]"+ checkCode);
#endif
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId+destination), checkCode, cacheTimeout);
            var user = new MessageUserInfo() { UserName = CurrentUser.UserName, SiteName = CurrentSiteSetting.SiteName, CheckCode = checkCode.ToString() };
            _iMessageService.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginReBindTime(CurrentUser.UserName, pluginId), "0", DateTime.Now.AddSeconds(120));//验证码超时时间
            return Json(new Result() { success = true, msg = "发送成功" });
        }

        [HttpPost]
        public ActionResult SendCodeStep2(string pluginId, string destination)
        {
            var timeout = CacheKeyCollection.MemberPluginReBindStepTime(CurrentUser.UserName, pluginId);
            if (Core.Cache.Get(timeout) != null)
            {
                return Json(new Result() { success = false, msg = "120秒内只允许请求一次，请稍后重试!" });
            }
            var checkCode = new Random().Next(10000, 99999);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId+destination), checkCode, cacheTimeout);
            var user = new MessageUserInfo() { UserName = CurrentUser.UserName, SiteName = CurrentSiteSetting.SiteName, CheckCode = checkCode.ToString() };
            _iMessageService.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginReBindStepTime(CurrentUser.UserName, pluginId), "0", DateTime.Now.AddSeconds(120));
            return Json(new Result() { success = true, msg = "发送成功" });
        }

        public ActionResult ReBindStep2(string pluginId, string key)
        {
            if (Core.Cache.Get("Rebind" + key) as string != "step2")
            {
                RedirectToAction("ReBind", new { pluginId = pluginId });
            }
            var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>(pluginId);
            ViewBag.ShortName = messagePlugin.Biz.ShortName;
            ViewBag.id = pluginId;
            ViewBag.ContactInfo = _iMessageService.GetDestination(CurrentUser.Id, pluginId, MemberContactsInfo.UserTypes.General);
            return View();
        }

        public ActionResult ReBindStep3(string name)
        {
            ViewBag.ShortName = name;
            return View();
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public JsonResult ChangePassword(string oldpassword, string password)
        {
            if (string.IsNullOrWhiteSpace(oldpassword) || string.IsNullOrWhiteSpace(password))
            {
                return Json(new Result() { success = false, msg = "密码不能为空！" });
            }
            var model = CurrentUser;
            var pwd = SecureHelper.MD5(SecureHelper.MD5(oldpassword) + model.PasswordSalt);
            if (pwd == model.Password)
            {
                _iMemberService.ChangePassword(model.Id, password);
                return Json(new Result() { success = true, msg = "修改成功" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "旧密码错误" });
            }
        }

        public JsonResult CheckOldPassWord(string password)
        {
            var model = CurrentUser;
            var pwd = SecureHelper.MD5(SecureHelper.MD5(password) + model.PasswordSalt);
            if (model.Password == pwd)
            {
                return Json(new Result() { success = true });
            }
            return Json(new Result() { success = false });
        }

		/// <summary>
		/// 获取用户标识
		/// </summary>
		/// <returns></returns>
		public JsonResult UserIdentity()
		{
			if (CurrentUser == null)
				return Json(0, JsonRequestBehavior.AllowGet);

			var identity=(CurrentUser.Id + CurrentUser.CreateDate.ToString("yyyyMMddHHmmss")).GetHashCode();
			return Json(identity, JsonRequestBehavior.AllowGet);
		}
    }
}