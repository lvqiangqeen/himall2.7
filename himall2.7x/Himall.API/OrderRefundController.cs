using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Himall.Web;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Himall.IServices.QueryModel;
using Himall.API.Model;
using Himall.API.Model.ParamsModel;

namespace Himall.API
{
    public class OrderRefundController : BaseApiController
    {
        /// <summary>
        /// 获取申请售后的信息
        /// </summary>
        /// <param name="id">订单ID</param>
        /// <param name="itemId">子订单ID</param>
        /// <returns></returns>
        public object GetOrderRefundModel(long id, long? itemId = null, long? refundId = null)
        {
            CheckUserLogin();
            try
            {
                dynamic d = new System.Dynamic.ExpandoObject();
                var ordser = ServiceProvider.Instance<IOrderService>.Create;
                var refundser = ServiceProvider.Instance<IRefundService>.Create;

                var order = ordser.GetOrder(id, CurrentUser.Id);
                if (order == null)
                    throw new Himall.Core.HimallException("该订单已删除或不属于该用户");
                if ((int)order.OrderStatus < 2)
                    throw new Himall.Core.HimallException("错误的售后申请,订单状态有误");
                if (itemId == null && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                    throw new Himall.Core.HimallException("错误的订单退款申请,订单状态有误");
                //计算可退金额 预留
                ordser.CalculateOrderItemRefund(id);

                OrderRefundModel refundModel = new OrderRefundModel();
                OrderItemInfo item = new OrderItemInfo();
                refundModel.MaxRGDNumber = 0;
                refundModel.MaxRefundAmount = order.OrderEnabledRefundAmount;
                if (itemId == null)
                {
                    item = order.OrderItemInfo.FirstOrDefault();
                }
                else
                {
                    item = order.OrderItemInfo.Where(a => a.Id == itemId).FirstOrDefault();
                    refundModel.MaxRGDNumber = item.Quantity - item.ReturnQuantity;
                    refundModel.MaxRefundAmount = (decimal)(item.EnabledRefundAmount - item.RefundPrice);
                }

                bool isCanApply = false;

                if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery)
                {
                    isCanApply = refundser.CanApplyRefund(id, item.Id);
                }
                else
                {
                    isCanApply = refundser.CanApplyRefund(id, item.Id, false);
                }

                if (!refundId.HasValue)
                {
                    if (!isCanApply)
                        throw new Himall.Core.HimallException("您已申请过售后，不可重复申请");
                    d.ContactPerson = CurrentUser.RealName;
                    d.ContactCellPhone = CurrentUser.CellPhone;
                    d.OrderItemId = itemId;
                    d.RefundType = 0;
                    d.IsRefundOrder = false;
                    if (!itemId.HasValue)
                    {
                        d.IsRefundOrder = true;
                        d.RefundType = 1;
                    }
                    var reasonlist = refundser.GetRefundReasons();
                    d.Id = order.Id;
                    d.MaxRGDNumber = refundModel.MaxRGDNumber;
                    d.MaxRefundAmount = refundModel.MaxRefundAmount;
                    d.OrderStatus = order.OrderStatus.ToDescription();
                    d.OrderStatusValue = (int)order.OrderStatus;
                    d.BackOut = false;
                    d.RefundReasons = reasonlist;
                    if (order.CanBackOut())
                        d.BackOut = true;
                }
                else
                {
                    var refunddata = refundser.GetOrderRefund(refundId.Value, CurrentUser.Id);
                    if (refunddata == null)
                    {
                        throw new Himall.Core.HimallException("错误的售后数据");
                    }
                    if (refunddata.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.UnAudit)
                    {
                        throw new Himall.Core.HimallException("错误的售后状态，不可激活");
                    }
                    d.ContactPerson = refunddata.ContactPerson;
                    d.ContactCellPhone = refunddata.ContactCellPhone;
                    d.OrderItemId = refunddata.OrderItemId;
                    d.IsRefundOrder = (refunddata.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund);
                    d.RefundType = (refunddata.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund ? 1 : 0);
                    var reasonlist = refundser.GetRefundReasons();
                    d.RefundReasons = reasonlist;    //理由List
                    d.Id = id;
                    d.MaxRGDNumber = refundModel.MaxRGDNumber;
                    d.MaxRefundAmount = refundModel.MaxRefundAmount;
                    d.OrderStatus = order.OrderStatus.ToDescription();
                    d.OrderStatusValue = (int)order.OrderStatus;
                    d.BackOut = false;
                    if (order.CanBackOut())
                        d.BackOut = true;
                }

                if (!d.IsRefundOrder && item.EnabledRefundAmount.HasValue)
                {
                    d.RefundGoodsPrice = item.EnabledRefundAmount.Value / item.Quantity;
                }
                return Json(new { Success = true, RefundMode = d });
            }
            catch (HimallException himallex)
            {
                return Json(new { Success = false, Msg = himallex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Msg = "系统异常：" + ex.Message });
            }
        }

        /// <summary>
        /// 提交退款/售后申请
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object PostRefundApply(OrderRefundApplyModel value)
        {
            CheckUserLogin();
            try
            {
                var ordser = ServiceProvider.Instance<IOrderService>.Create;
                var refundser = ServiceProvider.Instance<IRefundService>.Create;
                OrderRefundInfo info = new OrderRefundInfo();
                #region 表单数据
                info.OrderId = value.OrderId;
                if (null != value.OrderItemId)
                    info.OrderItemId = value.OrderItemId.Value;
                if (null != value.refundId)
                    info.Id = value.refundId.Value;
                info.RefundType = value.RefundType;
                info.ReturnQuantity = value.ReturnQuantity;
                info.Amount = value.Amount;
                info.Reason = value.Reason;
                info.ContactPerson = value.ContactPerson;
                info.ContactCellPhone = value.ContactCellPhone;
                info.RefundPayType = value.RefundPayType;
                #endregion

                #region 初始化售后单的数据
                var order = ordser.GetOrder(info.OrderId, CurrentUser.Id);
                if (order == null) throw new Himall.Core.HimallException("该订单已删除或不属于该用户");
                if ((int)order.OrderStatus < 2) throw new Himall.Core.HimallException("错误的售后申请,订单状态有误");
                if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || order.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                {
                    info.RefundMode = OrderRefundInfo.OrderRefundMode.OrderRefund;
                    info.ReturnQuantity = 0;
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
                    if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery  && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp) throw new Himall.Core.HimallException("错误的订单退款申请,订单状态有误");
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
                return Json(new { success = true, msg = "提交成功", id = info.Id });
            }
            catch (HimallException he)
            {
                return Json(new { success = false, msg = he.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "系统异常：" + ex.Message });
            }
        }

        /// <summary>
        /// 获取退款/售后列表
        /// </summary>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns></returns>
        public object GetRefundList(int pageNo, int pageSize)
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
                PageNo = pageNo,
                ShowRefundType = 0
            };
            var refunds = refundser.GetOrderRefunds(queryModel);
            var list = refunds.Models.Select(item =>
            {
                var vshop = vshopser.GetVShopByShopId(item.ShopId) ?? new VShopInfo() { Id = 0 };
                var order = orderser.GetOrder(item.OrderId, CurrentUser.Id);
                var status = item.RefundStatus;
                if (order != null && (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0))
                    status = status.Replace("商家", "门店");

                IEnumerable<OrderItemInfo> orderItems = null;
                if (item.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    orderItems = orderser.GetOrder(item.OrderId, CurrentUser.Id).OrderItemInfo.ToList();
                }
                return new
                {
                    ShopName = item.ShopName,
                    Vshopid = vshop.Id,
                    RefundStatus = status,
                    Id = item.Id,
                    ProductName = item.OrderItemInfo.ProductName,
                    EnabledRefundAmount = item.EnabledRefundAmount,
                    Amount = item.Amount,
                    Img = Core.HimallIO.GetRomoteProductSizeImage(item.OrderItemInfo.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_350),
                    ShopId = item.ShopId,
                    RefundMode = item.RefundMode.ToDescription(),
                    RefundModeValue = (int)item.RefundMode,
                    OrderId = item.OrderId,
                    OrderItems = orderItems != null ? orderItems.Select(e => new
                    {
                        ThumbnailsUrl = Core.HimallIO.GetRomoteProductSizeImage(e.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_350),
                        ProductName = e.ProductName
                    }) : null,
                    SellerAuditStatus = item.SellerAuditStatus.ToDescription(),
                    SellerAuditStatusValue = (int)item.SellerAuditStatus,
                };
            });

            return Json(new { Total = refunds.Total, Data = list, Success = true });
        }

        /// <summary>
        /// 获取 退款/售后 详情
        /// </summary>
        /// <param name="id">退款/售后ID</param>
        /// <returns></returns>
        public object GetRefundDetail(long id)
        {
            CheckUserLogin();
            var _iOrderService = ServiceProvider.Instance<IOrderService>.Create;
            var refundser = ServiceProvider.Instance<IRefundService>.Create;
            var refundinfo = refundser.GetOrderRefund(id, CurrentUser.Id);
            var order = _iOrderService.GetOrder(refundinfo.OrderId, CurrentUser.Id);
            dynamic d = new System.Dynamic.ExpandoObject();
            d.ManagerConfirmStatus = refundinfo.ManagerConfirmStatus.ToDescription();
            d.ManagerConfirmStatusValue = (int)refundinfo.ManagerConfirmStatus;
            d.ManagerConfirmDate = refundinfo.ManagerConfirmDate;
            d.SellerAuditStatus = refundinfo.SellerAuditStatus.ToDescription();
            d.SellerAuditStatusValue = (int)refundinfo.SellerAuditStatus;
            d.SellerRemark = refundinfo.SellerRemark;
            d.SellerAuditDate = refundinfo.SellerAuditDate;
            d.RefundStatus = refundinfo.RefundStatus;
            d.Amount = refundinfo.Amount;
            d.Id = refundinfo.Id;
            d.OrderId = refundinfo.OrderId;
            d.OrderItemId = refundinfo.OrderItemId;
            d.ShopName = refundinfo.ShopName;
            d.RefundMode = refundinfo.RefundMode.ToDescription();
            d.RefundModeValue = (int)refundinfo.RefundMode;
            d.ReturnQuantity = refundinfo.ReturnQuantity;
            d.RefundPayType = refundinfo.RefundPayType.ToDescription();
            d.RefundPayTypeValue = (int)refundinfo.RefundPayType;
            d.Reason = refundinfo.Reason;
            d.ApplyDate = refundinfo.ApplyDate;
            d.IsOrderRefundTimeOut = _iOrderService.IsRefundTimeOut(refundinfo.OrderId);
            d.LastConfirmDate = (refundinfo.ManagerConfirmDate > refundinfo.SellerAuditDate ? refundinfo.ManagerConfirmDate : refundinfo.SellerAuditDate);
            if ((refundinfo.RefundMode != Himall.Model.OrderRefundInfo.OrderRefundMode.OrderRefund) || (refundinfo.RefundMode == Himall.Model.OrderRefundInfo.OrderRefundMode.OrderRefund && refundinfo.OrderItemInfo.OrderInfo.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.WaitDelivery))
                d.ResetActive = true;
            else
                d.ResetActive = false;
            if(order!=null&&(order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0))
                d.RefundStatus=d.RefundStatus.Replace("商家", "门店");
            return d;
        }

        /// <summary>
        /// 获取 退款/售后进程 详情
        /// </summary>
        /// <param name="id">退款/售后ID</param>
        /// <returns></returns>
        public object GetRefundProcessDetail(long id)
        {
            CheckUserLogin();
            var refundser = ServiceProvider.Instance<IRefundService>.Create;
            var refundinfo = refundser.GetOrderRefund(id, CurrentUser.Id);
            //是否弃货
            var isDiscard = false;
            if (refundinfo.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited
                && refundinfo.BuyerDeliverDate == null
                && refundinfo.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund
                && refundinfo.IsReturn == true
               )
            {
                isDiscard = true;
            }
            //是否拒绝
            bool isUnAudit = (refundinfo.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.UnAudit);
            //是否退货
            bool isReturnGoods = (refundinfo.RefundMode == OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund);

            dynamic d = new System.Dynamic.ExpandoObject();
            d.IsDiscard = isDiscard;
            d.IsUnAudit = isUnAudit;
            d.IsReturnGoods = isReturnGoods;

            d.ManagerConfirmStatus = refundinfo.ManagerConfirmStatus.ToDescription();
            d.ManagerConfirmStatusValue = (int)refundinfo.ManagerConfirmStatus;
            d.ManagerConfirmDate = refundinfo.ManagerConfirmDate;
            d.ManagerRemark = refundinfo.ManagerRemark;

            d.SellerAuditStatus = refundinfo.SellerAuditStatus.ToDescription();
            d.SellerAuditStatusValue = (int)refundinfo.SellerAuditStatus;
            d.SellerConfirmArrivalDate = refundinfo.SellerConfirmArrivalDate;
            d.SellerRemark = refundinfo.SellerRemark;
            d.SellerAuditDate = refundinfo.SellerAuditDate;

            d.BuyerDeliverDate = refundinfo.BuyerDeliverDate;
            d.ExpressCompanyName = refundinfo.ExpressCompanyName;
            d.ShipOrderNumber = refundinfo.ShipOrderNumber;
            d.ApplyDate = refundinfo.ApplyDate;

            int curappnum = refundinfo.ApplyNumber.HasValue ? refundinfo.ApplyNumber.Value : 1;
            var log = refundser.GetRefundLogs(refundinfo.Id, curappnum, false);
            d.RefundLogs = log;
            return d;
        }

        public static string HTMLEncode(string txt)
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

        public object PostSellerSendGoods(OrderRefundSellerSendGoodsModel value)
        {
            CheckUserLogin();
            var refundser = ServiceProvider.Instance<IRefundService>.Create;
            long id = value.Id;
            string expressCompanyName = value.ExpressCompanyName;
            string shipOrderNumber = value.ShipOrderNumber;
            refundser.UserConfirmRefundGood(id, CurrentUser.UserName, expressCompanyName, shipOrderNumber);
            return Json(new { success = true, msg = "提交成功" });
        }
    }
}
