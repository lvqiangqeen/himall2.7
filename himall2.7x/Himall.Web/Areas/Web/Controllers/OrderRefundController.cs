using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Core;
using Himall.Web.Areas.Web.Models;
using System.IO;
using Himall.Application;

namespace Himall.Web.Areas.Web.Controllers
{
    public class OrderRefundController : BaseMemberController
    {
        IRefundService _iRefundService;
        IOrderService _iOrderService;
        private ITypeService _iTypeService;
        public OrderRefundController(IRefundService iRefundService, IOrderService iOrderService, ITypeService iTypeService)
        {
            _iRefundService = iRefundService;
            _iOrderService = iOrderService;
            _iTypeService = iTypeService;
        }

        [ValidateInput(false)]
        // GET: Web/OrderRefund
        public ActionResult Index(string orderDate, string keywords, int pageNo = 1, int pageSize = 10)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
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

            var queryModel = new OrderQuery()
            {
                StartDate = startDate,
                EndDate = endDate,
                Status = Model.OrderInfo.OrderOperateStatus.Finish,
                UserId = CurrentUser.Id,
                SearchKeyWords = keywords,
                PageSize = pageSize,
                PageNo = pageNo
            };
            queryModel.MoreStatus = new List<OrderInfo.OrderOperateStatus>();
            queryModel.MoreStatus.Add(OrderInfo.OrderOperateStatus.WaitReceiving);
            ObsoletePageModel<OrderInfo> orders = _iOrderService.GetOrders<OrderInfo>(queryModel);
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = orders.Total
            };
            ViewBag.UserId = CurrentUser.Id;
            ViewBag.pageInfo = info;
            var siteSetting = ServiceHelper.Create<ISiteSettingService>().GetSiteSettings();
            ViewBag.SalesRefundTimeout = siteSetting.SalesReturnTimeout;
            return View(orders.Models.ToList());
        }
        /// <summary>
        /// 退款申请
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public ActionResult RefundApply(long orderid, long? itemId, long? refundid)
        {
            RefundApplyModel model = new RefundApplyModel();
            model.RefundMode = null;
            model.OrderItemId = null;
            var order = _iOrderService.GetOrder(orderid, CurrentUser.Id);
            if (order == null)
                throw new Himall.Core.HimallException("该订单已删除或不属于该用户");
            if ((int)order.OrderStatus < 2)
                throw new Himall.Core.HimallException("错误的售后申请,订单状态有误");
            if (itemId == null && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                throw new Himall.Core.HimallException("错误的订单退款申请,订单状态有误");
            //售后时间限制
            if (_iOrderService.IsRefundTimeOut(orderid))
            {
                throw new Himall.Core.HimallException("订单已超过售后期");
            }


            //计算可退金额 预留
            _iOrderService.CalculateOrderItemRefund(orderid);

            OrderItemInfo item = new OrderItemInfo();
            model.MaxRefundGoodsNumber = 0;
            model.MaxRefundAmount = order.OrderEnabledRefundAmount;
            if (itemId == null)
            {
                model.OrderItems = order.OrderItemInfo.ToList();
                if (model.OrderItems.Count == 1)
                    item = model.OrderItems.FirstOrDefault();
            }
            else
            {
                item = order.OrderItemInfo.Where(a => a.Id == itemId).FirstOrDefault();
                model.OrderItems.Add(item);
                model.MaxRefundGoodsNumber = item.Quantity - item.ReturnQuantity;
                model.MaxRefundAmount = item.EnabledRefundAmount - item.RefundPrice;
            }
            foreach (var orderItem in model.OrderItems)
            {
                ProductTypeInfo typeInfo = _iTypeService.GetTypeByProductId(orderItem.ProductId);
                orderItem.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                orderItem.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                orderItem.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            }
            if (!model.MaxRefundAmount.HasValue)
            {
                model.MaxRefundAmount = 0;
            }
            bool isCanApply = false;
            var refundser = _iRefundService;
            OrderRefundInfo refunddata;

            if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                isCanApply = refundser.CanApplyRefund(orderid, item.Id);
            }
            else
            {
                isCanApply = refundser.CanApplyRefund(orderid, item.Id, false);
            }
            if (!refundid.HasValue)
            {
                if (!isCanApply)
                {
                    var orderRefunds = OrderApplication.GetOrderRefunds(new long[] { item.Id });
                    if (orderRefunds.Count == 1)
                    {
                        Response.Redirect("/OrderRefund/Detail/" + orderRefunds[0].Id);
                    }

                    throw new Himall.Core.HimallException("您已申请过售后，不可重复申请");
                }

                model.ContactPerson = CurrentUser.RealName;
                model.ContactCellPhone = CurrentUser.CellPhone;
                model.OrderItemId = itemId;
                if (!model.OrderItemId.HasValue)
                {
                    model.IsOrderAllRefund = true;
                    model.RefundMode = OrderRefundInfo.OrderRefundMode.OrderRefund;
                }
            }
            else
            {
                refunddata = refundser.GetOrderRefund(refundid.Value, CurrentUser.Id);
                if (refunddata == null)
                {
                    throw new Himall.Core.HimallException("错误的售后数据");
                }
                if (refunddata.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.UnAudit)
                {
                    throw new Himall.Core.HimallException("错误的售后状态，不可激活");
                }
                model.ContactPerson = refunddata.ContactPerson;
                model.ContactCellPhone = refunddata.ContactCellPhone;
                model.OrderItemId = refunddata.OrderItemId;
                model.IsOrderAllRefund = (refunddata.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund);
                model.RefundMode = refunddata.RefundMode;
                model.RefundReasonValue = refunddata.Reason;
                model.RefundReasonDetail = refunddata.ReasonDetail;
                model.RefundWayValue = refunddata.RefundPayType;
                model.CertPic1 = refunddata.CertPic1;
                model.CertPic2 = refunddata.CertPic2;
                model.CertPic3 = refunddata.CertPic3;
            }
            if (!model.IsOrderAllRefund && item.EnabledRefundAmount.HasValue)
            {
                model.RefundGoodsPrice = item.EnabledRefundAmount.Value / item.Quantity;
            }
            model.OrderInfo = order;
            model.OrderId = orderid;
            model.RefundId = refundid;

            var reasons = refundser.GetRefundReasons();
            foreach (var _ir in reasons)
            {
                _ir.AfterSalesText = _ir.AfterSalesText.Trim();
            }
            List<SelectListItem> reasel = new List<SelectListItem>();
            SelectListItem _tmpsel;
            _tmpsel = new SelectListItem { Text = "选择售后原因", Value = "" };
            reasel.Add(_tmpsel);
            foreach (var _i in reasons)
            {
                _tmpsel = new SelectListItem { Text = _i.AfterSalesText, Value = _i.AfterSalesText };
                if (!string.IsNullOrWhiteSpace(model.RefundReasonValue))
                {
                    if (_i.AfterSalesText == model.RefundReasonValue)
                    {
                        _tmpsel.Selected = true;
                    }
                }
                reasel.Add(_tmpsel);
            }
            model.RefundReasons = reasel;

            List<SelectListItem> list = new List<SelectListItem> {
                new SelectListItem{
                  Text=OrderRefundInfo.OrderRefundPayType.BackCapital.ToDescription(),
                  Value=((int)OrderRefundInfo.OrderRefundPayType.BackCapital).ToString()
                }
            };
            if (order.CanBackOut())
            {
                _tmpsel = new SelectListItem
                {
                    Text = OrderRefundInfo.OrderRefundPayType.BackOut.ToDescription(),
                    Value = ((int)OrderRefundInfo.OrderRefundPayType.BackOut).ToString()
                };
                //if (model.RefundWayValue.HasValue)
                //{
                //    if (_tmpsel.Value == model.RefundWayValue.ToString())
                //    {
                //        _tmpsel.Selected = true;
                //    }
                //}
                _tmpsel.Selected = true;  //若订单支付方式为支付宝、微信支付则退款方式默认选中“退款原路返回”
                list.Add(_tmpsel);
            }
            model.RefundWay = list;

            if (order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake)
            {
                var shopBranch = ShopBranchApplication.GetShopBranchById(order.ShopBranchId.Value);
                model.ReturnGoodsAddress = RegionApplication.GetFullName(shopBranch.AddressId);
                model.ReturnGoodsAddress += " " + shopBranch.AddressDetail;
                model.ReturnGoodsAddress += " " + shopBranch.ContactPhone;
            }

            return View(model);
        }
        /// <summary>
        /// 退款申请处理
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult RefundApply(OrderRefundInfo info)
        {
            if (info.RefundType == 1)
            {
                info.ReturnQuantity = 0;
                info.IsReturn = false;
            }
            info.IsReturn = false;
            if (info.ReturnQuantity > 0) info.IsReturn = true;
            if (info.RefundType == 2) info.IsReturn = true;
            if (info.IsReturn == true && info.ReturnQuantity < 1) throw new Himall.Core.HimallException("错误的退货数量");
            if (info.Amount <= 0) throw new Himall.Core.HimallException("错误的退款金额");
            if (info.ReasonDetail != null && info.ReasonDetail.Length > 1000)
                throw new Himall.Core.HimallException("退款原因不能超过1000字符");
            // info.ReasonDetail=
            info.UserId = CurrentUser.Id;
            info.Applicant = CurrentUser.UserName;
            info.ApplyDate = DateTime.Now;
            info.ReasonDetail = HttpUtility.HtmlEncode(info.ReasonDetail);
            info.Reason = HTMLEncode(info.Reason.Replace("'", "‘").Replace("\"", "”"));
            info.CertPic1 = MoveImages(info.CertPic1, CurrentUser.Id, info.OrderItemId);
            info.CertPic2 = MoveImages(info.CertPic2, CurrentUser.Id, info.OrderItemId);
            info.CertPic3 = MoveImages(info.CertPic3, CurrentUser.Id, info.OrderItemId);

            //info.RefundAccount = HTMLEncode(info.RefundAccount.Replace("'", "‘").Replace("\"", "”"));
            Result result = new Result() { success = true, Data = info.Id, msg = "提交成功" };
            if (info.Id > 0)
            {
                _iRefundService.ActiveRefund(info);
            }
            else
            {
                var status = RefundApplication.CanApplyRefund(info.OrderId, info.OrderItemId, info.OrderItemId == 0 ? true : false);
                if (!status)
                {
                    result.success = false;
                    result.status = 2;
                    result.msg = "您已申请过售后，不可重复申请";
                }
                else
                {
                    RefundApplication.AddOrderRefund(info);
                }
                //取退款ID
                var refundInfos = RefundApplication.GetOrderRefunds(new RefundQuery() { OrderId = info.OrderId, PageNo = 1, PageSize = int.MaxValue }).Models;
                if (refundInfos.Count > 0)
                {
                    if (info.OrderItemId != 0)
                    {
                        var refund = refundInfos.FirstOrDefault(e => e.OrderItemId == info.OrderItemId);
                        result.Data = refund.Id;
                    }
                    else
                    {
                        var refund = refundInfos.FirstOrDefault();
                        result.Data = refund.Id;
                    }
                }
            }
            return Json(result);
        }

        private string MoveImages(string image, long userId, long itemid)
        {
            if (!string.IsNullOrWhiteSpace(image))
            {
                var ext = Path.GetFileName(image);
                string ImageDir = string.Empty;
                //转移图片
                string relativeDir = "/Storage/Plat/Refund/" + userId.ToString() + "/";
                string fileName = itemid.ToString() + "_" + DateTime.Now.ToString("yyMMddHHmmssffff") + ext;
                if (image.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
                {
                    var de = image.Substring(image.LastIndexOf("/temp/"));
                    Core.HimallIO.CopyFile(de, relativeDir + fileName, true);
                    return relativeDir + fileName;
                }  //目标地址
                else if (image.Contains("/Storage/"))
                {
                    return image.Substring(image.LastIndexOf("/Storage/"));
                }

                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 显示售后记录
        /// </summary>
        /// <param name="applyDate"></param>
        /// <param name="status"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="showtype">0 所有 1 订单退款 2 仅退款(包含订单退款) 3 退货 4 仅退款</param>
        /// <returns></returns>
        public ActionResult List(string applyDate, int? status, int pageNo = 1, int pageSize = 10, int showtype = 0)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            if (!string.IsNullOrEmpty(applyDate) && applyDate.ToLower() != "all")
            {
                switch (applyDate.ToLower())
                {
                    case "threemonth":
                        startDate = DateTime.Now.AddMonths(-3);
                        break;
                    case "threemonthago":
                        endDate = DateTime.Now.AddMonths(-3);
                        break;
                }
            }
            if (status.HasValue)
            {
                if (status == 0)
                {
                    status = null;
                }
            }

            var queryModel = new RefundQuery()
            {
                StartDate = startDate,
                EndDate = endDate,
                UserId = CurrentUser.Id,
                PageSize = pageSize,
                PageNo = pageNo,
                ShowRefundType = showtype
            };

            if (status.HasValue)
            {
                if (status > (int)OrderRefundInfo.OrderRefundAuditStatus.Audited)
                    queryModel.ConfirmStatus = (OrderRefundInfo.OrderRefundConfirmStatus)status;
                else
                    queryModel.AuditStatus = (OrderRefundInfo.OrderRefundAuditStatus)status;
            }

            var refunds = _iRefundService.GetOrderRefunds(queryModel);
            var orders = OrderApplication.GetOrders(refunds.Models.Select(p => p.OrderId));

            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = refunds.Total
            };
            ViewBag.pageInfo = info;
            ViewBag.UserId = CurrentUser.Id;
            ViewBag.ShowType = showtype;
            ViewBag.Orders = orders;
            return View(refunds.Models);
        }

        public ActionResult Detail(long id)
        {
            var refund = _iRefundService.GetOrderRefund(id, CurrentUser.Id);
            if (refund == null)
            {
                throw new HimallException("错误的退款服务号");
            }
            ViewBag.UserName = CurrentUser.UserName;
            int curappnum = refund.ApplyNumber.HasValue ? refund.ApplyNumber.Value : 1;
            refund.IsOrderRefundTimeOut = _iOrderService.IsRefundTimeOut(refund.OrderId);
            var order = OrderApplication.GetOrder(refund.OrderId);
            refund.Order = order;

            var refundLogs = RefundApplication.GetRefundLogs(refund.Id, curappnum, false);
            if (order.DeliveryType == Himall.CommonModel.Enum.DeliveryType.SelfTake || (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0))
            {
                foreach (var item in refundLogs)
                {
                    var temp = item.OperateContent.Split('】');
                    item.OperateContent = temp[0].Replace("商家", "门店") + '】' + temp[1];
                }
            }

            ViewBag.RefundLogs = refundLogs;
            return View(refund);
        }
        [HttpGet]
        public JsonResult GetShopInfo(long shopId)
        {
            var shopinfo = ServiceHelper.Create<IShopService>().GetShop(shopId);
            var model = new { SenderAddress = shopinfo.SenderAddress, SenderPhone = shopinfo.SenderPhone, SenderName = shopinfo.SenderName };
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateRefund(long id, string expressCompanyName, string shipOrderNumber)
        {
            _iRefundService.UserConfirmRefundGood(id, CurrentUser.UserName, expressCompanyName, shipOrderNumber);
            return Json(new { success = true, msg = "提交成功" });
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