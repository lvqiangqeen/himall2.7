using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System.Linq;
using System.Web.Mvc;
using Himall.Core;
using Himall.Core.Plugins.Payment;
using System.Collections;
using System.Collections.Generic;
using Senparc.Weixin.MP.Helpers;
using System;
using Himall.Core.Helper;
using System.Threading.Tasks;
using Himall.Web.App_Code.Common;
using Himall.Web.Areas.Mobile.Models;
using Himall.Web.Areas.Web.Models;
using Himall.Application;
using Himall.DTO;
using Himall.CommonModel;

namespace Himall.Web.Areas.Mobile.Controllers
{

    //TODO:Service 好多Service ？
    public class MemberController : BaseMobileMemberController
    {
        private IOrderService _iOrderService;
        private IMemberService _iMemberService;
        private IMemberCapitalService _iMemberCapitalService;
        private ICouponService _iCouponService;
        private IShopBonusService _iShopBonusService;
        private IVShopService _iVShopService;
        private IProductService _iProductService;
        private IShippingAddressService _iShippingAddressService;
        private IMessageService _iMessageService;
        private IMemberSignInService _iMemberSignInService;
        private IDistributionService _iDistributionService;
        private IRefundService _iRefundService;
        private ICommentService _iCommentService;
        public MemberController(
            IOrderService iOrderService,
            IMemberService iMemberService,
             IMemberCapitalService iMemberCapitalService,
             ICouponService iCouponService,
             IShopBonusService iShopBonusService,
             IVShopService iVShopService,
             IProductService iProductService,
             IShippingAddressService iShippingAddressService,
             IMessageService iMessageService,
            IDistributionService iDistributionService,
            IMemberSignInService iMemberSignInService,
            IRefundService iRefundService,
            ICommentService iCommentService
            )
        {
            _iOrderService = iOrderService;
            _iMemberService = iMemberService;
            _iMemberCapitalService = iMemberCapitalService;
            _iCouponService = iCouponService;
            _iShopBonusService = iShopBonusService;
            _iVShopService = iVShopService;
            _iProductService = iProductService;
            _iShippingAddressService = iShippingAddressService;
            _iMessageService = iMessageService;
            _iMemberSignInService = iMemberSignInService;
            _iDistributionService = iDistributionService;
            _iRefundService = iRefundService;
            _iCommentService = iCommentService;
        }
        public ActionResult Center()
        {
            MemberCenterModel model = new MemberCenterModel();
            var orders = _iOrderService.GetTopOrders(int.MaxValue, CurrentUser.Id);
            var setting = _iDistributionService.GetDistributionSetting();

            model.CanDistribution = false;
            if (setting != null && setting.Enable)
            {
                model.CanDistribution = true;
            }
            //待评价
            //var queryModel = new OrderQuery()
            //{
            //    Status = Model.OrderInfo.OrderOperateStatus.Finish,
            //    UserId = CurrentUser.Id,
            //    PageSize = int.MaxValue,
            //    PageNo = 1,
            //    Commented = false
            //};
            //ViewBag.WaitingForComments = _iOrderService.GetOrders<OrderInfo>(queryModel).Total;

            var member = _iMemberService.GetMember(CurrentUser.Id);
            model.Member = member;
            model.AllOrders = orders.Count();
            model.WaitingForRecieve = orders.Count(item => item.UserId == CurrentUser.Id && (item.OrderStatus == Model.OrderInfo.OrderOperateStatus.WaitReceiving || item.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp));//获取待收货订单数
            model.WaitingForPay = orders.Count(item => item.OrderStatus == Model.OrderInfo.OrderOperateStatus.WaitPay);//获取待支付订单数
            var waitdelordnum = orders.Count(item => item.OrderStatus == Model.OrderInfo.OrderOperateStatus.WaitDelivery);//获取待发货订单数
            var fgwaitdelordnum = _iOrderService.GetFightGroupOrderByUser(CurrentUser.Id);
            model.WaitingForDelivery = waitdelordnum - fgwaitdelordnum;
            model.WaitingForComments = orders.Count(item => item.OrderStatus == Model.OrderInfo.OrderOperateStatus.Finish && item.OrderCommentInfo.Count == 0);

            //拼团
            model.CanFightGroup = FightGroupApplication.IsOpenMarketService();
            model.BulidFightGroupNumber = FightGroupApplication.CountJoiningOrder(CurrentUser.Id);

            RefundQuery query = new RefundQuery()
            {
                UserId = CurrentUser.Id,
                PageNo = 1,
                PageSize = int.MaxValue
            };
            var refundPage = _iRefundService.GetOrderRefunds(query);
            DateTime endsrtime = DateTime.Now.Date;
            if (CurrentSiteSetting.SalesReturnTimeout > 0)
            {
                endsrtime = endsrtime.AddDays(-CurrentSiteSetting.SalesReturnTimeout).Date;
            }
            model.RefundOrders = refundPage.Models.Where(e => e.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm
                || (e.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.UnAudit && e.OrderItemInfo.OrderInfo.FinishDate >= endsrtime)).Count();
            var capital = _iMemberCapitalService.GetCapitalInfo(CurrentUser.Id);
            decimal cap;
            if (capital != null)
            {
                cap = capital.Balance.HasValue ? capital.Balance.Value : 0;
            }
            else
            {
                cap = 0;
            }
            model.Capital = cap;
            var CouponsCount = _iCouponService.GetAllUserCoupon(CurrentUser.Id).Count();
            CouponsCount += _iShopBonusService.GetCanUseDetailByUserId(CurrentUser.Id).Count();
            model.CouponsCount = CouponsCount;

            var userInte = MemberIntegralApplication.GetMemberIntegral(UserId);
            var userGrade = MemberGradeApplication.GetMemberGradeByUserIntegral(userInte.HistoryIntegrals);
            model.GradeName = userGrade.GradeName;
            model.MemberAvailableIntegrals = userInte.AvailableIntegrals;
            model.CollectionShop = _iVShopService.GetUserConcernVShops(CurrentUser.Id, 1, int.MaxValue).Count();

            model.CanSignIn = _iMemberSignInService.CanSignInByToday(CurrentUser.Id);
            model.SignInIsEnable = _iMemberSignInService.GetConfig().IsEnable;
            model.userMemberInfo = CurrentUser;
            return View(model);
        }

        public ActionResult ShippingAddress()
        {
            return View();
        }

        #region 订单相关处理
        public ActionResult Orders(int? orderStatus)
        {
            //判断是否需要跳转到支付地址
            if (this.Request.Url.AbsolutePath.EndsWith("/member/orders", StringComparison.OrdinalIgnoreCase) && (orderStatus == null || orderStatus == 0 || orderStatus == 1))
            {
                return Redirect(Url.RouteUrl("PayRoute") + "?area=mobile&platform=" + this.PlatformType.ToString() + "&controller=member&action=orders&orderStatus=" + orderStatus);
            }

            var orders = _iOrderService.GetTopOrders(int.MaxValue, CurrentUser.Id);

            //待评价
            var queryModel = new OrderQuery()
            {
                Status = Model.OrderInfo.OrderOperateStatus.Finish,
                UserId = CurrentUser.Id,
                PageSize = int.MaxValue,
                PageNo = 1,
                Commented = false
            };
            ViewBag.WaitingForComments = _iOrderService.GetOrders<OrderInfo>(queryModel).Total;


            var member = _iMemberService.GetMember(CurrentUser.Id);
            ViewBag.AllOrders = orders.Count();
            ViewBag.WaitingForRecieve = orders.Count(item => item.UserId == CurrentUser.Id && (item.OrderStatus == Model.OrderInfo.OrderOperateStatus.WaitReceiving || item.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp));//获取待收货订单数
            ViewBag.WaitingForPay = orders.Count(item => item.OrderStatus == Model.OrderInfo.OrderOperateStatus.WaitPay);//获取待支付订单数
            var waitdelordnum = orders.Count(item => item.OrderStatus == Model.OrderInfo.OrderOperateStatus.WaitDelivery);//获取待发货订单数
            var fgwaitdelordnum = _iOrderService.GetFightGroupOrderByUser(CurrentUser.Id);
            ViewBag.WaitingForDelivery = waitdelordnum - fgwaitdelordnum;
            return View();
        }

        public ActionResult PaymentToOrders(string ids)
        {
            //红包数据
            Dictionary<long, ShopBonusInfo> bonusGrantIds = new Dictionary<long, ShopBonusInfo>();
            string url = "http://" + Request.Url.Host.ToString() + "/m-weixin/shopbonus/index/";
            if (!string.IsNullOrEmpty(ids))
            {
                string[] strIds = ids.Split(',');
                List<long> longIds = new List<long>();
                foreach (string id in strIds)
                {
                    longIds.Add(long.Parse(id));
                }
                var result = PaymentHelper.GenerateBonus(longIds, Request.Url.Host.ToString());
                foreach (var item in result)
                {
                    bonusGrantIds.Add(item.Key, item.Value);
                }

            }

            ViewBag.Path = url;
            ViewBag.BonusGrantIds = bonusGrantIds;
            ViewBag.BaseAddress = "http://" + Request.Url.Host.ToString();

            var orders = _iOrderService.GetTopOrders(int.MaxValue, CurrentUser.Id);
            //待评价
            var queryModel = new OrderQuery()
            {
                Status = Model.OrderInfo.OrderOperateStatus.Finish,
                UserId = CurrentUser.Id,
                PageSize = int.MaxValue,
                PageNo = 1,
                Commented = false
            };
            ViewBag.WaitingForComments = _iOrderService.GetOrders<OrderInfo>(queryModel).Total;

            var member = _iMemberService.GetMember(CurrentUser.Id);
            ViewBag.AllOrders = orders.Count();
            ViewBag.WaitingForRecieve = orders.Count(item => item.OrderStatus == Model.OrderInfo.OrderOperateStatus.WaitReceiving);//获取待收货订单数
            ViewBag.WaitingForPay = orders.Count(item => item.OrderStatus == Model.OrderInfo.OrderOperateStatus.WaitPay);//获取待支付订单数
            var waitdelordnum = orders.Count(item => item.OrderStatus == Model.OrderInfo.OrderOperateStatus.WaitDelivery);//获取待发货订单数
            var fgwaitdelordnum = _iOrderService.GetFightGroupOrderByUser(CurrentUser.Id);
            ViewBag.WaitingForDelivery = waitdelordnum - fgwaitdelordnum;
            if (orders != null)
            {
                if (orders.Count() > 0)
                {
                    var order = orders.FirstOrDefault();
                    if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
                    {
                        var gpord = FightGroupApplication.GetOrder(order.Id);
                        if (gpord != null)
                        {
                            return Redirect(string.Format("/m-{0}/FightGroup/GroupOrderOk?orderid={1}", PlatformType.ToString(), order.Id));
                        }
                    }
                }
            }
            return View("~/Areas/Mobile/Templates/Default/Views/Member/Orders.cshtml");
        }

        public JsonResult GetUserOrders(int? orderStatus, int pageNo, int pageSize = 8)
        {
            if (orderStatus.HasValue && orderStatus == 0)
            {
                orderStatus = null;
            }
            var queryModel = new OrderQuery()
            {
                Status = (Model.OrderInfo.OrderOperateStatus?)orderStatus,
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
            if (orderStatus.GetValueOrDefault() == (int)Model.OrderInfo.OrderOperateStatus.Finish)
                queryModel.Commented = false;//只查询未评价的订单

            var orders = OrderApplication.GetOrders(queryModel);
            var orderItems = OrderApplication.GetOrderItemsByOrderId(orders.Models.Select(p => p.Id));
            var orderComments = OrderApplication.GetOrderCommentCount(orders.Models.Select(p => p.Id));
            var orderRefunds = OrderApplication.GetOrderRefunds(orderItems.Select(p => p.Id));
            var products = ProductManagerApplication.GetProductsByIds(orderItems.Select(p => p.ProductId));
            var vshops = VshopApplication.GetVShopsByShopIds(products.Select(p => p.ShopId));

            var result = orders.Models.Select(item =>
            {
                var _ordrefobj = _iRefundService.GetOrderRefundByOrderId(item.Id) ?? new OrderRefundInfo { Id = 0 };
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
                    shopname = item.ShopName,
                    orderTotalAmount = item.OrderTotalAmount.ToString("F2"),
                    productCount = item.OrderProductQuantity,
                    commentCount = orderComments.ContainsKey(item.Id) ? orderComments[item.Id] : 0,
                    PaymentType = item.PaymentType,
                    RefundStats = ordrefstate,
                    OrderRefundId = _ordrefobj.Id,
                    OrderType = item.OrderType,
                    PickUp = item.PickupCode,
                    ShopBranchId = item.ShopBranchId,
                    DeliveryType = item.DeliveryType,
                    ShipOrderNumber = item.ShipOrderNumber,
                    EnabledRefundAmount = item.OrderEnabledRefundAmount,
                    itemInfo = orderItems.Where(oi => oi.OrderId == item.Id).Select(a =>
                            {
                                var prodata = products.FirstOrDefault(p => p.Id == a.ProductId);
                                VShop vshop = null;
                                if (prodata != null)
                                    vshop = vshops.FirstOrDefault(vs => vs.ShopId == prodata.ShopId);
                                if (vshop == null)
                                    vshop = new VShop { Id = 0 };

                                var itemrefund = orderRefunds.Where(or => or.OrderItemId == a.Id).FirstOrDefault(d => d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                                int? itemrefstate = (itemrefund == null ? null : (int?)itemrefund.SellerAuditStatus);
                                itemrefstate = (itemrefstate > 4 ? (int?)itemrefund.ManagerConfirmStatus : itemrefstate);
                                return new
                                {
                                    itemid = a.Id,
                                    productId = a.ProductId,
                                    productName = a.ProductName,
                                    image = HimallIO.GetProductSizeImage(a.ThumbnailsUrl, 1, (int)ImageSize.Size_100),
                                    count = a.Quantity,
                                    price = a.SalePrice,
                                    Unit = prodata == null ? "" : prodata.MeasureUnit,
                                    vshopid = vshop.Id,
                                    color = a.Color,
                                    size = a.Size,
                                    version = a.Version,
                                    RefundStats = itemrefstate,
                                    OrderRefundId = (itemrefund == null ? 0 : itemrefund.Id),
                                    EnabledRefundAmount = a.EnabledRefundAmount
                            };
                            }),
                    HasAppendComment = HasAppendComment(orderItems.Where(oi => oi.OrderId == item.Id).FirstOrDefault()),
                    CanRefund = (item.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.WaitDelivery || item.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                    && !item.RefundStats.HasValue && item.PaymentType != Himall.Model.OrderInfo.PaymentTypes.CashOnDelivery && item.PaymentType != Himall.Model.OrderInfo.PaymentTypes.None
                    && (item.FightGroupCanRefund == null || item.FightGroupCanRefund == true) && ordrefstate.GetValueOrDefault().Equals(0)
                };
            });

            foreach (var item in result)
            {
                var refund = item.itemInfo.Any(p => p.OrderRefundId > 0);
                //if (!refund)
                //item.CanRefund = false;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PickupGoods(long id)
        {
            var orderInfo = OrderApplication.GetOrder(id);
            if (orderInfo == null)
                throw new HimallException("订单不存在！");
            if (orderInfo.UserId != CurrentUser.Id)
                throw new HimallException("只能查看自己的提货码！");

            AutoMapper.Mapper.CreateMap<Order, Himall.DTO.OrderListModel>();
            AutoMapper.Mapper.CreateMap<DTO.OrderItem, OrderItemListModel>();
            var orderModel = AutoMapper.Mapper.Map<Order, Himall.DTO.OrderListModel>(orderInfo);
            var orderItems = OrderApplication.GetOrderItemsByOrderId(orderInfo.Id);
            orderModel.OrderItemList = AutoMapper.Mapper.Map<List<DTO.OrderItem>, List<OrderItemListModel>>(orderItems);
            if (orderInfo.ShopBranchId.HasValue && orderInfo.ShopBranchId.Value != 0)
            {//补充数据
                var branch = ShopBranchApplication.GetShopBranchById(orderInfo.ShopBranchId.Value);
                orderModel.ShopBranchName = branch.ShopBranchName;
                orderModel.ShopBranchAddress = branch.AddressFullName;
                orderModel.ShopBranchContactPhone = branch.ContactPhone;
            }

            return View(orderModel);
        }
        #endregion 订单相关处理
        public ActionResult CollectionProduct()
        {
            return View();
        }
        private bool HasAppendComment(DTO.OrderItem orderItem)
        {
            var result = _iCommentService.HasAppendComment(orderItem.Id);
            return result;
        }
        public ActionResult CollectionShop()
        {
            ViewBag.SiteName = CurrentSiteSetting.SiteName;
            return View();
        }

        public ActionResult ChangeLoginPwd()
        {
            return View(CurrentUser);
        }

        /// <summary>
        /// 修改支付密码
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangePayPwd()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePayPwd(ChangePayPwd model)
        {
            if (string.IsNullOrEmpty(model.NewPayPwd))
                return Json(new { success = false, msg = "请输入新支付密码" });

            if (!string.IsNullOrEmpty(model.OldPayPwd))
            {
                var success = MemberApplication.VerificationPayPwd(CurrentUser.Id, model.OldPayPwd);
                if (!success)
                    return Json(new { success = false, msg = "原支付密码输入不正确" });
                MemberApplication.ChangePayPassword(CurrentUser.Id, model.NewPayPwd);
            }
            else if (!string.IsNullOrEmpty(model.PhoneCode))
            {
                var codeCacheKey = CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, model.SendCodePluginId);
                var codeCache = Core.Cache.Get(codeCacheKey);
                if (codeCache == null)
                    return Json(new { success = false, msg = "验证码已过期" });

                if (!string.Equals(codeCache.ToString(), model.PhoneCode, StringComparison.OrdinalIgnoreCase))
                    return Json(new { success = false, msg = "验证码不正确" });

                MemberApplication.ChangePayPassword(CurrentUser.Id, model.NewPayPwd);
            }
            else
                return Json(new { success = false });
            return Json(new { success = true });
        }

        public JsonResult GetUserCollectionProduct(int pageNo, int pageSize = 16)
        {
            var model = _iProductService.GetUserConcernProducts(CurrentUser.Id, pageNo, pageSize);
            var result = model.Models.ToArray().Select(item => new
            {
                Id = item.ProductId,
                Image = item.ProductInfo.GetImage(ImageSize.Size_220),
                ProductName = item.ProductInfo.ProductName,
                SalePrice = item.ProductInfo.MinSalePrice.ToString("F2"),
                Evaluation = item.ProductInfo.Himall_ProductComments.Count()
            });
            return Json(result);
        }

        public JsonResult GetUserCollectionShop(int pageNo, int pageSize = 8)
        {
            var model = _iVShopService.GetUserConcernVShops(CurrentUser.Id, pageNo, pageSize);
            var result = model.ToArray().Select(item => new
            {
                Id = item.Id,
                Logo = HimallIO.GetImagePath(item.Logo),
                Name = item.Name
            });
            return Json(result);
        }

        [HttpPost]
        public JsonResult AddShippingAddress(ShippingAddressInfo info)
        {
            info.UserId = CurrentUser.Id;
            _iShippingAddressService.AddShippingAddress(info);
            return Json(new
            {
                Success = true,
                Msg = "添加成功",
                RegionFullName = RegionApplication.GetFullName(info.RegionId),
                Id = info.Id
            }, true);
        }

        [HttpPost]
        public JsonResult DeleteShippingAddress(long id)
        {
            var userId = CurrentUser.Id;
            _iShippingAddressService.DeleteShippingAddress(id, userId);
            return Json(new Result() { success = true, msg = "删除成功" });
        }

        [HttpPost]
        public JsonResult EditShippingAddress(ShippingAddressInfo info)
        {
            info.UserId = CurrentUser.Id;
            _iShippingAddressService.UpdateShippingAddress(info);
            return Json(new
            {
                Success = true,
                RegionFullName = RegionApplication.GetFullName(info.RegionId),
                Msg = "修改成功"
            }, true);
        }

        [HttpPost]
        public JsonResult ChangePassword(string oldpassword, string password)
        {
            if (string.IsNullOrWhiteSpace(oldpassword) || string.IsNullOrWhiteSpace(password))
            {
                return Json(new Result() { success = false, msg = "密码不能为空！" });
            }
            var model = CurrentUser;
            var pwd = SecureHelper.MD5(SecureHelper.MD5(oldpassword) + model.PasswordSalt);
            bool CanChange = false;
            if (pwd == model.Password)
            {
                CanChange = true;
            }
            if (model.PasswordSalt.StartsWith("o"))
            {
                CanChange = true;
            }
            if (CanChange)
            {
                _iMemberService.ChangePassword(model.Id, password);
                return Json(new Result() { success = true, msg = "修改成功" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "旧密码错误" });
            }
        }

        public ActionResult AccountManagement()
        {
            return View();
        }

        public ActionResult AccountSecure()
        {
            return View(CurrentUser);
        }

        public ActionResult BindPhone()
        {
            return View(CurrentUser);
        }

        [HttpPost]
        public JsonResult SendCode(string pluginId, string destination = null)
        {
            if (string.IsNullOrEmpty(destination))
                destination = CurrentUser.CellPhone;

            if (string.IsNullOrEmpty(destination))
                return Json(new { success = false, msg = "请先绑定手机" });

            _iMemberService.CheckContactInfoHasBeenUsed(pluginId, destination);
            var timeout = CacheKeyCollection.MemberPluginCheckTime(CurrentUser.UserName, pluginId);
            if (Core.Cache.Get(timeout) != null)
            {
                return Json(new Result() { success = false, msg = "120秒内只允许请求一次，请稍后重试!" });
            }
            var checkCode = new Random().Next(10000, 99999);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            if (pluginId.ToLower().Contains("email"))
            {
                cacheTimeout = DateTime.Now.AddHours(24);
            }
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId), checkCode, cacheTimeout);
            var user = new Himall.Core.Plugins.Message.MessageUserInfo() { UserName = CurrentUser.UserName, SiteName = CurrentSiteSetting.SiteName, CheckCode = checkCode.ToString() };
            _iMessageService.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheckTime(CurrentUser.UserName, pluginId), "0", DateTime.Now.AddSeconds(120));
            return Json(new Result() { success = true, msg = "发送成功" });
        }

        [HttpPost]
        public JsonResult SendFindCode(string pluginId, string destination = null)
        {
            if (string.IsNullOrEmpty(destination))
                destination = CurrentUser.CellPhone;

            if (string.IsNullOrEmpty(destination))
                return Json(new { success = false, msg = "请先绑定手机" });

            var timeout = CacheKeyCollection.MemberPluginCheckTime(CurrentUser.UserName, pluginId);
            if (Core.Cache.Get(timeout) != null)
            {
                return Json(new Result() { success = false, msg = "120秒内只允许请求一次，请稍后重试!" });
            }
            var checkCode = new Random().Next(10000, 99999);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            if (pluginId.ToLower().Contains("email"))
            {
                cacheTimeout = DateTime.Now.AddHours(24);
            }
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId), checkCode, cacheTimeout);
            var user = new Himall.Core.Plugins.Message.MessageUserInfo() { UserName = CurrentUser.UserName, SiteName = CurrentSiteSetting.SiteName, CheckCode = checkCode.ToString() };
            _iMessageService.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheckTime(CurrentUser.UserName, pluginId), "0", DateTime.Now.AddSeconds(120));
            return Json(new Result() { success = true, msg = "发送成功" });
        }



        [HttpPost]
        public JsonResult CheckCode(string pluginId, string code, string destination)
        {
            var cache = CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId);
            var cacheCode = Core.Cache.Get(cache);
            var member = CurrentUser;
            var mark = "";
            if (cacheCode != null && cacheCode.ToString() == code)
            {
                var service = _iMessageService;
                if (service.GetMemberContactsInfo(pluginId, destination, MemberContactsInfo.UserTypes.General) != null)
                {
                    return Json(new Result() { success = false, msg = destination + "已经绑定过了！" });
                }
                if (pluginId.ToLower().Contains("email"))
                {
                    member.Email = destination;
                    mark = "邮箱";
                }
                else if (pluginId.ToLower().Contains("sms"))
                {
                    member.CellPhone = destination;
                    mark = "手机";
                }

                _iMemberService.UpdateMember(member);
                service.UpdateMemberContacts(new Model.MemberContactsInfo() { Contact = destination, ServiceProvider = pluginId, UserId = CurrentUser.Id, UserType = MemberContactsInfo.UserTypes.General });
                Core.Cache.Remove(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId));
                Core.Cache.Remove(CacheKeyCollection.Member(CurrentUser.Id));//移除用户缓存
                Core.Cache.Remove("Rebind" + CurrentUser.Id);

                MemberIntegralRecord info = new MemberIntegralRecord();
                info.UserName = member.UserName;
                info.MemberId = member.Id;
                info.RecordDate = DateTime.Now;
                info.TypeId = MemberIntegral.IntegralType.Reg;
                info.ReMark = "绑定" + mark;
                var memberIntegral = ServiceHelper.Create<IMemberIntegralConversionFactoryService>().Create(MemberIntegral.IntegralType.Reg);
                ServiceHelper.Create<IMemberIntegralService>().AddMemberIntegral(info, memberIntegral);

                if (member.InviteUserId.HasValue)
                {
                    var inviteMember = _iMemberService.GetMember(member.InviteUserId.Value);
                    if (inviteMember != null)
                        ServiceHelper.Create<IMemberInviteService>().AddInviteIntegel(member, inviteMember);
                }

                return Json(new Result() { success = true, msg = "验证正确" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "验证码不正确或者已经超时" });
            }
        }
    }
}