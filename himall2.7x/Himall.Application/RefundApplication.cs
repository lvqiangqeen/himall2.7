using System.Collections.Generic;
using Himall.IServices;
using Himall.Core;
using Himall.Model;
using Himall.IServices.QueryModel;
using Himall.CommonModel;
using Himall.Application.Mappers;
using Himall.DTO;
using System.Linq;
using Himall.CommonModel.Delegates;

namespace Himall.Application
{
    public class RefundApplication
    {
		#region 字段
		private static IRefundService _iRefundService = ObjectContainer.Current.Resolve<IRefundService>();
		#endregion

		#region 属性
		public static event RefundSuccessed OnRefundSuccessed
		{
			add
			{
				_iRefundService.OnRefundSuccessed += value;
			}
			remove
			{
				_iRefundService.OnRefundSuccessed -= value;
			}
		}
		#endregion

		#region 方法
		/// <summary>
		/// 添加一个退款申请
		/// </summary>
		/// <param name="info"></param>
		public static void AddOrderRefund(OrderRefundInfo info)
		{
			_iRefundService.AddOrderRefund(info);
		}

		/// <summary>
		/// 通过订单编号获取整笔退款
		/// </summary>
		/// <param name="id">订单编号</param>
		/// <returns></returns>
		public static OrderRefundInfo GetOrderRefundByOrderId(long id)
		{
			return _iRefundService.GetOrderRefundByOrderId(id);
		}
		/// <summary>
		/// 获取一条退货记录
		/// </summary>
		/// <param name="id"></param>
		public static OrderRefund GetOrderRefund(long id, long? userId = null, long? shopId = null)
		{
			var result = _iRefundService.GetOrderRefund(id, userId).Map<DTO.OrderRefund>();
			return result;
		}
		/// <summary>
		/// 获取退款/退货列表
		/// </summary>
		/// <param name="refundQuery"></param>
		/// <returns></returns>
		public static QueryPageModel<DTO.OrderRefund> GetOrderRefunds(RefundQuery refundQuery)
		{
			var data = _iRefundService.GetOrderRefunds(refundQuery);

			return new QueryPageModel<DTO.OrderRefund>()
			{
				Models = data.Models.Map<List<DTO.OrderRefund>>(),
				Total = data.Total
			};
		}

		/// <summary>
		/// 检查是否可以退款
		/// </summary>
		/// <param name="refundId"></param>
		/// <returns></returns>
		public static bool HasMoneyToRefund(long refundId)
		{
			return _iRefundService.HasMoneyToRefund(refundId);
		}

		/// <summary>
		/// 结算
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		public static List<OrderRefundInfo> GetOrderRefundList(long orderId)
		{
			return _iRefundService.GetOrderRefundList(orderId);
		}

		/// <summary>
		/// 管理员确认退款/退货
		/// </summary>
		/// <param name="id"></param>
		/// <param name="managerRemark"></param>
		/// <param name="managerName"></param>
		/// <param name="notifyurl">导步通知地址</param>
		public static string ConfirmRefund(long id, string managerRemark, string managerName, string notifyurl)
		{
			return _iRefundService.ConfirmRefund(id, managerRemark, managerName, notifyurl);
		}
		/// <summary>
		/// 异步通知确认退款
		/// </summary>
		/// <param name="batchno"></param>
		public static void NotifyRefund(string batchNo)
		{
			_iRefundService.NotifyRefund(batchNo);
		}

		/// <summary>
		/// 买家确定退回商品
		/// </summary>
		/// <param name="id"></param>
		/// <param name="sellerName">用户名</param>
		/// <param name="expressCompanyName">快递公司名</param>
		/// <param name="shipOrderNumber">快递号码</param>
		public static void UserConfirmRefundGood(long id, string sellerName, string expressCompanyName, string shipOrderNumber)
		{
			_iRefundService.UserConfirmRefundGood(id, sellerName, expressCompanyName, shipOrderNumber);
		}

		/// <summary>
		/// 商家处理退款退货申请
		/// </summary>
		/// <param name="id"></param>
		/// <param name="auditStatus"></param>
		/// <param name="sellerRemark"></param>
		/// <param name="sellerName"></param>
		public static void SellerDealRefund(long id, OrderRefundInfo.OrderRefundAuditStatus auditStatus, string sellerRemark, string sellerName)
		{
			_iRefundService.SellerDealRefund(id, auditStatus, sellerRemark, sellerName);
		}

		/// <summary>
		/// 商家确认收到退货
		/// </summary>
		/// <param name="id"></param>
		/// <param name="sellerName"></param>
		public static void SellerConfirmRefundGood(long id, string sellerName)
		{
			_iRefundService.SellerConfirmRefundGood(id, sellerName);
		}

		/// <summary>
		/// 是否可以申请退款
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="orderItemId"></param>
		/// <param name="isAllOrderRefund">是否为整笔退 null 所有 true 整笔退 false 货品售后</param>
		/// <returns></returns>
		public static bool CanApplyRefund(long orderId, long orderItemId, bool? isAllOrderRefund = null)
		{
			return _iRefundService.CanApplyRefund(orderId, orderItemId, isAllOrderRefund);
		}
		/// <summary>
		/// 添加或修改售后原因
		/// </summary>
		/// <param name="id"></param>
		/// <param name="reason"></param>
		public static void UpdateAndAddRefundReason(string reason, long id)
		{
			_iRefundService.UpdateAndAddRefundReason(reason, id);
		}
		/// <summary>
		/// 获取售后原因列表
		/// </summary>
		/// <returns></returns>
		public static List<RefundReasonInfo> GetRefundReasons()
		{
			return _iRefundService.GetRefundReasons();
		}

		/// <summary>
		/// 获取售后日志
		/// </summary>
		/// <param name="refundId">售后编号</param>
		/// <returns></returns>
		public static List<OrderRefundlog> GetRefundLogs(long refundId, int currentApplyNumber = 0, bool haveCurrentApplyNumber = true)
		{
			return _iRefundService.GetRefundLogs(refundId, currentApplyNumber, haveCurrentApplyNumber).Map<List<OrderRefundlog>>();
		}

		/// <summary>
		/// 删除售后原因
		/// </summary>
		/// <param name="id"></param>
		public static void DeleteRefundReason(long id)
		{
			_iRefundService.DeleteRefundReason(id);
		}
		/// <summary>
		/// 激活售后
		/// </summary>
		/// <param name="info"></param>
		public static void ActiveRefund(OrderRefundInfo info)
		{
			_iRefundService.ActiveRefund(info);
		}
		/// <summary>
		/// 自动审核退款(job)
		/// </summary>
		public static void AutoAuditRefund()
		{
			_iRefundService.AutoAuditRefund();
		}
		/// <summary>
		/// 自动关闭过期未寄货退款(job)
		/// </summary>
		public static void AutoCloseByDeliveryExpired()
		{
			_iRefundService.AutoCloseByDeliveryExpired();
		}
		/// <summary>
		/// 自动商家确认到货(job)
		/// </summary>
		public static void AutoShopConfirmArrival()
		{
			_iRefundService.AutoShopConfirmArrival();
		}
		#endregion
    }
}
