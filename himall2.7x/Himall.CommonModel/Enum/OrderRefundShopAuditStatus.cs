using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel.Enum
{
	/// <summary>
	/// 售后门店审核状态
	/// </summary>
	public enum OrderRefundShopAuditStatus
	{
		/// <summary>
		/// 待门店审核
		/// </summary>
		[Description("待门店审核")]
		WaitAudit = 1,

		/// <summary>
		/// 待买家寄货
		/// </summary>
		[Description("待买家寄货")]
		WaitDelivery = 2,

		/// <summary>
		/// 待门店收货
		/// </summary>
		[Description("待门店收货")]
		WaitReceiving = 3,

		/// <summary>
		/// 门店拒绝
		/// </summary>
		[Description("门店拒绝")]
		UnAudit = 4,

		/// <summary>
		/// 门店通过审核
		/// </summary>
		[Description("门店通过审核")]
		Audited = 5
	}
}
