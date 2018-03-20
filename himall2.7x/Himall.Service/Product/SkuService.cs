using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices;
using Himall.Model;

namespace Himall.Service
{
	public class SkuService : ServiceBase, ISkuService
	{
		/// <summary>
		/// 根据id获取sku信息
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public List<SKUInfo> GetByIds(IEnumerable<long> ids)
		{
			return this.Context.SKUInfo.Where(p => ids.Contains(p.AutoId)).ToList();
		}

		/// <summary>
		/// 根据商品id获取sku信息
		/// </summary>
		/// <param name="productIds"></param>
		/// <returns></returns>
		public List<SKUInfo> GetByProductIds(IEnumerable<long> productIds)
		{
			return this.Context.SKUInfo.Where(p => productIds.Contains(p.ProductId)).ToList();
		}
	}
}
