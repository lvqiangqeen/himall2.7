using System;
using System.Collections.Generic;
using System.Linq;
using Himall.Application.Mappers;
using Himall.CommonModel;
using Himall.CommonModel.Delegates;
using Himall.Core;
using Himall.Core.Plugins.Payment;
using Himall.DTO;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using AutoMapper;
using System.Threading.Tasks;
using Himall.Core.Plugins.Message;
using Himall.Core.Plugins;

namespace Himall.Application
{
    public class OrderApplication
    {
        #region 字段
        private static IOrderService _iOrderService = ObjectContainer.Current.Resolve<IOrderService>();
        private static IMemberIntegralService _iMemberIntegralService = ObjectContainer.Current.Resolve<IMemberIntegralService>();
        private static IProductService _iProductService = ObjectContainer.Current.Resolve<IProductService>();
        private static ILimitTimeBuyService _iLimitTimeBuyService = ObjectContainer.Current.Resolve<ILimitTimeBuyService>();
        private static ITypeService _iTypeService = ObjectContainer.Current.Resolve<ITypeService>();
        private static IRefundService _iRefundService = ObjectContainer.Current.Resolve<IRefundService>();
        private static IExpressService _iExpressService = ObjectContainer.Current.Resolve<IExpressService>();
        #endregion

        #region 属性
        /// <summary>
        /// 订单支付成功事件
        /// </summary>
        public static event OrderPaySuccessed OnOrderPaySuccessed
        {
            add
            {
                _iOrderService.OnOrderPaySuccessed += value;
            }
            remove
            {
                _iOrderService.OnOrderPaySuccessed -= value;
            }
        }
        #endregion

        #region web公共方法

        /// <summary>
        /// 立即购买提交订单时调用的POST方法
        /// </summary>
        /// <param name="userid">用户标识</param>
        /// <param name="skuIds">库存标识集合</param>
        /// <param name="counts">每个库存购买数量</param>
        /// <param name="recieveAddressId">客户收货区域ID</param>
        /// <param name="couponIds">商品对应的优惠券ID集合</param>
        /// <param name="invoiceType">发票类型0不要发票1增值税发票2普通发票</param>
        /// <param name="invoiceTitle">发票抬头</param>
        /// <param name="invoiceContext">发票内容</param>
        /// <param name="integral">使用积分</param>
        /// <param name="collIds">组合购Id集合</param>
        /// <param name="PlatformType">订单来源平台</param>
        /// <returns>订单集合,是否操作成功</returns>
        public static OrderReturnModel SubmitOrder(CommonModel.OrderPostModel submitModel)
        {
            var skuIdsArr = submitModel.OrderShops.SelectMany(p => p.OrderSkus.Select(pp => pp.SkuId));
            var counts = submitModel.OrderShops.SelectMany(p => p.OrderSkus.Select(pp => pp.Count));
            var productService = _iProductService;
            var orderService = _iOrderService;
            if (submitModel.Integral < 0)
            {
                throw new HimallException("兑换积分数量不正确");
            }
            IEnumerable<long> collocationPidArr = null;
            if (!string.IsNullOrEmpty(submitModel.CollpIds))
            {
                collocationPidArr = submitModel.CollpIds.Split(',').Select(item => long.Parse(item));
            }
            if (submitModel.OrderShops == null || submitModel.OrderShops.Any(p => p.OrderSkus == null || p.OrderSkus.Any(pp => string.IsNullOrWhiteSpace(pp.SkuId) || pp.Count <= 0)))
                throw new Himall.Core.HimallException("创建订单的时候，SKU为空，或者数量为0");
            var model = new OrderCreateModel();
            model.SkuIds = skuIdsArr;
            model.Counts = counts;
            model.CurrentUser = (UserMemberInfo)submitModel.CurrentUser;
            model.Integral = submitModel.Integral;
            model.IsCashOnDelivery = submitModel.IsCashOnDelivery;
            model.OrderRemarks = submitModel.OrderShops.Select(p => p.Remark).ToArray();
            model.CouponIdsStr = ConvertUsedCoupon(submitModel.CouponIds);
            model.Invoice = (InvoiceType)submitModel.InvoiceType;
            model.InvoiceTitle = submitModel.InvoiceTitle;
            model.InvoiceContext = submitModel.InvoiceContext;
            model.CollPids = collocationPidArr;
            model.ReceiveAddressId = submitModel.RecieveAddressId;
            model.PlatformType = (PlatformType)submitModel.PlatformType;
            model.OrderShops = submitModel.OrderShops;
            if (!string.IsNullOrWhiteSpace(submitModel.CartItemIds))
                model.CartItemIds = submitModel.CartItemIds.Split(',').Select(item => long.Parse(item)).ToArray();
            model.GroupId = submitModel.GroupId;
            model.ActiveId = submitModel.groupActionId;

            var orders = orderService.CreateOrder(model);

            var result = new OrderReturnModel();
            //更新所属分销员
            Task.Factory.StartNew(() =>
            {
                MemberApplication.UpdateDistributionUserLink(submitModel.DistributionUserLinkId, model.CurrentUser.Id);
            });
            decimal orderTotals = orders.Sum(item => item.OrderTotalAmount);
            result = new OrderReturnModel();
            result.Success = true;
            result.OrderIds = orders.Select(item => item.Id).ToArray();
            result.OrderTotal = orderTotals;
            return result;
        }

        /// <summary>
        /// 获提交订单页面数据
        /// </summary>
        /// <param name="cartItemIds">提交的购物车物品集合</param>
        /// <param name="regionId">客户送货区域标识</param>
        /// <param name="userid">用户标识</param>
        /// <param name="cartInfo">cookie的购物车物品集合</param>
        /// <returns>页面数据</returns>
        public static OrderSubmitModel Submit(string cartItemIds, long? regionId, long userid, string cartInfo)
        {
            var integralExchange = _iMemberIntegralService.GetIntegralChangeRule();
            var intergralModel = _iMemberIntegralService.GetMemberIntegral(userid);
            int MoneyPerIntegral = integralExchange == null ? 0 : integralExchange.MoneyPerIntegral;
            OrderSubmitModel submitModel = new OrderSubmitModel();
            submitModel.IntegralPerMoney = integralExchange == null ? 0 : integralExchange.IntegralPerMoney;
            submitModel.Integral = intergralModel == null ? 0 : intergralModel.AvailableIntegrals;
            //设置会员信息
            submitModel.Member = MemberApplication.GetMember(userid);
            submitModel.cartItemIds = cartItemIds;
            //获取订单商品信息
            GetOrderProductsInfo(submitModel, cartItemIds, regionId, userid, cartInfo);
            submitModel.TotalIntegral = MoneyPerIntegral == 0 ? 0 : Convert.ToInt32(Math.Floor(submitModel.totalAmount / MoneyPerIntegral));
            submitModel.MoneyPerIntegral = MoneyPerIntegral;
            //获取收货地址
            var address = GetShippingAddress(regionId, userid);
            submitModel.address = address;
            if (address != null)
            {
                bool hasRegion = PaymentConfigApplication.IsCashOnDelivery(address.RegionId);
                submitModel.IsCashOnDelivery = hasRegion;
            }
            else
            {
                submitModel.IsCashOnDelivery = false;
            }
            submitModel.IsLimitBuy = false;
            //发票信息
            submitModel.InvoiceTitle = _iOrderService.GetInvoiceTitles(userid);
            submitModel.InvoiceContext = _iOrderService.GetInvoiceContexts();
            return submitModel;
        }

        /// <summary>
        /// 根据订单ID获取订单信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static Order GetOrder(long orderId)
        {
            return _iOrderService.GetOrder(orderId).Map<DTO.Order>();
        }

        /// <summary>
        /// 根据订单Id获取FullOrder
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static FullOrder GetFullOrder(long orderId)
        {
            var order = _iOrderService.GetOrder(orderId).Map<DTO.FullOrder>();
            order.OrderItems = GetOrderItemsByOrderId(order.Id);
            return order;
        }

        /// <summary>
        /// 根据提货码取订单
        /// </summary>
        /// <param name="pickCode"></param>
        /// <param name="fullOrderItems">是否填充OrderItems属性</param>
        /// <returns></returns>
        public static Order GetOrderByPickCode(string pickCode)
        {
            return _iOrderService.GetOrderByPickCode(pickCode).Map<DTO.Order>();
        }
        /// <summary>
        /// 根据提货码获取FullOrder
        /// </summary>
        /// <param name="pickCode"></param>
        /// <returns></returns>
        public static FullOrder GetFullOrderByPickCode(string pickCode)
        {
            var order = _iOrderService.GetOrderByPickCode(pickCode).Map<DTO.FullOrder>();
            order.OrderItems = GetOrderItemsByOrderId(order.Id);
            return order;
        }

        /// <summary>
        /// 获取商品已购数
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public static Dictionary<long, int> GetProductBuyCount(long userId, IEnumerable<long> productIds)
        {
            return _iOrderService.GetProductBuyCount(userId, productIds);
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<Order> GetOrders(OrderQuery query)
        {
            var data = _iOrderService.GetOrders(query);
            var models = data.Models.Map<List<DTO.Order>>();
            foreach (var item in data.Models)
            {
                if (item.OrderStatus >= OrderInfo.OrderOperateStatus.WaitDelivery)
                {
                    _iOrderService.CalculateOrderItemRefund(item.Id);
                }
            }
            return new QueryPageModel<Order>
            {
                Models = models,
                Total = data.Total
            };
        }



        /// <summary>
        /// 根据每个店铺的购物列表获取每个店铺的满额减优惠金额
        /// </summary>
        /// <param name="cartItems"></param>
        /// <returns></returns>
        private static decimal GetShopFullDiscount(List<CartItemModel> cartItems)
        {
            var productIds = cartItems.Select(a => a.id).Distinct();
            var shopId = cartItems.FirstOrDefault().shopId;
            var actives = FullDiscountApplication.GetOngoingActiveByProductIds(productIds, shopId);
            decimal shopFullDiscount = 0;
            foreach (var active in actives)
            {
                var pids = active.Products.Select(a => a.ProductId);
                List<CartItemModel> items = cartItems;
                if (!active.IsAllProduct)
                {
                    items = items.Where(a => pids.Contains(a.id)).ToList();
                }
                var realTotal = items.Sum(a => a.price * a.count);  //满额减的总金额
                var rule = active.Rules.Where(a => a.Quota <= realTotal).OrderByDescending(a => a.Quota).FirstOrDefault();
                decimal fullDiscount = 0;
                if (rule != null)//找不到就是不满足金额
                {
                    fullDiscount = rule.Discount;
                    shopFullDiscount += fullDiscount; //店铺总优惠金额
                }
            }
            return shopFullDiscount;
        }


        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<FullOrder> GetFullOrders(OrderQuery query)
        {
            var data = _iOrderService.GetOrders(query);
            if (data.Models.Count <= 0)
                return new QueryPageModel<FullOrder>()
                {
                    Models = new List<FullOrder>(),
                    Total = data.Total
                };
            var models = data.Models.Map<List<DTO.FullOrder>>();
            var orderids = models.Select(p => p.Id).ToList();
            var orderItems = GetOrderItemsByOrderId(models.Select(p => p.Id));
            //补充商品单位
            var products = ProductManagerApplication.GetAllStatusProductByIds(orderItems.Select(e => e.ProductId).Distinct());
            //补充门店名称
            var branchIds = models.Where(e => e.ShopBranchId.HasValue && e.ShopBranchId.Value != 0).Select(e => e.ShopBranchId.Value).Distinct();
            var branchModels = ShopBranchApplication.GetShopBranchByIds(branchIds);
            RefundQuery rquery = new RefundQuery()
            {
                OrderId = orderids[0],
                MoreOrderId = orderids,
                PageNo = 1,
                PageSize = int.MaxValue
            };
            var refundPage = _iRefundService.GetOrderRefunds(rquery).Models;
            foreach (var order in models)
            {
                order.OrderItems = orderItems.Where(p => p.OrderId == order.Id).ToList();
                if (order.ShopBranchId.HasValue && order.ShopBranchId.Value != 0)
                {//补充门店名称
                    var branch = branchModels.FirstOrDefault(e => e.Id == order.ShopBranchId.Value);
                    if (branch != null)
                    {
                        order.ShopBranchName = branch.ShopBranchName;
                    }
                }
                else
                {
                    order.ShopBranchName = "总店";
                }
                //订单售后
                var ordref = refundPage.FirstOrDefault(d => d.OrderId == order.Id && d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund);
                if (ordref != null)
                {
                    order.RefundStats = ordref.RefundStatusValue;
                    order.ShowRefundStats = ordref.RefundStatus;
                    if (order.ShopBranchId > 0)
                    {
                        order.ShowRefundStats = order.ShowRefundStats.Replace("商家", "门店");
                    }
                }
                foreach (var item in order.OrderItems)
                {
                    var p = products.FirstOrDefault(e => e.Id == item.ProductId);
                    if (p != null)
                    {
                        item.Unit = p.MeasureUnit;
                        item.ThumbnailsUrl = Himall.Core.HimallIO.GetRomoteProductSizeImage(item.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_100);
                    }
                    ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetTypeByProductId(item.ProductId);
                    item.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    item.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    item.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                    //订单项售后
                    var orditemref = refundPage.FirstOrDefault(d => d.OrderId == order.Id && d.OrderItemId == item.Id && d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                    if (orditemref != null)
                    {
                        item.RefundStats = orditemref.RefundStatusValue;
                        item.ShowRefundStats = orditemref.RefundStatus;
                        if (order.ShopBranchId > 0)
                        {
                            item.ShowRefundStats = item.ShowRefundStats.Replace("商家", "门店");
                        }
                    }
                }
            }
            return new QueryPageModel<FullOrder>
            {
                Models = models,
                Total = data.Total
            };
        }

        /// <summary>
        /// 获取订单列表(忽略分页)
        /// </summary>
        /// <param name="orderQuery"></param>
        /// <returns></returns>
        public static List<Order> GetOrdersNoPage(OrderQuery orderQuery)
        {
            return _iOrderService.GetOrdersNoPage(orderQuery).Map<List<Order>>();
        }

        /// <summary>
        /// 获取订单列表(忽略分页)
        /// </summary>
        /// <param name="orderQuery"></param>
        /// <param name="fullOrderItems">是否填充OrderItems属性</param>
        /// <returns></returns>
        public static List<FullOrder> GetFullOrdersNoPage(OrderQuery orderQuery)
        {
            var list = _iOrderService.GetOrdersNoPage(orderQuery).Map<List<FullOrder>>();

            var orderItems = GetOrderItemsByOrderId(list.Select(p => p.Id));
            foreach (var item in list)
            {
                item.OrderItems = orderItems.Where(p => p.OrderId == item.Id).ToList();
            }

            return list;
        }

        /// <summary>
        /// 分页查询平台会员购买记录
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<Order> GetUserBuyRecord(long userId, OrderQuery query)
        {
            query.UserId = userId;
            query.IsBuyRecord = true;
            var order = _iOrderService.GetOrders<OrderInfo>(query);
            var models = order.Models.ToList().Map<List<Order>>();

            return new QueryPageModel<Order>()
            {
                Total = order.Total,
                Models = models
            };
        }

        /// <summary>
        /// 分页查询平台会员购买记录
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<FullOrder> GetFullOrdersUserBuyRecord(long userId, OrderQuery query)
        {
            query.UserId = userId;
            query.IsBuyRecord = true;
            var order = _iOrderService.GetOrders(query);
            var models = order.Models.Map<List<FullOrder>>();

            var orderItems = GetOrderItemsByOrderId(models.Select(p => p.Id));
            foreach (var item in models)
            {
                item.OrderItems = orderItems.Where(p => p.OrderId == item.Id).ToList();
            }

            return new QueryPageModel<FullOrder>()
            {
                Total = order.Total,
                Models = models
            };
        }

        public static QueryPageModel<Order> GetOrders<TOrderBy>(OrderQuery query, System.Linq.Expressions.Expression<Func<OrderInfo, TOrderBy>> orderBy)
        {
            var data = _iOrderService.GetOrders<TOrderBy>(query, orderBy);
            var models = data.Models.ToList().Map<List<DTO.Order>>();

            return new QueryPageModel<Order>
            {
                Models = models,
                Total = data.Total
            };
        }

        public static QueryPageModel<FullOrder> GetFullOrders<TOrderBy>(OrderQuery query, System.Linq.Expressions.Expression<Func<OrderInfo, TOrderBy>> orderBy)
        {
            var data = _iOrderService.GetOrders<TOrderBy>(query, orderBy);
            var models = data.Models.ToList().Map<List<DTO.FullOrder>>();

            var orderItems = GetOrderItemsByOrderId(models.Select(p => p.Id));
            foreach (var item in models)
            {
                item.OrderItems = orderItems.Where(p => p.OrderId == item.Id).ToList();
            }

            return new QueryPageModel<FullOrder>
            {
                Models = models,
                Total = data.Total
            };
        }

        /// <summary>
        /// 根据订单id获取订单
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<Order> GetOrders(IEnumerable<long> ids)
        {
            return _iOrderService.GetOrders(ids).Map<List<Order>>();
        }

        /// <summary>
        /// 根据订单id获取订单
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<FullOrder> GetFullOrders(IEnumerable<long> ids)
        {
            var list = _iOrderService.GetOrders(ids).Map<List<FullOrder>>();

            var orderItems = GetOrderItemsByOrderId(list.Select(p => p.Id));
            foreach (var item in list)
            {
                item.OrderItems = orderItems.Where(p => p.OrderId == item.Id).ToList();
            }

            return list;
        }

        /// <summary>
        /// 获得立即购买数据
        /// </summary>
        /// <param name="userid">用户ID</param>
        /// <param name="skuIds">库存Id集合</param>
        /// <param name="counts">每个库存购买数量</param>
        /// <param name="regionId">客户收货地区的标识</param>
        /// <param name="collpids">组合购Id集合</param>
        /// <returns>返回订单提交页面数据</returns>
        public static OrderSubmitModel SubmitByProductId(long userid, string skuIds, string counts, long? regionId, string collpids = null)
        {
            OrderSubmitModel submitModel = new OrderSubmitModel();
            //获取订单商品信息
            SetOrderProductsInfo(submitModel, skuIds, counts, userid, collpids);

            //获取收货地址
            var address = GetShippingAddress(regionId, userid);

            submitModel.address = address;
            if (address != null)
            {
                bool hasRegion = PaymentConfigApplication.IsCashOnDelivery(address.RegionId);
                var isEnable = PaymentConfigApplication.IsEnable();
                if (hasRegion && isEnable)
                {
                    submitModel.IsCashOnDelivery = true;
                }
                else
                {
                    submitModel.IsCashOnDelivery = false;
                }
            }
            else
            {
                submitModel.IsCashOnDelivery = false;
            }

            var integralExchange = _iMemberIntegralService.GetIntegralChangeRule();
            var intergralModel = _iMemberIntegralService.GetMemberIntegral(userid);
            submitModel.IntegralPerMoney = integralExchange == null ? 0 : integralExchange.IntegralPerMoney;
            submitModel.MoneyPerIntegral = integralExchange == null ? 0 : integralExchange.MoneyPerIntegral;
            submitModel.Integral = intergralModel == null ? 0 : intergralModel.AvailableIntegrals;
            submitModel.TotalIntegral = submitModel.MoneyPerIntegral == 0 ? 0 : Convert.ToDecimal(Math.Floor(submitModel.totalAmount / submitModel.MoneyPerIntegral));

            var sku = _iProductService.GetSku(skuIds.Split(',')[0]);
            submitModel.IsLimitBuy = _iProductService.IsLimitBuy(sku.ProductId);

            submitModel.collIds = collpids;//组合购商品ID
            submitModel.skuIds = skuIds;//sku集合
            submitModel.counts = counts;//数量集合
                                        //发票信息
            submitModel.InvoiceTitle = _iOrderService.GetInvoiceTitles(userid);
            submitModel.InvoiceContext = _iOrderService.GetInvoiceContexts();
            return submitModel;
        }

        /// <summary>
        /// 拼团订单信息
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="skuId"></param>
        /// <param name="count"></param>
        /// <param name="GroupActionId"></param>
        /// <param name="GroupId"></param>
        /// <returns></returns>
        public static MobileOrderDetailConfirmModel SubmitByGroupId(long userid, string skuId, int count, long GroupActionId, long? GroupId = null)
        {

            MobileOrderDetailConfirmModel result = new MobileOrderDetailConfirmModel();
            result.InvoiceContext = _iOrderService.GetInvoiceContexts();

            if (GroupActionId <= 0)
            {
                throw new InvalidPropertyException("无效的拼团活动");
            }
            if (GroupId > 0)
            {
                var gpobj = FightGroupApplication.GetGroup(GroupActionId, GroupId.Value);
                if (gpobj == null)
                {
                    throw new InvalidPropertyException("无效的团信息");
                }
            }

            //获取购买商品信息
            GetOrderProductsInfoOnGroup(skuId, count, userid, GroupActionId, result, GroupId);
            result.IsCashOnDelivery = false; //不支持货到付款
            result.Sku = skuId;
            result.Count = count.ToString();
            return result;
        }

        /// <summary>
        /// 判断用户是否有支付密码
        /// </summary>
        /// <param name="userid">用户标识</param>
        /// <returns>是否</returns>
        public static bool GetPayPwd(long userid)
        {
            string paypwd = MemberApplication.GetMember(userid).PayPwd;
            if (string.IsNullOrWhiteSpace(paypwd))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 根据用户ID获取用户收获地址列表
        /// </summary>
        /// <param name="userid">用户ID</param>
        /// <returns>收获地址列表</returns>
        public static List<ShipAddressInfo> GetUserShippingAddresses(long userid)
        {
            var addresses = ShippingAddressApplication.GetUserShippingAddressByUserId(userid).ToArray();
            List<ShipAddressInfo> result = new List<ShipAddressInfo>();
            foreach (var item in addresses)
            {
                ShipAddressInfo addr = new ShipAddressInfo();
                addr.id = item.Id;
                addr.fullRegionName = item.RegionFullName;
                addr.address = item.Address;
                addr.phone = item.Phone;
                addr.shipTo = item.ShipTo;
                addr.fullRegionIdPath = item.RegionIdPath;
                addr.regionId = item.RegionId;
                addr.latitude = item.Latitude.HasValue ? item.Latitude.Value : 0;
                addr.longitude = item.Longitude.HasValue ? item.Longitude.Value : 0;
                result.Add(addr);
            }
            return result;
        }

        /// <summary>
        /// 确认零元订单
        /// </summary>
        /// <param name="userid">用户ID</param>
        /// <param name="orderIds">订单ID集合</param>
        public static void PayConfirm(long userid, string orderIds)
        {
            var orderIdArr = orderIds.Split(',').Select(item => long.Parse(item));
            _iOrderService.ConfirmZeroOrder(orderIdArr, userid);
        }

        /// <summary>
        /// 保存发票抬头
        /// </summary>
        /// <param name="userid">用户ID</param>
        /// <param name="name">抬头名称</param>
        /// <returns>返回发票抬头ID</returns>
        public static long SaveInvoiceTitle(long userid, string name)
        {
            InvoiceTitleInfo info = new InvoiceTitleInfo
            {
                Name = name,
                UserId = userid,
                IsDefault = 0
            };
            long result = -1;
            if (!string.IsNullOrWhiteSpace(info.Name))
            {
                result = _iOrderService.SaveInvoiceTitle(info);
            }
            return result;
        }

        /// <summary>
        /// 删除发票抬头
        /// </summary>
        /// <param name="id">发票抬头标识</param>
        public static void DeleteInvoiceTitle(long id)
        {
            _iOrderService.DeleteInvoiceTitle(id);
        }

        /// <summary>
        /// 获取运费
        /// </summary>
        /// <param name="addressId"></param>
        /// <param name="counts">门店，商品id和数量的集合</param>
        /// <returns></returns>
        public static Dictionary<long, decimal> CalcFreight(int addressId, Dictionary<long, Dictionary<long, int>> counts)
        {
            var result = new Dictionary<long, decimal>();

            foreach (var shopId in counts.Keys)
            {
                var freight = _iProductService.GetFreight(counts[shopId].Keys, counts[shopId].Values, addressId);
                result.Add(shopId, freight);
            }

            return result;
        }

        /// <summary>
        /// 预付款支付
        /// </summary>
        /// <param name="userid">用户ID</param>
        /// <param name="orderIds">订单ID集合</param>
        /// <param name="pwd">密码</param>
        /// <param name="hostUrl">网站地址</param>
        /// <returns>支付是否成功</returns>
        public static bool PayByCapital(long userid, string orderIds, string pwd, string hostUrl)
        {
            if (string.IsNullOrWhiteSpace(orderIds))
            {
                throw new HimallException("错误的订单编号");
            }
            var success = MemberApplication.VerificationPayPwd(userid, pwd);
            if (!success)
            {
                throw new HimallException("支付密码不对");
            }
            IEnumerable<long> ids = orderIds.Split(',').Select(e => long.Parse(e));
            //获取待支付的所有订单
            var orders = _iOrderService.GetOrders(ids).Where(item => item.OrderStatus == Model.OrderInfo.OrderOperateStatus.WaitPay && item.UserId == userid).ToList();

            if (orders == null || orders.Count() == 0) //订单状态不正确
            {
                throw new HimallException("错误的订单编号");
            }
            /* 积分支付的订单金额，可能为0
            decimal total = orders.Sum(a => a.OrderTotalAmount);
            if (total == 0)
            {
                throw new HimallException("错误的订单总价");
            }*/

            foreach (var item in orders)
            {
                if (item.OrderType == OrderInfo.OrderTypes.FightGroup)
                {
                    if (!FightGroupApplication.OrderCanPay(item.Id))
                    {
                        throw new HimallException("有拼团订单为不可付款状态");
                    }
                }
            }

            #region 支付流水获取
            var orderPayModel = orders.Select(item => new OrderPayInfo
            {
                PayId = 0,
                OrderId = item.Id
            });
            //保存支付订单
            long payid = _iOrderService.SaveOrderPayInfo(orderPayModel, PlatformType.PC);
            #endregion

            _iOrderService.PayCapital(ids, payId: payid);

            //限时购
            IncreaseSaleCount(ids.ToList());
            //红包
            GenerateBonus(ids, hostUrl);
            return true;
        }
        public static bool PayByCapitalIsOk(long userid, string orderIds)
        {
            IEnumerable<long> ids = orderIds.Split(',').Select(e => long.Parse(e));
            return _iOrderService.PayByCapitalIsOk(userid, ids);
        }
        /// <summary>
        /// 获取支付页面数据
        /// </summary>
        /// <param name="userid">用户ID</param>
        /// <param name="orderIds">订单ID集合</param>
        /// <param name="webRoot">站点地址</param>
        /// <returns>数据</returns>
        public static PaymentViewModel GetPay(long userid, string orderIds, string webRoot)
        {
            PaymentViewModel result = new PaymentViewModel();
            result.IsSuccess = true;
            if (string.IsNullOrEmpty(orderIds))
            {
                result.IsSuccess = false;
                result.Msg = "订单号错误，不能进行支付。";
                return result;
            }
            var orderIdArr = orderIds.Split(',').Select(item => long.Parse(item));
            var orders = _iOrderService.GetOrders(orderIdArr).Where(p => p.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay && p.UserId == userid).ToList();
            if (orders.Count <= 0)//订单已经支付，则跳转至订单页面
            {

                var errorOrder = _iOrderService.GetOrders(orderIdArr).Where(p => p.OrderStatus == OrderInfo.OrderOperateStatus.Close && p.UserId == userid).Count();
                result.IsSuccess = false;
                if (errorOrder > 0)
                    result.Msg = "订单已关闭，不能进行支付。";
                else
                    result.Msg = "系统错误，您可以到 “我的订单” 查看付款操作是否成功。";
                return result;
            }
            else
            {

                //获取待支付的所有订单
                var orderser = _iOrderService;

                foreach (var item in orders)
                {
                    if (item.OrderType == OrderInfo.OrderTypes.FightGroup)
                    {
                        if (!FightGroupApplication.OrderCanPay(item.Id))
                        {
                            throw new HimallException("有拼团订单为不可付款状态");
                        }
                    }
                }

                #region 数据补偿
                //EDIT DZY [150703]
                //是否有已删商品
                bool isHaveNoSaleProOrd = false;   //是否有非销售中的商品
                List<OrderInfo> delOrders = new List<OrderInfo>();
                foreach (var order in orders)
                {
                    if (order.OrderStatus == OrderInfo.OrderOperateStatus.Close)
                    {
                        delOrders.Add(order);
                        isHaveNoSaleProOrd = true;
                    }
                }
                if (isHaveNoSaleProOrd)
                {
                    foreach (var _item in delOrders)
                    {
                        orders.Remove(_item);  //执行清理
                    }
                    throw new HimallException("有订单商品处于非销售状态，请手动处理。");
                }
                result.HaveNoSalePro = isHaveNoSaleProOrd;
                #endregion

                if (orders == null || orders.Count == 0) //订单状态不正确
                {
                    result.IsSuccess = false;
                    result.Msg = "系统错误，您可以到 “我的订单” 查看付款操作是否成功。";
                }

                result.Orders = orders;

                decimal total = orders.Sum(a => a.OrderTotalAmount);

                result.TotalAmount = total;

                //获取所有订单中的商品名称
                var productInfos = GetProductNameDescriptionFromOrders(orders);

                //获取同步返回地址
                string returnUrl = webRoot + "/Pay/Return/{0}";

                //获取异步通知地址
                string payNotify = webRoot + "/Pay/Notify/{0}";

                var payments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.Biz.SupportPlatforms.Contains(PlatformType.PC));

                const string RELATEIVE_PATH = "/Plugins/Payment/";

                var models = payments.Select(item =>
                {
                    string requestUrl = string.Empty;

                    #region 适应改价(注释)
                    //TODO:DZY[160428] 适应改价需求，支付过程分离
                    //try
                    //{
                    //    requestUrl = item.Biz.GetRequestUrl(string.Format(returnUrl, EncodePaymentId(item.PluginInfo.PluginId)), string.Format(payNotify, EncodePaymentId(item.PluginInfo.PluginId)), ids, total, productInfos);
                    //}
                    //catch (Exception ex)
                    //{
                    //    Core.Log.Error("支付页面加载支付插件出错", ex);
                    //}
                    #endregion

                    return new PaymentModel()
                    {
                        Logo = RELATEIVE_PATH + item.PluginInfo.ClassFullName.Split(',')[1] + "/" + item.Biz.Logo,
                        RequestUrl = requestUrl,
                        UrlType = item.Biz.RequestUrlType,
                        Id = item.PluginInfo.PluginId
                    };
                });
                result.Models = models;
                //models = models.Where( item => !string.IsNullOrEmpty( item.RequestUrl ) );//只选择正常加载的插件
                //TODO:【2015-08-31】支付页面增加预付款
                var capital = MemberCapitalApplication.GetCapitalInfo(userid);
                if (capital == null)
                {
                    result.Capital = 0;
                }
                else
                {
                    result.Capital = capital.Balance != null ? capital.Balance.Value : 0;
                }
                return result;
            }
        }

        /// <summary>
        /// 获取支付相关信息
        /// </summary>
        /// <param name="userid">用户id</param>
        /// <param name="orderIds">订单id</param>
        /// <param name="webRoot">网站根目录</param>
        /// <returns>支付相信息</returns>
        public static ChargePayModel ChargePay(long userid, string orderIds, string webRoot)
        {

            ChargePayModel viewmodel = new ChargePayModel();
            var model = MemberCapitalApplication.GetChargeDetail(long.Parse(orderIds));
            if (model == null || model.MemId != userid || model.ChargeStatus == ChargeDetailInfo.ChargeDetailStatus.ChargeSuccess)//订单已经支付，则跳转至订单页面
            {
                Log.Error("调用ChargePay方法时未找到充值申请记录：" + orderIds);
                //return RedirectToAction("index", "userCenter", new { url = "/UserCapital", tar = "UserCapital" });
                return null;
            }
            else
            {

                //ViewBag.Orders = model;
                viewmodel.Orders = model;
                //string webRoot = Request.Url.Scheme + "://" + HttpContext.Request.Url.Host + (HttpContext.Request.Url.Port == 80 ? "" : (":" + HttpContext.Request.Url.Port.ToString()));

                //获取同步返回地址
                string returnUrl = webRoot + "/Pay/CapitalChargeReturn/{0}";

                //获取异步通知地址
                string payNotify = webRoot + "/Pay/CapitalChargeNotify/{0}";

                var payments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.Biz.SupportPlatforms.Contains(PlatformType.PC));

                const string RELATEIVE_PATH = "/Plugins/Payment/";

                var models = payments.Select(item =>
                {
                    string requestUrl = string.Empty;
                    try
                    {
                        requestUrl = item.Biz.GetRequestUrl(string.Format(returnUrl, EncodePaymentId(item.PluginInfo.PluginId)), string.Format(payNotify, EncodePaymentId(item.PluginInfo.PluginId)), orderIds, model.ChargeAmount, "预付款充值");
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
                models = models.Where(item => !string.IsNullOrEmpty(item.RequestUrl));//只选择正常加载的插件
                viewmodel.OrderIds = orderIds;
                viewmodel.TotalAmount = model.ChargeAmount;
                viewmodel.Step = 1;
                viewmodel.UnpaidTimeout = SiteSettingApplication.GetSiteSettings().UnpaidTimeout;
                viewmodel.models = models.ToList();
                //return View(viewmodel);
                return viewmodel;
            }
        }

        /// <summary>
        /// 获得限时购订单提交数据对像
        /// </summary>
        /// <returns></returns>
        public static OrderCreateModel GetLimitOrder(CommonModel.OrderPostModel model)
        {
            var skuIdsArr = model.OrderShops.SelectMany(p => p.OrderSkus.Select(pp => pp.SkuId));
            var pCountsArr = model.OrderShops.SelectMany(p => p.OrderSkus.Select(pp => pp.Count));
            var productService = _iProductService;
            var orderService = _iOrderService;
            if (model.Integral < 0)
            {
                throw new HimallException("兑换积分数量不正确");
            }
            IEnumerable<long> collocationPidArr = null;
            if (!string.IsNullOrEmpty(model.CollpIds))
            {
                collocationPidArr = model.CollpIds.Split(',').Select(item => long.Parse(item));
            }

            var result = new OrderCreateModel();
            result.SkuIds = skuIdsArr;
            result.Counts = pCountsArr;
            result.CurrentUser = (UserMemberInfo)model.CurrentUser;
            result.Integral = model.Integral;
            result.IsCashOnDelivery = model.IsCashOnDelivery;
            result.OrderRemarks = model.OrderShops.Select(p => p.Remark);
            result.CouponIdsStr = ConvertUsedCoupon(model.CouponIds);
            result.Invoice = (InvoiceType)model.InvoiceType;
            result.InvoiceTitle = model.InvoiceTitle;
            result.InvoiceContext = model.InvoiceContext;
            result.CollPids = collocationPidArr;
            result.ReceiveAddressId = model.RecieveAddressId;
            result.IslimitBuy = true;
            result.OrderShops = model.OrderShops;
            if (result.Counts.Count() == 0)
                throw new InvalidPropertyException("待提交订单的商品数量不能这空");
            else if (result.Counts.Count(item => item <= 0) > 0)
                throw new InvalidPropertyException("待提交订单的商品数量必须都大于0");
            else if (result.SkuIds.Count() != result.Counts.Count())
                throw new InvalidPropertyException("商品数量不一致");
            else if (model.RecieveAddressId <= 0)
                throw new InvalidPropertyException("收货地址无效");
            else
                return result;
        }

        /// <summary>
        /// 获得限时购订单提交数据对像
        /// </summary>
        /// <returns></returns>
        public static OrderCreateModel GetGroupOrder(
            long userid,
            string skuIds,
            string counts,
            long recieveAddressId,
            int invoiceType,
            string invoiceTitle,
            string invoiceContext,
            long activeId,
            PlatformType platformType,
            long groupId = 0,
            bool isCashOnDelivery = false,
            string orderRemarks = "",
            long invitationUserId = 0
            )
        {
            var skuIdsArr = skuIds.Split(',');
            var pCountsArr = counts.TrimEnd(',').Split(',').Select(t => int.Parse(t));
            var productService = _iProductService;
            var orderService = _iOrderService;
            IEnumerable<long> collocationPidArr = null;
            if (string.IsNullOrWhiteSpace(skuIds) || string.IsNullOrWhiteSpace(counts))
                throw new Himall.Core.HimallException("创建订单的时候，SKU为空，或者数量为0");
            if (userid <= 0)
                throw new InvalidPropertyException("会员Id无效");
            OrderCreateModel model = new OrderCreateModel();
            model.SkuIds = skuIdsArr;
            model.Counts = pCountsArr;
            model.CurrentUser = MemberApplication.GetMember(userid);
            model.Integral = 0;
            model.IsCashOnDelivery = isCashOnDelivery;
            model.OrderRemarks = orderRemarks.Split(',');
            model.Invoice = (InvoiceType)invoiceType;
            model.InvoiceTitle = invoiceTitle;
            model.InvoiceContext = invoiceContext;
            model.CollPids = collocationPidArr;
            model.ReceiveAddressId = recieveAddressId;
            model.IslimitBuy = false;
            model.ActiveId = activeId;
            model.GroupId = groupId;
            model.PlatformType = platformType;
            model.InvitationUserId = invitationUserId;
            if (model.Counts.Count() == 0)
            {
                throw new InvalidPropertyException("待提交订单的商品数量不能为空");
            }

            if (model.Counts.Count(item => item <= 0) > 0)
            {
                throw new InvalidPropertyException("待提交订单的商品数量必须都大于0");
            }

            if (model.SkuIds.Count() != model.Counts.Count())
            {
                throw new InvalidPropertyException("商品数量不一致");
            }

            if (recieveAddressId <= 0)
            {
                throw new InvalidPropertyException("收货地址无效");
            }

            if (activeId <= 0)
            {
                throw new InvalidPropertyException("无效的拼团ID");
            }

            if (groupId > 0)
            {
                var gpobj = FightGroupApplication.GetGroup(activeId, groupId);
                if (gpobj == null)
                {
                    throw new InvalidPropertyException("无效的团信息");
                }
                if (gpobj.BuildStatus != CommonModel.FightGroupBuildStatus.Ongoing)
                {
                    throw new InvalidPropertyException("拼团当前状态无法参团");
                }
            }
            return model;

        }



        /// <summary>
        /// 限时购缓存提交订单
        /// </summary>
        public static string LimitRedisSubmit(OrderCreateModel model)
        {
            //string id = "";
            //SubmitOrderResult r = LimitOrderHelper.SubmitOrder(model, out id);
            //if (r == SubmitOrderResult.SoldOut)
            //    throw new HimallException("已售空");
            //else if (r == SubmitOrderResult.NoSkuId)
            //    throw new Himall.Core.InvalidPropertyException("创建订单的时候，SKU为空，或者数量为0");
            //else if (r == SubmitOrderResult.NoData)
            //    throw new Himall.Core.InvalidPropertyException("参数错误");
            //else if (string.IsNullOrEmpty(id))
            //    throw new Himall.Core.InvalidPropertyException("参数错误");
            //else
            //    return id;
            throw new NotImplementedException();
        }

        /// <summary>
        /// 数据库直接提交订单
        /// </summary>
        public static long[] OrderSubmit(OrderCreateModel model)
        {
            var orders = _iOrderService.CreateOrder(model);
            long[] orderIds = orders.Select(item => item.Id).ToArray();
            return orderIds;
        }
        public static long[] SmallProgOrderSubmit(OrderCreateModel model,out long groupId)
        {
            groupId = 0;
            var orders = _iOrderService.CreateOrder(model);
            long[] orderIds = orders.Select(item => item.Id).ToArray();
            groupId = orders.FirstOrDefault().GroupId;
            return orderIds;
        }
        /// <summary>
        /// 更新用户关系
        /// </summary>
        /// <param name="promotionids"></param>
        /// <param name="userid"></param>
        public static void UpdateDistributionUserLink(long[] promotionids, long userid)
        {
            MemberApplication.UpdateDistributionUserLink(promotionids, userid);
        }
        #endregion

        #region mobile公共方法
        /// <summary>
        /// 获得立即购买提交页面数据
        /// </summary>
        /// <param name="skuIds">库存ID集合</param>
        /// <param name="counts">库存ID对应的数量</param>
        /// <returns>数据</returns>
        public static MobileOrderDetailConfirmModel GetMobileSubmit(long userid, string skuIds, string counts)
        {
            if (string.IsNullOrEmpty(skuIds))
                throw new InvalidPropertyException("待提交订单的商品ID不能为空");
            if (string.IsNullOrEmpty(counts))
                throw new InvalidPropertyException("待提交订单的商品数量不能为空");
            MobileOrderDetailConfirmModel result = new MobileOrderDetailConfirmModel();
            result.InvoiceContext = _iOrderService.GetInvoiceContexts();
            //获取收货地址
            GetShippingAddress(userid, result);

            //获取购买商品信息
            var sku = skuIds.Split(',').Select(item => item);
            var count = counts.Split(',').Select(item => int.Parse(item));
            GetOrderProductsInfo(userid, sku, count, result);
            result.Sku = skuIds;
            result.Count = counts;
            return result;
        }

        /// <summary>
        /// 进入购物车提交页面
        /// </summary>
        /// <param name="cartItemIds">购物车旬</param>
        /// <returns></returns>
        public static MobileOrderDetailConfirmModel GetMobileSubmiteByCart(long userid, string cartItemIds)
        {
            MobileOrderDetailConfirmModel result = new MobileOrderDetailConfirmModel();
            result.InvoiceContext = _iOrderService.GetInvoiceContexts();
            GetShippingAddress(userid, result);
            GetOrderProductsInfo(userid, cartItemIds, result);
            return result;
        }

        /// <summary>
        /// 组合购提交页面
        /// </summary>
        /// <param name="cartItemIds">购物车旬</param>
        /// <returns></returns>
        public static MobileOrderDetailConfirmModel GetMobileCollocationBuy(long userid, string skuIds, string counts, long? regionId, string collpids = null)
        {
            if (string.IsNullOrEmpty(collpids))
                throw new InvalidPropertyException("组合构ID不能为空");
            MobileOrderDetailConfirmModel result = new MobileOrderDetailConfirmModel();
            result.InvoiceContext = _iOrderService.GetInvoiceContexts();
            GetShippingAddress(userid, result);
            string[] skus = skuIds.Split(',');
            string[] countarr = counts.Split(',');
            int[] cs = new int[countarr.Length];
            for (int i = 0; i < countarr.Length; i++)
            {
                cs[i] = int.Parse(countarr.ElementAt(i));
            }

            string[] colarr = collpids.Split(',');
            if (colarr.Count() == 0)
                throw new InvalidPropertyException("组合构ID不能为空");
            int[] ps = new int[colarr.Length];
            for (int i = 0; i < colarr.Length; i++)
            {
                ps[i] = int.Parse(colarr[i]);
            }


            GetOrderProductInfoColl(userid, skus, cs, ps, result);
            return result;
        }

        /// <summary>
        /// 积分支付
        /// </summary>
        /// <param name="orderIds">订单id</param>
        /// <param name="userid">用户id</param>
        public static void PayOrderByIntegral(string orderIds, long userid)
        {
            var orderIdArr = orderIds.Split(',').Select(item => long.Parse(item));
            _iOrderService.ConfirmZeroOrder(orderIdArr, userid);
        }

        /// <summary>
        /// 使用积分支付的订单取消
        /// </summary>
        /// <param name="orderIds">订单id</param>
        /// <param name="userid">用户id</param>
        public static void CancelOrder(string orderIds, long userid)
        {
            var orderIdArr = orderIds.Split(',').Select(item => long.Parse(item));
            _iOrderService.CancelOrders(orderIdArr, userid);

        }

        /// <summary>
        /// 是否全部抵扣
        /// </summary>
        /// <param name="integral">积分</param>
        /// <param name="total">总共需要积分</param>
        /// <param name="userid">用户标识</param>
        /// <returns>抵扣是否成功</returns>
        public static bool IsAllDeductible(int integral, decimal total, long userid)
        {
            if (integral == 0) //没使用积分时的0元订单
                return false;
            var result = _iOrderService.GetIntegralDiscountAmount(integral, userid);
            if (result < total)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 添加微店购买数量
        /// </summary>
        /// <param name="orderIds">订单ID</param>
        public static void AddVshopBuyNumber(IEnumerable<long> orderIds)
        {
            var shopIds = _iOrderService.GetOrders(orderIds).Select(item => item.ShopId).ToList();//从订单信息获取店铺id
            List<long> vshopIds = new List<long>();
            foreach (var item in shopIds)
            {
                var vshop = VshopApplication.GetVShopByShopId(item);
                if (vshop != null)
                {
                    vshopIds.Add(vshop.Id);
                }
            }
            foreach (var vshopId in vshopIds)
            {
                VshopApplication.AddBuyNumber(vshopId);
            }
        }

        /// <summary>
        /// 根据用户获收获地址列表
        /// </summary>
        /// <param name="userid">用户id</param>
        /// <returns>收获地址列表</returns>
        public static List<ShippingAddressInfo> GetUserAddresses(long userid)
        {
            return ShippingAddressApplication.GetUserShippingAddressByUserId(userid).ToList();
        }

        /// <summary>
        /// 设置用户默认收货地址
        /// </summary>
        /// <param name="addrId">地址Id</param>
        /// <param name="userid">用户Id</param>
        public static void SetDefaultUserShippingAddress(long addrId, long userid)
        {
            ShippingAddressApplication.SetDefaultShippingAddress(addrId, userid);
        }

        /// <summary>
        /// 获取指定收获地址的信息
        /// </summary>
        /// <param name="addressId">收获地址Id</param>
        /// <returns>收获地址信息</returns>
        public static ShippingAddressInfo GetUserAddress(long addressId)
        {
            var ShipngInfo = new ShippingAddressInfo();
            if (addressId != 0)
            {
                ShipngInfo = ShippingAddressApplication.GetUserShippingAddress(addressId);
            }
            return ShipngInfo;
        }

        /// <summary>
        /// 删除指定的收获地址信息
        /// </summary>
        /// <param name="addressId">收获地址Id</param>
        public static void DeleteShippingAddress(long addressId, long userid)
        {
            ShippingAddressApplication.DeleteShippingAddress(addressId, userid);
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="orderId">订单Id</param>
        /// <param name="userid">用户Id</param>
        /// <param name="username">用户名</param>
        /// <returns>是否成功</returns>
        public static bool CloseOrder(long orderId, long userid, string username)
        {
            var order = _iOrderService.GetOrder(orderId, userid);
            if (order != null)
            {
                _iOrderService.MemberCloseOrder(orderId, username);
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 确认订单收货
        /// </summary>
        public static int ConfirmOrder(long orderId, long userId, string username)
        {
            var order = _iOrderService.GetOrder(orderId, userId);
            if (order.OrderStatus == OrderInfo.OrderOperateStatus.Finish)
            {
                return 1;
                //throw new HimallException("该订单已经确认过!");
            }
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitReceiving && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp)
            {
                return 2;
                //throw new HimallException("订单状态发生改变，请重新刷页面操作!");
            }
            _iOrderService.MembeConfirmOrder(orderId, username);
            return 0;
        }
        /// <summary>
        /// 门店核销订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="managerName"></param>
        public static void ShopBranchConfirmOrder(long orderId, long shopBranchId, string managerName)
        {
            _iOrderService.ShopBranchConfirmOrder(orderId, shopBranchId, managerName);
        }
        /// <summary>
        /// 获取订单详细信息
        /// </summary>
        /// <param name="id">订单Id</param>
        /// <param name="userid">用户Id</param>
        /// <param name="type">平台类型</param>
        /// <param name="host">网站host地址</param>
        /// <returns>订单详细信息</returns>
        public static OrderDetailView Detail(long id, long userid, PlatformType type, string host)
        {
            OrderInfo order = _iOrderService.GetOrder(id, userid);
            var shopinfo = ShopApplication.GetShopInfo(order.ShopId);
            var vshop = shopinfo.Himall_VShop.FirstOrDefault() ?? new VShopInfo() { Id = 0 };
            bool isCanApply = false;
            bool IsRefundTimeOut = false;
            var _ordrefobj = RefundApplication.GetOrderRefundByOrderId(id) ?? new OrderRefundInfo { Id = 0 };
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp)
            {
                _ordrefobj = new OrderRefundInfo { Id = 0 };
            }
            int? ordrefstate = (_ordrefobj == null ? null : (int?)_ordrefobj.SellerAuditStatus);
            ordrefstate = (ordrefstate > 4 ? (int?)_ordrefobj.ManagerConfirmStatus : ordrefstate);
            //获取订单商品项数据
            var orderDetail = new OrderDetail()
            {

                ShopName = shopinfo.ShopName,
                ShopId = order.ShopId,
                VShopId = vshop.Id,
                RefundStats = ordrefstate,
                OrderRefundId = _ordrefobj.Id,
                OrderItems = order.OrderItemInfo.Select(item =>
                {
                    var productinfo = _iProductService.GetProduct(item.ProductId);
                    if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || order.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                    {
                        isCanApply = RefundApplication.CanApplyRefund(id, item.Id);
                    }
                    else
                    {
                        isCanApply = RefundApplication.CanApplyRefund(id, item.Id, false);
                    }
                    var itemrefund = item.OrderRefundInfo.FirstOrDefault(d => d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                    int? itemrefstate = (itemrefund == null ? null : (int?)itemrefund.SellerAuditStatus);
                    itemrefstate = (itemrefstate > 4 ? (int?)itemrefund.ManagerConfirmStatus : itemrefstate);
                    return new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Count = item.Quantity,
                        Price = item.SalePrice,
                        ProductImage = productinfo.GetImage(ImageSize.Size_100),
                        Id = item.Id,
                        Unit = productinfo.MeasureUnit,
                        IsCanRefund = isCanApply,
                        Color = item.Color,
                        Size = item.Size,
                        Version = item.Version,
                        RefundStats = itemrefstate,
                        OrderRefundId = (itemrefund == null ? 0 : itemrefund.Id),
                        EnabledRefundAmount = item.EnabledRefundAmount
                    };
                })
            };
            OrderDetailView view = new OrderDetailView();
            IsRefundTimeOut = _iOrderService.IsRefundTimeOut(id);
            view.Detail = orderDetail;
            view.Bonus = null;
            if (type == Core.PlatformType.WeiXin)
            {
                var bonusmodel = ShopBonusApplication.GetGrantByUserOrder(id, userid);
                if (bonusmodel != null)
                {
                    view.Bonus = bonusmodel;
                    view.ShareHref = "http://" + host + "/m-weixin/shopbonus/index/" + ShopBonusApplication.GetGrantIdByOrderId(id);
                }
            }
            view.Order = order;

            view.FightGroupCanRefund = true;
            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)  //拼团状态补充
            {
                var fgord = FightGroupApplication.GetFightGroupOrderStatusByOrderId(order.Id);
                view.FightGroupJoinStatus = CommonModel.FightGroupOrderJoinStatus.JoinFailed;
                if (fgord != null)
                {
                    view.FightGroupJoinStatus = fgord.GetJoinStatus;
                    view.FightGroupCanRefund = fgord.CanRefund;
                }
            }

            view.IsRefundTimeOut = IsRefundTimeOut;
            return view;
        }

        /// <summary>
        /// 是否超过售后期
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static bool IsRefundTimeOut(DTO.Order order)
        {
            return _iOrderService.IsRefundTimeOut(order.Map<Model.OrderInfo>());
        }

        /// <summary>
        /// 获取快递信息
        /// </summary>
        /// <param name="orderId">订单Id</param>
        /// <param name="userid">用户Id</param>
        /// <returns>快递信息 [0]:快递公司 [1]:单号</returns>
        public static string[] GetExpressInfo(long orderId, long userid)
        {
            OrderInfo order = _iOrderService.GetOrder(orderId, userid);
            string[] result = new string[2];
            if (order != null)
            {
                result[0] = order.ExpressCompanyName;
                result[1] = order.ShipOrderNumber;
            }
            return result;
        }
        #endregion
        /// <summary>
        /// 计算会员折扣价
        /// </summary>
        /// <param name="discount"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        static void CalculateDiscountPrice(decimal discount, List<CartItemModel> cartItems)
        {
            foreach (var item in cartItems)
            {
                if (item.IsLimit) continue;//如果是限时购，则不处理折扣价
                item.price = Math.Round(item.price * discount, 2);//折扣价为四舍五入保留两位小数
            }
        }
        #region mobile私有方法
        /// <summary>
        /// 获取订单相关的产品信息
        /// </summary>
        /// <param name="userid">用户id</param>
        /// <param name="skuIds">库存id</param>
        /// <param name="counts">数量</param>
        /// <param name="confirmModel">保存数据的实体</param>
        static void GetOrderProductsInfo(long userid, IEnumerable<string> skuIds, IEnumerable<int> counts, MobileOrderDetailConfirmModel confirmModel)
        {
            int cityId = 0;
            var address = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(userid);
            if (address != null)
            {
                // cityId = RegionApplication.GetRegion(address.RegionId, Region.RegionLevel.City).Id;
                cityId = address.RegionId;
            }
            var products = GenerateCartItem(skuIds, counts);
            var shopList = products.GroupBy(a => a.shopId);
            var member = MemberApplication.GetMember(userid);
            List<MobileShopCartItemModel> list = new List<MobileShopCartItemModel>();
            foreach (var shopcartItem in shopList)
            {
                IEnumerable<long> productIds = shopcartItem.Select(r => r.id);
                IEnumerable<int> productCounts = shopcartItem.Select(r => r.count);
                MobileShopCartItemModel item = new MobileShopCartItemModel();
                item.shopId = shopcartItem.Key;
                item.CartItemModels = products.Where(a => a.shopId == item.shopId).ToList();
                if (cityId > 0)
                {
                    item.Freight = _iProductService.GetFreight(productIds, productCounts, cityId);
                }

                var shop = ShopApplication.GetShopInfo(item.shopId);
                if (shop.IsSelf)
                {
                    //只有官方自营店商品，才有会员折扣，会员折扣优先级高于满减、优惠券
                    //计算会员折扣
                    CalculateDiscountPrice(member.MemberDiscount, item.CartItemModels);
                }
                item.IsSelf = shop.IsSelf;
                //计算满额减的金额
                item.FullDiscount = GetShopFullDiscount(item.CartItemModels);
                //满足优惠券(商品总金额除去满额减金额)
                item.OneCoupons = GetDefaultCoupon(item.shopId, userid, item.ShopTotalWithoutFreight);
                decimal ordPaidPrice = CalculatePaidPrice(item);
                item.ShopName = shop.ShopName;
                //满额免
                SetFullFree(ordPaidPrice, shop.FreeFreight, item);
                item.VshopId = shop.Himall_VShop.FirstOrDefault() != null ? shop.Himall_VShop.FirstOrDefault().Id : 0;
                list.Add(item);
            }
            var totalUserCoupons = list.Where(a => a.OneCoupons != null).Sum(b => b.OneCoupons.BasePrice);
            confirmModel.products = list;
            //  confirmModel.totalAmount = products.Sum(item => item.price * item.count);

            confirmModel.totalAmount = products.Sum(item => item.price * item.count) - list.Sum(a => a.FullDiscount);
            confirmModel.Freight = list.Sum(a => a.Freight);
            confirmModel.orderAmount = confirmModel.totalAmount + confirmModel.Freight - totalUserCoupons;
            var memberIntegralInfo = _iMemberIntegralService.GetMemberIntegral(userid);
            var memberIntegral = memberIntegralInfo == null ? 0 : memberIntegralInfo.AvailableIntegrals;
            //confirmModel.memberIntegralInfo = memberIntegralInfo;
            OrderIntegralModel integral = GetAvailableIntegral(confirmModel.totalAmount, totalUserCoupons, memberIntegral);
            confirmModel.integralPerMoney = integral.IntegralPerMoney;
            confirmModel.userIntegrals = integral.UserIntegrals;
        }

        static void GetOrderProductInfoColl(long userid, IEnumerable<string> skuIds, int[] counts, int[] collpids, MobileOrderDetailConfirmModel confirmModel)
        {
            int cityId = 0;
            var address = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(userid);
            if (address != null)
            {
                // cityId = RegionApplication.GetRegion(address.RegionId, Region.RegionLevel.City).Id;
                cityId = address.RegionId;
            }
            var products = GenerateCartItem(skuIds, counts);
            var shopList = products.GroupBy(a => a.shopId);
            List<MobileShopCartItemModel> list = new List<MobileShopCartItemModel>();
            foreach (var shopcartItem in shopList)
            {
                IEnumerable<long> productIds = shopcartItem.Select(r => r.id);
                IEnumerable<int> productCounts = shopcartItem.Select(r => r.count);
                MobileShopCartItemModel item = new MobileShopCartItemModel();
                item.shopId = shopcartItem.Key;
                item.CartItemModels = products.Where(a => a.shopId == item.shopId).ToList();
                foreach (CartItemModel cartitemmodel in item.CartItemModels)
                {
                    var sku = _iProductService.GetSku(cartitemmodel.skuId);
                    if (sku == null)
                        throw new HimallException("未找到库存!");
                    long collpid = 0;
                    for (int i = 0; i < skuIds.Count(); i++)
                    {
                        if (skuIds.ElementAt(i) == cartitemmodel.skuId)
                        {
                            collpid = collpids.ElementAt(i);
                            break;
                        }
                    }

                    cartitemmodel.price = GetSalePrice(cartitemmodel.id, sku, collpid, skuIds.Count(), userid);
                }
                if (cityId > 0)
                {
                    item.Freight = _iProductService.GetFreight(productIds, productCounts, cityId);
                }
                item.OneCoupons = GetDefaultCoupon(item.shopId, userid, shopcartItem.Sum(a => a.price * a.count));
                decimal ordPaidPrice = CalculatePaidPrice(item);
                var shop = ShopApplication.GetShopInfo(item.shopId);
                item.ShopName = shop.ShopName;

                //计算满额减的金额
                item.FullDiscount = GetShopFullDiscount(item.CartItemModels);
                //满额免
                SetFullFree(ordPaidPrice, shop.FreeFreight, item);
                item.VshopId = shop.Himall_VShop.FirstOrDefault() != null ? shop.Himall_VShop.FirstOrDefault().Id : 0;
                list.Add(item);
            }
            var totalUserCoupons = list.Where(a => a.OneCoupons != null).Sum(b => b.OneCoupons.BasePrice);
            confirmModel.products = list;
            confirmModel.totalAmount = products.Sum(item => item.price * item.count) - list.Sum(a => a.FullDiscount);
            confirmModel.Freight = list.Sum(a => a.Freight);
            confirmModel.orderAmount = confirmModel.totalAmount + confirmModel.Freight - totalUserCoupons;
            var memberIntegralInfo = _iMemberIntegralService.GetMemberIntegral(userid);
            var memberIntegral = memberIntegralInfo == null ? 0 : memberIntegralInfo.AvailableIntegrals;
            //confirmModel.memberIntegralInfo = memberIntegralInfo;
            OrderIntegralModel integral = GetAvailableIntegral(confirmModel.totalAmount, totalUserCoupons, memberIntegral);
            confirmModel.integralPerMoney = integral.IntegralPerMoney;
            confirmModel.userIntegrals = integral.UserIntegrals;
        }

        /// <summary>
        /// 获取订单相关的产品信息-拼团
        /// </summary>
        /// <param name="model"></param>
        /// <param name="skuId"></param>
        /// <param name="count"></param>
        /// <param name="userid">用户id</param>
        /// <param name="GroupActionId">拼团活动编号</param>
        static void GetOrderProductsInfoOnGroup(string skuId, int count, long userid, long GroupActionId, MobileOrderDetailConfirmModel confirmModel, long? groupId = null)
        {
            int cityId = 0;
            var address = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(userid);
            if (address != null)
            {
                address.RegionFullName = RegionApplication.GetFullName(address.RegionId);
                //  cityId = RegionApplication.GetRegion(address.RegionId, Region.RegionLevel.City).Id;
                cityId = address.RegionId;
            }
            confirmModel.Address = address;
            var products = GenerateGroupItem(GroupActionId, skuId, count, groupId);

            var shopList = products.GroupBy(a => a.shopId);
            List<MobileShopCartItemModel> list = new List<MobileShopCartItemModel>();
            foreach (var shopcartItem in shopList)
            {
                IEnumerable<long> productIds = shopcartItem.Select(r => r.id);
                IEnumerable<int> productCounts = shopcartItem.Select(r => r.count);
                MobileShopCartItemModel item = new MobileShopCartItemModel();
                item.shopId = shopcartItem.Key;
                item.CartItemModels = products.Where(a => a.shopId == item.shopId).ToList();
                foreach (var product in item.CartItemModels)
                {
                    product.imgUrl = Core.HimallIO.GetRomoteImagePath(product.imgUrl);
                }
                if (cityId > 0)
                {
                    item.Freight = _iProductService.GetFreight(productIds, productCounts, cityId);
                }

                item.OneCoupons = null; //不可以使用优惠券
                decimal ordPaidPrice = CalculatePaidPrice(item);
                var shop = ShopApplication.GetShopInfo(item.shopId);
                item.ShopName = shop.ShopName;
                item.IsSelf = shop.IsSelf;
                //计算满额减的金额
                item.FullDiscount = GetShopFullDiscount(item.CartItemModels);
                //满额免
                SetFullFree(ordPaidPrice, shop.FreeFreight, item);
                item.VshopId = shop.Himall_VShop.FirstOrDefault() != null ? shop.Himall_VShop.FirstOrDefault().Id : 0;
                list.Add(item);
            }
            var totalUserCoupons = 0; //不可以使用优惠券
            confirmModel.products = list;
            confirmModel.totalAmount = products.Sum(item => item.price * item.count) - list.Sum(e => e.FullDiscount);
            confirmModel.Freight = list.Sum(a => a.Freight);
            confirmModel.orderAmount = confirmModel.totalAmount + confirmModel.Freight - totalUserCoupons;

            //不可以使用积分
            confirmModel.integralPerMoney = 0;
            confirmModel.userIntegrals = 0;
        }

        /// <summary>
        /// 获取订单相关的产品信息
        /// </summary>
        /// <param name="userid">用户id</param>
        /// <param name="cartItemIds">购物车的物品id</param>
        /// <param name="confirmModel">保存数据的实体</param>
        static void GetOrderProductsInfo(long userid, string cartItemIds, MobileOrderDetailConfirmModel confirmModel)
        {
            IEnumerable<ShoppingCartItem> cartItems = null;
            if (string.IsNullOrWhiteSpace(cartItemIds))
                cartItems = GetCart(userid, "").Items;
            else
            {
                var cartItemIdsArr = cartItemIds.Split(',').Select(t => long.Parse(t));
                cartItems = CartApplication.GetCartItems(cartItemIdsArr);
            }
            int cityId = 0;
            var address = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(userid);
            if (address != null)
            {
                cityId = ServiceProvider.Instance<IRegionService>.Create.GetRegion(address.RegionId).Id;
            }
            var member = MemberApplication.GetMember(userid);
            var products = GenerateCartItem(cartItems);
            var shopList = products.GroupBy(a => a.shopId);
            List<MobileShopCartItemModel> list = new List<MobileShopCartItemModel>();
            foreach (var shopcartItem in shopList)
            {
                IEnumerable<long> productIds = shopcartItem.Select(r => r.id);
                IEnumerable<int> productCounts = shopcartItem.Select(r => r.count);
                MobileShopCartItemModel item = new MobileShopCartItemModel();
                item.shopId = shopcartItem.Key;
                if (VshopApplication.GetVShopByShopId(item.shopId) == null)
                    item.VshopId = 0;
                else
                    item.VshopId = VshopApplication.GetVShopByShopId(item.shopId).Id;
                item.CartItemModels = products.Where(a => a.shopId == item.shopId).ToList();

                if (cityId > 0)
                {
                    item.Freight = _iProductService.GetFreight(productIds, productCounts, cityId);
                }

                var shop = ShopApplication.GetShopInfo(item.shopId);
                if (shop.IsSelf)
                {//只有官方自营店商品，才有会员折扣，会员折扣优先级高于满减、优惠券
                    //计算会员折扣
                    CalculateDiscountPrice(member.MemberDiscount, item.CartItemModels);
                }
                item.IsSelf = shop.IsSelf;
                //计算满额减的金额
                item.FullDiscount = GetShopFullDiscount(item.CartItemModels);
                //满足优惠券(商品总金额除去满额减金额)
                item.OneCoupons = GetDefaultCoupon(item.shopId, userid, item.ShopTotalWithoutFreight);
                decimal ordPaidPrice = CalculatePaidPrice(item);
                item.ShopName = shop.ShopName;
                //满额免
                SetFullFree(ordPaidPrice, shop.FreeFreight, item);

                list.Add(item);
            }
            var totalUserCoupons = list.Where(a => a.OneCoupons != null).Sum(b => b.OneCoupons.BasePrice);


            confirmModel.products = list;
            confirmModel.totalAmount = products.Sum(item => item.price * item.count) - list.Sum(a => a.FullDiscount);
            confirmModel.Freight = list.Sum(a => a.Freight);
            confirmModel.orderAmount = confirmModel.totalAmount + confirmModel.Freight - totalUserCoupons;

            var memberIntegralInfo = _iMemberIntegralService.GetMemberIntegral(userid);
            var memberIntegral = memberIntegralInfo == null ? 0 : memberIntegralInfo.AvailableIntegrals;

            confirmModel.memberIntegralInfo = memberIntegralInfo;
            OrderIntegralModel integral = GetAvailableIntegral(confirmModel.totalAmount, totalUserCoupons, memberIntegral);
            confirmModel.integralPerMoney = integral.IntegralPerMoney;
            confirmModel.userIntegrals = integral.UserIntegrals;
        }

        /// <summary>
        /// 获取收获地址
        /// </summary>
        /// <param name="userid">用户id</param>
        /// <param name="confirm">保存数据的实体</param>
        static void GetShippingAddress(long userid, MobileOrderDetailConfirmModel confirm)
        {
            var address = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(userid);
            if (address != null)
            {
                bool hasRegion = PaymentConfigApplication.IsCashOnDelivery(address.RegionId);
                var isEnable = PaymentConfigApplication.IsEnable();
                if (hasRegion && isEnable)
                {
                    confirm.IsCashOnDelivery = true;
                }
                else
                {
                    confirm.IsCashOnDelivery = false;
                }
            }
            else
            {
                confirm.IsCashOnDelivery = false;
            }
            confirm.Address = address;
        }

        /// <summary>
        /// 在无法手动选择优惠券的场景下，自动选择合适的优惠券
        /// </summary>
        public static BaseCoupon GetDefaultCoupon(long shopid, long userid, decimal totalPrice)
        {
            var shopBonus = ShopBonusApplication.GetDetailToUse(shopid, userid, totalPrice);
            var userCoupons = CouponApplication.GetUserCoupon(shopid, userid, totalPrice);
            BaseCoupon c;
            if (shopBonus.Count() > 0 && userCoupons.Count() > 0)
            {

                var sb = shopBonus.FirstOrDefault();      //商家红包
                var uc = userCoupons.FirstOrDefault();  //优惠卷
                if (sb.BasePrice > uc.BasePrice)
                {
                    c = new BaseCoupon();
                    c.BaseEndTime = sb.BaseEndTime;
                    c.BaseId = sb.BaseId;
                    c.BaseName = sb.BaseName;
                    c.BasePrice = sb.BasePrice;
                    c.BaseShopId = sb.BaseShopId;
                    c.BaseShopName = sb.BaseShopName;
                    c.BaseType = sb.BaseType.GetHashCode() == 0 ? Himall.DTO.CouponType.Coupon : Himall.DTO.CouponType.ShopBonus;
                    c.OrderAmount = sb.Himall_ShopBonusGrant.Himall_ShopBonus.UsrStatePrice;
                    //超过优惠券金额，使用优惠券最大金额
                    if (c.BasePrice >= totalPrice)
                        c.BasePrice = totalPrice;

                    return c;
                }
                else
                {
                    c = new BaseCoupon();
                    c.BaseEndTime = uc.BaseEndTime;
                    c.BaseId = uc.BaseId;
                    c.BaseName = uc.BaseName;
                    c.BasePrice = uc.BasePrice;
                    c.BaseShopId = uc.BaseShopId;
                    c.BaseShopName = uc.BaseShopName;
                    c.BaseType = uc.BaseType.GetHashCode() == 0 ? Himall.DTO.CouponType.Coupon : Himall.DTO.CouponType.ShopBonus;
                    c.OrderAmount = uc.Himall_Coupon.OrderAmount;
                    if (c.BasePrice >= totalPrice)
                        c.BasePrice = totalPrice;
                    return c;
                }
            }
            else if (shopBonus.Count() <= 0 && userCoupons.Count() <= 0)
            {
                return null;
            }
            else if (shopBonus.Count() <= 0 && userCoupons.Count() > 0)
            {
                var coupon = userCoupons.FirstOrDefault();
                c = new BaseCoupon();
                c.BaseEndTime = coupon.BaseEndTime;
                c.BaseId = coupon.BaseId;
                c.BaseName = coupon.BaseName;
                c.BasePrice = coupon.BasePrice;
                c.BaseShopId = coupon.BaseShopId;
                c.BaseShopName = coupon.BaseShopName;
                c.BaseType = coupon.BaseType.GetHashCode() == 0 ? Himall.DTO.CouponType.Coupon : Himall.DTO.CouponType.ShopBonus;
                c.OrderAmount = coupon.Himall_Coupon.OrderAmount;
                if (c.BasePrice >= totalPrice)
                    c.BasePrice = totalPrice;
                return c;
            }
            else if (shopBonus.Count() > 0 && userCoupons.Count() <= 0)
            {
                var coupon = shopBonus.FirstOrDefault();
                c = new BaseCoupon();
                c.BaseEndTime = coupon.BaseEndTime;
                c.BaseId = coupon.BaseId;
                c.BaseName = coupon.BaseName;
                c.BasePrice = coupon.BasePrice;
                c.BaseShopId = coupon.BaseShopId;
                c.BaseShopName = coupon.BaseShopName;
                c.BaseType = coupon.BaseType.GetHashCode() == 0 ? Himall.DTO.CouponType.Coupon : Himall.DTO.CouponType.ShopBonus;
                c.OrderAmount = coupon.Himall_ShopBonusGrant.Himall_ShopBonus.UsrStatePrice;
                if (c.BasePrice >= totalPrice)
                    c.BasePrice = totalPrice;
                return c;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 满额免运费
        /// </summary>
        static void SetFullFree(decimal ordPaidPrice, decimal freeFreight, MobileShopCartItemModel item)
        {
            item.IsFreeFreight = false;
            if (freeFreight > 0)
            {
                item.FreeFreight = freeFreight;
                if (ordPaidPrice >= freeFreight)
                {
                    item.Freight = 0;
                    item.IsFreeFreight = true;
                }
            }
        }

        /// <summary>
        /// 计算积分
        /// </summary>
        static OrderIntegralModel GetAvailableIntegral(decimal totalAmount, decimal totalUserCoupons, decimal memberIntegral)
        {
            var integralPerMoney = _iMemberIntegralService.GetIntegralChangeRule();

            OrderIntegralModel result = new OrderIntegralModel();
            if (integralPerMoney != null && integralPerMoney.IntegralPerMoney > 0)
            {
                if ((totalAmount - totalUserCoupons) - Math.Round(memberIntegral / (decimal)integralPerMoney.IntegralPerMoney, 2) > 0)
                {
                    result.IntegralPerMoney = Math.Round(memberIntegral / (decimal)integralPerMoney.IntegralPerMoney, 2);
                    result.UserIntegrals = memberIntegral;
                }
                else
                {
                    //result.IntegralPerMoney = Math.Round(totalAmount - totalUserCoupons, 2);
                    result.UserIntegrals = Math.Floor((totalAmount - totalUserCoupons) * integralPerMoney.IntegralPerMoney);
                    result.IntegralPerMoney = Math.Round(result.UserIntegrals / integralPerMoney.IntegralPerMoney, 2);
                }
                if (result.IntegralPerMoney <= 0)
                {
                    result.IntegralPerMoney = 0;
                    result.UserIntegrals = 0;
                }
            }
            else
            {
                result.IntegralPerMoney = 0;
                result.UserIntegrals = 0;
            }

            return result;
        }

        /// <summary>
        /// 计算需付款
        /// </summary>
        static decimal CalculatePaidPrice(MobileShopCartItemModel cart)
        {
            decimal ordTotalPrice = cart.ShopTotalWithoutFreight;
            decimal ordDisPrice = cart.OneCoupons == null ? 0 : cart.OneCoupons.BasePrice;
            return ordTotalPrice - ordDisPrice;
        }

        #endregion

        #region 公共方法
        /// <summary>
        /// 订单完成订单数据写入待结算表
        /// </summary>
        /// <param name="o"></param>
        public static void WritePendingSettlnment(OrderInfo o)
        {
            _iOrderService.WritePendingSettlnment(o);
        }

        /// <summary>
        /// 获取昨天订单交易金额
        /// </summary>
        /// <param name="shopId">店铺ID平台不需要填写</param>
        /// <returns></returns>
        public static decimal GetYesterDaySaleAmount(long? shopId = null)
        {
            return _iOrderService.GetYesterDaySaleAmount(shopId);
        }

        /// <summary>
        /// 昨天下单订单数
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static int GetYesterDayOrdersNum(long? shopId = null)
        {
            return _iOrderService.GetYesterDayOrdersNum(shopId);
        }

        /// <summary>
        /// 昨天付款订单数
        /// </summary>
        /// <returns></returns>
        public static int GetYesterDayPayOrdersNum(long? shopId = null)
        {
            return _iOrderService.GetYesterDayPayOrdersNum(shopId);
        }

        public static int GetWaitingForPayOrders(long? shopId = null, long? shopBranchId = null)
        {
            OrderQuery orderQuery = new OrderQuery();
            orderQuery.ShopId = shopId;
            orderQuery.Status = OrderInfo.OrderOperateStatus.WaitPay;
            orderQuery.ShopBranchId = shopBranchId;
            var list = _iOrderService.GetOrdersNoPage(orderQuery);
            return list.Count;
        }
        public static int GetWaitingForReceive(long? shopId = null, long? shopBranchId = null)
        {
            OrderQuery orderQuery = new OrderQuery();
            orderQuery.ShopId = shopId;
            orderQuery.Status = OrderInfo.OrderOperateStatus.WaitReceiving;
            orderQuery.ShopBranchId = shopBranchId;
            var list = _iOrderService.GetOrdersNoPage(orderQuery);
            return list.Count;
        }
        public static int GetWaitingForDelivery(long? shopId = null, long? shopBranchId = null)
        {
            OrderQuery orderQuery = new OrderQuery();
            orderQuery.ShopId = shopId;
            orderQuery.Status = OrderInfo.OrderOperateStatus.WaitDelivery;
            orderQuery.ShopBranchId = shopBranchId;
            var list = _iOrderService.GetOrdersNoPage(orderQuery);
            return list.Count;
        }
        public static int GetWaitingForSelfPickUp(long? shopId = null, long? shopBranchId = null)
        {
            OrderQuery orderQuery = new OrderQuery();
            orderQuery.ShopId = shopId;
            orderQuery.Status = OrderInfo.OrderOperateStatus.WaitSelfPickUp;
            orderQuery.ShopBranchId = shopBranchId;
            var list = _iOrderService.GetOrdersNoPage(orderQuery);
            return list.Count;
        }
        /// <summary>
        /// 商家给订单备注
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="remark"></param>
        /// <param name="shopId">店铺ID</param>
        /// <param name="flag">紧急标识</param>
        public static void UpdateSellerRemark(long orderId, long shopId, string remark, int flag)
        {
            _iOrderService.UpdateSellerRemark(orderId, shopId, remark, flag);
        }

        /// <summary>
        /// 根据订单id获取OrderPayInfo
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public static List<DTO.OrderPay> GetOrderPays(IEnumerable<long> orderIds)
        {
            var list = _iOrderService.GetOrderPays(orderIds);
            return AutoMapper.Mapper.Map<List<DTO.OrderPay>>(list);
        }

        /// <summary>
        /// 根据订单项id获取订单项
        /// </summary>
        /// <param name="orderItemIds"></param>
        /// <returns></returns>
        public static List<DTO.OrderItem> GetOrderItemsByOrderItemId(IEnumerable<long> orderItemIds)
        {
            var list = _iOrderService.GetOrderItemsByOrderItemId(orderItemIds);
            return list.Map<List<DTO.OrderItem>>();
        }

        /// <summary>
        /// 根据订单id获取订单项
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static List<OrderItem> GetOrderItemsByOrderId(long orderId)
        {
            var list = _iOrderService.GetOrderItemsByOrderId(orderId);
            return list.Map<List<DTO.OrderItem>>();
        }

        /// <summary>
        /// 根据订单id获取订单项
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public static List<OrderItem> GetOrderItemsByOrderId(IEnumerable<long> orderIds)
        {
            var list = _iOrderService.GetOrderItemsByOrderId(orderIds);
            return list.Map<List<DTO.OrderItem>>();
        }

        /// <summary>
        /// 获取订单的评论数
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public static Dictionary<long, int> GetOrderCommentCount(IEnumerable<long> orderIds)
        {
            return _iOrderService.GetOrderCommentCount(orderIds);
        }

        /// <summary>
        /// 根据订单项id获取售后记录
        /// </summary>
        /// <param name="orderItemIds"></param>
        /// <returns></returns>
        public static List<DTO.OrderRefund> GetOrderRefunds(IEnumerable<long> orderItemIds)
        {
            var result = _iOrderService.GetOrderRefunds(orderItemIds).Map<List<DTO.OrderRefund>>();
            return result;
        }

        /// <summary>
        /// 商家发货
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="companyName"></param>
        /// <param name="shipOrderNumber"></param>
        /// <param name="kuaidi100ReturnUrl"></param>
        public static void SellerSendGood(long orderId, string sellerName, string companyName, string shipOrderNumber, string kuaidi100ReturnUrl = "")
        {
            var order = _iOrderService.SellerSendGood(orderId, sellerName, companyName, shipOrderNumber);

            var key = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().Kuaidi100Key;
            if (!string.IsNullOrEmpty(key))
            {
                Task.Factory.StartNew(() => ServiceProvider.Instance<IExpressService>.Create.SubscribeExpress100(order.ExpressCompanyName, order.ShipOrderNumber, key, order.RegionFullName, kuaidi100ReturnUrl));
            }

            //发送通知消息
            var orderMessage = new MessageOrderInfo();
            orderMessage.OrderTime = order.OrderDate;
            orderMessage.OrderId = order.Id.ToString();
            orderMessage.ShopId = order.ShopId;
            orderMessage.UserName = order.UserName;
            orderMessage.ShopName = order.ShopName;
            orderMessage.SiteName = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().SiteName;
            orderMessage.TotalMoney = order.OrderTotalAmount;
            orderMessage.ShippingCompany = companyName;
            orderMessage.ShippingNumber = shipOrderNumber;
            orderMessage.ShipTo = order.ShipTo + " " + order.RegionFullName + " " + order.Address;
            orderMessage.ProductName = order.OrderItemInfo.FirstOrDefault().ProductName;

            Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnOrderShipping(order.UserId, orderMessage));
        }

        /// <summary>
        /// 门店发货
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="companyName"></param>
        /// <param name="shipOrderNumber"></param>
        /// <param name="kuaidi100ReturnUrl"></param>
        public static void ShopSendGood(long orderId, int deliveryType, string shopkeeperName, string companyName, string shipOrderNumber, string kuaidi100ReturnUrl = "")
        {
            var order = _iOrderService.ShopSendGood(orderId, deliveryType, shopkeeperName, companyName, shipOrderNumber);
            if (deliveryType != 2)
            {
                var key = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().Kuaidi100Key;
                if (!string.IsNullOrEmpty(key))
                {
                    Task.Factory.StartNew(() => ServiceProvider.Instance<IExpressService>.Create.SubscribeExpress100(order.ExpressCompanyName, order.ShipOrderNumber, key, order.RegionFullName, kuaidi100ReturnUrl));
                }
            }
            //发送通知消息
            var orderMessage = new MessageOrderInfo();
            orderMessage.OrderTime = order.OrderDate;
            orderMessage.OrderId = order.Id.ToString();
            orderMessage.ShopId = order.ShopId;
            orderMessage.UserName = order.UserName;
            orderMessage.ShopName = order.ShopName;
            orderMessage.SiteName = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().SiteName;
            orderMessage.TotalMoney = order.OrderTotalAmount;
            orderMessage.ShippingCompany = companyName;
            orderMessage.ShippingNumber = shipOrderNumber;
            orderMessage.ShipTo = order.ShipTo + " " + order.RegionFullName + " " + order.Address;
            orderMessage.ProductName = order.OrderItemInfo.FirstOrDefault().ProductName;

            Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnOrderShipping(order.UserId, orderMessage));
        }
        /// <summary>
        /// 判断订单是否正在申请售后
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static bool IsOrderAfterService(long orderId)
        {
            return _iOrderService.IsOrderAfterService(orderId);
        }

        public static ExpressData GetExpressData(string expressCompanyName, string shipOrderNumber)
        {
            return _iExpressService.GetExpressData(expressCompanyName, shipOrderNumber);
        }

        /// <summary>
        /// 修改快递信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="companyName"></param>
        /// <param name="shipOrderNumber"></param>
        /// <returns></returns>
        public static void UpdateExpress(long orderId, string companyName, string shipOrderNumber, string kuaidi100ReturnUrl = "")
        {
            var order = _iOrderService.UpdateExpress(orderId, companyName, shipOrderNumber);

            var key = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().Kuaidi100Key;
            if (!string.IsNullOrEmpty(key))
            {
                Task.Factory.StartNew(() => ServiceProvider.Instance<IExpressService>.Create.SubscribeExpress100(order.ExpressCompanyName, order.ShipOrderNumber, key, order.RegionFullName, kuaidi100ReturnUrl));
            }
        }
        /// <summary>
        /// 所有订单是否都支付
        /// </summary>
        /// <param name="orderids"></param>
        /// <returns></returns>
        public static bool AllOrderIsPaied(string orderids)
        {
            var orders = _iOrderService.GetOrders(orderids.Split(',').Select(t => long.Parse(t)));
            IEnumerable<OrderInfo> waitPayOrders = orders.Where(p => p.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay);
            if (waitPayOrders.Count() > 0)
            {//有待付款的订单，则未支付完成
                return false;
            }
            return true;
        }
        #endregion

        #region web私有方法

        /// <summary>
        /// 对PaymentId进行加密（因为PaymentId中包含小数点"."，因此进行编码替换）
        /// </summary>
        static string EncodePaymentId(string paymentId)
        {
            return paymentId.Replace(".", "-");
        }
        /// <summary>
        /// 取得商品描述字符串
        /// </summary>
        /// <param name="orders">商品对象集合</param>
        /// <returns>描述字符串</returns>
        static string GetProductNameDescriptionFromOrders(IEnumerable<OrderInfo> orders)
        {
            List<string> productNames = new List<string>();
            foreach (var order in orders.ToList())
                productNames.AddRange(order.OrderItemInfo.Select(t => t.ProductName));
            string productInfos = "";
            if (productNames.Count > 0)
                productInfos = productNames.Count() > 1 ? (productNames.ElementAt(0) + " 等" + productNames.Count() + "种商品") : productNames.ElementAt(0);
            return productInfos;
        }

        /// <summary>
        /// 将前端传入参数转换成适合操作的格式
        /// </summary>
        static IEnumerable<string[]> ConvertUsedCoupon(string couponIds)
        {
            //couponIds格式  "id_type,id_type,id_type"
            IEnumerable<string> couponArr = null;
            if (!string.IsNullOrEmpty(couponIds))
            {
                couponArr = couponIds.Split(',');
            }

            //返回格式  string[0] = id , string[1] = type
            return couponArr == null ? null : couponArr.Select(p => p.Split('_'));
        }


        static void GetOrderProductsInfo(OrderSubmitModel model, string cartItemIds, long? regionId, long userid, string cartInfo)
        {
            ShippingAddressInfo address = new ShippingAddressInfo();
            if (regionId != null)
            {
                address = ShippingAddressApplication.GetUserShippingAddress((long)regionId);
            }
            else
            {
                address = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(userid);
            }
            int cityId = 0;
            if (address != null)
            {
                cityId = address.RegionId;
            }

            IEnumerable<ShoppingCartItem> cartItems = null;
            if (string.IsNullOrWhiteSpace(cartItemIds))
                cartItems = GetCart(userid, cartInfo).Items;
            else
            {
                var cartItemIdsArr = cartItemIds.Split(',').Select(t => long.Parse(t));
                cartItems = CartApplication.GetCartItems(cartItemIdsArr);
            }

            var products = GenerateCartItem(cartItems);
            var shopList = products.GroupBy(a => a.shopId);
            List<ShopCartItemModel> list = new List<ShopCartItemModel>();
            var orderService = _iOrderService;

            decimal discount = model.Member.MemberDiscount;

            foreach (var shopcartItem in shopList)
            {
                IEnumerable<long> productIds = shopcartItem.Select(r => r.id);
                IEnumerable<int> counts = shopcartItem.Select(r => r.count);

                ShopCartItemModel item = new ShopCartItemModel();
                item.shopId = shopcartItem.Key;
                var shopInfo = ShopApplication.GetShop(item.shopId);
                item.CartItemModels = products.Where(a => a.shopId == item.shopId).ToList();
                var shop = ShopApplication.GetShopInfo(item.shopId);
                item.ShopName = shop.ShopName;
                item.FreeFreight = shop.FreeFreight;
                if (cityId > 0)
                {
                    item.Freight = _iProductService.GetFreight(productIds, counts, cityId);
                }
                //会员折扣
                foreach (var c in item.CartItemModels)
                {
                    var price = c.price * discount;
                    c.price = shopInfo.IsSelf ? price : c.price;
                }

                //计算满额减的金额
                item.FullDiscount = GetShopFullDiscount(item.CartItemModels);

                //优惠券
                item.BaseCoupons = GetBaseCoupon(item.shopId, userid, item.ShopTotalWithoutFreight);

                var OrderItems = GetOrderItems(item);
                item.freightProductGroup = OrderItems.OrderBy(e => e.FreightTemplateId).ToList();
                list.Add(item);
            }
            model.products = list;
            model.totalAmount = products.Sum(item => item.price * item.count) - list.Sum(a => a.FullDiscount);
            model.Freight = list.Sum(a => a.Freight);
        }

        static void SetOrderProductsInfo(OrderSubmitModel model, string skuIds, string counts, long userid, string collIds = null)
        {
            var address = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(userid);
            int cityId = 0;
            if (address != null)
            {
                // cityId = RegionApplication.GetRegion(address.RegionId, Region.RegionLevel.City).Id;
                cityId = address.RegionId;
            }

            if (cityId <= 0)
            {
                //跳转填写收货地址
            }
            IEnumerable<long> CollPidArr = null;
            if (string.IsNullOrEmpty(skuIds))
                throw new HimallException("sku不能为空");
            var skuIdsArr = skuIds.Split(',');
            var pCountsArr = counts.TrimEnd(',').Split(',').Select(t => int.Parse(t));
            if (!string.IsNullOrEmpty(collIds))
            {
                CollPidArr = collIds.TrimEnd(',').Split(',').Select(t => long.Parse(t));
            }
            var productService = _iProductService;
            int index = 0;
            var skuCount = skuIdsArr.Length;//有多少个SKU就是多少个商品
            var products = skuIdsArr.Select(item =>
            {
                var sku = productService.GetSku(item);
                var count = pCountsArr.ElementAt(index);
                var collpid = CollPidArr != null ? CollPidArr.ElementAt(index) : 0;
                index++;
                var skuprice = GetSalePrice(sku.ProductInfo.Id, sku, collpid, skuCount, userid);
                return new CartItemModel()
                {
                    skuId = item,
                    id = sku.ProductInfo.Id,
                    imgUrl = sku.ProductInfo.GetImage(ImageSize.Size_50),
                    name = sku.ProductInfo.ProductName,
                    shopId = sku.ProductInfo.ShopId,
                    price = skuprice,
                    count = count,
                    productCode = sku.ProductInfo.ProductCode,
                    collpid = collpid
                };
            }).ToList();
            var shopList = products.GroupBy(a => a.shopId);
            List<ShopCartItemModel> list = new List<ShopCartItemModel>();
            foreach (var shopcartItem in shopList)
            {
                IEnumerable<long> productIds = shopcartItem.Select(r => r.id);
                IEnumerable<int> productCounts = shopcartItem.Select(r => r.count);

                ShopCartItemModel item = new ShopCartItemModel();
                item.shopId = shopcartItem.Key;
                item.CartItemModels = products.Where(a => a.shopId == item.shopId).ToList();
                var shop = ShopApplication.GetShopInfo(item.shopId);
                item.ShopName = shop.ShopName;
                item.FreeFreight = shop.FreeFreight;
                if (cityId > 0)
                {
                    item.Freight = _iProductService.GetFreight(productIds, productCounts, cityId);
                }

                //计算满额减的金额
                item.FullDiscount = GetShopFullDiscount(item.CartItemModels);
                //优惠券
                item.BaseCoupons = GetBaseCoupon(item.shopId, userid, item.ShopTotalWithoutFreight);
                var orderItems = GetOrderItems(item);

                item.freightProductGroup = orderItems.OrderBy(e => e.FreightTemplateId).ToList();
                list.Add(item);
            }
            model.products = list;
            model.totalAmount = products.Sum(item => item.price * item.count) - list.Sum(a => a.FullDiscount);
            model.Freight = list.Sum(a => a.Freight);
        }
        /// <summary>
        /// 获取售价
        /// <para>己计算会员折</para>
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="sku"></param>
        /// <param name="collid"></param>
        /// <param name="Count"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        static decimal GetSalePrice(long productId, SKUInfo sku, long? collid, int Count, long? userId = null)
        {
            var price = sku.SalePrice;

            #region 会员折
            decimal discount = 1;  //默认无折扣
            if (userId.HasValue && userId > 0)
            {
                var user = MemberApplication.GetMember(userId.Value);
                var shopInfo = ShopApplication.GetShop(sku.ProductInfo.ShopId);
                if (shopInfo != null && shopInfo.IsSelf)
                {
                    discount = user.MemberDiscount;
                }
            }
            price = discount * price; //折扣价
            #endregion

            if (collid.HasValue && collid.Value != 0 && Count > 1)//组合购大于一个商品
            {
                var collsku = CollocationApplication.GetColloSku(collid.Value, sku.Id);
                if (collsku != null)
                {
                    price = collsku.Price;
                }
                //获取组合购的价格
            }
            else if (Count == 1) //只有一个商品可能是限时购
            {
                var limit = _iLimitTimeBuyService.GetDetail(sku.Id);

                if (limit != null)
                {
                    price = (decimal)limit.Price;
                }
            }
            return price;
        }

        static IEnumerable<OrderSubmitItemModel> GetOrderItems(ShopCartItemModel item)
        {
            var productService = _iProductService;
            var iTypeService = _iTypeService;
            var orderItems = item.CartItemModels.Select(r =>
            {
                var productcode = r.productCode;
                var skuinfo = productService.GetSku(r.skuId);
                if (skuinfo != null)
                {
                    if (!string.IsNullOrWhiteSpace(skuinfo.Sku))
                    {
                        productcode = skuinfo.Sku;
                    }
                }
                var product = productService.GetProduct(skuinfo.ProductId);
                //杨振国加的保证金标识，这里请重构
                var cashDeposit = CashDepositsApplication.GetCashDepositsObligation(skuinfo.ProductId);
                ProductTypeInfo typeInfo = iTypeService.GetTypeByProductId(skuinfo.ProductId);
                string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                return new OrderSubmitItemModel
                {
                    id = r.id,
                    ProductId = product.Id,
                    FreightTemplateId = product != null ? product.FreightTemplateId : 0,
                    price = r.price,
                    count = r.count,
                    skuId = r.skuId,
                    name = r.name,
                    productCode = productcode,
                    imgUrl = r.imgUrl,
                    //杨振国加的保证金标识，这里请重构
                    sevenDayNoReasonReturn = cashDeposit.IsSevenDayNoReasonReturn,
                    timelyShip = cashDeposit.IsTimelyShip,
                    customerSecurity = cashDeposit.IsCustomerSecurity,
                    skuColor = skuinfo.Color,
                    skuSize = skuinfo.Size,
                    skuVersion = skuinfo.Version,
                    colorAlias = colorAlias,
                    sizeAlias = sizeAlias,
                    versionAlias = versionAlias,
                    collpid = r.collpid
                };
            });

            return orderItems;
        }

        static ShippingAddressInfo GetShippingAddress(long? regionId, long userid)
        {
            if (regionId != null)
            {
                return ShippingAddressApplication.GetUserShippingAddress((long)regionId);
            }
            else
                return ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(userid);
        }

        /// <summary>
        /// 订单提交页面，需要展示的数据
        /// </summary>
        static List<CartItemModel> GenerateCartItem(IEnumerable<ShoppingCartItem> cartItems)
        {
            var productService = _iProductService;
            var products = cartItems.Select(item =>
            {
                var product = productService.GetProduct(item.ProductId);
                var sku = productService.GetSku(item.SkuId);

                ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetTypeByProductId(item.ProductId);
                string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

                return new CartItemModel()
                {
                    skuId = item.SkuId,
                    id = product.Id,
                    imgUrl = Core.HimallIO.GetRomoteProductSizeImage(product.RelativePath, 1, (int)ImageSize.Size_100),
                    name = product.ProductName,
                    price = sku.SalePrice,
                    shopId = product.ShopId,
                    count = item.Quantity,
                    productCode = product.ProductCode,
                    color = sku.Color,
                    size = sku.Size,
                    version = sku.Version,
                    IsSelf = product.Himall_Shops.IsSelf,
                    ColorAlias = colorAlias,
                    SizeAlias = sizeAlias,
                    VersionAlias = versionAlias
                };
            }).ToList();

            return products;
        }


        /// <summary>
        /// 获取购物车中的商品
        /// </summary>
        /// <returns></returns>
        static ShoppingCartInfo GetCart(long memberId, string cartInfo)
        {
            ShoppingCartInfo shoppingCartInfo;
            if (memberId > 0)//已经登录，系统从服务器读取购物车信息，否则从Cookie获取购物车信息
                shoppingCartInfo = CartApplication.GetCart(memberId);
            else
            {
                shoppingCartInfo = new ShoppingCartInfo();

                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    var cartInfoItems = new ShoppingCartItem[cartItems.Length];
                    int i = 0;
                    foreach (string cartItem in cartItems)
                    {
                        var cartItemParts = cartItem.Split(':');
                        cartInfoItems[i++] = new ShoppingCartItem() { ProductId = long.Parse(cartItemParts[0].Split('_')[0]), SkuId = cartItemParts[0], Quantity = int.Parse(cartItemParts[1]) };
                    }
                    shoppingCartInfo.Items = cartInfoItems;
                }
            }
            return shoppingCartInfo;
        }
        /// <summary>
        /// 订单提交页面，需要展示的数据
        /// </summary>
        static List<CartItemModel> GenerateCartItem(IEnumerable<string> skuIds, IEnumerable<int> counts)
        {
            int i = 0;
            var products = skuIds.Select(item =>
            {
                var sku = _iProductService.GetSku(item);
                var count = counts.ElementAt(i++);
                var ltmbuy = _iLimitTimeBuyService.GetLimitTimeMarketItemByProductId(sku.ProductInfo.Id);
                if (ltmbuy != null)
                {
                    if (count > ltmbuy.LimitCountOfThePeople)
                        throw new HimallException("超过最大限购数量：" + ltmbuy.LimitCountOfThePeople.ToString() + "");
                }
                if (sku.Stock < count)
                {
                    //throw new HimallException("库存不足");
                }
                ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetTypeByProductId(sku.ProductInfo.Id);
                string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                return new CartItemModel()
                {
                    skuId = item,
                    id = sku.ProductInfo.Id,
                    imgUrl = Core.HimallIO.GetRomoteProductSizeImage(sku.ProductInfo.RelativePath, 1, (int)(ImageSize.Size_100)),
                    name = sku.ProductInfo.ProductName,
                    shopId = sku.ProductInfo.ShopId,
                    price = ltmbuy == null ? sku.SalePrice : (decimal)_iLimitTimeBuyService.GetDetail(item).Price,
                    count = count,
                    productCode = sku.ProductInfo.ProductCode,
                    unit = sku.ProductInfo.MeasureUnit,
                    size = sku.Size,
                    color = sku.Color,
                    version = sku.Version,
                    IsSelf = sku.ProductInfo.Himall_Shops.IsSelf,
                    ColorAlias = colorAlias,
                    SizeAlias = sizeAlias,
                    VersionAlias = versionAlias,
                    IsLimit = ltmbuy != null
                };
            }).ToList();

            return products;
        }

        /// <summary>
        /// 订单提交页面，拼团数据组装
        /// </summary>
        /// <param name="actionId">活动编号</param>
        /// <param name="skuId">规格</param>
        /// <param name="count">数量</param>
        /// <returns></returns>
        static List<CartItemModel> GenerateGroupItem(long actionId, string skuId, int count, long? groupId = null)
        {
            bool isnewgroup = false;
            if (groupId > 0)
            {
                isnewgroup = true;
            }
            List<CartItemModel> result = new List<CartItemModel>();
            var actobj = FightGroupApplication.GetActive(actionId);

            var sku = actobj.ActiveItems.FirstOrDefault(d => d.SkuId == skuId);
            if (sku == null)
            {
                throw new HimallException("错误的规格信息");
            }
            if (count > actobj.LimitQuantity)
            {
                throw new HimallException("超过最大限购数量：" + actobj.LimitQuantity.ToString() + "");
            }
            if (sku.ActiveStock < count)
            {
                //throw new HimallException("库存不足");
            }
            if (isnewgroup)
            {
                if (actobj.ActiveStatus != CommonModel.FightGroupActiveStatus.Ongoing)
                {
                    throw new HimallException("拼团活动已结束，不可以开团");
                }
            }
            ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetTypeByProductId(sku.ProductId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            var data = new CartItemModel()
            {
                skuId = skuId,
                id = sku.ProductId,
                imgUrl = HimallIO.GetRomoteProductSizeImage(actobj.ProductImgPath, 1, (int)ImageSize.Size_100),
                name = actobj.ProductName,
                shopId = actobj.ShopId,
                price = sku.ActivePrice,
                count = count,
                productCode = actobj.ProductCode,
                unit = actobj.MeasureUnit,
                size = sku.Size,
                color = sku.Color,
                version = sku.Version,
                IsSelf = ShopApplication.IsSelfShop(actobj.ShopId),
                ColorAlias = colorAlias,
                SizeAlias = sizeAlias,
                VersionAlias = versionAlias
            };
            result.Add(data);

            return result;
        }

        /// <summary>
        /// 获取用户所有可用的优惠券
        /// </summary>
        static List<BaseCoupon> GetBaseCoupon(long shopId, long userId, decimal totalPrice)
        {
            var userCoupons = CouponApplication.GetUserCoupon(shopId, userId, totalPrice);
            var userBonus = ShopBonusApplication.GetDetailToUse(shopId, userId, totalPrice);
            List<BaseCoupon> coupons = new List<BaseCoupon>();
            foreach (var coupon in userCoupons)
            {
                BaseCoupon c = new BaseCoupon();
                c.BaseEndTime = coupon.BaseEndTime;
                c.BaseId = coupon.BaseId;
                c.BaseName = coupon.BaseName;
                c.BasePrice = coupon.BasePrice;
                c.BaseShopId = coupon.BaseShopId;
                c.BaseShopName = coupon.BaseShopName;
                c.BaseType = coupon.BaseType.GetHashCode() == 0 ? Himall.DTO.CouponType.Coupon : Himall.DTO.CouponType.ShopBonus;
                c.OrderAmount = coupon.Himall_Coupon.OrderAmount;
                coupons.Add(c);
            }
            foreach (var coupon in userBonus)
            {
                BaseCoupon c = new BaseCoupon();
                c.BaseEndTime = coupon.BaseEndTime;
                c.BaseId = coupon.BaseId;
                c.BaseName = coupon.BaseName;
                c.BasePrice = coupon.BasePrice;
                c.BaseShopId = coupon.BaseShopId;
                c.BaseShopName = coupon.BaseShopName;
                c.BaseType = coupon.BaseType.GetHashCode() == 0 ? Himall.DTO.CouponType.Coupon : Himall.DTO.CouponType.ShopBonus;
                c.OrderAmount = coupon.Himall_ShopBonusGrant.Himall_ShopBonus.UsrStatePrice;
                coupons.Add(c);
            }
            return coupons;
        }

        /// <summary>
        /// 支付完生成红包
        /// </summary>
        private static Dictionary<long, ShopBonusInfo> GenerateBonus(IEnumerable<long> orderIds, string urlHost)
        {
            Dictionary<long, ShopBonusInfo> bonusGrantIds = new Dictionary<long, ShopBonusInfo>();
            string url = "http://" + urlHost + "/m-weixin/shopbonus/index/";
            var buyOrders = _iOrderService.GetOrders(orderIds);
            foreach (var o in buyOrders)
            {
                var shopBonus = ShopBonusApplication.GetByShopId(o.ShopId);
                if (shopBonus == null)
                {
                    continue;
                }
                if (shopBonus.GrantPrice <= o.OrderTotalAmount)
                {
                    long grantid = ShopBonusApplication.GenerateBonusDetail(shopBonus, o.Id, url);
                    bonusGrantIds.Add(grantid, shopBonus);
                }
            }
            return bonusGrantIds;
        }

        /// <summary>
        /// 更改限时购销售量
        /// </summary>
        private static void IncreaseSaleCount(List<long> orderid)
        {
            if (orderid.Count == 1)
            {
                _iLimitTimeBuyService.IncreaseSaleCount(orderid);
            }
        }


        // 平台确认订单支付
        public static void PlatformConfirmOrderPay(long orderId, string payRemark, string managerName)
        {
            _iOrderService.PlatformConfirmOrderPay(orderId, payRemark, managerName);
        }




        /// <summary>
        /// 处理会员订单类别
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        public static void DealDithOrderCategoryByUserId(long orderId, long userId)
        {
            var orderItem = GetOrderItemsByOrderId(orderId);
            var productIds = orderItem.Select(p => p.ProductId);
            var product = ProductManagerApplication.GetProductsByIds(productIds);
            foreach (var item in product)
            {
                var categoryId = long.Parse(item.CategoryPath.Split('|')[0]);
                OrderAndSaleStatisticsApplication.SynchronizeMemberBuyCategory(categoryId, userId);
            }
        }
        /// <summary>
        /// 根据订单ID获取订单商品明细，包括商品店铺信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<OrderDetailView> GetOrderDetailViews(IEnumerable<long> ids)
        {
            var list = _iOrderService.GetOrders(ids).Map<List<FullOrder>>();
            List<OrderDetailView> orderDetails = new List<OrderDetailView>();
            var orderItems = GetOrderItemsByOrderId(list.Select(p => p.Id));//订单明细
            var shops = ShopApplication.GetShops(orderItems.Select(e => e.ShopId));//店铺信息
            var vShops = VshopApplication.GetVShopsByShopIds(orderItems.Select(e => e.ShopId));//微店信息
            foreach (var orderItem in orderItems)
            {//完善图片地址
                orderItem.ThumbnailsUrl = HimallIO.GetRomoteProductSizeImage(orderItem.ThumbnailsUrl, 1, (int)ImageSize.Size_350);
            }
            foreach (var item in list)
            {
                OrderDetailView detail = new OrderDetailView() { };
                var vshop = vShops.FirstOrDefault(e => e.ShopId == item.ShopId);
                long vshopId = 0;
                if (vshop != null)
                {
                    vshopId = vshop.Id;
                }
                detail.Detail = new OrderDetail
                {
                    ShopId = item.ShopId,
                    ShopName = shops.FirstOrDefault(e => e.Id == item.ShopId).ShopName,
                    VShopId = vshopId,
                    OrderItems = orderItems.Where(p => p.OrderId == item.Id).ToList()
                };
                orderDetails.Add(detail);
            }

            return orderDetails;
        }
        #endregion
        #region 商家手动分配门店
        /// <summary>
        /// 分配门店时更新商家、门店库存
        /// </summary>
        /// <param name="skuIds"></param>
        /// <param name="quantity"></param>
        public static void DistributionStoreUpdateStock(List<string> skuIds, List<int> counts, long shopBranchId)
        {
            _iOrderService.DistributionStoreUpdateStock(skuIds, counts, shopBranchId);
        }
        /// <summary>
        /// 分配门店订单到新门店
        /// </summary>
        /// <param name="skuIds"></param>
        /// <param name="newShopBranchId"></param>
        /// <param name="oldShopBranchId"></param>
        public static void DistributionStoreUpdateStockToNewShopBranch(List<string> skuIds, List<int> counts, long newShopBranchId, long oldShopBranchId)
        {
            _iOrderService.DistributionStoreUpdateStockToNewShopBranch(skuIds, counts, newShopBranchId, oldShopBranchId);
        }
        /// <summary>
        /// 分配门店订单回到商家
        /// </summary>
        /// <param name="skuIds"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="shopId"></param>
        public static void DistributionStoreUpdateStockToShop(List<string> skuIds, List<int> counts, long shopBranchId)
        {
            _iOrderService.DistributionStoreUpdateStockToShop(skuIds, counts, shopBranchId);
        }
        /// <summary>
        /// 更新订单所属门店
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shopBranchId"></param>
        public static void UpdateOrderShopBranch(long orderId, long shopBranchId)
        {
            _iOrderService.UpdateOrderShopBranch(orderId, shopBranchId);
        }

        public static IEnumerable<IExpress> GetAllExpress()
        {
            return _iExpressService.GetAllExpress();
        }
        #endregion
    }
}
