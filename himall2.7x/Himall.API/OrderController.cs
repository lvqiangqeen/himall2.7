using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Himall.Web;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

using Himall.CommonModel;
using Himall.Application;
using Himall.Service;
using Himall.API.Model;
using Himall.API.Helper;
using Himall.API.Model.ParamsModel;

namespace Himall.API
{
    public class OrderController : BaseApiController
    {
        private IOrderService _iOrderService;
        private IMemberIntegralService _iMemberIntegralService;
        private ICartService _iCartService;
        private IMemberService _iMemberService;
        private IProductService _iProductService;
        private IPaymentConfigService _iPaymentConfigService;
        private IShippingAddressService _iShippingAddressService;
        private IRegionService _iRegionService;
        private ICashDepositsService _iCashDepositsService;
        private ISiteSettingService _iSiteSettingService;
        private IShopService _iShopService;
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private ICouponService _iCouponService;
        private IShopBonusService _iShopBonusService;
        private ICollocationService _iCollocationService;
        private IMemberCapitalService _iMemberCapitalService;
        private IVShopService _iVShopService;
        private IRefundService _iRefundService;
        private IFightGroupService _iFightGroupService;
        public OrderController()
        {
            _iOrderService = new OrderService();
            _iCartService = new CartService();
            _iMemberService = new MemberService();
            _iProductService = new ProductService();
            _iPaymentConfigService = new PaymentConfigService();
            _iCashDepositsService = new CashDepositsService();
            _iSiteSettingService = new SiteSettingService();
            _iShopService = new ShopService();
            _iLimitTimeBuyService = new LimitTimeBuyService();
            _iCouponService = new CouponService();
            _iShopBonusService = new ShopBonusService();
            _iCollocationService = new CollocationService();
            _iMemberCapitalService = new MemberCapitalService();
            _iShippingAddressService = new ShippingAddressService();
            _iMemberIntegralService = new MemberIntegralService();
            _iRegionService = new RegionService();
            _iVShopService = new VShopService();
            _iRefundService = new RefundService();
            _iFightGroupService = new FightGroupService();
        }
        /// <summary>
        /// 获取立即购买提交页面的数据
        /// </summary>
        /// <param name="skuIds">库存ID集合</param>
        /// <param name="counts">库存ID对应的数量</param>
        public object GetSubmitModel(string skuId, int count)
        {
            CheckUserLogin();
            var result = OrderApplication.GetMobileSubmit(CurrentUserId, skuId.ToString(), count.ToString());
            dynamic d = new System.Dynamic.ExpandoObject();

            if (result.Address != null)
            {
                var add = new
                {
                    Id = result.Address.Id,
                    ShipTo = result.Address.ShipTo,
                    Phone = result.Address.Phone,
                    Address = result.Address.RegionFullName + " " + result.Address.Address,
                    RegionId = result.Address.RegionId
                };
                d.Address = add;
            }
            else
                d.Address = null;

            d.Success = "true"; ;
            d.InvoiceContext = result.InvoiceContext;
            d.products = result.products;
            d.integralPerMoney = result.integralPerMoney;
            d.userIntegrals = result.userIntegrals;
            d.TotalAmount = result.totalAmount;
            d.Freight = result.Freight;
            d.orderAmount = result.orderAmount;
            d.IsCashOnDelivery = result.IsCashOnDelivery;
            d.IsOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            return d;
        }

        /// <summary>
        /// 获取购物车提交页面的数据
        /// </summary>
        /// <param name="cartItemIds">购物车物品id集合</param>
        /// <returns></returns>
        public object GetSubmitByCartModel(string cartItemIds = "")
        {
            CheckUserLogin();
            var result = OrderApplication.GetMobileSubmiteByCart(CurrentUserId, cartItemIds);

            //解决循环引用的序列化的问题
            dynamic address = new System.Dynamic.ExpandoObject();
            if (result.Address != null)
            {
                var add = new
                {
                    Id = result.Address.Id,
                    ShipTo = result.Address.ShipTo,
                    Phone = result.Address.Phone,
                    Address = result.Address.RegionFullName + " " + result.Address.Address,
                    RegionId = result.Address.RegionId
                };
                address = add;
            }
            else
                address = null;

            return Json(new
            {
                Success = "true",
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
        });
        }

        /// <summary>
        /// 立即购买方式提交的订单
        /// </summary>
        /// <param name="value">数据</param>
        public object PostSubmitOrder(OrderSubmitOrderModel value)
        {
            CheckUserLogin();
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
            model.PlatformType = PlatformType.Android;
            model.CurrentUser = CurrentUser;
            model.ReceiveAddressId = recieveAddressId;
            model.SkuIds = skuIdArr;
            model.Counts = countArr;
            model.Integral = integral;



            model.IsCashOnDelivery = isCashOnDelivery;
            model.Invoice = (InvoiceType)invoiceType;
            model.InvoiceContext = invoiceContext;
            model.InvoiceTitle = invoiceTitle;

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
                return Json(new { Success = "true", OrderIds = orderIds, RealTotalIsZero = orderTotals == 0 });
            }
            catch (HimallException he)
            {
                return Json(new { Success = "false", Msg = he.Message });
            }
            catch (Exception ex)
            {
                return Json(new { Success = "false", Msg = "提交订单异常" });
            }
        }

        /// <summary>
        /// 购物车方式提交的订单
        /// </summary>
        /// <param name="value">数据</param>
        public object PostSubmitOrderByCart(OrderSubmitOrderByCartModel value)
        {
            CheckUserLogin();
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
            var orderService = ServiceProvider.Instance<IOrderService>.Create;
            IEnumerable<long> orderIds;
            model.PlatformType = PlatformType.Android;
            model.CurrentUser = CurrentUser;
            model.ReceiveAddressId = recieveAddressId;
            model.Integral = integral;


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
                var realTotalIsZero = "false";
                if (orderTotals == 0)
                    realTotalIsZero = "true";
                return Json(new { Success = "true", OrderIds = orderIds, RealTotalIsZero = realTotalIsZero });
            }
            catch (HimallException he)
            {
                return Json(new { Success = "false", Msg = he.Message });
            }
            catch (Exception ex)
            {
                return Json(new { Success = "false", Msg = "提交订单异常" });
            }
        }

        /// <summary>
        /// 拼团订单提交
        /// </summary>
        /// <param name="value">表单数据</param>
        /// <returns></returns>
        public object PostSubmitFightGroupOrder(OrderSubmitFightGroupOrderModel value)
        {
            CheckUserLogin();
            string skuIds = value.skuId;
            long counts = value.count;
            long recieveAddressId = value.recieveAddressId;
            long activeId = value.GroupActionId;
            long groupId = value.GroupId;

            bool isCashOnDelivery = value.isCashOnDelivery;
            int invoiceType = value.invoiceType;
            string invoiceTitle = value.invoiceTitle;
            string invoiceContext = value.invoiceContext;

            string orderRemarks = "";//value.orderRemarks;//订单备注
            List<FightGroupOrderJoinStatus> seastatus = new List<FightGroupOrderJoinStatus>();
            seastatus.Add(FightGroupOrderJoinStatus.Ongoing);
            seastatus.Add(FightGroupOrderJoinStatus.JoinSuccess);
            var groupData = ServiceProvider.Instance<IFightGroupService>.Create.GetActive(activeId, false, false);

            if (counts > groupData.LimitQuantity)
            {
                return Json(new { Success = "false", Msg = string.Format("每人限购数量：{0}！", groupData.LimitQuantity) });
            }
            if (groupId > 0)   //非开团，判断人数限制
            {
                var orderData = ServiceProvider.Instance<IFightGroupService>.Create.GetFightGroupOrderByGroupId(seastatus, groupId);
                if (orderData != null && groupData.LimitedNumber <= orderData.Count)
                {
                    return Json(new { Success = "false", Msg = "该团参加人数已满！" });
                }
            }

            try
            {
                var model = OrderApplication.GetGroupOrder(CurrentUser.Id, skuIds, counts.ToString(), recieveAddressId, invoiceType, invoiceTitle, invoiceContext, activeId, PlatformType.Android, groupId, isCashOnDelivery, orderRemarks);
                CommonModel.OrderShop[] OrderShops = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderShop[]>(value.jsonOrderShops);
                model.OrderShops = OrderShops;//用户APP选择门店自提时用到，2.5版本未支持门店自提
                model.OrderRemarks = OrderShops.Select(p => p.Remark).ToArray();
                var orderIds = OrderApplication.OrderSubmit(model);
                AddVshopBuyNumber(orderIds);//添加微店购买数量
                return Json(new { Success = "true", OrderIds = orderIds });
            }
            catch (HimallException he)
            {
                return Json(new { Success = "false", Msg = he.Message });
            }
            catch (Exception ex)
            {
                return Json(new { Success = "false", Msg = "提交订单异常" });
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

        /// <summary>
        /// 是否可用积分购买
        /// </summary>
        /// <param name="orderIds">订单id</param>
        public object GetPayOrderByIntegral(string orderIds)
        {
            CheckUserLogin();
            var orderIdArr = orderIds.Split(',').Select(item => long.Parse(item));
            var service = ServiceHelper.Create<IOrderService>();
            service.ConfirmZeroOrder(orderIdArr, CurrentUser.Id);
            return Json(new { Success = "true" });
        }

        /// <summary>
        /// 获取拼团订单信息Model
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="count"></param>
        /// <param name="GroupActionId"></param>
        /// <param name="GroupId"></param>
        /// <returns></returns>
        public object GetGroupOrderModel(string skuId, int count, long GroupActionId, long? GroupId = null)
        {
            CheckUserLogin();
            Himall.DTO.MobileOrderDetailConfirmModel result = OrderApplication.SubmitByGroupId(CurrentUser.Id, skuId, count, GroupActionId, GroupId);
            if (result.Address != null)
            {
                result.Address.MemberInfo = new UserMemberInfo();
            }
            result.IsOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            return result;
        }

        public object GetOrderShareProduct(string orderids)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(orderids))
            {
                throw new HimallException("订单号不能为空！");
            }
            long orderId = 0;
            var ids = orderids.Split(',').Select(e =>
            {
                if (long.TryParse(e, out orderId))
                {
                    return orderId;
                }
                else
                {
                    return 0;
                }
            }
            );
            var orders = OrderApplication.GetOrderDetailViews(ids);
            return Json(new { Success = true, OrderDetail = orders });
        }
        public object PostOrderShareAddIntegral(OrderShareAddIntegralModel OrderIds)
        {
            CheckUserLogin();
            var orderids = OrderIds.orderids;
            if (string.IsNullOrWhiteSpace(orderids))
            {
                throw new HimallException("订单号不能为空！");
            }
            long orderId = 0;
            var ids = orderids.Split(',').Select(e =>
            {
                if (long.TryParse(e, out orderId))
                    return orderId;
                else
                    throw new HimallException("订单分享增加积分时，订单号异常！");
            }
            );
            if (MemberIntegralApplication.OrderIsShared(ids))
            {
                throw new HimallException("订单已经分享过！");
            }
            MemberIntegralRecord record = new MemberIntegralRecord();
            record.MemberId = CurrentUser.Id;
            record.UserName = CurrentUser.UserName;
            record.RecordDate = DateTime.Now;
            record.TypeId = MemberIntegral.IntegralType.Share;
            record.ReMark = string.Format("订单号:{0}", orderids);
            List<MemberIntegralRecordAction> recordAction = new List<MemberIntegralRecordAction>();

            foreach (var id in ids)
            {
                recordAction.Add(new MemberIntegralRecordAction
                {
                    VirtualItemId = id,
                    VirtualItemTypeId = MemberIntegral.VirtualItemType.ShareOrder
                });
            }
            record.Himall_MemberIntegralRecordAction = recordAction;
            MemberIntegralApplication.AddMemberIntegralByEnum(record, MemberIntegral.IntegralType.Share);
            return Json(new { success = true, msg = "晒单添加积分成功！" });
        }

        /// <summary>
        /// 获取自提门店点
        /// </summary>
        /// <returns></returns>
        public object GetShopBranchs(long shopId, bool getParent, string skuIds, string counts, int page, int rows, long shippingAddressId, long regionId)
        {
            string[] _skuIds = skuIds.Split(',');
            int[] _counts = counts.Split(',').Select(p => Himall.Core.Helper.TypeHelper.ObjectToInt(p)).ToArray();

            var shippingAddressInfo = ShippingAddressApplication.GetUserShippingAddress(shippingAddressId);
            int streetId = 0, districtId = 0;//收货地址的街道、区域

            var query = new CommonModel.ShopBranchQuery()
            {
                ShopId = shopId,
                PageNo = page,
                PageSize = rows,
                Status = CommonModel.ShopBranchStatus.Normal
            };
            if (shippingAddressInfo != null)
            {
                query.FromLatLng = string.Format("{0},{1}", shippingAddressInfo.Latitude, shippingAddressInfo.Longitude);//需要收货地址的经纬度
                streetId = shippingAddressInfo.RegionId;
                var parentAreaInfo = RegionApplication.GetRegion(shippingAddressInfo.RegionId, Region.RegionLevel.Town);//判断当前区域是否为第四级
                if (parentAreaInfo != null && parentAreaInfo.ParentId > 0) districtId = parentAreaInfo.ParentId;
                else { districtId = streetId; streetId = 0; }
            }
            bool hasLatLng = false;
            if (!string.IsNullOrWhiteSpace(query.FromLatLng)) hasLatLng = query.FromLatLng.Split(',').Length == 2;

            var region = RegionApplication.GetRegion(regionId, getParent ? CommonModel.Region.RegionLevel.City : CommonModel.Region.RegionLevel.County);//同城内门店
            if (region != null) query.AddressPath = region.GetIdPath();

            #region 3.0版本排序规则
            var skuInfos = ProductManagerApplication.GetSKUs(_skuIds);
            query.ProductIds = skuInfos.Select(p => p.ProductId).ToArray();
            var data = ShopBranchApplication.GetShopBranchsAll(query);
            var shopBranchSkus = ShopBranchApplication.GetSkus(shopId, data.Models.Select(p => p.Id));//获取该商家下具有订单内所有商品的门店状态正常数据,不考虑库存
            data.Models.ForEach(p =>
            {
                p.Enabled = skuInfos.All(skuInfo => shopBranchSkus.Any(sbSku => sbSku.ShopBranchId == p.Id && sbSku.Stock >= _counts[skuInfos.IndexOf(skuInfo)] && sbSku.SkuId == skuInfo.Id));
            });

            List<Himall.DTO.ShopBranch> newList = new List<Himall.DTO.ShopBranch>();
            List<long> fillterIds = new List<long>();
            var currentList = data.Models.Where(p => hasLatLng && p.Enabled && (p.Latitude > 0 && p.Longitude > 0)).OrderBy(p => p.Distance).ToList();
            if (currentList != null && currentList.Count() > 0)
            {
                fillterIds.AddRange(currentList.Select(p => p.Id));
                newList.AddRange(currentList);
            }
            var currentList2 = data.Models.Where(p => !fillterIds.Contains(p.Id) && p.Enabled && p.AddressPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + streetId + CommonConst.ADDRESS_PATH_SPLIT)).ToList();
            if (currentList2 != null && currentList2.Count() > 0)
            {
                fillterIds.AddRange(currentList2.Select(p => p.Id));
                newList.AddRange(currentList2);
            }
            var currentList3 = data.Models.Where(p => !fillterIds.Contains(p.Id) && p.Enabled && p.AddressPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + districtId + CommonConst.ADDRESS_PATH_SPLIT)).ToList();
            if (currentList3 != null && currentList3.Count() > 0)
            {
                fillterIds.AddRange(currentList3.Select(p => p.Id));
                newList.AddRange(currentList3);
            }
            var currentList4 = data.Models.Where(p => !fillterIds.Contains(p.Id) && p.Enabled).ToList();//非同街、非同区，但一定会同市
            if (currentList4 != null && currentList4.Count() > 0)
            {
                fillterIds.AddRange(currentList4.Select(p => p.Id));
                newList.AddRange(currentList4);
            }
            var currentList5 = data.Models.Where(p => !fillterIds.Contains(p.Id)).ToList();//库存不足的排最后
            if (currentList5 != null && currentList5.Count() > 0)
            {
                newList.AddRange(currentList5);
            }
            if (newList.Count() != data.Models.Count())//如果新组合的数据与原数据数量不一致
            {
                return Json(new
                {
                    Success = false
                });
            }
            var storeList = newList.Select(sb =>
            {
                return new
                {
                    ContactUser = sb.ContactUser,
                    ContactPhone = sb.ContactPhone,
                    AddressDetail = sb.AddressDetail,
                    ShopBranchName = sb.ShopBranchName,
                    Id = sb.Id,
                    Enabled = sb.Enabled
                };
            });

            #endregion

            var result = new
            {
                Success = true,
                StoreList = storeList
            };
            return Json(result);
        }

        /// <summary>
        /// 是否允许门店自提
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="regionId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public object GetExistShopBranch(long shopId, long regionId, string productIds)
        {
            var query = new CommonModel.ShopBranchQuery();
            query.Status = CommonModel.ShopBranchStatus.Normal;
            query.ShopId = shopId;

            var region = RegionApplication.GetRegion(regionId, CommonModel.Region.RegionLevel.City);
            query.AddressPath = region.GetIdPath();
            query.ProductIds = productIds.Split(',').Select(p => long.Parse(p)).ToArray();
            var existShopBranch = ShopBranchApplication.Exists(query);

            return Json(new { Success = true, ExistShopBranch = existShopBranch ? 1 : 0 });
        }
    }
}