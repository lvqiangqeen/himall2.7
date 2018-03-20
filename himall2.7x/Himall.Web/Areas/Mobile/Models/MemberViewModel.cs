using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Mobile.Models
{
	public class ChangePayPwd
	{
		/// <summary>
		/// 原支付密码
		/// </summary>
		public string OldPayPwd { get; set; }

		/// <summary>
		/// 新支付密码
		/// </summary>
		public string NewPayPwd { get; set; }

		/// <summary>
		/// 手机验证码
		/// </summary>
		public string PhoneCode { get; set; }

		/// <summary>
		/// 发送验证码的插件id
		/// </summary>
		public string SendCodePluginId { get; set; }
	}
}