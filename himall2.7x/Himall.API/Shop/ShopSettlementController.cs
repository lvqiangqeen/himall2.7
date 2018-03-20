using System;
using Himall.Application;
using Himall.API.Model;
using Himall.CommonModel;
using System.Collections.Generic;
using Himall.DTO;

namespace Himall.API
{
    public class ShopSettlementController : BaseShopApiController
    {
        /// <summary>
        /// 获取店铺账户
        /// </summary>
        /// <returns></returns>
        public ShopAccountModel GetShopAccount()
        {
            CheckShopManageLogin();
            var shop = CurrentShop;
            long shopId = shop.Id;
            var shopAccount = BillingApplication.GetShopAccount(shopId);
            ShopAccountModel model = new ShopAccountModel()
            {
                PeriodSettlement = shopAccount.PendingSettlement,
                Settlement = shopAccount.Settled,
                ShopName = shopAccount.ShopName,
                Balance = shopAccount.Balance,
                LastSettlement = BillingApplication.GetLastSettlementByShopId(shopId),
                LastSettlementModel = BillingApplication.GetLastSettlementInfo(),
                IsShowWithDraw = CurrentUser.IsMainAccount
            };
            return model;
        }

        /// <summary>
        /// 获取当前结算周期
        /// </summary>
        /// <returns></returns>
       public  SettlementCycle GetCurrentSettlementCycle()
        {
            return BillingApplication.GetCurrentBilingTime();
        }




        /// <summary>
        /// 分页获取门店待结算订单
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public DataGridModel<DTO.PendingSettlementOrders> GetPendingSettlementOrders(int page = 1, int pagesize = 10, DateTime? startDate = null, DateTime? endDate = null, long? orderId = null)
        {
            CheckShopManageLogin();
            var shop = CurrentShop;
            long shopId = shop.Id;

            var query = new PendingSettlementOrderQuery()
            {
                OrderStart = startDate,
                OrderEnd = endDate,
                PageNo = page,
                PageSize = pagesize,
                OrderId = orderId,
                ShopId = shopId,
            };
            var model = BillingApplication.GetPendingSettlementOrders(query);
            DataGridModel<DTO.PendingSettlementOrders> result = new DataGridModel<DTO.PendingSettlementOrders>()
            {
                rows = model.Models,
                total = model.Total
            };
            return result;
        }

        /// <summary>
        /// 获取待结算详情
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public  OrderSettlementDetail GetPendingOrderSettlementDetail(long orderId)
        {
            CheckShopManageLogin();
            var shop = CurrentShop;
            long shopId = shop.Id;
            var model = BillingApplication.GetPendingOrderSettlementDetail(orderId, shopId);
            return model;
        }
        /// <summary>
        /// 获取已结算详情
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public OrderSettlementDetail GetOrderSettlementDetail(long orderId)
        {
            CheckShopManageLogin();
            var shop = CurrentShop;
            long shopId = shop.Id;
            var model = BillingApplication.GetOrderSettlementDetail(orderId, shopId);
            return model;
        }

        /// <summary>
        /// 分页获取门店已结算订单
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public DataGridModel<DTO.SettledOrders> GetSettlementOrders(int page = 1, int pagesize = 10,
            DateTime? startDate = null, DateTime? endDate = null, long? orderId = null, long?WeekSettlementId=null)
        {
            CheckShopManageLogin();
            var shop = CurrentShop;
            long shopId = shop.Id;

            var query = new SettlementOrderQuery()
            {
                OrderStart = startDate,
                OrderEnd = endDate,
                PageNo = page,
                PageSize = pagesize,
                OrderId = orderId,
                ShopId = shopId,
                WeekSettlementId=WeekSettlementId
            };
            var model = BillingApplication.GetSettlementOrders(query);
            DataGridModel<DTO.SettledOrders> result = new DataGridModel<DTO.SettledOrders>()
            {
                rows = model.Models,
                total = model.Total
            };
            return result;
        }

        public DataGridModel<ShopSettledHistory> GetShopYearSettledHistory(int page = 1, int pagesize = 10)
        {
            CheckShopManageLogin();
            var shop = CurrentShop;
            long shopId = shop.Id;
            var query = new ShopSettledHistoryQuery() { ShopId = shopId, PageNo=page,PageSize=pagesize  };
            var model = BillingApplication.GetShopYearSettledHistory(query);
            DataGridModel<ShopSettledHistory> result = new DataGridModel<ShopSettledHistory>()
            {
                rows = model.Models,
                total = model.Total
            };
            return result;

        }

        /// <summary>
        /// 店铺结算明细
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>

        public DataGridModel<ShopAccountItem> GetShopAccountItem(int page = 1, int pagesize = 10,bool? isIncome=null)
        {
            CheckShopManageLogin();
            var shop = CurrentShop;
            long shopId = shop.Id;
            var query = new ShopAccountItemQuery() { PageNo=page,PageSize=pagesize, ShopId=shopId,IsIncome=isIncome};
            var model = BillingApplication.GetShopAccountItem(query);
            DataGridModel<ShopAccountItem> result = new DataGridModel<ShopAccountItem>()
            {
                rows = model.Models,
                total = model.Total
            };
            return result;
        }
    }
}
