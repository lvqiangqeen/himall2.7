using System;
using System.Collections;
using System.Web.Http;
using Himall.CommonModel;
using Himall.Application;
using Himall.API.Model;

namespace Himall.API
{
    public class ShopBilityController : BaseShopApiController
    {
        /// <summary>
        /// 获取店铺业绩能力数据
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <param name="shopId">门店id</param>
        /// <returns></returns>
        public SaleStatistics Get(DateTime? startDate = null, DateTime? endDate = null)
        {

            CheckShopManageLogin();
            var shop = CurrentShop;
            var ShopId = shop.Id;
            if (!startDate.HasValue)
            {
                startDate = DateTime.Now.Date;
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.Now;
            }
            else
                endDate = endDate.Value.AddDays(1).AddMilliseconds(-1);
            var Sale = OrderAndSaleStatisticsApplication.GetShopSaleStatistics(ShopId, startDate.Value, endDate.Value);
            return Sale;
        }

        /// <summary>
        /// 获取店铺下门店的业绩排行
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public DataGridModel<BranchShopFeat> GetBranchShopFeat(DateTime? startDate = null, DateTime? endDate = null,int pageNo = 1, int pageSize = 10)
        {
            if (!startDate.HasValue)
            {
                startDate = DateTime.Now.Date;
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.Now;
            }
            else
                endDate = endDate.Value.AddDays(1).AddMilliseconds(-1);
            CheckShopManageLogin();
            var shop = CurrentShop;
            long shopId = shop.Id;
            BranchShopFeatsQuery query = new BranchShopFeatsQuery();
            query.EndDate = endDate;
            query.StartDate = startDate;
            query.ShopId = shopId;
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            var model = OrderAndSaleStatisticsApplication.GetBranchShopFeat(query);
            DataGridModel<BranchShopFeat> result = new DataGridModel<BranchShopFeat>()
            {
                rows = model.Models,
                total = model.Total
            };
            return result;
        }
    }
}
