using Himall.Application;
using Himall.Core;
using Himall.Core.Plugins;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.DTO;
using Himall.Web.App_Code.Common;
using Himall.Web.Areas.Admin.Models.Product;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace Himall.Web.Areas.Web.Controllers
{
    //TODO:YZY
    public class UserOrderController : BaseMemberController
    {
        private IShopBonusService _iShopBonusService;
        private ICashDepositsService _iCashDepositsService;
        private ISiteSettingService _iSiteSettingService;
        private IOrderService _iOrderService;
        private IRefundService _iRefundService;
        private ICustomerService _iCustomerService;
        private IProductService _iProductService;
        private ICouponService _iCouponService;
        private ICommentService _iCommentService;
        private IFightGroupService _iFightGroupService;
        private ITypeService _iTypeService;
        public UserOrderController(
            IShopBonusService iShopBonusService,
            ICashDepositsService iCashDepositsService,
            ISiteSettingService iSiteSettingService,
            IOrderService iOrderService,
            IRefundService iRefundService,
            ICustomerService iCustomerService,
            IProductService iProductService,
            ICouponService iCouponService,
            ICommentService iCommentService,
            IFightGroupService iFightGroupService, ITypeService iTypeService
            )
        {
            _iShopBonusService = iShopBonusService;
            _iCashDepositsService = iCashDepositsService;
            _iSiteSettingService = iSiteSettingService;
            _iOrderService = iOrderService;
            _iRefundService = iRefundService;
            _iCustomerService = iCustomerService;
            _iProductService = iProductService;
            _iCouponService = iCouponService;
            _iCommentService = iCommentService;
            _iFightGroupService = iFightGroupService;
            _iTypeService = iTypeService;
        }

        // GET: Web/UserOrder
        [ValidateInput(false)]
        public ActionResult Index(string orderDate, string keywords, string orderids, DateTime? startDateTime, DateTime? endDateTime, int? orderStatus, int pageNo = 1, int pageSize = 10)
        {
            ViewBag.Grant = null;

            if (!string.IsNullOrEmpty(orderids) && orderids.IndexOf(',') <= 0)
            {
                ViewBag.Grant = _iShopBonusService.GetByOrderId(long.Parse(orderids));
            }

            DateTime? startDate = startDateTime;
            DateTime? endDate = endDateTime;
            if (!string.IsNullOrEmpty(orderDate) && orderDate.ToLower() != "all")
            {
                switch (orderDate.ToLower())
                {
                    case "threemonth":
                        startDate = DateTime.Now.AddMonths(-3);
                        break;
                    case "halfyear":
                        startDate = DateTime.Now.AddMonths(-6);
                        break;
                    case "year":
                        startDate = DateTime.Now.AddYears(-1);
                        break;
                    case "yearago":
                        endDate = DateTime.Now.AddYears(-1);
                        break;
                }
            }

            if (orderStatus.HasValue && orderStatus == 0)
            {
                orderStatus = null;
            }

            var queryModel = new OrderQuery()
            {
                StartDate = startDate,
                EndDate = endDate,
                Status = (Model.OrderInfo.OrderOperateStatus?)orderStatus,
                UserId = CurrentUser.Id,
                SearchKeyWords = keywords,
                PageSize = pageSize,
                PageNo = pageNo
            };

            var orders = OrderApplication.GetOrders(queryModel);
			var orderComments=OrderApplication.GetOrderCommentCount(orders.Models.Select(p => p.Id));
			var orderItems = OrderApplication.GetOrderItemsByOrderId(orders.Models.Select(p => p.Id));
			var orderRefunds=OrderApplication.GetOrderRefunds(orderItems.Select(p => p.Id));

            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = orders.Total
            };
            ViewBag.pageInfo = info;
            ViewBag.UserId = CurrentUser.Id;
            var siteSetting = _iSiteSettingService.GetSiteSettings();
            var shopBonus = _iShopBonusService;
            ViewBag.SalesRefundTimeout = siteSetting.SalesReturnTimeout;

            var cashDepositsService = _iCashDepositsService;
            //IEnumerable<OrderListModel> orderList = new IEnumerable<OrderListModel>();
            IEnumerable<OrderListModel> orderList = orders.Models.Select(item => new OrderListModel
            {
                Id = item.Id,
                ActiveType = item.ActiveType,
                OrderType = item.OrderType,
                Address = item.Address,
                CellPhone = item.CellPhone,
                CloseReason = item.CloseReason,
                CommisTotalAmount = item.CommisAmount,
                DiscountAmount = item.DiscountAmount,
                ExpressCompanyName = item.ExpressCompanyName,
                FinishDate = item.FinishDate,
                Freight = item.Freight,
                GatewayOrderId = item.GatewayOrderId,
                IntegralDiscount = item.IntegralDiscount,
                UserId = item.UserId,
                ShopId = item.ShopId,
                ShopName = item.ShopName,
                ShipTo = item.ShipTo,
                OrderTotalAmount = item.OrderTotalAmount,
                PaymentTypeName = item.PaymentTypeName,
                //满额减
                FullDiscount = item.FullDiscount,
                OrderStatus = item.OrderStatus,
                RefundStats = item.RefundStats,
				CommentCount = orderComments.ContainsKey(item.Id)?orderComments[item.Id]:0,
                OrderDate = item.OrderDate,
                PaymentType = item.PaymentType,
                PickupCode=item.PickupCode,
                OrderItemList = orderItems.Where(oi=>oi.OrderId==item.Id).Select(oItem =>
                {
                    var itemrefund = orderRefunds.Where(or=>or.OrderItemId==oItem.Id).FirstOrDefault();
                    var orderItem= new OrderItemListModel
                    {
                        Id = oItem.Id,
                        ProductId = oItem.ProductId,
                        Color = oItem.Color,
                        Size = oItem.Size,
                        Version = oItem.Version,
                        ProductName = oItem.ProductName,
                        ThumbnailsUrl = oItem.ThumbnailsUrl,
                        SalePrice = oItem.SalePrice,
                        SkuId = oItem.SkuId,
                        Quantity = oItem.Quantity,
                        CashDepositsObligation = cashDepositsService.GetCashDepositsObligation(oItem.ProductId),
                    };

					if (itemrefund != null)
					{
						orderItem.RefundStats = itemrefund.RefundStatusValue;
						orderItem.ItemRefundId = itemrefund.Id;

						string showRefundStats = "";
						if (itemrefund.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited)
							showRefundStats = itemrefund.ManagerConfirmStatus.ToDescription();
                        else if (item.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake || (item.ShopBranchId.HasValue && item.ShopBranchId.Value > 0))//如果是自提订单或分配门店订单则转为门店审核状态
                            showRefundStats = ((CommonModel.Enum.OrderRefundShopAuditStatus)itemrefund.SellerAuditStatus).ToDescription();
						else
							showRefundStats = itemrefund.SellerAuditStatus.ToDescription();

						orderItem.ShowRefundStats = showRefundStats;
					}
                    orderItem.EnabledRefundAmount = oItem.EnabledRefundAmount;
                    ProductTypeInfo typeInfo = _iTypeService.GetTypeByProductId(oItem.ProductId);
                    orderItem.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    orderItem.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    orderItem.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

                    return orderItem;
                }).ToList(),
                ReceiveBonus = shopBonus.GetGrantByUserOrder(item.Id, CurrentUser.Id),
            }).ToList();
            foreach (var o in orderList)
            {
                o.HasAppendComment = HasAppendComment(o);
            }

            #region 数据补偿
            List<long> ordidl = orderList.Select(d => d.Id).ToList();
            if (ordidl.Count > 0)
            {
                foreach (var item in orderList)
                {
                    var _ord = orders.Models.FirstOrDefault(o => o.Id == item.Id);

                    item.IsRefundTimeOut = OrderApplication.IsRefundTimeOut(_ord);
                    item.EnabledRefundAmount = _ord.OrderEnabledRefundAmount;
                    //退款状态补偿
                    var _tmpobj = orderRefunds.FirstOrDefault(d => d.OrderId == item.Id && d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund);
					if (_tmpobj != null)
					{
						item.RefundStats = (int)_tmpobj.SellerAuditStatus;
						item.OrderRefundId = _tmpobj.Id;
					}

                    item.OrderCanRefund = false;

                    if (item.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.Finish)
                    {
						if (item.FinishDate.Value.AddDays(siteSetting.SalesReturnTimeout) > DateTime.Now)
							item.OrderCanRefund = true;
                    }
					if (item.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.WaitReceiving)
						item.OrderCanRefund = true;
                    if (item.PaymentType == Himall.Model.OrderInfo.PaymentTypes.CashOnDelivery)
                    {
						if (item.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.Finish)
							item.OrderCanRefund = true;
                    }
					else
						item.OrderCanRefund = true;

                    item.FightGroupCanRefund = true;   //非拼团订单默认可退

                    //拼团状态补偿
                    if (item.OrderType == OrderInfo.OrderTypes.FightGroup)
                    {
                        var fgord = _iFightGroupService.GetFightGroupOrderStatusByOrderId(item.Id);
                        if (fgord != null)
                        {
                            item.FightGroupJoinStatus = fgord.GetJoinStatus;
                            item.FightGroupCanRefund = fgord.CanRefund;
                        }
                        else
                        {
                            item.FightGroupJoinStatus = CommonModel.FightGroupOrderJoinStatus.JoinFailed;
                            item.FightGroupCanRefund = false;
                        }
                        item.OrderCanRefund = item.OrderCanRefund && item.FightGroupCanRefund;
                    }

                }
            }
            #endregion

            return View(orderList.ToList());
        }

        private bool HasAppendComment(OrderListModel list)
        {
            var item = list.OrderItemList.FirstOrDefault();
            var result = _iCommentService.HasAppendComment(item.Id);
            return result;
        }

        private CommentStatus AppendReply(long orderId, long userId)
        {
            var model = _iCommentService.GetProductEvaluationByOrderIdNew(orderId, userId).FirstOrDefault();
            if (model == null)
            {
                return CommentStatus.First;
            }
            else if (model.ReplyAppendTime.HasValue)
            {
                return CommentStatus.Finshed;
            }
            else
            {
                return CommentStatus.Append;
            }
        }

        [ChildActionOnly]
        public ActionResult CustmerServices(long shopId)
        {
			var model = CustomerServiceApplication.GetAfterSaleByShopId(shopId).GroupBy(p => p.Tool).Select(p => p.FirstOrDefault()).ToList();
			var mqcs=CustomerServiceApplication.GetPreSaleByShopId(shopId).FirstOrDefault(p => p.Tool == CustomerServiceInfo.ServiceTool.MeiQia);
			if (mqcs != null)
				model.Insert(0, mqcs);
            return PartialView(model);
        }

        public ActionResult Detail(long id)
        {
            OrderInfo order = _iOrderService.GetOrder(id, CurrentUser.Id);//限制到用户
            //补充商品货号
            IEnumerable<long> proids = order.OrderItemInfo.Select(d => d.ProductId).AsEnumerable();
            var procodelist = _iProductService.GetProductByIds(proids).Select(d => new { d.Id, d.ProductCode, d.FreightTemplateId }).ToList();
            var orderiItems = order.OrderItemInfo.ToList();

            foreach (var item in orderiItems)
            {
                var _tmp = procodelist.Find(d => d.Id == item.ProductId);
                if (_tmp != null)
                {
                    item.ProductCode = _tmp.ProductCode;
                    item.FreightId = _tmp.FreightTemplateId;
                }
            }
            var service = Himall.Web.Framework.ServiceHelper.Create<Himall.IServices.IProductService>();
            var regionService = Himall.Web.Framework.ServiceHelper.Create<Himall.IServices.IRegionService>();
          //  string RegionIdPath = regionService.GetRegionPath(order.RegionId);
            int cityId = regionService.GetRegion(order.RegionId).Id;
            var freightProductGroup = orderiItems.GroupBy(a => a.FreightId);

			if (order.DeliveryType != CommonModel.Enum.DeliveryType.SelfTake)
			{
				foreach (var f in freightProductGroup)
				{
					var productIds = f.Select(a => a.ProductId);
					var counts = f.Select(a => Convert.ToInt32(a.Quantity));
					decimal freight = service.GetFreight(productIds, counts, cityId);

					foreach (var item in f)
					{
						item.Freight = freight;
					}
				}
			}

            ViewBag.freightProductGroup = freightProductGroup;

            ViewBag.Coupon = 0;
            var coupon = _iCouponService.GetCouponRecordInfo(order.UserId, order.Id);
            var bonus = _iShopBonusService.GetUsedPrice(order.Id, order.UserId);
            if (coupon != null)
            {
                ViewBag.Coupon = coupon.Himall_Coupon.Price;
            }
            else if (bonus != null)
            {
                ViewBag.Coupon = bonus;
            }

            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                var fgord = _iFightGroupService.GetFightGroupOrderStatusByOrderId(order.Id);
                order.FightGroupOrderJoinStatus = fgord.GetJoinStatus;
                order.FightGroupCanRefund = fgord.CanRefund;
            }
            //使用OrderListModel
            AutoMapper.Mapper.CreateMap<OrderInfo, OrderListModel>();
            AutoMapper.Mapper.CreateMap<OrderItemInfo, OrderItemListModel>();
            var orderModel = AutoMapper.Mapper.Map<OrderInfo, OrderListModel>(order);
            orderModel.OrderItemList = AutoMapper.Mapper.Map<IEnumerable<OrderItemInfo>, IEnumerable<OrderItemListModel>>(order.OrderItemInfo);
            if (order.ShopBranchId.HasValue && order.ShopBranchId.Value!=0)
            {//补充数据
                var branch = ShopBranchApplication.GetShopBranchById(order.ShopBranchId.Value);
                if (branch != null)
                {
                    orderModel.ShopBranchName = branch.ShopBranchName;
                    orderModel.ShopBranchAddress = branch.AddressFullName;
                    orderModel.ShopBranchContactPhone = branch.ContactPhone;
                }
            }
            if (order.FightGroupOrderJoinStatus.HasValue)
            {
                orderModel.FightGroupJoinStatus = order.FightGroupOrderJoinStatus.Value;
            }
            orderModel.UserRemark = order.OrderRemarks;
            return View(orderModel);
        }
        //TODO YZY快递信息提取到公共的Controller中
        [HttpPost]
        public JsonResult GetExpressData(string expressCompanyName, string shipOrderNumber)
        {
            if (string.IsNullOrWhiteSpace(expressCompanyName) || string.IsNullOrWhiteSpace(shipOrderNumber))
            {
                throw new HimallException("错误的订单信息");
            }
            string kuaidi100Code = ServiceHelper.Create<IExpressService>().GetExpress(expressCompanyName).Kuaidi100Code;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://www.kuaidi100.com/query?type={0}&postid={1}", kuaidi100Code, shipOrderNumber));
            request.Timeout = 8000;

            string content = "暂时没有此快递单号的信息";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream stream = response.GetResponseStream();
                    System.IO.StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.GetEncoding("UTF-8"));

                    // 读取流字符串内容
                    content = streamReader.ReadToEnd();
                    content = content.Replace("&amp;", "");
                    content = content.Replace("&nbsp;", "");
                    content = content.Replace("&", "");
                }
            }
            catch
            {
            }
            return Json(content);
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CloseOrder(long orderId)
        {
            var order = _iOrderService.GetOrder(orderId, CurrentUser.Id);
            if (order != null)
            {
                //if( order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery && order.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery )
                //{
                //    _iOrderService.MemberApplyCloseOrder( orderId , CurrentUser.UserName );
                //}
                //else
                //{
                //    _iOrderService.MemberCloseOrder( orderId , CurrentUser.UserName );
                //    foreach( var item in order.OrderItemInfo )
                //    {
                //        _iProductService.UpdateStock( item.SkuId , item.Quantity );
                //    }
                //}
                _iOrderService.MemberCloseOrder(orderId, CurrentUser.UserName);
                foreach (var item in order.OrderItemInfo)
                {
                    LimitOrderHelper.ReleaseStock(item.SkuId, item.Quantity);
                }
            }
            else
            {
                return Json(new Result() { success = false, msg = "取消失败，该订单已删除或者不属于当前用户！" });
            }
            return Json(new Result() { success = true, msg = "取消成功" });
        }


        /// <summary>
        /// 确认订单收货
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ConfirmOrder(long orderId)
        {
            var status = OrderApplication.ConfirmOrder(orderId, CurrentUser.Id, CurrentUser.UserName);
            Result result = new Result() { status = status };
            switch(status)
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
           // var data = _iOrderService.GetOrder(orderId);
            //确认收货写入结算表
           // OrderApplication.WritePendingSettlnment(data);
            return Json(result);
        }




    }
}