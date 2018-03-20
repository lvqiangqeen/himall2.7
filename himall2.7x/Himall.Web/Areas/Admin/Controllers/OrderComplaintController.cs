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

namespace Himall.Web.Areas.Admin.Controllers
{
    public class OrderComplaintController : BaseAdminController
    {
        IComplaintService _iComplaintService;
        public OrderComplaintController(IComplaintService iComplaintService)
        {
            _iComplaintService = iComplaintService;
        }
        public ActionResult Management()
        {
            return View();
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult List(DateTime? startDate, DateTime? endDate, long? orderId, int? complaintStatus, string shopName, string userName, int page, int rows)
        {
            var queryModel = new ComplaintQuery()
            {
                StartDate = startDate,
                EndDate = endDate,
                OrderId = orderId,
                Status = (Model.OrderComplaintInfo.ComplaintStatus?)complaintStatus,
                ShopName = shopName,
                UserName = userName,
                PageSize = rows,
                PageNo = page
            };

            ObsoletePageModel<OrderComplaintInfo> orderComplaints = _iComplaintService.GetOrderComplaints(queryModel);

            var orderComplaintMode = orderComplaints.Models.ToArray().Select(item => new
            {
                Id = item.Id,
                OrderId = item.OrderId,
                ComplaintStatus = item.Status.ToDescription(),
                ShopName = item.ShopName,
                ShopPhone = item.ShopPhone,
                UserName = item.UserName,
                UserPhone = item.UserPhone,
                ComplaintDate = item.ComplaintDate.ToShortDateString(),
                ComplaintReason = System.Text.RegularExpressions.Regex.Replace(item.ComplaintReason, @"(<|(&lt;))br[^>]*?(>|(&gt;))", "").Replace("<", "&lt;").Replace(">", "&gt;"),
                SellerReply = item.SellerReply
            }).ToList();
            return Json(new { rows = orderComplaintMode, total = orderComplaints.Total });
        }

        [OperationLog(Message = "处理交易投诉")]
        [UnAuthorize]
        [HttpPost]
        public JsonResult DealComplaint(long id)
        {
            Result result = new Result();
            try
            {
                _iComplaintService.DealComplaint(id);
                result.success = true;
            }
            catch (Exception ex)
            {
                result.msg = ex.Message;
            }
            return Json(result);
        }
    }
}