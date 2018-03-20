using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Himall.IServices;

namespace Himall.Application
{
	public class SKUApplication
	{
		#region 字段
		private static ISkuService _skuService;
		#endregion

		#region 构造函数
		static SKUApplication()
		{
			_skuService = ObjectContainer.Current.Resolve<IServices.ISkuService>();
		}
		#endregion

		#region 方法
		
		/// <summary>
		/// 根据id获取sku信息
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public static List<DTO.SKU> GetByIds(IEnumerable<long> ids)
		{
			return _skuService.GetByIds(ids).Map<List<DTO.SKU>>();
		}

		/// <summary>
		/// 根据商品id获取sku信息
		/// </summary>
		/// <param name="productIds"></param>
		/// <returns></returns>
		public static List<DTO.SKU> GetByProductIds(IEnumerable<long> productIds)
		{
            var data = _skuService.GetByProductIds(productIds);
			return data.Map<List<DTO.SKU>>();
		}
		#endregion
	}
}
