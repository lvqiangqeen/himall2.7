using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using Himall.CommonModel;
using Himall.CommonModel.Delegates;
using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.Core.Plugins.Payment;
using Himall.Entity;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using MySql.Data.MySqlClient;
using Dapper;
using System.Text;
using Himall.ServiceProvider;

namespace Himall.Service
{
    public class OrderService : ServiceBase, IOrderService
    {
        #region 静态字段
        private static readonly System.Security.Cryptography.RandomNumberGenerator _randomPickupCode = System.Security.Cryptography.RandomNumberGenerator.Create();
        #endregion

        #region 字段
        private IFightGroupService _iFightGroupService;
        private IShopService _iShopService;
        private IShippingAddressService _iShippingAddressService;
        private IRegionService _iRegionService;
        private IShopBranchService _iShopBranchService;
        private IAppMessageService _iAppMessageService;
        private ISiteSettingService _iSiteSettingService;
        //private ITypeService _iTypeService;
        #endregion

        #region 构造函数
        public OrderService()
        {
            /**  订单windows服务里调用此处有异常,无法注册 特注释掉
            //_iFightGroupService = ObjectContainer.Current.Resolve<IFightGroupService>();
            //_iShopService = ObjectContainer.Current.Resolve<IShopService>();
            //_iShippingAddressService = ObjectContainer.Current.Resolve<IShippingAddressService>();
            //_iRegionService = ObjectContainer.Current.Resolve<IRegionService>();
            //_iShopBranchService = ObjectContainer.Current.Resolve<IShopBranchService>();
            //_iAppMessageService = ObjectContainer.Current.Resolve<IAppMessageService>();
            //_iSiteSettingService = ObjectContainer.Current.Resolve<ISiteSettingService>();**/
            _iFightGroupService = Instance<IFightGroupService>.Create;
            _iShopService = Instance<IShopService>.Create;
            _iShippingAddressService = Instance<IShippingAddressService>.Create;
            _iRegionService = Instance<IRegionService>.Create;
            _iShopBranchService = Instance<IShopBranchService>.Create;
            _iAppMessageService = Instance<IAppMessageService>.Create;
            _iSiteSettingService = Instance<ISiteSettingService>.Create;
        }
        #endregion

        #region 属性
        public event OrderPaySuccessed OnOrderPaySuccessed;
        #endregion

        #region 方法
        #region 获取订单相关 done
        public ObsoletePageModel<OrderInfo> GetOrders<Tout>(OrderQuery query, Expression<Func<OrderInfo, Tout>> sort = null)
        {
            var orders = ToWhere(query);

            int total = 0;
            if (sort == null)
            {
                orders = orders.FindBy(item => item.Id > 0, query.PageNo, query.PageSize, out total, item => item.OrderDate, false).ToList().AsQueryable();

            }

            else
            {
                orders = orders.FindBy(item => item.Id > 0, query.PageNo, query.PageSize, out total, sort, true).ToList().AsQueryable();
            }

            foreach (var orderInfo in orders)
            {
                foreach (var itemInfo in orderInfo.OrderItemInfo)
                {
                    //ProductTypeInfo typeInfo = _iTypeService.GetTypeByProductId(itemInfo.ProductId);
                    ProductTypeInfo typeInfo = (ProductTypeInfo)Context.ProductTypeInfo.Join(Context.ProductInfo.Where(d => d.Id == itemInfo.ProductId), x => x.Id, y => y.TypeId, (x, y) => x).ToList().FirstOrDefault();
                    itemInfo.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    itemInfo.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    itemInfo.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                }
            }
            var pageModel = new ObsoletePageModel<OrderInfo>()
            {
                Models = orders,
                Total = total
            };

            return pageModel;
        }
        /// <summary>
        /// 商家用
        /// </summary>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="query"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public ObsoletePageModel<OrderInfo> GetOrdersOnshop<Tout>(OrderQuery query, Expression<Func<OrderInfo, Tout>> sort = null)
        {
            var orders = ToWhere(query);

            int total = 0;
            if (sort == null)
            {
                orders = orders.FindBy(item => item.Id > 0, query.PageNo, query.PageSize, out total, item => item.OrderDate, false).ToList().AsQueryable();
                if (query.ShopId > 0)
                    orders = orders.FindBy(item => item.Id > 0 && item.ShopId == query.ShopId, query.PageNo, query.PageSize, out total, item => item.OrderDate, false).ToList().AsQueryable();
                if (query.OrderId != null)
                    orders = orders.FindBy(item => item.Id > 0 && item.Id == long.Parse(query.OrderId), query.PageNo, query.PageSize, out total, item => item.OrderDate, false).ToList().AsQueryable();

            }

            else
            {
                orders = orders.FindBy(item => item.Id > 0, query.PageNo, query.PageSize, out total, sort, true).ToList().AsQueryable();
                if (query.ShopId > 0)
                    orders = orders.FindBy(item => item.Id > 0 && item.ShopId == query.ShopId, query.PageNo, query.PageSize, out total, sort, true).ToList().AsQueryable();
                if (query.OrderId != null)
                    orders = orders.FindBy(item => item.Id > 0 && item.Id == long.Parse(query.OrderId), query.PageNo, query.PageSize, out total, sort, true).ToList().AsQueryable();
            }

            foreach (var orderInfo in orders)
            {
                foreach (var itemInfo in orderInfo.OrderItemInfo)
                {
                    //ProductTypeInfo typeInfo = _iTypeService.GetTypeByProductId(itemInfo.ProductId);
                    ProductTypeInfo typeInfo = (ProductTypeInfo)Context.ProductTypeInfo.Join(Context.ProductInfo.Where(d => d.Id == itemInfo.ProductId), x => x.Id, y => y.TypeId, (x, y) => x).ToList().FirstOrDefault();
                    itemInfo.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    itemInfo.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    itemInfo.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                   
                }
            }
            //ObsoletePageModel<OrderInfo>  order2;
            //foreach (var orderInfo in orders)
            //{
            //    if(ServiceProvider.Instance<IFightGroupService>.Create.IsExistOrder(orderInfo.Id, orderInfo.ShopId))

            //}
            var pageModel = new ObsoletePageModel<OrderInfo>()
            {
                Models = orders,
                Total = total
            };

            return pageModel;
        }

        /// <summary>
        /// 分页获取订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<OrderInfo> GetOrders(OrderQuery query)
        {
            var orders = ToWhere(query);

            int total = 0;
            orders = orders.FindBy(item => item.Id > 0, query.PageNo, query.PageSize, out total, item => item.OrderDate, false);

            var pageModel = new QueryPageModel<OrderInfo>()
            {
                Models = orders.ToList(),
                Total = total
            };

            return pageModel;
        }

        /// <summary>
        /// 获取订单列表(忽略分页)
        /// </summary>
        /// <param name="orderQuery"></param>
        /// <returns></returns>
        public List<OrderInfo> GetOrdersNoPage(OrderQuery orderQuery)
        {
            var orders = ToWhere(orderQuery);

            return orders.ToList();
        }

        /// <summary>
        /// 获取增量订单
        /// </summary>
        /// <param name="orderQuery"></param>
        /// <returns></returns>
        public ObsoletePageModel<OrderInfo> GetOrdersByLastModifyTime(OrderQuery orderQuery)
        {
            long result;
            bool IsNumber = long.TryParse(orderQuery.SearchKeyWords, out result);
            //所有的子账号
            IList<long> childIds = new List<long>();

            Expression<Func<OrderInfo, bool>> where = item =>
                    (!orderQuery.ShopId.HasValue || orderQuery.ShopId == item.ShopId) &&
                    (orderQuery.ShopName == null || orderQuery.ShopName.Trim() == "" || item.ShopName.Contains(orderQuery.ShopName)) &&
                    (orderQuery.UserName == null || orderQuery.UserName.Trim() == "" || item.UserName.Contains(orderQuery.UserName)) &&
                    ((!orderQuery.UserId.HasValue || item.UserId == orderQuery.UserId)) &&
                    (orderQuery.PaymentTypeName == null || orderQuery.PaymentTypeName.Trim() == "" || item.PaymentTypeName.Contains(orderQuery.PaymentTypeName)) &&
                    (orderQuery.PaymentTypeGateway == null || orderQuery.PaymentTypeGateway.Trim() == "" || item.PaymentTypeGateway.Contains(orderQuery.PaymentTypeGateway))
                    && (orderQuery.SearchKeyWords == null || orderQuery.SearchKeyWords.Trim() == "" || (IsNumber && item.Id == result || item.OrderItemInfo.Any(a => a.ProductName.Contains(orderQuery.SearchKeyWords)) ||
                    IsNumber && item.OrderItemInfo.Any(a => a.ProductId == result))
                    )
                    && (!orderQuery.Commented.HasValue || (orderQuery.Commented.Value ? (item.OrderCommentInfo.Count > 0) : (item.OrderCommentInfo.Count == 0)));

            var orderIdRange = GetOrderIdRange(orderQuery.OrderId);
            if (orderIdRange != null)
            {
                var min = orderIdRange[0];
                if (orderIdRange.Length == 2)
                {
                    var max = orderIdRange[1];
                    where = where.And(item => item.Id >= min && item.Id <= max);
                }
                else
                    where = where.And(item => item.Id == min);
            }

            IQueryable<OrderInfo> orders = Context.OrderInfo.FindBy(where);

            int total = 0;
            if (orderQuery.Commented.HasValue && orderQuery.Commented.Value == false)
            {
                var pc = from p in Context.ProductCommentInfo
                         select p.Himall_OrderItems.OrderId;

                orders = orders.Where(item => !pc.Contains(item.Id));
            }
            //订单类型
            if (orderQuery.OrderType != null)
            {
                orders = orders.Where(item => (int)item.Platform == orderQuery.OrderType);
            }
            if (orderQuery.Status.HasValue)
            {
                var _where = orders.GetDefaultPredicate(false);
                switch (orderQuery.Status)
                {
                    case OrderInfo.OrderOperateStatus.UnComment:
                        _where = _where.Or(d => d.OrderCommentInfo.Count == 0 && d.OrderStatus == OrderInfo.OrderOperateStatus.Finish);
                        break;
                    case OrderInfo.OrderOperateStatus.WaitDelivery:
                        var _ordswhere = orders.GetDefaultPredicate(true);
                        //处理拼团的情况
                        _ordswhere = _ordswhere.And(d => d.OrderStatus == orderQuery.Status);
                        var fgordids = Context.FightGroupOrderInfo.Where(d => d.JoinStatus != 4).Select(d => d.OrderId);
                        _ordswhere = _ordswhere.And(d => !fgordids.Contains(d.Id));
                        _where = _where.Or(_ordswhere);
                        break;
                    default:
                        _where = _where.Or(d => d.OrderStatus == orderQuery.Status);
                        break;
                }


                if (orderQuery.MoreStatus != null)
                {
                    foreach (var stitem in orderQuery.MoreStatus)
                    {
                        _where = _where.Or(d => d.OrderStatus == stitem);
                    }
                }
                orders = orders.FindBy(_where);
            }

            if (orderQuery.PaymentType != OrderInfo.PaymentTypes.None)
            {
                orders = orders.Where(item => item.PaymentType == orderQuery.PaymentType);
            }

            //开始结束时间
            if (orderQuery.StartDate.HasValue)
            {
                DateTime sdt = orderQuery.StartDate.Value;
                orders = orders.Where(d => d.LastModifyTime >= sdt);
            }
            if (orderQuery.EndDate.HasValue)
            {
                DateTime edt = orderQuery.EndDate.Value.AddDays(1).AddSeconds(-1);
                orders = orders.Where(d => d.LastModifyTime <= edt);
            }

            orders = orders.FindBy(item => item.Id > 0, orderQuery.PageNo, orderQuery.PageSize, out total,
                item => item.OrderDate, false);

            var pageModel = new ObsoletePageModel<OrderInfo>()
            {
                Models = orders,
                Total = total
            };
            return pageModel;
        }

        public List<OrderInfo> GetOrders(IEnumerable<long> ids)
        {
            return Context.OrderInfo.Where(item => ids.Contains(item.Id)).ToList();
        }

        public OrderInfo GetOrder(long orderId, long userId)
        {
            var result = Context.OrderInfo.Where(a => a.Id == orderId && a.UserId == userId).FirstOrDefault();
            if (result != null && result.OrderStatus >= OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                CalculateOrderItemRefund(orderId);
            }
            return result;
        }

        public OrderInfo GetOrder(long orderId)
        {
            var result = Context.OrderInfo.FindById(orderId);
            return result;
        }

        public bool CreatePayCodeByOrderId(long orderId, string payCode)
        {
            OrderInfo model = Context.OrderInfo.FindById(orderId);
            if (model.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                throw new HimallException("只用待发货的订单才能创建消费码");
            }
            else
            {
                model.PayCode = payCode;
                return Context.SaveChanges() > 0;
            }
        }
        /// <summary>
        /// 商家根据消费码改变订单状态
        /// </summary>
        /// <param name="payCode">消费码</param>
        /// <returns></returns>
        public bool UpdateOrderStateByPayCode(string payCode,out string message)
        {
            OrderInfo model = Context.OrderInfo.Where(o=>o.PayCode==payCode).FirstOrDefault();
            if (model == null)
            {
                Log.Info("消费码输入有误！");
                message = "消费码输入有误";
                return false;
            }
            else if (model.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                Log.Info("请检查当前订单状态！");
                message = "请检查当前订单状态";
                return false;
            }
            
            model.OrderStatus = OrderInfo.OrderOperateStatus.WaitReceiving;
            message = "操作成功！";
            return Context.SaveChanges() > 0;
        }
        /// <summary>
        /// 是否存在订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shopId">店铺Id,0表示不限店铺</param>
        /// <returns></returns>
        public bool IsExistOrder(long orderId, long shopId = 0)
        {
            var sql = Context.OrderInfo.Where(d => d.Id == orderId);
            if (shopId > 0)
            {
                sql = sql.Where(d => d.ShopId == shopId);
            }
            return sql.Any();
        }

        public OrderInfo GetFirstFinishedOrderForSettlement()
        {
            var order = Context.OrderInfo.Where(c => c.OrderStatus == OrderInfo.OrderOperateStatus.Finish).OrderBy(c => c.FinishDate).FirstOrDefault();
            return order;
        }

        public IQueryable<OrderPayInfo> GetOrderPay(long id)
        {
            return Context.OrderPayInfo.Where(item => item.PayId == id);
        }

        public IQueryable<OrderInfo> GetTopOrders(int top, long userId)
        {
            return Context.OrderInfo.Where(a => a.UserId == userId).OrderByDescending(a => a.OrderDate).Take(top);
        }
        public IQueryable<OrderInfo> GetOrdersOnshop(int top, long shopid)
        {
            return Context.OrderInfo.Where(a => a.ShopId == shopid).OrderByDescending(a => a.OrderDate).Take(top);
        }
        public int GetFightGroupOrderByUser(long userId)
        {
            var fightOrderCount = (
               from p in Context.OrderInfo
               join o in Context.FightGroupOrderInfo on p.Id equals o.OrderId
               where p.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.WaitDelivery
               && o.JoinStatus < 4
               && o.OrderUserId == userId
               select new
               {
                   p.Id
               }
               ).Count();
            return fightOrderCount;
        }

        /// <summary>
        /// 根据订单项id获取订单项
        /// </summary>
        /// <param name="orderItemIds"></param>
        /// <returns></returns>
        public List<OrderItemInfo> GetOrderItemsByOrderItemId(IEnumerable<long> orderItemIds)
        {
            return Context.OrderItemInfo.Where(p => orderItemIds.Contains(p.Id)).ToList();
        }

        /// <summary>
        /// 根据订单id获取订单项
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public List<OrderItemInfo> GetOrderItemsByOrderId(long orderId)
        {
            return Context.OrderItemInfo.Where(p => p.OrderId == orderId).ToList();
        }

        /// <summary>
        /// 根据订单id获取订单项
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public List<OrderItemInfo> GetOrderItemsByOrderId(IEnumerable<long> orderIds)
        {
            return Context.OrderItemInfo.Where(p => orderIds.Contains(p.OrderId)).ToList();
        }

        /// <summary>
        /// 获取订单的评论数
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public Dictionary<long, int> GetOrderCommentCount(IEnumerable<long> orderIds)
        {
            return this.Context.OrderCommentInfo.Where(p => orderIds.Contains(p.OrderId)).GroupBy(p => p.OrderId).ToDictionary(p => p.Key, p => p.Count());
        }

        public SKUInfo GetSkuByID(string skuid)
        {
            return Context.SKUInfo.FindById(skuid);
        }

        //获取所有订单明细
        public int GetSuccessOrderCountByProductID(long productId = 0, OrderInfo.OrderOperateStatus orserStatus = OrderInfo.OrderOperateStatus.Finish)
        {
            ProductVistiInfo productVisti = Context.ProductVistiInfo.FindBy(p => p.ProductId == productId).FirstOrDefault();
            if (productVisti == null)
            {
                return 0;
            }
            return productVisti.OrderCounts == null ? 0 : (int)productVisti.OrderCounts;
        }

        /// <summary>
        /// 根据订单项id获取售后记录
        /// </summary>
        /// <param name="orderItemIds"></param>
        /// <returns></returns>
        public List<OrderRefundInfo> GetOrderRefunds(IEnumerable<long> orderItemIds)
        {
            return this.Context.OrderRefundInfo.Where(p => orderItemIds.Contains(p.OrderItemId)).ToList();
        }

        /// <summary>
        /// 获取商品已购数
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public Dictionary<long, int> GetProductBuyCount(long userId, IEnumerable<long> productIds)
        {
            return this.Context.OrderItemInfo.Where(p => productIds.Contains(p.ProductId) && p.OrderInfo.UserId == userId && p.OrderInfo.OrderStatus != OrderInfo.OrderOperateStatus.Close)
                .GroupBy(p => p.ProductId).ToDictionary(p => p.Key, p => (int)p.Sum(pp => pp.Quantity));
        }
        #endregion 获取订单相关 done

        #region 创建订单相关  done
        public List<OrderInfo> CreateOrder(OrderCreateModel model)
        {
            CheckWhenCreateOrder(model);

            //创建SKU列表对象
            var orderSkuInfos = GetOrderSkuInfo(model);

            //发票保存
            if (model.Invoice == InvoiceType.OrdinaryInvoices)
            {
                AddInvoiceTitle(model.InvoiceTitle, model.CurrentUser.Id);
            }

            //创建订单额外对象
            var additional = CreateAdditional(orderSkuInfos, model);

            //创建订单列表对象
            var infos = GetOrderInfos(orderSkuInfos, model, additional);

            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                StringBuilder ordersql = new StringBuilder();
                StringBuilder orderitemsql = new StringBuilder();
                DynamicParameters orderparms = new DynamicParameters();
                DynamicParameters orderitemparms = new DynamicParameters();
                int i = 0;
                int j = 0;
                #region 组装Sql
                foreach (var order in infos)
                {
                    ordersql.Append("INSERT INTO `himall_orders`(");
                    ordersql.Append("`Id`,`OrderStatus`, `OrderDate`, `CloseReason`, `ShopId`, `ShopName`, `SellerPhone`, `SellerAddress`, `SellerRemark`, ");
                    ordersql.Append("`SellerRemarkFlag`, `UserId`, `UserName`, `UserRemark`, `ShipTo`, `CellPhone`, `TopRegionId`, `RegionId`, `RegionFullName`, `Address`, `ExpressCompanyName`, ");
                    ordersql.Append("`Freight`, `ShipOrderNumber`, `ShippingDate`, `IsPrinted`, `PaymentTypeName`, `PaymentTypeGateway`, `PaymentType`, `GatewayOrderId`, `PayRemark`, `PayDate`, ");
                    ordersql.Append("`InvoiceType`, `InvoiceTitle`, `Tax`, `FinishDate`, `ProductTotalAmount`, `RefundTotalAmount`, `CommisTotalAmount`, `RefundCommisAmount`, `ActiveType`, `Platform`, ");
                    ordersql.Append("`DiscountAmount`, `IntegralDiscount`, `InvoiceContext`, `OrderType`, `ShareUserId`, `OrderRemarks`, `LastModifyTime`, `DeliveryType`, `ShopBranchId`, `PickupCode`, ");
                    ordersql.Append("`TotalAmount`, `ActualPayAmount`, `FullDiscount`");
                    ordersql.Append(") VALUES(");
                    ordersql.AppendFormat("@Id{0}, @OrderStatus{0}, @OrderDate{0}, @CloseReason{0}, @ShopId{0}, @ShopName{0}, @SellerPhone{0}, @SellerAddress{0}, @SellerRemark{0}, ", i.ToString());
                    ordersql.AppendFormat("@SellerRemarkFlag{0},@UserId{0},@UserName{0},@UserRemark{0},@ShipTo{0},@CellPhone{0},@TopRegionId{0},@RegionId{0},@RegionFullName{0},@Address{0},@ExpressCompanyName{0}, ", i.ToString());
                    ordersql.AppendFormat("@Freight{0},@ShipOrderNumber{0},@ShippingDate{0},@IsPrinted{0},@PaymentTypeName{0},@PaymentTypeGateway{0},@PaymentType{0},@GatewayOrderId{0},@PayRemark{0},@PayDate{0}, ", i.ToString());
                    ordersql.AppendFormat("@InvoiceType{0},@InvoiceTitle{0},@Tax{0},@FinishDate{0},@ProductTotalAmount{0},@RefundTotalAmount{0},@CommisTotalAmount{0},@RefundCommisAmount{0},@ActiveType{0},@Platform{0}, ", i.ToString());
                    ordersql.AppendFormat("@DiscountAmount{0},@IntegralDiscount{0},@InvoiceContext{0},@OrderType{0},@ShareUserId{0},@OrderRemarks{0},@LastModifyTime{0},@DeliveryType{0},@ShopBranchId{0},@PickupCode{0}, ", i.ToString());
                    ordersql.AppendFormat("@TotalAmount{0},@ActualPayAmount{0},@FullDiscount{0}", i.ToString());
                    ordersql.Append(");");
                    orderparms.Add("@Id" + i.ToString(), order.Id);
                    orderparms.Add("@OrderStatus" + i.ToString(), order.OrderStatus);
                    orderparms.Add("@OrderDate" + i.ToString(), order.OrderDate);
                    orderparms.Add("@CloseReason" + i.ToString(), order.CloseReason);
                    orderparms.Add("@ShopId" + i.ToString(), order.ShopId);
                    orderparms.Add("@ShopName" + i.ToString(), order.ShopName);
                    orderparms.Add("@SellerPhone" + i.ToString(), order.SellerPhone);
                    orderparms.Add("@SellerAddress" + i.ToString(), order.SellerAddress);
                    orderparms.Add("@SellerRemark" + i.ToString(), order.SellerRemark);
                    orderparms.Add("@SellerRemarkFlag" + i.ToString(), order.SellerRemarkFlag);
                    orderparms.Add("@UserId" + i.ToString(), order.UserId);
                    orderparms.Add("@UserName" + i.ToString(), order.UserName);
                    orderparms.Add("@UserRemark" + i.ToString(), order.UserRemark);
                    orderparms.Add("@ShipTo" + i.ToString(), order.ShipTo);
                    orderparms.Add("@CellPhone" + i.ToString(), order.CellPhone);
                    orderparms.Add("@TopRegionId" + i.ToString(), order.TopRegionId);
                    orderparms.Add("@RegionId" + i.ToString(), order.RegionId);
                    orderparms.Add("@RegionFullName" + i.ToString(), order.RegionFullName);
                    orderparms.Add("@Address" + i.ToString(), order.Address);
                    orderparms.Add("@ExpressCompanyName" + i.ToString(), order.ExpressCompanyName);
                    orderparms.Add("@Freight" + i.ToString(), order.Freight);
                    orderparms.Add("@ShipOrderNumber" + i.ToString(), order.ShipOrderNumber);
                    orderparms.Add("@ShippingDate" + i.ToString(), order.ShippingDate);
                    orderparms.Add("@IsPrinted" + i.ToString(), order.IsPrinted);
                    orderparms.Add("@PaymentTypeName" + i.ToString(), order.PaymentTypeName);
                    orderparms.Add("@PaymentTypeGateway" + i.ToString(), order.PaymentTypeGateway);
                    orderparms.Add("@PaymentType" + i.ToString(), order.PaymentType);
                    orderparms.Add("@GatewayOrderId" + i.ToString(), order.GatewayOrderId);
                    orderparms.Add("@PayRemark" + i.ToString(), order.PayRemark);
                    orderparms.Add("@PayDate" + i.ToString(), order.PayDate);
                    orderparms.Add("@InvoiceType" + i.ToString(), order.InvoiceType);
                    orderparms.Add("@InvoiceTitle" + i.ToString(), order.InvoiceTitle);
                    orderparms.Add("@Tax" + i.ToString(), order.Tax);
                    orderparms.Add("@FinishDate" + i.ToString(), order.FinishDate);
                    orderparms.Add("@ProductTotalAmount" + i.ToString(), order.ProductTotalAmount);
                    orderparms.Add("@RefundTotalAmount" + i.ToString(), order.RefundTotalAmount);
                    orderparms.Add("@CommisTotalAmount" + i.ToString(), order.CommisTotalAmount);
                    orderparms.Add("@RefundCommisAmount" + i.ToString(), order.RefundCommisAmount);
                    orderparms.Add("@ActiveType" + i.ToString(), order.ActiveType);
                    orderparms.Add("@Platform" + i.ToString(), order.Platform);
                    orderparms.Add("@DiscountAmount" + i.ToString(), order.DiscountAmount);
                    orderparms.Add("@IntegralDiscount" + i.ToString(), order.IntegralDiscount);
                    orderparms.Add("@InvoiceContext" + i.ToString(), order.InvoiceContext);
                    orderparms.Add("@OrderType" + i.ToString(), order.OrderType);
                    orderparms.Add("@ShareUserId" + i.ToString(), order.ShareUserId);
                    orderparms.Add("@OrderRemarks" + i.ToString(), order.OrderRemarks);
                    orderparms.Add("@LastModifyTime" + i.ToString(), order.LastModifyTime);
                    orderparms.Add("@DeliveryType" + i.ToString(), order.DeliveryType);
                    orderparms.Add("@ShopBranchId" + i.ToString(), order.ShopBranchId);
                    orderparms.Add("@PickupCode" + i.ToString(), order.PickupCode);
                    orderparms.Add("@TotalAmount" + i.ToString(), order.TotalAmount);
                    orderparms.Add("@ActualPayAmount" + i.ToString(), order.ActualPayAmount);
                    orderparms.Add("@FullDiscount" + i.ToString(), order.FullDiscount);

                    foreach (var orderitem in order.OrderItemInfo)
                    {
                        orderitemsql.Append("INSERT INTO `himall_orderitems` (");
                        orderitemsql.Append("`OrderId`, `ShopId`, `ProductId`, `SkuId`, `SKU`, `Quantity`, `ReturnQuantity`, `CostPrice`, `SalePrice`, `DiscountAmount`, `RealTotalPrice`, `RefundPrice`, ");
                        orderitemsql.Append("`ProductName`, `Color`, `Size`, `Version`, `ThumbnailsUrl`, `CommisRate`, `EnabledRefundAmount`, `IsLimitBuy`, `DistributionRate`, `EnabledRefundIntegral`, `CouponDiscount`, `FullDiscount`");
                        orderitemsql.Append(") VALUES(");
                        orderitemsql.AppendFormat("@OrderId_{0},@ShopId_{0},@ProductId_{0},@SkuId_{0},@SKU_{0},@Quantity_{0},@ReturnQuantity_{0},@CostPrice_{0},@SalePrice_{0},@DiscountAmount_{0},@RealTotalPrice_{0},@RefundPrice_{0}, ", j.ToString());
                        orderitemsql.AppendFormat("@ProductName_{0},@Color_{0},@Size_{0},@Version_{0},@ThumbnailsUrl_{0},@CommisRate_{0},@EnabledRefundAmount_{0},@IsLimitBuy_{0},@DistributionRate_{0},@EnabledRefundIntegral_{0},@CouponDiscount_{0},@FullDiscount_{0}", j.ToString());
                        orderitemsql.Append(");");
                        orderitemparms.Add("@OrderId_" + j.ToString(), orderitem.OrderId);
                        orderitemparms.Add("@ShopId_" + j.ToString(), orderitem.ShopId);
                        orderitemparms.Add("@ProductId_" + j.ToString(), orderitem.ProductId);
                        orderitemparms.Add("@SkuId_" + j.ToString(), orderitem.SkuId);
                        orderitemparms.Add("@SKU_" + j.ToString(), orderitem.SKU);
                        orderitemparms.Add("@Quantity_" + j.ToString(), orderitem.Quantity);
                        orderitemparms.Add("@ReturnQuantity_" + j.ToString(), orderitem.ReturnQuantity);
                        orderitemparms.Add("@CostPrice_" + j.ToString(), orderitem.CostPrice);
                        orderitemparms.Add("@SalePrice_" + j.ToString(), orderitem.SalePrice);
                        orderitemparms.Add("@DiscountAmount_" + j.ToString(), orderitem.DiscountAmount);
                        orderitemparms.Add("@RealTotalPrice_" + j.ToString(), orderitem.RealTotalPrice);
                        orderitemparms.Add("@RefundPrice_" + j.ToString(), orderitem.RefundPrice);
                        orderitemparms.Add("@ProductName_" + j.ToString(), orderitem.ProductName);
                        orderitemparms.Add("@Color_" + j.ToString(), orderitem.Color);
                        orderitemparms.Add("@Size_" + j.ToString(), orderitem.Size);
                        orderitemparms.Add("@Version_" + j.ToString(), orderitem.Version);
                        orderitemparms.Add("@ThumbnailsUrl_" + j.ToString(), orderitem.ThumbnailsUrl);
                        orderitemparms.Add("@CommisRate_" + j.ToString(), orderitem.CommisRate);
                        orderitemparms.Add("@EnabledRefundAmount_" + j.ToString(), orderitem.EnabledRefundAmount);
                        orderitemparms.Add("@IsLimitBuy_" + j.ToString(), orderitem.IsLimitBuy);
                        orderitemparms.Add("@DistributionRate_" + j.ToString(), orderitem.DistributionRate);
                        orderitemparms.Add("@EnabledRefundIntegral_" + j.ToString(), orderitem.EnabledRefundIntegral);
                        orderitemparms.Add("@CouponDiscount_" + j.ToString(), orderitem.CouponDiscount);
                        orderitemparms.Add("@FullDiscount_" + j.ToString(), orderitem.FullDiscount);
                        j++;
                    }

                    i++;
                }
                #endregion

                ordersql.Append(orderitemsql);
                orderparms.AddDynamicParams(orderitemparms);

                conn.Execute(ordersql.ToString(), orderparms);
            }

            if (model.CartItemIds != null && model.CartItemIds.Any())
            {
                var skuIds = orderSkuInfos.Select(a => a.SKU.Id).ToArray();
                Context.ShoppingCartItemInfo.Remove(item => skuIds.Contains(item.SkuId) && item.UserId == model.CurrentUser.Id);
                Context.SaveChanges();
            }

            //优惠券状态改变
            if (model.CouponIdsStr != null && model.CouponIdsStr.Count() > 0)
            {
                var couponService = ServiceProvider.Instance<ICouponService>.Create;
                UseCoupon(infos.ToList(), additional.BaseCoupons.ToList(), model.CurrentUser.Id);
                Context.SaveChanges();
            }

            foreach (var info in infos)
            {
                if (info.OrderType == OrderInfo.OrderTypes.FightGroup)
                {
                    //处理拼团
                    var ifgser = ServiceProvider.Instance<IFightGroupService>.Create;
                    var groupOrder = ifgser.AddOrder(model.ActiveId, info.Id, model.CurrentUser.Id, model.GroupId, model.ShopId, model.InvitationUserId);
                    info.GroupId = groupOrder.GroupId.Value;
                }

                //减少库存
                foreach (var orderItem in info.OrderItemInfo)
                {
                    if (info.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake)
                        UpdateShopBranchSku(info.ShopBranchId.Value, orderItem.SkuId, -(int)orderItem.Quantity);
                    else
                    {
                        using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
                        {
                            string sql = "update himall_skus set stock = stock - @Quantity where Id=@SkuId";
                            conn.Execute(sql, new { Quantity = orderItem.Quantity, SkuId = orderItem.SkuId });
                        }
                    }
                }
            }
            Context.SaveChanges();

            #region 系统处理订单自动分配门店
            if (_iSiteSettingService.GetSiteSettings() != null && _iSiteSettingService.GetSiteSettings().IsOpenStore)//如果开启门店授权
            {
                var shipAddressInfo = additional.Address;//获取当前订单收货地址对象
                ShopBranchQuery query = null;
                foreach (var p in infos)
                {
                    if ((p.ShopBranchId.HasValue && p.ShopBranchId.Value == 0) || (!p.ShopBranchId.HasValue))//如果订单之前不属于任何门店，则系统尝试匹配
                    {
                        var shopInfo = _iShopService.GetShop(p.ShopId);
                        bool autoAllotOrder = shopInfo != null && shopInfo.AutoAllotOrder.HasValue && shopInfo.AutoAllotOrder.Value;//每个订单所属商家的后台是否开启订单自动分配
                        if (!autoAllotOrder) continue;

                        if (shipAddressInfo != null)
                        {
                            query = new ShopBranchQuery()
                            {
                                ShopId = p.ShopId,
                                FromLatLng = string.Format("{0},{1}", shipAddressInfo.Latitude, shipAddressInfo.Longitude),
                                StreetId = p.RegionId//街道或区县(有的只有3级)
                            };
                            var parentAreaInfo = _iRegionService.GetRegion(p.RegionId, Region.RegionLevel.Town);//判断当前区域是否为第四级
                            if (parentAreaInfo != null && parentAreaInfo.ParentId > 0)
                                query.DistrictId = parentAreaInfo.ParentId;
                            else
                            {
                                query.DistrictId = query.StreetId;
                                query.StreetId = 0;
                            }
                            ShopBranchInfo obj = _iShopBranchService.GetAutoMatchShopBranch(query, p.OrderItemInfo.Select(x => x.SkuId).ToArray<string>(), p.OrderItemInfo.Select(y => Core.Helper.TypeHelper.ObjectToInt(y.Quantity)).ToArray<int>());
                            if (obj != null && obj.Id > 0)
                            {
                                foreach (var orderItem in p.OrderItemInfo)
                                {
                                    UpdateShopBranchSku(obj.Id, orderItem.SkuId, -Core.Helper.TypeHelper.ObjectToInt(orderItem.Quantity));
                                    UpdateSKUStockInOrder(orderItem.SkuId, orderItem.Quantity);
                                }
                                Context.SaveChanges();
                                UpdateOrderShopBranch(p.Id, obj.Id);
                            }
                        }
                    }
                }
            }
            #endregion

            var orderIds = infos.Select(p => p.Id);
            //限时购销量
            if (model.IsCashOnDelivery)
            {
                ServiceProvider.Instance<ILimitTimeBuyService>.Create.IncreaseSaleCount(orderIds.ToList());
            }

            //发送微信消息  提交订单
            if (!string.IsNullOrEmpty(model.formId))
            {
                var orderId = string.Join(",", orderIds);
                WXAppletFormDatasInfo info = new WXAppletFormDatasInfo();
                info.EventId = Convert.ToInt64(MessageTypeEnum.OrderCreated);
                info.EventTime = DateTime.Now;
                info.EventValue = orderId;
                info.ExpireTime = DateTime.Now.AddDays(7);
                info.FormId = model.formId;
                ServiceProvider.Instance<IWXMsgTemplateService>.Create.AddWXAppletFromData(info);
            }
            //减少积分
            DeductionIntegral(model.CurrentUser, orderIds, model.Integral);
            //发送短信通知
            Task.Factory.StartNew(() =>
            {
                SendMessage(infos);
            });
            return infos;
        }

        /// <summary>
        /// 设置订单的优惠券金额分摊到每个子订单
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="Coupon"></param>
        /// <returns></returns>
        private void SetActualItemPrice(OrderInfo info)
        {
            var t = info.OrderItemInfo.ToList();
            decimal couponDiscount = 0;
            var num = t.Count();
            for (var i = 0; i < t.Count(); i++)
            {
                var _item = t[i];

                if (i < num - 1)
                {
                    _item.CouponDiscount = GetItemCouponDisCount(_item.RealTotalPrice - _item.FullDiscount, info.ProductTotalAmount - info.FullDiscount, info.DiscountAmount);
                    couponDiscount += _item.CouponDiscount;
                }
                else
                {
                    _item.CouponDiscount = info.DiscountAmount - couponDiscount;
                }
            }
        }

        public long SaveOrderPayInfo(IEnumerable<OrderPayInfo> model, Core.PlatformType platform)
        {
            //只有一个订单就取第一个订单号，否则生成一个支付订单号
            var orderid = long.Parse(model.FirstOrDefault().OrderId.ToString() + ((int)platform).ToString());
            var payid = model.Count() == 1 ? orderid : GetOrderPayId();
            foreach (var pay in model)
            {
                var orderPayInfo = Context.OrderPayInfo.FirstOrDefault(item => item.PayId == payid && item.OrderId == pay.OrderId);
                if (orderPayInfo == null)
                {
                    orderPayInfo = new OrderPayInfo
                    {
                        OrderId = pay.OrderId,
                        PayId = payid
                    };
                    Context.OrderPayInfo.Add(orderPayInfo);
                }
            }
            Context.SaveChanges();
            return payid;
        }

        /// <summary>
        /// 根据订单id获取OrderPayInfo
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public List<OrderPayInfo> GetOrderPays(IEnumerable<long> orderIds)
        {
            return Context.OrderPayInfo.Where(p => orderIds.Contains(p.OrderId)).ToList();
        }

        #endregion 创建订单相关  done

        #region 订单操作 done

        // 商家发货
        public OrderInfo SellerSendGood(long orderId, string sellerName, string companyName, string shipOrderNumber)
        {
            OrderInfo order = Context.OrderInfo.FindById(orderId);
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                throw new HimallException("只有待发货状态的订单才能发货");
            }
            if (!CanSendGood(orderId))
            {
                throw new HimallException("拼团完成后订单才可以发货");
            }
            order.OrderStatus = OrderInfo.OrderOperateStatus.WaitReceiving;
            order.ExpressCompanyName = companyName;
            order.ShipOrderNumber = shipOrderNumber;
            order.ShippingDate = DateTime.Now;
            order.LastModifyTime = DateTime.Now;

            //处理订单退款
            var refund = Context.OrderRefundInfo.FirstOrDefault(d => d.OrderId == orderId && d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund && d.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit);
            if (refund != null)
            {
                //自动拒绝退款申请
                ServiceProvider.Instance<IRefundService>.Create.SellerDealRefund(refund.Id, OrderRefundInfo.OrderRefundAuditStatus.UnAudit, "商家已发货", sellerName);
            }

            Context.SaveChanges();

            //处理推荐用户佣金
            if (order.ShareUserId.HasValue)
            {
                //增加分销员清退处理
                var promoter = Context.PromoterInfo.FirstOrDefault(e => e.UserId == order.ShareUserId.Value);
                if (promoter != null && promoter.Status == PromoterInfo.PromoterStatus.Audited)
                {//只有审核状态的分销员才计算佣金
                    //处理佣金
                    foreach (var item in order.OrderItemInfo)
                    {
                        if (item.DistributionRate.HasValue && item.DistributionRate.Value > 0)
                        {
                            var income = new BrokerageIncomeInfo();
                            income.SkuID = item.SkuId;
                            income.SkuInfo = item.SKU;
                            income.ProductID = item.ProductId;
                            income.OrderItemId = item.Id;
                            income.OrderId = item.OrderId;
                            income.ProductName = item.ProductName;
                            income.CreateTime = DateTime.Now;
                            income.OrderTime = order.OrderDate;
                            income.ShopId = item.ShopId;
                            income.TotalPrice = item.RealTotalPrice;
                            income.SettlementTime = null;
                            income.UserId = order.ShareUserId.Value;
                            income.Status = BrokerageIncomeInfo.BrokerageStatus.NotSettled;
                            income.Brokerage = Math.Round((item.DistributionRate.Value * (item.RealTotalPrice - item.CouponDiscount - item.FullDiscount)) / 100, 2, MidpointRounding.AwayFromZero);
                            income.BuyerUserId = order.UserId;
                            Context.BrokerageIncomeInfo.Add(income);
                            var m = Context.ProductBrokerageInfo.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
                            m.saleAmount += item.RealTotalPrice;
                            m.SaleNum += Convert.ToInt32(item.Quantity); //增加商品销售量和商品销售额
                            m.BrokerageTotal += income.Brokerage;
                        }
                    }
                    Context.SaveChanges();
                }
            }

            AddOrderOperationLog(orderId, sellerName, "商家发货");

            return order;
        }

        /// <summary>
        /// 门店发货
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <param name="deliveryType">配送方式（2店员配送或1快递配送）</param>
        /// <param name="shopkeeperName">发货人（门店管理员账号名称）</param>
        /// <param name="companyName">快递公司</param>
        /// <param name="shipOrderNumber">快递单号</param>
        /// <returns></returns>
        public OrderInfo ShopSendGood(long orderId, int deliveryType, string shopkeeperName, string companyName, string shipOrderNumber)
        {
            OrderInfo order = Context.OrderInfo.FindById(orderId);
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                throw new HimallException("只有待发货状态的订单才能发货");
            }
            if (!CanSendGood(orderId))
            {
                throw new HimallException("拼团完成后订单才可以发货");
            }
            order.OrderStatus = OrderInfo.OrderOperateStatus.WaitReceiving;
            if (deliveryType == 2)
            {
                order.DeliveryType = CommonModel.Enum.DeliveryType.ShopStore;
            }
            else
            {
                order.DeliveryType = CommonModel.Enum.DeliveryType.Express;
            }
            order.ExpressCompanyName = companyName;
            order.ShipOrderNumber = shipOrderNumber;
            order.ShippingDate = DateTime.Now;
            order.LastModifyTime = DateTime.Now;

            //处理订单退款
            var refund = Context.OrderRefundInfo.FirstOrDefault(d => d.OrderId == orderId && d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund && d.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit);

            if (refund != null)
            {
                //自动拒绝退款申请
                ServiceProvider.Instance<IRefundService>.Create.SellerDealRefund(refund.Id, OrderRefundInfo.OrderRefundAuditStatus.UnAudit, "门店已发货", shopkeeperName);
            }

            Context.SaveChanges();

            //处理推荐用户佣金
            if (order.ShareUserId.HasValue)
            {
                //增加分销员清退处理
                var promoter = Context.PromoterInfo.FirstOrDefault(e => e.UserId == order.ShareUserId.Value);
                if (promoter != null && promoter.Status == PromoterInfo.PromoterStatus.Audited)
                {//只有审核状态的分销员才计算佣金
                    //处理佣金
                    foreach (var item in order.OrderItemInfo)
                    {
                        if (item.DistributionRate.HasValue && item.DistributionRate.Value > 0)
                        {
                            var income = new BrokerageIncomeInfo();
                            income.SkuID = item.SkuId;
                            income.SkuInfo = item.SKU;
                            income.ProductID = item.ProductId;
                            income.OrderItemId = item.Id;
                            income.OrderId = item.OrderId;
                            income.ProductName = item.ProductName;
                            income.CreateTime = DateTime.Now;
                            income.OrderTime = order.OrderDate;
                            income.ShopId = item.ShopId;
                            income.TotalPrice = item.RealTotalPrice;
                            income.SettlementTime = null;
                            income.UserId = order.ShareUserId.Value;
                            income.Status = BrokerageIncomeInfo.BrokerageStatus.NotSettled;
                            income.Brokerage = Math.Round((item.DistributionRate.Value * (item.RealTotalPrice - item.CouponDiscount - item.FullDiscount)) / 100, 2, MidpointRounding.AwayFromZero);
                            income.BuyerUserId = order.UserId;
                            Context.BrokerageIncomeInfo.Add(income);
                            var m = Context.ProductBrokerageInfo.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
                            m.saleAmount += item.RealTotalPrice;
                            m.SaleNum += Convert.ToInt32(item.Quantity); //增加商品销售量和商品销售额
                            m.BrokerageTotal += income.Brokerage;
                        }
                    }
                    Context.SaveChanges();
                }
            }

            AddOrderOperationLog(orderId, shopkeeperName, "门店发货");

            return order;
        }

        /// <summary>
        /// 判断订单是否在申请售后
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public bool IsOrderAfterService(long orderId)
        {
            var refund = Context.OrderRefundInfo.FirstOrDefault(d => d.OrderId == orderId && d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund && d.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit);
            if (refund != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 修改快递信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="companyName"></param>
        /// <param name="shipOrderNumber"></param>
        /// <returns></returns>
        public OrderInfo UpdateExpress(long orderId, string companyName, string shipOrderNumber)
        {
            var context = this.Context;

            var order = context.OrderInfo.FirstOrDefault(p => p.Id == orderId);

            order.ExpressCompanyName = companyName;
            order.ShipOrderNumber = shipOrderNumber;
            order.ShippingDate = DateTime.Now;
            order.LastModifyTime = DateTime.Now;

            context.SaveChanges();

            return order;
        }

        // 商家更新收货地址
        public void SellerUpdateAddress(long orderId, string sellerName, string shipTo, string cellPhone, int topRegionId, int regionId, string regionFullName, string address)
        {
            OrderInfo order = Context.OrderInfo.FindById(orderId);
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                throw new HimallException("只有待付款或待发货状态的订单才能修改收货地址");
            }

            order.ShipTo = shipTo;
            order.CellPhone = cellPhone;
            order.TopRegionId = topRegionId;
            order.RegionId = regionId;
            order.RegionFullName = regionFullName;
            order.Address = address;
            Context.SaveChanges();
            AddOrderOperationLog(orderId, sellerName, "商家修改订单的收货地址");
        }

        // 会员确认订单
        public void MembeConfirmOrder(long orderId, string memberName)
        {
            OrderInfo order = Context.OrderInfo.Where(a => a.Id == orderId && a.UserName == memberName).FirstOrDefault();

            if (order.OrderStatus == OrderInfo.OrderOperateStatus.Finish)
            {
                throw new HimallException("该订单已经确认过!");
            }
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitReceiving && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp)
            {
                throw new HimallException("订单状态发生改变，请重新刷页面操作!");
            }
            this.SetStateToConfirm(order);
            order.LastModifyTime = DateTime.Now;
            Context.SaveChanges();

            var member = Context.UserMemberInfo.FirstOrDefault(a => a.UserName == memberName);

            AddIntegral(member, order.Id, order.ActualPayAmount);//增加积分
            AddOrderOperationLog(orderId, memberName, "会员确认收货");
            UpdateProductVistiOrderCount(orderId);

            if (order.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery)
            {
                var orderItems = Context.OrderItemInfo.Where(p => p.OrderId == order.Id).ToList();
                UpdateProductVisti(orderItems);
            }
            //现在是拼团完成就结算
            //WritePendingSettlnment(order);
        }
        /// <summary>
        /// 门店核销订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="managerName"></param>
        public void ShopBranchConfirmOrder(long orderId, long shopBranchId, string managerName)
        {
            OrderInfo order = Context.OrderInfo.Where(a => a.Id == orderId && a.ShopBranchId.Value == shopBranchId).FirstOrDefault();
            if (order == null)
            {
                throw new HimallException("处理订单错误，请确认参数正确");
            }
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp)
            {
                throw new HimallException("只有待自提状态的订单才能进行核销操作");
            }
            if (order.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery)
            {
                order.PayDate = DateTime.Now;
            }
            order.OrderStatus = OrderInfo.OrderOperateStatus.Finish;
            order.FinishDate = DateTime.Now;
            order.LastModifyTime = DateTime.Now;
            Context.SaveChanges();

            var member = Context.UserMemberInfo.FirstOrDefault(a => a.Id == order.UserId);

            AddIntegral(member, order.Id, order.ActualPayAmount);//增加积分
            AddOrderOperationLog(orderId, managerName, "门店核销订单");
            UpdateProductVistiOrderCount(orderId);

            if (order.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery)
            {
                var orderItems = Context.OrderItemInfo.Where(p => p.OrderId == order.Id).ToList();
                UpdateProductVisti(orderItems);
            }
            //现在是拼团完成就结算
            //WritePendingSettlnment(order);
        }
        // 平台关闭订单
        public void PlatformCloseOrder(long orderId, string managerName, string closeReason = "")
        {
            OrderInfo order = Context.OrderInfo.FindById(orderId);
            if (string.IsNullOrWhiteSpace(closeReason))
            {
                closeReason = "平台取消订单";
            }
            order.CloseReason = closeReason;
            order.LastModifyTime = DateTime.Now;
            this.CloseOrder(order);

            ReturnStock(order);
            Context.SaveChanges();

            //拼团处理
            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                SetFightGroupOrderStatus(orderId, FightGroupOrderJoinStatus.BuildFailed);
            }

            var member = Context.UserMemberInfo.FirstOrDefault(a => a.Id == order.UserId);
            CancelIntegral(member, order.Id, order.IntegralDiscount);  //取消订单增加积分
            AddOrderOperationLog(orderId, managerName, closeReason);
        }

        // 商家关闭订单
        public void SellerCloseOrder(long orderId, string sellerName)
        {
            OrderInfo order = Context.OrderInfo.FindById(orderId);
            if (order == null)
            {
                throw new HimallException("错误的订单编号！");
            }
            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                throw new HimallException("拼团订单，不可以手动取消！");
            }
            if (order.OrderStatus == OrderInfo.OrderOperateStatus.Close)
            {
                throw new HimallException("您的订单已被取消了，不需再重复操作！");
            }
            if (order.PaymentType != OrderInfo.PaymentTypes.CashOnDelivery
                && (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || order.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp))
            {
                throw new HimallException("订单已付款，不能取消订单！");
            }
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay && order.PaymentType != OrderInfo.PaymentTypes.CashOnDelivery ||
                order.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                throw new HimallException("您的订单状态已发生变化，不能进行取消操作！");
            }
            order.CloseReason = "商家取消订单";
            order.OrderStatus = OrderInfo.OrderOperateStatus.Close;
            order.LastModifyTime = DateTime.Now;
            ReturnStock(order);
            Context.SaveChanges();

            //拼团处理
            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                SetFightGroupOrderStatus(orderId, FightGroupOrderJoinStatus.BuildFailed);
            }
            var member = Context.UserMemberInfo.FirstOrDefault(a => a.Id == order.UserId);
            CancelIntegral(member, order.Id, order.IntegralDiscount);  //取消订单增加积分
            AddOrderOperationLog(orderId, sellerName, order.CloseReason);
        }

        // 会员关闭订单
        public void MemberCloseOrder(long orderId, string memberName)
        {
            OrderInfo order = Context.OrderInfo.Where(a => a.Id == orderId && a.UserName == memberName).FirstOrDefault();
            if (order == null)
            {
                throw new HimallException("该订单不属于该用户！");
            }
            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                throw new HimallException("拼团订单，不可以手动取消！");
            }
            if (order.OrderStatus == OrderInfo.OrderOperateStatus.Close)
            {
                throw new HimallException("您的订单已被取消了，不需再重复操作！");
            }
            if (order.PaymentType != OrderInfo.PaymentTypes.CashOnDelivery
                && (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || order.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp))
            {
                throw new HimallException("订单已付款，不能取消订单！");
            }
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay && order.PaymentType != OrderInfo.PaymentTypes.CashOnDelivery ||
                order.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                throw new HimallException("您的订单状态已发生变化，不能进行取消操作！");
            }

            string closeReason = "会员取消订单";
            this.CloseOrder(order);
            ReturnStock(order);

            order.LastModifyTime = DateTime.Now;
            Context.SaveChanges();
            var member = Context.UserMemberInfo.FirstOrDefault(a => a.Id == order.UserId);
            CancelIntegral(member, order.Id, order.IntegralDiscount);  //取消订单增加积分
            AddOrderOperationLog(orderId, memberName, closeReason);
        }

        /// <summary>
        /// 用户申请关闭订单（需审核才能真正关闭） 去掉申请取消订单状态
        /// </summary>

        public void MemberApplyCloseOrder(long orderId, string memberName, bool isBackStock = false)
        {
            OrderInfo order = Context.OrderInfo.Where(a => a.Id == orderId && a.UserName == memberName).FirstOrDefault();
            if (order == null)
            {
                throw new HimallException("该订单不属于该用户！");
            }
            string closeReason = "会员取消订单";
            // order.OrderStatus = OrderInfo.OrderOperateStatus.CloseByUser;

            if (isBackStock)
            {
                ReturnStock(order);
            }
            Context.SaveChanges();
            var member = Context.UserMemberInfo.FirstOrDefault(a => a.Id == order.UserId);
            CancelIntegral(member, order.Id, order.IntegralDiscount);  //取消订单增加积分
            AddOrderOperationLog(orderId, memberName, closeReason);
        }

        /// <summary>
        /// 是否超过售后期
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns>true 不可售后 false 可以售后</returns>
        public bool IsRefundTimeOut(long orderId)
        {
            var order = Context.OrderInfo.FirstOrDefault(a => a.Id == orderId);
            return IsRefundTimeOut(order);
        }

        public bool IsRefundTimeOut(OrderInfo order)
        {
            var result = true;
            if (order != null)
            {
                result = false;   //默认可以售后
                switch (order.OrderStatus)
                {
                    case OrderInfo.OrderOperateStatus.Close:
                        result = true;
                        break;

                        //case OrderInfo.OrderOperateStatus.CloseByUser:
                        //    result = true;
                        //    break;
                }
                if (order.OrderStatus == OrderInfo.OrderOperateStatus.Finish)
                {
                    result = true;
                    var sitesetser = ServiceProvider.Instance<ISiteSettingService>.Create;
                    var siteSetting = sitesetser.GetSiteSettings();
                    if (order.FinishDate.HasValue)
                    {
                        DateTime EndSalesReturn = order.FinishDate.Value.AddDays(siteSetting.SalesReturnTimeout).Date;
                        if (EndSalesReturn >= DateTime.Now.Date)
                        {
                            result = false;
                        }
                    }
                }
            }
            return result;
        }

        // 设置订单快递信息
        public void SetOrderExpressInfo(long shopId, string expressName, string startCode, IEnumerable<long> orderIds)
        {
            var expressService = ServiceProvider.Instance<IExpressService>.Create;
            var express = expressService.GetExpress(expressName);
            if (!express.CheckExpressCodeIsValid(startCode))
            {
                throw new HimallException("起始快递单号格式不正确");
            }

            var orders = Context.OrderInfo.FindBy(item => item.ShopId == shopId && orderIds.Contains(item.Id)).ToArray();
            var orderedOrders = orderIds.Select(item => orders.FirstOrDefault(t => item == t.Id)).Where(item => item != null);

            int i = 0;
            string code = string.Empty;
            var shop = ServiceProvider.Instance<IShopService>.Create.GetShop(shopId);
            string sendFullAddress = ServiceProvider.Instance<IRegionService>.Create.GetFullName(shop.SenderRegionId.Value) + " " + shop.SenderAddress;

            foreach (var order in orderedOrders)
            {
                if (i++ == 0)
                {
                    code = startCode;
                }
                else
                {
                    code = express.NextExpressCode(code);
                }
                order.ShipOrderNumber = code;
                order.ExpressCompanyName = expressName;
                order.SellerPhone = shop.SenderPhone;
                order.SellerAddress = sendFullAddress;
            }
            Context.SaveChanges();
        }
        /// <summary>
        /// 设置订单商家备注
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="mark"></param>
        public void SetOrderSellerRemark(long orderId, string mark)
        {
            var orderdata = Context.OrderInfo.FirstOrDefault(d => d.Id == orderId);
            if (orderdata == null)
            {
                throw new HimallException("错误的订单编号");
            }
            orderdata.SellerRemark = mark;
            Context.SaveChanges();
        }

        //商家更新金额
        public void SellerUpdateItemDiscountAmount(long orderItemId, decimal discountAmount, string sellerName)
        {
            OrderItemInfo item = Context.OrderItemInfo.FindById(orderItemId);
            var order = Context.OrderInfo.FindById(item.OrderId);
            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                throw new HimallException("拼团订单不可以改价");
            }

            item.DiscountAmount += discountAmount;

            item.RealTotalPrice = this.GetRealTotalPrice(order, item, discountAmount);

            Context.SaveChanges();
            item = Context.OrderItemInfo.FindById(orderItemId);
            item.OrderInfo.ProductTotalAmount = item.OrderInfo.ProductTotalAmount - discountAmount;
            item.OrderInfo.TotalAmount = item.OrderInfo.OrderTotalAmount;
            item.OrderInfo.CommisTotalAmount = Context.OrderItemInfo.Where(i => i.OrderId == item.OrderId).Sum(i => i.RealTotalPrice * i.CommisRate);

            SetActualItemPrice(order);         //平摊订单的优惠券金额
            order.LastModifyTime = DateTime.Now;
            Context.SaveChanges();

            AddOrderOperationLog(item.OrderId, sellerName, "商家修改订单商品的优惠金额");
        }

        public void SellerUpdateOrderFreight(long orderId, decimal freight)
        {
            OrderInfo order = Context.OrderInfo.FindById(orderId);
            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                throw new HimallException("拼团订单不可以改价");
            }
            this.SetFreight(order, freight);
            order.LastModifyTime = DateTime.Now;
            Context.SaveChanges();
        }

        // 平台确认订单支付
        public void PlatformConfirmOrderPay(long orderId, string payRemark, string managerName)
        {
            OrderInfo order = Context.OrderInfo.FindById(orderId);
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay)
            {
                throw new HimallException("只有待付款状态的订单才能进行付款操作");
            }
            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                if (!FightGroupOrderCanPay(orderId))
                {
                    throw new HimallException("拼团订单的状态为不可付款状态");
                }
            }

            if (order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake)
            {
                OperaOrderPickupCode(order);
            }
            else
                order.OrderStatus = OrderInfo.OrderOperateStatus.WaitDelivery;
            //设置实收金额=实付金额
            order.ActualPayAmount = order.TotalAmount;
            order.PayRemark = payRemark;
            order.PaymentTypeName = string.Format("平台线下收款", managerName);
            order.PaymentType = OrderInfo.PaymentTypes.Offline;
            order.PayDate = DateTime.Now;
            order.LastModifyTime = DateTime.Now;

            AddOrderOperationLog(orderId, managerName, "平台确认收到订单货款");

            var orderItems = Context.OrderItemInfo.Where(p => p.OrderId == orderId).ToList();

            UpdateShopVisti(order);                // 修改店铺销量
            UpdateProductVisti(orderItems.Where(p => p.OrderId == order.Id));            // 修改商品销量
            UpdateLimitTimeBuyLog(orderItems.Where(p => p.OrderId == order.Id));    // 修改限时购销售数量

            var firstOrderItem = orderItems.First();
            PaySuccessed(order, firstOrderItem.ProductName, "线下收款", "已付款", DateTime.Now);
        }

        // 更新用户订单数
        public void UpdateMemberOrderInfo(long userId, decimal addOrderAmount = 0, int addOrderCount = 1)
        {
            var member = Context.UserMemberInfo.FirstOrDefault(item => item.Id == userId);
            member.OrderNumber += addOrderCount;//变更订单数
            member.Expenditure += addOrderAmount;//变更总金额
            Context.SaveChanges();
        }

        // 订单支付成功
        public void PaySucceed(IEnumerable<long> orderIds, string paymentId, DateTime payTime, string payNo = null, long payId = 0)
        {
            var orders = Context.OrderInfo.FindBy(item => orderIds.Contains(item.Id)).ToArray();
            var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(paymentId);
            var orderItems = Context.OrderItemInfo.Where(p => orderIds.Contains(p.OrderId)).ToList();
            foreach (var order in orders)
            {
                if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay)
                {
                    if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
                    {
                        if (!FightGroupOrderCanPay(order.Id))
                        {
                            //在线支付流程不处理拼团状态，在拼团服务中自己完成退款操作
                            //throw new HimallException("拼团订单的状态为不可付款状态");
                        }
                    }
                    var orderPayInfo = Context.OrderPayInfo.FirstOrDefault(item => item.OrderId == order.Id && item.PayId == payId);
                    using (TransactionScope scope = new TransactionScope())
                    {
                        order.PayDate = payTime;
                        order.PaymentTypeGateway = paymentId;
                        if (order.OrderTotalAmount == 0)
                        {
                            order.PaymentTypeName = "积分支付";
                        }
                        else
                        {
                            order.PaymentTypeName = payment.PluginInfo.DisplayName;
                        }
                        order.PaymentType = OrderInfo.PaymentTypes.Online;

                        if (order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake)
                        {
                            OperaOrderPickupCode(order);
                        }
                        else
                            order.OrderStatus = OrderInfo.OrderOperateStatus.WaitDelivery;

                        if (orderPayInfo != null)
                        {
                            orderPayInfo.PayState = true;
                            orderPayInfo.PayTime = payTime;
                        }

                        //设置实收金额=实付金额
                        order.ActualPayAmount = order.TotalAmount;

                        //  SetActualItemPrice(order);         //平摊订单的优惠券金额
                        UpdateShopVisti(order);               // 修改店铺销量
                        UpdateProductVisti(orderItems.Where(p => p.OrderId == order.Id));           // 修改商品销量
                        UpdateLimitTimeBuyLog(orderItems.Where(p => p.OrderId == order.Id));   // 修改限时购销售数量
                        order.GatewayOrderId = payNo;
                        order.LastModifyTime = DateTime.Now;
                        Context.SaveChanges();
                        scope.Complete();
                    }

                    var firstOrderItem = orderItems.First(p => p.OrderId == order.Id);
                    PaySuccessed(order, firstOrderItem.ProductName, payment.PluginInfo.DisplayName, "已付款", payTime);
                }
            }
            //发送通知消息
            var orderMessage = new MessageOrderInfo();
            orderMessage.OrderId = string.Join(",", orderIds);
            orderMessage.ShopId = 0;
            orderMessage.SiteName = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().SiteName;
            orderMessage.TotalMoney = orders.Sum(a => a.OrderTotalAmount);
            orderMessage.PaymentType = payment.PluginInfo.DisplayName;

            orderMessage.OrderTime = orders.FirstOrDefault().OrderDate;
            orderMessage.PayTime = payTime;
            orderMessage.PaymentType = "已付款";
            orderMessage.ProductName = orders.FirstOrDefault().OrderItemInfo.FirstOrDefault().ProductName;
            orderMessage.UserName = orders.FirstOrDefault().UserName;
            var userId = orders.FirstOrDefault().UserId;
            var firstOrder = orders.FirstOrDefault();
            if (firstOrder != null && firstOrder.Platform == PlatformType.WeiXinSmallProg)
            {
                orderMessage.MsgOrderType = MessageOrderType.Applet;
            }
            Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnOrderPay(userId, orderMessage));
        }

        //TODO:【2015-09-01】预付款支付订单
        public void PayCapital(IEnumerable<long> orderIds, string payNo = null, long payId = 0)
        {
            var orders = Context.OrderInfo.FindBy(item => orderIds.Contains(item.Id)).ToArray();
            var totalAmount = orders.Sum(e => e.OrderTotalAmount);
            var userid = orders.FirstOrDefault().UserId;
            var capital = Context.CapitalInfo.FirstOrDefault(e => e.MemId == userid);
            if (capital == null)
            {
                throw new HimallException("预付款金额少于订单金额");
            }
            if (capital.Balance < totalAmount)
            {
                throw new HimallException("预付款金额少于订单金额");
            }
            var orderItems = Context.OrderItemInfo.Where(p => orderIds.Contains(p.OrderId)).ToList();
            foreach (var order in orders)
            {
                if (order != null && (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay))
                {
                    var orderPayInfo = Context.OrderPayInfo.FirstOrDefault(item => item.OrderId == order.Id && item.PayId == payId);
                    if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
                    {
                        if (!FightGroupOrderCanPay(order.Id))
                        {
                            throw new HimallException("拼团订单的状态为不可付款状态");
                        }
                    }
                    CapitalDetailInfo detail = new CapitalDetailInfo()
                    {
                        Amount = -order.OrderTotalAmount,
                        CapitalID = capital.Id,
                        CreateTime = DateTime.Now,
                        SourceType = CapitalDetailInfo.CapitalDetailType.Consume,
                        SourceData = order.Id.ToString(),
                        Id = this.GenerateOrderNumber()
                    };

                    using (TransactionScope scope = new TransactionScope())
                    {
                        order.PayDate = DateTime.Now;
                        order.PaymentTypeGateway = string.Empty;
                        if (order.OrderTotalAmount == 0)
                        {
                            order.PaymentTypeName = "积分支付";
                        }
                        else
                        {
                            order.PaymentTypeName = "预付款支付";
                        }
                        order.PaymentType = OrderInfo.PaymentTypes.Online;
                        if (order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake)
                        {
                            OperaOrderPickupCode(order);
                        }
                        else
                            order.OrderStatus = OrderInfo.OrderOperateStatus.WaitDelivery;
                        //设置实收金额=实付金额
                        order.ActualPayAmount = order.TotalAmount;
                        order.LastModifyTime = DateTime.Now;
                        if (orderPayInfo != null)
                        {
                            orderPayInfo.PayState = true;
                            orderPayInfo.PayTime = DateTime.Now;
                        }
                        capital.Balance -= order.OrderTotalAmount;
                        Context.CapitalDetailInfo.Add(detail);
                        //    SetActualItemPrice(order);         //平摊订单的优惠券金额
                        UpdateShopVisti(order);               // 修改店铺销量
                        UpdateProductVisti(orderItems.Where(p => p.OrderId == order.Id));           // 修改商品销量
                        UpdateLimitTimeBuyLog(orderItems.Where(p => p.OrderId == order.Id));   // 修改限时购销售数量

                        Context.SaveChanges();
                        scope.Complete();
                    }
                    var firstOrderItem = orderItems.First(p => p.OrderId == order.Id);
                    PaySuccessed(order, firstOrderItem.ProductName, "预付款支付", "已付款", DateTime.Now);
                }
            }
            //发送通知消息
            var orderMessage = new MessageOrderInfo();
            orderMessage.OrderId = string.Join(",", orderIds);
            orderMessage.OrderTime = orders.FirstOrDefault().OrderDate;
            orderMessage.ShopId = 0;
            orderMessage.SiteName = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().SiteName;
            orderMessage.TotalMoney = orders.Sum(a => a.OrderTotalAmount);
            orderMessage.UserName = orders.FirstOrDefault().UserName;
            orderMessage.PaymentType = "预付款支付";
            orderMessage.PayTime = DateTime.Now;
            orderMessage.PaymentType = "已付款";
            orderMessage.ProductName = orderItems.First().ProductName;
            var userId = orders.FirstOrDefault().UserId;
            if (orders.FirstOrDefault().Platform == PlatformType.WeiXinSmallProg)
            {
                orderMessage.MsgOrderType = MessageOrderType.Applet;
            }
            Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnOrderPay(userId, orderMessage));
        }

        private void PaySuccessed(OrderInfo order, string productName, string paymentType, string paymentStatus, DateTime payTime)
        {
            //拼团成功
            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                Log.Debug("付款修改订单状态：订单id:" + order.Id);
                SetFightGroupOrderStatus(order.Id, FightGroupOrderJoinStatus.JoinSuccess);
            }

            //生成消费码
            Random r = new Random();
            string payCode = DateTime.Now.AddSeconds(r.Next(60)).Ticks.ToString().Substring(12, 6) + DateTime.Now.Ticks.ToString().Substring(12, 6);
            CreatePayCodeByOrderId(order.Id,payCode);

            //发布付款成功消息
            //MessageQueue.PublishTopic(CommonConst.MESSAGEQUEUE_PAYSUCCESSED, order.Id);
            if (OnOrderPaySuccessed != null)
                OnOrderPaySuccessed(order.Id);

            //发送店铺通知消息
            var sordmsg = new MessageOrderInfo();
            sordmsg.OrderId = order.Id.ToString();
            sordmsg.ShopId = order.ShopId;
            sordmsg.ShopName = order.ShopName;
            sordmsg.SiteName = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().SiteName;
            sordmsg.TotalMoney = order.OrderTotalAmount;
            sordmsg.PaymentType = paymentType;
            sordmsg.PayTime = payTime;
            sordmsg.OrderTime = order.OrderDate;
            sordmsg.PaymentStatus = paymentStatus;
            sordmsg.ProductName = productName;
            sordmsg.UserName = order.UserName;
            Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnShopHasNewOrder(order.ShopId, sordmsg));

            SendAppMessage(order);//支付成功后推送APP消息
        }

        public bool PayByCapitalIsOk(long userid, IEnumerable<long> orderIds)
        {
            var orders = Context.OrderInfo.FindBy(item => orderIds.Contains(item.Id)).ToArray();
            var totalAmount = orders.Sum(e => e.OrderTotalAmount);
            var capital = Context.CapitalInfo.FirstOrDefault(e => e.MemId == userid);
            if (capital != null && capital.Balance >= totalAmount)
            {
                return true;
            }
            return false;
        }
        // 计算订单条目可退款金额
        // 看不懂具体逻辑，暂时不改
        public void CalculateOrderItemRefund(long orderId, bool isCompel = false)
        {
            var order = Context.OrderInfo.FindById(orderId);
            if (order != null)
            {
                if (!isCompel)
                {
                    var ord1stitem = order.OrderItemInfo.FirstOrDefault();
                    if (ord1stitem.EnabledRefundAmount == null || ord1stitem.EnabledRefundAmount <= 0
                        || ord1stitem.EnabledRefundIntegral == null)
                    {
                        isCompel = true;
                    }
                }
            }
            if (isCompel)
            {
                int orditemcnt = order.OrderItemInfo.Count();
                int curnum = 0;
                decimal ordprosumnum = order.ProductTotalAmount;
                decimal orddisnum = order.DiscountAmount + order.FullDiscount;
                decimal ordrefnum = order.ProductTotal;
                decimal ordindisnum = order.IntegralDiscount;
                decimal refcount = 0;
                decimal refincount = 0;   //只做整数处理
                long firstid = 0;
                foreach (var item in order.OrderItemInfo)
                {
                    decimal itemprosumnum = item.RealTotalPrice;
                    decimal curref = 0;
                    decimal curinref = 0;
                    if (curnum == 0)
                    {
                        curref = 0;    //首件退款为结果计算
                        firstid = item.Id;
                    }
                    else
                    {
                        curref = (decimal)Math.Round((itemprosumnum - (orddisnum / ordprosumnum) * itemprosumnum), 2);
                        if (curref < 0)
                            curref = 0;

                        //计算积分
                        curinref = (decimal)Math.Round(((ordindisnum / ordprosumnum) * itemprosumnum), 2);
                        if (curinref < 0)
                            curinref = 0;
                    }
                    item.EnabledRefundAmount = curref;
                    item.EnabledRefundIntegral = curinref;
                    refcount += curref;
                    refincount += curinref;
                    curnum++;
                }
                //处理首件
                OrderItemInfo firstitem = order.OrderItemInfo.FirstOrDefault(d => d.Id == firstid);
                firstitem.EnabledRefundAmount = ordrefnum - refcount;
                firstitem.EnabledRefundIntegral = ordindisnum - refincount;
                Context.SaveChanges();
            }
        }

        // 商家同意退款，关闭订单
        public void AgreeToRefundBySeller(long orderId)
        {
            OrderInfo order = Context.OrderInfo.FindById(orderId);
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                throw new HimallException("只可以关闭待发货订单！");
            }

            order.OrderStatus = OrderInfo.OrderOperateStatus.Close;
            order.CloseReason = "商家同意退款，取消订单";
            order.LastModifyTime = DateTime.Now;
            ReturnStock(order);
            Context.SaveChanges();
        }

        #endregion 订单操作 done

        #region 发票相关 done

        //发票内容
        public QueryPageModel<InvoiceContextInfo> GetInvoiceContexts(int PageNo, int PageSize = 20)
        {
            var sql = Context.InvoiceContextInfo;
            int total = 0;
            var data = sql.GetPage(out total, PageNo, PageSize);
            QueryPageModel<InvoiceContextInfo> result = new QueryPageModel<InvoiceContextInfo>();
            result.Models = data.ToList();
            result.Total = total;
            return result;
        }
        //发票内容
        public List<InvoiceContextInfo> GetInvoiceContexts()
        {
            return Context.InvoiceContextInfo.ToList();
        }

        public void SaveInvoiceContext(InvoiceContextInfo info)
        {
            if (info.Id >= 0)  //update
            {
                var model = Context.InvoiceContextInfo.First(p => p.Id == info.Id);
                model.Name = info.Name;
            }
            else //create
            {
                Context.InvoiceContextInfo.Add(info);
            }
            Context.SaveChanges();
        }

        public void DeleteInvoiceContext(long id)
        {
            Context.InvoiceContextInfo.Remove(id);
            Context.SaveChanges();
        }

        //发票抬头
        public List<InvoiceTitleInfo> GetInvoiceTitles(long userid)
        {
            var model = Context.InvoiceTitleInfo.Where(p => p.UserId == userid).ToList();
            if (model == null)
            {
                return new List<InvoiceTitleInfo>();
            }
            else
            {
                return model;
            }
        }

        public long SaveInvoiceTitle(InvoiceTitleInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.Name))
            {
                return -1;
            }
            if (info.Name == "个人")
            {
                return 0;
            }
            //已存在则不添加
            if (Context.InvoiceTitleInfo.Any(p => p.UserId == info.UserId && p.Name == info.Name))
            {
                return 0;
            }
            var result = Context.InvoiceTitleInfo.Add(info);
            Context.SaveChanges();
            return result.Id;
        }

        public void DeleteInvoiceTitle(long id)
        {
            Context.InvoiceTitleInfo.Remove(id);
            Context.SaveChanges();
        }

        #endregion 发票相关 done

        #region 私有函数

        /// <summary>
        /// 更改库存
        /// </summary>
        private void ReturnStock(OrderInfo order)
        {
            foreach (var orderItem in order.OrderItemInfo)
            {
                SKUInfo sku = Context.SKUInfo.FindById(orderItem.SkuId);
                if (sku != null)
                {
                    //if (order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake)
                    if (order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake || (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0))//此处如果是系统自动将订单匹配到门店或者由商家手动分配订单到门店，其配送方式仍为快递。所以改为也能根据门店ID去判断
                    {
                        var sbSku = Context.ShopBranchSkusInfo.FirstOrDefault(p => p.SkuId == sku.Id && p.ShopBranchId == order.ShopBranchId.Value);
                        if (sbSku != null)
                            sbSku.Stock += (int)orderItem.Quantity;
                    }
                    else
                        sku.Stock += orderItem.Quantity;
                }
            }
            Context.SaveChanges();
        }

        private void CancelIntegral(UserMemberInfo member, long orderId, decimal integralDiscount)
        {
            if (integralDiscount == 0)
            {
                return; //没使用积分直接返回
            }
            var IntegralExchange = ServiceProvider.Instance<IMemberIntegralService>.Create.GetIntegralChangeRule();
            if (IntegralExchange == null)
            {
                return; //没设置兑换规则直接返回
            }
            var IntegralPerMoney = IntegralExchange.IntegralPerMoney;
            var integral = Convert.ToInt32(Math.Floor(integralDiscount * IntegralPerMoney));
            MemberIntegralRecord record = new MemberIntegralRecord();
            record.UserName = member.UserName;
            record.MemberId = member.Id;
            record.RecordDate = DateTime.Now;
            record.TypeId = MemberIntegral.IntegralType.Cancel;
            record.ReMark = "订单被取消，返还积分，订单号:" + orderId.ToString();
            MemberIntegralRecordAction action = new MemberIntegralRecordAction();
            action.VirtualItemTypeId = MemberIntegral.VirtualItemType.Cancel;
            action.VirtualItemId = orderId;
            record.Himall_MemberIntegralRecordAction.Add(action);
            var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.Cancel, integral);
            ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(record, memberIntegral);
        }

        //消费获得积分
        private void AddIntegral(UserMemberInfo member, long orderId, decimal orderTotal)
        {
            var IntegralExchange = ServiceProvider.Instance<IMemberIntegralService>.Create.GetIntegralChangeRule();
            if (IntegralExchange == null)
            {
                return; //没设置兑换规则直接返回
            }
            var MoneyPerIntegral = IntegralExchange.MoneyPerIntegral;
            if (MoneyPerIntegral == 0)
            {
                return;
            }
            var integral = Convert.ToInt32(Math.Floor(orderTotal / MoneyPerIntegral));
            MemberIntegralRecord record = new MemberIntegralRecord();
            record.UserName = member.UserName;
            record.MemberId = member.Id;
            record.RecordDate = DateTime.Now;
            record.TypeId = MemberIntegral.IntegralType.Consumption;
            MemberIntegralRecordAction action = new MemberIntegralRecordAction();
            action.VirtualItemTypeId = MemberIntegral.VirtualItemType.Consumption;
            action.VirtualItemId = orderId;
            record.Himall_MemberIntegralRecordAction.Add(action);
            var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.Consumption, integral);
            ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(record, memberIntegral);
        }

        // 添加订单操作日志
        private void AddOrderOperationLog(long orderId, string userName, string operateContent)
        {
            OrderOperationLogInfo orderOperationLog = new OrderOperationLogInfo();
            orderOperationLog.Operator = userName;
            orderOperationLog.OrderId = orderId;
            orderOperationLog.OperateDate = DateTime.Now;
            orderOperationLog.OperateContent = operateContent;

            Context.OrderOperationLogInfo.Add(orderOperationLog);
            Context.SaveChanges();
        }

        public void DeductionIntegral(UserMemberInfo member, IEnumerable<long> Ids, int integral)
        {
            if (integral == 0)
            {
                return;
            }
            MemberIntegralRecord record = new MemberIntegralRecord();
            record.UserName = member.UserName;
            record.MemberId = member.Id;
            record.RecordDate = DateTime.Now;
            var remark = "订单号:";
            record.TypeId = MemberIntegral.IntegralType.Exchange;
            foreach (var t in Ids)
            {
                remark += t + ",";
                MemberIntegralRecordAction action = new MemberIntegralRecordAction();
                action.VirtualItemTypeId = MemberIntegral.VirtualItemType.Exchange;
                action.VirtualItemId = t;
                record.Himall_MemberIntegralRecordAction.Add(action);
            }
            record.ReMark = remark.TrimEnd(',');
            var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.Exchange, integral);
            ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(record, memberIntegral);
        }

        // 获取积分所能兑换的总金额
        public decimal GetIntegralDiscountAmount(int integral, long userId)
        {
            if (integral == 0)
            {
                return 0;
            }
            var integralService = ServiceProvider.Instance<IMemberIntegralService>.Create;
            var userIntegralModel = integralService.GetMemberIntegral(userId);
            var userIntegral = userIntegralModel == null ? 0 : userIntegralModel.AvailableIntegrals;
            if (userIntegral < integral)
            {
                throw new Himall.Core.HimallException("用户积分不足不能抵扣订单");
            }
            var exchangeModel = integralService.GetIntegralChangeRule();
            var integralPerMoney = exchangeModel == null ? 0 : exchangeModel.IntegralPerMoney;
            return integralPerMoney == 0 ? 0 : Math.Round(integral / (decimal)integralPerMoney, 2, MidpointRounding.AwayFromZero);
        }

        // 获取单个商品的销售价格
        public decimal GetSalePrice(string skuId, decimal salePrice, long? collid, int SkuCount, bool IslimitBuy = false)
        {
            var price = salePrice;
            if (collid.HasValue && collid.Value != 0 && SkuCount > 1)//组合购且大于一个商品
            {
                var collsku = ServiceProvider.Instance<ICollocationService>.Create.GetColloSku(collid.Value, skuId);
                if (collsku != null)
                {
                    price = collsku.Price;
                }
                //获取组合购的价格
            }
            else if (SkuCount == 1 && IslimitBuy) //订单是限时购
            {
                var limit = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetDetail(skuId);
                if (limit != null)
                {
                    price = (decimal)limit.Price;
                }
            }
            return price;
        }

        //  获取一个订单所使用的优惠券金额
        private decimal GetShopCouponDiscount(IEnumerable<CouponRecordInfo> coupons, long shopId)
        {
            if (coupons != null)
            {
                var couponAmount = coupons.ToList().Where(a => a.ShopId == shopId).Select(a => a.Himall_Coupon.Price).FirstOrDefault(); //单个订单使用的优惠券金额
                return couponAmount;
            }
            return 0;
        }

        private decimal GetShopCouponDiscount(IEnumerable<BaseAdditionalCoupon> coupons, long shopId)
        {
            if (coupons != null)
            {
                var first = coupons.Where(p => p.ShopId == shopId).FirstOrDefault();
                if (first == null)
                {
                    return 0;
                }

                if (first.Type == 0)
                {
                    CouponRecordInfo o = first.Coupon as CouponRecordInfo;
                    return o.Himall_Coupon.Price;
                }
                else if (first.Type == 1)
                {
                    ShopBonusReceiveInfo o = first.Coupon as ShopBonusReceiveInfo;
                    return (decimal)o.Price;
                }
            }
            return 0;
        }

        // 获取一个类目的返点比例
        private decimal GetCommisRate(long categoryId, long shopId)
        {
            var CommisRate = Context.BusinessCategoryInfo.Where(b => b.CategoryId == categoryId && b.ShopId == shopId).Select(a => a.CommisRate).FirstOrDefault();
            return CommisRate;
        }

        /// <summary>
        /// 获取一个订单所使用的积分金额
        /// </summary>
        /// <param name="OrderTotal">当前订单金额</param>
        /// <param name="OrdersTotal">所有订单金额</param>
        /// <param name="integral">积分</param>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        private decimal GetShopIntegralDiscount(decimal orderTotal, decimal ordersTotal, decimal integralTotal)
        {
            var perOrderIntegral = Math.Round(integralTotal * orderTotal / ordersTotal, 2);
            return perOrderIntegral;
        }


        /// <summary>
        /// 獲取當個訂單项所使用的优惠券的金额大小
        /// </summary>
        /// <param name="realTotalPrice">當前商品的減價後的價格</param>
        /// <param name="ProductTotal">訂單所有商品的總價格</param>
        /// <param name="couponDisCount">總的優惠券金額</param>
        /// <returns></returns>
        private decimal GetItemCouponDisCount(decimal realTotalPrice, decimal ProductTotal, decimal couponDiscount)
        {
            var ItemCouponDiscount = Math.Round(couponDiscount * realTotalPrice / ProductTotal, 2);
            return ItemCouponDiscount;
        }


        //发送短信通知
        public void SendMessage(IEnumerable<OrderInfo> infos)
        {
            if (infos == null || infos.Count() == 0)
            {
                return;
            }
            var orderMessage = new MessageOrderInfo();
            var orderIds = infos.Select(item => item.Id).ToArray();
            orderMessage.OrderId = string.Join(",", orderIds);
            var model = infos.FirstOrDefault();
            if (orderIds.Length > 1)
            {
                orderMessage.ShopId = 0;
            }
            else
            {
                orderMessage.ShopId = model.ShopId;
                orderMessage.ShopName = model.ShopName;
            }
            var userId = model.UserId;
            orderMessage.SiteName = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().SiteName;
            orderMessage.TotalMoney = infos.Sum(a => a.OrderTotalAmount);
            orderMessage.UserName = model.UserName;
            orderMessage.Quantity = model.OrderItemInfo.Sum(d => d.Quantity);
            orderMessage.OrderTime = model.OrderDate;
            orderMessage.ProductName = model.OrderItemInfo.FirstOrDefault().ProductName;

            if (model.Platform == PlatformType.WeiXinSmallProg)
            {
                orderMessage.MsgOrderType = MessageOrderType.Applet;
            }
            Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnOrderCreate(userId, orderMessage));
        }

        // 更新限时购活动购买记录
        private void UpdateLimitTimeBuyLog(IEnumerable<OrderItemInfo> orderItems)
        {
            var marketService = ServiceProvider.Instance<ILimitTimeBuyService>.Create;
            //如果是限时购活动商品，更新已购买记录，（日龙）
            foreach (OrderItemInfo orderItem in orderItems)
            {
                if (marketService.IsLimitTimeMarketItem(orderItem.ProductId))
                {
                    var item = Context.LimitTimeMarketInfo.FirstOrDefault(m => m.ProductId == orderItem.ProductId && m.AuditStatus == LimitTimeMarketInfo.LimitTimeMarketAuditStatus.Ongoing);
                    if (null != item)
                    {
                        item.SaleCount += (int)orderItem.Quantity;
                    }
                }
            }
            Context.SaveChanges();
        }

        private void UpdateProductVisti(IEnumerable<OrderItemInfo> orderItems)
        {
            var date1 = DateTime.Now.Date;
            var date2 = DateTime.Now.Date.AddDays(1);

            foreach (OrderItemInfo orderItem in orderItems)
            {
                ProductVistiInfo productVisti = Context.ProductVistiInfo.FindBy(item =>
                    item.ProductId == orderItem.ProductId && item.Date >= date1 && item.Date <= date2).FirstOrDefault();
                if (productVisti == null)
                {
                    productVisti = new ProductVistiInfo();
                    productVisti.ProductId = orderItem.ProductId;
                    productVisti.Date = DateTime.Now.Date;
                    productVisti.OrderCounts = 0;
                    Context.ProductVistiInfo.Add(productVisti);
                }

                var productInfo = Context.ProductInfo.Find(orderItem.ProductId);
                var searchProduct = Context.SearchProductsInfo.FirstOrDefault(r => r.ProductId == orderItem.ProductId);
                if (productInfo != null)
                {
                    productInfo.SaleCounts += orderItem.Quantity;
                    if (searchProduct != null)
                        searchProduct.SaleCount += (int)orderItem.Quantity;
                }
                productVisti.SaleCounts += orderItem.Quantity;
                productVisti.SaleAmounts += orderItem.RealTotalPrice;
            }
            Context.SaveChanges();
        }

        // 更新商品购买的订单总数
        public void UpdateProductVistiOrderCount(long orderId)
        {
            //获取订单明细
            var items = Context.OrderItemInfo.Where(o => o.OrderId == orderId);
            //更新商品购买的订单总数
            foreach (OrderItemInfo model in items.ToList())
            {
                ProductVistiInfo productVisti = Context.ProductVistiInfo.FindBy(p => p.ProductId == model.ProductId).FirstOrDefault();
                if (productVisti != null)
                {
                    productVisti.OrderCounts = (productVisti.OrderCounts == null ? 0 : productVisti.OrderCounts) + 1;
                    Context.SaveChanges();
                }
            }
        }

        // 更新店铺访问量
        private void UpdateShopVisti(OrderInfo order)
        {//TODO:店铺访问量统计，暂时取消实时统计
            /* 
            var date = DateTime.Now.Date;
            ShopVistiInfo shopVisti = Context.ShopVistiInfo.FindBy(item =>
                item.ShopId == order.ShopId && item.Date.Year == date.Year && item.Date.Month == date.Month && item.Date.Day == date.Day).FirstOrDefault();
            if (shopVisti == null)
            {
                shopVisti = new ShopVistiInfo();
                shopVisti.ShopId = order.ShopId;
                shopVisti.Date = DateTime.Now.Date;
                Context.ShopVistiInfo.Add(shopVisti);
            }
            shopVisti.SaleCounts += order.OrderProductQuantity;
            shopVisti.SaleAmounts += order.ProductTotalAmount;
            Context.SaveChanges();
             */
        }

        /// <summary>
        /// 是否可以发货
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private bool CanSendGood(long orderId)
        {
            bool result = false;
            var ordobj = Context.OrderInfo.FirstOrDefault(d => d.Id == orderId);
            if (ordobj == null)
            {
                throw new HimallException("错误的订单编号");
            }
            if (ordobj.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                var fgord = Context.FightGroupOrderInfo.FirstOrDefault(d => d.OrderId == orderId);
                if (fgord.CanSendGood)
                {
                    result = true;
                }
            }
            else
            {
                result = true;
            }
            return result;
        }

        #region 拼团订单
        /// <summary>
        /// 设定拼团订单状态
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private FightGroupOrderJoinStatus SetFightGroupOrderStatus(long orderId, FightGroupOrderJoinStatus status)
        {
            FightGroupOrderJoinStatus result = _iFightGroupService.SetOrderStatus(orderId, status);
            return result;
        }
        /// <summary>
        /// 拼团订单是否可以付款
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private bool FightGroupOrderCanPay(long orderId)
        {
            bool result = false;
            result = _iFightGroupService.OrderCanPay(orderId);
            return result;
        }
        #endregion

        #endregion 私有函数

        public decimal GetRecentMonthAveragePrice(long shopId, long productId)
        {
            decimal average = 0;
            var start = DateTime.Now.AddMonths(-1);

            var list = (from o in Context.OrderInfo
                        join oi in Context.OrderItemInfo on o.Id equals oi.OrderId
                        where o.ShopId == shopId && o.OrderStatus == OrderInfo.OrderOperateStatus.Finish
                        && o.PayDate >= start && o.PayDate <= DateTime.Now && oi.ProductId == productId
                        select oi
                     ).Take(30);

            if (list.Count() == 0)
            {
                average = Context.ProductInfo.FindById(productId).MinSalePrice;
            }
            else
            {
                average = list.Average(s => s.RealTotalPrice - s.DiscountAmount);
            }
            return average;
        }

        public void AutoConfirmOrder()
        {
            try
            {
                //  var siteSetting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();
                var siteSetting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettingsByObjectCache();
                //退换货间隔天数
                int intIntervalDay = siteSetting == null ? 7 : (siteSetting.NoReceivingTimeout == 0 ? 7 : siteSetting.NoReceivingTimeout);
                DateTime waitReceivingDate = DateTime.Now.AddDays(-intIntervalDay);
                var orders = Context.OrderInfo.Where(a => a.ShippingDate < waitReceivingDate && a.OrderStatus == OrderInfo.OrderOperateStatus.WaitReceiving).ToList();
                foreach (var o in orders)
                {
                    //Log.Debug("orderid=" + o.Id.ToString());
                    o.OrderStatus = OrderInfo.OrderOperateStatus.Finish;
                    o.CloseReason = "完成过期未确认收货的订单";
                    o.FinishDate = DateTime.Now;
                    var member = Context.UserMemberInfo.FirstOrDefault(a => a.Id == o.UserId);
                    AddIntegral(member, o.Id, o.ProductTotalAmount - o.DiscountAmount - o.IntegralDiscount - o.RefundTotalAmount);//增加积分
                    if (o.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery)
                    {
                        o.PayDate = DateTime.Now;
                        var orderItems = Context.OrderItemInfo.Where(p => p.OrderId == o.Id).ToList();
                        UpdateProductVisti(orderItems);
                    }

                    #region 到期自动订单确认,写入待结算
                    //现在是拼团完成就结算
                    //WritePendingSettlnment(o);
                    #endregion
                }
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Error("AutoConfirmOrder:" + ex.Message + "/r/n" + ex.StackTrace);
            }
        }

        public void WritePendingSettlnment(OrderInfo o)
        {
            FightGroupService fightGroupService = new FightGroupService();
            //using (TransactionScope transaction = new TransactionScope())
            //{
            try
            {
                //根据订单详情获取团购相关信息
                //1、当前订单对应的团购订单信息
                var fightGroupOrder = fightGroupService.GetOrder(o.Id);
                //2、团购活动信息
                var fightGroupActive = fightGroupService.GetActive((long)fightGroupOrder.ActiveId);
                //3、当前团购订单所有的入团订单
                var fightGroupOrderList = fightGroupService.GetFightGroupOrderByGroupId(null, (long)fightGroupOrder.GroupId);

                //4、当前开团最低成交金额
                decimal miniPay = GetOrder((long)fightGroupOrderList.Last().OrderId).ActualPayAmount;

                var entity = new Entities();
                var orderDetail = Context.OrderItemInfo.Where(a => a.OrderId == o.Id).ToList();
                //退货单的详细
                var hasRefundOrdersDetails = Context.OrderRefundInfo.Where(oo => oo.ManagerConfirmStatus == Himall.Model.OrderRefundInfo.OrderRefundConfirmStatus.Confirmed && oo.OrderId == o.Id).ToList();

                var item = new PendingSettlementOrdersInfo();
                item.ShopId = o.ShopId;
                item.ShopName = o.ShopName;
                item.OrderId = o.Id;
                item.ProductsAmount = o.ProductTotalAmount - o.DiscountAmount - o.FullDiscount;
                item.FreightAmount = o.Freight;

                item.OpenCommission = 0;
                item.JoinCommission = 0;
                item.PlatCommission = 0;


                //如果是团长的订单的话，给开团奖励
                if (Convert.ToBoolean(fightGroupOrder.IsFirstOrder))
                {
                    item.OpenCommission = Convert.ToDecimal(fightGroupActive.OpenGroupReward);
                }


                //当前用户总应该返现
                int returnCount = fightGroupOrderList.Count - 1;
                for (int i = 0; i < fightGroupOrderList.Count; i++)
                {
                    if (fightGroupOrderList[i].OrderId == o.Id)
                    {
                        continue;
                    }
                    returnCount = returnCount - 1;
                }
                item.JoinCommission = Convert.ToDecimal(fightGroupActive.ReturnMoney) * returnCount;


                //平台佣金,团长的订单不需要平台佣金
                //============BEGIN 原来的===============
                //item.PlatCommission = orderDetail.Sum(c => (c.RealTotalPrice - c.CouponDiscount - c.FullDiscount) * c.CommisRate);
                //============END 原来的===============

                if (!Convert.ToBoolean(fightGroupOrder.IsFirstOrder))
                {
                    item.PlatCommission = miniPay * 6 / 100;
                }
                item.PlatCommission = item.PlatCommission;


                //分销佣金
                decimal brokerage = decimal.Zero;
                //============BEGIN 原来的===============
                //var items = orderDetail.Where(a => a.DistributionRate.HasValue).ToList();
                //if (items != null && items.Count > 0)
                //{
                //    brokerage = items.Sum(a => (a.RealTotalPrice - a.CouponDiscount - a.FullDiscount) * a.DistributionRate.Value / 100);
                //}
                //============END 原来的===============

                if (!Convert.ToBoolean(fightGroupOrder.IsFirstOrder))
                {
                    brokerage = Convert.ToDecimal(fightGroupActive.InvitationReward);
                }
                item.DistributorCommission = brokerage;


                decimal refundAmount = hasRefundOrdersDetails.Sum(c => c.Amount);//退款金额
                item.RefundAmount = refundAmount;
                //平台佣金退还
                item.PlatCommissionReturn = hasRefundOrdersDetails.Sum(a => a.ReturnPlatCommission);
                //分销佣金退还
                item.DistributorCommissionReturn = hasRefundOrdersDetails.Sum(a => a.ReturnBrokerage);
                //结算金额=商品实付+运费-平台佣金-分销佣金-退款金额+平台佣金退还+分销佣金退还
                item.SettlementAmount = item.ProductsAmount + item.FreightAmount - item.PlatCommission - item.DistributorCommission - refundAmount + item.PlatCommissionReturn + item.DistributorCommissionReturn;

                if (o.FinishDate.HasValue)
                {
                    item.OrderFinshTime = (DateTime)o.FinishDate;
                }
                else
                {
                    item.OrderFinshTime = DateTime.Now;
                }
                item.PaymentTypeName = o.PaymentTypeName;
                item.OrderAmount = o.OrderAmount;
                Context.PendingSettlementOrdersInfo.Add(item);
                //更新店铺资金账户
                var m = Context.ShopAccountInfo.Where(a => a.ShopId == o.ShopId).FirstOrDefault();
                if (m != null)
                {
                    m.PendingSettlement += item.SettlementAmount;
                }
                //更新平台资金账户
                var plat = Context.PlatAccountInfo.FirstOrDefault();
                if (plat != null)
                {
                    //  var mid = item.PlatCommission - item.PlatCommissionReturn;
                    plat.PendingSettlement += item.SettlementAmount;
                }
                Context.SaveChanges();
                //transaction.Complete();


                ExecuteAccount(item);
            }
            catch (Exception ex)
            {
                Log.Error("WritePendingSettlnment:" + ex.Message + "/r/n" + ex.StackTrace);
            }
            //}
        }

        private void ExecuteAccount(PendingSettlementOrdersInfo pendingSettlementOrdersInfo)
        {
            Log.Debug("[FG]CheckBuildSuccess3.6");
            try
            {
                Entity.Entities entity = new Entities();
                var accountInfo = new AccountInfo();
                accountInfo.ShopId = 0;
                accountInfo.ShopName = "订单结算";
                accountInfo.AccountDate = DateTime.Now;
                accountInfo.StartDate = DateTime.Now;
                accountInfo.EndDate = DateTime.Now;
                accountInfo.Status = AccountInfo.AccountStatus.Accounted;
                accountInfo.ProductActualPaidAmount = pendingSettlementOrdersInfo.ProductsAmount;
                accountInfo.FreightAmount = pendingSettlementOrdersInfo.FreightAmount;
                accountInfo.CommissionAmount = pendingSettlementOrdersInfo.PlatCommission;
                accountInfo.RefundCommissionAmount = pendingSettlementOrdersInfo.PlatCommissionReturn;
                accountInfo.RefundAmount = pendingSettlementOrdersInfo.RefundAmount;
                accountInfo.AdvancePaymentAmount = 0;
                accountInfo.Brokerage = pendingSettlementOrdersInfo.DistributorCommission;
                accountInfo.ReturnBrokerage = pendingSettlementOrdersInfo.DistributorCommissionReturn;
                accountInfo.PeriodSettlement = pendingSettlementOrdersInfo.SettlementAmount;

                accountInfo.OpenCommission = pendingSettlementOrdersInfo.OpenCommission;
                accountInfo.JoinCommission = pendingSettlementOrdersInfo.JoinCommission;
                //结算主表汇总数据

                var accountDetail = new AccountDetailInfo
                {
                    ShopId = pendingSettlementOrdersInfo.ShopId,
                    ShopName = pendingSettlementOrdersInfo.ShopName,
                    OrderType = Model.AccountDetailInfo.EnumOrderType.FinishedOrder,
                    Date = DateTime.Now,
                    OrderFinshDate = pendingSettlementOrdersInfo.OrderFinshTime,
                    OrderId = pendingSettlementOrdersInfo.OrderId,
                    ProductActualPaidAmount = pendingSettlementOrdersInfo.ProductsAmount,
                    FreightAmount = pendingSettlementOrdersInfo.FreightAmount,
                    CommissionAmount = pendingSettlementOrdersInfo.PlatCommission,
                    RefundCommisAmount = pendingSettlementOrdersInfo.PlatCommissionReturn,
                    OrderRefundsDates = "",
                    RefundTotalAmount = pendingSettlementOrdersInfo.RefundAmount,
                    OrderAmount = pendingSettlementOrdersInfo.OrderAmount,
                    BrokerageAmount = pendingSettlementOrdersInfo.DistributorCommission,
                    ReturnBrokerageAmount = pendingSettlementOrdersInfo.DistributorCommissionReturn,
                    SettlementAmount = pendingSettlementOrdersInfo.SettlementAmount,
                    PaymentTypeName = pendingSettlementOrdersInfo.PaymentTypeName,
                    OpenCommission = pendingSettlementOrdersInfo.OpenCommission,
                    JoinCommission = pendingSettlementOrdersInfo.JoinCommission
                };
                accountInfo.Himall_AccountDetails.Add(accountDetail);
                entity.AccountInfo.Add(accountInfo);

                entity.SaveChanges();
                Random r = new Random();

                var plat = entity.PlatAccountInfo.FirstOrDefault();//平台账户
                                                                   //写平台资金明细表
                var platAccountItem = new PlatAccountItemInfo();
                platAccountItem.AccoutID = plat.Id;
                platAccountItem.CreateTime = DateTime.Now;
                platAccountItem.AccountNo = string.Format("{0:yyyyMMddHHmmssfff}{1}", DateTime.Now, r.Next(1000, 9999));
                platAccountItem.Amount = accountInfo.CommissionAmount - accountInfo.RefundCommissionAmount;//平台佣金-平台佣金退还
                platAccountItem.Balance = plat.Balance + platAccountItem.Amount;//账户余额+平台佣金-平台佣金退还
                platAccountItem.TradeType = PlatAccountType.SettlementIncome;
                platAccountItem.IsIncome = true;
                platAccountItem.ReMark = DateTime.Now + "平台结算" + accountInfo.Id;
                platAccountItem.DetailId = accountInfo.Id.ToString();
                entity.PlatAccountItemInfo.Add(platAccountItem);

                if (plat != null)
                {
                    //平台账户总金额(加这次平台的佣金)
                    plat.Balance += platAccountItem.Amount;//平台佣金-平台佣金退还
                                                           //平台待结算金额
                    plat.PendingSettlement -= accountInfo.PeriodSettlement;//本次结算的总金额。//platAccountItem.Amount;//平台佣金-平台佣金退还
                                                                           //平台已结算金额
                    plat.Settled += accountInfo.PeriodSettlement;//本次结算的总金额。//platAccountItem.Amount;//平台佣金-平台佣金退还
                }

                //商户资金明细表
                var shopAccount = entity.ShopAccountInfo.Where(a => a.ShopId == pendingSettlementOrdersInfo.ShopId).FirstOrDefault();

                var shopAccountItemInfo = new ShopAccountItemInfo();
                shopAccountItemInfo.AccoutID = shopAccount.Id;
                shopAccountItemInfo.AccountNo = string.Format("{0:yyyyMMddHHmmssfff}{1}", DateTime.Now, r.Next(1000, 9999));
                shopAccountItemInfo.ShopId = shopAccount.ShopId;
                shopAccountItemInfo.ShopName = shopAccount.ShopName;
                shopAccountItemInfo.CreateTime = DateTime.Now;
                shopAccountItemInfo.Amount = pendingSettlementOrdersInfo.SettlementAmount;//结算金额
                shopAccountItemInfo.Balance = shopAccount.Balance + shopAccountItemInfo.Amount; ;//账户余额+结算金额
                shopAccountItemInfo.TradeType = ShopAccountType.SettlementIncome;
                shopAccountItemInfo.IsIncome = true;
                shopAccountItemInfo.ReMark = "店铺结算明细" + accountInfo.Id; ;
                shopAccountItemInfo.DetailId = accountInfo.Id.ToString();
                shopAccountItemInfo.SettlementCycle = 0;
                entity.ShopAccountItemInfo.Add(shopAccountItemInfo);

                if (shopAccount != null)
                {
                    shopAccount.Balance += shopAccountItemInfo.Amount;//结算金额
                    shopAccount.PendingSettlement -= shopAccountItemInfo.Amount;
                    shopAccount.Settled += shopAccountItemInfo.Amount;//平台佣金-平台佣金退还
                }
                entity.PendingSettlementOrdersInfo.Remove(pendingSettlementOrdersInfo);//结算完了删除已结算数据 
                entity.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Error("ExecuteAccount:" + ex.Message + "/r/n" + ex.StackTrace);
            }
        }

        public void AutoCloseOrder()
        {
            try
            {
                var date = DateTime.Now;
                //  var siteSetting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();
                //采用asp.net cache
                var siteSetting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettingsByObjectCache();
                var orders = Context.OrderInfo.Where(a => a.OrderDate < date && a.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay).ToList();
                var productService = ServiceProvider.Instance<Himall.IServices.IProductService>.Create;
                foreach (var o in orders)
                {
                    int hours = siteSetting == null ? 2 : (siteSetting.UnpaidTimeout == 0 ? 2 : siteSetting.UnpaidTimeout);
                    if (DateTime.Now.Subtract(o.OrderDate).TotalHours >= hours)
                    {
                        //Log.Debug("OrderJob:orderid=" + o.Id.ToString());
                        o.OrderStatus = Model.OrderInfo.OrderOperateStatus.Close;
                        o.CloseReason = "过期没付款，自动关闭";
                        //加回库存
                        ReturnStock(o);
                        var member = Context.UserMemberInfo.FirstOrDefault(a => a.Id == o.UserId);
                        CancelIntegral(member, o.Id, o.IntegralDiscount);//取消订单增加积分

                        //拼团失败
                        if (o.OrderType == OrderInfo.OrderTypes.FightGroup)
                        {
                            SetFightGroupOrderStatus(o.Id, FightGroupOrderJoinStatus.JoinFailed);
                        }
                    }
                }
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Error("AutoCloseOrder:" + ex.Message + "/r/n" + ex.StackTrace);
            }
        }

        public void ConfirmZeroOrder(IEnumerable<long> Ids, long userId)
        {
            var orders = Context.OrderInfo.Where(item => Ids.Contains(item.Id) && item.UserId == userId && item.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay
                || item.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery && item.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery && Ids.Contains(item.Id) && item.UserId == userId).ToList();
            foreach (var order in orders)
            {
                if (order.OrderTotalAmount == 0)
                {
                    if (order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake)
                    {
                        OperaOrderPickupCode(order);
                    }
                    else
                        order.OrderStatus = OrderInfo.OrderOperateStatus.WaitDelivery;

                    order.PaymentType = OrderInfo.PaymentTypes.Online;
                    order.PaymentTypeName = "积分支付";
                    order.PayDate = DateTime.Now;

                    //发布付款成功消息
                    //MessageQueue.PublishTopic(CommonConst.MESSAGEQUEUE_PAYSUCCESSED, order.Id);
                    if (OnOrderPaySuccessed != null)
                        OnOrderPaySuccessed(order.Id);

                    SendAppMessage(order);//支付成功后推送APP消息
                }
            }

            Context.SaveChanges();

            var orderItems = Context.OrderItemInfo.Where(p => Ids.Contains(p.OrderId)).ToList();
            foreach (var order in orders)
            {
                UpdateProductVisti(orderItems.Where(p => p.OrderId == order.Id));
            }
            ServiceProvider.Instance<ILimitTimeBuyService>.Create.IncreaseSaleCount(Ids.ToList());
        }

        public void CancelOrders(IEnumerable<long> Ids, long userId)
        {
            if (Ids.Count() > 0)
            {
                Context.OrderItemInfo.Remove(p => Ids.Contains(p.OrderId));
                Context.OrderInfo.Remove(p => Ids.Contains(p.Id));
                Context.SaveChanges();
            }
        }

        //TODO LRL 2015/08/06 获取子订单对象
        public OrderItemInfo GetOrderItem(long orderItemId)
        {
            return Context.OrderItemInfo.FindById(orderItemId);
        }

        /// <summary>
        /// 获取昨天订单交易金额
        /// </summary>
        /// <param name="shopId">店铺ID平台不需要填写</param>
        /// <returns></returns>
        public decimal GetYesterDaySaleAmount(long? shopId = null)
        {
            decimal Amount = 0.00M;
            var cacheKey = CacheKeyCollection.YesterDaySaleAmount(shopId);
            if (Cache.Exists(cacheKey))
            {
                Amount = Core.Cache.Get<decimal>(cacheKey);
            }
            else
            {
                var nowDate = DateTime.Now.Date;
                var yesterDay = nowDate.AddDays(-1);
                var query = Context.OrderInfo.Where(a => a.PayDate.HasValue && a.PayDate.Value >= yesterDay && a.PayDate.Value < nowDate);
                if (shopId.HasValue && shopId != 0)
                {
                    query = query.Where(a => a.ShopId == shopId.Value);
                }
                Amount = query.Sum(a => (decimal?)(a.ProductTotalAmount + a.Freight + a.Tax - a.DiscountAmount)).GetValueOrDefault();
                Core.Cache.Insert<decimal>(cacheKey, Amount, DateTime.Now.AddDays(1));
            }
            return Amount;
        }

        /// <summary>
        /// 昨天下单订单数
        /// </summary>
        /// <param name="shopId">店铺ID平台不需要填写</param>
        /// <returns></returns>
        public int GetYesterDayOrdersNum(long? shopId = null)
        {
            int num = 0;
            var cacheKey = CacheKeyCollection.YesterDayOrdersNum(shopId);
            if (Cache.Exists(cacheKey))
            {
                num = Core.Cache.Get<Int32>(cacheKey);
            }
            else
            {
                var nowDate = DateTime.Now.Date;
                var yesterDay = nowDate.AddDays(-1);
                var query = Context.OrderInfo.Where(a => a.OrderDate >= yesterDay && a.OrderDate < nowDate);
                if (shopId.HasValue && shopId != 0)
                {
                    query = query.Where(a => a.ShopId == shopId.Value);
                }
                num = query.Count();
                Core.Cache.Insert<Int32>(cacheKey, num, DateTime.Now.AddDays(1));
            }
            return num;
        }
        /// <summary>
        /// 昨天付款订单数
        /// </summary>
        /// <param name="shopId">店铺ID平台不需要填写</param>
        /// <returns></returns>
        public int GetYesterDayPayOrdersNum(long? shopId = null)
        {
            int num = 0;

            var cacheKey = CacheKeyCollection.YesterDayPayOrdersNum(shopId);
            if (Cache.Exists(cacheKey))
            {
                num = Core.Cache.Get<Int32>(cacheKey);
            }
            else
            {
                var nowDate = DateTime.Now.Date;
                var yesterDay = nowDate.AddDays(-1);
                var query = Context.OrderInfo.Where(a => a.PayDate.HasValue && a.PayDate.Value >= yesterDay && a.PayDate.Value < nowDate);
                if (shopId.HasValue && shopId != 0)
                {
                    query = query.Where(a => a.ShopId == shopId.Value);
                }
                num = query.Count();
                Core.Cache.Insert<Int32>(cacheKey, num, DateTime.Now.AddDays(1));
            }
            return num;
        }

        /// <summary>
        /// 商家给订单备注
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="reMark"></param>
        /// <param name="shopId">店铺ID</param>
        public void UpdateSellerRemark(long orderId, long shopId, string reMark, int flag)
        {
            var order = Context.OrderInfo.Where(a => a.Id == orderId && a.ShopId == shopId).FirstOrDefault();
            if (order != null)
            {
                order.SellerRemark = reMark;
                order.SellerRemarkFlag = flag;
                Context.SaveChanges();
            }
            else
            {
                throw new HimallException("找不到该订单数据");
            }
        }

        /// <summary>
        /// 根据提货码取订单
        /// </summary>
        /// <param name="pickCode"></param>
        /// <returns></returns>
        public OrderInfo GetOrderByPickCode(string pickCode)
        {
            return Context.OrderInfo.FirstOrDefault(e => e.PickupCode == pickCode);
        }

        #region 创建sku列表
        /// <summary>
        /// 判断数据是否能获取sku列表，否则抛出异常
        /// </summary>
        private void CheckWhenCreateOrder(OrderCreateModel model)
        {
            if (model.CurrentUser.Id <= 0)
                throw new InvalidPropertyException("会员Id无效");
            if (model.SkuIds == null || model.SkuIds.Count() == 0)
                throw new InvalidPropertyException("待提交订单的商品不能为空");
            if (model.Counts == null || model.Counts.Count() == 0)
                throw new InvalidPropertyException("待提交订单的商品数量不能为空");
            if (model.Counts.Count(item => item <= 0) > 0)
                throw new InvalidPropertyException("待提交订单的商品数量必须都大于0");
            if (model.SkuIds.Count() != model.Counts.Count())
                throw new InvalidPropertyException("商品数量不一致");
            if (model.ReceiveAddressId <= 0)
                throw new InvalidPropertyException("收货地址无效");

            var fgser = ServiceProvider.Instance<IFightGroupService>.Create;
            FightGroupActiveInfo actobj = null;
            if (model.ActiveId > 0)
            {
                actobj = fgser.GetActive(model.ActiveId);
                if (model.GroupId > 0)
                {
                    var canjoin = fgser.CanJoinGroup(model.ActiveId, model.GroupId, model.CurrentUser.Id);
                    if (!canjoin)
                    {
                        throw new InvalidPropertyException("不可参团，可能重复参团或火拼团不在进行中");
                    }


                    var hasBuynum = fgser.GetMarketSaleCountForUserId(model.GroupId, model.CurrentUser.Id);

                    if (model.Counts.Any(p => hasBuynum + p > actobj.LimitQuantity))
                    {
                        throw new InvalidPropertyException("购买数量错误，每人限购" + actobj.LimitQuantity + "件，您还可以购买：" + (actobj.LimitQuantity - hasBuynum) + "件");
                    }
                }
            }

            var productService = ServiceProvider.Instance<IProductService>.Create;
            var limitTimeService = ServiceProvider.Instance<ILimitTimeBuyService>.Create;

            var skus = productService.GetSKUs(model.SkuIds);
            var skuAndCounts = new Dictionary<SKUInfo, int>();

            for (int i = 0; i < model.SkuIds.Count(); i++)
            {
                var skuId = model.SkuIds.ElementAt(i);
                var sku = skus.FirstOrDefault(p => p.Id == skuId);
                if (sku == null)
                    throw new InvalidPropertyException("未找到" + skuId + "对应的商品");
                if (!skuAndCounts.ContainsKey(sku))
                    skuAndCounts.Add(sku, model.Counts.ElementAt(i));
            }

            var products = productService.GetAllProductByIds(skus.Select(p => p.ProductId));

            if (products.Any(p => p.SaleStatus != ProductInfo.ProductSaleStatus.OnSale && p.AuditStatus != ProductInfo.ProductAuditStatus.Audited))
                throw new InvalidPropertyException(products.Count > 1 ? "商品中有下架商品" : "商品已下架");
            if (products.Any(p => p.IsDeleted))
                throw new InvalidPropertyException(products.Count > 1 ? "商品中有删除商品" : "商品已删除");

            if (products.Any(p => p.MaxBuyCount > 0))
            {
                var buyedCounts = GetProductBuyCount(model.CurrentUser.Id, products.Where(p => p.MaxBuyCount > 0).Select(p => p.Id));
                var outMaxBuyCountProduct = products.FirstOrDefault(p => p.MaxBuyCount > 0 && p.MaxBuyCount < skus.Where(sku => sku.ProductId == p.Id).Sum(sku => skuAndCounts[sku]) + (buyedCounts.ContainsKey(p.Id) ? buyedCounts[p.Id] : 0));
                if (outMaxBuyCountProduct != null)
                    throw new InvalidPropertyException(string.Format("已超出商品[{0}]的最大购买数", outMaxBuyCountProduct.ProductName));
            }
            
            int JoinedNumber = 0;
            if (model.GroupId > 0)
            {
                FightGroupsInfo fightGroupInfo = fgser.GetGroup(model.ActiveId, model.GroupId);
                JoinedNumber = Convert.ToInt32(fightGroupInfo.JoinedNumber);
            }
            foreach (var sku in skus)
            {
                int buynum = skuAndCounts[sku];
                var product = products.FirstOrDefault(p => p.Id == sku.ProductId);

                if (actobj != null)
                {
                    var activeItem = actobj.ActiveItems.FirstOrDefault(d => d.SkuId == sku.Id);
                    if (activeItem == null)
                    {
                        throw new InvalidPropertyException("未找到" + sku.Id + "对应的商品");
                    }
                    if (activeItem.ActiveStock < buynum)
                    {
                        throw new InvalidPropertyException("商品“" + actobj.ProductName + "”库存不够，仅剩" + activeItem.ActiveStock + "件");
                    }

                    sku.SalePrice = Convert.ToDecimal(activeItem.ActivePrice - actobj.ReturnMoney * JoinedNumber);
                    sku.Stock = activeItem.ActiveStock.Value;
                }
                else
                {
                    if (model.CollPids == null || (model.CollPids != null && model.CollPids.Count() <= 0))//非组合购，判断限时购
                    {
                        var limitProduct = limitTimeService.GetLimitTimeMarketItemByProductId(sku.ProductId);
                        if (limitProduct != null)
                        {
                            model.IslimitBuy = true;
                            var userByProductCount = limitTimeService.GetMarketSaleCountForUserId(sku.ProductId, model.CurrentUser.Id);
                            if (limitProduct != null && limitProduct.LimitCountOfThePeople < (userByProductCount + buynum))
                            {
                                throw new HimallException("您购买数量超过限时购限定的最大数！");
                            }
                        }
                    }

                    if (sku.Stock < buynum)
                    {
                        throw new HimallException("商品“" + product.ProductName + "”库存不够，仅剩" + sku.Stock + "件");
                    }
                }

                model.ProductList.Add(product);
                model.SKUList.Add(sku);
            }
        }

        /// <summary>
        /// 创建新sku列表对象
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<OrderSkuInfo> GetOrderSkuInfo(OrderCreateModel model)
        {
            int i = -1;
            var products = model.SkuIds.Select(skuId =>
            {
                var skuInfo = model.SKUList.FirstOrDefault(s => s.Id.Equals(skuId));
                var product = model.ProductList.FirstOrDefault(p => p.Id.Equals(skuInfo.ProductId));
                i += 1;
                return new OrderSkuInfo
                {
                    SKU = skuInfo,
                    Product = product,
                    Quantity = model.Counts.ElementAt(i),
                    ColloPid = model.CollPids != null && model.CollPids.Any() ? model.CollPids.ElementAt(i) : 0
                };
            }).ToList();

            return products;
        }
        #endregion

        #region OrderBO 的方法

        /// <summary>
        /// 设置运费
        /// </summary>
        /// <param name="order">订单</param>
        /// <param name="freight">运费</param>
        public void SetFreight(OrderInfo order, decimal freight)
        {
            if (freight < 0)
            {
                throw new HimallException("运费不能为负值！");
            }
            order.Freight = freight;
        }

        /// <summary>
        /// 设置订单状态为完成
        /// </summary>
        public void SetStateToConfirm(OrderInfo order)
        {
            if (order == null)
            {
                throw new HimallException("处理订单错误，请确认该订单状态正确");
            }
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitReceiving)
            {
                throw new HimallException("只有等待收货状态的订单才能进行确认操作");
            }
            if (order.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery)
            {
                order.PayDate = DateTime.Now;
            }
            order.OrderStatus = OrderInfo.OrderOperateStatus.Finish;
            order.FinishDate = DateTime.Now;
        }

        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="order">订单ID</param>
        /// <param name="closeReason">理由</param>
        public void CloseOrder(OrderInfo order)
        {
            CheckCloseOrder(order);

            order.OrderStatus = OrderInfo.OrderOperateStatus.Close;
        }
        /// <summary>
        /// 检测订单是否可以被关闭
        /// </summary>
        /// <param name="order"></param>
        public void CheckCloseOrder(OrderInfo order)
        {
            if (order.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.WaitPay ||
                   order.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.WaitDelivery && order.PaymentType == Himall.Model.OrderInfo.PaymentTypes.CashOnDelivery)
            {
                if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
                {
                    var fgser = ServiceProvider.Instance<IFightGroupService>.Create;
                    var fgord = fgser.GetFightGroupOrderStatusByOrderId(order.Id);
                    if (
                        fgord.JoinStatus == FightGroupOrderJoinStatus.Ongoing.GetHashCode() ||
                        fgord.JoinStatus == FightGroupOrderJoinStatus.JoinSuccess.GetHashCode() ||
                        fgord.JoinStatus == FightGroupOrderJoinStatus.BuildSuccess.GetHashCode()
                        )
                    {
                        throw new HimallException("拼团订单不可关闭");
                    }
                }
            }
            else
            {
                throw new HimallException("只有待付款状态或货到付款待发货状态的订单才能进行取消操作");
            }
        }

        /// <summary>
        /// 检查是否满额免运费
        /// </summary>
        public bool IsFullFreeFreight(ShopInfo shop, decimal OrderPaidAmount)
        {
            bool result = false;
            if (shop != null)
            {
                if (shop.FreeFreight > 0)
                {
                    if (OrderPaidAmount >= shop.FreeFreight)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取真实费用
        /// </summary>
        public decimal GetRealTotalPrice(OrderInfo order, OrderItemInfo item, decimal discountAmount)
        {
            if (item.RealTotalPrice - discountAmount < 0)
            {
                throw new HimallException("优惠金额不能大于商品总金额！");
            }
            if (order.OrderTotalAmount - discountAmount < 0)
            {
                throw new HimallException("减价不能导致订单总金额为负值！");
            }

            return item.RealTotalPrice - discountAmount;
        }


        private static object obj = new object();
        /// <summary>
        ///  生成订单号
        /// </summary>
        public long GenerateOrderNumber()
        {
            lock (obj)
            {
                int rand;
                char code;
                string orderId = string.Empty;
                Random random = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
                for (int i = 0; i < 5; i++)
                {
                    rand = random.Next();
                    code = (char)('0' + (char)(rand % 10));
                    orderId += code.ToString();
                }
                return long.Parse(DateTime.Now.ToString("yyyyMMddfff") + orderId);
            }
        }

        private static object objpay = new object();
        /// <summary>
        /// 生成支付订单号
        /// </summary>
        /// <returns></returns>
        public long GetOrderPayId()
        {
            lock (objpay)
            {
                int rand;
                char code;
                string orderId = string.Empty;
                Random random = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
                for (int i = 0; i < 6; i++)
                {
                    rand = random.Next();
                    code = (char)('0' + (char)(rand % 10));
                    orderId += code.ToString();
                }
                return long.Parse(DateTime.Now.ToString("yyMMddmmHHss") + orderId);
            }
        }


        /// <summary>
        /// 保存发票内容
        /// </summary>
        public void AddInvoiceTitle(string titleName, long userid)
        {
            if (!Context.InvoiceTitleInfo.Exist(p => p.Name == titleName))
            {
                Context.InvoiceTitleInfo.Add(new InvoiceTitleInfo() { Name = titleName, UserId = userid });
                Context.SaveChanges();
            }
        }

        #region 创建额外订单信息
        /// <summary>
        /// 从订单对象中拆分出额外的订单信息
        /// </summary>
        public OrderCreateAdditional CreateAdditional(IEnumerable<OrderSkuInfo> orderSkuInfos, OrderCreateModel model)
        {
            var baseOrderCoupons = GetOrdersCoupons(model.CurrentUser.Id, model.CouponIdsStr);//获取所有订单使用的优惠券列表
            var orderIntegral = GetIntegralDiscountAmount(model.Integral, model.CurrentUser.Id);//获取所有订单能使用的积分金额
            var address = ServiceProvider.Instance<IShippingAddressService>.Create.GetUserShippingAddress(model.ReceiveAddressId);//获取用户的收货地址
            if (address == null)
            {
                throw new HimallException("错误的收货地址！");
            }
            OrderCreateAdditional additional = new OrderCreateAdditional();
            additional.BaseCoupons = baseOrderCoupons;
            additional.Address = address;
            additional.IntegralTotal = orderIntegral;
            additional.CreateDate = DateTime.Now;
            var setting = GetDistributorSetting();
            if (setting != null)
                additional.IsEnableDistribution = setting.Enable;
            return additional;
        }

        private DistributorSettingInfo GetDistributorSetting()
        {
            string cacheKey = CacheKeyCollection.CACHE_DISTRIBUTORSETTING;
            if (Cache.Exists(cacheKey))
                return Cache.Get<DistributorSettingInfo>(cacheKey);

            DistributorSettingInfo result = new DistributorSettingInfo();
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                string sql = "select * from himall_distributorsetting";
                result = conn.QueryFirstOrDefault<DistributorSettingInfo>(sql);
            }
            Cache.Insert<DistributorSettingInfo>(cacheKey, result, 1800);
            return result;
        }

        /// <summary>
        /// 获取所有订单使用的优惠券列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="couponIdsStr"></param>
        /// <returns></returns>
        private IEnumerable<BaseAdditionalCoupon> GetOrdersCoupons(long userId, IEnumerable<string[]> couponIdsStr)
        {
            var couponService = ServiceProvider.Instance<ICouponService>.Create;
            var shopBonusService = ServiceProvider.Instance<IShopBonusService>.Create;
            if (couponIdsStr == null || couponIdsStr.Count() <= 0)
            {
                return null;
            }
            List<BaseAdditionalCoupon> list = new List<BaseAdditionalCoupon>();
            foreach (string[] str in couponIdsStr)
            {
                BaseAdditionalCoupon item;
                if (int.Parse(str[1]) == 0)
                {
                    var obj = couponService.GetOrderCoupons(userId, new long[] { long.Parse(str[0]) }).FirstOrDefault();
                    if (obj == null)
                        throw new HimallException("优惠券不存在或优惠券已使用!");
                    item = new BaseAdditionalCoupon();
                    item.Type = 0;
                    item.Coupon = obj;
                    item.ShopId = obj.ShopId;
                }
                else if (int.Parse(str[1]) == 1)
                {
                    var obj = shopBonusService.GetDetailById(userId, long.Parse(str[0]));
                    item = new BaseAdditionalCoupon();
                    item.Type = 1;
                    item.Coupon = obj;
                    item.ShopId = obj.Himall_ShopBonusGrant.Himall_ShopBonus.ShopId;
                }
                else
                {
                    item = new BaseAdditionalCoupon();
                    item.Type = 99;
                }

                list.Add(item);
            }

            return list;
        }
        #endregion

        #region  创建订单列表对象
        /// <summary>
        /// 创建订单列表对象
        /// </summary>
        public List<OrderInfo> GetOrderInfos(IEnumerable<OrderSkuInfo> orderSkuInfos, OrderCreateModel model, OrderCreateAdditional additional)
        {
            //把购买的商品信息按店铺分组
            var cartGroupInfo = orderSkuInfos.GroupBy(item => item.Product.ShopId).ToList();

            List<OrderInfo> infos = new List<OrderInfo>();
            int i = 0;
            foreach (var item in cartGroupInfo)
            {
                string remark = string.Empty;
                if (model.OrderRemarks.LongCount() >= i + 1)
                {//如果有输入备注，则补充。正常情况下，备注条数与订单数一致
                    remark = model.OrderRemarks.ElementAt(i);
                }
                var order = CreateOrderInfo(item, model, additional, remark);
                order.DeliveryType = CommonModel.Enum.DeliveryType.Express;
                if (model.OrderShops != null)
                {//过滤null,商城APP订单提交时，暂未处理门店逻辑
                    var orderShop = model.OrderShops.FirstOrDefault(p => p.ShopId == order.ShopId);
                    if (orderShop != null)
                    {
                        order.ShopBranchId = orderShop.ShopBrandId;
                        if (orderShop.ShopBrandId != null)
                        {
                            order.DeliveryType = orderShop.DeliveryType;
                        }
                    }
                    if (!order.ShopBranchId.HasValue)
                    {
                        order.ShopBranchId = 0;
                    }
                }
                if (order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake)
                    order.Freight = 0;

                i++;
                infos.Add(order);
            }

            //计算商品总金额，并且减去优惠券的金额，减满额减价格
            decimal productsTotals = infos.Sum(a => a.ProductTotal); //商品价-优惠券-满额减
            if (additional.IntegralTotal > productsTotals)
            {
                throw new HimallException("积分抵扣金额不能超过商品总金额！");
            }

            //处理积分
            ProcessIntegralDiscount(infos, additional, productsTotals);

            foreach (var item in infos)
            {
                item.TotalAmount = item.OrderTotalAmount;
            }

            return infos;
        }
        /// <summary>
        /// 创建单个订单对象
        /// </summary>
        /// <returns></returns>
        private OrderInfo CreateOrderInfo(IGrouping<long, OrderSkuInfo> groupItem, OrderCreateModel model, OrderCreateAdditional additional, string remark)
        {
            int cityId = 0;
            if (additional.Address != null)
            {
                //cityId = ServiceProvider.Instance<IRegionService>.Create.GetRegion(additional.Address.RegionId, Region.RegionLevel.City).Id;
                cityId = additional.Address.RegionId;
            }
            var user = model.CurrentUser;
            var shop = ServiceProvider.Instance<IShopService>.Create.GetShop(groupItem.Key);
            if (shop.ShopStatus == ShopInfo.ShopAuditStatus.Freeze || shop.ShopStatus == ShopInfo.ShopAuditStatus.HasExpired)
            {
                throw new HimallException(shop.ShopName + "已冻结或者过期，请移除此店铺的商品！");
            }

            IEnumerable<long> productIds = groupItem.ToList().Select(item => item.Product.Id);
            IEnumerable<int> productCounts = groupItem.ToList().Select(item => (int)item.Quantity);

            //总的商品价格计算每一个订单项的价格
            //  decimal productTotalAmount = groupItem.Sum(item => GetSalePrice(item.Product.Id, item.SKU, item.ColloPid, productIds.Count()) * item.Quantity);

            //货到付款相关
            string paymentTypeName = "";
            OrderInfo.OrderOperateStatus orderstatus;
            OrderInfo.PaymentTypes paymentType;
            if (model.IsCashOnDelivery && groupItem.First().Product.ShopId == 1)
            {
                paymentTypeName = "货到付款";
                orderstatus = OrderInfo.OrderOperateStatus.WaitDelivery;

                paymentType = OrderInfo.PaymentTypes.CashOnDelivery;
            }
            else
            {
                orderstatus = OrderInfo.OrderOperateStatus.WaitPay;
                paymentType = OrderInfo.PaymentTypes.None;
            }

            var order = new OrderInfo()
            {
                Id = GenerateOrderNumber(),
                ShopId = groupItem.Key,
                ShopName = shop.ShopName,
                UserId = user.Id,
                UserName = user.UserName,
                OrderDate = additional.CreateDate,
                RegionId = additional.Address.RegionId,
                ShipTo = additional.Address.ShipTo,
                Address = additional.Address.Address,
                RegionFullName = additional.Address.RegionFullName,
                CellPhone = additional.Address.Phone,
                TopRegionId = int.Parse(additional.Address.RegionIdPath.Split(',')[0]),
                ReceiveAddressId = additional.Address.Id,
                OrderStatus = orderstatus,
                Freight = ServiceProvider.Instance<IProductService>.Create.GetFreight(productIds, productCounts, cityId),
                IsPrinted = false,
                OrderRemarks = remark,
                // ProductTotalAmount = productTotalAmount,商品总价格
                RefundTotalAmount = 0,
                CommisTotalAmount = 0,
                RefundCommisAmount = 0,
                Platform = model.PlatformType,
                InvoiceType = model.Invoice,
                InvoiceTitle = model.InvoiceTitle,
                InvoiceContext = model.InvoiceContext,
                //修改优惠券优惠金额不在此计算
                //DiscountAmount = GetShopCouponDiscount(additional.BaseCoupons, groupItem.Key),
                PaymentTypeName = paymentTypeName,
                PaymentType = paymentType,
                LastModifyTime = DateTime.Now
            };
            if (model.CollPids != null && model.CollPids.Count() > 1)
            {
                order.OrderType = Himall.Model.OrderInfo.OrderTypes.Collocation;
            }
            if (model.ActiveId > 0)
            {
                order.OrderType = Himall.Model.OrderInfo.OrderTypes.FightGroup;  //拼团订单
            }
            if (model.IslimitBuy)
            {
                order.OrderType = Himall.Model.OrderInfo.OrderTypes.LimitBuy;
            }

            if (additional.IsEnableDistribution) //如果开启了分销并且有推销员的Id(重新取一遍用户)
            {
                var shareUser = Context.DistributionUserLinkInfo.Where(a => a.BuyUserId == user.Id && a.ShopId == order.ShopId).FirstOrDefault();
                if (shareUser != null && shareUser.PartnerId > 0)
                {
                    order.ShareUserId = shareUser.PartnerId;
                }
            }
            var productCount = productIds.Count();
            foreach (var item in groupItem)
            {
                if (item.Product.SaleStatus != Himall.Model.ProductInfo.ProductSaleStatus.OnSale || item.Product.AuditStatus != Himall.Model.ProductInfo.ProductAuditStatus.Audited)
                {
                    throw new HimallException("订单中有失效的商品，请返回重新提交！");
                }
                var orderItem = new OrderItemInfo();
                orderItem.OrderId = order.Id;
                orderItem.ShopId = order.ShopId;
                orderItem.ProductId = item.Product.Id;
                orderItem.SkuId = item.SKU.Id;
                orderItem.Quantity = item.Quantity;
                orderItem.SKU = item.SKU.Sku;
                orderItem.ReturnQuantity = 0;
                orderItem.CostPrice = item.SKU.CostPrice;
                orderItem.SalePrice = item.SKU.SalePrice;

                if (shop.IsSelf)//官方自营店无平台佣金
                {
                    orderItem.CommisRate = 0;
                }
                else
                {

                    decimal CommisRate = Context.BusinessCategoryInfo.Where(b => b.CategoryId == item.Product.CategoryId && b.ShopId == item.Product.ShopId).Select(a => a.CommisRate).FirstOrDefault();
                    orderItem.CommisRate = CommisRate / 100;
                }
                if (order.ShareUserId.HasValue && order.ShareUserId.Value > 0)
                {
                    bool IsDistributionProduct = Context.DistributionUserLinkInfo.Any(a => a.PartnerId == order.ShareUserId && a.ShopId == order.ShopId);

                    if (IsDistributionProduct)//如果用户分享过此店铺的商品则计算佣金
                    {
                        var productRate = Context.ProductBrokerageInfo.Where(a => a.ProductId == item.Product.Id && a.Status == ProductBrokerageInfo.ProductBrokerageStatus.Normal).FirstOrDefault();
                        if (productRate != null && productRate.rate != 0) //商品的佣金比例存在订单表防止改价的时候直接存金额不正确
                        {
                            orderItem.DistributionRate = productRate.rate;
                        }
                    }
                }
                //不是限时购和拼团计算会员价

                if (order.OrderType != OrderInfo.OrderTypes.FightGroup && order.OrderType != OrderInfo.OrderTypes.LimitBuy)
                {
                    if (shop.IsSelf) //如果是自营店计算会员折扣
                    {
                        orderItem.SalePrice = user.MemberDiscount * orderItem.SalePrice;
                    }
                }
                //组合购或者限时购的价格
                orderItem.SalePrice = GetSalePrice(item.SKU.Id, orderItem.SalePrice, item.ColloPid, productCount, order.OrderType == OrderInfo.OrderTypes.LimitBuy);

                orderItem.IsLimitBuy = model.IslimitBuy;
                orderItem.DiscountAmount = 0;
                var itemTotal = (orderItem.SalePrice * item.Quantity);
                orderItem.RealTotalPrice = itemTotal;
                orderItem.RefundPrice = 0;

                orderItem.Color = item.SKU.Color;
                orderItem.Size = item.SKU.Size;
                orderItem.Version = item.SKU.Version;
                orderItem.ProductName = item.Product.ProductName;
                orderItem.ThumbnailsUrl = item.Product.RelativePath;
                order.OrderItemInfo.Add(orderItem);
            }

            //订单的商品价格只包含会员价，限时购价，拼团价等不包含其他优惠
            order.ProductTotalAmount = order.OrderItemInfo.Sum(a => a.RealTotalPrice);

            //处理平摊满额减
            SetOrderFullDiscount(order);

            if (additional.BaseCoupons != null)
            {
                var coupon = additional.BaseCoupons.Where(a => a.ShopId == order.ShopId).FirstOrDefault();

                if (coupon != null)//存在使用优惠券的情况
                {
                    decimal couponUseAmount = 0;
                    decimal couponDiscount = 0;
                    if (coupon.Type == 0)//优惠券
                    {
                        var couponObj = (coupon.Coupon as CouponRecordInfo);
                        couponUseAmount = couponObj.BaseOrderAmount;
                        couponDiscount = couponObj.BasePrice;
                    }
                    else if (coupon.Type == 1)//代金红包
                    {
                        var couponObj = (coupon.Coupon as ShopBonusReceiveInfo);
                        couponUseAmount = couponObj.BaseUseStatePrice;
                        couponDiscount = couponObj.BasePrice;
                    }
                    if (order.ProductTotalAmount < couponUseAmount)
                    {
                        throw new HimallException("优惠券不满足使用条件");
                    }
                    if (couponDiscount >= order.ProductTotalAmount) //优惠券面值大于商品总金额,优惠金额为订单总金额
                    {
                        order.DiscountAmount = order.ProductTotalAmount;
                    }
                    else
                    {
                        order.DiscountAmount = couponDiscount;
                    }
                    //订单处理平摊优惠券
                    SetActualItemPrice(order);
                }
            }

            //满额免运费判断，不为货到付款时才执行
            if (IsFullFreeFreight(shop, order.ProductTotalAmount - order.FullDiscount - order.DiscountAmount))
            {
                order.Freight = 0;
            }
            return order;
        }
        #endregion


        /// <summary>
        /// 订单提交处理满额减
        /// </summary>
        /// <param name="order"></param>
        private static void SetOrderFullDiscount(OrderInfo order)
        {
            var productIds = order.OrderItemInfo.Select(A => A.ProductId).Distinct();
            var actives = ServiceProvider.Instance<IFullDiscountService>.Create.GetOngoingActiveByProductIds(productIds, order.ShopId);
            decimal orderFullDiscount = 0;
            foreach (var active in actives)
            {
                var pids = active.Products.Select(a => a.ProductId);
                var items = order.OrderItemInfo;
                if (!active.IsAllProduct)
                {
                    items = items.Where(a => pids.Contains(a.ProductId)).ToList();
                }
                var realTotal = items.Sum(a => a.RealTotalPrice);  //满足满额减的总商品金额
                var rule = active.Rules.Where(a => a.Quota <= realTotal).OrderByDescending(a => a.Quota).FirstOrDefault();
                decimal fullDiscount = 0;
                if (rule != null)//找不到就是不满足金额
                {
                    fullDiscount = rule.Discount;
                    var infos = items.ToArray();
                    var count = items.Count();
                    decimal itemFullDiscount = 0;
                    //平分优惠金额到各个订单项
                    for (var i = 0; i < count; i++)
                    {
                        var _order = infos[i];
                        if (i < count - 1)
                        {
                            infos[i].FullDiscount = Math.Round(fullDiscount * (infos[i].RealTotalPrice) / realTotal, 2);
                            itemFullDiscount += infos[i].FullDiscount;
                        }
                        else
                        {
                            infos[i].FullDiscount = fullDiscount - itemFullDiscount;
                        }
                    }
                    orderFullDiscount += fullDiscount; //订单总优惠金额 
                }

            }
            order.FullDiscount = orderFullDiscount;
        }





        /// <summary>
        /// 处理积分
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="additional"></param>
        /// <param name="productsTotals"></param>
        /// <returns></returns>
        public void ProcessIntegralDiscount(List<OrderInfo> infos, OrderCreateAdditional additional, decimal productsTotals)
        {
            var t = infos.Count;
            decimal integralDiscount = 0;
            for (var i = 0; i < infos.Count; i++)
            {
                var _order = infos[i];
                if (i < t - 1)
                {
                    infos[i].IntegralDiscount = Math.Round(additional.IntegralTotal * (infos[i].ProductTotalAmount - infos[i].DiscountAmount - infos[i].FullDiscount) / productsTotals, 2);//积分抵扣应该也要减去满额减
                    integralDiscount += infos[i].IntegralDiscount;
                }
                else
                {
                    infos[i].IntegralDiscount = additional.IntegralTotal - integralDiscount;
                }
            }
        }

        /// <summary>
        /// 处理商品库存，这是订单服务内部的方法，整个网站就一个地方使用
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="stockChange"></param>
        private void UpdateSKUStockInOrder(string skuId, long stockChange)
        {
            var sku = Context.SKUInfo.FirstOrDefault(item => item.Id == skuId);
            if (sku != null)
            {
                sku.Stock += stockChange;
                if (sku.Stock < 0)
                    throw new HimallException("商品库存不足");
            }
        }

        private void UpdateShopBranchSku(long shopBranchId, string skuId, int stockChange)
        {
            var sku = Context.SKUInfo.FirstOrDefault(item => item.Id == skuId);
            if (sku != null)
            {
                var sbSku = Context.ShopBranchSkusInfo.FirstOrDefault(p => p.ShopBranchId == shopBranchId && p.SkuId == sku.Id);
                if (sbSku != null)
                {
                    sbSku.Stock += stockChange;
                    if (sbSku.Stock < 0)
                        throw new HimallException("门店库存不足");
                }
            }
        }

        /// <summary>
        /// 优惠券状态改变
        /// </summary>
        public void UseCoupon(List<OrderInfo> orders, List<BaseAdditionalCoupon> coupons, long userid)
        {
            var couponService = ServiceProvider.Instance<ICouponService>.Create;
            var shopBonusService = ServiceProvider.Instance<IShopBonusService>.Create;
            foreach (var coupon in coupons)
            {
                if (coupon.Type == 0)
                {
                    long id = (coupon.Coupon as CouponRecordInfo).Id;
                    couponService.UseCoupon(userid, new long[] { id }, orders);
                }
                else if (coupon.Type == 1)
                {
                    long id = (coupon.Coupon as ShopBonusReceiveInfo).Id;
                    shopBonusService.SetBonusToUsed(userid, orders, id);
                }
            }
        }
        #endregion
        #endregion

        #region 私有方法
        /// <summary>
        /// 处理自提订单提货码
        /// <para>拼团订单的提货码需要成团成功后生成</para>
        /// </summary>
        /// <param name="order"></param>
        /// <param name="isMust">必须生成</param>
        private void OperaOrderPickupCode(OrderInfo order)
        {
            if (order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake)
            {
                order.OrderStatus = OrderInfo.OrderOperateStatus.WaitSelfPickUp;
                if (order.OrderType != OrderInfo.OrderTypes.FightGroup)
                {
                    order.PickupCode = GeneratePickupCode(order.Id);
                }
            }
        }
        public static string GeneratePickupCode(long orderId)
        {
            var digits = "0123456789";
            var random = new byte[3];
            _randomPickupCode.GetBytes(random);

            string newOrderId = orderId.ToString().Substring(2);
            var pickupCode = string.Format("{0}{1}", newOrderId, string.Join("", random.Select(p => digits[p % digits.Length])));
            return pickupCode;
        }

        private long[] GetOrderIdRange(string orderIdStr)
        {
            long orderId;
            if (!string.IsNullOrEmpty(orderIdStr) && long.TryParse(orderIdStr, out orderId))
            {
                var temp = this.GenerateOrderNumber().ToString();
                if (orderIdStr.Length < temp.Length)
                {
                    var len = temp.Length - orderIdStr.Length;
                    orderId = orderId * (long)Math.Pow(10, len);
                    var max = orderId + long.Parse(string.Join("", new int[len].Select(p => 9)));
                    return new[] { orderId, max };
                }
                else if (orderIdStr.Length == temp.Length)
                    return new[] { orderId };
            }

            return null;
        }

        private IQueryable<OrderInfo> ToWhere(OrderQuery query)
        {
            var orders = Context.OrderInfo.AsQueryable();

            var orderIdRange = GetOrderIdRange(query.OrderId);

            if (orderIdRange == null)
            {
                orderIdRange = GetOrderIdRange(query.SearchKeyWords);

                if (orderIdRange == null && !string.IsNullOrWhiteSpace(query.SearchKeyWords))
                    orders = orders.Where(p => p.OrderItemInfo.Any(pp => pp.ProductName.Contains(query.SearchKeyWords)));
            }

            if (orderIdRange != null)
            {
                var min = orderIdRange[0];
                if (orderIdRange.Length == 2)
                {
                    var max = orderIdRange[1];
                    orders = orders.Where(item => item.Id >= min && item.Id <= max);
                }
                else
                    orders = orders.Where(item => item.Id == min);
            }

            if (query.ShopId.HasValue)
            {
                var shopId = query.ShopId.Value;
                orders = orders.Where(p => p.ShopId == shopId);
            }
            if (query.ShopBranchId.HasValue)
            {
                if (query.ShopBranchId.Value == 0)  //查询总店
                    orders = orders.Where(e => e.ShopBranchId == query.ShopBranchId.Value || e.ShopBranchId == null);
                else
                    orders = orders.Where(e => e.ShopBranchId == query.ShopBranchId.Value);
            }
            if (query.AllotStore.HasValue && query.AllotStore.Value != 0)
            {
                if (query.AllotStore.Value == 1) orders = orders.Where(e => e.ShopBranchId.Value > 0);
                else orders = orders.Where(e => e.ShopBranchId.Value <= 0);
            }
            if (!string.IsNullOrWhiteSpace(query.ShopName))
                orders = orders.Where(p => p.ShopName.Contains(query.ShopName));
            if (!string.IsNullOrWhiteSpace(query.UserName))
                orders = orders.Where(p => p.UserName.Contains(query.UserName));
            if (query.UserId.HasValue)
            {
                var userId = query.UserId.Value;
                orders = orders.Where(p => p.UserId == userId);
            }
            if (!string.IsNullOrWhiteSpace(query.PaymentTypeName))
                orders = orders.Where(p => p.PaymentTypeName.Contains(query.PaymentTypeName));
            if (!string.IsNullOrWhiteSpace(query.PaymentTypeGateway))
                //此处会造成 查询微信支付 Himall.Plugin.Payment.WeiXinPay 时会把微信APP支付Himall.Plugin.Payment.WeiXinPay_App 和微信扫码支付 Himall.Plugin.Payment.WeiXinPay_Native查询出来
                // orders = orders.Where(p => p.PaymentTypeGateway.Contains(query.PaymentTypeGateway));  
                orders = orders.Where(p => p.PaymentTypeGateway == query.PaymentTypeGateway);
            if (query.Commented.HasValue)
            {
                var commented = query.Commented.Value;
                if (commented)
                    orders = orders.Where(p => p.OrderCommentInfo.Any());
                else
                    orders = orders.Where(p => p.OrderCommentInfo.Count == 0);
            }

            if (query.OrderType.HasValue)
            {
                // var orderType = (PlatformType)query.OrderType.Value;
                // orders = orders.Where(item => item.Platform == orderType);
                var orderType = (Himall.Model.OrderInfo.OrderTypes)query.OrderType.Value;
                orders = orders.Where(item => item.OrderType == orderType);
            }

            if (query.Status.HasValue)
            {
                var _where = orders.GetDefaultPredicate(false);
                switch (query.Status)
                {
                    case OrderInfo.OrderOperateStatus.UnComment:
                        _where = _where.Or(d => d.OrderCommentInfo.Count == 0 && d.OrderStatus == OrderInfo.OrderOperateStatus.Finish);
                        break;
                    case OrderInfo.OrderOperateStatus.WaitDelivery:
                        //---------------2017-10-17 add by yz ------------
                        _where = _where.Or(d => d.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery);
                        //--------------2017-10-17 update by yz----------------------------
                        //var _ordswhere = orders.GetDefaultPredicate(true);
                        ////处理拼团的情况
                        //_ordswhere = _ordswhere.And(d => d.OrderStatus == query.Status);
                        //var fgordids = Context.FightGroupOrderInfo.Where(d => d.JoinStatus != 4).Select(d => d.OrderId);
                        //_ordswhere = _ordswhere.And(d => !fgordids.Contains(d.Id));
                        //_where = _where.Or(_ordswhere);
                        //--------------2017-10-17 update by yz----------------------------
                        break;
                    default:
                        _where = _where.Or(d => d.OrderStatus == query.Status);
                        break;
                }


                if (query.MoreStatus != null)
                {
                    foreach (var stitem in query.MoreStatus)
                    {
                        _where = _where.Or(d => d.OrderStatus == stitem);
                    }
                }
                orders = orders.FindBy(_where);
            }

            if (query.PaymentType != OrderInfo.PaymentTypes.None)
            {
                orders = orders.Where(item => item.PaymentType == query.PaymentType);
            }

            if (query.IsBuyRecord)//购买记录只查询付了款的
            {
                orders = orders.Where(a => a.PayDate.HasValue);
            }

            //开始结束时间
            if (query.StartDate.HasValue)
            {
                DateTime sdt = query.StartDate.Value;
                orders = orders.Where(d => d.OrderDate >= sdt);
            }
            if (query.EndDate.HasValue)
            {
                DateTime edt = query.EndDate.Value.AddDays(1);
                orders = orders.Where(d => d.OrderDate < edt);
            }

            if ((query.ShopBranchId.HasValue && query.ShopBranchId.Value != 0) || !string.IsNullOrWhiteSpace(query.ShopBranchName))
            {
                //orders = orders.Where(p => p.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake);
                orders = orders.Where(p => p.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake || (p.ShopBranchId.HasValue && p.ShopBranchId.Value > 0));//3.0版本新增订单自动分配到门店，其配送方式不是到店自提但仍属于门店订单

                if (query.ShopBranchId.HasValue)
                {
                    var shopBranchId = query.ShopBranchId.Value;
                    orders = orders.Where(p => p.ShopBranchId == shopBranchId);
                }
                else
                {
                    var sbIds = Context.ShopBranchInfo.Where(p => p.ShopBranchName.Contains(query.ShopBranchName)).Select(p => p.Id);
                    orders = orders.Where(p => sbIds.Contains(p.ShopBranchId.Value));
                }
            }

            if (!string.IsNullOrEmpty(query.UserContact))
                orders = orders.Where(p => p.CellPhone.StartsWith(query.UserContact));
            return orders;
        }
        #endregion

        #region 内部类
        public class OrderSkuInfo
        {

            public SKUInfo SKU { get; set; }

            public ProductInfo Product { get; set; }

            public int Quantity { get; set; }

            public long ColloPid { get; set; }
        }
        #endregion
        #region 分配门店
        /// <summary>
        /// 商家订单分配门店时更新商家、门店库存(单个订单)
        /// </summary>
        /// <param name="skuIds"></param>
        /// <param name="quantity"></param>
        public void DistributionStoreUpdateStock(List<string> skuIds, List<int> counts, long shopBranchId)
        {
            if (skuIds.Count > 0)
            {
                using (TransactionScope tran = new TransactionScope())
                {
                    int quantity = 0; string skuId = string.Empty;
                    for (int i = 0; i < skuIds.Count(); i++)
                    {
                        skuId = skuIds[i];
                        quantity = counts.ElementAt(i);
                        SKUInfo sku = Context.SKUInfo.FindById(skuId);
                        if (sku != null)
                            sku.Stock += quantity;

                        ShopBranchSkusInfo sbSku = Context.ShopBranchSkusInfo.FirstOrDefault(e => e.ShopBranchId == shopBranchId && e.SkuId == sku.Id);
                        if (sbSku != null)
                            sbSku.Stock -= quantity;

                        if (sbSku.Stock < 0)
                            throw new HimallException("门店商品库存不足");
                    }
                    Context.SaveChanges();
                    //提交事务
                    tran.Complete();
                }
            }
        }

        /// <summary>
        /// 更改旧门店订单到新门店(单个订单)
        /// </summary>
        /// <param name="stuIds"></param>
        /// <param name="newShopBranchId"></param>
        /// <param name="oldShopBranchId"></param>
        public void DistributionStoreUpdateStockToNewShopBranch(List<string> skuIds, List<int> counts, long newShopBranchId, long oldShopBranchId)
        {
            using (TransactionScope tran = new TransactionScope())
            {
                int quantity = 0; string skuId = string.Empty;
                for (int i = 0; i < skuIds.Count(); i++)
                {
                    skuId = skuIds[i];
                    quantity = counts.ElementAt(i);
                    ShopBranchSkusInfo sbSkuNew = Context.ShopBranchSkusInfo.FirstOrDefault(e => e.ShopBranchId == newShopBranchId && e.SkuId == skuId);
                    if (sbSkuNew != null)
                        sbSkuNew.Stock -= quantity;
                    ShopBranchSkusInfo sbSkuOld = Context.ShopBranchSkusInfo.FirstOrDefault(e => e.ShopBranchId == oldShopBranchId && e.SkuId == skuId);
                    if (sbSkuOld != null)
                        sbSkuOld.Stock += quantity;
                    if (sbSkuNew.Stock < 0)
                        throw new HimallException("门店商品库存不足");
                }
                Context.SaveChanges();
                //提交事务
                tran.Complete();
            }
        }

        /// <summary>
        /// 更改门店订单回到商家(单个订单)
        /// </summary>
        public void DistributionStoreUpdateStockToShop(List<string> skuIds, List<int> counts, long shopBranchId)
        {
            using (TransactionScope tran = new TransactionScope())
            {
                int quantity = 0; string skuId = string.Empty;
                for (int i = 0; i < skuIds.Count(); i++)
                {
                    skuId = skuIds[i];
                    quantity = counts.ElementAt(i);
                    SKUInfo sku = Context.SKUInfo.FindById(skuId);
                    if (sku != null)
                        sku.Stock -= quantity;

                    ShopBranchSkusInfo sbSku = Context.ShopBranchSkusInfo.FirstOrDefault(e => e.ShopBranchId == shopBranchId && e.SkuId == skuId);
                    if (sbSku != null)
                        sbSku.Stock += quantity;

                    if (sku.Stock < 0)
                        throw new HimallException("商品库存不足");
                }

                Context.SaveChanges();
                //提交事务
                tran.Complete();
            }
        }
        /// <summary>
        /// 更新订单所属门店
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shopBranchId"></param>
        public void UpdateOrderShopBranch(long orderId, long shopBranchId)
        {
            var orderdata = Context.OrderInfo.FirstOrDefault(d => d.Id == orderId);
            if (orderdata == null)
            {
                throw new HimallException("错误的订单编号");
            }
            orderdata.ShopBranchId = shopBranchId;
            Context.SaveChanges();
        }
        #endregion
        #region 门店/商家APP发货消息推送
        public void SendAppMessage(OrderInfo orderInfo)
        {
            var app = new AppMessagesInfo()
            {
                Content = string.Format("{0} 等待您发货", orderInfo.Id),
                IsRead = false,
                sendtime = DateTime.Now,
                SourceId = orderInfo.Id,
                Title = "您有新的订单",
                TypeId = (int)AppMessagesType.Order,
                OrderPayDate = Core.Helper.TypeHelper.ObjectToDateTime(orderInfo.PayDate),
                ShopId = 0,
                ShopBranchId = 0
            };
            if (orderInfo.ShopBranchId.HasValue && orderInfo.ShopBranchId.Value > 0)
            {
                app.ShopBranchId = orderInfo.ShopBranchId.Value;
            }
            else app.ShopId = orderInfo.ShopId;

            if (orderInfo.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake)
            {
                app.Title = "您有新自提订单";
                app.TypeId = (int)AppMessagesType.Order;
                app.Content = string.Format("{0} 等待您备货", orderInfo.Id);
            }
            _iAppMessageService.AddAppMessages(app);
        }
        #endregion
    }
}