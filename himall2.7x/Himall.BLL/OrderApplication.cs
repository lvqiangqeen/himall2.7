using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices;
using Himall.Model;
using Himall.Core;
using Himall.DTO;
using Himall.Core.Plugins.Payment;

namespace Himall.Application
{
    public class OrderApplication
    {
        private static IOrderService _iOrderService = ObjectContainer.Current.Resolve<IOrderService>();
        private static IMemberIntegralService _iMemberIntegralService = ObjectContainer.Current.Resolve<IMemberIntegralService>();
        private static ICartService _iCartService = ObjectContainer.Current.Resolve<ICartService>();
        private static IMemberService _iMemberService = ObjectContainer.Current.Resolve<IMemberService>();
        private static IProductService _iProductService = ObjectContainer.Current.Resolve<IProductService>();
        private static IPaymentConfigService _iPaymentConfigService = ObjectContainer.Current.Resolve<IPaymentConfigService>();
        private static IShippingAddressService _iShippingAddressService = ObjectContainer.Current.Resolve<IShippingAddressService>();
        private static IRegionService _iRegionService = ObjectContainer.Current.Resolve<IRegionService>();
        private static ICashDepositsService _iCashDepositsService = ObjectContainer.Current.Resolve<ICashDepositsService>();
        private static ISiteSettingService _iSiteSettingService = ObjectContainer.Current.Resolve<ISiteSettingService>();
        private static IShopService _iShopService = ObjectContainer.Current.Resolve<IShopService>();
        private static ILimitTimeBuyService _iLimitTimeBuyService = ObjectContainer.Current.Resolve<ILimitTimeBuyService>();
        private static ICouponService _iCouponService = ObjectContainer.Current.Resolve<ICouponService>();
        private static IShopBonusService _iShopBonusService = ObjectContainer.Current.Resolve<IShopBonusService>();
        private static ICollocationService _iCollocationService = ObjectContainer.Current.Resolve<ICollocationService>();
        private static IMemberCapitalService _iMemberCapitalService = ObjectContainer.Current.Resolve<IMemberCapitalService>();
        private static IVShopService _iVShopService = ObjectContainer.Current.Resolve<IVShopService>();
        private static IRefundService _iRefundService = ObjectContainer.Current.Resolve<IRefundService>();
        private static IFightGroupService _iFightGroupService = ObjectContainer.Current.Resolve<IFightGroupService>();

        #region web公共方法
        /// <summary>
        /// 从购物车中提交订单
        /// </summary>
        /// <param name="userid">用户ID</param>
        /// <param name="promoterIds">分销销售员ID集合</param>
        /// <param name="cartItemIds">购物车商品ID集合</param>
        /// <param name="recieveAddressId">客户收货区域ID</param>
        /// <param name="couponIds">商品对应的优惠券ID集合</param>
        /// <param name="invoiceType">发票类型0不要发票1增值税发票2普通发票</param>
        /// <param name="invoiceTitle">发票抬头</param>
        /// <param name="invoiceContext">发票内容</param>
        /// <param name="integral">使用积分</param>
        /// <param name="isCashOnDelivery">是否货到付款</param>
        /// <param name="PlatformType">订单来源平台</param>
        /// <returns>返回订单列表和调转地址</returns>
        public static OrderReturnModel SubmitOrder(long userid, List<long> promoterIds, string cartItemIds, long recieveAddressId,
            string couponIds, int invoiceType, string invoiceTitle,
            string invoiceContext, int integral = 0, bool isCashOnDelivery = false, string orderRemarks = "", PlatformType PlatformType = PlatformType.PC)
        {
            var orderService = _iOrderService;
            long[] orderIds;
            IEnumerable<long> cartItemArr;
            if (string.IsNullOrWhiteSpace(cartItemIds))
            {
                var cartInfo = _iCartService.GetCart(userid);
                cartItemArr = cartInfo.Items.Select(item => item.Id);
            }
            else
            {
                cartItemArr = cartItemIds.Split(',').Select(item => long.Parse(item));
            }
            if (integral < 0)
            {
                throw new HimallException("兑换积分数量不正确");
            }
            OrderCreateModel model = new OrderCreateModel();
            model.CartItemIds = cartItemArr;
            model.CouponIdsStr = ConvertUsedCoupon(couponIds);
            model.CurrentUser = _iMemberService.GetMember(userid);
            model.Integral = integral;
            model.Invoice = (InvoiceType)invoiceType;
            model.InvoiceTitle = invoiceTitle;
            model.IsCashOnDelivery = isCashOnDelivery;
            model.InvoiceContext = invoiceContext;
            model.ReceiveAddressId = recieveAddressId;
            model.OrderRemarks = orderRemarks.Split(',');
            model.PlatformType = PlatformType;

            //更新所属分销员
            _iMemberService.UpdateDistributionUserLink(promoterIds, userid);
            var orders = orderService.CreateOrder(model);
            var redirect = false;
            if (orders.Sum(a => a.OrderTotalAmount) == 0)
            {
                redirect = true;
            }
            orderIds = orders.Select(item => item.Id).ToArray();
            OrderReturnModel vm = new OrderReturnModel();
            vm.Ids = orderIds;
            vm.Redirect = redirect;
            return vm;
        }

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
        public static OrderReturnModel2 SubmitOrderByProductId(
            long userid,
            List<long> ids,
            string skuIds,
            string counts,
            long recieveAddressId,
            string couponIds,
            int invoiceType,
            string invoiceTitle,
            string invoiceContext,
            int integral = 0,
            string collpIds = "",
            bool isCashOnDelivery = false,
            string orderRemarks = "", PlatformType PlatformType = PlatformType.PC)
        {
            var collIds = collpIds;
            var skuIdsArr = skuIds.Split(',');
            var pCountsArr = counts.TrimEnd(',').Split(',').Select(t => int.Parse(t));
            var productService = _iProductService;
            var orderService = _iOrderService;
            IEnumerable<long> orderIds;
            if (integral < 0)
            {
                throw new HimallException("兑换积分数量不正确");
            }
            IEnumerable<long> collocationPidArr = null;
            if (!string.IsNullOrEmpty(collIds))
            {
                collocationPidArr = collIds.Split(',').Select(item => long.Parse(item));
            }
            if (string.IsNullOrWhiteSpace(skuIds) || string.IsNullOrWhiteSpace(counts))
                throw new Himall.Core.HimallException("创建订单的时候，SKU为空，或者数量为0");
            OrderCreateModel model = new OrderCreateModel();
            model.SkuIds = skuIdsArr;
            model.Counts = pCountsArr;
            model.CurrentUser = _iMemberService.GetMember(userid);
            model.Integral = integral;
            model.IsCashOnDelivery = isCashOnDelivery;
            model.OrderRemarks = orderRemarks.Split(',');
            model.CouponIdsStr = ConvertUsedCoupon(couponIds);
            model.Invoice = (InvoiceType)invoiceType;
            model.InvoiceTitle = invoiceTitle;
            model.InvoiceContext = invoiceContext;
            model.CollPids = collocationPidArr;
            model.ReceiveAddressId = recieveAddressId;
            model.PlatformType = PlatformType;

            //更新所属分销员
            _iMemberService.UpdateDistributionUserLink(ids, userid);
            var orders = orderService.CreateOrder(model);
            orderIds = orders.Select(item => item.Id).ToArray();
            decimal orderTotals = orders.Sum(item => item.OrderTotalAmount);
            OrderReturnModel2 result = new OrderReturnModel2();
            result.success = true;
            result.orderIds = orderIds;
            result.orderTotal = orderTotals;
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
            submitModel.Member = _iMemberService.GetMember(userid);
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
                bool hasRegion = _iPaymentConfigService.IsCashOnDelivery(address.RegionId);
                var isEnable = _iPaymentConfigService.IsEnable();
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
            submitModel.IsLimitBuy = false;
            //发票信息
            submitModel.InvoiceTitle = _iOrderService.GetInvoiceTitles(userid);
            submitModel.InvoiceContext = _iOrderService.GetInvoiceContexts();
            return submitModel;
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
            GetOrderProductsInfo(submitModel, skuIds, counts, userid, collpids);

            //获取收货地址
            var address = GetShippingAddress(regionId, userid);

            submitModel.address = address;
            if (address != null)
            {
                bool hasRegion = _iPaymentConfigService.IsCashOnDelivery(address.RegionId);
                var isEnable = _iPaymentConfigService.IsEnable();
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
                var gpobj = _iFightGroupService.GetGroup(GroupActionId, GroupId.Value);
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
            string paypwd = _iMemberService.GetMember(userid).PayPwd;
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
            var addresses = _iShippingAddressService.GetUserShippingAddressByUserId(userid).ToArray();
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
        /// <param name="datas"></param>
        /// <returns></returns>
        public static List<object[]> CalcFreight(long addressId, string datas)
        {
            List<object[]> list = new List<object[]>();
            if (string.IsNullOrEmpty(datas))
            {
                return list;
            }
            string[] data = datas.Split('|');
            for (int i = 0; i < data.Length; i++)
            {
                List<CartItemModel> cartItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CartItemModel>>(data[i]);
                List<string> skuIds = new List<string>();
                List<int> counts = new List<int>();
                decimal amount = 0;
                foreach (var item in cartItems)
                {
                    string skuid = item.skuId;
                    if (skuIds.Contains(skuid))
                    {
                        int idx = skuIds.IndexOf(skuid);
                        int c = counts[idx];
                        counts[idx] = c + item.count;
                    }
                    else
                    {
                        skuIds.Add(skuid);
                        counts.Add(item.count);
                    }
                    amount += item.count * item.price;
                }
                ShippingAddressInfo address = _iShippingAddressService.GetUserShippingAddress(addressId);
                int cityId = 0;
                if (address != null)
                {
                    cityId = _iRegionService.GetCityId(address.RegionIdPath);
                }
                decimal freight = _iProductService.GetFreight(skuIds, counts, cityId);

                object[] obj = new object[] { amount, freight };
                list.Add(obj);
            }
            return list;
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
            var member = _iMemberCapitalService.GetMemberInfoByPayPwd(userid, pwd);
            if (member == null)
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

            decimal total = orders.Sum(a => a.OrderTotalAmount);
            if (total == 0)
            {
                throw new HimallException("错误的订单总价");
            }

            foreach (var item in orders)
            {
                if (item.OrderType == OrderInfo.OrderTypes.FightGroup)
                {
                    if (!_iFightGroupService.OrderCanPay(item.Id))
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
                return result;
            }
            var orderIdArr = orderIds.Split(',').Select(item => long.Parse(item));
            var payDone = _iOrderService.GetOrders(orderIdArr).Where(p => p.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay || p.UserId == userid).Count();
            if (payDone <= 0)//订单已经支付，则跳转至订单页面
            {
                result.IsSuccess = false;
                return result;
            }
            else
            {

                //获取待支付的所有订单
                var orderser = _iOrderService;
                var orders = orderser.GetOrders(orderIdArr).Where(item => item.OrderStatus == Model.OrderInfo.OrderOperateStatus.WaitPay && item.UserId == userid).ToList();

                foreach (var item in orders)
                {
                    if (item.OrderType == OrderInfo.OrderTypes.FightGroup)
                    {
                        if (!_iFightGroupService.OrderCanPay(item.Id))
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
                var capital = _iMemberCapitalService.GetCapitalInfo(userid);
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
            var model = _iMemberCapitalService.GetChargeDetail(long.Parse(orderIds));
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
                viewmodel.UnpaidTimeout = _iSiteSettingService.GetSiteSettings().UnpaidTimeout;
                viewmodel.models = models.ToList();
                //return View(viewmodel);
                return viewmodel;
            }
        }

        /// <summary>
        /// 获得限时购订单提交数据对像
        /// </summary>
        /// <returns></returns>
        public static OrderCreateModel GetLimitOrder(
            long userid,
            string skuIds,
            string counts,
            long recieveAddressId,
            string couponIds,
            int invoiceType,
            string invoiceTitle,
            string invoiceContext,
            int integral = 0,
            string collpIds = "",
            bool isCashOnDelivery = false,
            string orderRemarks = ""
            )
        {
            var collIds = collpIds;
            var skuIdsArr = skuIds.Split(',');
            var pCountsArr = counts.TrimEnd(',').Split(',').Select(t => int.Parse(t));
            var productService = _iProductService;
            var orderService = _iOrderService;
            if (integral < 0)
            {
                throw new HimallException("兑换积分数量不正确");
            }
            IEnumerable<long> collocationPidArr = null;
            if (!string.IsNullOrEmpty(collIds))
            {
                collocationPidArr = collIds.Split(',').Select(item => long.Parse(item));
            }
            if (string.IsNullOrWhiteSpace(skuIds) || string.IsNullOrWhiteSpace(counts))
                throw new Himall.Core.HimallException("创建订单的时候，SKU为空，或者数量为0");
            if (userid <= 0)
                throw new InvalidPropertyException("会员Id无效");
            OrderCreateModel model = new OrderCreateModel();
            model.SkuIds = skuIdsArr;
            model.Counts = pCountsArr;
            model.CurrentUser = _iMemberService.GetUserByCache(userid);
            model.Integral = integral;
            model.IsCashOnDelivery = isCashOnDelivery;
            model.OrderRemarks = orderRemarks.Split(',');
            model.CouponIdsStr = ConvertUsedCoupon(couponIds);
            model.Invoice = (InvoiceType)invoiceType;
            model.InvoiceTitle = invoiceTitle;
            model.InvoiceContext = invoiceContext;
            model.CollPids = collocationPidArr;
            model.ReceiveAddressId = recieveAddressId;
            model.IslimitBuy = true;
            if (model.Counts.Count() == 0)
                throw new InvalidPropertyException("待提交订单的商品数量不能这空");
            else if (model.Counts.Count(item => item <= 0) > 0)
                throw new InvalidPropertyException("待提交订单的商品数量必须都大于0");
            else if (model.SkuIds.Count() != model.Counts.Count())
                throw new InvalidPropertyException("商品数量不一致");
            else if (recieveAddressId <= 0)
                throw new InvalidPropertyException("收货地址无效");
            else
                return model;
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
            string orderRemarks = ""
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
            model.CurrentUser = _iMemberService.GetMember(userid);
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
                var gpobj = _iFightGroupService.GetGroup(activeId, groupId);
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

        /// <summary>
        /// 更新用户关系
        /// </summary>
        /// <param name="promotionids"></param>
        /// <param name="userid"></param>
        public static void UpdateDistributionUserLink(long[] promotionids, long userid)
        {
            _iMemberService.UpdateDistributionUserLink(promotionids, userid);
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
                var vshop = _iVShopService.GetVShopByShopId(item);
                if (vshop != null)
                {
                    vshopIds.Add(vshop.Id);
                }
            }
            foreach (var vshopId in vshopIds)
            {
                _iVShopService.AddBuyNumber(vshopId);
            }
        }

        /// <summary>
        /// 根据用户获收获地址列表
        /// </summary>
        /// <param name="userid">用户id</param>
        /// <returns>收获地址列表</returns>
        public static List<ShippingAddressInfo> GetUserAddresses(long userid)
        {
            return _iShippingAddressService.GetUserShippingAddressByUserId(userid).ToList();
        }

        /// <summary>
        /// 设置用户默认收货地址
        /// </summary>
        /// <param name="addrId">地址Id</param>
        /// <param name="userid">用户Id</param>
        public static void SetDefaultUserShippingAddress(long addrId, long userid)
        {
            _iShippingAddressService.SetDefaultShippingAddress(addrId, userid);
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
                ShipngInfo = _iShippingAddressService.GetUserShippingAddress(addressId);
            }
            return ShipngInfo;
        }

        /// <summary>
        /// 删除指定的收获地址信息
        /// </summary>
        /// <param name="addressId">收获地址Id</param>
        public static void DeleteShippingAddress(long addressId, long userid)
        {
            _iShippingAddressService.DeleteShippingAddress(addressId, userid);
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
                foreach (var item in order.OrderItemInfo)
                {
                    _iProductService.UpdateStock(item.SkuId, item.Quantity);
                }
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
        public static void ConfirmOrder(long orderId, string username)
        {
            _iOrderService.MembeConfirmOrder(orderId, username);
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
            var shopinfo = _iShopService.GetShop(order.ShopId);
            var vshop = shopinfo.Himall_VShop.FirstOrDefault() ?? new VShopInfo() { Id = 0 };
            bool isCanApply = false;
            bool IsRefundTimeOut = false;
            var _ordrefobj = _iRefundService.GetOrderRefundByOrderId(id) ?? new OrderRefundInfo { Id = 0 };
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
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
                    if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery)
                    {
                        isCanApply = _iRefundService.CanApplyRefund(id, item.Id);
                    }
                    else
                    {
                        isCanApply = _iRefundService.CanApplyRefund(id, item.Id, false);
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
                        ProductImage = productinfo.GetImage(ProductInfo.ImageSize.Size_100),
                        Id = item.Id,
                        Unit = productinfo.MeasureUnit,
                        IsCanRefund = isCanApply,
                        Color = item.Color,
                        Size = item.Size,
                        Version = item.Version,
                        RefundStats = itemrefstate,
                        OrderRefundId = (itemrefund == null ? 0 : itemrefund.Id),
                    };
                })
            };
            OrderDetailView view = new OrderDetailView();
            IsRefundTimeOut = _iOrderService.IsRefundTimeOut(id);
            view.Detail = orderDetail;
            view.Bonus = null;
            if (type == Core.PlatformType.WeiXin)
            {
                var bonusmodel = _iShopBonusService.GetGrantByUserOrder(id, userid);
                if (bonusmodel != null)
                {
                    view.Bonus = bonusmodel;
                    view.ShareHref = "http://" + host + "/m-weixin/shopbonus/index/" + _iShopBonusService.GetGrantIdByOrderId(id);
                }
            }
            view.Order = order;

            view.FightGroupCanRefund = true;
            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)  //拼团状态补充
            {
                var fgord = _iFightGroupService.GetFightGroupOrderStatusByOrderId(order.Id);
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
            var address = _iShippingAddressService.GetDefaultUserShippingAddressByUserId(userid);
            if (address != null)
            {
                cityId = _iRegionService.GetCityId(address.RegionIdPath);
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
                if (cityId > 0)
                {
                    item.Freight = _iProductService.GetFreight(productIds, productCounts, cityId);
                }
                item.OneCoupons = GetDefaultCoupon(item.shopId, userid, shopcartItem.Sum(a => a.price * a.count));
                decimal ordPaidPrice = CalculatePaidPrice(item);
                var shop = _iShopService.GetShop(item.shopId);
                item.ShopName = shop.ShopName;
                //满额免
                SetFullFree(ordPaidPrice, shop.FreeFreight, item);
                item.VshopId = shop.Himall_VShop.FirstOrDefault() != null ? shop.Himall_VShop.FirstOrDefault().Id : 0;
                list.Add(item);
            }
            var totalUserCoupons = list.Where(a => a.OneCoupons != null).Sum(b => b.OneCoupons.BasePrice);
            confirmModel.products = list;
            confirmModel.totalAmount = products.Sum(item => item.price * item.count);
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
            var address = _iShippingAddressService.GetDefaultUserShippingAddressByUserId(userid);
            if (address != null)
            {
                cityId = _iRegionService.GetCityId(address.RegionIdPath);
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

                    cartitemmodel.price = GetSalePrice(cartitemmodel.id, sku, collpid, skuIds.Count());
                }
                if (cityId > 0)
                {
                    item.Freight = _iProductService.GetFreight(productIds, productCounts, cityId);
                }
                item.OneCoupons = GetDefaultCoupon(item.shopId, userid, shopcartItem.Sum(a => a.price * a.count));
                decimal ordPaidPrice = CalculatePaidPrice(item);
                var shop = _iShopService.GetShop(item.shopId);
                item.ShopName = shop.ShopName;

                //满额免
                SetFullFree(ordPaidPrice, shop.FreeFreight, item);
                item.VshopId = shop.Himall_VShop.FirstOrDefault() != null ? shop.Himall_VShop.FirstOrDefault().Id : 0;
                list.Add(item);
            }
            var totalUserCoupons = list.Where(a => a.OneCoupons != null).Sum(b => b.OneCoupons.BasePrice);
            confirmModel.products = list;
            confirmModel.totalAmount = products.Sum(item => item.price * item.count);
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
            var address = _iShippingAddressService.GetDefaultUserShippingAddressByUserId(userid);
            if (address != null)
            {
                cityId = _iRegionService.GetCityId(address.RegionIdPath);
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
                if (cityId > 0)
                {
                    item.Freight = _iProductService.GetFreight(productIds, productCounts, cityId);
                }
                item.OneCoupons = null; //不可以使用优惠券
                decimal ordPaidPrice = CalculatePaidPrice(item);
                var shop = _iShopService.GetShop(item.shopId);
                item.ShopName = shop.ShopName;
                //满额免
                SetFullFree(ordPaidPrice, shop.FreeFreight, item);
                item.VshopId = shop.Himall_VShop.FirstOrDefault() != null ? shop.Himall_VShop.FirstOrDefault().Id : 0;
                list.Add(item);
            }
            var totalUserCoupons = 0; //不可以使用优惠券
            confirmModel.products = list;
            confirmModel.totalAmount = products.Sum(item => item.price * item.count);
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
                cartItems = _iCartService.GetCartItems(cartItemIdsArr);
            }
            int cityId = 0;
            var address = _iShippingAddressService.GetDefaultUserShippingAddressByUserId(userid);
            if (address != null)
            {
                cityId = ServiceProvider.Instance<IRegionService>.Create.GetCityId(address.RegionIdPath);
            }
            var products = GenerateCartItem(cartItems);
            var shopList = products.GroupBy(a => a.shopId);
            List<MobileShopCartItemModel> list = new List<MobileShopCartItemModel>();
            foreach (var shopcartItem in shopList)
            {
                IEnumerable<long> productIds = shopcartItem.Select(r => r.id);
                IEnumerable<int> productCounts = shopcartItem.Select(r => r.count);
                MobileShopCartItemModel item = new MobileShopCartItemModel();
                item.shopId = shopcartItem.Key;
                if (_iVShopService.GetVShopByShopId(item.shopId) == null)
                    item.VshopId = 0;
                else
                    item.VshopId = _iVShopService.GetVShopByShopId(item.shopId).Id;
                item.CartItemModels = products.Where(a => a.shopId == item.shopId).ToList();

                if (cityId > 0)
                {
                    item.Freight = _iProductService.GetFreight(productIds, productCounts, cityId);
                }

                item.OneCoupons = GetDefaultCoupon(item.shopId, userid, shopcartItem.Sum(a => a.price * a.count));
                decimal ordPaidPrice = CalculatePaidPrice(item);
                var shop = _iShopService.GetShop(item.shopId);
                item.ShopName = shop.ShopName;
                //满额免
                SetFullFree(ordPaidPrice, shop.FreeFreight, item);

                list.Add(item);
            }
            var totalUserCoupons = list.Where(a => a.OneCoupons != null).Sum(b => b.OneCoupons.BasePrice);


            confirmModel.products = list;
            confirmModel.totalAmount = products.Sum(item => item.price * item.count);
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
            var address = _iShippingAddressService.GetDefaultUserShippingAddressByUserId(userid);
            if (address != null)
            {
                bool hasRegion = _iPaymentConfigService.IsCashOnDelivery(address.RegionId);
                var isEnable = _iPaymentConfigService.IsEnable();
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
            var shopBonus = _iShopBonusService.GetDetailToUse(shopid, userid, totalPrice);
            var userCoupons = _iCouponService.GetUserCoupon(shopid, userid, totalPrice);

            if (shopBonus.Count() > 0 && userCoupons.Count() > 0)
            {
                var sb = shopBonus.FirstOrDefault();      //商家红包
                var uc = userCoupons.FirstOrDefault();  //优惠卷
                if (sb.BasePrice > uc.BasePrice)
                {
                    BaseCoupon c = new BaseCoupon();
                    c.BaseEndTime = sb.BaseEndTime;
                    c.BaseId = sb.BaseId;
                    c.BaseName = sb.BaseName;
                    c.BasePrice = sb.BasePrice;
                    c.BaseShopId = sb.BaseShopId;
                    c.BaseShopName = sb.BaseShopName;
                    c.BaseType = sb.BaseType.GetHashCode() == 0 ? Himall.DTO.CouponType.Coupon : Himall.DTO.CouponType.ShopBonus;
                    c.OrderAmount = sb.Himall_ShopBonusGrant.Himall_ShopBonus.UsrStatePrice;
                    return c;
                }
                else
                {
                    BaseCoupon c = new BaseCoupon();
                    c.BaseEndTime = uc.BaseEndTime;
                    c.BaseId = uc.BaseId;
                    c.BaseName = uc.BaseName;
                    c.BasePrice = uc.BasePrice;
                    c.BaseShopId = uc.BaseShopId;
                    c.BaseShopName = uc.BaseShopName;
                    c.BaseType = uc.BaseType.GetHashCode() == 0 ? Himall.DTO.CouponType.Coupon : Himall.DTO.CouponType.ShopBonus;
                    c.OrderAmount = uc.Himall_Coupon.OrderAmount;
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
                BaseCoupon c = new BaseCoupon();
                c.BaseEndTime = coupon.BaseEndTime;
                c.BaseId = coupon.BaseId;
                c.BaseName = coupon.BaseName;
                c.BasePrice = coupon.BasePrice;
                c.BaseShopId = coupon.BaseShopId;
                c.BaseShopName = coupon.BaseShopName;
                c.BaseType = coupon.BaseType.GetHashCode() == 0 ? Himall.DTO.CouponType.Coupon : Himall.DTO.CouponType.ShopBonus;
                c.OrderAmount = coupon.Himall_Coupon.OrderAmount;
                return c;
            }
            else if (shopBonus.Count() > 0 && userCoupons.Count() <= 0)
            {
                var coupon = shopBonus.FirstOrDefault();
                BaseCoupon c = new BaseCoupon();
                c.BaseEndTime = coupon.BaseEndTime;
                c.BaseId = coupon.BaseId;
                c.BaseName = coupon.BaseName;
                c.BasePrice = coupon.BasePrice;
                c.BaseShopId = coupon.BaseShopId;
                c.BaseShopName = coupon.BaseShopName;
                c.BaseType = coupon.BaseType.GetHashCode() == 0 ? Himall.DTO.CouponType.Coupon : Himall.DTO.CouponType.ShopBonus;
                c.OrderAmount = coupon.Himall_ShopBonusGrant.Himall_ShopBonus.UsrStatePrice;
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
                    result.IntegralPerMoney = Math.Round(totalAmount - totalUserCoupons, 2);
                    result.UserIntegrals = Math.Round((totalAmount - totalUserCoupons) * integralPerMoney.IntegralPerMoney);
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
            decimal ordTotalPrice = cart.CartItemModels.Sum(c => c.price * c.count);
            decimal ordDisPrice = cart.OneCoupons == null ? 0 : cart.OneCoupons.BasePrice;
            return ordTotalPrice - ordDisPrice;
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
                address = _iShippingAddressService.GetUserShippingAddress((long)regionId);
            }
            else
            {
                address = _iShippingAddressService.GetDefaultUserShippingAddressByUserId(userid);
            }
            int cityId = 0;
            if (address != null)
            {
                cityId = _iRegionService.GetCityId(address.RegionIdPath);
            }

            IEnumerable<ShoppingCartItem> cartItems = null;
            if (string.IsNullOrWhiteSpace(cartItemIds))
                cartItems = GetCart(userid, cartInfo).Items;
            else
            {
                var cartItemIdsArr = cartItemIds.Split(',').Select(t => long.Parse(t));
                cartItems = _iCartService.GetCartItems(cartItemIdsArr);
            }

            var products = GenerateCartItem(cartItems);
            var shopList = products.GroupBy(a => a.shopId);
            List<ShopCartItemModel> list = new List<ShopCartItemModel>();


            var regionService = _iRegionService;
            var orderService = _iOrderService;
            //
            var cashDepositService = _iCashDepositsService;

            foreach (var shopcartItem in shopList)
            {
                IEnumerable<long> productIds = shopcartItem.Select(r => r.id);
                IEnumerable<int> counts = shopcartItem.Select(r => r.count);

                ShopCartItemModel item = new ShopCartItemModel();
                item.shopId = shopcartItem.Key;
                item.CartItemModels = products.Where(a => a.shopId == item.shopId).ToList();
                var shop = _iShopService.GetShop(item.shopId);
                item.ShopName = shop.ShopName;
                item.FreeFreight = shop.FreeFreight;
                if (cityId > 0)
                {
                    item.Freight = _iProductService.GetFreight(productIds, counts, cityId);
                }
                item.BaseCoupons = GetBaseCoupon(item.shopId, userid, shopcartItem.Sum(a => a.price * a.count));

                var OrderItems = GetOrderItems(item);

                item.freightProductGroup = OrderItems.OrderBy(e => e.FreightTemplateId).ToList();
                list.Add(item);
            }
            model.products = list;
            model.totalAmount = products.Sum(item => item.price * item.count);
            model.Freight = list.Sum(a => a.Freight);
        }

        static void GetOrderProductsInfo(OrderSubmitModel model, string skuIds, string counts, long userid, string collIds = null)
        {
            var address = _iShippingAddressService.GetDefaultUserShippingAddressByUserId(userid);
            int cityId = 0;
            if (address != null)
            {
                cityId = _iRegionService.GetCityId(address.RegionIdPath);
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
                return new CartItemModel()
                {
                    skuId = item,
                    id = sku.ProductInfo.Id,
                    imgUrl = sku.ProductInfo.GetImage(ProductInfo.ImageSize.Size_50),
                    name = sku.ProductInfo.ProductName,
                    shopId = sku.ProductInfo.ShopId,
                    price = GetSalePrice(sku.ProductInfo.Id, sku, collpid, skuCount),
                    count = count,
                    productCode = sku.ProductInfo.ProductCode
                };
            }).ToList();
            IShopService shopService = _iShopService;
            var cashDepositService = _iCashDepositsService;
            var shopList = products.GroupBy(a => a.shopId);
            List<ShopCartItemModel> list = new List<ShopCartItemModel>();
            foreach (var shopcartItem in shopList)
            {
                IEnumerable<long> productIds = shopcartItem.Select(r => r.id);
                IEnumerable<int> productCounts = shopcartItem.Select(r => r.count);

                ShopCartItemModel item = new ShopCartItemModel();
                item.shopId = shopcartItem.Key;
                item.CartItemModels = products.Where(a => a.shopId == item.shopId).ToList();
                var shop = shopService.GetShop(item.shopId);
                item.ShopName = shop.ShopName;
                item.FreeFreight = shop.FreeFreight;
                if (cityId > 0)
                {
                    item.Freight = _iProductService.GetFreight(productIds, productCounts, cityId);
                }
                item.BaseCoupons = GetBaseCoupon(item.shopId, userid, shopcartItem.Sum(a => a.price * a.count));

                var OrderItems = GetOrderItems(item);

                item.freightProductGroup = OrderItems.OrderBy(e => e.FreightTemplateId).ToList();
                list.Add(item);
            }
            model.products = list;
            model.totalAmount = products.Sum(item => item.price * item.count);
            model.Freight = list.Sum(a => a.Freight);
        }

        static decimal GetSalePrice(long productId, SKUInfo sku, long? collid, int Count)
        {
            var price = sku.SalePrice;
            if (collid.HasValue && collid.Value != 0 && Count > 1)//组合购大于一个商品
            {
                var collsku = _iCollocationService.GetColloSku(collid.Value, sku.Id);
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
            var cashDepositService = _iCashDepositsService;
            var OrderItems = item.CartItemModels.Select(r =>
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
                var cashDeposit = cashDepositService.GetCashDepositsObligation(skuinfo.ProductId);
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
                    skuVersion = skuinfo.Version
                };
            }
               );

            return OrderItems;
        }

        static ShippingAddressInfo GetShippingAddress(long? regionId, long userid)
        {
            if (regionId != null)
            {
                return _iShippingAddressService.GetUserShippingAddress((long)regionId);
            }
            else
                return _iShippingAddressService.GetDefaultUserShippingAddressByUserId(userid);
        }

        /// <summary>
        /// 订单提交页面，需要展示的数据
        /// </summary>
      static  List<CartItemModel> GenerateCartItem(IEnumerable<ShoppingCartItem> cartItems)
        {
            var productService = _iProductService;
            var products = cartItems.Select(item =>
            {
                var product = productService.GetProduct(item.ProductId);
                var sku = productService.GetSku(item.SkuId);
                return new CartItemModel()
                {
                    skuId = item.SkuId,
                    id = product.Id,
                    imgUrl = product.GetImage(ProductInfo.ImageSize.Size_100),
                    name = product.ProductName,
                    price = sku.SalePrice,
                    shopId = product.ShopId,
                    count = item.Quantity,
                    productCode = product.ProductCode,
                    color = sku.Color,
                    size = sku.Size,
                    version = sku.Version,
                    IsSelf = product.Himall_Shops.IsSelf
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
                shoppingCartInfo = _iCartService.GetCart(memberId);
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
                return new CartItemModel()
                {
                    skuId = item,
                    id = sku.ProductInfo.Id,
                    imgUrl = sku.ProductInfo.GetImage(ProductInfo.ImageSize.Size_100),
                    name = sku.ProductInfo.ProductName,
                    shopId = sku.ProductInfo.ShopId,
                    price = ltmbuy == null ? sku.SalePrice : (decimal)_iLimitTimeBuyService.GetDetail(item).Price,
                    count = count,
                    productCode = sku.ProductInfo.ProductCode,
                    unit = sku.ProductInfo.MeasureUnit,
                    size = sku.Size,
                    color = sku.Color,
                    version = sku.Version,
                    IsSelf = sku.ProductInfo.Himall_Shops.IsSelf
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
            var actobj = _iFightGroupService.GetActive(actionId);

            var sku = actobj.ActiveItems.FirstOrDefault(d => d.SkuId == skuId);
            if (sku == null)
            {
                throw new HimallException("错误的规格信息");
            }
            if (count > actobj.LimitedNumber)
            {
                throw new HimallException("超过最大限购数量：" + actobj.LimitedNumber.ToString() + "");
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
            var data = new CartItemModel()
            {
                skuId = skuId,
                id = sku.ProductId.Value,
                imgUrl = HimallIO.GetProductSizeImage(actobj.ProductImgPath, 1, (int)ProductInfo.ImageSize.Size_100),
                name = actobj.ProductName,
                shopId = actobj.ShopId,
                price = sku.ActivePrice,
                count = count,
                productCode = actobj.ProductCode,
                unit = actobj.MeasureUnit,
                size = sku.Size,
                color = sku.Color,
                version = sku.Version,
                IsSelf = _iShopService.IsSelfShop(actobj.ShopId)
            };
            result.Add(data);

            return result;
        }

        /// <summary>
        /// 获取用户所有可用的优惠券
        /// </summary>
       static List<BaseCoupon> GetBaseCoupon(long shopId, long userId, decimal totalPrice)
        {
            var userCoupons = _iCouponService.GetUserCoupon(shopId, userId, totalPrice);
            var userBonus = _iShopBonusService.GetDetailToUse(shopId, userId, totalPrice);
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
                var shopBonus = _iShopBonusService.GetByShopId(o.ShopId);
                if (shopBonus == null)
                {
                    continue;
                }
                if (shopBonus.GrantPrice <= o.OrderTotalAmount)
                {
                    long grantid = _iShopBonusService.GenerateBonusDetail(shopBonus, o.Id, url);
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
        #endregion
        /// <summary>
        /// 订单完成订单数据写入待结算表
        /// </summary>
        /// <param name="o"></param>
        public static void WritePendingSettlnment(OrderInfo o)
        {
            _iOrderService.WritePendingSettlnment(o);
        }

    }
}
