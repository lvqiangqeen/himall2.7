using Himall.Application;
using Himall.Core;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.SmallProgAPI.Helper;
using Himall.SmallProgAPI.Model;
using Himall.CommonModel;
using System.IO;
using System.Web.Http;
using Himall.Core.Plugins.Message;
using Himall.Core.Plugins;

namespace Himall.SmallProgAPI
{
    public class ShopAccountController : BaseApiController
    {
        /// <summary>
        /// 同一用户名无需验证的的尝试登录次数
        /// </summary>
        const int TIMES_WITHOUT_CHECKCODE = 3;

        /// <summary>
        ///账号密码登录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public object Login(string username = "", string password = "")
        {
            try
            {
                //检查输入合法性
                CheckInput(username, password);

                var manager = ServiceProvider.Instance<IManagerService>.Create.Login(username, password);
                if (manager == null)
                {
                    return Json(new { success = false, msg = "用户名和密码不匹配" });
                }

                if (manager.ShopId == 0)
                {
                    return Json(new { success = false, msg = "不存在此商家" });
                }

                var shopinfo = ServiceProvider.Instance<IShopService>.Create.GetShop(manager.ShopId);
                if (shopinfo == null)
                {
                    return Json(new { success = false, msg = "不存在此商家" });
                }

                ClearErrorTimes(username);//清除输入错误记录次数
                return Json(new { success = true, msg = UserCookieEncryptHelper.Encrypt(manager.Id, CookieKeysCollection.USERROLE_SELLERADMIN), shopId = manager.ShopId });
            }
            catch (Himall.Core.HimallException ex)
            {
                int errorTimes = SetErrorTimes(username);
                return Json(new { success = false, msg = ex.Message, errorTimes = errorTimes, minTimesWithoutCheckCode = TIMES_WITHOUT_CHECKCODE });
            }
            catch (Exception ex)
            {
                int errorTimes = SetErrorTimes(username);
                Core.Log.Error("用户" + username + "小程序接口登录时发生异常", ex);
                return Json(new { success = false, msg = "未知错误", errorTimes = errorTimes, minTimesWithoutCheckCode = TIMES_WITHOUT_CHECKCODE });
            }
        }
        /// <summary>
        ///退出登录
        /// </summary>
        /// <returns></returns>
        public object Logout(string openId)
        {
            if (!string.IsNullOrEmpty(openId))
            {
                var member = Application.MemberApplication.GetMemberByOpenId(SmallProgServiceProvider, openId);
                if (member == CurrentUser)
                {
                    var cacheKey = WebHelper.GetCookie(CookieKeysCollection.HIMALL_USER);
                    if (!string.IsNullOrWhiteSpace(cacheKey))
                    {
                        //_iMemberService.DeleteMemberOpenId(userid, string.Empty);
                        WebHelper.DeleteCookie(CookieKeysCollection.HIMALL_USER);
                        WebHelper.DeleteCookie(CookieKeysCollection.SELLER_MANAGER);
                        //记录主动退出符号
                        WebHelper.SetCookie(CookieKeysCollection.HIMALL_ACTIVELOGOUT, "1", DateTime.MaxValue);
                        return Json(new { success = true });
                    }
                }
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public object Register(StoreInfoModel model)
        {
            if (string.IsNullOrWhiteSpace(model.contactsPhone))
            {
                return Json(new { success = false, msg = "错误的手机号码" });
            }

            bool result = ServiceProvider.Instance<IMemberService>.Create.CheckMobileExist(model.contactsPhone);
            if (result)
            {
                return Json(new { success = false, msg = "当前手机号已注册" });
            }
            else
            {
                result = ServiceProvider.Instance<IMemberService>.Create.CheckMemberExist(model.contactsPhone);
                if (result)
                {
                    return Json(new { success = false, msg = "当前手机号已注册" });
                }
            }
            
            var member = ServiceProvider.Instance<IMemberService>.Create.Register(model.contactsPhone, model.password, model.contactsPhone);
            if (member != null)
            {
                var manager = ServiceProvider.Instance<IManagerService>.Create.AddSellerManager(model.contactsPhone, member.Password, member.PasswordSalt);
                if (manager != null)
                {
                    var shop = ServiceProvider.Instance<IShopService>.Create.GetShop(manager.ShopId);
                    shop.ShopName = model.shopName;
                    shop.Industry = model.industry;
                    shop.CompanyPhone = model.companyPhone;
                    shop.Lat = model.lat;
                    shop.Lng = model.lng;
                    shop.CompanyAddress = model.companyAddress;
                    shop.OpeningTime = model.openingTime;
                    shop.BusinessLicenceNumberPhoto = model.businessLicenceNumberPhoto;
                    shop.BranchImage = model.branchImage;

                    if (!string.IsNullOrEmpty(model.branchImage))
                    {
                        if (model.branchImage.Split(';').Length > 0)
                        {
                            shop.Logo = model.branchImage.Split(';')[0];
                        }
                    }

                    shop.ShopDescription = model.shopDescription;
                    shop.WelcomeTitle = model.shopDescription;
                    shop.ContactsName = model.contactsName;
                    shop.ContactsPhone = model.contactsPhone;
                    shop.ContactsPosition = model.contactsPosition;
                    shop.BusinessType = Himall.CommonModel.ShopBusinessType.Enterprise;
                    shop.Stage = Himall.Model.ShopInfo.ShopStage.Finish;
                    shop.ShopStatus = Himall.Model.ShopInfo.ShopAuditStatus.WaitConfirm;//默认状态为待确认
                    shop.EndDate = DateTime.Now.AddYears(1);
                    shop.GradeId = 1;//写入店铺默认套餐

                    //更新商家信息
                    ServiceProvider.Instance<IShopService>.Create.UpdateShop(shop);

                    //添加商家结算账户
                    ShopAccountInfo shopAccountInfo = new ShopAccountInfo()
                    {
                        ShopId = shop.Id,
                        ShopName = shop.ShopName,
                        Balance = 0,
                        PendingSettlement = 0,
                        Settled = 0
                    };
                    ServiceProvider.Instance<IBillingService>.Create.AddShopAccount(shopAccountInfo);

                    //登录
                    ServiceProvider.Instance<IManagerService>.Create.Login(model.contactsPhone, model.password);
                    return Json(new { success = true, msg = UserCookieEncryptHelper.Encrypt(manager.Id, CookieKeysCollection.USERROLE_SELLERADMIN) });
                }
                else
                {
                    return Json(new { success = false, msg = "注册失败" });
                }
            }
            else
            {
                return Json(new { success = false, msg = "注册失败" });
            }
        }


        [HttpPost]
        public object UpdateShopInfo(StoreInfoModel model, string userKey)
        {
            try
            {
                long userId = UserCookieEncryptHelper.Decrypt(userKey, CookieKeysCollection.USERROLE_SELLERADMIN);
                IMemberService iMenberService = ServiceProvider.Instance<IMemberService>.Create;
                IManagerService iManagerService = ServiceProvider.Instance<IManagerService>.Create;
                IShopService iShopService = ServiceProvider.Instance<IShopService>.Create;
                UserMemberInfo memberInfo = iMenberService.GetMember(userId);
                if (memberInfo != null)
                {
                    ////修改用户账号信息
                    //string encryptedPassword = SecureHelper.MD5(model.password);//一次MD5加密
                    //string encryptedWithSaltPassword = SecureHelper.MD5(encryptedPassword + memberInfo.PasswordSalt);//一次结果加盐后二次加密
                    //memberInfo.Password = encryptedWithSaltPassword;
                    //iMenberService.UpdateMemberInfo(memberInfo);

                    ////修改商家账号信息
                    ManagerInfo manager = iManagerService.GetSellerManager(memberInfo.Id);
                    //manager.Password = model.password;
                    //iManagerService.ChangeSellerManager(manager);

                    //修改商家信息
                    var shop = iShopService.GetShop(manager.ShopId);
                    shop.ShopName = model.shopName;
                    shop.Industry = model.industry;
                    shop.CompanyPhone = model.companyPhone;
                    shop.Lat = model.lat;
                    shop.Lng = model.lng;
                    shop.CompanyAddress = model.companyAddress;
                    shop.OpeningTime = model.openingTime;
                    shop.BusinessLicenceNumberPhoto = model.businessLicenceNumberPhoto;
                    shop.BranchImage = model.branchImage;

                    if (!string.IsNullOrEmpty(model.branchImage))
                    {
                        if (model.branchImage.Split(';').Length > 0)
                        {
                            shop.Logo = model.branchImage.Split(';')[0];
                        }
                    }

                    shop.ShopDescription = model.shopDescription;
                    shop.WelcomeTitle = model.shopDescription;
                    //shop.ContactsName = model.contactsName;
                    //shop.ContactsPosition = model.contactsPosition;
                    iShopService.UpdateShop(shop);

                    //登录
                    ServiceProvider.Instance<IManagerService>.Create.Login(model.contactsPhone, model.password);
                    return Json(new { success = true, msg = UserCookieEncryptHelper.Encrypt(manager.Id, CookieKeysCollection.USERROLE_SELLERADMIN) });
                }
                else
                {
                    return Json(new { success = false, msg = "修改失败" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        [HttpPost]
        public object UpdateShopPassword(StorePasswordInfoModel model, string userKey)
        {
            try
            {
                long userId = UserCookieEncryptHelper.Decrypt(userKey, CookieKeysCollection.USERROLE_SELLERADMIN);
                IMemberService iMenberService = ServiceProvider.Instance<IMemberService>.Create;
                IManagerService iManagerService = ServiceProvider.Instance<IManagerService>.Create;
                IShopService iShopService = ServiceProvider.Instance<IShopService>.Create;
                UserMemberInfo memberInfo = iMenberService.GetMember(userId);
                if (memberInfo != null)
                {
                    //修改用户账号信息
                    string encryptedPassword = SecureHelper.MD5(model.password);//一次MD5加密
                    string encryptedWithSaltPassword = SecureHelper.MD5(encryptedPassword + memberInfo.PasswordSalt);//一次结果加盐后二次加密
                    memberInfo.Password = encryptedWithSaltPassword;
                    iMenberService.UpdateMemberInfo(memberInfo);

                    //修改商家账号信息
                    ManagerInfo manager = iManagerService.GetSellerManager(memberInfo.Id);
                    manager.Password = model.password;
                    iManagerService.ChangeSellerManager(manager);

                    //登录
                    ServiceProvider.Instance<IManagerService>.Create.Login(memberInfo.CellPhone, model.password);
                    return Json(new { success = true, msg = UserCookieEncryptHelper.Encrypt(manager.Id, CookieKeysCollection.USERROLE_SELLERADMIN) });
                }
                else
                {
                    return Json(new { success = false, msg = "修改失败" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        /// <summary>
		/// 发送手机验证码
		/// </summary>
		/// <param name="imageCheckCode"></param>
		/// <param name="contact"></param>
		/// <returns></returns>
		public object GetPhoneCheckCode(string contactsPhone)
        {
            string msg;
            var checkResult = this.CheckContact(contactsPhone, out msg);
            if (contactsPhone == "")
                return ErrorResult("请输入手机号");

            PluginInfo pluginInfo;
            var isMobile = Core.Helper.ValidateHelper.IsMobile(contactsPhone);
            if (isMobile)
                pluginInfo = PluginsManagement.GetInstalledPluginInfos(Core.Plugins.PluginType.SMS).First();
            else
                pluginInfo = PluginsManagement.GetInstalledPluginInfos(PluginType.Email).First();

            if (pluginInfo == null)
            {
                Log.Error(string.Format("未找到{0}发送插件", isMobile ? "短信" : "邮件"));
                return ErrorResult("验证码发送失败");
            }

            var timeoutKey = CacheKeyCollection.MemberPluginCheckTime(contactsPhone, pluginInfo.PluginId);
            if (Core.Cache.Get(timeoutKey) != null)
                return ErrorResult("请求过于频繁，请稍后再试!");
            int cacheTime = 60;
            Core.Cache.Insert(timeoutKey, cacheTime, DateTime.Now.AddSeconds(cacheTime));

            var checkCode = new Random().Next(10000, 99999);
            var siteName = Application.SiteSettingApplication.GetSiteSettings().SiteName;
            var message = new Himall.Core.Plugins.Message.MessageUserInfo() { UserName = contactsPhone, SiteName = siteName, CheckCode = checkCode.ToString() };

            Application.MessageApplication.SendMessageCode(contactsPhone, pluginInfo.PluginId, message);
            //缓存验证码
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(contactsPhone, pluginInfo.PluginId), checkCode, DateTime.Now.AddMinutes(10));

            return SuccessResult("验证码发送成功");
        }

        /// <summary>
        /// 验证手机验证码
        /// </summary>
        /// <param name="checkCode">验证码</param>
        /// <param name="contact">手机号或邮箱</param>
        /// <returns></returns>
        public object GetCheckPhoneCheckCode(string checkCode, string contactsPhone)
        {
            if (string.IsNullOrEmpty(checkCode))
                return ErrorResult("请输入验证码");

            PluginInfo pluginInfo;
            pluginInfo = PluginsManagement.GetInstalledPluginInfos(Core.Plugins.PluginType.SMS).First();

            var cache = CacheKeyCollection.MemberPluginCheck(contactsPhone, pluginInfo.PluginId);
            var cacheCode = Core.Cache.Get(cache);

            if (cacheCode != null && cacheCode.ToString() == checkCode)
                return OnCheckCheckCodeSuccess(contactsPhone);
            else
                return ErrorResult("验证码输入错误");
        }


        object GetErrorJson(string errorMsg)
        {
            var message = new
            {
                Status = "NO",
                Message = errorMsg
            };
            return message;
        }

        /// <summary>
        /// 获取指定用户名在30分钟内的错误登录次数
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        int GetErrorTimes(string username)
        {
            var timesObject = Core.Cache.Get(CacheKeyCollection.ManagerLoginError(username));
            var times = timesObject == null ? 0 : (int)timesObject;
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

        void CheckInput(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new Himall.Core.HimallException("请填写用户名");

            if (string.IsNullOrWhiteSpace(password))
                throw new Himall.Core.HimallException("请填写密码");
        }
    }
}
