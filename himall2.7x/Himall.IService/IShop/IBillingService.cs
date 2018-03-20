using System;
using System.Collections.Generic;
using Himall.CommonModel;
using Himall.CommonModel.QueryModel;
using Himall.Model;

namespace Himall.IServices
{
	/// <summary>
	/// 结算的数据层接口
	/// </summary>
	public interface IBillingService : IService
	{
		/// <summary>
		/// 店铺帐户（在入驻成功后建立一个帐户）
		/// </summary>
		/// <param name="shpAccount"></param>
		void AddShopAccount(ShopAccountInfo model);

		/// <summary>
		/// 更新店铺资金信息（结算时，退款时，充值时）
		/// </summary>
		/// <param name="shopAccount"></param>
		void UpdateShopAccount(ShopAccountInfo model);


		/// <summary>
		/// 更新平台资金信息（结算时，退款时，充值时）
		/// </summary>
		/// <param name="shopAccount"></param>
		void UpdatePlatAccount(PlatAccountInfo model);

		/// <summary>
		/// 根据时间段生成折线图表
		/// </summary>
		/// <param name="start">开始时间</param>
		/// <param name="end">结束时间</param>
		/// <param name="shopId">店铺ID</param>
		/// <returns></returns>
		LineChartDataModel<decimal> GetTradeChart(DateTime start, DateTime end, long? shopId);
		LineChartDataModel<decimal> GetTradeChartMonth(DateTime start, DateTime end, long? shopId);

		/// <summary>
		/// 获取上次结算的时间
		/// </summary>
		/// <returns></returns>
		DateTime? GetLastSettlementTime();

		/// <summary>
		/// 平台佣金合计
		/// </summary>
		/// <param name="shopId"></param>
		/// <param name="accountId"></param>
		/// <returns></returns>
		decimal GetPlatCommission(long? shopId = null, long? accountId = null);

		/// <summary>
		/// 分销佣金合计
		/// </summary>
		/// <param name="shopId"></param>
		/// <param name="accountId"></param>
		/// <returns></returns>

		decimal GetDistributorCommission(long? shopId = null, long? accountId = null);

		/// <summary>
		/// 获取结算总金额
		/// </summary>
		/// <param name="shopId"></param>
		/// <param name="accountId"></param>
		/// <returns></returns>
		decimal GetSettlementAmount(long? shopId = null, long? accountId = null);

		/// <summary>
		///获取平台帐户信息
		/// </summary>
		/// <returns></returns>
		PlatAccountInfo GetPlatAccount();

		/// <summary>
		/// 平台流水
		/// </summary>
		/// <param name="item"></param>
		void AddPlatAccountItem(PlatAccountItemInfo model);


		/// <summary>
		/// 根据店铺ID获取店铺帐户信息
		/// </summary>
		/// <param name="shopId"></param>
		/// <returns></returns>
		ShopAccountInfo GetShopAccount(long shopId);


		/// <summary>
		/// 店铺流水
		/// </summary>
		/// <param name="item"></param>
		void AddShopAccountItem(ShopAccountItemInfo model);

		/// <summary>
		/// 分页获取店铺流水
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		QueryPageModel<ShopAccountItemInfo> GetShopAccountItem(ShopAccountItemQuery query);

		/// <summary>
		/// 获取店铺流水
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		List<ShopAccountItemInfo> GetShopAccountItemNoPage(ShopAccountItemQuery query);

		/// <summary>
		/// 分页获取平台流水
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		QueryPageModel<PlatAccountItemInfo> GetPlatAccountItem(PlatAccountItemQuery query);

		/// <summary>
		/// 分页获取平台流水
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		List<PlatAccountItemInfo> GetPlatAccountItemNoPage(PlatAccountItemQuery query);


		/// <summary>
		/// 获取待结算订单详情
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		PendingSettlementOrdersInfo GetPendingSettlementOrderDetail(long orderId);


		/// <summary>
		/// 已结算订单详情
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		AccountDetailInfo GetSettlementOrderDetail(long orderId);


		/// <summary>
		/// 店铺提现申请
		/// </summary>
		/// <param name="item"></param>
		void AddShopWithDrawInfo(ShopWithDrawInfo info);

		/// <summary>
		/// 获取店铺提现详情
		/// </summary>
		/// <param name="Id"></param>
		/// <returns></returns>
		ShopWithDrawInfo GetShopWithDrawInfo(long Id);

		/// <summary>
		/// 更新提现数据
		/// </summary>
		/// <param name="model"></param>
		void UpdateShopWithDraw(ShopWithDrawInfo model);


		/// <summary>
		/// 分页获取店铺提现记录
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		QueryPageModel<ShopWithDrawInfo> GetShopWithDraw(WithdrawQuery query);

		/// <summary>
		/// 获取店铺提现记录
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		List<ShopWithDrawInfo> GetShopWithDrawNoPage(WithdrawQuery query);


		/// <summary>
		/// 添加待结算订单
		/// </summary>
		/// <param name="model"></param>
		void AddPendingSettlementOrders(PendingSettlementOrdersInfo model);

		/// <summary>
		/// 分页获取待结算订单
		/// </summary>
		/// <param name="query">待结算订单查询实体</param>
		/// <returns></returns>
		QueryPageModel<PendingSettlementOrdersInfo> GetPendingSettlementOrders(PendingSettlementOrderQuery query);

		/// <summary>
		/// 获取待结算订单
		/// </summary>
		/// <param name="query">待结算订单查询实体</param>
		/// <returns></returns>
		List<PendingSettlementOrdersInfo> GetPendingSettlementOrdersNoPage(PendingSettlementOrderQuery query);

		/// <summary>
		/// 统计待结算订单
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		QueryPageModel<StatisticsPendingSettlement> StatisticsPendingSettlementOrders(StatisticsPendingSettlementQuery query);

		/// <summary>
		/// 统计待结算订单
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		List<StatisticsPendingSettlement> StatisticsPendingSettlementOrdersNoPage(StatisticsPendingSettlementQuery query);

		/// <summary>
		/// 分页获取已结算订单
		/// </summary>
		/// <param name="query">结算订单查询实体</param>
		/// <returns></returns>
		QueryPageModel<AccountDetailInfo> GetSettlementOrders(SettlementOrderQuery query);

		/// <summary>
		/// 获取已结算订单
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		List<AccountDetailInfo> GetSettlementOrdersNoPage(SettlementOrderQuery query);

		/// <summary>
		/// 结算（把待结算订单转为结算订单）
		/// </summary>
		void Settle();



		/// <summary>
		/// 获取平台待结算佣金总和(平台待结算页面汇总)
		/// </summary>
		/// <returns></returns>
		decimal GetPendingPlatCommission();


        /// <summary>
        /// 获取店铺上次结算金额
        /// </summary>
        decimal GetLastSettlementByShopId(long shopId);

        /// <summary>
        /// 结算历史记录
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>

        QueryPageModel<AccountInfo> GetSettledHistory(ShopSettledHistoryQuery query);


        /// <summary>
        /// 获取某个店铺某个结算周期的结算总金额
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        decimal GetShopSettledAmountByAccountId(long shopId, long accountId);


        /// <summary>
        /// 处理余额私有方法
        /// </summary>
        /// <param name="shopId">店铺ID</param>
        /// <param name="money">金额</param>
        /// <param name="TradeType">类别</param>
        /// <param name="AccountNo">交易流水号</param>
        /// <param name="ChargeWay">备注</param>
        /// <param name="AccoutID">关联资金编号</param>
        void UpdateAccount(long shopId, decimal money, Himall.CommonModel.ShopAccountType TradeType, string AccountNo, string ChargeWay, long detailID = 0);



        /// <summary>
        /// 获取上一次结算的基本信息
        /// </summary>
        /// <returns></returns>
        AccountInfo GetLastAccountInfo();






        /// <summary>
        /// 获取某个店铺某个结算周期的结算总金额
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        decimal GetShopTodaySettledAmountByAccountId(long shopId, long accountId);
        /// <summary>
        /// 获取某个店铺某个结算周期的结算总金额
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        decimal GetShopTotalSettledAmountByAccountId(long shopId, long accountId);
    }
}
