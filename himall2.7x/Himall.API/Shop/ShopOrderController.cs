using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Himall.IServices.QueryModel;
using Himall.Application;
using Himall.Model;
using Himall.IServices;
using System.Web;
using Himall.Core;

namespace Himall.API
{
    public class ShopOrderController : BaseShopApiController
    {
        /// <summary>
        /// 搜索门店订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public List<DTO.FullOrder> PostSearchShopOrder(OrderQuery query)
        {
            if (query.PageNo < 1)
                query.PageNo = 1;
            if (query.PageSize < 1)
                query.PageSize = 10;

            CheckShopManageLogin();

            query.ShopId = CurrentShop.Id;

            var data = Application.OrderApplication.GetFullOrders(query);


            return data.Models;
        }

        public object GetShopBranchs()
        {
            CheckUserLogin();
            var branchs = ShopBranchApplication.GetShopBranchByShopId(CurrentUser.ShopId);
            return Json(new { success = true, branchs = branchs });
        }

        public object GetShopOrderCount()
        {
            CheckUserLogin();
            var waitPayCount = OrderApplication.GetWaitingForPayOrders(CurrentUser.ShopId);
            var waitReceive = OrderApplication.GetWaitingForReceive(CurrentUser.ShopId);
            var waitDelivery = OrderApplication.GetWaitingForDelivery(CurrentUser.ShopId);
            var waitSelfPickUp = OrderApplication.GetWaitingForSelfPickUp(CurrentUser.ShopId);
            return Json(new { success = true, waitPayCount = waitPayCount, waitReceive = waitReceive, waitDelivery = waitDelivery, waitSelfPickUp = waitSelfPickUp });
        }

        public object GetOrderDetail(long id)
        {
            CheckUserLogin();
            long shopid = CurrentUser.ShopId;

            var ordser = ServiceProvider.Instance<IOrderService>.Create;

            OrderInfo order = ordser.GetOrder(id);
            if (order == null || order.ShopId != shopid)
            {
                throw new HimallApiException("错误的订单编号");
            }
            var bonusService = ServiceProvider.Instance<IShopBonusService>.Create;
            var orderRefundService = ServiceProvider.Instance<IRefundService>.Create;
            var shopService = ServiceProvider.Instance<IShopService>.Create;
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var vshop = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(order.ShopId);
            bool isCanApply = false;
            DTO.ShopBranch ShopBranchInfo = null;
            if (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0)
            {
                ShopBranchInfo = ShopBranchApplication.GetShopBranchById(order.ShopBranchId.Value);
            }
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
                ShopBranchName = (ShopBranchInfo != null ? ShopBranchInfo.ShopBranchName : ""),
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
