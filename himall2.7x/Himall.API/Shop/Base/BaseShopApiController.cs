using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Himall.Core;
using Himall.Core.Helper;
using Himall.DTO;
using Himall.Application;
using Himall.Web.Framework;

namespace Himall.API
{
    public abstract class BaseShopApiController : BaseApiController<Manager>
    {
        #region 私有
        /// <summary>
        /// 管理员编号
        /// </summary>
        private long _manageId;
        /// <summary>
        /// 门店签名Key
        /// </summary>
        private const string SELLER_ADMIN_ENCRYP_KEY = "SellerAdmin";
        private const string CACHE_KEY_PREFIX = "SA_";
        #endregion

        /// <summary>
        /// 当前店铺信息
        /// </summary>
        public DTO.Shop CurrentShop
        {
            get
            {
				var user = CurrentUser;
				if (user == null)
					return null;

				return GetShopByShopId(user.ShopId);
            }
        }
        /// <summary>
        /// 检测用户登录信息是否有效
        /// </summary>
        public void CheckShopManageLogin()
        {
            if (CurrentShop == null)
            {
                throw new HimallApiException(ApiErrorCode.Invalid_User_Key_Info, "userkey");
            }
        }

		#region 重写父类方法
		protected override Manager GetUser()
		{
			return ManagerApplication.GetSellerManager(this.CurrentUserId);
		}

		protected override long DecryptUserKey(string userKey)
		{
			return UserCookieEncryptHelper.Decrypt(userKey, CookieKeysCollection.USERROLE_SELLERADMIN);
		}
		#endregion

        #region 私有
        /// <summary>
        /// 获取店铺信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private DTO.Shop GetShopByShopId(long shopId)
        {
            string cachekey = CACHE_KEY_PREFIX + shopId;
            var result = Cache.Get<DTO.Shop>(cachekey);
            if (result == null)
            {
				if (shopId != 0)
				{
					result = ShopApplication.GetShop(shopId);
					Cache.Insert<DTO.Shop>(cachekey, result);
				}
            }
            return result;
        }
        #endregion
        /// <summary>
        /// 清除店铺信息缓存
        /// </summary>
        protected void RemoveShopCache()
        {
            string cachekey = CACHE_KEY_PREFIX + CurrentUser.ShopId;
            var shop = Cache.Get<DTO.Shop>(cachekey);
            if (shop != null)
            {
                Cache.Remove(cachekey);
            };
        }
    }
}
