using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace Himall.Application.Mappers.Profiles
{
	public class OrderProfile:Profile
	{
		protected override void Configure()
		{
			base.Configure();

			CreateMap<Model.OrderInfo, DTO.Order>();
			CreateMap<DTO.Order, Model.OrderInfo>();

			CreateMap<Model.OrderInfo, DTO.FullOrder>();
			CreateMap<DTO.FullOrder, Model.OrderInfo>();

			CreateMap<Model.OrderPayInfo, DTO.OrderPay>();
			CreateMap<DTO.OrderPay, Model.OrderPayInfo>();

			CreateMap<Model.OrderRefundInfo, DTO.OrderRefund>();
			CreateMap<DTO.OrderRefund, Model.OrderRefundInfo>();

			CreateMap<Model.OrderItemInfo, DTO.OrderItem>();
			CreateMap<DTO.OrderItem, Model.OrderItemInfo>();

			CreateMap<Model.OrderCommentInfo, DTO.OrderComment>();
			CreateMap<DTO.OrderComment, Model.OrderCommentInfo>();

			CreateMap<Model.OrderRefundlogsInfo, DTO.OrderRefundlog>();
			CreateMap<DTO.OrderRefundlog, Model.OrderRefundlogsInfo>();
		}
	}
}
