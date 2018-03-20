using Himall.CommonModel;
using System;
using System.Collections.Generic;
using Himall.Model;

namespace Himall.IServices
{
    /// <summary>
    /// 订单和销售统计
    /// </summary>
    public interface IOrderAndSaleStatisticsService : IService
    {

        /// <summary>
        /// 获取指定会员最近三个月订单统计数据
        /// </summary>
        /// <param name="userId">会员编号</param>
        /// <returns></returns>
        OrderBasicStatistics GetLastThreeMonthOrderStatisticsByUser(long userId);

        #region 会员购买商品类别冗余统计

        /// <summary>
        /// 会员购买商品类别添加
        /// </summary>
        /// <param name="model"></param>
        void AddMemberBuyCategory(MemberBuyCategoryInfo model);

        /// <summary>
        /// 会员购买商品类别查询
        /// </summary>
        /// <param name="model"></param>
        MemberBuyCategoryInfo GetMemberBuyCategory(long categoryId, long userId);

        /// <summary>
        /// 会员购买商品类别删除
        /// </summary>
        /// <param name="model"></param>
        void DeleteMemberBuyCategory(long Id);

        /// <summary>
        /// 会员购买商品类别修改
        /// </summary>
        /// <param name="model"></param>
        void UpdateMemberBuyCategory(MemberBuyCategoryInfo model);
        #endregion

        /// <summary>
        /// 获取门店销售统计数据
        /// </summary>
        /// <param name="shopId">门店编号</param>
        /// <param name="startTime">统计起始时间</param>
        /// <param name="endTime">统计截止时间</param>
        /// <returns></returns>
        SaleStatistics GetSaleStatisticsByShop(long shopId, DateTime? startTime, DateTime? endTime);


        /// <summary>
        /// 获取门店销售统计数据
        /// </summary>
        /// <param name="shopId">门店编号</param>
        /// <param name="startTime">统计起始时间</param>
        /// <param name="endTime">统计截止时间</param>
        /// <returns></returns>
        SaleStatistics GetBranchShopSaleStatistics(long branchShopId, DateTime? startTime, DateTime? endTime);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<BranchShopFeat> GetBranchShopFeat(BranchShopFeatsQuery query);

        /// <summary>
        /// 获取门店每天的业绩排行
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="branchShopId"></param>
        /// <returns></returns>
        List<BranchShopDayFeat> GetDayAmountSale(DateTime start, DateTime end, long branchShopId);

        /// <summary>
        /// 获取门店在某天的业绩排行 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="shopId"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        int GetRank(DateTime date, long shopId, decimal Amount);
    }
}
