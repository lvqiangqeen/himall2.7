using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Framework;
using Himall.Model;
using Himall.IServices.QueryModel;
using Himall.IServices;
using Himall.Core;
using Himall.Web.Models;
using Himall.Web.Areas.Web.Models;
using Himall.Core.Plugins.Payment;
using Himall.Application;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class ShopWithDrawController : BaseAdminController
    {


        public ActionResult Management(int status = 1)
        {
            ViewBag.Status = status;
            return View();
        }

        /// <summary>
        /// 提现信息列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult List(DateTime? startDate, DateTime? endDate, DateTime? startDates, DateTime? endDates, string shopName, int Status, int page, int rows)
        {
            Himall.CommonModel.WithdrawQuery query = new CommonModel.WithdrawQuery()
            {
                ApplyStartTime = startDate,
                ApplyEndTime = endDate,
                AuditedStartTime = startDates,
                AuditedEndTime = endDates,
                ShopName = shopName,
                Status = (Himall.CommonModel.WithdrawStaus?)Status,
                PageSize = rows,
                PageNo = page
            };
            var model = BillingApplication.GetShopWithDraw(query);
            return Json(new { rows = model.Models, total = model.Total });
        }

        /// <summary>
        /// 审核操作
        /// </summary>
        /// <param name="id">申请ID</param>
        /// <param name="status">状态</param>
        /// <param name="remark">备注</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ConfirmPay(long id, int status, string remark)
        {
            if (BillingApplication.ShopApplyWithDraw(id, (Himall.CommonModel.WithdrawStaus)status, remark, Request.UserHostAddress, CurrentManager.UserName))
                return Json(new { success = true, msg = "成功！" });
            else
                return Json(new { success = false, msg = "操作失败！" });
        }
    }
}