using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.SmallProgAPI.Helper;
using Himall.SmallProgAPI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Himall.SmallProgAPI
{
    /// <summary>
    /// 售后
    /// </summary>
    public class OrderRefundController : BaseApiController
    {
        #region 获取信息
        /// <summary>
        /// 获取售后列表
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public object GetList(string openId, int pageIndex = 1, int pageSize = 10)
        {

            CheckUserLogin();
            var orderser = ServiceProvider.Instance<IOrderService>.Create;
            var refundser = ServiceProvider.Instance<IRefundService>.Create;
            var vshopser = ServiceProvider.Instance<IVShopService>.Create;

            DateTime? startDate = null;
            DateTime? endDate = null;
            var queryModel = new RefundQuery()
            {
                StartDate = startDate,
                EndDate = endDate,
                UserId = CurrentUser.Id,
                PageSize = pageSize,
                PageNo = pageIndex,
                ShowRefundType = 0
            };
            var refunds = refundser.GetOrderRefunds(queryModel);
            var result = new OrderRefundGetListModel(true);
            result.RecordCount = refunds.Total;
            result.Data = refunds.Models.Select(item =>
            {
                var vshop = vshopser.GetVShopByShopId(item.ShopId) ?? new VShopInfo() { Id = 0 };
                var order = orderser.GetOrder(item.OrderId, CurrentUser.Id);
                var status = item.RefundStatus;
                if (order != null && (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0))
                    status = status.Replace("商家", "门店");

                List<OrderItemInfo> orderItems = new List<OrderItemInfo>();
                if (item.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    orderItems = orderser.GetOrder(item.OrderId, CurrentUser.Id).OrderItemInfo.ToList();
                }
                else
                {
                    orderItems.Add(item.OrderItemInfo);
                }
                return new OrderRefundItem
                {
                    OrderId = item.OrderId.ToString(),
                    OrderTotal = order.RefundTotalAmount.ToString("f2"),
                    Status = item.RefundStatusValue.Value,
                    StatusText = status,
                    AdminRemark = (item.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed ? item.ManagerRemark : item.SellerRemark),
                    AfterSaleId = item.Id,
                    AfterSaleType = item.RefundMode.GetHashCode(),
                    ApplyForTime = item.ApplyDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    RefundAmount = item.Amount.ToString("f2"),
                    RefundType = item.RefundPayType.GetHashCode(),
                    RefundTypeText = item.RefundPayType.ToDescription(),
                    SkuId = item.OrderItemInfo.SkuId,
                    UserExpressCompanyName = item.ExpressCompanyName,
                    UserRemark = "",
                    UserShipOrderNumber = item.ShipOrderNumber,
                    IsRefund = item.RefundMode != OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund,
                    IsReturn = item.RefundMode == OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund,
                    ShopName = item.ShopName,
                    Vshopid = vshop.Id,
                    LineItems = orderItems.Select(d => new OrderRefundSku
                    {
                        Status = item.SellerAuditStatus.GetHashCode(),
                        StatusText = item.SellerAuditStatus.ToDescription(),
                        SkuId = d.SkuId,
                        Name = d.ProductName,
                        Price = d.SalePrice,
                        Amount = d.SalePrice * d.Quantity,
                        Quantity = d.Quantity,
                        Image = Core.HimallIO.GetRomoteProductSizeImage(d.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_150),
                        SkuText = d.Color + " " + d.Size + " " + d.Version,
                        ProductId = d.ProductId
                    }).ToList(),
                };
            }).ToList();

            return Json(result);
        }
        /// <summary>
        ///获取售后详情
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="RefundId"></param>
        /// <returns></returns>
        public object GetRefundDetail(string openId, long RefundId)
        {
            CheckUserLogin();
            var _iOrderService = ServiceProvider.Instance<IOrderService>.Create;
            var refundser = ServiceProvider.Instance<IRefundService>.Create;
            var refundinfo = refundser.GetOrderRefund(RefundId, CurrentUser.Id);
            var order = _iOrderService.GetOrder(refundinfo.OrderId, CurrentUser.Id);
            var status = refundinfo.RefundStatus;
            if (order != null && (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0))
                status = status.Replace("商家", "门店");
            bool ResetActive = false;
            if ((refundinfo.RefundMode != Himall.Model.OrderRefundInfo.OrderRefundMode.OrderRefund) || (refundinfo.RefundMode == Himall.Model.OrderRefundInfo.OrderRefundMode.OrderRefund && refundinfo.OrderItemInfo.OrderInfo.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.WaitDelivery))
                ResetActive = true;
            string DealTime = refundinfo.SellerAuditDate.ToString("yyyy-MM-dd HH:mm:ss");
            if (refundinfo.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited)
            {
                DealTime = refundinfo.ManagerConfirmDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
            List<OrderItemInfo> orderItems = new List<OrderItemInfo>();
            if (refundinfo.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
            {
                orderItems = order.OrderItemInfo.ToList();
            }
            else
            {
                orderItems.Add(refundinfo.OrderItemInfo);
            }
            OrderRefundGetRefundDetailModel result = new OrderRefundGetRefundDetailModel(true);
            result.Data = new RefundDetail()
            {
                AdminRemark = (refundinfo.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed ? refundinfo.ManagerRemark : refundinfo.SellerRemark),
                ApplyForTime = refundinfo.ApplyDate.ToString("yyyy-MM-dd HH:mm:ss"),
                Remark = "",
                Status = refundinfo.RefundStatusValue.Value,
                StatusText = status,
                DealTime = DealTime,
                Operator = (refundinfo.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited ? "管理员" : "商家"),
                Reason = refundinfo.Reason,
                RefundId = refundinfo.Id,
                OrderId = refundinfo.OrderId.ToString(),
                Quantity = refundinfo.ReturnQuantity.Value,
                RefundMoney = refundinfo.Amount.ToString("f2"),
                RefundType = refundinfo.RefundPayType.ToDescription(),
                ShopName = refundinfo.ShopName,
                BankAccountName = "",
                BankAccountNo = "",
                BankName = "",
                OrderTotal = order.RefundTotalAmount.ToString("f2"),
                CanResetActive = ResetActive,
                IsOrderRefundTimeOut = _iOrderService.IsRefundTimeOut(refundinfo.OrderId),
                FinishTime = refundinfo.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed ? refundinfo.ManagerConfirmDate.ToString("yyyy-MM-dd HH:mm:ss") : "",
                ProductInfo = orderItems.Select(l => new RefundDetailSKU
                {
                    ProductName = l.ProductName,
                    SKU = l.SKU,
                    SKUContent = l.Color + " " + l.Size + " " + l.Version,
                    Price = l.SalePrice.ToString("f2"),
                    Quantity = (int)l.Quantity,
                    ThumbnailsUrl = Core.HimallIO.GetRomoteProductSizeImage(l.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_150),
                }).ToList()
            };
            return Json(result);
        }
        /// <summary>
        ///获取售后详情
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="RefundId"></param>
        /// <returns></returns>
        public object GetReturnDetail(string openId, long returnId)
        {
            CheckUserLogin();
            var _iOrderService = ServiceProvider.Instance<IOrderService>.Create;
            var refundser = ServiceProvider.Instance<IRefundService>.Create;
            var refundinfo = refundser.GetOrderRefund(returnId, CurrentUser.Id);
            var order = _iOrderService.GetOrder(refundinfo.OrderId, CurrentUser.Id);
            var status = refundinfo.RefundStatus;
            if (order != null && (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0))
                status = status.Replace("商家", "门店");
            bool ResetActive = false;
            if ((refundinfo.RefundMode != Himall.Model.OrderRefundInfo.OrderRefundMode.OrderRefund) || (refundinfo.RefundMode == Himall.Model.OrderRefundInfo.OrderRefundMode.OrderRefund && refundinfo.OrderItemInfo.OrderInfo.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.WaitDelivery))
                ResetActive = true;
            string DealTime = refundinfo.SellerAuditDate.ToString("yyyy-MM-dd HH:mm:ss");
            if (refundinfo.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited)
            {
                DealTime = refundinfo.ManagerConfirmDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
            List<OrderItemInfo> orderItems = new List<OrderItemInfo>();
            if (refundinfo.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
            {
                orderItems = order.OrderItemInfo.ToList();
            }
            else
            {
                orderItems.Add(refundinfo.OrderItemInfo);
            }
            OrderRefundGetReturnDetailModel result = new OrderRefundGetReturnDetailModel(true);
            result.Data = new ReturnDetail()
            {
                AdminRemark = (refundinfo.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed ? refundinfo.ManagerRemark : refundinfo.SellerRemark),
                ApplyForTime = refundinfo.ApplyDate.ToString("yyyy-MM-dd HH:mm:ss"),
                Remark = "",
                Status = refundinfo.RefundStatusValue.Value,
                StatusText = status,
                DealTime = DealTime,
                Operator = (refundinfo.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited ? "管理员" : "商家"),
                Reason = refundinfo.Reason,
                ReturnId = refundinfo.Id,
                OrderId = refundinfo.OrderId.ToString(),
                Quantity = refundinfo.ReturnQuantity.Value,
                RefundMoney = refundinfo.Amount.ToString("f2"),
                RefundType = refundinfo.RefundPayType.ToDescription(),
                ShopName = refundinfo.ShopName,
                BankAccountName = "",
                BankAccountNo = "",
                BankName = "",
                OrderTotal = order.RefundTotalAmount.ToString("f2"),
                CanResetActive = ResetActive,
                IsOrderRefundTimeOut = _iOrderService.IsRefundTimeOut(refundinfo.OrderId),
                SkuId = refundinfo.OrderItemInfo.SkuId,
                Cellphone = order != null ? order.CellPhone : "",
                ShipAddress = order != null ? order.RegionFullName + " " + order.Address : "",
                ShipTo = order != null ? order.ShipTo : "",
                ShipOrderNumber = refundinfo.ShipOrderNumber,
                IsOnlyRefund = refundinfo.ReturnQuantity == 0,
                UserCredentials = new List<string>(),
                UserSendGoodsTime = refundinfo.BuyerDeliverDate.HasValue ? refundinfo.BuyerDeliverDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                ConfirmGoodsTime = refundinfo.SellerConfirmArrivalDate.HasValue ? refundinfo.SellerConfirmArrivalDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : refundinfo.SellerAuditDate.ToString("yyyy-MM-dd HH:mm:ss"),
                FinishTime = refundinfo.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed ? refundinfo.ManagerConfirmDate.ToString("yyyy-MM-dd HH:mm:ss") : "",
                ProductInfo = orderItems.Select(l => new RefundDetailSKU
                {
                    ProductName = l.ProductName,
                    SKU = l.SKU,
                    SKUContent = l.Color + " " + l.Size + " " + l.Version,
                    Price = l.SalePrice.ToString("f2"),
                    Quantity = (int)l.Quantity,
                    ThumbnailsUrl = Core.HimallIO.GetRomoteProductSizeImage(l.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_150),
                }).ToList()
            };
            if (!string.IsNullOrWhiteSpace(refundinfo.CertPic1))
            {
                result.Data.UserCredentials.Add(Himall.Core.HimallIO.GetRomoteImagePath(refundinfo.CertPic1));
            }
            if (!string.IsNullOrWhiteSpace(refundinfo.CertPic2))
            {
                result.Data.UserCredentials.Add(Himall.Core.HimallIO.GetRomoteImagePath(refundinfo.CertPic2));
            }
            if (!string.IsNullOrWhiteSpace(refundinfo.CertPic3))
            {
                result.Data.UserCredentials.Add(Himall.Core.HimallIO.GetRomoteImagePath(refundinfo.CertPic3));
            }
            return Json(result);
        }
        /// <summary>
        /// 获取物流列表
        /// </summary>
        /// <returns></returns>
        public object GetExpressList()
        {
            var _iExpressService = ServiceProvider.Instance<IExpressService>.Create;
            var express = _iExpressService.GetAllExpress();
            var result = new OrderRefundGetExpressListModel(true);
            result.Data = express.Select(d => new ExpressItem
            {
                ExpressName = d.Name,
                Kuaidi100Code = d.Kuaidi100Code,
                TaobaoCode = d.TaobaoCode,
            }).ToList();
            return Json(result);
        }
        /// <summary>
        /// 售后检测
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public object AfterSalePreCheck(string openId, long OrderId, string SkuId = "", bool IsReturn = false)
        {
            CheckUserLogin();

            try
            {
                var result = new OrderRefundAfterSalePreCheckModel(true);
                var ordser = ServiceProvider.Instance<IOrderService>.Create;
                var refundser = ServiceProvider.Instance<IRefundService>.Create;
                //计算可退金额 预留
                ordser.CalculateOrderItemRefund(OrderId);
                var order = ordser.GetOrder(OrderId, CurrentUser.Id);
                if (order == null) throw new Himall.Core.HimallException("该订单已删除或不属于该用户");
                if ((int)order.OrderStatus < 2) throw new Himall.Core.HimallException("错误的售后申请,订单状态有误");
                result.CanBackReturn = order.CanBackOut();
                result.CanReturnOnStore = result.CanBackReturn;
                result.CanToBalance = true;
                if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || order.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                {
                    if (IsReturn)
                    {
                        throw new HimallException("订单状态不可以退货");
                    }
                    var _tmprefund = refundser.GetOrderRefundByOrderId(OrderId);
                    if (_tmprefund != null && _tmprefund.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.UnAudit)
                    {
                        throw new HimallException("售后中，不可激活");
                    }
                    result.MaxRefundQuantity = 0;
                    result.oneReundAmount = "0.00";
                    result.MaxRefundAmount = order.OrderEnabledRefundAmount.ToString("f2");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(SkuId))
                    {
                        throw new Himall.Core.HimallException("错误的参数:SkuId");
                    }
                    OrderItemInfo _ordItem = order.OrderItemInfo.FirstOrDefault(d => d.SkuId == SkuId);
                    if (_ordItem != null)
                    {
                        var _tmprefund = refundser.GetOrderRefundList(OrderId).FirstOrDefault(d => d.OrderItemId == _ordItem.Id);
                        if (_tmprefund != null && _tmprefund.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.UnAudit)
                        {
                            throw new HimallException("售后中，不可激活");
                        }
                    }
                    else
                    {
                        throw new Himall.Core.HimallException("错误的参数:SkuId");
                    }

                    result.MaxRefundQuantity = _ordItem.Quantity - _ordItem.ReturnQuantity;
                    result.oneReundAmount = (_ordItem.EnabledRefundAmount.Value / _ordItem.Quantity).ToString("f2");
                    result.MaxRefundAmount = (_ordItem.EnabledRefundAmount.Value - _ordItem.RefundPrice).ToString("f2");
                }
                var reasonlist = refundser.GetRefundReasons();
                result.RefundReasons = reasonlist;    //理由List
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new BaseResultModel(false) { Message = ex.Message });
            }
        }
        #endregion

        /// <summary>
        /// 申请订单退款
        /// </summary>
        /// <returns></returns>
        public object PostApplyRefund(RefundApplyRefundPModel model)
        {
            var result = new BaseResultModel(true);
            CheckUserLogin();

            try
            {
                var ordser = ServiceProvider.Instance<IOrderService>.Create;
                var refundser = ServiceProvider.Instance<IRefundService>.Create;

                //计算可退金额 预留
                ordser.CalculateOrderItemRefund(model.orderId);
                //参数处理
                if (!model.refundId.HasValue)
                {
                    var _tmprefund = refundser.GetOrderRefundByOrderId(model.orderId);
                    if (_tmprefund != null)
                    {
                        model.refundId = _tmprefund.Id;
                    }
                }

                OrderRefundInfo info = new OrderRefundInfo();

                #region 表单数据
                info.OrderId = model.orderId;
                if (null != model.OrderItemId)
                    info.OrderItemId = model.OrderItemId.Value;
                if (null != model.refundId)
                    info.Id = model.refundId.Value;
                info.RefundType = 1;
                info.ReturnQuantity = 0;
                info.Reason = model.RefundReason;
                info.ContactPerson = model.ContactPerson;
                info.ContactCellPhone = model.ContactCellPhone;
                info.RefundPayType = model.RefundType;

                info.formId = model.formId;
                #endregion

                #region 初始化售后单的数据
                var order = ordser.GetOrder(info.OrderId, CurrentUser.Id);
                info.Amount = order.OrderEnabledRefundAmount;
                if (order == null) throw new Himall.Core.HimallException("该订单已删除或不属于该用户");
                if ((int)order.OrderStatus < 2) throw new Himall.Core.HimallException("错误的售后申请,订单状态有误");
                if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || order.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                {
                    info.RefundMode = OrderRefundInfo.OrderRefundMode.OrderRefund;
                    info.ReturnQuantity = 0;
                }
                else
                {
                    throw new Himall.Core.HimallException("仅待发货或待自提订单可以申请订单退款");
                }
                if (info.RefundType == 1)
                {
                    info.ReturnQuantity = 0;
                    info.IsReturn = false;
                }
                if (info.ReturnQuantity < 0) throw new Himall.Core.HimallException("错误的退货数量");
                var orderitem = order.OrderItemInfo.FirstOrDefault(a => a.Id == info.OrderItemId);
                if (orderitem == null && info.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund) throw new Himall.Core.HimallException("该订单条目已删除或不属于该用户");
                if (info.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp) throw new Himall.Core.HimallException("错误的订单退款申请,订单状态有误");
                    info.IsReturn = false;
                    info.ReturnQuantity = 0;
                    if (info.Amount > order.OrderEnabledRefundAmount) throw new Himall.Core.HimallException("退款金额不能超过订单的实际支付金额");
                }
                else
                {
                    if (info.Amount > (orderitem.EnabledRefundAmount - orderitem.RefundPrice)) throw new Himall.Core.HimallException("退款金额不能超过订单的可退金额");
                    if (info.ReturnQuantity > (orderitem.Quantity - orderitem.ReturnQuantity)) throw new Himall.Core.HimallException("退货数量不可以超出可退数量");
                }
                info.IsReturn = false;
                if (info.ReturnQuantity > 0) info.IsReturn = true;
                if (info.RefundType == 2) info.IsReturn = true;
                if (info.IsReturn == true && info.ReturnQuantity < 1) throw new Himall.Core.HimallException("错误的退货数量");
                if (info.Amount <= 0) throw new Himall.Core.HimallException("错误的退款金额");
                info.ShopId = order.ShopId;
                info.ShopName = order.ShopName;
                info.UserId = CurrentUser.Id;
                info.Applicant = CurrentUser.UserName;
                info.ApplyDate = DateTime.Now;
                info.Reason = HTMLEncode(info.Reason.Replace("'", "‘").Replace("\"", "”"));
                #endregion
                //info.RefundAccount = HTMLEncode(info.RefundAccount.Replace("'", "‘").Replace("\"", "”"));

                if (info.Id > 0)
                {
                    refundser.ActiveRefund(info);
                }
                else
                {
                    refundser.AddOrderRefund(info);
                }
                result.SetStatus(true);
                result.Message = "成功的申请了退款";
                return Json(new { Status=result.Status,Message=result.Message});
            }
            catch (HimallException he)
            {
                result.SetStatus(false);
                result.Message = he.Message;
                return Json(new { Status = result.Status, Message = result.Message});
            }
            catch (Exception ex)
            {
                Log.Error("[SPAPI]Refund：" + ex.Message);
                result.SetStatus(false);
                result.Message = "系统内部异常";
                return Json(new { Status = result.Status, Message = result.Message });
            }
        }
        /// <summary>
        /// 订单项退货退款
        /// </summary>
        /// <returns></returns>
        public object PostApplyReturn(RefundApplyReturnPModel model)
        {
            var result = new BaseResultModel(true);
            CheckUserLogin();

            try
            {
                OrderRefundInfo info = new OrderRefundInfo();
                var ordser = ServiceProvider.Instance<IOrderService>.Create;
                var refundser = ServiceProvider.Instance<IRefundService>.Create;
                //计算可退金额 预留
                ordser.CalculateOrderItemRefund(model.orderId);
                var order = ordser.GetOrder(model.orderId, CurrentUser.Id);
                if (order == null) throw new Himall.Core.HimallException("该订单已删除或不属于该用户");
                if ((int)order.OrderStatus < 2) throw new Himall.Core.HimallException("错误的售后申请,订单状态有误");
                if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || order.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                {
                    info.RefundMode = OrderRefundInfo.OrderRefundMode.OrderRefund;
                    info.ReturnQuantity = 0;
                }
                info.formId = model.formId;
                //参数处理
                if (!model.refundId.HasValue)  // 通过skuid获取售后编号
                {
                    if (info.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                    {
                        var _tmprefund = refundser.GetOrderRefundByOrderId(model.orderId);
                        if (_tmprefund != null)
                        {
                            model.refundId = _tmprefund.Id;
                        }
                    }
                    else
                    {
                        if (!model.OrderItemId.HasValue)
                        {
                            if (string.IsNullOrWhiteSpace(model.skuId))
                            {
                                throw new Himall.Core.HimallException("参数错误");
                            }
                            foreach (var item in order.OrderItemInfo)
                            {
                                if (item.SkuId == model.skuId)
                                {
                                    model.OrderItemId = item.Id;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (info.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund)
                    {
                        var _tmprefund = refundser.GetOrderRefund(model.refundId.Value);
                        if (_tmprefund == null)
                        {
                            throw new Himall.Core.HimallException("参数错误");
                        }
                        model.OrderItemId = _tmprefund.OrderItemId;

                    }
                }

                #region 表单数据
                info.OrderId = model.orderId;
                if (null != model.OrderItemId)
                    info.OrderItemId = model.OrderItemId.Value;
                if (null != model.refundId)
                    info.Id = model.refundId.Value;
                info.RefundType = model.Quantity > 0 ? 2 : 1;
                info.ReturnQuantity = model.Quantity;
                info.Amount = model.RefundAmount;
                info.Reason = model.RefundReason;
                info.ContactPerson = model.ContactPerson;
                info.ContactCellPhone = model.ContactCellPhone;
                info.RefundPayType = model.RefundType;
                #endregion

                #region 初始化售后单的数据
                if (info.RefundType == 1)
                {
                    info.ReturnQuantity = 0;
                    info.IsReturn = false;
                }
                if (info.ReturnQuantity < 0) throw new Himall.Core.HimallException("错误的退货数量");
                var orderitem = order.OrderItemInfo.FirstOrDefault(a => a.Id == info.OrderItemId);
                if (orderitem == null && info.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund) throw new Himall.Core.HimallException("该订单条目已删除或不属于该用户");
                if (info.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp) throw new Himall.Core.HimallException("错误的订单退款申请,订单状态有误");
                    info.IsReturn = false;
                    info.ReturnQuantity = 0;
                    if (info.Amount > order.OrderEnabledRefundAmount) throw new Himall.Core.HimallException("退款金额不能超过订单的实际支付金额");
                }
                else
                {
                    if (info.Amount > (orderitem.EnabledRefundAmount - orderitem.RefundPrice)) throw new Himall.Core.HimallException("退款金额不能超过订单的可退金额");
                    if (info.ReturnQuantity > (orderitem.Quantity - orderitem.ReturnQuantity)) throw new Himall.Core.HimallException("退货数量不可以超出可退数量");
                }
                info.IsReturn = false;
                if (info.ReturnQuantity > 0) info.IsReturn = true;
                if (info.RefundType == 2) info.IsReturn = true;
                if (info.IsReturn == true && info.ReturnQuantity < 1) throw new Himall.Core.HimallException("错误的退货数量");
                if (info.Amount <= 0) throw new Himall.Core.HimallException("错误的退款金额");
                info.ShopId = order.ShopId;
                info.ShopName = order.ShopName;
                info.UserId = CurrentUser.Id;
                info.Applicant = CurrentUser.UserName;
                info.ApplyDate = DateTime.Now;
                info.Reason = HTMLEncode(info.Reason.Replace("'", "‘").Replace("\"", "”"));
                #endregion
                if (!string.IsNullOrEmpty(model.UserCredentials))
                {
                    var certPics = model.UserCredentials.Split(',');
                    switch(certPics.Length)
                    {
                        case 1:
                            info.CertPic1 = MoveImages(certPics[0], CurrentUser.Id, info.OrderItemId);
                            break;
                        case 2:
                            info.CertPic1 = MoveImages(certPics[0], CurrentUser.Id, info.OrderItemId);
                            info.CertPic2 = MoveImages(certPics[1], CurrentUser.Id, info.OrderItemId);
                            break;
                        case 3:
                            info.CertPic1 = MoveImages(certPics[0], CurrentUser.Id, info.OrderItemId);
                            info.CertPic2 = MoveImages(certPics[1], CurrentUser.Id, info.OrderItemId);
                            info.CertPic3 = MoveImages(certPics[2], CurrentUser.Id, info.OrderItemId);
                            break;
                    }
                }
                //info.RefundAccount = HTMLEncode(info.RefundAccount.Replace("'", "‘").Replace("\"", "”"));

                if (info.Id > 0)
                {
                    refundser.ActiveRefund(info);
                }
                else
                {
                    refundser.AddOrderRefund(info);
                }
                result.SetStatus(true);
                result.Message = "成功的申请了售后";
                return Json(new { Status = result.Status, Message = result.Message });
            }
            catch (HimallException he)
            {
                result.SetStatus(false);
                result.Message = he.Message;
                return Json(new { Status = result.Status, Message = result.Message });
            }
            catch (Exception ex)
            {
                Log.Error("[SPAPI]Refund：" + ex.Message);
                result.SetStatus(false);
                result.Message = "系统内部异常";
                return Json(new { Status = result.Status, Message = result.Message });
            }
        }
        private string MoveImages(string image, long userId, long itemid)
        {
            if (!string.IsNullOrWhiteSpace(image))
            {
                var ext = Path.GetFileName(image);
                string ImageDir = string.Empty;
                //转移图片
                string relativeDir = "/Storage/Plat/Refund/" + userId.ToString() + "/";
                string fileName = itemid.ToString() + "_" + DateTime.Now.ToString("yyMMddHHmmssffff") + ext;
                if (image.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
                {
                    var de = image.Substring(image.LastIndexOf("/temp/"));
                    Core.HimallIO.CopyFile(de, relativeDir + fileName, true);
                    return relativeDir + fileName;
                }  //目标地址
                else if (image.Contains("/Storage/"))
                {
                    return image.Substring(image.LastIndexOf("/Storage/"));
                }

                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="context"></param>
        public object PostUploadAppletImage()
        {
            CheckUserLogin();
            IList<string> images = new List<string>();
            HttpFileCollection files = HttpContext.Current.Request.Files;
            if (files != null)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    HttpPostedFile file = files[i];

                    string filename = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + ".png";
                    var fname = "/temp/" + filename;
                    var ioname = Core.HimallIO.GetImagePath(fname);
                    try
                    {
                        Core.HimallIO.CreateFile(fname, file.InputStream);
                        images.Add(ioname);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("WeChatApplet_FileUpload_Error:" + ex.Message);
                        images.Add("upload error");
                    }
                }
            }
            return Json(new
            {
                Status = "OK",
                Count = images.Count,
                Data = images.Select(c => new
                {
                    ImageUrl = c,
                })
            });
        }

        /// <summary>
        /// 退货
        /// </summary>
        /// <param name="id"></param>
        /// <param name="expressCompanyName"></param>
        /// <param name="shipOrderNumber"></param>
        /// <returns></returns>
        public object PostReturnSendGoods(RefundReturnSendGoodsPModel model)
        {
            CheckUserLogin();
            var refundser = ServiceProvider.Instance<IRefundService>.Create;
            refundser.UserConfirmRefundGood(model.ReturnsId, CurrentUser.UserName, model.express, model.shipOrderNumber);

            var result = new BaseResultModel(true);
            result.Message = "发货成功！";
            return Json(result);
        }

        private string HTMLEncode(string txt)
        {
            if (string.IsNullOrEmpty(txt))
                return string.Empty;
            string Ntxt = txt;

            Ntxt = Ntxt.Replace(" ", "&nbsp;");

            Ntxt = Ntxt.Replace("<", "&lt;");

            Ntxt = Ntxt.Replace(">", "&gt;");

            Ntxt = Ntxt.Replace("\"", "&quot;");

            Ntxt = Ntxt.Replace("'", "&#39;");

            //Ntxt = Ntxt.Replace("\n", "<br>");

            return Ntxt;

        }
    }
}
