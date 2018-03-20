using Himall.Core;
using Himall.Core.Plugins;
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

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class OrderComplaintController : BaseSellerController
    {
        private IComplaintService _iComplaintService;
        public OrderComplaintController(IComplaintService iComplaintService)
        {
            _iComplaintService = iComplaintService;
        }
        public ActionResult Management()
        {
            return View();
        }


        [HttpPost]
        public JsonResult List(DateTime? startDate, DateTime? endDate, long? orderId, int? complaintStatus, string userName, int page, int rows)
        {
            var queryModel = new ComplaintQuery()
            {
                StartDate = startDate,
                EndDate = endDate,
                OrderId = orderId,
                Status = (Model.OrderComplaintInfo.ComplaintStatus?)complaintStatus,
                ShopId = CurrentSellerManager.ShopId,
                UserName = userName,
                PageSize = rows,
                PageNo = page
            };

            ObsoletePageModel<OrderComplaintInfo> orderComplaints = _iComplaintService.GetOrderComplaints(queryModel);

            var orderComplaintMode = orderComplaints.Models.ToArray().Select(item => new
            {
                Id = item.Id,                
                OrderId = item.OrderId,
                OrderTotalAmount =item.OrderInfo.OrderTotalAmount.ToString("F2"),
                PaymentTypeName = item.OrderInfo.OrderTotalAmount == 0 ? "积分支付" : item.OrderInfo.PaymentTypeName ,
                ComplaintStatus = item.Status.ToDescription(),
                ShopName = item.ShopName,
                ShopPhone = item.ShopPhone,
                UserName = item.UserName,
                UserPhone = item.UserPhone,
                ComplaintDate = item.ComplaintDate.ToShortDateString(),
                ComplaintReason = System.Text.RegularExpressions.Regex.Replace(item.ComplaintReason, @"(<|(&lt;))br[^>]*?(>|(&gt;))", "").Replace("<", "&lt;").Replace(">", "&gt;"),
                SellerReply = item.SellerReply
            });
            return Json(new { rows = orderComplaintMode, total = orderComplaints.Total });
        }


        [HttpPost]
        public JsonResult DealComplaint(long id, string reply)
        {
            if(string.IsNullOrWhiteSpace(reply))
            {
                return Json(new Result() { success=false,msg="回复内容不能为空！"});
            }

            Result result = new Result();
            try
            {
                _iComplaintService.SellerDealComplaint(id, reply);
                result.success = true;
            }
            catch(Exception ex)
            {
                result.msg = ex.Message;
            }
            return Json(result);
        }
    }
}