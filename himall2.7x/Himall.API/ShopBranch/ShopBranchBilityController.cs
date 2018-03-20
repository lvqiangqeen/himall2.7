using System;
using System.Collections;
using System.Web.Http;
using Himall.CommonModel;
using Himall.Application;
using Himall.API.Model;
using Himall.API;

namespace Himall.API
{
    public class ShopBranchBilityController : BaseShopBranchApiController
    {
        /// <summary>
        /// 获取门店业绩能力数据
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <param name="shopId">门店id</param>
        /// <returns></returns>
        public SaleStatistics Get(DateTime? startDate = null, DateTime? endDate = null)
        {
            CheckUserLogin();
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
            var Sale = OrderAndSaleStatisticsApplication.GetBranchShopSaleStatistics(CurrentShopBranch.Id, startDate.Value, endDate.Value);
            return Sale;
        }

        /// <summary>
        /// 获取店铺下某个门店的每天的业绩排行
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public DataGridModel<BranchShopDayFeat> GetBranchShopFeat(DateTime? startDate = null, DateTime? endDate = null, int pageNo =1,int pageSize=10)
        {
            if (!startDate.HasValue)
            {
                startDate = DateTime.Now.Date.AddDays(-7);
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.Now.Date;
            }
            CheckUserLogin();
            if (startDate.HasValue && startDate.Value < CurrentShopBranch.CreateDate)
            {
                startDate = CurrentShopBranch.CreateDate.Date;
            }
            BranchShopDayFeatsQuery query = new BranchShopDayFeatsQuery();
            query.StartDate = startDate.Value;
            query.EndDate = endDate.Value;
            query.ShopId = CurrentShopBranch.ShopId;
            query.BranchShopId = CurrentShopBranch.Id;
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            var model = Himall.Application.OrderAndSaleStatisticsApplication.GetDayAmountSale(query);
            DataGridModel<BranchShopDayFeat> result = new DataGridModel<BranchShopDayFeat>()
            {
                rows = model.Models,
                total = model.Total
            };
            return result;
        }
    }
}
