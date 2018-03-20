using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
	/// <summary>
	/// 统计待结算信息
	/// </summary>
	public class StatisticsPendingSettlement
	{
		/// <summary>
		/// 商家id
		/// </summary>
		public long ShopId { get; set; }

		/// <summary>
		/// 预计可结算金额
		/// </summary>
		public decimal Amount { get; set; }
	}
}
