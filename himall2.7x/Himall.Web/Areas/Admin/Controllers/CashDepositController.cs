using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Model;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Core;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class CashDepositController : BaseAdminController
    {
        ICashDepositsService _iCashDepositsService;
        public CashDepositController(ICashDepositsService iCashDepositsService)
        {
            this._iCashDepositsService = iCashDepositsService;

        }
        // GET: Admin/CashDeposit
        public ActionResult Management()
        {
            return View();
        }

        public ActionResult CashDepositDetail(long id)
        {
            ViewBag.Id = id;
            return View();
        }

        public ActionResult CashDepositRule()
        {
            var sql=_iCashDepositsService.GetCategoryCashDeposits();
            var categoryCashDeposits = sql.ToList();
            return View(categoryCashDeposits);
        }

        [HttpPost]
        public JsonResult List(string shopName, int type, int page, int rows)
        {
            var queryModel = new CashDepositQuery()
            {
                ShopName = shopName,
                PageNo = page,
                PageSize = rows
            };
            if (type == 1)
                queryModel.Type = true;
            if (type == 2)
                queryModel.Type = false;
            var cashDepositsService = _iCashDepositsService;
            ObsoletePageModel<CashDepositInfo> cashDeposits = cashDepositsService.GetCashDeposits(queryModel);

            var cashDepositModel = cashDeposits.Models.ToArray().Select(item =>
                {
                    var needPay = cashDepositsService.GetNeedPayCashDepositByShopId(item.ShopId);

                    return new
                    {
                        Id = item.Id,
                        ShopName = item.Himall_Shops.ShopName,
                        Type = needPay > 0 ? "欠费" : "正常",
                        TotalBalance = item.TotalBalance,
                        CurrentBalance = item.CurrentBalance,
                        Date = item.Date.ToString("yyyy-MM-dd HH:mm"),
                        NeedPay = needPay,
                        EnableLabels = item.EnableLabels,
                    };
                });
            return Json(new { rows = cashDepositModel, total = cashDeposits.Total });
        }

        [HttpPost]
        public JsonResult CashDepositDetailList(long id, string name, DateTime? startDate, DateTime? endDate, int page, int rows)
        {
            var queryModel = new CashDepositDetailQuery()
            {
                CashDepositId = id,
                Operator = name,
                StartDate = startDate,
                EndDate = endDate,
                PageNo = page,
                PageSize = rows
            };
            ObsoletePageModel<CashDepositDetailInfo> cashDepositDetail = _iCashDepositsService.GetCashDepositDetails(queryModel);
            var cashDepositDetailModel = cashDepositDetail.Models.ToArray().Select(item => new
            {
                Id = item.Id,
                Date = item.AddDate.ToString("yyyy-MM-dd HH:mm"),
                Balance = item.Balance,
                Operator = item.Operator,
                Description = item.Description
            });
            return Json(new { rows = cashDepositDetailModel, total = cashDepositDetail.Total });
        }

        public JsonResult Deduction(long id, string balance, string description)
        {
            if (Convert.ToDecimal(balance) < 0)
                throw new HimallException("扣除保证金不能为负值");
            CashDepositDetailInfo model = new CashDepositDetailInfo()
            {
                AddDate = DateTime.Now,
                Balance = -Convert.ToDecimal(balance),
                CashDepositId = id,
                Description = description,
                Operator = CurrentManager.UserName
            };
            _iCashDepositsService.AddCashDepositDetails(model);
            return Json(new { Success = true });
        }

        public JsonResult UpdateEnableLabels(long id, bool enableLabels)
        {
            _iCashDepositsService.UpdateEnableLabels(id, enableLabels);
            return Json(new { Success = true });
        }

        public JsonResult OpenNoReasonReturn(long categoryId)
        {
            _iCashDepositsService.OpenNoReasonReturn(categoryId);
            return Json(new { Success = true });
        }
        public JsonResult CloseNoReasonReturn(long categoryId)
        {
            _iCashDepositsService.CloseNoReasonReturn(categoryId);
            return Json(new { Success = true });
        }

        public JsonResult UpdateNeedPayCashDeposit(long categoryId, decimal cashDeposit)
        {
            if (cashDeposit < 0)
            {
                return Json(new { Success = false, msg="不可为负数！" });
            }
            _iCashDepositsService.UpdateNeedPayCashDeposit(categoryId, cashDeposit);
            return Json(new { Success = true });
        }
    }
}