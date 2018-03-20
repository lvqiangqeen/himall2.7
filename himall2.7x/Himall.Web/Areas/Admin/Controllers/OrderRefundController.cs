using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Plugins;
using Himall.Core.Plugins.Payment;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.Admin.Models.Product;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class OrderRefundController : BaseAdminController
    {
        private IRefundService _iRefundService;
        public OrderRefundController(IRefundService iRefundService)
        {
            _iRefundService = iRefundService;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="showtype">0 所有 1 订单退款 2 仅退款(包含订单退款) 3 退货 4 仅退款</param>
        /// <returns></returns>
        public ActionResult Management(int showtype = 0)
        {
            ViewBag.ShowType = showtype;
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="orderId"></param>
        /// <param name="auditStatus"></param>
        /// <param name="shopName"></param>
        /// <param name="ProductName"></param>
        /// <param name="userName"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="showtype">0 所有 1 订单退款 2 仅退款(包含订单退款) 3 退货 4 仅退款</param>
        /// <returns></returns>
        [ValidateInput(false)]
        [HttpPost]
        [UnAuthorize]
        public JsonResult List(DateTime? startDate, DateTime? endDate, long? orderId, int? auditStatus, string shopName, string ProductName, string userName, int page, int rows, int showtype = 0)
        {
            var queryModel = new RefundQuery()
            {
                StartDate = startDate,
                EndDate = endDate,
                OrderId = orderId,
                ProductName = ProductName,
                AuditStatus = (Model.OrderRefundInfo.OrderRefundAuditStatus?)auditStatus,
                ShopName = shopName,
                UserName = userName,
                PageSize = rows,
                PageNo = page,
                ShowRefundType = showtype
            };

            if (auditStatus.HasValue && auditStatus.Value == (int)OrderRefundInfo.OrderRefundAuditStatus.Audited)
                queryModel.ConfirmStatus = OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm;

			var refunds = _iRefundService.GetOrderRefunds(queryModel);
            var orders = Application.OrderApplication.GetOrders(refunds.Models.Select(p => p.OrderId));
            IEnumerable<OrderRefundModel> refundModels = refunds.Models.Select(item =>
            {
                var order = orders.FirstOrDefault(p => p.Id == item.OrderId);
                string spec = ((string.IsNullOrWhiteSpace(item.OrderItemInfo.Color) ? "" : item.OrderItemInfo.Color + "，")
                                + (string.IsNullOrWhiteSpace(item.OrderItemInfo.Size) ? "" : item.OrderItemInfo.Size + "，")
                                + (string.IsNullOrWhiteSpace(item.OrderItemInfo.Version) ? "" : item.OrderItemInfo.Version + "，")).TrimEnd('，');
                if (!string.IsNullOrWhiteSpace(spec))
                {
                    spec = "  【" + spec + " 】";
                }
                string showAuditStatus = "";
                //  showAuditStatus = item.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited ? item.ManagerConfirmStatus.ToDescription() : item.SellerAuditStatus.ToDescription();

                showAuditStatus = ((item.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited)
                                    ? item.ManagerConfirmStatus.ToDescription()
                                    : (order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake ? ((CommonModel.Enum.OrderRefundShopAuditStatus)item.SellerAuditStatus).ToDescription() : item.SellerAuditStatus.ToDescription()));
                if (item.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited
                    && item.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm
                    && item.RefundPayStatus == OrderRefundInfo.OrderRefundPayStatus.Payed)
                {
                    showAuditStatus = "退款中";                    
                }
                return new OrderRefundModel()
                    {
                        RefundId = item.Id,
                        OrderId = item.OrderId,
                        AuditStatus = showAuditStatus,
                        ProductId = item.OrderItemInfo.ProductId,
                        ThumbnailsUrl = Core.HimallIO.GetProductSizeImage(item.OrderItemInfo.ThumbnailsUrl, 1, (int)ImageSize.Size_100),//截取图片所在目录，从而获取图片
                        ConfirmStatus = item.ManagerConfirmStatus.ToDescription(),
                        ApplyDate = item.ApplyDate.ToShortDateString(),
                        ShopId = item.ShopId,
                        ShopName = item.ShopName.Replace("'", "‘").Replace("\"", "”"),
                        UserId = item.UserId,
                        UserName = item.Applicant,
                        Amount = item.Amount.ToString("F2"),
                        ReturnQuantity = item.ShowReturnQuantity == 0 ? item.OrderItemInfo.ReturnQuantity : item.ShowReturnQuantity,
                        ProductName = item.OrderItemInfo.ProductName + spec,
                        Reason = string.IsNullOrEmpty(item.Reason) ? string.Empty : HTMLEncode(item.Reason.Replace("'", "‘").Replace("\"", "”")),
                        ReasonDetail= string.IsNullOrEmpty(item.ReasonDetail) ? string.Empty : item.ReasonDetail.Replace("'", "‘").Replace("\"", "”"),
                    RefundAccount = string.IsNullOrEmpty(item.RefundAccount) ? string.Empty : HTMLEncode(item.RefundAccount.Replace("'", "‘").Replace("\"", "”")),
                        ContactPerson = string.IsNullOrEmpty(item.ContactPerson) ? string.Empty : HTMLEncode(item.ContactPerson.Replace("'", "‘").Replace("\"", "”")),
                        ContactCellPhone = HTMLEncode(item.ContactCellPhone),
                        PayeeAccount = string.IsNullOrEmpty(item.PayeeAccount) ? string.Empty : HTMLEncode(item.PayeeAccount.Replace("'", "‘").Replace("\"", "”")),
                        Payee = string.IsNullOrEmpty(item.Payee) ? string.Empty : HTMLEncode(item.Payee),
                        RefundMode = (int)item.RefundMode,
                        SellerRemark = string.IsNullOrEmpty(item.SellerRemark) ? string.Empty : HTMLEncode(item.SellerRemark.Replace("'", "‘").Replace("\"", "”")),
                        ManagerRemark = string.IsNullOrEmpty(item.ManagerRemark) ? string.Empty : HTMLEncode(item.ManagerRemark.Replace("'", "‘").Replace("\"", "”")),
                       RefundStatus = ((item.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited)
                                    ? item.ManagerConfirmStatus.ToDescription()
                                    : (order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake ? ((CommonModel.Enum.OrderRefundShopAuditStatus)item.SellerAuditStatus).ToDescription() : item.SellerAuditStatus.ToDescription())),
                    RefundPayType = item.RefundPayType == null ? "线下处理" : item.RefundPayType.ToDescription(),
                        RefundPayStatus = item.RefundPayStatus.HasValue ? (int)item.RefundPayStatus.Value : 0,
                        ApplyNumber = (item.ApplyNumber.HasValue ? item.ApplyNumber.Value : 1),
                        CertPic1 = Core.HimallIO.GetImagePath(item.CertPic1),
                        CertPic2 = Core.HimallIO.GetImagePath(item.CertPic2),
                        CertPic3 = Core.HimallIO.GetImagePath(item.CertPic3)
                    };
            });

            DataGridModel<OrderRefundModel> dataGrid = new DataGridModel<OrderRefundModel>() { rows = refundModels, total = refunds.Total };
            return Json(dataGrid);
        }

        private double GetNextSecond(OrderRefundInfo data)
        {
            double result = -999;
            var sitesetser = ServiceHelper.Create<ISiteSettingService>();
            var siteSetting = sitesetser.GetSiteSettings();
            if (data != null)
            {
                if (data.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit ||
                    data.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery ||
                    data.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving
                    )
                {
                    int num = 0;
                    DateTime _time = DateTime.Now;
                    switch (data.SellerAuditStatus)
                    {
                        case OrderRefundInfo.OrderRefundAuditStatus.WaitAudit:
                            _time = data.ApplyDate;
                            num = siteSetting.AS_ShopConfirmTimeout;
                            break;
                        case OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery:
                            _time = data.SellerAuditDate;
                            num = siteSetting.AS_SendGoodsCloseTimeout;
                            break;
                        case OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving:
                            _time = data.BuyerDeliverDate.GetValueOrDefault();
                            num = siteSetting.AS_ShopNoReceivingTimeout;
                            break;
                    }
                    TimeSpan ts = (DateTime.Now - _time);
                    if (num > 0)
                    {
                        result = num * 24 * 60 * 60;
                        result = result - ts.TotalSeconds;
                        if (result < 0)
                        {
                            result = -1;
                        }
                    }
                }
            }
            return result;
        }

        [HttpPost]
        public JsonResult ConfirmRefund(long refundId, string managerRemark)
        {
            Result result = new Result();
            string notifyurl = "";

            string webRoot = Request.Url.Scheme + "://" + HttpContext.Request.Url.Host + (HttpContext.Request.Url.Port == 80 ? "" : (":" + HttpContext.Request.Url.Port.ToString()));
            //获取异步通知地址
            notifyurl = webRoot + "/Pay/RefundNotify/{0}";

            string refundurl = _iRefundService.ConfirmRefund(refundId, managerRemark, CurrentManager.UserName, notifyurl);

            result.success = true;
            if (!string.IsNullOrWhiteSpace(refundurl))
            {
                result.msg = refundurl;
                result.status = 2;   //表示需要继续异步请求
            }

            return Json(result);
        }


        [HttpPost]
        public JsonResult CheckRefund(long refundId)
        {
            Result result = new Result();        
            var model = _iRefundService.HasMoneyToRefund(refundId);
            result.success= model;
            return Json(result);
        }


        public static string HTMLEncode(string txt)
        {
            if (string.IsNullOrEmpty(txt))
                return string.Empty;
            string Ntxt = txt;

            Ntxt = Ntxt.Replace(" ", "&nbsp;");

            Ntxt = Ntxt.Replace("<", "&lt;");

            Ntxt = Ntxt.Replace(">", "&gt;");

            Ntxt = Ntxt.Replace("\"", "&quot;");

            Ntxt = Ntxt.Replace("'", "&#39;");

            //Ntxt = Ntxt.Replace("\n", "<br>");

            return Ntxt;

        }
    }
}