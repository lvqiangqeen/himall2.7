using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.IServices
{
	public interface ISkuService:IService
	{
		/// <summary>
		/// 根据id获取sku信息
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		List<SKUInfo> GetByIds(IEnumerable<long> ids);

		/// <summary>
		/// 根据商品id获取sku信息
		/// </summary>
		/// <param name="productIds"></param>
		/// <returns></returns>
		List<SKUInfo> GetByProductIds(IEnumerable<long> productIds);
	}
}
