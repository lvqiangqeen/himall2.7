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
    public class OrderCommentController : BaseAdminController
    {
        ITradeCommentService _iTradeCommentService;
        public OrderCommentController(ITradeCommentService iTradeCommentService)
        {
            _iTradeCommentService = iTradeCommentService;
        }
        public ActionResult Management()
        {
            return View();
        }


        [UnAuthorize]
        [HttpPost]
        public JsonResult List(DateTime? startDate, DateTime? endDate, long? orderId, string shopName, string userName, int page, int rows)
        {
            var queryModel = new OrderCommentQuery()
            {
                StartDate = startDate,
                EndDate = endDate,
                OrderId = orderId,
                ShopName = shopName,
                UserName = userName,
                PageSize = rows,
                PageNo = page
            };


            ObsoletePageModel<OrderCommentInfo> orderComments = _iTradeCommentService.GetOrderComments(queryModel);

            var orderCommentMode = orderComments.Models.ToArray().Select(item => new
            {
                Id = item.Id,
                OrderId = item.OrderId,
                ShopName = item.ShopName,
                UserName = item.UserName,
                CommentDate = item.CommentDate.ToShortDateString(),
                PackMark = item.PackMark,
                DeliveryMark = item.DeliveryMark,
                ServiceMark = item.ServiceMark
            });
            return Json(new { rows = orderCommentMode, total = orderComments.Total });
        }

        [OperationLog(Message = "确认结算")]
        [HttpPost]
        [UnAuthorize]
        public JsonResult Delete(int id)
        {
            _iTradeCommentService.DeleteOrderComment(id);

            return Json(new Result() { success = true, msg = "删除成功！" });

        }
    }
}