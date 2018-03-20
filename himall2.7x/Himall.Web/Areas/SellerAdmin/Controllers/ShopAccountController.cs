using Himall.Core;
using Himall.Core.Plugins;

using Himall.Web.Areas.Admin.Models;
using Himall.Web.Areas.Admin.Models.Product;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Himall.DTO;
using Himall.Application;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class ShopAccountController : BaseSellerController
    {
    
        /// <summary>
        /// 充值页面
        /// </summary>
        /// <returns></returns>
        public ActionResult CapitalCharge()
        {
            Himall.DTO.ShopAccount model = BillingApplication.GetShopAccount(CurrentSellerManager.ShopId);
            return View(model);
        }

        /// <summary>
        /// 充值请求提交
        /// </summary>
        /// <param name="amount">充值金额</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ChargeSubmit(decimal amount)
        {
            string webRoot = Request.Url.Scheme + "://" + HttpContext.Request.Url.Host + (HttpContext.Request.Url.Port == 80 ? "" : (":" + HttpContext.Request.Url.Port.ToString()));
            var model = BillingApplication.PaymentList(CurrentSellerManager.ShopId, amount, webRoot);
            return Json(model.models);
        }

        /// <summary>
        /// 提现申请页面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult ApplyWithDraw()
        {
            Himall.Model.ShopInfo.ShopStage Stage = Himall.Model.ShopInfo.ShopStage.Agreement;
            var shop = ShopApplication.GetShopProfileStep2(CurrentSellerManager.ShopId, out Stage);
            Himall.DTO.MemberAccountSafety mMemberAccountSafety = MemberApplication.GetMemberAccountSafety(CurrentUser.Id);
            ViewBag.MemberEmail = mMemberAccountSafety.Email;
            ViewBag.MemberPhone = mMemberAccountSafety.Phone;
            return View(shop);
        }

        /// <summary>
        /// 提现申请数据提交
        /// </summary>
        /// <returns></returns>
        public JsonResult ApplyWithDrawSubmit(string pluginId, string destination, string code, decimal amount, int WithdrawType)
        {
            int result = MemberApplication.CheckCode(pluginId, code, destination, CurrentUser.Id);
            if (result > 0)
            {
                Himall.DTO.ShopWithDraw model = new ShopWithDraw()
                {
                    SellerId = CurrentUser.Id,
                    SellerName = CurrentUser.UserName,
                    ShopId = CurrentSellerManager.ShopId,
                    WithdrawalAmount = amount,
                    WithdrawType = (Himall.CommonModel.WithdrawType)WithdrawType
                };

                bool isbool = BillingApplication.ShopApplyWithDraw(model);

                if (isbool)
                    return Json(new { success = true, msg = "成功！" });
                else
                    return Json(new { success = false, msg = "余额不足，无法提现！" });
            }
            else
            {
                return Json(new { success = false, msg = "验证码错误！" });
            }
        }

        /// <summary>
        /// 提现记录列表
        /// </summary>
        /// <returns></returns>
        public ActionResult Management(long Id = 0)
        {
            ViewBag.Id = Id;
            return View();
        }

        /// <summary>
        /// 提现信息列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult List(DateTime? startDate, DateTime? endDate, DateTime? startDates, DateTime? endDates, long? WithdrawStaus, long? Id, int Status, int page, int rows)
        {
            Himall.CommonModel.WithdrawQuery query = new CommonModel.WithdrawQuery()
            {
                ApplyStartTime = startDate,
                ApplyEndTime = endDate,
                AuditedStartTime = startDates,
                AuditedEndTime = endDates,
                Id = Id,
                Status = (Himall.CommonModel.WithdrawStaus?)WithdrawStaus,
                ShopId = CurrentSellerManager.ShopId,
                PageSize = rows,
                PageNo = page
            };
            var model = BillingApplication.GetShopWithDraw(query);
            return Json(new { rows = model.Models, total = model.Total });
        }

		/// <summary>
		/// 提现信息列表
		/// </summary>
		/// <returns></returns>
		public ActionResult ExportShopWithDraw(DateTime? applyStartTime, DateTime? applyEndTime, DateTime? auditedStartTime, DateTime? auditedEndTime, long? staus)
		{
			var query = new CommonModel.WithdrawQuery()
			{
				ApplyStartTime = applyStartTime,
				ApplyEndTime = applyEndTime,
				AuditedStartTime = auditedStartTime,
				AuditedEndTime = auditedEndTime,
				Status = (Himall.CommonModel.WithdrawStaus?)staus,
				ShopId = CurrentSellerManager.ShopId,
			};
			var models = BillingApplication.GetShopWithDrawNoPage(query);
			return ExcelView("提现明细", models);
		}
    }
}