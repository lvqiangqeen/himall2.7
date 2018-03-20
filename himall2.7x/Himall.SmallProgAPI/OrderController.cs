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
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Himall.SmallProgAPI
{

    public class OrderController : BaseApiController
    {
        private IOrderService _orderService;
        public OrderController()
        {
            _orderService = ServiceProvider.Instance<IOrderService>.Create;
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="openId">openId</param>
        /// <param name="orderId">订单编号</param>
        /// <returns></returns>
        public object GetCloseOrder(string openId, string orderId)
        {
            CheckUserLogin();
            long order_Id = long.Parse(orderId);
            var order = ServiceProvider.Instance<IOrderService>.Create.GetOrder(order_Id, CurrentUser.Id);
            if (order != null)
            {
                //拼团处理
                if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
                {
                    return Json(new { Status = "NO", Message = "拼团订单，会员不能取消！" });
                }
                ServiceProvider.Instance<IOrderService>.Create.MemberCloseOrder(order_Id, CurrentUser.UserName);
            }
            else
            {
                return Json(new { Status = "NO", Message = "取消失败，该订单已删除或者不属于当前用户！" });
            }
            return Json(new { Status = "OK", Message = "操作成功！" });
        }
        /// <summary>
        /// 当前订单付款成功后，生成消费码
        /// </summary>
        /// <param name="openId">openId</param>
        /// <param name="orderId">订单编号</param>
        /// <returns></returns>
        public object GetCreatePayCode(string openId, long orderId)
        {
            CheckUserLogin();
            Random r = new Random();
            string payCode = DateTime.Now.AddSeconds(r.Next(60)).Ticks.ToString().Substring(12, 6) + DateTime.Now.Ticks.ToString().Substring(12, 6);
            _orderService = ServiceProvider.Instance<IOrderService>.Create;
            if (_orderService.CreatePayCodeByOrderId(orderId, payCode))
            {
                return Json(new { Status = "OK", Message = "操作成功！" });
            }
            else
            {
                return Json(new { Status = "OK", Message = "操作失败，请查看日志记录！" });
            }

        }
        /// <summary>
        /// 商家根据消费码改变订单状态
        /// </summary>
        /// <param name="payCode">消费码</param>
        /// <returns></returns>
        public object GetUpdateOrderStateByPayCode(string payCode)
        {
            string message;
            if (_orderService.UpdateOrderStateByPayCode(payCode.Trim(),out message))
            {
                return Json(new { Status = "OK", Message = message });
            }
            else
            {
                return Json(new { Status = "NO", Message = message });
            }
        }

        /// <summary>
        /// 确认收货
        /// </summary>
        /// <param name="openId">openId</param>
        /// <param name="orderId">订单编号</param>
        /// <returns></returns>
        public object GetConfirmOrder(string openId, string orderId)
        {
            CheckUserLogin();
            long order_Id = long.Parse(orderId);
            ServiceProvider.Instance<IOrderService>.Create.MembeConfirmOrder(order_Id, CurrentUser.UserName);
            // var data = ServiceProvider.Instance<IOrderService>.Create.GetOrder(orderId);
            //确认收货写入结算表(修改LH的方法)
            // ServiceProvider.Instance<IOrderService>.Create.WritePendingSettlnment(data);
            return Json(new { Status = "OK", Message = "操作成功！" });
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
                success = expressData.Success,
                traces = expressData.ExpressDataItems.Select(item => new
                {
                    acceptTime = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    acceptStation = item.Content
                })
            };
            return Json(new { Status = "OK", ShipTo = order.ShipTo, CellPhone = order.CellPhone, Address = order.RegionFullName + order.Address, ShipOrderNumber = order.ShipOrderNumber, ExpressCompanyName = order.ExpressCompanyName, LogisticsData = json });
        }

        public object GetOrderDetail(long orderId)
        {
            CheckUserLogin();
            var orderService = ServiceProvider.Instance<IOrderService>.Create;
            OrderInfo order = orderService.GetOrder(orderId, CurrentUser.Id);

            var orderRefundService = ServiceProvider.Instance<IRefundService>.Create;
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var coupon = ServiceProvider.Instance<ICouponService>.Create.GetCouponRecordInfo(order.UserId, order.Id);

            bool isCanApply = false;
            string couponName = "";
            decimal couponAmout = 0;
            if (coupon != null)
            {
                couponName = coupon.Himall_Coupon.CouponName;
                couponAmout = coupon.Himall_Coupon.Price;
            }

            //订单信息是否正常
            if (order == null)
            {
                throw new HimallException("订单号不存在！");
            }
            dynamic expressTrace = new ExpandoObject();

            //取订单物流信息
            if (!string.IsNullOrWhiteSpace(order.ShipOrderNumber))
            {
                var expressData = ServiceProvider.Instance<IExpressService>.Create.GetExpressData(order.ExpressCompanyName, order.ShipOrderNumber);
                if (expressData.Success)
                {
                    expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列
                    expressTrace.traces = expressData.ExpressDataItems.Select(item => new
                    {
                        acceptTime = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                        acceptStation = item.Content
                    });

                }
            }
            var orderRefunds = OrderApplication.GetOrderRefunds(order.OrderItemInfo.Select(p => p.Id));
            var isCanOrderReturn = false;
            //获取订单商品项数据
            var orderDetail = new
            {
                ShopId = order.ShopId,
                OrderItems = order.OrderItemInfo.Select(item =>
                {
                    var productinfo = productService.GetProduct(item.ProductId);
                    //是否有售后记录
                    if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery)
                    {
                        isCanApply = orderRefundService.CanApplyRefund(orderId, item.Id, true);
                    }
                    else
                    {
                        isCanApply = orderRefundService.CanApplyRefund(orderId, item.Id, false);
                    }
                    ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(productinfo.TypeId);
                    string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                    var itemStatusText = "";
                    var itemrefund = orderRefunds.Where(or => or.OrderItemId == item.Id).FirstOrDefault(d => d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                    int? itemrefstate = (itemrefund == null ? 0 : (int?)itemrefund.SellerAuditStatus);
                    itemrefstate = (itemrefstate > 4 ? (int?)itemrefund.ManagerConfirmStatus : itemrefstate);
                    if (itemrefund != null)
                    {//默认为商家处理进度
                        if (itemrefstate == 4)
                        {//商家拒绝,可以再发起申请
                            itemStatusText = "";
                        }
                        else
                        {
                            itemStatusText = "售后处理中";
                        }
                    }
                    if (itemrefstate > 4)
                    {//如果商家已经处理完，则显示平台处理进度
                        if (itemrefstate == 7)
                        {
                            itemStatusText = "退款成功";
                        }
                    }

                    return new
                    {
                        Status = itemrefstate,
                        StatusText = itemStatusText,
                        Id = item.Id,
                        SkuId = item.SkuId,
                        ProductId = item.ProductId,
                        Name = item.ProductName,
                        Amount = item.Quantity,
                        Price = item.SalePrice,
                        //ProductImage = "http://" + Url.Request.RequestUri.Host + productService.GetProduct(item.ProductId).GetImage(ProductInfo.ImageSize.Size_100),
                        Image = Core.HimallIO.GetRomoteProductSizeImage(productService.GetProduct(item.ProductId).RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_100),
                        color = item.Color,
                        size = item.Size,
                        version = item.Version,
                        IsCanRefund = isCanApply,
                        ColorAlias = colorAlias,
                        SizeAlias = sizeAlias,
                        VersionAlias = versionAlias,
                        SkuText = colorAlias + ":" + item.Color + ";" + sizeAlias + ":" + item.Size + ";" + versionAlias + ":" + item.Version,
                        EnabledRefundAmount = item.EnabledRefundAmount
                    };
                })
            };
            //取拼团订单状态
            var fightGroupOrderInfo = ServiceProvider.Instance<IFightGroupService>.Create.GetFightGroupOrderStatusByOrderId(order.Id);
            var orderModel = new
            {
                OrderId = order.Id,
                Status = (int)order.OrderStatus,
                StatusText = order.OrderStatus.ToDescription(),
                OrderTotal = order.OrderTotalAmount,
                OrderAmount = order.ProductTotalAmount,
                DeductionPoints = 0,
                DeductionMoney = order.IntegralDiscount,
                CouponAmount = couponAmout.ToString("F2"),//优惠劵金额
                CouponName = couponName,//优惠劵名称
                RefundAmount = order.RefundTotalAmount,
                Tax = 0,
                AdjustedFreight = order.Freight,
                OrderDate = order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                ItemStatus = 0,
                ItemStatusText = "",
                ShipTo = order.ShipTo,
                ShipToDate = order.ShippingDate.HasValue ? order.ShippingDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                Cellphone = order.CellPhone,
                Address = order.RegionFullName + " " + order.Address,
                FreightFreePromotionName = string.Empty,
                ReducedPromotionName = string.Empty,
                ReducedPromotionAmount = string.Empty,
                SentTimesPointPromotionName = string.Empty,
                CanBackReturn = !string.IsNullOrWhiteSpace(order.PaymentTypeGateway),
                CanCashierReturn = false,
                PaymentType = order.PaymentType.ToDescription(),
                Remark = string.IsNullOrEmpty(order.OrderRemarks) ? "" : order.OrderRemarks,
                InvoiceTitle = order.InvoiceTitle,
                ModeName = order.DeliveryType.ToDescription(),
                LogisticsData = expressTrace,
                TakeCode = order.PickupCode,
                LineItems = orderDetail.OrderItems,
                IsCanRefund = (orderDetail.OrderItems.Where(e => e.IsCanRefund == false).Count() == 0) && !orderService.IsRefundTimeOut(order.Id)

            };

            return Json(new { Status = "OK", Data = orderModel });
        }

        public object GetOrders(int? status, int pageIndex, int pageSize = 8)
        {
            CheckUserLogin();
            IShopAppletService iShopAppletService = ServiceProvider.Instance<IShopAppletService>.Create;
            var orderService = ServiceProvider.Instance<IOrderService>.Create;
            var allOrders = orderService.GetTopOrders(int.MaxValue, CurrentUser.Id);

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

            if (status.HasValue && status == 0)
            {
                status = null;
            }
            var queryModel = new OrderQuery()
            {
                Status = (OrderInfo.OrderOperateStatus?)status,
                UserId = CurrentUser.Id,
                PageSize = pageSize,
                PageNo = pageIndex
            };
            if (queryModel.Status.HasValue && queryModel.Status.Value == OrderInfo.OrderOperateStatus.WaitReceiving)
            {
                if (queryModel.MoreStatus == null)
                {
                    queryModel.MoreStatus = new List<OrderInfo.OrderOperateStatus>() { };
                }
                queryModel.MoreStatus.Add(OrderInfo.OrderOperateStatus.WaitSelfPickUp);
            }
            if (status.GetValueOrDefault() == (int)OrderInfo.OrderOperateStatus.Finish)
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
                //参照PC端会员中心的状态描述信息
                string statusText = item.OrderStatus.ToDescription();
                if (item.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || item.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                {
                    if (ordrefstate.HasValue && ordrefstate != 0 && ordrefstate != 4)
                    {
                        statusText = "退款中";
                    }
                }
                var shopInfo = ServiceProvider.Instance<IShopService>.Create.GetShop(item.ShopId);
                if (shopInfo != null)
                {
                    shopInfo.Logo = Himall.Core.HimallIO.GetRomoteImagePath(shopInfo.Logo);
                    //是否可退货、退款
                    bool IsShowReturn = (item.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.WaitDelivery || item.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                        && !item.RefundStats.HasValue && item.PaymentType != Himall.Model.OrderInfo.PaymentTypes.CashOnDelivery && item.PaymentType != Himall.Model.OrderInfo.PaymentTypes.None
                        && (item.FightGroupCanRefund == null || item.FightGroupCanRefund == true) && ordrefstate.GetValueOrDefault().Equals(0);
                    return new
                    {
                        PayCode = item.PayCode,
                        ShopId = item.ShopId,
                        ShopLogo = shopInfo.Logo,
                        OrderAmount = item.OrderAmount,
                        OrderId = item.Id,
                        StatusText = statusText,
                        Status = item.OrderStatus,
                        orderType = item.OrderType,
                        orderTypeName = item.OrderType.ToDescription(),
                        shopname = item.ShopName,
                        vshopId = vshop == null ? 0 : vshop.Id,
                        Amount = item.OrderTotalAmount.ToString("F2"),
                        Quantity = item.OrderProductQuantity,
                        commentCount = item.OrderCommentInfo.Count(),
                        pickupCode = item.PickupCode,
                        EnabledRefundAmount = item.OrderEnabledRefundAmount,
                        LineItems = item.OrderItemInfo.Select(a =>
                        {
                            var prodata = productService.GetProduct(a.ProductId);
                            ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(prodata.TypeId);
                            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                            var itemStatusText = "";
                            var itemrefund = orderRefunds.Where(or => or.OrderItemId == a.Id).FirstOrDefault(d => d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                            int? itemrefstate = (itemrefund == null ? 0 : (int?)itemrefund.SellerAuditStatus);
                            itemrefstate = (itemrefstate > 4 ? (int?)itemrefund.ManagerConfirmStatus : itemrefstate);
                            if (itemrefund != null)
                            {//默认为商家处理进度
                                if (itemrefstate == 4)
                                {//商家拒绝
                                    itemStatusText = "";
                                }
                                else
                                {
                                    itemStatusText = "售后处理中";
                                }
                            }
                            if (itemrefstate > 4)
                            {//如果商家已经处理完，则显示平台处理进度
                                if (itemrefstate == 7)
                                {
                                    itemStatusText = "退款成功";
                                }
                            }

                            var activeModel = ServiceProvider.Instance<IFightGroupService>.Create.GetActiveByProId(a.ProductId);

                            return new
                            {
                                Status = itemrefstate,
                                StatusText = itemStatusText,
                                Id = activeModel.Id,
                                productId = a.ProductId,
                                Name = a.ProductName,
                                Image = activeModel.ProductDefaultImage,
                                Amount = a.Quantity,
                                Price = a.SalePrice,
                                Unit = prodata == null ? "" : prodata.MeasureUnit,
                                SkuText = colorAlias + ":" + a.Color + " " + sizeAlias + ":" + a.Size + " " + versionAlias + ":" + a.Version,
                                color = a.Color,
                                size = a.Size,
                                version = a.Version,
                                ColorAlias = colorAlias,
                                SizeAlias = sizeAlias,
                                VersionAlias = versionAlias,
                                RefundStats = itemrefstate,
                                OrderRefundId = (itemrefund == null ? 0 : itemrefund.Id),
                                EnabledRefundAmount = a.EnabledRefundAmount,
                                IsShowRefund = IsShowReturn,
                                IsShowAfterSale = IsShowReturn,
                                SalePrice = iShopAppletService.GetSkuSalePrice(a.SkuId, a.ProductId)
                            };
                        }),
                        RefundStats = ordrefstate,
                        OrderRefundId = _ordrefobj.Id,
                        IsShowLogistics = !string.IsNullOrWhiteSpace(item.ShipOrderNumber),
                        ShipOrderNumber = item.ShipOrderNumber,
                        IsShowCreview = item.OrderStatus == OrderInfo.OrderOperateStatus.Finish,
                        IsShowPreview = false,
                        Invoice = item.InvoiceType.ToDescription(),
                        InvoiceValue = (int)item.InvoiceType,
                        InvoiceContext = item.InvoiceContext,
                        InvoiceTitle = item.InvoiceTitle,
                        PaymentType = item.PaymentType.ToDescription(),
                        PaymentTypeValue = (int)item.PaymentType,
                        IsShowClose = (item.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay),
                        IsShowFinishOrder = (item.OrderStatus == OrderInfo.OrderOperateStatus.WaitReceiving),
                        IsShowRefund = IsShowReturn,
                        IsShowReturn = IsShowReturn,
                        IsShowTakeCodeQRCode = !string.IsNullOrWhiteSpace(item.PickupCode),
                        OrderDate = item.OrderDate,
                        SupplierId = 0,
                        ShipperName = string.Empty,
                        StoreName = item.ShopName,
                        IsShowCertification = false,
                        CreviewText = !HasAppendComment(item) ? "评价订单" : "追加评论",
                        ProductCommentPoint = 0
                    };
                }
                else
                {
                    return null;
                }
            });
            return Json(new { Status = "OK", AllOrderCounts = allOrderCounts, WaitingForComments = waitingForComments, WaitingForRecieve = waitingForRecieve, WaitingForPay = waitingForPay, Data = result });
        }
        /// <summary>
        /// 获取物流公司信息
        /// </summary>
        /// <returns></returns>
        public object GetExpressInfo(string openId, long orderId)
        {
            CheckUserLogin();
            var json = new object();
            var orderService = ServiceProvider.Instance<IOrderService>.Create;
            OrderInfo order = orderService.GetOrder(orderId, CurrentUser.Id);
            //订单信息是否正常
            if (order == null)
            {
                json = new
                {
                    Status = "NO",
                    Data = "订单号不存在"
                };
            }
            List<object> TracesList = new List<object>();
            //取订单物流信息
            if (!string.IsNullOrWhiteSpace(order.ShipOrderNumber))
            {
                var expressData = ServiceProvider.Instance<IExpressService>.Create.GetExpressData(order.ExpressCompanyName, order.ShipOrderNumber);
                if (expressData.Success)
                {
                    expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列
                    foreach (var item in expressData.ExpressDataItems)
                    {
                        var traces = new
                        {
                            acceptStation = item.Content,
                            acceptTime = item.Time.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        TracesList.Add(traces);
                    }
                }
            }
            if (TracesList.Count > 0)
            {
                json = new
                {
                    Status = "OK",
                    Data = new
                    {
                        LogisticsData = new
                        {
                            success = true,
                            traces = TracesList
                        },
                        ExpressCompanyName = order.ExpressCompanyName,
                        ShipOrderNumber = order.ShipOrderNumber,
                        ShipTo = order.ShipTo,
                        CellPhone = order.CellPhone,
                        Address = order.RegionFullName + order.Address
                    }
                };
            }
            else
            {
                json = new
                {
                    Status = "NO",
                    Data = "[]"
                };
            }
            return json;

        }

        public object GetExpressList()
        {
            var express = ServiceProvider.Instance<IExpressService>.Create.GetAllExpress();
            var list = express.ToList().Select(item => new
            {
                ExpressName = item.Name,
                Kuaidi100Code = item.Kuaidi100Code,
                TaobaoCode = item.TaobaoCode
            }).ToList();
            var json = new
            {
                Status = "OK",
                Data = list
            };
            return json;
        }


        private bool HasAppendComment(OrderInfo list)
        {
            var item = list.OrderItemInfo.FirstOrDefault();
            var result = ServiceProvider.Instance<ICommentService>.Create.HasAppendComment(item.Id);
            return result;
        }

        public object GetSubmitModel(string cartItemIds, string productSku, WXSmallProgFromPageType fromPage, long shipAddressId = 0, long countDownId = 0, long buyAmount = 0)
        {
            CheckUserLogin();
            if (fromPage == WXSmallProgFromPageType.SignBuy)
            {
                if (!string.IsNullOrWhiteSpace(productSku))
                {
                    return GetSubmitModelById(productSku, (int)buyAmount);
                }
            }
            if (fromPage == WXSmallProgFromPageType.Cart)
            {
                return GetSubmitByCartModel(cartItemIds);
            }
            return Json(new { Status = "NO" });
        }
        /// <summary>
        /// 获取立即购买提交页面的数据
        /// </summary>
        /// <param name="skuIds">库存ID集合</param>
        /// <param name="counts">库存ID对应的数量</param>
        object GetSubmitModelById(string skuId, int count)
        {
            CheckUserLogin();
            var result = OrderApplication.GetMobileSubmit(CurrentUserId, skuId.ToString(), count.ToString());
            dynamic d = new System.Dynamic.ExpandoObject();
            dynamic add = new System.Dynamic.ExpandoObject();
            if (result.Address != null)
            {
                add = new
                {
                    ShippingId = result.Address.Id,
                    ShipTo = result.Address.ShipTo,
                    CellPhone = result.Address.Phone,
                    FullRegionName = result.Address.RegionFullName,
                    FullAddress = result.Address.RegionFullName + " " + result.Address.Address,
                    Address = result.Address.Address,
                    RegionId = result.Address.RegionId
                };
            }
            else
            {
                add = null;
            }

            d.Status = "OK";
            d.Data = new
            {
                InvoiceContext = result.InvoiceContext,
                products = result.products,
                integralPerMoney = result.integralPerMoney,
                userIntegrals = result.userIntegrals,
                TotalAmount = result.totalAmount,
                Freight = result.Freight,
                orderAmount = result.orderAmount,
                IsCashOnDelivery = result.IsCashOnDelivery,
                IsOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore,
                Address = add
            };


            return d;
        }

        /// <summary>
        /// 获取购物车提交页面的数据
        /// </summary>
        /// <param name="cartItemIds">购物车物品id集合</param>
        /// <returns></returns>
        object GetSubmitByCartModel(string cartItemIds = "")
        {
            CheckUserLogin();
            var result = OrderApplication.GetMobileSubmiteByCart(CurrentUserId, cartItemIds);

            //解决循环引用的序列化的问题
            dynamic address = new System.Dynamic.ExpandoObject();
            if (result.Address != null)
            {
                var add = new
                {
                    ShippingId = result.Address.Id,
                    ShipTo = result.Address.ShipTo,
                    CellPhone = result.Address.Phone,
                    FullRegionName = result.Address.RegionFullName,
                    FullAddress = result.Address.RegionFullName + " " + result.Address.Address,
                    Address = result.Address.Address,
                    RegionId = result.Address.RegionId
                };
                address = add;
            }
            else
                address = null;

            return Json(new
            {
                Status = "OK",
                Data = new
                {
                    Address = address,
                    IsCashOnDelivery = result.IsCashOnDelivery,
                    InvoiceContext = result.InvoiceContext,
                    products = result.products,
                    integralPerMoney = result.integralPerMoney,
                    userIntegrals = result.userIntegrals,
                    TotalAmount = result.totalAmount,
                    Freight = result.Freight,
                    orderAmount = result.orderAmount,
                    IsOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore
                }
            });
        }

        [HttpPost]
        public object SubmitOrder(SmallProgSubmitOrderModel value)
        {
            CheckUserLogin();
            if (value.fromPage == WXSmallProgFromPageType.SignBuy)
            {
                //立即购买（限时购）
                OrderSubmitOrderModel orderModel = new OrderSubmitOrderModel();
                orderModel.counts = value.buyAmount.ToString();
                orderModel.couponIds = value.couponCode;
                orderModel.integral = (int)value.deductionPoints;
                orderModel.recieveAddressId = value.shippingId;
                orderModel.skuIds = value.productSku;
                orderModel.orderRemarks = value.remark;
                orderModel.formId = value.formId;
                orderModel.isCashOnDelivery = false;//货到付款
                orderModel.invoiceType = 0;//发票类型
                orderModel.jsonOrderShops = value.jsonOrderShops;
                //提交
                return SubmitOrderById(orderModel);
            }
            else if (value.fromPage == WXSmallProgFromPageType.Cart)
            {
                //购物车
                OrderSubmitOrderByCartModel cartModel = new OrderSubmitOrderByCartModel();
                cartModel.couponIds = value.couponCode;
                cartModel.integral = (int)value.deductionPoints;
                cartModel.recieveAddressId = value.shippingId;
                cartModel.cartItemIds = value.cartItemIds;//
                cartModel.formId = value.formId;
                cartModel.isCashOnDelivery = false;//货到付款
                cartModel.invoiceType = 0;//发票类型
                cartModel.jsonOrderShops = value.jsonOrderShops;
                return SubmitOrderByCart(cartModel);
            }

            return Json(new { Status = "NO", Message = "提交来源异常" });
        }
        /// <summary>
        /// 立即购买方式提交的订单
        /// </summary>
        /// <param name="value">数据</param>
        private object SubmitOrderById(OrderSubmitOrderModel value)
        {
            string skuIds = value.skuIds;
            string counts = value.counts;
            long recieveAddressId = value.recieveAddressId;
            string couponIds = value.couponIds;
            int integral = value.integral;

            bool isCashOnDelivery = value.isCashOnDelivery;
            int invoiceType = value.invoiceType;
            string invoiceTitle = value.invoiceTitle;
            string invoiceContext = value.invoiceContext;
            //end
            string orderRemarks = string.Empty;//value.orderRemarks;//订单备注

            OrderCreateModel model = new OrderCreateModel();
            var orderService = ServiceProvider.Instance<IOrderService>.Create;
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var skuIdArr = skuIds.Split(',').Select(item => item.ToString());
            var countArr = counts.Split(',').Select(item => int.Parse(item));
            model.CouponIdsStr = OrderHelper.ConvertUsedCoupon(couponIds);
            IEnumerable<long> orderIds;
            model.PlatformType = PlatformType.WeiXinSmallProg;
            model.CurrentUser = CurrentUser;
            model.ReceiveAddressId = recieveAddressId;
            model.SkuIds = skuIdArr;
            model.Counts = countArr;
            model.Integral = integral;

            model.IsCashOnDelivery = isCashOnDelivery;
            model.Invoice = (InvoiceType)invoiceType;
            model.InvoiceContext = invoiceContext;
            model.InvoiceTitle = invoiceTitle;

            model.formId = value.formId;

            CommonModel.OrderShop[] OrderShops = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderShop[]>(value.jsonOrderShops);
            model.OrderShops = OrderShops;//用户APP选择门店自提时用到，2.5版本未支持门店自提
            model.OrderRemarks = OrderShops.Select(p => p.Remark).ToArray();
            //end

            try
            {
                //处理限时购
                if (skuIdArr.Count() == 1)
                {
                    var skuid = skuIdArr.ElementAt(0);
                    if (!string.IsNullOrWhiteSpace(skuid))
                    {
                        var sku = productService.GetSku(skuid);
                        bool isltmbuy = ServiceProvider.Instance<ILimitTimeBuyService>.Create.IsLimitTimeMarketItem(sku.ProductId);
                        model.IslimitBuy = isltmbuy;//标识为限时购计算价格按限时购价格核算
                    }
                }
                var orders = orderService.CreateOrder(model);
                orderIds = orders.Select(item => item.Id).ToArray();
                decimal orderTotals = orders.Sum(item => item.OrderTotalAmount);
                //orderIds = orderService.CreateOrder(CurrentUser.Id, skuIdArr, countArr, recieveAddressId, PlatformType);
                AddVshopBuyNumber(orderIds);//添加微店购买数量

                return Json(new { Status = "OK", Message = "提交成功", OrderId = string.Join(",", orderIds), OrderTotal = orderTotals });
            }
            catch (HimallException he)
            {
                return Json(new { Status = "NO", Message = he.Message });
            }
        }

        /// <summary>
        /// 购物车方式提交的订单
        /// </summary>
        /// <param name="value">数据</param>
        private object SubmitOrderByCart(OrderSubmitOrderByCartModel value)
        {
            string cartItemIds = value.cartItemIds;
            long recieveAddressId = value.recieveAddressId;
            string couponIds = value.couponIds;
            int integral = value.integral;

            bool isCashOnDelivery = value.isCashOnDelivery;
            int invoiceType = value.invoiceType;
            string invoiceTitle = value.invoiceTitle;
            string invoiceContext = value.invoiceContext;
            //end
            string orderRemarks = "";//value.orderRemarks;//订单备注
            OrderCreateModel model = new OrderCreateModel();
            List<OrderInfo> infos = new List<OrderInfo>();

            var orderService = ServiceProvider.Instance<IOrderService>.Create;
            IEnumerable<long> orderIds;
            model.PlatformType = PlatformType.WeiXinSmallProg;
            model.CurrentUser = CurrentUser;
            model.ReceiveAddressId = recieveAddressId;
            model.Integral = integral;

            model.formId = value.formId;

            model.IsCashOnDelivery = isCashOnDelivery;
            model.Invoice = (InvoiceType)invoiceType;
            model.InvoiceContext = invoiceContext;
            model.InvoiceTitle = invoiceTitle;
            //end
            CommonModel.OrderShop[] OrderShops = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderShop[]>(value.jsonOrderShops);
            model.OrderShops = OrderShops;//用户APP选择门店自提时用到，2.5版本未支持门店自提
            model.OrderRemarks = OrderShops.Select(p => p.Remark).ToArray();
            try
            {
                var cartItemIdsArr = cartItemIds.Split(',').Select(item => long.Parse(item)).ToArray();
                //根据购物车项补充sku数据
                var cartItems = CartApplication.GetCartItems(cartItemIdsArr);
                model.SkuIds = cartItems.Select(e => e.SkuId).ToList();
                model.Counts = cartItems.Select(e => e.Quantity).ToList();

                model.CartItemIds = cartItemIdsArr;
                model.CouponIdsStr = OrderHelper.ConvertUsedCoupon(couponIds);

                var orders = orderService.CreateOrder(model);
                orderIds = orders.Select(item => item.Id).ToArray();
                decimal orderTotals = orders.Sum(item => item.OrderTotalAmount);

                return Json(new { Status = "OK", Message = "提交成功", OrderId = string.Join(",", orderIds), OrderTotal = orderTotals });
            }
            catch (HimallException he)
            {
                return Json(new { Status = "NO", Message = he.Message });
            }
        }

        /// <summary>
        /// 添加微店购买数量
        /// </summary>
        /// <param name="orderIds"></param>
        void AddVshopBuyNumber(IEnumerable<long> orderIds)
        {
            var shopIds = ServiceProvider.Instance<IOrderService>.Create.GetOrders(orderIds).Select(item => item.ShopId);//从订单信息获取店铺id
            var vshopService = ServiceProvider.Instance<IVShopService>.Create;
            var vshopIds = shopIds.Select(item =>
            {
                var vshop = vshopService.GetVShopByShopId(item);
                if (vshop != null)
                    return vshop.Id;
                else
                    return 0;
            }
                ).Where(item => item > 0);//从店铺id反查vshopId

            foreach (var vshopId in vshopIds)
                vshopService.AddBuyNumber(vshopId);
        }

        public object GetOrderCommentProduct(long orderId)
        {
            CheckUserLogin();
            var order = ServiceProvider.Instance<IOrderService>.Create.GetOrder(orderId);
            if (order != null && order.OrderCommentInfo.Count == 0)
            {
                var model = ServiceProvider.Instance<ICommentService>.Create.GetProductEvaluationByOrderId(orderId, CurrentUser.Id).Select(item => new
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    SkuContent = item.ColorAlias + ":" + item.Color + ";" + item.SizeAlias + ":" + item.Size + ";" + item.VersionAlias + ":" + item.Version,
                    Price = item.Price,
                    SkuId = item.SkuId,
                    //Image = "http://" + Url.Request.RequestUri.Host + item.ThumbnailsUrl
                    //Image = Core.HimallIO.GetRomoteImagePath(item.ThumbnailsUrl)
                    Image = Core.HimallIO.GetRomoteProductSizeImage(item.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_220) //商城App评论时获取商品图片
                });

                var orderEvaluation = ServiceProvider.Instance<ITradeCommentService>.Create.GetOrderCommentInfo(orderId, CurrentUser.Id);
                return Json(new { Status = "OK", Data = model });
            }
            else
                return Json(new { Status = "NO", ErrorMsg = "该订单不存在或者已评论过" });
        }
    }
}
