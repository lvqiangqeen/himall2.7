using System;
using System.Collections.Generic;
using System.Linq;
using Himall.IServices;
using Himall.DTO;
using Himall.CommonModel;
using Himall.Model;
using AutoMapper;
using Himall.Core;
using Himall.Core.Plugins.Payment;

namespace Himall.Application
{
    public class BillingApplication
    {
        private static IBillingService _iBillingService = ObjectContainer.Current.Resolve<IBillingService>();
        private static IOrderService _iOrderService= ObjectContainer.Current.Resolve<IOrderService>();
        private static IMarketService _iMarketService= ObjectContainer.Current.Resolve<IMarketService>();
        private static IMemberCapitalService _iMemberCapitalService= ObjectContainer.Current.Resolve<IMemberCapitalService>();
        private static ISiteSettingService _iSiteSettingService= ObjectContainer.Current.Resolve<ISiteSettingService>();
        private static IRefundService _iRefundService= ObjectContainer.Current.Resolve<IRefundService>();
        private static IShopService _iShopService= ObjectContainer.Current.Resolve<IShopService>();
        private static IOperationLogService _iOperationLogService=ObjectContainer.Current.Resolve<IOperationLogService>();
   

        /// <summary>
        /// 根据ShopID获取该店铺的财务总览信息
        /// </summary>
        /// <param name="shopId">店铺ID</param>
        /// <returns></returns>
        public static ShopAccount GetShopAccount(long shopId)
        {
            if (shopId == 0)
            {
                throw new Core.HimallException("错误的店铺ID");
            }
            var model = _iBillingService.GetShopAccount(shopId);
            Mapper.CreateMap<ShopAccountInfo, ShopAccount>();
            var shopAccount = Mapper.Map<ShopAccountInfo, ShopAccount>(model);
            return shopAccount;
        }

        /// <summary>
        /// 获取平台帐户信息
        /// </summary>
        /// <returns></returns>
        public static PlatAccount GetPlatAccount()
        {
            var model = _iBillingService.GetPlatAccount();
            Mapper.CreateMap<PlatAccountInfo, PlatAccount>();
            var platAccount = Mapper.Map<PlatAccountInfo, PlatAccount>(model);
            return platAccount;
        }


        /// <summary>
        /// 获取首页交易额图表
        /// </summary>
        public static LineChartDataModel<decimal> GetTradeChart(DateTime start, DateTime end, long? shopId)
        {
            start = start.Date;
            end = end.Date;
            var model = _iBillingService.GetTradeChart(start, end, shopId);
            return model;
        }
        /// <summary>
        /// 获取本月交易额图表
        /// </summary>
        public static LineChartDataModel<decimal> GetTradeChartMonth(DateTime start, DateTime end, long? shopId)
        {
            start = start.Date;
            end = end.Date;
            var model = _iBillingService.GetTradeChartMonth(start, end, shopId);
            return model;
        }



        /// <summary>
        /// 获取店铺财务总览
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static ShopBillingIndex GetShopBillingIndex(long shopId)
        {
            var shopaccount = GetShopAccount(shopId);
            ShopBillingIndex model = new ShopBillingIndex();
            model.YesterDayOrders = _iOrderService.GetYesterDayOrdersNum(shopId);
            model.YesterDayPayOrders = _iOrderService.GetYesterDayPayOrdersNum(shopId);
            model.YesterDaySaleAmount = _iOrderService.GetYesterDaySaleAmount(shopId);
            model.ShopAccout = shopaccount;
            return model;
        }


        /// <summary>
        /// 获取平台财务总览
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static PlatBillingIndex GetPlatBillingIndex()
        {
            var platAccount = GetPlatAccount();
            PlatBillingIndex model = new PlatBillingIndex();
            model.YesterDayOrders = _iOrderService.GetYesterDayOrdersNum();
            model.YesterDayPayOrders = _iOrderService.GetYesterDayPayOrdersNum();
            model.YesterDaySaleAmount = _iOrderService.GetYesterDaySaleAmount();
            model.PlatAccout = platAccount;
            return model;
        }

        /// <summary>
        /// 根据日期获取当前结算周期
        /// </summary>
        /// <returns></returns>
        private static SettlementCycle GetCurrentBilingTime()
        {
            var settlementCycle = _iSiteSettingService.GetSiteSettings().WeekSettlement;
            var end = _iBillingService.GetLastSettlementTime();
            return GetDateBilingTime(settlementCycle, end, DateTime.Now);
        }


        /// <summary>
        /// 根据日期获取该日期的结算周期
        /// </summary>
        /// <returns></returns>
        public static SettlementCycle GetDateBilingTime(int settlementCycle, DateTime? endDate, DateTime dt)
        {
            SettlementCycle model = new SettlementCycle();
            var end = endDate;
            if (!end.HasValue)
            {
                model.StartTime = DateTime.Now.Date;
                model.EndTime = model.StartTime.AddDays(settlementCycle);
            }
            else
            {
                var now = dt.Date;

                var days = (now - end.Value.Date).Days; //和最后结算时间相差的天数

                var d = days % settlementCycle;

                var newend = now.AddDays(settlementCycle - d);

                var newStart = newend.AddDays(-settlementCycle);

                model.StartTime = newStart;
                model.EndTime = newend;
            }
            return model;
        }

        /// <summary>
        /// 平台待结算列表上方显示的汇总信息
        /// </summary>
        /// <returns></returns>
        public static PlatSettlementCycle GetPlatSettlementCycle()
        {
            var model = GetCurrentBilingTime();
            PlatSettlementCycle info = new PlatSettlementCycle();
            info.StartTime = model.StartTime;
            info.EndTime = model.EndTime;
            info.PlatCommission = _iBillingService.GetPendingPlatCommission();
            return info;
        }

        /// <summary>
        /// 店铺待结算列表上方显示的汇总信息
        /// </summary>
        /// <returns></returns>
        public static ShopSettlementCycle GetShopSettlementCycle(long shopId)
        {
            var shopAccount = GetShopAccount(shopId);
            var model = GetCurrentBilingTime();
            ShopSettlementCycle info = new ShopSettlementCycle();
            info.StartTime = model.StartTime;
            info.EndTime = model.EndTime;
            info.PendingSettlement = shopAccount.PendingSettlement;
            return info;
        }



        /// <summary>
        ///分页获取待结算订单列表
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="shopId">店铺ID</param>
        /// <param name="StartTime">订单完成时间</param>
        /// <param name="EndTime">订单完成时间</param>
        ///<param name="pageNo">当前页</param>
        ///<param name="pageSize">每页显示记录数</param>
        /// <returns></returns>
        public static QueryPageModel<PendingSettlementOrders> GetPendingSettlementOrders(PendingSettlementOrderQuery query)
        {
            QueryPageModel<PendingSettlementOrders> orders = new QueryPageModel<PendingSettlementOrders>();
            var model = _iBillingService.GetPendingSettlementOrders(query);
            orders.Total = model.Total;
            Mapper.CreateMap<PendingSettlementOrdersInfo, PendingSettlementOrders>();
            orders.Models = Mapper.Map<List<PendingSettlementOrdersInfo>, List<PendingSettlementOrders>>(model.Models);
            var settlementCycle = _iSiteSettingService.GetSiteSettings().WeekSettlement;
            var end = _iBillingService.GetLastSettlementTime();
            var CurrentSettlementCycle = GetDateBilingTime(settlementCycle, end, DateTime.Now); //节省一次查询
            foreach (var m in orders.Models)
            {
                m.DistributorCommission = m.DistributorCommission - m.DistributorCommissionReturn;
                m.PlatCommission = m.PlatCommission - m.PlatCommissionReturn;
                if (m.OrderFinshTime < CurrentSettlementCycle.StartTime) //如果订单完成时间不在当前结算周期之内
                {
                    var cycle = GetDateBilingTime(settlementCycle, end, m.OrderFinshTime);
                    m.SettlementCycle = "此订单为" + cycle.StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "至" + cycle.EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "周期内订单";
                }
            }

            return orders;
        }

        /// <summary>
        /// 根据订单ID获取结算详情（传入shopId防止跨店铺调用）
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="shopId">店铺ID</param>
        /// <returns></returns>
        public static OrderSettlementDetail GetPendingOrderSettlementDetail(long orderId, long? shopId = null)
        {
            var model = _iBillingService.GetPendingSettlementOrderDetail(orderId);
            if (shopId.HasValue && shopId.Value != model.ShopId)
            {
                throw new Core.HimallException("找不到该店铺的结算详情");
            }

            var order = _iOrderService.GetOrder(orderId);
            var refund = _iRefundService.GetOrderRefundList(orderId);

            OrderSettlementDetail detail = new OrderSettlementDetail();
            detail.Freight = model.FreightAmount;
            detail.RefundAmount = model.RefundAmount;
            detail.DistributorCommission = model.DistributorCommission;
            detail.DistributorCommissionReturn = model.DistributorCommissionReturn;
            detail.PlatCommission = model.PlatCommission;
            detail.PlatCommissionReturn = model.PlatCommissionReturn;
            detail.ProductsTotal = model.ProductsAmount;
            detail.OrderPayTime = order.PayDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
            if (refund != null && refund.Count > 0)
            {
                detail.OrderRefundTime = refund.FirstOrDefault().ManagerConfirmDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
            return detail;
        }



        /// <summary>
        /// 根据订单ID获取结算详情（传入shopId防止跨店铺调用）
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="shopId">店铺ID</param>
        /// <returns></returns>
        public static OrderSettlementDetail GetOrderSettlementDetail(long orderId, long? shopId = null)
        {
            var model = _iBillingService.GetSettlementOrderDetail(orderId);
            if (shopId.HasValue && shopId.Value != model.ShopId)
            {
                throw new Core.HimallException("找不到该店铺的结算详情");
            }

            var order = _iOrderService.GetOrder(orderId);
            var refund = _iRefundService.GetOrderRefundList(orderId);

            OrderSettlementDetail detail = new OrderSettlementDetail();
            detail.Freight = model.FreightAmount;
            detail.RefundAmount = model.RefundTotalAmount;
            detail.DistributorCommission = model.BrokerageAmount;
            detail.DistributorCommissionReturn = model.ReturnBrokerageAmount;
            detail.PlatCommission = model.CommissionAmount;
            detail.PlatCommissionReturn = model.RefundCommisAmount;
            detail.ProductsTotal = model.ProductActualPaidAmount;
            detail.OrderPayTime = order.PayDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
            if (refund != null && refund.Count > 0)
            {
                detail.OrderRefundTime = refund.FirstOrDefault().ManagerConfirmDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
            return detail;
        }







        /// <summary>
        /// 分页获取已结算订单列表
        /// </summary>
        /// <param name="query">查询实体</param>
        /// <returns></returns>
        public static QueryPageModel<SettledOrders> GetSettlementOrders(SettlementOrderQuery query)
        {
            QueryPageModel<SettledOrders> orders = new QueryPageModel<SettledOrders>();
            var model = _iBillingService.GetSettlementOrders(query);
            orders.Total = model.Total;
            List<SettledOrders> SettledOrders = new List<SettledOrders>();
            foreach (var m in model.Models)
            {
                SettledOrders o = new SettledOrders();
                o.DistributorCommission = m.BrokerageAmount - m.ReturnBrokerageAmount;
                o.OrderAmount = m.OrderAmount;
                o.OrderFinshTime = m.OrderFinshDate.ToString("yyyy-MM-dd HH:mm:ss");
                o.OrderId = m.OrderId;
                o.PlatCommission = m.CommissionAmount - m.RefundCommisAmount;
                o.RefundAmount = m.RefundTotalAmount;
                o.SettledTime = m.Date.ToString("yyyy-MM-dd HH:mm:ss");
                o.SettlementAmount = m.SettlementAmount;
                o.ShopName = m.ShopName;
                o.ShopId = m.ShopId;
                o.PaymentTypeName = m.PaymentTypeName;
                SettledOrders.Add(o);
            }
            orders.Models = SettledOrders;
            return orders;
        }

        /// <summary>
        /// 分页获取店铺的收支明细
        /// </summary>
        /// <param name="query">查询实体</param>
        /// <returns></returns>
        public static QueryPageModel<ShopAccountItem> GetShopAccountItem(ShopAccountItemQuery query)
        {
            var model = _iBillingService.GetShopAccountItem(query);
            QueryPageModel<ShopAccountItem> item = new QueryPageModel<ShopAccountItem>();
            item.Total = model.Total;

            List<ShopAccountItem> items = new List<ShopAccountItem>();
            foreach (var m in model.Models)
            {
                ShopAccountItem shopAccountItem = new ShopAccountItem();
                shopAccountItem.AccountNo = m.AccountNo;
                shopAccountItem.ShopId = m.ShopId;
                shopAccountItem.ShopAccountType = m.TradeType;
                shopAccountItem.Balance = m.Balance.ToString();
                shopAccountItem.CreateTime = m.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
                var detailLink = string.Empty;
                shopAccountItem.DetailId = m.DetailId;
                if (m.IsIncome)
                {
                    shopAccountItem.Income = m.Amount.ToString();
                }
                else
                {
                    shopAccountItem.Expenditure = m.Amount.ToString();
                }
                shopAccountItem.Id = m.Id;
                items.Add(shopAccountItem);
            }
            item.Models = items;
            return item;
        }

        /// <summary>
        /// 分页获取平台的收支明细
        /// </summary>
        /// <param name="query">查询实体</param> 
        /// <returns></returns>
        public static QueryPageModel<PlatAccountItem> GetPlatAccountItem(PlatAccountItemQuery query)
        {
            var model = _iBillingService.GetPlatAccountItem(query);
            QueryPageModel<PlatAccountItem> item = new QueryPageModel<PlatAccountItem>();
            item.Total = model.Total;
            List<PlatAccountItem> items = new List<PlatAccountItem>();
            foreach (var m in model.Models)
            {
                PlatAccountItem PlatAccountItem = new PlatAccountItem();
                PlatAccountItem.AccountNo = m.AccountNo;
                PlatAccountItem.Balance = m.Balance.ToString();
                PlatAccountItem.CreateTime = m.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
                PlatAccountItem.DetailId = m.DetailId;
                PlatAccountItem.PlatAccountType = m.TradeType;
                if (m.IsIncome)
                {
                    PlatAccountItem.Income = m.Amount.ToString();
                }
                else
                {
                    PlatAccountItem.Expenditure = m.Amount.ToString();
                }
                PlatAccountItem.Id = m.Id;
                items.Add(PlatAccountItem);
            }
            item.Models = items;
            return item;
        }



        /// <summary>
        /// 获取营销服务费用明细（需移至营销服务BLL但目前没有此BLL）
        /// </summary>
        /// <param name="query">营销费用购买Id</param>
        /// <returns></returns>
        public static MarketServicesRecord GetMarketServiceRecord(long Id, long? shopId = null)
        {
            var model = _iMarketService.GetShopMarketServiceRecordInfo(Id);
            if (shopId.HasValue && shopId.Value != model.ActiveMarketServiceInfo.ShopId)
            {
                throw new Core.HimallException("找不到店铺的购买明细");
            }
            var record = ConvertToMarketServicesRecord(model);
            return record;
        }

        private static  MarketServicesRecord ConvertToMarketServicesRecord(MarketServiceRecordInfo info)
        {
            MarketServicesRecord record = null;
            if (info != null)
            {
                record = new MarketServicesRecord();
                record.BuyTime = info.BuyTime.ToString("yyyy-MM-dd HH:mm:ss");
                record.BuyingCycle = info.StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "至" + info.EndTime.ToString("yyyy-MM-dd HH:mm:ss");
                record.MarketType = (info.ActiveMarketServiceInfo.TypeId).ToDescription();
                record.Price = info.Price;
                record.ShopId = info.ActiveMarketServiceInfo.ShopId;
                record.ShopName = info.ActiveMarketServiceInfo.ShopName;
            }
            return record;
        }

        #region 充值
        /// <summary>
        /// 添加充值流水
        /// </summary>
        /// <param name="chargeDetail">充值实体</param>
        /// <returns></returns>
        public static long AddChargeApply(ChargeDetail chargeDetail)
        {

            Mapper.CreateMap<ChargeDetail, ChargeDetailInfo>();
            var Models = Mapper.Map<ChargeDetail, ChargeDetailInfo>(chargeDetail);
            long id = _iMemberCapitalService.AddChargeApply(Models);
            return id;
        }

        /// <summary>
        /// 店铺创建支付
        /// </summary>
        /// <param name="shopId">店铺ID</param>
        /// <param name="balance">支付金额</param>
        /// <param name="webRoot">站点URL</param>
        /// <returns></returns>
        public static ChargePayModel PaymentList(long shopId, decimal balance, string webRoot)
        {

            ChargeDetailShopInfo model = new ChargeDetailShopInfo()
            {
                ChargeAmount = balance,
                ChargeStatus = Himall.Model.ChargeDetailInfo.ChargeDetailStatus.WaitPay,
                CreateTime = DateTime.Now,
                ShopId = shopId
            };
            long orderIds = _iMemberCapitalService.AddChargeDetailShop(model);

            ChargePayModel viewmodel = new ChargePayModel();

            //获取同步返回地址
            string returnUrl = webRoot + "/SellerAdmin/Pay/CapitalChargeReturn/{0}";

            //获取异步通知地址
            string payNotify = webRoot + "/SellerAdmin/Pay/CapitalChargeNotify/{0}";

            var payments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.Biz.SupportPlatforms.Contains(PlatformType.PC));

            const string RELATEIVE_PATH = "/Plugins/Payment/";

            var models = payments.Select(item =>
            {
                string requestUrl = string.Empty;
                try
                {
                    requestUrl = item.Biz.GetRequestUrl(string.Format(returnUrl, EncodePaymentId(item.PluginInfo.PluginId)), string.Format(payNotify, EncodePaymentId(item.PluginInfo.PluginId)), orderIds.ToString(), model.ChargeAmount, "预付款充值");
                }
                catch (Exception ex)
                {
                    Core.Log.Error("支付页面加载支付插件出错", ex);
                }
                return new PaymentModel()
                {
                    Logo = RELATEIVE_PATH + item.PluginInfo.ClassFullName.Split(',')[1] + "/" + item.Biz.Logo,
                    RequestUrl = requestUrl,
                    UrlType = item.Biz.RequestUrlType,
                    Id = item.PluginInfo.PluginId
                };
            });
            models = models.Where(item => !string.IsNullOrEmpty(item.RequestUrl) && item.Id != "Himall.Plugin.Payment.WeiXinPay" && item.Id != "Himall.Plugin.Payment.WeiXinPay_Native");//只选择正常加载的插件
            viewmodel.OrderIds = orderIds.ToString();
            viewmodel.TotalAmount = model.ChargeAmount;
            viewmodel.Step = 1;
            viewmodel.UnpaidTimeout = _iSiteSettingService.GetSiteSettings().UnpaidTimeout;
            viewmodel.models = models.ToList();
            return viewmodel;
        }

        /// <summary>
        /// 对PaymentId进行加密（因为PaymentId中包含小数点"."，因此进行编码替换）
        /// </summary>
        private static string EncodePaymentId(string paymentId)
        {
            return paymentId.Replace(".", "-");
        }

        /// <summary>
        /// 店铺充值
        /// </summary>
        /// <param name="Id">充值流水订单ID</param>
        /// <param name="TradNo">支付流水号</param>
        /// <param name="ChargeWay">支付方式</param>
        public  static void ShopRecharge(long Id, string TradNo, string ChargeWay)
        {
            //处理充值流水记录
            var model = _iMemberCapitalService.GetChargeDetailShop(Id);
            if (model.ChargeStatus != ChargeDetailInfo.ChargeDetailStatus.ChargeSuccess)
            {
                model.ChargeStatus = ChargeDetailInfo.ChargeDetailStatus.ChargeSuccess;
                model.ChargeTime = DateTime.Now;
                model.ChargeWay = ChargeWay;
                _iMemberCapitalService.UpdateChargeDetailShop(model);

                //资金处理
                _iBillingService.updateAccount(model.ShopId, model.ChargeAmount, Himall.CommonModel.ShopAccountType.Recharge, TradNo, ChargeWay, Id);
            }
        }

        #endregion

        #region 提现
        private static object obj = new object();
        /// <summary>
        /// 商家申请提现
        /// </summary>
        /// <param name="draw">申请提现实体</param>
        /// <returns></returns>
        public static bool ShopApplyWithDraw(ShopWithDraw draw)
        {
            var mAccount = _iBillingService.GetShopAccount(draw.ShopId);
            if (mAccount.Balance >= draw.WithdrawalAmount)
            {
                var model = _iShopService.GetShop(draw.ShopId);
                string Account = "";
                string AccountName = "";
                if (draw.WithdrawType.Equals(WithdrawType.BankCard))
                {
                    Account = model.BankAccountNumber;
                    AccountName = model.BankAccountName;
                }
                else
                {
                    Account = model.WeiXinOpenId;
                    AccountName = model.WeiXinTrueName;
                }


                lock (obj)
                {
                    //处理余额
                    var mShopAccountInfo = _iBillingService.GetShopAccount(draw.ShopId);
                    mShopAccountInfo.Balance -= draw.WithdrawalAmount;
                    _iBillingService.UpdateShopAccount(mShopAccountInfo);


                    ShopWithDrawInfo Models = new ShopWithDrawInfo()
                    {
                        Account = Account,
                        AccountName = AccountName,
                        ApplyTime = DateTime.Now,
                        CashAmount = draw.WithdrawalAmount,
                        CashType = draw.WithdrawType,
                        SellerId = draw.SellerId,
                        SellerName = draw.SellerName,
                        ShopId = draw.ShopId,
                        ShopRemark = "",
                        Status = Himall.CommonModel.WithdrawStaus.WatingAudit,
                        ShopName = model.ShopName,
                        CashNo = DateTime.Now.ToString("yyyyMMddHHmmssffff")
                    };
                    _iBillingService.AddShopWithDrawInfo(Models);

                }


                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 店铺提现审核
        /// </summary>
        /// <param name="Id">审核ID</param>
        /// <param name="status">审核状态</param>
        /// <param name="Remark">平台备注</param>
        /// <param name="IpAddress">操作IP</param>
        /// <param name="UserName">操作人名称</param>
        /// <returns></returns>
        public static bool ShopApplyWithDraw(long Id, Himall.CommonModel.WithdrawStaus status, string Remark, string IpAddress = "", string UserName = "")
        {
            var model = _iBillingService.GetShopWithDrawInfo(Id);

            if (status == Himall.CommonModel.WithdrawStaus.Refused)//拒绝
            {
                model.Status = status;
                model.PlatRemark = Remark;
                model.DealTime = DateTime.Now;
                _iBillingService.UpdateShopWithDraw(model);

                lock (obj)
                {
                    //处理余额
                    var mShopAccountInfo = _iBillingService.GetShopAccount(model.ShopId);
                    mShopAccountInfo.Balance += model.CashAmount;
                    _iBillingService.UpdateShopAccount(mShopAccountInfo);
                }


                //操作日志
                _iOperationLogService.AddPlatformOperationLog(
          new LogInfo
          {
              Date = DateTime.Now,
              Description = string.Format("店铺提现拒绝，店铺Id={0},状态为：{1}, 说明是：{2}", model.ShopId,
              status, Remark),
              IPAddress = IpAddress,
              PageUrl = "/Admin/ShopWithDraw/Management",
              UserName = UserName,
              ShopId = 0

          });
                return true;
            }
            else if (model.CashType == Himall.CommonModel.WithdrawType.BankCard)//银行卡
            {
                model.Status = status;
                model.PlatRemark = Remark;
                model.DealTime = DateTime.Now;
                _iBillingService.UpdateShopWithDraw(model);
                //资金处理
                updateAccount(model.ShopId, model.CashAmount, Himall.CommonModel.ShopAccountType.WithDraw, DateTime.Now.ToString("yyyyMMddHHmmssffff"), "银行卡提现", Id);


                //操作日志
                _iOperationLogService.AddPlatformOperationLog(
          new LogInfo
          {
              Date = DateTime.Now,
              Description = string.Format("店铺银行卡提现审核成功，店铺Id={0},状态为：{1}, 说明是：{2}", model.ShopId,
              status, Remark),
              IPAddress = IpAddress,
              PageUrl = "/Admin/ShopWithDraw/Management",
              UserName = UserName,
              ShopId = 0

          });
                return true;
            }
            else
            {
                var plugins = PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(e => e.PluginInfo.PluginId.ToLower().Contains("weixin")).FirstOrDefault();
                if (plugins != null)
                {
                    try
                    {
                        var shopModel = _iShopService.GetShop(model.ShopId);
                        EnterprisePayPara para = new EnterprisePayPara()
                        {
                            amount = model.CashAmount,
                            check_name = false,
                            openid = shopModel.WeiXinOpenId,
                            out_trade_no = model.CashNo.ToString(),
                            desc = "提现"
                        };
                        PaymentInfo result = plugins.Biz.EnterprisePay(para);

                        model.SerialNo = result.TradNo;
                        model.DealTime = DateTime.Now;
                        model.Status = WithdrawStaus.Succeed;
                        model.PlatRemark = Remark;
                        _iBillingService.UpdateShopWithDraw(model);
                        //资金处理
                        updateAccount(model.ShopId, model.CashAmount, Himall.CommonModel.ShopAccountType.WithDraw, result.TradNo, "微信提现", Id);

                        //操作日志
                        _iOperationLogService.AddPlatformOperationLog(
                  new LogInfo
                  {
                      Date = DateTime.Now,
                      Description = string.Format("店铺微信提现审核成功，店铺Id={0},状态为：{1}, 说明是：{2}", model.ShopId,
                      status, Remark),
                      IPAddress = IpAddress,
                      PageUrl = "/Admin/ShopWithDraw/Management",
                      UserName = UserName,
                      ShopId = 0

                  });

                        return true;
                    }
                    catch (Exception ex)
                    {
                        Log.Error("调用企业付款接口异常：" + ex.Message);
                        model.Status = WithdrawStaus.Fail;
                        model.PlatRemark = Remark;
                        model.DealTime = DateTime.Now;
                        _iBillingService.UpdateShopWithDraw(model);

                        //操作日志
                        _iOperationLogService.AddPlatformOperationLog(
                  new LogInfo
                  {
                      Date = DateTime.Now,
                      Description = string.Format("店铺微信提现审核失败，店铺Id={0},状态为：{1}, 说明是：{2}", model.ShopId,
                      status, Remark),
                      IPAddress = IpAddress,
                      PageUrl = "/Admin/ShopWithDraw/Management",
                      UserName = UserName,
                      ShopId = 0

                  });
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

        }

        /// <summary>
        /// 资金记录添加
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="money"></param>
        /// <param name="TradeType"></param>
        /// <param name="AccountNo"></param>
        /// <param name="ChargeWay"></param>
        /// <param name="detailID"></param>
        private static void updateAccount(long shopId, decimal money, Himall.CommonModel.ShopAccountType TradeType, string AccountNo, string ChargeWay, long detailID = 0)
        {
            lock (obj)
            {
                //处理余额
                var mShopAccountInfo = _iBillingService.GetShopAccount(shopId);
                //处理充值记录
                ShopAccountItemInfo mShopAccountItemInfo = new ShopAccountItemInfo()
                {
                    AccountNo = AccountNo,
                    AccoutID = mShopAccountInfo.Id,
                    Amount = money,
                    Balance = mShopAccountInfo.Balance,
                    CreateTime = DateTime.Now,
                    DetailId = detailID.ToString(),
                    IsIncome = true,
                    ReMark = ChargeWay,
                    ShopId = shopId,
                    ShopName = mShopAccountInfo.ShopName,
                    TradeType = TradeType
                };
                _iBillingService.AddShopAccountItem(mShopAccountItemInfo);
            }
        }

        /// <summary>
        /// 分页查询店铺提现记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ShopWithDrawItem> GetShopWithDraw(WithdrawQuery query)
        {
            var model = _iBillingService.GetShopWithDraw(query);

            QueryPageModel<ShopWithDrawItem> Models = new QueryPageModel<ShopWithDrawItem>();

            List<ShopWithDrawItem> items = new List<ShopWithDrawItem>();
            foreach (ShopWithDrawInfo mInfo in model.Models)
            {
                ShopWithDrawItem swdi = new ShopWithDrawItem();

                swdi.Id = mInfo.Id;
                swdi.Account = mInfo.Account;
                swdi.AccountName = mInfo.AccountName;
                swdi.AccountNo = long.Parse(mInfo.CashNo);
                swdi.ApplyTime = mInfo.ApplyTime.ToString("yyyy-MM-dd HH:mm:ss");
                swdi.CashAmount = mInfo.CashAmount.ToString("f2");
                swdi.WithStatus = (int)mInfo.Status;
                swdi.CashType = mInfo.CashType.ToDescription();
                swdi.DealTime = mInfo.DealTime == null ? "" : mInfo.DealTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                swdi.PlatRemark = mInfo.PlatRemark;
                swdi.SellerId = mInfo.SellerId;
                swdi.SellerName = mInfo.SellerName;
                swdi.ShopId = mInfo.ShopId;
                swdi.ShopName = mInfo.ShopName;
                swdi.ShopRemark = mInfo.ShopRemark;
                swdi.Status = mInfo.Status.ToDescription();

                items.Add(swdi);
            }
            Models.Models = items;
            Models.Total = model.Total;
            return Models;
        }
        #endregion

        /// <summary>
        /// 店铺结算统计信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static SettlementStatistics GetShopSettlementStatistics(long shopId)
        {
            var SettlementAmount = _iBillingService.GetSettlementAmount(shopId);
            var PlatCommission = _iBillingService.GetPlatCommission(shopId);
            var DistributorCommission = _iBillingService.GetDistributorCommission(shopId);
            return new SettlementStatistics()
            {
                DistributorCommission = DistributorCommission,
                PlatCommission = PlatCommission,
                SettlementAmount = SettlementAmount
            };
        }
        /// <summary>
        /// 平台结算统计信息
        /// </summary>
        /// <returns></returns>

        public static PlatSettlementStatistics GetPlatSettlementStatistics()
        {
            var SettlementAmount = _iBillingService.GetSettlementAmount();
            var PlatCommission = _iBillingService.GetPlatCommission();
            var DistributorCommission = _iBillingService.GetDistributorCommission();

            return new PlatSettlementStatistics()
            {
                DistributorCommission = DistributorCommission,
                PlatCommission = PlatCommission,
                SettlementAmount = SettlementAmount
            };
        }
    }
}
