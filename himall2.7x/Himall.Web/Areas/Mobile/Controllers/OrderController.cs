using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Web.Mvc;
using System.Linq;

using Himall.DTO;
using Himall.IServices;
using Himall.Web.Areas.Mobile.Models;
using Himall.Web.App_Code.Common;
using Himall.Core;
using Himall.Application;
using System.Collections.Generic;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Common;
using Himall.CommonModel;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class OrderController : BaseMobileMemberController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (CurrentUser != null && CurrentUser.Disabled)
            {
                filterContext.Result = RedirectToAction("Entrance", "Login", new { returnUrl = Request.Url.AbsoluteUri });
            }
        }
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 组合购提交页面
        /// </summary>
        /// <param name="skuIds">多个库存Id</param>
        /// <param name="counts">每个库存对应的数据量</param>
        /// <param name="regionId">客户收货地区的id</param>
        /// <param name="collpids">组合购Id集合</param>
        /// <returns>订单提交页面的数据</returns>
        public ActionResult SubmitByProductId(string skuIds, string counts, long? regionId, string collpids = null)
        {
            //Logo
            ViewBag.Logo = CurrentSiteSetting.Logo;//获取Logo
            //设置会员信息
            ViewBag.Member = CurrentUser;
            var result = OrderApplication.GetMobileCollocationBuy(UserId, skuIds, counts, regionId, collpids);

            ViewBag.collpids = collpids;
            ViewBag.skuIds = skuIds;
            ViewBag.counts = counts;
            ViewBag.InvoiceContext = result.InvoiceContext;
            ViewBag.IsCashOnDelivery = result.IsCashOnDelivery;
            ViewBag.address = result.Address;
            ViewBag.ConfirmModel = result;
            ViewBag.Islimit = false;

            var orderTag = Guid.NewGuid().ToString("N");
            ViewBag.OrderTag = orderTag;
            Session["OrderTag"] = orderTag;

            InitOrderSubmitModel(result);
            #region 是否开启门店授权
            ViewBag.IsOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            #endregion
            return View("Submit");
        }

        #region 拼团下单
        /// <summary>
        /// 拼团下单
        /// </summary>
        /// <param name="skuId">规格</param>
        /// <param name="count">数量</param>
        /// <param name="GroupActionId">拼团活动</param>
        /// <param name="GroupId">团组</param>
        /// <returns>订单提交页面的数据</returns>
        public ActionResult SubmitGroup(string skuId, int count, long GroupActionId, long? GroupId = null)
        {
            var result = OrderApplication.SubmitByGroupId(UserId, skuId, count, GroupActionId, GroupId);

            ViewBag.ConfirmModel = result;
            ViewBag.GroupActionId = GroupActionId;
            ViewBag.GroupId = GroupId;
            ViewBag.IsCashOnDelivery = result.IsCashOnDelivery;
            ViewBag.address = result.Address;
            ViewBag.InvoiceContext = result.InvoiceContext;
            var orderTag = Guid.NewGuid().ToString("N");
            ViewBag.OrderTag = orderTag;
            Session["OrderTag"] = orderTag;
            ViewBag.IsFightGroup = true;

            InitOrderSubmitModel(result);
            #region 是否开启门店授权
            ViewBag.IsOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            #endregion
            return View("submit");
        }

        #endregion

        /// <summary>
        /// 进入立即购买提交页面
        /// </summary>
        /// <param name="skuIds">库存ID集合</param>
        /// <param name="counts">库存ID对应的数量</param>
        /// <param name="GroupActionId">拼团活动编号</param>
        /// <param name="GroupId">拼团编号</param>
        public ActionResult Submit(string skuIds, string counts, int islimit = 0)
        {
            var result = OrderApplication.GetMobileSubmit(UserId, skuIds, counts);

            ViewBag.InvoiceContext = result.InvoiceContext;
            ViewBag.skuIds = skuIds;
            ViewBag.counts = counts;
            ViewBag.IsCashOnDelivery = result.IsCashOnDelivery;
            ViewBag.address = result.Address;
            ViewBag.ConfirmModel = result;
            ViewBag.Islimit = islimit == 1 ? true : false;

            var orderTag = Guid.NewGuid().ToString("N");
            ViewBag.OrderTag = orderTag;
            Session["OrderTag"] = orderTag;

            InitOrderSubmitModel(result);
            #region 是否开启门店授权
            ViewBag.IsOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            #endregion

            return View();
        }

        /// <summary>
        /// 判断订单是否已提交
        /// </summary>
        /// <param name="orderTag"></param>
        /// <returns></returns>
        public ActionResult IsSubmited(string orderTag)
        {
            return Json(object.Equals(Session["OrderTag"], orderTag) == false, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 展示门店列表
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="regionId"></param>
        /// <param name="skuIds"></param>
        /// <param name="counts"></param>
        /// <returns></returns>
        public ActionResult ShopBranchs(int shopId, int regionId, string[] skuIds, int[] counts, long shippingAddressId)
        {
            ViewBag.ShippingAddressId = shippingAddressId;
            return View(new ShopBranchModel
            {
                ShopId = shopId,
                RegionId = regionId,
                SkuIds = skuIds,
                Counts = counts
            });
        }

        /// <summary>
        /// 进入购物车提交页面
        /// </summary>
        /// <param name="cartItemIds">购物车物品id集合</param>
        public ActionResult SubmiteByCart(string cartItemIds)
        {
            var result = OrderApplication.GetMobileSubmiteByCart(UserId, cartItemIds);

            ViewBag.InvoiceContext = result.InvoiceContext;
            ViewBag.IsCashOnDelivery = result.IsCashOnDelivery;
            ViewBag.address = result.Address;
            ViewBag.ConfirmModel = result;

            var orderTag = Guid.NewGuid().ToString("N");
            ViewBag.OrderTag = orderTag;
            Session["OrderTag"] = orderTag;

            InitOrderSubmitModel(result);
            #region 是否开启门店授权
            ViewBag.IsOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            #endregion

            return View("submit");
        }


        /// <summary>
        /// 立即购买提交页面
        /// </summary>
        /// <param name="skuIds">库存ID集合</param>
        /// <param name="counts">每个库存对应的数量</param>
        /// <param name="recieveAddressId">客户收货地区ID</param>
        /// <param name="couponIds">优惠卷ID集合</param>
        /// <param name="integral">使用积分</param>
        /// <param name="isCashOnDelivery">是否货到付款</param>
        /// <param name="invoiceType">发票类型0不要发票1增值税发票2普通发票</param>
        /// <param name="invoiceTitle">发票抬头</param>
        /// <param name="invoiceContext">发票内容</param>
        [HttpPost]
        public JsonResult SubmitOrder(CommonModel.OrderPostModel model)
        {
            model.CurrentUser = CurrentUser;
            model.DistributionUserLinkId = GetDistributionUserLinkId();
            model.PlatformType = PlatformType.GetHashCode();

            var result = OrderApplication.SubmitOrder(model);
            ClearDistributionUserLinkId();   //清理分销cookie
            OrderApplication.AddVshopBuyNumber(result.OrderIds);
            Session.Remove("OrderTag");
            return Json(new { success = result.Success, orderIds = result.OrderIds, realTotalIsZero = result.OrderTotal == 0 });
        }

        /// <summary>
        /// 限时购订单提交
        /// </summary>
        /// <param name="skuIds">库存ID</param>
        /// <param name="counts">购买数量</param>
        /// <param name="recieveAddressId">客户收货区域ID</param>
        /// <param name="couponIds">优惠卷</param>
        /// <param name="invoiceType">发票类型0不要发票1增值税发票2普通发票</param>
        /// <param name="invoiceTitle">发票抬头</param>
        /// <param name="invoiceContext">发票内容</param>
        /// <param name="integral">使用积分</param>
        /// <param name="collpIds">组合构ID</param>
        /// <param name="isCashOnDelivery">是否货到付款</param>
        /// <returns>redis方式返回虚拟订单ID，数据库方式返回实际订单ID</returns>
        [HttpPost]
        public JsonResult SubmitLimitOrder(CommonModel.OrderPostModel model)
        {
            model.CurrentUser = CurrentUser;
            model.PlatformType = (int)PlatformType.WeiXin;
            var result = OrderApplication.GetLimitOrder(model);
            if (LimitOrderHelper.IsRedisCache())
            {
                string id = "";
                SubmitOrderResult r = LimitOrderHelper.SubmitOrder(result, out id);
                if (r == SubmitOrderResult.SoldOut)
                    throw new HimallException("已售空");
                else if (r == SubmitOrderResult.NoSkuId)
                    throw new InvalidPropertyException("创建订单的时候，SKU为空，或者数量为0");
                else if (r == SubmitOrderResult.NoData)
                    throw new InvalidPropertyException("参数错误");
                else if (r == SubmitOrderResult.NoLimit)
                    throw new InvalidPropertyException("没有限时购活动");
                else if (string.IsNullOrEmpty(id))
                    throw new InvalidPropertyException("参数错误");
                else
                {
                    OrderApplication.UpdateDistributionUserLink(GetDistributionUserLinkId().ToArray(), UserId);
                    return Json(new { success = true, Id = id });
                }
            }
            else
            {
                var orderIds = OrderApplication.OrderSubmit(result);
                return Json(new { success = true, orderIds = orderIds });
            }
        }

        /// <summary>
        /// 积分支付
        /// </summary>
        /// <param name="orderIds">订单Id</param>
        [HttpPost]
        public JsonResult PayOrderByIntegral(string orderIds)
        {
            try
            {
                OrderApplication.PayOrderByIntegral(orderIds, UserId);
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message });
            }
        }

        /// <summary>
        /// 取消积分支付订单
        /// </summary>
        /// <param name="orderIds">订单Id</param>
        [HttpPost]
        public JsonResult CancelOrders(string orderIds)
        {
            try
            {
                OrderApplication.CancelOrder(orderIds, UserId);
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message });
            }

        }

        /// <summary>
        /// 是否全部抵扣
        /// </summary>
        /// <param name="integral">积分</param>
        /// <param name="total">总价</param>
        [HttpPost]
        public ActionResult IsAllDeductible(int integral, decimal total)
        {
            return Json(OrderApplication.IsAllDeductible(integral, total, UserId));
        }

        /// <summary>
        /// 获取收货地址界面
        /// </summary>
        /// <param name="returnURL">返回url路径</param>
        public ActionResult ChooseShippingAddress(string returnURL = "")
        {
            return View(OrderApplication.GetUserAddresses(UserId));
        }

        /// <summary>
        /// 设置默认收货地址
        /// </summary>
        /// <param name="addId">收货地址Id</param>
        [HttpPost]
        public JsonResult SetDefaultUserShippingAddress(long addId)
        {
            OrderApplication.SetDefaultUserShippingAddress(addId, UserId);
            return Json(new { success = true, addId = addId });
        }

        /// <summary>
        /// 获得编辑收获地址页面
        /// </summary>
        /// <param name="addressId">收货地址Id</param>
        /// <param name="returnURL">返回url路径</param>
        public ActionResult EditShippingAddress(long addressId = 0, string returnURL = "")
        {
            var ShipngInfo = OrderApplication.GetUserAddress(addressId);
            ViewBag.addId = addressId;
            if (ShipngInfo != null)
            {
                ViewBag.fullPath = RegionApplication.GetRegionPath(ShipngInfo.RegionId);
                ViewBag.fullName = RegionApplication.GetFullName(ShipngInfo.RegionId);
            }
            return View(ShipngInfo);
        }

        /// <summary>
        /// 删除收货地址
        /// </summary>
        /// <param name="addressId">收货地址Id</param>
        [HttpPost]
        public ActionResult DeleteShippingAddress(long addressId)
        {
            OrderApplication.DeleteShippingAddress(addressId, UserId);
            return Json(new { success = true });
        }

        /// <summary>
        /// 获得用户的收货地址信息
        /// </summary>
        /// <param name="addressId">收货地址Id</param>
        [HttpPost]
        public JsonResult GetUserShippingAddresses(long addressId)
        {
            var addresses = OrderApplication.GetUserAddress(addressId);
            var json = new
            {
                id = addresses.Id,
                fullRegionName = addresses.RegionFullName,
                address = addresses.Address,
                phone = addresses.Phone,
                shipTo = addresses.ShipTo,
                fullRegionIdPath = addresses.RegionIdPath
            };
            return Json(json);
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="orderId">订单Id</param>
        [HttpPost]
        public JsonResult CloseOrder(long orderId)
        {
            UserMemberInfo umi = CurrentUser;
            bool isClose = OrderApplication.CloseOrder(orderId, umi.Id, umi.UserName);
            if (isClose)
                return Json(new Result() { success = true, msg = "取消成功" });
            else
                return Json(new Result() { success = false, msg = "取消失败，该订单已删除或者不属于当前用户！" });
        }

        /// <summary>
        /// 确认订单收货
        /// </summary>
        [HttpPost]
        public JsonResult ConfirmOrder(long orderId)
        {
            var status = OrderApplication.ConfirmOrder(orderId, CurrentUser.Id, CurrentUser.UserName);
            Result result = new Result() { status = status };
            switch (status)
            {
                case 0:
                    result.success = true;
                    result.msg = "操作成功";
                    break;
                case 1:
                    result.success = false;
                    result.msg = "该订单已经确认过!";
                    break;
                case 2:
                    result.success = false;
                    result.msg = "订单状态发生改变，请重新刷页面操作!";
                    break;
            }
            // var data = ServiceHelper.Create<IOrderService>.Create.GetOrder(orderId);
            //确认收货写入结算表 改LH写在Controller里的
            // ServiceHelper.Create<IOrderService>.Create.WritePendingSettlnment(data);
            return Json(result);
        }

        /// <summary>
        /// 订单详细信息页面
        /// </summary>
        /// <param name="id">订单Id</param>
        public ActionResult Detail(long id)
        {
            OrderDetailView view = OrderApplication.Detail(id, UserId, PlatformType, Request.Url.Host);
            ViewBag.Detail = view.Detail;
            ViewBag.Bonus = view.Bonus;
            ViewBag.ShareHref = view.ShareHref;
            ViewBag.IsRefundTimeOut = view.IsRefundTimeOut;
            ViewBag.Logo = CurrentSiteSetting.Logo;
            view.Order.FightGroupOrderJoinStatus = view.FightGroupJoinStatus;
            view.Order.FightGroupCanRefund = view.FightGroupCanRefund;

            var customerServices = CustomerServiceApplication.GetMobileCustomerService(view.Order.ShopId);
            var meiqia = CustomerServiceApplication.GetPreSaleByShopId(view.Order.ShopId).FirstOrDefault(p => p.Tool == CustomerServiceInfo.ServiceTool.MeiQia);
            if (meiqia != null)
                customerServices.Insert(0, meiqia);
            ViewBag.CustomerServices = customerServices;
            #region 门店信息
            if (view.Order.ShopBranchId.HasValue && view.Order.ShopBranchId > 0)
            {
                ViewBag.ShopBranchInfo = ShopBranchApplication.GetShopBranchById(view.Order.ShopBranchId.Value);
            }
            #endregion
            return View(view.Order);
        }

        /// <summary>
        /// 快递信息
        /// </summary>
        /// <param name="orderId">订单Id</param>
        public ActionResult ExpressInfo(long orderId)
        {
            string[] result = OrderApplication.GetExpressInfo(orderId, UserId);
            ViewBag.ExpressCompanyName = result[0];
            ViewBag.ShipOrderNumber = result[1];
            return View();
        }

        /// <summary>
        /// 获取商家分店
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="regionId">街道id</param>
        /// <param name="getParent">是否获取县/区下面所有街道的分店</param>
        /// <param name="skuIds">购买的商品的sku</param>
        /// <param name="counts">商品sku对应的购买数量</param>
        /// <param name="shippingAddressesId">订单收货地址ID</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetShopBranchs(long shopId, long regionId, bool getParent, string[] skuIds, int[] counts, int page, int rows, long shippingAddressId)
        {
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

            var region = RegionApplication.GetRegion(regionId, getParent ? CommonModel.Region.RegionLevel.City : CommonModel.Region.RegionLevel.County);
            if (region != null) query.AddressPath = region.GetIdPath();

            #region 旧排序规则
            //var skuInfos = ProductManagerApplication.GetSKUs(skuIds);

            //query.ProductIds = skuInfos.Select(p => p.ProductId).ToArray();
            //var data = ShopBranchApplication.GetShopBranchs(query);

            //var shopBranchSkus = ShopBranchApplication.GetSkus(shopId, data.Models.Select(p => p.Id));

            //var models = new
            //{
            //    Rows = data.Models.Select(sb => new
            //    {
            //        sb.ContactUser,
            //        sb.ContactPhone,
            //        sb.AddressDetail,
            //        sb.ShopBranchName,
            //        sb.Id,
            //        Enabled = skuInfos.All(skuInfo => shopBranchSkus.Any(sbSku => sbSku.ShopBranchId == sb.Id && sbSku.Stock >= counts[skuInfos.IndexOf(skuInfo)] && sbSku.SkuId == skuInfo.Id))
            //    }).OrderByDescending(p => p.Enabled).ToArray(),
            //    data.Total
            //};
            #endregion
            #region 3.0版本排序规则
            var skuInfos = ProductManagerApplication.GetSKUs(skuIds);
            query.ProductIds = skuInfos.Select(p => p.ProductId).ToArray();
            var data = ShopBranchApplication.GetShopBranchsAll(query);
            var shopBranchSkus = ShopBranchApplication.GetSkus(shopId, data.Models.Select(p => p.Id));//获取该商家下具有订单内所有商品的门店状态正常数据,不考虑库存
            data.Models.ForEach(p =>
            {
                p.Enabled = skuInfos.All(skuInfo => shopBranchSkus.Any(sbSku => sbSku.ShopBranchId == p.Id && sbSku.Stock >= counts[skuInfos.IndexOf(skuInfo)] && sbSku.SkuId == skuInfo.Id));
            });

            List<ShopBranch> newList = new List<ShopBranch>();
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
            if (newList.Count() != data.Models.Count())//如果新组合的数据与原数据数量不一致，则异常
            {
                return Json(new
                {
                    Rows = ""
                }, true);
            }
            var models = new
            {
                Rows = newList.Select(sb => new
                {
                    sb.ContactUser,
                    sb.ContactPhone,
                    sb.AddressDetail,
                    sb.ShopBranchName,
                    sb.Id,
                    Enabled = sb.Enabled
                }).ToArray(),
                Total = newList.Count
            };
            #endregion

            return Json(models, true);
        }

        public JsonResult ExistShopBranch(int shopId, int regionId, long[] productIds)
        {
            var query = new CommonModel.ShopBranchQuery();
            query.Status = CommonModel.ShopBranchStatus.Normal;
            query.ShopId = shopId;

            var region = RegionApplication.GetRegion(regionId, CommonModel.Region.RegionLevel.City);
            query.AddressPath = region.GetIdPath();
            query.ProductIds = productIds;
            var existShopBranch = ShopBranchApplication.Exists(query);

            return Json(existShopBranch, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取运费
        /// </summary>
        /// <param name="addressId">地址ID</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CalcFreight(int addressId, CalcFreightparameter[] parameters)
        {
            var result = OrderApplication.CalcFreight(addressId, parameters.GroupBy(p => p.ShopId).ToDictionary(p => p.Key, p => p.GroupBy(pp => pp.ProductId).ToDictionary(pp => pp.Key, pp => pp.Sum(ppp => ppp.Count))));
            if (result.Count == 0)
                return Json(new { success = false, msg = "计算运费失败" });
            else
                return Json(new { success = true, freights = result.Select(p => new { shopId = p.Key, freight = p.Value }).ToArray() });
        }
        [HttpPost]
        public JsonResult GetOrderPayStatus(string orderids)
        {
            var isPaied = OrderApplication.AllOrderIsPaied(orderids);
            return Json(new { success = isPaied });
        }

        public ActionResult OrderShare(string orderids, string source)
        {
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
            if (MemberIntegralApplication.OrderIsShared(ids))
            {
                ViewBag.IsShared = true;
            }
            ViewBag.Source = source;
            ViewBag.OrderIds = orderids;
            var orders = OrderApplication.GetOrderDetailViews(ids);
            return View(orders);
        }


        [HttpPost]
        public JsonResult OrderShareAddIntegral(string orderids)
        {
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

        [HttpGet]
        public ActionResult InitRegion(string fromLatLng)
        {
            string address = string.Empty, province = string.Empty, city = string.Empty, district = string.Empty, street = string.Empty, newStreet = string.Empty;
            ShopbranchHelper.GetAddressByLatLng(fromLatLng, ref address, ref province, ref city, ref district, ref street);
            if (district == "" && street != "")
            {
                district = street;
                street = "";
            }
            string fullPath = RegionApplication.GetAddress_Components(city, district, street, out newStreet);
            if (fullPath.Split(',').Length <= 3) newStreet = string.Empty;//如果无法匹配街道，则置为空
            return Json(new { fullPath = fullPath, showCity = string.Format("{0} {1} {2}", province, city, district), street = newStreet }, JsonRequestBehavior.AllowGet);
        }
        #region 私有方法
        private void InitOrderSubmitModel(MobileOrderDetailConfirmModel model)
        {
            if (model.Address != null)
            {
                var query = new CommonModel.ShopBranchQuery();
                query.Status = CommonModel.ShopBranchStatus.Normal;

                var region = RegionApplication.GetRegion(model.Address.RegionId, CommonModel.Region.RegionLevel.City);
                query.AddressPath = region.GetIdPath();

                foreach (var item in model.products)
                {
                    query.ShopId = item.shopId;
                    query.ProductIds = item.CartItemModels.Select(p => p.id).ToArray();
                    item.ExistShopBranch = ShopBranchApplication.Exists(query);
                }
            }
        }

        /// <summary>
        /// 是否超出限购数
        /// </summary>
        /// <param name="products"></param>
        /// <param name="buyCounts">buyCounts</param>
        /// <returns></returns>
        private bool IsOutMaxBuyCount(IEnumerable<ProductInfo> products, Dictionary<long, int> buyCounts)
        {
            var buyedCounts = OrderApplication.GetProductBuyCount(CurrentUser.Id, products.Select(pp => pp.Id));
            var isOutMaxBuyCount = products.Any(pp => pp.MaxBuyCount > 0 && pp.MaxBuyCount < (buyedCounts.ContainsKey(pp.Id) ? buyedCounts[pp.Id] : 0) + buyCounts[pp.Id]);

            return isOutMaxBuyCount;
        }
        #endregion
    }

}