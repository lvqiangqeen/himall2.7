using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.IServices;
using Himall.Core;
using Himall.Web.Areas.Web.Models;
using Himall.IServices.QueryModel;
using Himall.Model;

namespace Himall.Web.Areas.Web.Controllers
{
    public class OrderComplaintController : BaseMemberController
    {
       private IOrderService _iOrderService;
       private IShopService _iShopService;
       private IComplaintService _iComplaintService;

        public OrderComplaintController(IOrderService iOrderService, IShopService iShopService, IComplaintService iComplaintService)
        {
            _iOrderService = iOrderService;
            _iShopService = iShopService;
            _iComplaintService = iComplaintService;
        }
        // GET: Web/OrderComplaint
        public ActionResult Index(int pageSize = 10, int pageNo = 1)
        {
            OrderQuery query = new OrderQuery();
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            query.UserId = CurrentUser.Id;
            query.Status = OrderInfo.OrderOperateStatus.Finish;
            var OrderComplaints=_iOrderService.GetOrders<OrderInfo>(query);
            var model = OrderComplaints.Models.Where(o => o.OrderComplaintInfo.Count() == 0);
            #region 分页控制
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = OrderComplaints.Total
            };
            ViewBag.pageInfo = info;
            ViewBag.UserPhone = CurrentUser.CellPhone;
            ViewBag.UserId = CurrentUser.Id;
            #endregion
            return View(model.ToList());
        }

        [HttpPost]
        public JsonResult AddOrderComplaint(OrderComplaintInfo model)
        {
            model.UserId = CurrentUser.Id;
            model.UserName = CurrentUser.UserName;
            model.ComplaintDate = DateTime.Now;
            model.Status = OrderComplaintInfo.ComplaintStatus.WaitDeal;
            var shop = _iShopService.GetShop(model.ShopId);
            var order = _iOrderService.GetOrder(model.OrderId, CurrentUser.Id);
            if (model.ComplaintReason.Length < 5)
            {
                throw new  HimallException("投诉内容不能小于5个字符！");
            }
            if (string.IsNullOrWhiteSpace(model.UserPhone))
            {
                throw new HimallException("投诉电话不能为空！");
            }
            if (order == null || order.ShopId != model.ShopId)
            {
                throw new HimallException("该订单不属于当前用户！");
            }
            model.ShopName = shop == null ? "" : shop.ShopName;
            model.ShopPhone = shop == null ? "" : shop.CompanyPhone;
            model.ShopPhone = model.ShopPhone == null ? "" : model.ShopPhone;
            model.ShopName = model.ShopName == null ? "" : model.ShopName;

            _iComplaintService.AddComplaint(model);
            return Json(new { success = true, msg = "提交成功" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Record(int pageSize = 10, int pageNo = 1)
        {
            ComplaintQuery query = new ComplaintQuery();
            query.UserId = CurrentUser.Id;
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            var model = _iComplaintService.GetOrderComplaints(query);
            #region 分页控制
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = model.Total
            };
            ViewBag.pageInfo = info;
            #endregion
            return View(model.Models.ToList());
        }
        [HttpPost]
        public JsonResult ApplyArbitration(long id)
        {
            _iComplaintService.UserApplyArbitration(id, CurrentUser.Id);
            return Json(new { success = true, msg = "处理成功" });
        }
        [HttpPost]
        public JsonResult DealComplaint(long id)
        {
            _iComplaintService.UserDealComplaint(id, CurrentUser.Id);
            return Json(new { success = true, msg = "处理成功" });
        }
    }
}