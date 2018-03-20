using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel.Enum
{
	/// <summary>
	/// 退款步聚
	/// </summary>
	public enum OrderRefundStep:short
	{
		/// <summary>
		/// 待商家/门店审核
		/// </summary>
		[Description("待商家审核")]
		WaitAudit = 1,

		/// <summary>
		/// 待买家寄货
		/// </summary>
		[Description("待买家寄货")]
		WaitDelivery = 2,

		/// <summary>
		/// 待商家/门店收货
		/// </summary>
		[Description("待商家收货")]
		WaitReceiving = 3,

		/// <summary>
		/// 商家/门店拒绝
		/// </summary>
		[Description("商家拒绝")]
		UnAudit = 4,

		/// <summary>
		/// 待平台确认
		/// </summary>
		[Description("待平台确认")]
		UnConfirm = 5,

		/// <summary>
		/// 退款成功
		/// </summary>
		[Description("退款成功")]
		Confirmed = 6
	}
}
