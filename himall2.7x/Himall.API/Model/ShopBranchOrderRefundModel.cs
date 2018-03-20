using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.DTO;
using Himall.Model;
using Himall.Core;

namespace Himall.API.Model
{
	public class OrderRefundApiModel : OrderRefund
	{
		static OrderRefundApiModel()
		{
			AutoMapper.Mapper.CreateMap<OrderRefund, OrderRefundApiModel>();
		}

		public DTO.OrderItem OrderItem { get; set; }

		public string StatusDescription { get; set; }

		public int Status { get; set; }

		public string RefundModelDescription
		{
			get
			{
				return RefundMode.ToDescription();
			}
		}

		public string UserName { get; set; }

		public string UserCellPhone { get; set; }

		public string RefundPayTypeDescription
		{
			get
			{
				return RefundPayType.ToDescription();
			}
		}

		public string[] CertPics { get; set; }
	}

	/// <summary>
	/// 订单退款回复实体
	/// </summary>
	public class OrderRefundReplyModel
	{
		public long RefundId { get; set; }

		public OrderRefundInfo.OrderRefundAuditStatus AuditStatus { get; set; }

		public string SellerRemark { get; set; }
	}
}
