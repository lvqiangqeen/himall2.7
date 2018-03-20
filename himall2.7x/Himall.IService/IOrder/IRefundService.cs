using Himall.IServices.QueryModel;
using Himall.Model;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Expressions;
using Himall.CommonModel;
using Himall.CommonModel.Delegates;

namespace Himall.IServices
{
    /// <summary>
    /// 退款/退货服务接口
    /// </summary>
    public interface IRefundService : IService
    {
		#region 属性
		/// <summary>
		/// 退款成功
		/// </summary>
		event RefundSuccessed OnRefundSuccessed;
		#endregion

		#region 方法
		/// <summary>
		/// 添加一个退款申请
		/// </summary>
		/// <param name="info"></param>
		void AddOrderRefund(OrderRefundInfo info);
		/// <summary>
		/// 通过订单编号获取整笔退款
		/// </summary>
		/// <param name="id">订单编号</param>
		/// <returns></returns>
		OrderRefundInfo GetOrderRefundByOrderId(long id);
        /// <summary>
        /// 获取一条退货记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="shopId"></param>
        OrderRefundInfo GetOrderRefund(long id, long? userId = null, long? shopId = null);

		/// <summary>
		/// 获取退款/退货列表
		/// </summary>
		/// <param name="refundQuery"></param>
		/// <returns></returns>
		QueryPageModel<OrderRefundInfo> GetOrderRefunds(RefundQuery refundQuery);

		/// <summary>
		/// 检查是否可以退款
		/// </summary>
		/// <param name="refundId"></param>
		/// <returns></returns>
		bool HasMoneyToRefund(long refundId);

		/// <summary>
		/// 结算
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		List<OrderRefundInfo> GetOrderRefundList(long orderId);

		/// <summary>
		/// 管理员确认退款/退货
		/// </summary>
		/// <param name="id"></param>
		/// <param name="managerRemark"></param>
		/// <param name="managerName"></param>
		/// <param name="notifyurl">导步通知地址</param>
		string ConfirmRefund(long id, string managerRemark, string managerName, string notifyurl);
		/// <summary>
		/// 异步通知确认退款
		/// </summary>
		/// <param name="batchno"></param>
		void NotifyRefund(string batchNo);

		/// <summary>
		/// 买家确定退回商品
		/// </summary>
		/// <param name="id"></param>
		/// <param name="sellerName">用户名</param>
		/// <param name="expressCompanyName">快递公司名</param>
		/// <param name="shipOrderNumber">快递号码</param>
		void UserConfirmRefundGood(long id, string sellerName, string expressCompanyName, string shipOrderNumber);

		/// <summary>
		/// 商家处理退款退货申请
		/// </summary>
		/// <param name="id"></param>
		/// <param name="auditStatus"></param>
		/// <param name="sellerRemark"></param>
		/// <param name="sellerName"></param>
		void SellerDealRefund(long id, OrderRefundInfo.OrderRefundAuditStatus auditStatus, string sellerRemark, string sellerName);

		/// <summary>
		/// 商家确认收到退货
		/// </summary>
		/// <param name="id"></param>
		/// <param name="sellerName"></param>
		void SellerConfirmRefundGood(long id, string sellerName);/// <summary>
		/// 是否可以申请退款
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="orderItemId"></param>
		/// <param name="isAllOrderRefund">是否为整笔退 null 所有 true 整笔退 false 货品售后</param>
		/// <returns></returns>
		bool CanApplyRefund(long orderId, long orderItemId, bool? isAllOrderRefund = null);
		/// <summary>
		/// 添加或修改售后原因
		/// </summary>
		/// <param name="id"></param>
		/// <param name="reason"></param>
		void UpdateAndAddRefundReason(string reason, long id);
		/// <summary>
		/// 获取售后原因列表
		/// </summary>
		/// <returns></returns>
		List<RefundReasonInfo> GetRefundReasons();
		/// <summary>
		/// 获取售后日志
		/// </summary>
		/// <param name="refundId">售后编号</param>
		/// <param name="currentApplyNumber">当前售后次数,0表示所有</param>
		/// <param name="haveCurrentApplyNumber">是否包含当前售后的日志</param>
		/// <returns></returns>
		List<OrderRefundlogsInfo> GetRefundLogs(long refundId, int currentApplyNumber = 0, bool haveCurrentApplyNumber = true);
		/// <summary>
		/// 删除售后原因
		/// </summary>
		/// <param name="id"></param>
		void DeleteRefundReason(long id);
		/// <summary>
		/// 激活售后
		/// </summary>
		/// <param name="info"></param>
		void ActiveRefund(OrderRefundInfo info);
		/// <summary>
		/// 自动审核退款(job)
		/// </summary>
		void AutoAuditRefund();
		/// <summary>
		/// 自动关闭过期未寄货退款(job)
		/// </summary>
		void AutoCloseByDeliveryExpired();
		/// <summary>
		/// 自动商家确认到货(job)
		/// </summary>
		void AutoShopConfirmArrival();
		#endregion
    }
}
