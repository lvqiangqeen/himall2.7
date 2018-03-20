using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.DTO;

namespace Himall.Application
{
	public class ShopCategoryApplication
	{
		#region 字段
		private static IServices.IShopCategoryService _shopCategoryService;
		#endregion

		#region 构造函数
		static ShopCategoryApplication()
		{
			_shopCategoryService = Himall.Core.ObjectContainer.Current.Resolve<IServices.IShopCategoryService>();
		}
		#endregion

		#region 方法
		public static List<ShopCategory> GetShopCategory(long shopId)
		{
			return _shopCategoryService.GetShopCategory(shopId).ToList().Map<List<ShopCategory>>();
		}
        /// <summary>
        /// 根据父级Id获取商品分类
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static List<ShopCategory> GetCategoryByParentId(long id,long shopId)
        {
            return _shopCategoryService.GetCategoryByParentId(id, shopId).ToList().Map<List<ShopCategory>>();
        }
        #endregion
    }
}
