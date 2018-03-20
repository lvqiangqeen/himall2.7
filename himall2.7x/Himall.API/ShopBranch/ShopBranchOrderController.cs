using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Application;
using Himall.IServices;
using Himall.Core;
using Himall.DTO;
using System.Web;
using Himall.API.Model.ParamsModel;

namespace Himall.API
{
    /// <summary>
    /// 门店订单类
    /// </summary>
    public class ShopBranchOrderController : BaseShopBranchApiController
    {
        /// <summary>
        /// 根据提货码取订单
        /// </summary>
        /// <param name="pickcode"></param>
        /// <returns></returns>
        public object GetShopBranchOrder(string pickcode)
        {
            CheckUserLogin();
            var order = Application.OrderApplication.GetOrderByPickCode(pickcode);

            if (order == null)
                return Json(new { success = false, msg = "该提货码无效" });
            if (order.ShopBranchId.Value != CurrentShopBranch.Id)
                return Json(new { success = false, msg = "非本门店提货码，请买家核对提货信息" });
            if (order.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.Finish && order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake)
                return Json(new { success = false, msg = "该提货码于" + order.FinishDate.ToString() + "已核销" });

            var orderItem = Application.OrderApplication.GetOrderItemsByOrderId(order.Id);
            foreach (var item in orderItem)
            {
                item.ThumbnailsUrl = Core.HimallIO.GetRomoteProductSizeImage(item.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_100);
                ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetTypeByProductId(item.ProductId);
                item.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                item.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                item.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            }
            //退款状态
            var refundobjs = OrderApplication.GetOrderRefunds(orderItem.Select(e => e.Id));
            //小于4表示商家未确认；与平台未审核，都算退款、退货中
            var refundProcessing = refundobjs.Where(e => (int)e.SellerAuditStatus < 4 || e.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm);
            if (refundProcessing.Count() > 0)
                order.RefundStats = 1;

            return Json(new { success = true, order = order, orderItem = orderItem });
        }
        /// <summary>
        /// 门店核销订单
        /// </summary>
        /// <param name="pickcode"></param>
        /// <returns></returns>
        public object GetShopBranchOrderConfirm(string pickcode)
        {
            CheckUserLogin();
            var order = Application.OrderApplication.GetOrderByPickCode(pickcode);

            if (order == null)
                return Json(new { success = false, msg = "该提货码无效" });
            if (order.ShopBranchId.Value != CurrentShopBranch.Id)
                return Json(new { success = false, msg = "非本门店提货码，请买家核对提货信息" });
            if (order.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.Finish && order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake)
                return Json(new { success = false, msg = "该提货码于" + order.FinishDate.ToString() + "已核销" });
            if (order.OrderStatus != Himall.Model.OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                return Json(new { success = false, msg = "只有待自提的订单才能进行核销" });

            Application.OrderApplication.ShopBranchConfirmOrder(order.Id, CurrentShopBranch.Id, this.CurrentUser.UserName);

            return Json(new { success = true, msg = "已核销" });
        }

        /// <summary>
        /// 搜索门店订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<DTO.FullOrder> PostSearchShopBranchOrder(OrderQuery query)
        {
            if (query.PageNo < 1)
                query.PageNo = 1;
            if (query.PageSize < 1)
                query.PageSize = 10;

            CheckUserLogin();

            query.ShopBranchId = CurrentShopBranch.Id;

            var status = new[] {
                OrderInfo.OrderOperateStatus.WaitPay,
                 OrderInfo.OrderOperateStatus.WaitDelivery,
                  OrderInfo.OrderOperateStatus.WaitReceiving,
                OrderInfo.OrderOperateStatus.WaitSelfPickUp,
                OrderInfo.OrderOperateStatus.Finish,
                OrderInfo.OrderOperateStatus.Close
            };
            if (query.Status == null || !status.Contains(query.Status.Value))//门店只能查询这几种状态的订单
                query.Status = OrderInfo.OrderOperateStatus.WaitSelfPickUp;

            var data = OrderApplication.GetFullOrders(query);

            return data.Models;
        }

        public object GetShopBranchOrderCount()
        {
            CheckUserLogin();
            long shopid = CurrentShopBranch.ShopId;
            long sbid = CurrentUser.ShopBranchId;

            var waitPayCount = OrderApplication.GetWaitingForPayOrders(shopId: shopid, shopBranchId: sbid);
            var waitSelfPickUp = OrderApplication.GetWaitingForSelfPickUp(shopid, sbid);
            var waitReceive = OrderApplication.GetWaitingForReceive(shopid, sbid);
            var waitDelivery = OrderApplication.GetWaitingForDelivery(shopid, sbid);
            return Json(new { success = true, waitPayCount = waitPayCount, waitReceive = waitReceive, waitDelivery = waitDelivery, waitSelfPickUp = waitSelfPickUp });
        }

        /// <summary>
        /// 获取订单信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public object GetOrderInfo(long orderId)
        {
            //CheckUserLogin();
            return Json(Application.OrderApplication.GetOrder(orderId));
        }
        /// <summary>
        /// 获取所有快递公司名称
        /// </summary>
        /// <returns></returns>
        public string GetAllExpress()
        {
            //CheckUserLogin();
            var listData = OrderApplication.GetAllExpress().Select(i => i.Name);
            return String.Join(",", listData);
        }
        /// <summary>
        /// 门店发货
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public object PostShopSendGood(OrderDeliveryModel model)
        {
            CheckUserLogin();
            string shopkeeperName = "";
            shopkeeperName = CurrentShopBranch.UserName;
            var returnurl = String.Format("http://{0}/Common/ExpressData/SaveExpressData", Request.RequestUri.Authority);
            OrderApplication.ShopSendGood(model.orderId, model.deliveryType, shopkeeperName, model.companyName, model.shipOrderNumber, returnurl);
            return SuccessResult("发货成功");
        }

        /// <summary>
        /// 订单是否正在申请售后
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public object GetIsOrderAfterService(long orderId)
        {
            //CheckUserLogin();
            bool isAfterService = Application.OrderApplication.IsOrderAfterService(orderId);
            if (isAfterService)
            {
                return Json(new { isAfterService = true });
            }
            else
            {
                return Json(new { isAfterService = false });
            }
        }

        /// <summary>
        /// 查看订单物流
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public object GetLogisticsData(string expressCompanyName, string shipOrderNumber)
        {
            //CheckUserLogin();
            var expressData = Application.OrderApplication.GetExpressData(expressCompanyName, shipOrderNumber);
            if (expressData != null)
            {
                if (expressData.Success)
                    expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列
                var json = new
                {
                    success = expressData.Success,
                    msg = expressData.Message,
                    data = expressData.ExpressDataItems.Select(item => new
                    {
                        time = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                        content = item.Content
                    })
                };
                return Json(json);
            }
            else
            {
                var json = new
                {
                    success = false,
                    msg = "无物流信息"
                };
                return Json(json);
            }
        }

        public object GetOrderDetail(long id)
        {
            CheckUserLogin();
            long shopid = CurrentShopBranch.ShopId;
            long sbid = CurrentUser.ShopBranchId;

            var ordser = ServiceProvider.Instance<IOrderService>.Create;

            OrderInfo order = ordser.GetOrder(id);
            if (order == null || order.ShopBranchId != sbid)
            {
                throw new HimallApiException("错误的订单编号");
            }
            var bonusService = ServiceProvider.Instance<IShopBonusService>.Create;
            var orderRefundService = ServiceProvider.Instance<IRefundService>.Create;
            var shopService = ServiceProvider.Instance<IShopService>.Create;
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var vshop = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(order.ShopId);
            bool isCanApply = false;
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
                        VersionAlias = versionAlias
                    };
                })
            };
            var orderModel = new
            {
                Id = order.Id,
                OrderType = order.OrderType,
                OrderTypeName = order.OrderType.ToDescription(),
                Status = order.OrderStatus.ToDescription(),
                ShipTo = order.ShipTo,
                Phone = order.CellPhone,
                Address = order.RegionFullName + " " + order.Address,
                HasExpressStatus = !string.IsNullOrWhiteSpace(order.ShipOrderNumber),
                ExpressCompanyName = order.ExpressCompanyName,
                Freight = order.Freight,
                IntegralDiscount = order.IntegralDiscount,
                RealTotalAmount = order.OrderTotalAmount - order.RefundTotalAmount,
                OrderDate = order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                ShopName = order.ShopName,
                ShopBranchName = CurrentShopBranch.ShopBranchName,
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
                OrderRemarks = order.OrderRemarks,
                DeliveryType=order.DeliveryType
            };
            return Json(new { Success = "true", Order = orderModel, OrderItem = orderDetail.OrderItems });
        }
    }
}
