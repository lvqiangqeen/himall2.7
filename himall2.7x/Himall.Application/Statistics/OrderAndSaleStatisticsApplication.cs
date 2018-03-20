using Himall.CommonModel;
using Himall.Core;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Application
{
    public class OrderAndSaleStatisticsApplication
    {

        private static IOrderAndSaleStatisticsService _iOrderAndSaleStatisticsService = ObjectContainer.Current.Resolve<IOrderAndSaleStatisticsService>();


        /// <summary>
        /// c最近三个月订单统计数据
        /// </summary>
        /// <param name="userId">会员编号</param>
        /// <returns></returns>
        public static OrderBasicStatistics GetLastThreeMonthOrderStatisticsByUser(long userId)
        {
            return _iOrderAndSaleStatisticsService.GetLastThreeMonthOrderStatisticsByUser(userId);
        }


        /// <summary>
        /// 获得店铺销售汇总
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static SaleStatistics GetShopSaleStatistics(long shopId, DateTime? start, DateTime? end)
        {
            return _iOrderAndSaleStatisticsService.GetSaleStatisticsByShop(shopId, start, end);
        }

        /// <summary>
        /// 获得店铺下门店销售汇总
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static SaleStatistics GetBranchShopSaleStatistics(long branchShopId, DateTime? start, DateTime? end)
        {
            return _iOrderAndSaleStatisticsService.GetBranchShopSaleStatistics(branchShopId, start, end);
        }

        /// <summary>
        /// 店铺下的所有门店销售排行
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<BranchShopFeat> GetBranchShopFeat(BranchShopFeatsQuery query)
        {
            var model = _iOrderAndSaleStatisticsService.GetBranchShopFeat(query);

            var ids = model.Models.Select(a => a.BranchShopId).ToList();

            if (ids != null && ids.Count() > 0)
            {
                var branchShop = ShopBranchApplication.GetShopBranchByIds(ids);
                foreach (var m in model.Models)
                {
                    var branch = branchShop.Where(a => a.Id == m.BranchShopId).FirstOrDefault();
                    if (branch != null)
                    {
                        m.BranchShopName = branch.ShopBranchName;
                    }
                }
            }
            return model;
        }
        /// <summary>
        /// 店铺下某个门店每天的销售排行及销量
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<BranchShopDayFeat> GetDayAmountSale(BranchShopDayFeatsQuery query)
        {
            var list = _iOrderAndSaleStatisticsService.GetDayAmountSale(query.StartDate, query.EndDate, query.BranchShopId);
            var pageList = list.Skip((query.PageNo - 1) * query.PageSize).Take(query.PageSize).ToList();
            foreach (var m in pageList)
            {
                m.Rank = _iOrderAndSaleStatisticsService.GetRank(m.Day, query.ShopId, m.SaleAmount);
            }
            QueryPageModel<BranchShopDayFeat> result = new QueryPageModel<BranchShopDayFeat>();
            result.Total = list.Count();
            result.Models = pageList;
            return result;
        }


        /// <summary>
        /// 维护会员购买商品类别
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="userId"></param>
        public static void SynchronizeMemberBuyCategory(long categoryId, long userId)
        {
            //如果有记录删除，重新添加
            var model = _iOrderAndSaleStatisticsService.GetMemberBuyCategory(categoryId, userId);
            if (model != null)
            {
                model.OrdersCount += 1;
                _iOrderAndSaleStatisticsService.UpdateMemberBuyCategory(model);
            }
            else
                _iOrderAndSaleStatisticsService.AddMemberBuyCategory(new MemberBuyCategoryInfo() { CategoryId = categoryId, UserId = userId, OrdersCount = 1 });
        }
    }
}
