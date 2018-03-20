using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices;

namespace Himall.Application
{
	public class TradeCommentApplication
	{
		#region 字段
		private static ITradeCommentService _tradeCommentService;
		#endregion

		#region 构造函数
		static TradeCommentApplication()
		{
			_tradeCommentService = Himall.Core.ObjectContainer.Current.Resolve<ITradeCommentService>();
		}
		#endregion

		#region 方法
		/// <summary>
		/// 根据用户ID和订单ID获取单个订单评价信息
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		public static DTO.OrderComment GetOrderComment(long orderId, long userId)
		{
			return _tradeCommentService.GetOrderCommentInfo(orderId, userId).Map<DTO.OrderComment>();
		}

		public static void Add(DTO.OrderComment model)
		{
			var info = model.Map<Model.OrderCommentInfo>();
			_tradeCommentService.AddOrderComment(info);
			model.Id = info.Id;
		}
		#endregion
	}
}
