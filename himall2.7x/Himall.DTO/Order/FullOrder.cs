using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
	/// <summary>
	/// 包括额外相关信息的订单
	/// </summary>
	public class FullOrder:Order
	{
		public List<OrderItem> OrderItems { get; set; }

		/// <summary>
		/// 订单包含的所有商品总件数
		/// </summary>
		public long ProductCount
		{
			get
			{
				if (OrderItems == null || OrderItems.Count == 0)
					return 0;
				return OrderItems.Sum(p => p.Quantity);
			}
		}
        /// <summary>
        /// 门店名称
        /// </summary>
        public string ShopBranchName { get; set; }
	}
}
