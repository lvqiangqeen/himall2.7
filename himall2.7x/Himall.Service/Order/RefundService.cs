using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.Core.Plugins.Payment;
using Himall.Entity;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Himall.CommonModel;
using Himall.CommonModel.Delegates;
using Himall.CommonModel.Enum;

namespace Himall.Service
{
    public class RefundService : ServiceBase, IRefundService
    {
        #region 属性
        /// <summary>
        /// 退款成功
        /// </summary>
        public event RefundSuccessed OnRefundSuccessed;
        #endregion

        #region 方法
        public QueryPageModel<OrderRefundInfo> GetOrderRefunds(RefundQuery refundQuery)
        {
            var context = this.Context;
            var refunds = context.OrderRefundInfo.Include("OrderItemInfo").AsQueryable();

            #region 条件组合
            if (refundQuery.StartDate.HasValue)
            {
                refunds = refunds.Where(item => item.ApplyDate >= refundQuery.StartDate);
            }
            //if (refundQuery.OrderId.HasValue)
            //{
            //    refunds = refunds.Where(item => item.OrderId == refundQuery.OrderId);
            //}
            if (refundQuery.EndDate.HasValue)
            {
                var enddate = refundQuery.EndDate.Value.Date.AddDays(1);
                refunds = refunds.Where(item => item.ApplyDate < enddate);
            }
            if (refundQuery.ConfirmStatus.HasValue)
            {
                refunds = refunds.Where(item => refundQuery.ConfirmStatus == item.ManagerConfirmStatus);
            }
            if (refundQuery.ShopId.HasValue)
            {
                refunds = refunds.Where(item => refundQuery.ShopId == item.ShopId);
            }
            if (refundQuery.UserId.HasValue)
            {
                refunds = refunds.Where(item => item.UserId == refundQuery.UserId);
            }
            if (!string.IsNullOrWhiteSpace(refundQuery.ProductName))
            {
                refunds = refunds.Where(item => item.OrderItemInfo.ProductName.Contains(refundQuery.ProductName));
            }
            if (!string.IsNullOrWhiteSpace(refundQuery.ShopName))
            {
                refunds = refunds.Where(item => item.ShopName.Contains(refundQuery.ShopName));
            }
            if (!string.IsNullOrWhiteSpace(refundQuery.UserName))
            {
                refunds = refunds.Where(item => item.Applicant.Contains(refundQuery.UserName));
            }
            //多订单结果集查询
            if (refundQuery.MoreOrderId == null)
                refundQuery.MoreOrderId = new List<long>();
            if (refundQuery.OrderId.HasValue)
            {
                refundQuery.MoreOrderId.Add((long)refundQuery.OrderId);
                refundQuery.MoreOrderId = refundQuery.MoreOrderId.Distinct().ToList();
                refunds = refunds.FindBy(d => refundQuery.MoreOrderId.Contains(d.OrderId));
            }
            if (refundQuery.ShowRefundType.HasValue)
            {
                switch (refundQuery.ShowRefundType)
                {
                    case 1:
                        refunds = refunds.FindBy(d => d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund);
                        break;
                    case 2:
                        refunds = refunds.FindBy(d => d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderItemRefund || d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund);
                        break;
                    case 3:
                        refunds = refunds.FindBy(d => d.RefundMode == OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund);
                        break;
                    case 4:
                        refunds = refunds.FindBy(d => d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderItemRefund);
                        break;
                }
            }

            if (refundQuery.ConfirmStatus.HasValue)
                refunds = refunds.Where(item => item.ManagerConfirmStatus == refundQuery.ConfirmStatus.Value && item.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited);
            if (refundQuery.AuditStatus.HasValue)
            {
                if (refundQuery.AuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit)
                    refunds = refunds.Where(
                        item => item.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit
                            || item.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving);
                else
                    refunds = refunds.Where(item => item.SellerAuditStatus == refundQuery.AuditStatus);
            }

            if (refundQuery.ShopBranchId.HasValue)
            {
                if (refundQuery.ShopBranchId.Value > 0)
                {
                    var sbId = refundQuery.ShopBranchId.Value;
                    refunds = refunds.Join(context.OrderInfo.Where(p => p.ShopBranchId == sbId), refund => refund.OrderId, order => order.Id, (refund, order) => refund);
                }
                else
                {
                    refunds = refunds.Join(context.OrderInfo.Where(p => (p.ShopBranchId.HasValue && p.ShopBranchId == 0) || (!p.ShopBranchId.HasValue)), refund => refund.OrderId, order => order.Id, (refund, order) => refund);
                }
            }
            #endregion

            int total;
            refunds = refunds.GetPage(out total, refundQuery.PageNo, refundQuery.PageSize);
            var result = refunds.ToList();
            List<long> ordidlst = result.Select(r => r.OrderId).ToList();
            var ordlist = context.OrderInfo.Where(d => ordidlst.Contains(d.Id)).ToList();
            var ordser = ServiceProvider.Instance<IOrderService>.Create;
            foreach (var item in result)
            {
                if (item.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    var order = ordlist.FirstOrDefault(d => d.Id == item.OrderId);
                    if (order != null)
                    {
                        item.EnabledRefundAmount = order.ProductTotalAmount + order.Freight - order.DiscountAmount;
                    }
                }
                else
                {
                    item.EnabledRefundAmount = (item.OrderItemInfo.EnabledRefundAmount == null ? 0 : item.OrderItemInfo.EnabledRefundAmount.Value);
                }
                //处理订单售后期
                item.IsOrderRefundTimeOut = ordser.IsRefundTimeOut(item.OrderId);
                //ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetTypeByProductId(item.OrderItemInfo.ProductId);
                ProductTypeInfo typeInfo = (ProductTypeInfo)Context.ProductTypeInfo.Join(Context.ProductInfo.Where(d => d.Id == item.OrderItemInfo.ProductId), x => x.Id, y => y.TypeId, (x, y) => x).ToList().FirstOrDefault();
                item.OrderItemInfo.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                item.OrderItemInfo.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                item.OrderItemInfo.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            }
            var pageModel = new QueryPageModel<OrderRefundInfo>() { Models = result.ToList(), Total = total };
            return pageModel;
        }

        #region 退款方式方法体
        private object lockobj = new object();
        /// <summary>
        /// 生成一个新的退款批次号
        /// </summary>
        /// <returns></returns>
        private string GetNewRefundBatchNo()
        {
            string result = "";
            lock (lockobj)
            {
                int rand;
                char code;
                result = string.Empty;
                Random random = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
                for (int i = 0; i < 5; i++)
                {
                    rand = random.Next();
                    code = (char)('0' + (char)(rand % 10));
                    result += code.ToString();
                }
                result = DateTime.Now.ToString("yyyyMMddfff") + result;
            }
            return result;
        }

        /// <summary>
        /// 对PaymentId进行加密（因为PaymentId中包含小数点"."，因此进行编码替换）
        /// </summary>
        string EncodePaymentId(string paymentId)
        {
            return paymentId.Replace(".", "-");
        }
        /// <summary>
        /// 原路退回
        /// </summary>
        /// <param name="refund"></param>
        /// <returns>异步请求的地址，如果同步请返回空</returns>
        private string RefundBackOut(OrderRefundInfo refund, string notifyurl)
        {
            string result = "";
            if (refund.RefundPayStatus != OrderRefundInfo.OrderRefundPayStatus.PaySuccess)
            {
                var payWay = refund.OrderItemInfo.OrderInfo.PaymentTypeGateway;

                var paymentPlugins = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.PluginInfo.PluginId == payWay);
                if (paymentPlugins.Count() > 0)
                {
                    var OrderPayInfo = Context.OrderPayInfo.FirstOrDefault(e => e.PayState && e.OrderId == refund.OrderId);
                    if (OrderPayInfo != null)
                    {
                        var orderIds = Context.OrderPayInfo.Where(item => item.PayId == OrderPayInfo.PayId && item.PayState).Select(e => e.OrderId).ToList();
                        var amount = Context.OrderInfo.Where(o => orderIds.Contains(o.Id)).ToList().Sum(e => e.OrderTotalAmount);
                        var firstOrderid = orderIds.FirstOrDefault();
                        var order = Context.OrderInfo.FirstOrDefault(d => d.Id == firstOrderid);
                        string paytradeno = string.Empty;
                        if (order != null)
                        {
                            paytradeno = order.GatewayOrderId;
                        }
                        if (string.IsNullOrEmpty(paytradeno))
                        {
                            throw new HimallException("未找到支付流水号！");
                        }
                        string refund_batch_no = GetNewRefundBatchNo();
                        //退款流水号处理
                        if (!refund.RefundPostTime.HasValue)
                        {
                            refund.RefundPostTime = DateTime.Now.AddDays(-2);
                        }
                        //支付宝一天内可共用同一个流水号
                        if (refund.RefundPostTime.Value.Date == DateTime.Now.Date && !string.IsNullOrWhiteSpace(refund.RefundBatchNo))
                        {
                            refund_batch_no = refund.RefundBatchNo;
                        }
                        else
                        {
                            refund.RefundBatchNo = refund_batch_no;
                        }
                        refund.RefundPostTime = DateTime.Now;

                        notifyurl = string.Format(notifyurl, EncodePaymentId(payWay));
                        decimal refundfee = refund.Amount;
                        var orderinfo = refund.OrderItemInfo.OrderInfo;
                        if (refund.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund)
                        {
                            if (refund.OrderItemInfo.EnabledRefundIntegral > 0 && refund.OrderItemInfo.EnabledRefundAmount > 0)
                            {
                                if (refundfee > (refund.OrderItemInfo.EnabledRefundAmount - refund.OrderItemInfo.EnabledRefundIntegral))
                                {
                                    refundfee = refund.OrderItemInfo.EnabledRefundAmount.Value - refund.OrderItemInfo.EnabledRefundIntegral.Value;
                                }
                            }
                        }
                        else
                        {
                            refundfee = orderinfo.OrderTotalAmount;
                        }

                        PaymentPara para = new PaymentPara()
                        {
                            out_refund_no = refund_batch_no,
                            out_trade_no = OrderPayInfo.PayId.ToString(),
                            pay_trade_no = paytradeno,
                            refund_fee = refundfee,
                            total_fee = amount,
                            notify_url = notifyurl
                        };

                        var refundResult = paymentPlugins.FirstOrDefault().Biz.ProcessRefundFee(para);
                        if (refundResult.RefundResult == RefundState.Success)
                        {
                            if (refundResult.RefundMode == RefundRunMode.Sync)
                            {
                                refund.RefundPayStatus = OrderRefundInfo.OrderRefundPayStatus.PaySuccess;
                            }
                            if (refundResult.RefundMode == RefundRunMode.Async)
                            {
                                result = refundResult.ResponseContentWhenFinished;
                                refund.RefundBatchNo = refundResult.RefundNo;
                                refund.RefundPayStatus = OrderRefundInfo.OrderRefundPayStatus.Payed;
                            }
                            Context.SaveChanges();
                        }
                        else
                        {
                            throw new HimallException("退款插件工作未完成！");
                        }
                    }
                    else
                    {
                        throw new HimallException("退款时，未找到原支付订单信息！");
                    }
                }
                else
                {
                    throw new HimallException("退款时，未找到支付方式！");
                }
            }
            return result;
        }
        /// <summary>
        /// 退到预付款
        /// </summary>
        /// <param name="refund"></param>
        private void RefundBackCapital(OrderRefundInfo refund)
        {
            IMemberCapitalService capitalServicer = Himall.ServiceProvider.Instance<IMemberCapitalService>.Create;
            if (refund.RefundPayType == OrderRefundInfo.OrderRefundPayType.BackCapital)
            {
                if (!refund.RefundPayStatus.HasValue || (refund.RefundPayStatus.HasValue && refund.RefundPayStatus.Value != OrderRefundInfo.OrderRefundPayStatus.PaySuccess))
                {
                    decimal refundfee = refund.Amount;
                    var orderinfo = refund.OrderItemInfo.OrderInfo;
                    if (refund.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund)
                    {
                        if (refund.OrderItemInfo.EnabledRefundIntegral > 0 && refund.OrderItemInfo.EnabledRefundAmount > 0)
                        {
                            if (refundfee > (refund.OrderItemInfo.EnabledRefundAmount - refund.OrderItemInfo.EnabledRefundIntegral))
                            {
                                refundfee = refund.OrderItemInfo.EnabledRefundAmount.Value - refund.OrderItemInfo.EnabledRefundIntegral.Value;
                            }
                        }
                    }
                    else
                    {
                        refundfee = orderinfo.OrderTotalAmount;
                    }
                    CapitalDetailModel capita = new CapitalDetailModel
                    {
                        UserId = refund.UserId,
                        Amount = refundfee,
                        SourceType = CapitalDetailInfo.CapitalDetailType.Refund,
                        SourceData = refund.Id.ToString(),
                        CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    capitalServicer.AddCapital(capita);
                    refund.RefundPayStatus = OrderRefundInfo.OrderRefundPayStatus.PaySuccess;
                    Context.SaveChanges();
                }
            }
        }
        #endregion

        #region 退款成功处理
        /// <summary>
        /// 退款成功后的处理
        /// </summary>
        /// <param name="refund"></param>
        private void RefundSuccessed(OrderRefundInfo refund, string managerName)
        {
            OrderInfo order = Context.OrderInfo.FindById(refund.OrderId);
            UserMemberInfo member = Context.UserMemberInfo.FindById(refund.UserId);
            if (refund.ManagerConfirmStatus != OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm)
                throw new HimallException("只有未确认状态的退款/退货才能进行确认操作！");

            if (refund.RefundPayStatus == OrderRefundInfo.OrderRefundPayStatus.PaySuccess)
            {
                if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    var orditemlist = Context.OrderItemInfo.Where(d => d.OrderId == refund.OrderId).ToList();
                    foreach (var i in orditemlist)
                    {
                        i.ReturnQuantity = i.Quantity;
                        if (i.EnabledRefundAmount == null)
                            i.EnabledRefundAmount = 0;
                        i.RefundPrice = i.EnabledRefundAmount.Value;
                    }
                }
                else
                {
                    var orderItem = Context.OrderItemInfo.FirstOrDefault(p => p.Id == refund.OrderItemId);
                    orderItem.RefundPrice = refund.Amount;
                    orderItem.ReturnQuantity = refund.ShowReturnQuantity;
                }

                //实付(金额不含运费)
                decimal orderRealPay = (order.OrderTotalAmount - order.Freight);
                //可退(金额)
                decimal orderCanRealRefund = (order.OrderTotalAmount - order.Freight);
                //实退(金额不含积分)
                decimal amountPay = (orderRealPay == 0 ? 1 : orderRealPay);//计算实际退款金额使用
                if (orderRealPay < refund.Amount)
                {
                    amountPay = refund.Amount;
                }
                decimal realRefundAmount = (refund.Amount - order.IntegralDiscount * (refund.Amount / amountPay));
                realRefundAmount = Math.Round(realRefundAmount, 2);
                if (realRefundAmount > 0)
                {
                    //修改实收金额
                    order.ActualPayAmount -= realRefundAmount;
                    order.RefundTotalAmount += realRefundAmount;
                    if (order.RefundTotalAmount > orderRealPay)
                    {
                        order.RefundTotalAmount = orderRealPay;
                    }
                }
                //修正整笔退
                if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    orderRealPay = order.OrderTotalAmount;
                    orderCanRealRefund = order.OrderTotalAmount;
                    realRefundAmount = order.OrderTotalAmount;
                    order.RefundTotalAmount = realRefundAmount;
                }

                var integralExchange = ServiceProvider.Instance<IMemberIntegralService>.Create.GetIntegralChangeRule();

                #region 扣除订单产生的积分
                if (integralExchange != null && integralExchange.MoneyPerIntegral > 0)
                {

                    if (refund.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund)
                    {
                        //只处理有兑换规则的积分处理
                        var MoneyPerIntegral = integralExchange.MoneyPerIntegral;
                        decimal? refIntAmount = refund.OrderItemInfo.EnabledRefundAmount - refund.OrderItemInfo.EnabledRefundIntegral;
                        if (refIntAmount > refund.Amount)
                        {
                            refIntAmount = refund.Amount;
                        }
                        if (!refIntAmount.HasValue)
                        {
                            refIntAmount = 0;
                        }
                        if (refIntAmount > 0)
                        {
                            int DeductIntegral = (int)Math.Floor(refIntAmount.Value / MoneyPerIntegral);
                            var _tmp = member.Himall_MemberIntegral.FirstOrDefault();
                            var _curuintg = _tmp == null ? 0 : _tmp.AvailableIntegrals;
                            if (DeductIntegral > 0 && _curuintg > 0 && order.OrderStatus == OrderInfo.OrderOperateStatus.Finish)
                            {
                                //扣除订单产生的积分
                                MemberIntegralRecord info = new MemberIntegralRecord();
                                info.UserName = member.UserName;
                                info.MemberId = member.Id;
                                info.RecordDate = DateTime.Now;
                                info.TypeId = MemberIntegral.IntegralType.Others;
                                info.ReMark = "售后编号【" + refund.Id + "】退款应扣除积分" + DeductIntegral.ToString();
                                DeductIntegral = DeductIntegral > _curuintg ? _curuintg : DeductIntegral;      //超出当前用户积分，直接扣除用户所有积分
                                DeductIntegral = -DeductIntegral;
                                var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.Others, DeductIntegral);
                                ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
                            }
                        }
                    }
                }
                #endregion

                #region 积分抵扣补回
                if (refund.RefundPayType.HasValue)
                {
                    //if (refund.RefundPayType.Value == OrderRefundInfo.OrderRefundPayType.BackOut)
                    //{
                    decimal refundallfee = refund.Amount;
                    decimal refundfee = refundallfee;
                    if (refund.OrderItemInfo.EnabledRefundIntegral > 0 && refund.OrderItemInfo.EnabledRefundAmount > 0)
                    {
                        if (refundfee > (refund.OrderItemInfo.EnabledRefundAmount - refund.OrderItemInfo.EnabledRefundIntegral))
                        {
                            refundfee = refund.OrderItemInfo.EnabledRefundAmount.Value - refund.OrderItemInfo.EnabledRefundIntegral.Value;
                        }
                    }
                    decimal refundinfee = refundallfee - refundfee;
                    if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                    {
                        refundinfee = order.IntegralDiscount;
                    }
                    if (refundinfee > 0)
                    {
                        if (integralExchange != null && integralExchange.IntegralPerMoney > 0)
                        {
                            //只处理有兑换规则的积分处理
                            int IntegralPerMoney = integralExchange.IntegralPerMoney;
                            int BackIntegral = (int)Math.Floor(refundinfee * IntegralPerMoney);
                            var _tmp = member.Himall_MemberIntegral.FirstOrDefault();
                            var _curuintg = _tmp == null ? 0 : _tmp.AvailableIntegrals;
                            if (BackIntegral > 0)
                            {
                                //补充订单退款的积分
                                MemberIntegralRecord info = new MemberIntegralRecord();
                                info.UserName = member.UserName;
                                info.MemberId = member.Id;
                                info.RecordDate = DateTime.Now;
                                info.TypeId = MemberIntegral.IntegralType.Others;
                                info.ReMark = "售后编号【" + refund.Id + "】原路退款时退还抵扣积分" + BackIntegral.ToString();
                                var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.Others, BackIntegral);
                                ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
                            }
                        }
                    }
                    //}
                }
                #endregion

                //数据持久
                refund.ManagerConfirmStatus = OrderRefundInfo.OrderRefundConfirmStatus.Confirmed;
                refund.ManagerConfirmDate = DateTime.Now;

                //日志记录            
                OrderOperationLogInfo orderOperationLog = new OrderOperationLogInfo();
                orderOperationLog.Operator = managerName;
                orderOperationLog.OrderId = refund.OrderId;
                orderOperationLog.OperateDate = DateTime.Now;
                orderOperationLog.OperateContent = "确认退款/退货";

                Context.OrderOperationLogInfo.Add(orderOperationLog);

                //member.Expenditure -= refund.Amount;

                //if (!string.IsNullOrEmpty(model.formId))
                //{
                //    var orderId = string.Join(",", order.Id);
                //    WXAppletFormDatasInfo info = new WXAppletFormDatasInfo();
                //    info.EventId = Convert.ToInt64(MessageTypeEnum.OrderCreated);
                //    info.EventTime = DateTime.Now;
                //    info.EventValue = orderId;
                //    info.ExpireTime = DateTime.Now.AddDays(7);
                //    //info.FormId = model.formId;
                //    ServiceProvider.Instance<IWXSmallProgramService>.Create.AddWXAppletFromData(info);
                //}

                //消息通知
                var orderMessage = new MessageOrderInfo();
                orderMessage.OrderId = order.Id.ToString();
                orderMessage.ShopId = order.ShopId;
                orderMessage.ShopName = order.ShopName;
                orderMessage.RefundMoney = refund.Amount;
                orderMessage.UserName = order.UserName;
                orderMessage.SiteName = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().SiteName;
                orderMessage.TotalMoney = order.OrderTotalAmount;
                orderMessage.ProductName = refund.OrderItemInfo.ProductName;
                orderMessage.RefundTime = refund.ApplyDate;
                if (order.Platform== PlatformType.WeiXinSmallProg)
                {
                    orderMessage.MsgOrderType = MessageOrderType.Applet;
                }
                Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnOrderRefund(order.UserId, orderMessage, refund.Id));


                //销量退还(店铺、商品)
                if (order.PayDate.HasValue)
                {
                    // 修改店铺访问量
                    UpdateShopVisti(refund, order.PayDate.Value);

                    // 修改商品销量
                    UpdateProductVisti(refund, order.PayDate.Value);

                    //会员服务
                    var memberService = ServiceProvider.Instance<IMemberService>.Create;

                    memberService.UpdateNetAmount(refund.UserId, -realRefundAmount);//减少用户的净消费额

                    if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                    {
                        memberService.DecreaseMemberOrderNumber(refund.UserId);//减少用户的下单量
                    }
                }

                //分销
                if (refund.OrderItemInfo.DistributionRate > 0)
                {
                    IDistributionService _idisser = ServiceProvider.Instance<IDistributionService>.Create;
                    long refundnum = 0;
                    if (refund.ReturnQuantity.HasValue)
                    {
                        refundnum = refund.ReturnQuantity.Value;
                    }
                    _idisser.OverDistributionRefund(refund.OrderItemId, refund.Amount, refundnum);
                }
                Context.SaveChanges();

                #region 全部退货后关闭订单
                bool isCloseOrder = true;
                foreach (var item in order.OrderItemInfo.ToList())
                {
                    if (isCloseOrder)
                    {
                        foreach (var ri in item.OrderRefundInfo.ToList())
                        {
                            if (ri.ManagerConfirmStatus != OrderRefundInfo.OrderRefundConfirmStatus.Confirmed)
                            {
                                isCloseOrder = false;
                            }
                        }
                        if (item.Quantity > item.ReturnQuantity)
                        {
                            isCloseOrder = false;
                        }
                    }
                }
                //发生退款时重新计算待付结算订单
                RefundSettlement(refund.OrderId, refund.Id, isCloseOrder);
                if (isCloseOrder)
                {
                    order.CloseReason = "已退货，订单自动关闭";
                    order.OrderStatus = OrderInfo.OrderOperateStatus.Close;
                }
                #endregion

                //发布退款成功消息
                //MessageQueue.PublishTopic(CommonConst.MESSAGEQUEUE_REFUNDSUCCESSED, refund.Id);
                try
                {
                    if (OnRefundSuccessed != null)
                        OnRefundSuccessed(refund.Id);
                }
                catch (Exception e)
                {
                    //Log.Error("OnRefundSuccessed=" + e.Message);
                }
                //退款日志
                AddRefundLog(refund.Id, refund.ApplyNumber, OrderRefundStep.Confirmed, refund.RefundStatus, managerName, refund.ManagerRemark);
            }
        }


        /// <summary>
        /// 检查是否可以退款
        /// </summary>
        /// <param name="refundId"></param>
        /// <returns></returns>
        public bool HasMoneyToRefund(long refundId)
        {
            var model = Context.OrderRefundInfo.Where(a => a.Id == refundId).FirstOrDefault();
            var shopAccount = Context.ShopAccountInfo.Where(a => a.ShopId == model.ShopId).FirstOrDefault();
            var IsSettlement = Context.AccountDetailInfo.Where(a => a.OrderId == model.OrderId).FirstOrDefault();
            var result = true;
            if (IsSettlement != null && model.Amount > shopAccount.Balance)
            {
                return false;
            }
            return result;
        }


        private void RefundSettlement(long orderId, long refundId, bool isClose)
        {
            //获取该订单详情
            var orderInfo = Context.OrderInfo.Where(a => a.Id == orderId).FirstOrDefault();
            if (orderInfo == null)
            {
                return; //如果没有订单
            }
            //获取该订单下所有的退款
            var list = GetOrderRefundList(orderId);
            if (orderInfo.OrderStatus == OrderInfo.OrderOperateStatus.Finish)
            {
                //根据订单号获取待结算的订单
                var model = Context.PendingSettlementOrdersInfo.Where(a => a.OrderId == orderId).FirstOrDefault();
                //已结算订单
                var Settlement = Context.AccountDetailInfo.Where(a => a.OrderId == orderId).FirstOrDefault();
                decimal platCommissionReturn = 0;
                decimal distributorCommissionReturn = 0;
                decimal refundAmountTotal = 0; //总退款金额
                var refund = list.FirstOrDefault(a => a.Id == refundId);//单个项目退款
                var AccountNo = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + refund.Id;
                if (Settlement != null)//如果已经结算了直接扣取店铺余额
                {
                    var service = ServiceProvider.Instance<IBillingService>.Create;
                    service.UpdateAccount(orderInfo.ShopId, -refund.Amount, CommonModel.ShopAccountType.Refund, AccountNo, orderId + "发生退款", refund.OrderId);
                    if (refund.ReturnPlatCommission > 0)
                    {
                        service.UpdateAccount(orderInfo.ShopId, refund.ReturnPlatCommission, CommonModel.ShopAccountType.PlatCommissionRefund, AccountNo, orderId + "发生退款", refund.OrderId);

                    }
                    if (refund.ReturnBrokerage > 0)
                    {
                        service.UpdateAccount(orderInfo.ShopId, refund.ReturnBrokerage, CommonModel.ShopAccountType.DistributorCommissionRefund, AccountNo, orderId + "发生退款", refund.OrderId);
                    }
                }
                else if (model != null) //如果没结算，更新待结算订单
                {
                    foreach (var m in list)
                    {
                        platCommissionReturn += m.ReturnPlatCommission;
                        distributorCommissionReturn += m.ReturnBrokerage;
                        refundAmountTotal += m.Amount;
                    }
                    model.PlatCommissionReturn = platCommissionReturn;
                    model.DistributorCommissionReturn = distributorCommissionReturn;
                    model.RefundAmount = refundAmountTotal;
                    //结算金额-本次退的金额+本次返回的平台佣金和分销佣金
                    model.SettlementAmount = model.SettlementAmount - refund.Amount + refund.ReturnBrokerage + refund.ReturnPlatCommission;
                    Context.SaveChanges();
                }
            }
            else if (orderInfo.OrderStatus == OrderInfo.OrderOperateStatus.WaitReceiving)
            {
                if (isClose)
                {
                    ServiceProvider.Instance<IOrderService>.Create.WritePendingSettlnment(orderInfo);
                }
            }

            ////lly
            ////需要进入待结算的情况：
            ////1、已完成订单
            ////2、非完成订单，因退货(退款不包括在内)导致的订单关闭，需要进入待结算
            //if (orderInfo.OrderStatus != OrderInfo.OrderOperateStatus.Finish)
            //{//未完成
            //    if (!isClose)
            //    {//未关闭、未完成的订单，不结算
            //        return;//如果订单没完成
            //    }
            //    //准备关闭的订单，进入结算逻辑
            //    foreach (var r in list)
            //    {
            //        if (r.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
            //        {//是订单退款，不结算
            //            return;
            //        }
            //    }
            //    //lly
            //    //结果：准备要关闭的订单、且是退货情况下，进入待结算表
            // }
            //已完成的订单，按原流程，进入待结算表
        }


        /// <summary>
        /// 修改店铺访问量
        /// </summary>
        /// <param name="refund"></param>
        /// <param name="payDate"></param>
        void UpdateShopVisti(OrderRefundInfo refund, DateTime payDate)
        {
            //退款不影响金额、数量
            //ShopVistiInfo shopVisti = Context.ShopVistiInfo.FindBy(
            //    item => item.ShopId == refund.ShopId && item.Date == payDate.Date).FirstOrDefault();
            //if (shopVisti != null)
            //{
            //    if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
            //    {
            //        //整笔退
            //        var orditemlist = Context.OrderItemInfo.Where(d => d.OrderId == refund.OrderId).ToList();
            //        foreach (var item in orditemlist)
            //        {
            //            shopVisti.SaleCounts -= item.Quantity;
            //        }
            //    }
            //    else
            //    {
            //        if (refund.IsReturn)
            //            shopVisti.SaleCounts -= refund.OrderItemInfo.ReturnQuantity;
            //    }

            //    shopVisti.SaleAmounts = shopVisti.SaleAmounts - refund.Amount;
            //    Context.SaveChanges();
            //}

        }
        /// <summary>
        /// 修改商品访问量
        /// </summary>
        /// <param name="refund"></param>
        /// <param name="payDate"></param>
        void UpdateProductVisti(OrderRefundInfo refund, DateTime payDate)
        {
            OrderItemInfo orderItem = refund.OrderItemInfo;

            ProductInfo product = new ProductInfo();
            ProductVistiInfo productVisti = new ProductVistiInfo();

            //处理成交量
            if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
            {
                //整笔退
                var orditemlist = Context.OrderItemInfo.Where(d => d.OrderId == refund.OrderId).ToList();
                foreach (var item in orditemlist)
                {
                    product = Context.ProductInfo.FirstOrDefault(d => d.Id == item.ProductId);
                    if (product != null)
                    {
                        product.SaleCounts -= item.Quantity;
                        var searchProduct = Context.SearchProductsInfo.FirstOrDefault(r => r.ProductId == item.ProductId);
                        if (searchProduct != null)
                            searchProduct.SaleCount -= (int)item.Quantity;
                    }
                    //productVisti = Context.ProductVistiInfo.FindBy(
                    //d => d.ProductId == item.ProductId && d.Date == payDate.Date).FirstOrDefault();

                    //if (null != productVisti)
                    //{
                    //    productVisti.SaleCounts -= orderItem.Quantity;
                    //    productVisti.SaleAmounts -= refund.Amount;
                    //}
                }
            }
            else
            {
                if (refund.IsReturn)
                {
                    product = Context.ProductInfo.FirstOrDefault(d => d.Id == refund.OrderItemInfo.ProductId);

                    if (product != null)
                    {
                        product.SaleCounts -= refund.OrderItemInfo.ReturnQuantity;
                        var searchProduct = Context.SearchProductsInfo.FirstOrDefault(r => r.ProductId == product.Id);
                        if (searchProduct != null)
                            searchProduct.SaleCount -= (int)refund.OrderItemInfo.ReturnQuantity;
                    }
                    //productVisti = Context.ProductVistiInfo.FindBy(
                    //    item => item.ProductId == orderItem.ProductId && item.Date == payDate.Date).FirstOrDefault();

                    //if (null != productVisti)
                    //{
                    //    productVisti.SaleCounts -= orderItem.Quantity;
                    //    productVisti.SaleAmounts -= refund.Amount;
                    //}
                }
            }
            Context.SaveChanges();
        }

        #endregion
        /// <summary>
        /// 退款处理
        /// </summary>
        /// <param name="refundId"></param>
        /// <param name="managerRemark"></param>
        /// <param name="managerName"></param>
        /// <param name="notifyurl"></param>
        /// <returns></returns>
        public string ConfirmRefund(long refundId, string managerRemark, string managerName, string notifyurl)
        {
            string result = "";
            //退款信息与状态
            OrderRefundInfo refund = Context.OrderRefundInfo.FindById(refundId);
            OrderInfo order = Context.OrderInfo.FindById(refund.OrderId);
            UserMemberInfo member = Context.UserMemberInfo.FindById(refund.UserId);
            if (refund.ManagerConfirmStatus != OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm)
                throw new HimallException("只有未确认状态的退款/退货才能进行确认操作！");
            if (!refund.RefundPayType.HasValue)
                throw new HimallException("订单退款方式有错，请联系技术人员！");

            using (TransactionScope tran = new TransactionScope())
            {
                //退款逻辑处理
                if (refund.RefundPayType.HasValue)
                {
                    switch (refund.RefundPayType.Value)
                    {
                        case OrderRefundInfo.OrderRefundPayType.BackOut:
                            result = RefundBackOut(refund, notifyurl);
                            break;
                        case OrderRefundInfo.OrderRefundPayType.BackCapital:
                            result = "";
                            RefundBackCapital(refund);
                            break;
                    }

                }

                refund.ManagerRemark = managerRemark;

                if (refund.RefundPayStatus == OrderRefundInfo.OrderRefundPayStatus.PaySuccess)
                {
                    RefundSuccessed(refund, managerName);
                }
                Context.SaveChanges();
                //提交事务
                tran.Complete();
            }
            return result;
        }
        /// <summary>
        /// 异步通知确认退款
        /// </summary>
        /// <param name="batchno"></param>
        public void NotifyRefund(string batchNo)
        {
            if (string.IsNullOrWhiteSpace(batchNo))
            {
                throw new HimallException("错误的批次号");
            }

            OrderRefundInfo refund = Context.OrderRefundInfo.FirstOrDefault(d => d.RefundBatchNo == batchNo);
            if (refund != null)
            {
                refund.RefundPayStatus = OrderRefundInfo.OrderRefundPayStatus.PaySuccess;
                RefundSuccessed(refund, "系统异步退款");
            }
        }
        /// <summary>
        /// 商家审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="auditStatus"></param>
        /// <param name="sellerRemark"></param>
        /// <param name="sellerName"></param>
        public void SellerDealRefund(long id, OrderRefundInfo.OrderRefundAuditStatus auditStatus, string sellerRemark, string sellerName)
        {
            OrderRefundInfo refund = Context.OrderRefundInfo.FindById(id);
            if (auditStatus == OrderRefundInfo.OrderRefundAuditStatus.UnAudit)
            {
                if (refund.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.WaitAudit
                    && refund.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery   //自动任务
                    && refund.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving  //商家收到的货有问题
                    )
                    throw new HimallException("只有待审核状态的退款/退货才能进行处理，自动任务时需要状态为待买家寄货");
            }
            else
            {
                if (refund.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.WaitAudit)
                    throw new HimallException("只有待审核状态的退款/退货才能进行处理");
            }
            if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
            {
                //订单退款无需发货
                if (auditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery)
                {
                    //直接转换为商家审核通过
                    auditStatus = OrderRefundInfo.OrderRefundAuditStatus.Audited;
                    ServiceProvider.Instance<IOrderService>.Create.AgreeToRefundBySeller(refund.OrderId);        //关闭订单
                }
            }
            else
            {
                if (refund.IsReturn == false)
                {
                    if (auditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery)
                    {
                        //直接转换为商家审核通过
                        auditStatus = OrderRefundInfo.OrderRefundAuditStatus.Audited;
                    }
                }
            }

            //处理拒绝
            if (auditStatus == OrderRefundInfo.OrderRefundAuditStatus.UnAudit)
            {
                if (refund.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    var orderitem = Context.OrderItemInfo.FindById(refund.OrderItemId);
                    if (orderitem != null)
                    {
                        if (orderitem.DistributionRate > 0)
                        {
                            IDistributionService _idisser = ServiceProvider.Instance<IDistributionService>.Create;
                            _idisser.CloseDistributionRefund(orderitem.Id);
                        }
                    }
                }

            }

            if (auditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery && !refund.IsReturn)
                refund.SellerAuditStatus = OrderRefundInfo.OrderRefundAuditStatus.Audited;
            else
                refund.SellerAuditStatus = auditStatus;

            refund.SellerAuditDate = DateTime.Now;
            refund.SellerRemark = sellerRemark;
            if (auditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited)
                refund.ManagerConfirmDate = DateTime.Now;

            OrderOperationLogInfo orderOperationLog = new OrderOperationLogInfo();
            orderOperationLog.Operator = sellerName;
            orderOperationLog.OrderId = refund.OrderId;
            orderOperationLog.OperateDate = DateTime.Now;
            orderOperationLog.OperateContent = "商家处理退款退货申请";

            Context.OrderOperationLogInfo.Add(orderOperationLog);

            Context.SaveChanges();

            var stepMap = new Dictionary<OrderRefundInfo.OrderRefundAuditStatus, OrderRefundStep>();
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.Audited, OrderRefundStep.UnConfirm);
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.UnAudit, OrderRefundStep.UnAudit);
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.WaitAudit, OrderRefundStep.WaitAudit);
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery, OrderRefundStep.WaitDelivery);
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving, OrderRefundStep.WaitReceiving);

            //退款日志
            AddRefundLog(refund.Id, refund.ApplyNumber, stepMap[auditStatus], refund.RefundStatus, sellerName, refund.SellerRemark);

            #region 发送售后发货消息
            if (auditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery)
            {
                if (refund != null)
                {
                    var order = refund.OrderItemInfo.OrderInfo;
                    if (order != null)
                    {
                        //消息通知
                        var orderMessage = new MessageOrderInfo();
                        orderMessage.UserName = order.UserName;
                        orderMessage.OrderId = order.Id.ToString();
                        orderMessage.ShopId = order.ShopId;
                        orderMessage.ShopName = order.ShopName;
                        orderMessage.RefundMoney = refund.Amount;
                        orderMessage.SiteName = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().SiteName;
                        orderMessage.TotalMoney = order.OrderTotalAmount;
                        orderMessage.ProductName = refund.OrderItemInfo.ProductName;
                        orderMessage.RefundAuditTime = DateTime.Now;
                        orderMessage.Remark = string.IsNullOrWhiteSpace(sellerRemark) ? "请及时登录系统确认寄货并填写快递信息" : sellerRemark;
                        if (order.Platform== PlatformType.WeiXinSmallProg)
                        {
                            orderMessage.MsgOrderType = MessageOrderType.Applet;
                        }
                        Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnRefundDeliver(order.UserId, orderMessage, refund.Id));
                    }
                }
            }
            #endregion
        }
        /// <summary>
        /// 商家确认到货
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sellerName"></param>
        public void SellerConfirmRefundGood(long id, string sellerName)
        {
            OrderRefundInfo refund = Context.OrderRefundInfo.FindById(id);
            if (refund.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving)
                throw new HimallException("只有待收货状态的退货才能进行确认收货操作");
            refund.SellerAuditStatus = OrderRefundInfo.OrderRefundAuditStatus.Audited;
            refund.SellerConfirmArrivalDate = DateTime.Now;
            refund.ManagerConfirmDate = DateTime.Now;

            OrderOperationLogInfo orderOperationLog = new OrderOperationLogInfo();
            orderOperationLog.Operator = sellerName;
            orderOperationLog.OrderId = refund.OrderId;
            orderOperationLog.OperateDate = DateTime.Now;
            orderOperationLog.OperateContent = "商家确认收到退货";
            Context.OrderOperationLogInfo.Add(orderOperationLog);
            Context.SaveChanges();

            if (refund.OrderItemId > 0 && refund.ReturnQuantity.HasValue && refund.ReturnQuantity.Value > 0) ReturnStock(refund.OrderItemId, refund.OrderId, refund.ReturnQuantity.Value);

            //退款日志
            AddRefundLog(refund.Id, refund.ApplyNumber, OrderRefundStep.UnConfirm, refund.RefundStatus, sellerName, refund.ManagerRemark);

            //ServiceProvider.Instance<IOrderService>.Create.UpdateMemberOrderInfo(refund.UserId,-refund.Amount,-refund.ReturnQuantity); 不维护此冗余金额
        }
        /// <summary>
        /// 用户发货
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sellerName"></param>
        /// <param name="expressCompanyName"></param>
        /// <param name="shipOrderNumber"></param>
        public void UserConfirmRefundGood(long id, string sellerName, string expressCompanyName, string shipOrderNumber)
        {
            OrderRefundInfo refund = Context.OrderRefundInfo.FindById(id);
            if (refund.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery)
                throw new HimallException("只有待等待发货状态的能进行发货操作");
            refund.ShipOrderNumber = shipOrderNumber;
            refund.ExpressCompanyName = expressCompanyName;
            refund.SellerAuditStatus = OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving;
            refund.BuyerDeliverDate = DateTime.Now;
            OrderOperationLogInfo orderOperationLog = new OrderOperationLogInfo();
            orderOperationLog.Operator = sellerName;
            orderOperationLog.OrderId = refund.OrderId;
            orderOperationLog.OperateDate = DateTime.Now;
            orderOperationLog.OperateContent = "买家确认发回商品";
            Context.OrderOperationLogInfo.Add(orderOperationLog);
            Context.SaveChanges();
            //退款日志
            AddRefundLog(refund.Id, refund.ApplyNumber, OrderRefundStep.WaitReceiving, refund.RefundStatus, sellerName, refund.ExpressCompanyName + "：" + refund.ShipOrderNumber);
            //发送售后消息
            var order = ServiceProvider.Instance<IOrderService>.Create.GetOrder(refund.OrderId, refund.UserId);
            SendRefundAppMessage(refund, order);


        }

        public IQueryable<OrderRefundInfo> GetAllOrderRefunds()
        {
            return Context.OrderRefundInfo.FindAll();
        }


        /// <summary>
        /// 根据订单ID获取退款成功的列表
        /// </summary>
        /// <param name="OrderId"></param>
        /// <returns></returns>
        public List<OrderRefundInfo> GetOrderRefundList(long orderId)
        {
            var list = Context.OrderRefundInfo.Where(a => a.OrderId == orderId && a.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed).OrderByDescending(a => a.Id).ToList();
            return list;
        }
        /// <summary>
        /// 申请退款
        /// </summary>
        /// <param name="info"></param>
        public void AddOrderRefund(OrderRefundInfo info)
        {
            var ordser = ServiceProvider.Instance<IOrderService>.Create;
            var _iFightGroupService = ServiceProvider.Instance<IFightGroupService>.Create;
            var order = ordser.GetOrder(info.OrderId, info.UserId);
            if (order == null)
                throw new Himall.Core.HimallException("该订单已删除或不属于该用户");
            if ((int)order.OrderStatus < 2)
                throw new Himall.Core.HimallException("错误的售后申请,订单状态有误");
            info.ShopId = order.ShopId;
            info.ShopName = order.ShopName;

            if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || order.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp)
            {
                info.RefundMode = OrderRefundInfo.OrderRefundMode.OrderRefund;
                info.ReturnQuantity = 0;
            }
            //售后时间限制
            if (ordser.IsRefundTimeOut(info.OrderId))
            {
                throw new Himall.Core.HimallException("订单已超过售后期");
            }
            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                var fgord = _iFightGroupService.GetFightGroupOrderStatusByOrderId(order.Id);
                if (!fgord.CanRefund)
                {
                    throw new Himall.Core.HimallException("拼团订单处于不可售后状态");
                }
            }
            var orderitem = order.OrderItemInfo.FirstOrDefault(a => a.Id == info.OrderItemId);
            if (orderitem == null && info.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund)
                throw new Himall.Core.HimallException("该订单条目已删除或不属于该用户");
            if (info.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
            {
                if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                    throw new Himall.Core.HimallException("错误的订单退款申请,订单状态有误");
                info.IsReturn = false;
                info.ReturnQuantity = 0;
                if (info.Amount > order.OrderEnabledRefundAmount)
                    throw new Himall.Core.HimallException("退款金额不能超过订单的实际支付金额");
            }
            else
            {
                if (info.Amount > (orderitem.EnabledRefundAmount - orderitem.RefundPrice))
                    throw new Himall.Core.HimallException("退款金额不能超过订单的可退金额");
                if (info.ReturnQuantity > (orderitem.Quantity - orderitem.ReturnQuantity))
                    throw new Himall.Core.HimallException("退货数量不可以超出可退数量");
            }
            if (info.ReturnQuantity < 0)
                throw new Himall.Core.HimallException("错误的退货数量");
            bool isOrderRefund = false;    //是否整笔订单退款
            if (info.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
            {
                isOrderRefund = true;
            }

            var isCanApply = CanApplyRefund(info.OrderId, info.OrderItemId, isOrderRefund);

            if (!isCanApply)
                throw new Himall.Core.HimallException("您已申请过售后，不可重复申请");
            if (!isOrderRefund)
            {
                if (info.ReturnQuantity > 0)
                {
                    info.RefundMode = OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund;
                }
                else
                {
                    info.RefundMode = OrderRefundInfo.OrderRefundMode.OrderItemRefund;
                }
            }
            info.SellerAuditDate = DateTime.Now;
            info.SellerAuditStatus = OrderRefundInfo.OrderRefundAuditStatus.WaitAudit;
            info.ManagerConfirmDate = DateTime.Now;
            info.ManagerConfirmStatus = OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm;
            if (isOrderRefund == true)
            {
                info.OrderItemId = Context.OrderItemInfo.FirstOrDefault(d => d.OrderId == info.OrderId).Id;
            }

            decimal distributionBrokerage = 0;
            decimal distributionRate = 0;
            var orditemlist = new List<OrderItemInfo>();

            if (!isOrderRefund)
            {
                var model = Context.OrderItemInfo.FirstOrDefault(d => d.Id == info.OrderItemId);
                decimal itemRealTotalMoney = model.RealTotalPrice - model.CouponDiscount - model.FullDiscount;  //实付金额
                if ((model.Quantity - model.ReturnQuantity) < info.ReturnQuantity || (itemRealTotalMoney - model.RefundPrice) < info.Amount)
                {
                    throw new HimallException("退货和退款数量不能超过订单的实际数量和金额！");
                }
                // model.ReturnQuantity = info.ShowReturnQuantity; （YX放到完成后修改）
                //  model.RefundPrice = info.Amount;
                if (model.DistributionRate > 0)
                {
                    distributionRate = model.DistributionRate.Value;
                    //计算退还佣金
                    decimal unitPrice = Math.Round((itemRealTotalMoney / model.Quantity), 2);
                    int rnum = (int)Math.Ceiling(info.Amount / unitPrice);
                    decimal refundPrice = (unitPrice * rnum);
                    if (refundPrice > itemRealTotalMoney)
                    {
                        refundPrice = itemRealTotalMoney;
                    }
                    distributionBrokerage = Math.Round((refundPrice * distributionRate) / 100, 2, MidpointRounding.AwayFromZero);
                    //TODO:DZY[151201]  加上对退款表的维护
                    info.ReturnBrokerage = distributionBrokerage;
                }
                if (model.CommisRate > 0)
                {
                    //计算退还佣金
                    decimal unitPrice = Math.Round((itemRealTotalMoney / model.Quantity), 2);
                    int rnum = (int)Math.Ceiling(info.Amount / unitPrice);
                    decimal refundPrice = (unitPrice * rnum);
                    if (refundPrice > itemRealTotalMoney)
                    {
                        refundPrice = itemRealTotalMoney;
                    }
                    var returnPlatCommission = Math.Round((refundPrice * model.CommisRate), 2, MidpointRounding.AwayFromZero);
                    // 加上对退款表的维护
                    info.ReturnPlatCommission = returnPlatCommission;
                }

                if (info.RefundMode == OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund)
                {
                    if (info.ReturnQuantity <= 0 || info.ReturnQuantity > (model.Quantity - model.ReturnQuantity))
                        info.ReturnQuantity = model.Quantity - model.ReturnQuantity;
                }
                else
                    info.ReturnQuantity = 0;
            }
            else
            {
                info.ReturnQuantity = 0;
            }

            info.ApplyNumber = 1;

            Context.OrderRefundInfo.Add(info);
            Context.SaveChanges();

            var user = Context.UserMemberInfo.FirstOrDefault(d => d.Id == info.UserId);
            var reason = info.Reason;
            if (!string.IsNullOrEmpty(info.ReasonDetail))
                reason += ":" + info.ReasonDetail;
            //退款日志
            AddRefundLog(info.Id, info.ApplyNumber, OrderRefundStep.WaitAudit, info.RefundStatus, user.UserName, reason);

            //分销订单处理
            if (distributionRate > 0)
            {
                IDistributionService _idisser = ServiceProvider.Instance<IDistributionService>.Create;
                _idisser.UpdateDistributionRefund(info.OrderItemId, info.Amount, distributionBrokerage, info.Id);
            }

            //新增小程序推送Form数据
            if (!string.IsNullOrEmpty(info.formId))
            {
                WXAppletFormDatasInfo wxInfo = new WXAppletFormDatasInfo();
                wxInfo.EventId = Convert.ToInt64(MessageTypeEnum.OrderRefund);
                wxInfo.EventTime = DateTime.Now;
                wxInfo.EventValue = info.OrderId.ToString();
                wxInfo.ExpireTime = DateTime.Now.AddDays(7);
                wxInfo.FormId = info.formId;
                ServiceProvider.Instance<IWXMsgTemplateService>.Create.AddWXAppletFromData(wxInfo);
            }

            //发送售后消息
            SendRefundAppMessage(info, order);
        }

        /// <summary>
        /// 激活售后
        /// </summary>
        /// <param name="info"></param>
        public void ActiveRefund(OrderRefundInfo info)
        {
            var refund = Context.OrderRefundInfo.FirstOrDefault(d => d.Id == info.Id);
            if (refund == null)
            {
                throw new HimallException("错误的售后记录");
            }
            if (refund.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.UnAudit)
            {
                throw new HimallException("售后记录状态有误，不可激活");
            }

            //info数据值转换给refund
            refund.Applicant = info.Applicant;
            refund.ContactPerson = info.ContactPerson;
            refund.ContactCellPhone = info.ContactCellPhone;
            refund.RefundAccount = info.RefundAccount;
            refund.ApplyDate = info.ApplyDate;
            refund.Amount = info.Amount;
            refund.Reason = info.Reason;
            refund.SellerAuditStatus = info.SellerAuditStatus;
            refund.SellerAuditDate = info.SellerAuditDate;
            //refund.SellerRemark = info.SellerRemark;
            refund.ManagerConfirmStatus = info.ManagerConfirmStatus;
            refund.ManagerConfirmDate = info.ManagerConfirmDate;
            //refund.ManagerRemark = info.ManagerRemark;
            refund.IsReturn = info.IsReturn;
            refund.ExpressCompanyName = info.ExpressCompanyName;
            refund.ShipOrderNumber = info.ShipOrderNumber;
            refund.Payee = info.Payee;
            refund.PayeeAccount = info.PayeeAccount;
            refund.RefundPayStatus = info.RefundPayStatus;
            refund.RefundPayType = info.RefundPayType;
            refund.BuyerDeliverDate = info.BuyerDeliverDate;
            refund.SellerConfirmArrivalDate = info.SellerConfirmArrivalDate;
            refund.RefundBatchNo = info.RefundBatchNo;
            refund.RefundPostTime = info.RefundPostTime;
            refund.ReturnQuantity = info.ReturnQuantity;
            refund.ReturnBrokerage = info.ReturnBrokerage;
            if (!string.IsNullOrEmpty(info.ReasonDetail))
                refund.ReasonDetail = info.ReasonDetail;
            refund.CertPic1 = info.CertPic1;
            refund.CertPic2 = info.CertPic2;
            refund.CertPic3 = info.CertPic3;
            if (refund.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund)
            {
                refund.RefundMode = info.RefundMode;
            }

            bool isOrderRefund = false;
            if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
            {
                isOrderRefund = true;
            }

            if (!isOrderRefund)
            {
                if (refund.ReturnQuantity > 0)
                {
                    refund.RefundMode = OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund;
                }
                else
                {
                    refund.RefundMode = OrderRefundInfo.OrderRefundMode.OrderItemRefund;
                }
            }
            refund.SellerAuditDate = DateTime.Now;
            refund.SellerAuditStatus = OrderRefundInfo.OrderRefundAuditStatus.WaitAudit;
            refund.ManagerConfirmDate = DateTime.Now;
            refund.ManagerConfirmStatus = OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm;
            if (isOrderRefund == true)
            {
                refund.OrderItemId = Context.OrderItemInfo.FirstOrDefault(d => d.OrderId == refund.OrderId).Id;
            }

            decimal distributionBrokerage = 0;
            decimal distributionRate = 0;

            List<OrderItemInfo> orditemlist = new List<OrderItemInfo>();
            if (!isOrderRefund)
            {
                var model = Context.OrderItemInfo.FirstOrDefault(d => d.Id == refund.OrderItemId);
                decimal itemRealTotalMoney = model.RealTotalPrice - model.CouponDiscount;   //实付金额
                if ((model.Quantity - model.ReturnQuantity) < refund.ReturnQuantity || (itemRealTotalMoney - model.RefundPrice) < refund.Amount)
                {
                    throw new HimallException("退货和退款数量不能超过订单的实际数量和金额！");
                }

                if (model.DistributionRate > 0)
                {
                    distributionRate = model.DistributionRate.Value;
                    //计算退还佣金
                    decimal unitPrice = Math.Round((itemRealTotalMoney / model.Quantity), 2);
                    int rnum = (int)Math.Ceiling(refund.Amount / unitPrice);
                    decimal refundPrice = (unitPrice * rnum);
                    if (refundPrice > itemRealTotalMoney)
                    {
                        refundPrice = itemRealTotalMoney;
                    }
                    distributionBrokerage = Math.Round((refundPrice * distributionRate) / 100, 2, MidpointRounding.AwayFromZero);
                    //TODO:DZY[151201]  加上对退款表的维护
                    refund.ReturnBrokerage = distributionBrokerage;
                }
                if (model.CommisRate > 0)
                {
                    //计算退还佣金
                    decimal unitPrice = Math.Round((itemRealTotalMoney / model.Quantity), 2);
                    int rnum = (int)Math.Ceiling(refund.Amount / unitPrice);
                    decimal refundPrice = (unitPrice * rnum);
                    if (refundPrice > itemRealTotalMoney)
                    {
                        refundPrice = itemRealTotalMoney;
                    }
                    var returnPlatCommission = Math.Round((refundPrice * model.CommisRate), 2, MidpointRounding.AwayFromZero);
                    // 加上对退款表的维护
                    refund.ReturnPlatCommission = returnPlatCommission;
                }

                if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund)
                {
                    if (refund.ReturnQuantity <= 0 || refund.ReturnQuantity > (model.Quantity - model.ReturnQuantity))
                        refund.ReturnQuantity = model.Quantity - model.ReturnQuantity;
                }
                else
                    refund.ReturnQuantity = 0;
            }
            else
            {
                refund.ReturnQuantity = 0;
            }

            if (refund.ApplyNumber == null)
            {
                refund.ApplyNumber = 1;
            }
            refund.ApplyNumber += 1;

            Context.SaveChanges();

            var user = Context.UserMemberInfo.FirstOrDefault(d => d.Id == refund.UserId);

            var reason = info.Reason;
            if (!string.IsNullOrEmpty(info.ReasonDetail))
                reason += ":" + info.ReasonDetail;
            //退款日志
            AddRefundLog(refund.Id, refund.ApplyNumber, OrderRefundStep.WaitAudit, refund.RefundStatus, user.UserName, reason);

            //分销订单处理
            if (distributionRate > 0)
            {
                IDistributionService _idisser = ServiceProvider.Instance<IDistributionService>.Create;
                _idisser.UpdateDistributionRefund(refund.OrderItemId, refund.Amount, distributionBrokerage, refund.Id);
            }

            //新增小程序推送Form数据
            if (!string.IsNullOrEmpty(info.formId))
            {
                WXAppletFormDatasInfo wxInfo = new WXAppletFormDatasInfo();
                wxInfo.EventId = Convert.ToInt64(MessageTypeEnum.OrderRefund);
                wxInfo.EventTime = DateTime.Now;
                wxInfo.EventValue = info.OrderId.ToString();
                wxInfo.ExpireTime = DateTime.Now.AddDays(7);
                wxInfo.FormId = info.formId;
                ServiceProvider.Instance<IWXMsgTemplateService>.Create.AddWXAppletFromData(wxInfo);
            }

            //发送售后消息
            var order = ServiceProvider.Instance<IOrderService>.Create.GetOrder(info.OrderId, info.UserId);
            SendRefundAppMessage(info, order);
        }



        /// <summary>
        /// 通过订单编号获取整笔退款
        /// </summary>
        /// <param name="id">订单编号</param>
        /// <returns></returns>
        public OrderRefundInfo GetOrderRefundByOrderId(long id)
        {
            return Context.OrderRefundInfo.FirstOrDefault(a => a.OrderId == id && a.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund);
        }

        public OrderRefundInfo GetOrderRefund(long id, long? userId = null, long? shopId = null)
        {
            var model = Context.OrderRefundInfo.FirstOrDefault(a => a.Id == id);

            if (model == null || userId.HasValue && userId.Value != model.UserId || shopId.HasValue && shopId.Value != model.ShopId)
                return null;
            return model;
        }
        /// <summary>
        /// 是否可以申请退款
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="orderItemId"></param>
        /// <param name="isAllOrderRefund">是否为整笔退 null 所有 true 整笔退 false 货品售后</param>
        /// <returns></returns>
        public bool CanApplyRefund(long orderId, long orderItemId, bool? isAllOrderRefund = null)
        {
            bool result = false;
            var sql = Context.OrderRefundInfo.Where(d => d.OrderId == orderId);
            if (isAllOrderRefund == true)
            {
                sql = sql.Where(d => d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund);
            }
            else
            {
                sql = sql.Where(d => d.OrderItemId == orderItemId);
                if (isAllOrderRefund == false)
                {
                    sql = sql.Where(d => d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                }
            }
            result = (sql.Count() < 1);
            return result;
        }

        /// <summary>
        /// 添加或修改售后原因
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        public void UpdateAndAddRefundReason(string reason, long id)
        {
            if (Context.RefundReasonInfo.Any(d => d.Id != id && d.AfterSalesText == reason))
            {
                throw new HimallException("售后原因重复");
            }
            RefundReasonInfo data = Context.RefundReasonInfo.FirstOrDefault(d => d.Id == id);
            if (id == 0)
            {
                data = new RefundReasonInfo();
            }
            if (data == null)
            {
                throw new HimallException("售后原因为空");
            }
            data.AfterSalesText = reason;
            data.Sequence = 100;   //默认排序100

            if (id == 0)
            {
                Context.RefundReasonInfo.Add(data);
            }
            Context.SaveChanges();
        }
        /// <summary>
        /// 获取售后原因列表
        /// </summary>
        /// <returns></returns>
        public List<RefundReasonInfo> GetRefundReasons()
        {
            return Context.RefundReasonInfo.ToList();
        }
        /// <summary>
        /// 删除售后原因
        /// </summary>
        /// <param name="id"></param>
        public void DeleteRefundReason(long id)
        {
            var data = Context.RefundReasonInfo.FirstOrDefault(d => d.Id == id);
            if (data != null)
            {
                Context.RefundReasonInfo.Remove(data);
                Context.SaveChanges();
            }
        }
        /// <summary>
        /// 获取售后日志
        /// </summary>
        /// <param name="refundId"></param>
        /// <returns></returns>
        public List<OrderRefundlogsInfo> GetRefundLogs(long refundId, int currentApplyNumber = 0, bool haveCurrentApplyNumber = true)
        {
            var sql = Context.OrderRefundlogsInfo.Where(d => d.RefundId == refundId);
            if (currentApplyNumber > 0)
            {
                int getappnum = currentApplyNumber - 1;
                if (haveCurrentApplyNumber)
                {
                    getappnum++;
                }

                sql = sql.Where(d => d.ApplyNumber <= getappnum);
            }
            sql = sql.OrderByDescending(d => d.OperateDate);
            var list = sql.ToList();

            #region 填充Step和Remark
            //step和remark是后来添加的，为了适应老数据，所以需要根据OperateContent填充Step和Remark
            var stepMap = new Dictionary<string, OrderRefundStep>();
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.Audited.ToDescription(), OrderRefundStep.UnConfirm);
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.UnAudit.ToDescription(), OrderRefundStep.UnAudit);
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.WaitAudit.ToDescription(), OrderRefundStep.WaitAudit);
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery.ToDescription(), OrderRefundStep.WaitDelivery);
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving.ToDescription(), OrderRefundStep.WaitReceiving);
            stepMap.Add(OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm.ToDescription(), OrderRefundStep.UnConfirm);
            stepMap.Add(OrderRefundInfo.OrderRefundConfirmStatus.Confirmed.ToDescription(), OrderRefundStep.Confirmed);

            foreach (var item in list)
            {
                if (item.Step > 0)
                    continue;

                var match = System.Text.RegularExpressions.Regex.Match(item.OperateContent, "【(.+)】(.+)$");
                if (match.Success)
                {
                    var refundState = match.Groups[1].Value;
                    if (stepMap.ContainsKey(refundState))
                        item.Step = stepMap[refundState];
                    item.Remark = match.Groups[2].Value;
                }
            }
            #endregion

            return list;
        }
        /// <summary>
        /// 写入售后日志
        /// <para>写入日志的内容为：[状态]日志说明</para>
        /// </summary>
        /// <param name="RefundId"></param>
        /// <param name="LogContent"></param>
        public void AddRefundLog(long refundId, int? applyNumber, OrderRefundStep step, string refundState, string userName, string remark)
        {
            applyNumber = applyNumber.HasValue ? applyNumber.Value : 1;
            OrderRefundlogsInfo data = new OrderRefundlogsInfo();
            data.RefundId = refundId;
            data.ApplyNumber = applyNumber;
            data.Operator = userName;
            data.OperateDate = DateTime.Now;
            data.OperateContent = "【" + refundState + "】" + remark;
            data.Remark = remark;
            data.Step = step;
            Context.OrderRefundlogsInfo.Add(data);
            Context.SaveChanges();
        }
        /// <summary>
        /// 自动审核退款(job)
        /// </summary>
        public void AutoAuditRefund()
        {
            var sitesetser = ServiceProvider.Instance<ISiteSettingService>.Create;
            // var siteSetting = sitesetser.GetSiteSettings();
            var siteSetting = sitesetser.GetSiteSettingsByObjectCache();
            if (siteSetting.AS_ShopConfirmTimeout > 0)
            {
                DateTime stime = DateTime.Now.AddDays(-siteSetting.AS_ShopConfirmTimeout);
                var rflist = Context.OrderRefundInfo.Where(d => d.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit && d.ApplyDate < stime).Select(d => d.Id).ToList();
                if (rflist.Count > 0)
                {
                    Himall.Core.Log.Debug("RefundJob : AutoAuditRefund Number=" + rflist.Count);
                }
                foreach (var item in rflist)
                {
                    try
                    {
                        SellerDealRefund(item, OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery, "卖家超时未处理，系统自动同意售后", "系统Job");
                    }
                    catch (Exception ex)
                    {
                        Log.Debug("RefundJob : AutoAuditRefund [有错误]编号：" + item.ToString(), ex);
                    }
                }
            }
        }
        /// <summary>
        /// 自动关闭过期未寄货退款(job)
        /// </summary>
        public void AutoCloseByDeliveryExpired()
        {
            var sitesetser = ServiceProvider.Instance<ISiteSettingService>.Create;
            //  var siteSetting = sitesetser.GetSiteSettings();
            //windows服务调用此处不报错
            var siteSetting = sitesetser.GetSiteSettingsByObjectCache();
            if (siteSetting.AS_SendGoodsCloseTimeout > 0)
            {
                DateTime stime = DateTime.Now.AddDays(-siteSetting.AS_SendGoodsCloseTimeout);
                var rflist = Context.OrderRefundInfo.Where(d => d.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery && d.SellerAuditDate < stime).Select(d => d.Id).ToList();
                if (rflist.Count > 0)
                {
                    Himall.Core.Log.Debug("RefundJob : AutoCloseByDeliveryExpired Number=" + rflist.Count);
                }
                foreach (var item in rflist)
                {
                    try
                    {
                        SellerDealRefund(item, OrderRefundInfo.OrderRefundAuditStatus.UnAudit, "买家超时未寄货，系统自动拒绝售后", "系统Job");
                    }
                    catch (Exception ex)
                    {
                        Log.Debug("RefundJob : AutoCloseByDeliveryExpired [有错误]编号：" + item.ToString(), ex);
                    }
                }
            }
        }
        /// <summary>
        /// 自动商家确认到货(job)
        /// </summary>
        public void AutoShopConfirmArrival()
        {
            var sitesetser = ServiceProvider.Instance<ISiteSettingService>.Create;
            //  var siteSetting = sitesetser.GetSiteSettings();
            //windows服务得用此缓存
            var siteSetting = sitesetser.GetSiteSettingsByObjectCache();
            if (siteSetting.AS_ShopNoReceivingTimeout > 0)
            {
                DateTime stime = DateTime.Now.AddDays(-siteSetting.AS_ShopNoReceivingTimeout);
                var rflist = Context.OrderRefundInfo.Where(d => d.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving && d.BuyerDeliverDate < stime).Select(d => d.Id).ToList();
                if (rflist.Count > 0)
                {
                    Himall.Core.Log.Debug("RefundJob : AutoShopConfirmArrival Number=" + rflist.Count);
                }
                foreach (var item in rflist)
                {
                    try
                    {
                        SellerConfirmRefundGood(item, "系统Job");
                    }
                    catch (Exception ex)
                    {
                        Log.Debug("RefundJob : AutoShopConfirmArrival [有错误]编号：" + item.ToString(), ex);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 售后发送app消息
        /// </summary>
        /// <param name="orderInfo"></param>
        public void SendRefundAppMessage(OrderRefundInfo refundInfo, OrderInfo orderInfo)
        {
            IAppMessageService _iAppMessageService = Himall.ServiceProvider.Instance<IAppMessageService>.Create;
            var app = new AppMessagesInfo()
            {
                IsRead = false,
                sendtime = DateTime.Now,
                SourceId = refundInfo.Id,
                TypeId = (int)AppMessagesType.AfterSale,
                OrderPayDate = Core.Helper.TypeHelper.ObjectToDateTime(orderInfo.PayDate),
                ShopId = 0,
                ShopBranchId = 0
            };
            if (refundInfo.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit)
            {
                app.Content = string.Format("{0} 等待您审核", orderInfo.Id);
                app.Title = "您有新的售后申请";
            }
            else if (refundInfo.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving)
            {
                app.Content = string.Format("{0} 等待您收货", orderInfo.Id);
                app.Title = "您有买家寄回的商品";
            }
            if (orderInfo.ShopBranchId.HasValue && orderInfo.ShopBranchId.Value > 0)
            {
                app.ShopBranchId = orderInfo.ShopBranchId.Value;
            }
            else
            {
                app.ShopId = refundInfo.ShopId;
            }
            if (!string.IsNullOrEmpty(app.Title))
                _iAppMessageService.AddAppMessages(app);
        }

        /// <summary>
        /// 确认收货后，处理库存
        /// </summary>
        /// <param name="order"></param>
        private void ReturnStock(long orderItemId, long orderId, long returnQuantity)
        {
            OrderInfo order = Context.OrderInfo.FindById(orderId);
            var orderItem = Context.OrderItemInfo.FindById(orderItemId);
            if (orderItem != null && order != null)
            {
                SKUInfo sku = Context.SKUInfo.FindById(orderItem.SkuId);
                if (sku != null)
                {
                    if (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0)
                    {
                        var sbSku = Context.ShopBranchSkusInfo.FirstOrDefault(p => p.SkuId == sku.Id && p.ShopBranchId == order.ShopBranchId.Value);
                        if (sbSku != null)
                            sbSku.Stock += (int)returnQuantity;
                    }
                    else
                        sku.Stock += (int)returnQuantity;
                }
            }
            Context.SaveChanges();
        }       
    }
}
