using Himall.Core;
using Himall.Core.Helper;
using Himall.Core.Plugins.OAuth;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Himall.API.Model.ParamsModel;
using Himall.Application;
using Himall.CommonModel;

namespace Himall.API
{
    public class ShopBranchLoginController : BaseShopBranchApiController
    {
        #region 静态字段
        private static string _encryptKey = Guid.NewGuid().ToString("N");
        #endregion

        #region 方法
        public object GetUser(string userName, string password)
        {
            var _iSiteSettingService = ObjectContainer.Current.Resolve<ISiteSettingService>();
            var siteSettings = _iSiteSettingService.GetSiteSettings();

            //普通登录
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                //商家登录也放在这里，因为商家app和门店app为同一个并且没做登录区分，只能通过登录后才能知道是登录的商家还是门店管理员
                var seller = ManagerApplication.Login(userName, password);
                if (seller != null)
                {
                    if (!siteSettings.IsOpenShopApp)
                        return ErrorResult("未授权商家APP");
                    
                    var shop = ShopApplication.GetShop(seller.ShopId);
                    if (shop != null && shop.ShopStatus != ShopInfo.ShopAuditStatus.Open)
                        return ErrorResult("无效的账号");
                    dynamic result = SuccessResult();
                    string memberId = UserCookieEncryptHelper.Encrypt(seller.Id, CookieKeysCollection.USERROLE_SELLERADMIN);
                    result.UserKey = memberId;
                    result.type = ManagerType.ShopManager;
                    return result;
                }
                if (!siteSettings.IsOpenStore)
                    return ErrorResult("未授权门店模块");
                var member = Himall.Application.ShopBranchApplication.ShopBranchLogin(userName, password);
                if (member != null)
                {
                    dynamic result = SuccessResult();
                    string memberId = UserCookieEncryptHelper.Encrypt(member.Id, CookieKeysCollection.USERROLE_USER);
                    result.UserKey = memberId;
                    result.type = ManagerType.ShopBranchManager;
                    return result;
                }


                return ErrorResult("用户名或密码错误");
            }
            return ErrorResult("用户名或密码不能为空");
        }

        #region 重写方法
        protected override bool CheckContact(string contact, out string errorMessage)
        {
            errorMessage = string.Empty;
            var shopBranch = ShopBranchApplication.GetShopBranchByContact(contact);
            if (shopBranch == null)
                return false;

            var managers = ShopBranchApplication.GetShopBranchManagerByShopBranchId(shopBranch.Id);

            if (managers.Count > 0)
            {
                //TODO 固定取第一个管理员，因为一个门店只有一个管理员，如果有多个管理员，应在管理员表添加联系方式字段
                var manager = managers[0];
                Cache.Insert(_encryptKey + contact, string.Format("{0}:{1:yyyyMMddHHmmss}", manager.Id, manager.CreateDate), DateTime.Now.AddHours(1));
            }

            return managers.Count > 0;
        }

        protected override string CreateCertificate(string contact)
        {
            var identity = Cache.Get<string>(_encryptKey + contact);
            identity = SecureHelper.AESEncrypt(identity, _encryptKey);
            return identity;
        }

        protected override object ChangePassowrdByCertificate(string certificate, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return ErrorResult("密码不能为空");

            certificate = SecureHelper.AESDecrypt(certificate, _encryptKey);
            long userId = long.TryParse(certificate.Split(':')[0], out userId) ? userId : 0;

            if (userId == 0)
                throw new HimallException("数据异常");

            ShopBranchApplication.UpdateShopBranchManagerPwd(userId, password);

            return SuccessResult("密码修改成功");
        }

        protected override object ChangePasswordByOldPassword(string oldPassword, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return ErrorResult("密码不能为空");

            CheckUserLogin();

            var user = CurrentUser;

            var pwd = SecureHelper.MD5(SecureHelper.MD5(oldPassword) + user.PasswordSalt);

            if (pwd == user.Password)
            {
                ShopBranchApplication.UpdateShopBranchManagerPwd(user.Id, password);
                return SuccessResult("密码修改成功");
            }

            return ErrorResult("旧密码输入不正确");
        }
        #endregion
        #endregion
    }
}
