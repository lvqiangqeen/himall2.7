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
    public abstract class BaseShopBranchApiController : BaseApiController<ShopBranchManager>
    {
		#region 私有
		/// <summary>
		/// 门店管理员KEY
		/// </summary>
		private const string SBM_CACHE_KEY_PREFIX = "SBM_";
        /// <summary>
        /// 门店KEY
        /// </summary>
        private const string SB_CACHE_KEY_PREFIX = "SB_";
        #endregion

		#region 属性
		public DTO.ShopBranch CurrentShopBranch
		{
			get
			{
				DTO.ShopBranch result = GetShopBranchById(this.CurrentUser.ShopBranchId);
				return result;
			}
		}
		#endregion

		#region 重写方法
		protected override ShopBranchManager GetUser()
		{
			var userId = this.CurrentUserId;
			string cachekey = SBM_CACHE_KEY_PREFIX + userId;
			var result = Cache.Get<DTO.ShopBranchManager>(cachekey);
			if (result == null)
			{
				if (userId != 0)
				{
					result = ShopBranchApplication.GetShopBranchManager(userId);
					if (result != null && result.Id > 0)
					{
						Cache.Insert<DTO.ShopBranchManager>(cachekey, result);
					}
				}
			}
			return result;
		}
		#endregion

        /// <summary>
        /// 是否门店管理员
        /// </summary>
        public bool IsBranchManager
        {
            get
            {
				CheckUserLogin();

				if (this.CurrentUser.UserType == CommonModel.ManagerType.ShopBranchManager)
                { 
                    return true; 
                }
                else
                {
                    return false;
                }
            }
        }
        #region 私有方法
        /// <summary>
        /// 当前门店信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private DTO.ShopBranch GetShopBranchById(long id)
        {
            DTO.ShopBranch result = null;
			string cachekey = SB_CACHE_KEY_PREFIX + id;
            result = Cache.Get<DTO.ShopBranch>(cachekey);
            if (result == null)
            {
                result = ShopBranchApplication.GetShopBranchById(id);
                if (result != null && result.Id > 0)
                {
                    Cache.Insert<DTO.ShopBranch>(cachekey, result);
                }

            }
            return result;
        }
        #endregion
    }
}
