using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Himall.API.Model.ParamsModel;
using Himall.Application;
using Himall.DTO;
using Himall.API.Model;

namespace Himall.API
{
    public class MemberOrderController : BaseApiController
    {
        public object GetOrders(int? orderStatus, int pageNo, int pageSize = 8)
        {
            CheckUserLogin();
            var allOrders = ServiceProvider.Instance<IOrderService>.Create.GetTopOrders(int.MaxValue, CurrentUser.Id);
            var orderService = ServiceProvider.Instance<IOrderService>.Create;

            //待评价
            var queryModelAll = new OrderQuery()
            {
                Status = OrderInfo.OrderOperateStatus.Finish,
                UserId = CurrentUser.Id,
                PageSize = int.MaxValue,
                PageNo = 1,
                Commented = false
            };
            var allOrderCounts = allOrders.Count();
            var waitingForComments = orderService.GetOrders<OrderInfo>(queryModelAll).Total;
            var waitingForRecieve = allOrders.Count(item => item.OrderStatus == OrderInfo.OrderOperateStatus.WaitReceiving);//获取待收货订单数
            var waitingForPay = allOrders.Count(item => item.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay);//获取待支付订单数

            if (orderStatus.HasValue && orderStatus == 0)
            {
                orderStatus = null;
            }
            var queryModel = new OrderQuery()
            {
                Status = (OrderInfo.OrderOperateStatus?)orderStatus,
                UserId = CurrentUser.Id,
                PageSize = pageSize,
                PageNo = pageNo
            };
            if (queryModel.Status.HasValue && queryModel.Status.Value == OrderInfo.OrderOperateStatus.WaitReceiving)
            {
                if (queryModel.MoreStatus == null)
                {
                    queryModel.MoreStatus = new List<OrderInfo.OrderOperateStatus>() { };
                }
                queryModel.MoreStatus.Add(OrderInfo.OrderOperateStatus.WaitSelfPickUp);
            }
            if (orderStatus.GetValueOrDefault() == (int)OrderInfo.OrderOperateStatus.Finish)
                queryModel.Commented = false;//只查询未评价的订单
            ObsoletePageModel<OrderInfo> orders = orderService.GetOrders<OrderInfo>(queryModel);
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var vshopService = ServiceProvider.Instance<IVShopService>.Create;
            var orderRefundService = ServiceProvider.Instance<IRefundService>.Create;
            var orderItems = OrderApplication.GetOrderItemsByOrderId(orders.Models.Select(p => p.Id));
            var orderRefunds = OrderApplication.GetOrderRefunds(orderItems.Select(p => p.Id));
            var result = orders.Models.ToArray().Select(item =>
            {
                if (item.OrderStatus >= OrderInfo.OrderOperateStatus.WaitDelivery)
                {
                    orderService.CalculateOrderItemRefund(item.Id);
                }
                var vshop = vshopService.GetVShopByShopId(item.ShopId);
                var _ordrefobj = orderRefundService.GetOrderRefundByOrderId(item.Id) ?? new OrderRefundInfo { Id = 0 };
                if (item.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery && item.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                {
                    _ordrefobj = new OrderRefundInfo { Id = 0 };
                }
                int? ordrefstate = (_ordrefobj == null ? null : (int?)_ordrefobj.SellerAuditStatus);
                ordrefstate = (ordrefstate > 4 ? (int?)_ordrefobj.ManagerConfirmStatus : ordrefstate);
                return new
                {
                    id = item.Id,
                    status = item.OrderStatus.ToDescription(),
                    orderStatus = item.OrderStatus,
                    orderType = item.OrderType,
                    orderTypeName = item.OrderType.ToDescription(),
                    shopname = item.ShopName,
                    vshopId = vshop == null ? 0 : vshop.Id,
                    orderTotalAmount = item.OrderTotalAmount.ToString("F2"),
                    productCount = item.OrderProductQuantity,
                    commentCount = item.OrderCommentInfo.Count(),
                    pickupCode = item.PickupCode,
                    EnabledRefundAmount = item.OrderEnabledRefundAmount,
                    itemInfo = item.OrderItemInfo.Select(a =>
                    {
                        var prodata = productService.GetProduct(a.ProductId);
                        ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(prodata.TypeId);
                        string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                        string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                        string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

                        var itemrefund = orderRefunds.Where(or => or.OrderItemId == a.Id).FirstOrDefault(d => d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                        int? itemrefstate = (itemrefund == null ? null : (int?)itemrefund.SellerAuditStatus);
                        itemrefstate = (itemrefstate > 4 ? (int?)itemrefund.ManagerConfirmStatus : itemrefstate);

                        return new
                        {
                            productId = a.ProductId,
                            productName = a.ProductName,
                            image = Core.HimallIO.GetRomoteProductSizeImage(a.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_350),
                            count = a.Quantity,
                            price = a.SalePrice,
                            Unit = prodata == null ? "" : prodata.MeasureUnit,
                            color = a.Color,
                            size = a.Size,
                            version = a.Version,
                            ColorAlias = colorAlias,
                            SizeAlias = sizeAlias,
                            VersionAlias = versionAlias,
                            RefundStats = itemrefstate,
                            OrderRefundId = (itemrefund == null ? 0 : itemrefund.Id),
                            EnabledRefundAmount = a.EnabledRefundAmount
                        };
                    }),
                    RefundStats = ordrefstate,
                    OrderRefundId = _ordrefobj.Id,
                    HasExpressStatus = !string.IsNullOrWhiteSpace(item.ShipOrderNumber),
                    HasAppendComment = HasAppendComment(item),
                    Invoice = item.InvoiceType.ToDescription(),
                    InvoiceValue = (int)item.InvoiceType,
                    InvoiceContext = item.InvoiceContext,
                    InvoiceTitle = item.InvoiceTitle,
                    PaymentType = item.PaymentType.ToDescription(),
                    PaymentTypeValue = (int)item.PaymentType,
                    CanRefund = (item.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.WaitDelivery || item.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                    && !item.RefundStats.HasValue && item.PaymentType != Himall.Model.OrderInfo.PaymentTypes.CashOnDelivery && item.PaymentType != Himall.Model.OrderInfo.PaymentTypes.None
                    && (item.FightGroupCanRefund == null || item.FightGroupCanRefund == true) && ordrefstate.GetValueOrDefault().Equals(0)
                };
            });
            return Json(new { Success = "true", AllOrderCounts = allOrderCounts, WaitingForComments = waitingForComments, WaitingForRecieve = waitingForRecieve, WaitingForPay = waitingForPay, Orders = result });
        }

        private bool HasAppendComment(OrderInfo list)
        {
            var item = list.OrderItemInfo.FirstOrDefault();
            var result = ServiceProvider.Instance<ICommentService>.Create.HasAppendComment(item.Id);
            return result;
        }

        public object GetOrderDetail(long id)
        {
            CheckUserLogin();
            OrderInfo order = ServiceProvider.Instance<IOrderService>.Create.GetOrder(id, CurrentUser.Id);

            var orderService = ServiceProvider.Instance<IOrderService>.Create;
            var bonusService = ServiceProvider.Instance<IShopBonusService>.Create;
            var orderRefundService = ServiceProvider.Instance<IRefundService>.Create;
            var bonusmodel = bonusService.GetGrantByUserOrder(id, CurrentUser.Id);
            bool hasBonus = bonusmodel != null ? true : false;
            string shareHref = "";
            string shareTitle = "";
            string shareDetail = "";
            if (hasBonus)
            {
                shareHref = "http://" + HttpContext.Current.Request.Url.Host + "/m-weixin/shopbonus/index/" + bonusService.GetGrantIdByOrderId(id);
                shareTitle = bonusmodel.Himall_ShopBonus.ShareTitle;
                shareDetail = bonusmodel.Himall_ShopBonus.ShareDetail;
            }
            var shopService = ServiceProvider.Instance<IShopService>.Create;
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var vshop = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(order.ShopId);
            bool isCanApply = false;

            var customerServices = CustomerServiceApplication.GetMobileCustomerService(order.ShopId);
            var meiqia = CustomerServiceApplication.GetPreSaleByShopId(order.ShopId).FirstOrDefault(p => p.Tool == CustomerServiceInfo.ServiceTool.MeiQia);
            if (meiqia != null)
                customerServices.Insert(0, meiqia);

            //获取订单商品项数据
            var orderDetail = new
            {
                ShopName = shopService.GetShop(order.ShopId).ShopName,
                ShopId = order.ShopId,
                OrderItems = order.OrderItemInfo.Select(item =>
                {
                    var productinfo = productService.GetProduct(item.ProductId);
                    if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery)
                    {
                        isCanApply = orderRefundService.CanApplyRefund(id, item.Id);
                    }
                    else
                    {
                        isCanApply = orderRefundService.CanApplyRefund(id, item.Id, false);
                    }

                    ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(productinfo.TypeId);
                    string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                    return new
                    {
                        ItemId = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Count = item.Quantity,
                        Price = item.SalePrice,
                        //ProductImage = "http://" + Url.Request.RequestUri.Host + productService.GetProduct(item.ProductId).GetImage(ProductInfo.ImageSize.Size_100),
                        ProductImage = Core.HimallIO.GetRomoteProductSizeImage(productService.GetProduct(item.ProductId).RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_100),
                        color = item.Color,
                        size = item.Size,
                        version = item.Version,
                        IsCanRefund = isCanApply,
                        ColorAlias = colorAlias,
                        SizeAlias = sizeAlias,
                        VersionAlias = versionAlias,
                        EnabledRefundAmount = item.EnabledRefundAmount,
                    };
                })
            };
            //取拼团订单状态
            var fightGroupOrderInfo = ServiceProvider.Instance<IFightGroupService>.Create.GetFightGroupOrderStatusByOrderId(order.Id);
            var orderModel = new
            {
                Id = order.Id,
                OrderType = order.OrderType,
                OrderTypeName = order.OrderType.ToDescription(),
                Status = order.OrderStatus.ToDescription(),
                JoinStatus = fightGroupOrderInfo == null ? -2 : fightGroupOrderInfo.JoinStatus,
                ShipTo = order.ShipTo,
                Phone = order.CellPhone,
                Address = order.RegionFullName + " " + order.Address,
                HasExpressStatus = !string.IsNullOrWhiteSpace(order.ShipOrderNumber),
                ExpressCompanyName = order.ExpressCompanyName,
                Freight = order.Freight,
                IntegralDiscount = order.IntegralDiscount,
                RealTotalAmount = order.OrderTotalAmount,
                RefundTotalAmount = order.RefundTotalAmount,
                OrderDate = order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                ShopName = order.ShopName,
                VShopId = vshop == null ? 0 : vshop.Id,
                commentCount = order.OrderCommentInfo.Count(),
                ShopId = order.ShopId,
                orderStatus = (int)order.OrderStatus,
                Invoice = order.InvoiceType.ToDescription(),
                InvoiceValue = (int)order.InvoiceType,
                InvoiceContext = order.InvoiceContext,
                InvoiceTitle = order.InvoiceTitle,
                PaymentType = order.PaymentType.ToDescription(),
                PaymentTypeValue = (int)order.PaymentType,
                FullDiscount = order.FullDiscount,
                DiscountAmount = order.DiscountAmount,
                OrderRemarks = string.IsNullOrEmpty(order.OrderRemarks) ? "" : order.OrderRemarks,
                HasBonus = hasBonus,
                ShareHref = shareHref,
                ShareTitle = shareTitle,
                ShareDetail = shareDetail,
                IsCanRefund = (orderDetail.OrderItems.Where(e => e.IsCanRefund == false).Count() == 0) && !orderService.IsRefundTimeOut(order.Id),
                EnabledRefundAmount = order.OrderEnabledRefundAmount,
                HasAppendComment = HasAppendComment(order),
                SelfTake = order.DeliveryType == Himall.CommonModel.Enum.DeliveryType.SelfTake ? 1 : 0
            };
            #region 门店配送信息
            Himall.DTO.ShopBranch storeInfo = null;
            if (order.ShopBranchId.HasValue && order.ShopBranchId > 0)
            {
                storeInfo = Application.ShopBranchApplication.GetShopBranchById(order.ShopBranchId.Value);
            }
            #endregion
            return Json(new { Success = "true", Order = orderModel, OrderItem = orderDetail.OrderItems, StoreInfo = storeInfo, CustomerServices = customerServices });
        }

        public object GetExpressInfo(long orderId)
        {
            CheckUserLogin();
            OrderInfo order = ServiceProvider.Instance<IOrderService>.Create.GetOrder(orderId, CurrentUser.Id);
            var expressData = ServiceProvider.Instance<IExpressService>.Create.GetExpressData(order.ExpressCompanyName, order.ShipOrderNumber);

            if (expressData.Success)
                expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列
            var json = new
            {
                Success = expressData.Success,
                Msg = expressData.Message,
                Data = expressData.ExpressDataItems.Select(item => new
                {
                    time = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    content = item.Content
                })
            };
            return Json(new { Success = "true", ExpressNum = order.ShipOrderNumber, ExpressCompanyName = order.ExpressCompanyName, Comment = json });
        }

        //确认收货
        public object PostConfirmOrder(MemberOrderConfirmOrderModel value)
        {
            CheckUserLogin();
            long orderId = value.orderId;
            ServiceProvider.Instance<IOrderService>.Create.MembeConfirmOrder(orderId, CurrentUser.UserName);
            // var data = ServiceProvider.Instance<IOrderService>.Create.GetOrder(orderId);
            //确认收货写入结算表(修改LH的方法)
            // ServiceProvider.Instance<IOrderService>.Create.WritePendingSettlnment(data);
            return Json(new { Success = "true" });
        }

        //取消订单
        public object PostCloseOrder(MemberOrderCloseOrderModel value)
        {
            CheckUserLogin();
            long orderId = value.orderId;
            var order = ServiceProvider.Instance<IOrderService>.Create.GetOrder(orderId, CurrentUser.Id);
            if (order != null)
            {
                //拼团处理
                if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
                {
                    return Json(new { Success = "false", ErrorMsg = "拼团订单，会员不能取消！" });
                }
                ServiceProvider.Instance<IOrderService>.Create.MemberCloseOrder(orderId, CurrentUser.UserName);
            }
            else
            {
                return Json(new { Success = "false", ErrorMsg = "取消失败，该订单已删除或者不属于当前用户！" });
            }
            return Json(new { Success = "true" });
        }
        /// <summary>
        /// 订单提货码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public object GetPickupGoods(long id)
        {
            CheckUserLogin();
            var orderInfo = OrderApplication.GetOrder(id);
            if (orderInfo == null)
                return Json(new { Success = "false", ErrorMsg = "订单不存在！" });
            if (orderInfo.UserId != CurrentUser.Id)
                return Json(new { Success = "false", ErrorMsg = "只能查看自己的提货码！" });
            var productService = ServiceProvider.Instance<IProductService>.Create;
            AutoMapper.Mapper.CreateMap<Order, Himall.DTO.OrderListModel>();
            AutoMapper.Mapper.CreateMap<DTO.OrderItem, OrderItemListModel>();
            var orderModel = AutoMapper.Mapper.Map<Order, Himall.DTO.OrderListModel>(orderInfo);
            var orderItems = OrderApplication.GetOrderItemsByOrderId(orderInfo.Id);
            var newOrderItems = new List<DTO.OrderItem>();
            foreach (var item in orderItems)
            {
                item.ThumbnailsUrl = Core.HimallIO.GetRomoteProductSizeImage(productService.GetProduct(item.ProductId).RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_50);
                // item.ThumbnailsUrl = Himall.Core.HimallIO.GetRomoteProductSizeImage(item.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_50);
                newOrderItems.Add(item);
            }
            // orderModel.OrderItemList = AutoMapper.Mapper.Map<List<DTO.OrderItem>, List<OrderItemListModel>>(orderItems);
            orderModel.OrderItemList = AutoMapper.Mapper.Map<List<DTO.OrderItem>, List<OrderItemListModel>>(newOrderItems);
            if (orderInfo.ShopBranchId.HasValue && orderInfo.ShopBranchId.Value != 0)
            {//补充数据
                var branch = ShopBranchApplication.GetShopBranchById(orderInfo.ShopBranchId.Value);
                orderModel.ShopBranchName = branch.ShopBranchName;
                orderModel.ShopBranchAddress = branch.AddressFullName;
                orderModel.ShopBranchContactPhone = branch.ContactPhone;
            }

            return Json(new { Success = "true", OrderModel = orderModel });
        }
        //public object PostPayOrder([FromBody]dynamic value)
        //{
        //    string id = value.id;
        //    id = DecodePaymentId(id);
        //    string errorMsg = string.Empty;

        //    try
        //    {
        //        var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
        //        var payInfo = payment.Biz.ProcessReturn(HttpContext.Request);
        //        if (payInfo != null)
        //        {
        //            var payTime = payInfo.TradeTime;

        //            var orderid = payInfo.OrderIds.FirstOrDefault();
        //            var orderIds = ServiceHelper.Create<IOrderService>().GetOrderPay(orderid).Select(item => item.OrderId).ToList();

        //            ViewBag.OrderIds = string.Join(",", orderIds);
        //            ServiceHelper.Create<IOrderService>().PaySucceed(orderIds, id, payInfo.TradeTime.Value, payInfo.TradNo, payId: orderid);

        //            string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", orderIds));//获取支付状态缓存键
        //            Cache.Insert(payStateKey, true, 15);//标记为已支付
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        errorMsg = ex.Message;
        //        Core.Log.Error("移动端同步返回出错，支持方式：" + id, ex);
        //    }
        //    ServiceProvider.Instance<IOrderService>.Create.PaySucceed(orderIds, id, payInfo.TradeTime.Value, payInfo.TradNo, payId: orderid);
        //}
        private string DecodePaymentId(string paymentId)
        {
            return paymentId.Replace("-", ".");
        }

        public object GetOrderBonus(string orderIds)
        {
            CheckUserLogin();
            List<BonuModel> bonus = new List<BonuModel>();
            var shopService = ServiceProvider.Instance<IShopService>.Create;
            var orderService = ServiceProvider.Instance<IOrderService>.Create;
            var bonusService = ServiceProvider.Instance<IShopBonusService>.Create;
            string orderids = orderIds;
            string[] orderArray = orderids.Split(',');
            foreach (string item in orderArray)
            {
                long orderid = 0;
                if (long.TryParse(item, out orderid))
                {
                    var bonuInfo = bonusService.GetGrantByUserOrder(orderid, CurrentUser.Id);
                    if (bonuInfo != null)
                    {
                        BonuModel bonuObject = new BonuModel();
                        bonuObject.ShareHref = "http://" + HttpContext.Current.Request.Url.Host + "/m-weixin/shopbonus/index/" + bonuInfo.Id;
                        bonuObject.ShareCount = bonuInfo.Himall_ShopBonus.Count;
                        bonuObject.ShareDetail = bonuInfo.Himall_ShopBonus.ShareDetail;
                        bonuObject.ShareTitle = bonuInfo.Himall_ShopBonus.ShareTitle;
                        bonuObject.ShopName = shopService.GetShop(bonuInfo.Himall_ShopBonus.ShopId).ShopName;
                        bonus.Add(bonuObject);
                    }
                }
            }

            return Json(new { Success = "true", List = bonus });
        }
        /// <summary>
        /// 获取订单状态
        /// <para>供支付时使用</para>
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public object GetOrerStatus(string orderIds)
        {
            CheckUserLogin();
            var orderService = ServiceProvider.Instance<IOrderService>.Create;
            var fgService = ServiceProvider.Instance<IFightGroupService>.Create;
            List<long> ordids = orderIds.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(t => long.Parse(t)).ToList();
            IEnumerable<OrderInfo> orders = orderService.GetOrders(ordids).ToList();
            var data = orders.Select(d =>
            {
                long activeId = 0, groupId = 0;
                if (d.OrderType == OrderInfo.OrderTypes.FightGroup)
                {
                    var fg = fgService.GetFightGroupOrderStatusByOrderId(d.Id);
                    if (fg != null && fg.ActiveId.HasValue && fg.GroupId.HasValue)
                    {
                        activeId = fg.ActiveId.Value;
                        groupId = fg.GroupId.Value;
                    }
                }
                return new MemberOrderGetStatusModel
                {
                    orderId = d.Id,
                    status = d.OrderStatus.GetHashCode(),
                    activeId = activeId,
                    groupId = groupId

                };
            }).ToList();
            return Json(new { Success = "true", list = data });
        }
    }
}