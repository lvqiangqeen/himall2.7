using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel.Model
{
	/// <summary>
	/// 微信分享参数
	/// </summary>
	public class WeiXinShareArgs
	{
		#region 构造函数
		public WeiXinShareArgs(string appId, string timestamp, string nonceStr, string signature,string ticket)
		{
			this.AppId = appId;
			this.Timestamp = timestamp;
			this.NonceStr = nonceStr;
			this.Signature = signature;
			this.Ticket = ticket;
		}
		#endregion

		#region 属性
		public string AppId { get; set; }

		/// <summary>
		/// 生成签名的时间戳
		/// </summary>
		public string Timestamp { get; set; }

		/// <summary>
		/// 生成签名的随机串
		/// </summary>
		public string NonceStr { get; set; }

		/// <summary>
		/// 签名
		/// </summary>
		public string Signature { get; set; }

		public string Ticket { get; set; }
		#endregion
	}
}
