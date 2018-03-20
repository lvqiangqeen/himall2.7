using Himall.Core;
using Himall.Core.Plugins;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
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

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class AccountController : BaseSellerController
    {
        private IAccountService _iAccountService;
        public AccountController(IAccountService iAccountService)
        {
            _iAccountService = iAccountService;
        }
        public ActionResult Management()
        {
            return View();
        }

        public ActionResult AccountDetail(long id)
        {
            AccountInfo account = _iAccountService.GetAccount(id);
            if (account.ShopId != CurrentSellerManager.ShopId)
            {
                throw new HimallException("不存在该结算信息" + id);
            }
            return View(account);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult List(int status, int page, int rows)
        {
            var queryModel = new AccountQuery()
            {
                Status = (Model.AccountInfo.AccountStatus?)status,
                PageSize = rows,
                PageNo = page,
                ShopId = CurrentSellerManager.ShopId
            };

            ObsoletePageModel<AccountInfo> accounts = _iAccountService.GetAccounts(queryModel);
            IList<AccountModel> models = new List<AccountModel>();
            foreach (var item in accounts.Models.ToArray())
            {
                AccountModel model = new AccountModel();
                model.Id = item.Id;
                model.ShopId = item.ShopId;
                model.ShopName = item.ShopName;
                model.AccountDate = item.AccountDate.ToLocalTime().ToString();
                model.StartDate = item.StartDate;
                model.EndDate = item.EndDate;
                model.Status = (int)item.Status;
                model.ProductActualPaidAmount = item.ProductActualPaidAmount;
                model.FreightAmount = item.FreightAmount;
                model.CommissionAmount = item.CommissionAmount;
                model.RefundAmount = item.RefundAmount;
                model.RefundCommissionAmount = item.RefundCommissionAmount;
                model.AdvancePaymentAmount = item.AdvancePaymentAmount;
                model.PeriodSettlement = item.PeriodSettlement;
                model.Remark = item.Remark;
                model.BrokerageAmount = item.Brokerage;
                model.ReturnBrokerageAmount = item.ReturnBrokerage;
                model.TimeSlot = string.Format("{0} 至 {1}", model.StartDate.Date.ToString("yyyy-MM-dd"), model.EndDate.Date.ToString("yyyy-MM-dd"));
                models.Add(model);

            }
            return Json(new { rows = models, total = accounts.Total });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult DetailList(long accountId, int enumOrderTypeId, DateTime? startDate, DateTime? endDate, int page, int rows)
        {
            var queryModel = new AccountQuery()
            {
                StartDate = startDate,
                EndDate = endDate.HasValue ? endDate.Value.AddDays(1) : endDate,
                AccountId = accountId,
                PageSize = rows,
                EnumOrderType = (AccountDetailInfo.EnumOrderType)enumOrderTypeId,
                PageNo = page,
                ShopId = CurrentSellerManager.ShopId
            };
            ObsoletePageModel<AccountDetailInfo> accountDetails = _iAccountService.GetAccountDetails(queryModel);

            var accountDetailsModel = (from p in accountDetails.Models.ToList()
                                       select new
                                       {
                                           p.Id,
                                           p.OrderType,
                                           OrderTypeDescription = p.OrderType.ToDescription(),
                                           p.OrderId,
                                           p.ProductActualPaidAmount,
                                           p.FreightAmount,
                                           p.CommissionAmount,
                                           p.RefundCommisAmount,
                                           p.RefundTotalAmount,
                                           Date = p.Date.ToString(),
                                           OrderDate = p.OrderDate.ToString(),
                                           p.OrderRefundsDates
                                       });
            return Json(new { rows = accountDetailsModel, total = accountDetails.Total });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult MetaDetailList(long accountId, int enumOrderTypeId, DateTime? startDate, DateTime? endDate, int page, int rows)
        {
            var queryModel = new AccountQuery()
            {
                StartDate = startDate,
                EndDate = endDate.HasValue ? endDate.Value.AddDays(1) : endDate,
                AccountId = accountId,
                PageSize = rows,
                PageNo = page
            };
            ObsoletePageModel<AccountMetaModel> pageModelMetaInfo = _iAccountService.GetAccountMeta(queryModel);
            var mode = pageModelMetaInfo.Models.ToList().Select(e => new AccountMetaModel
            {
                AccountId = e.Id,
                Id = e.Id,
                EndDate = e.EndDate,
                StartDate = e.StartDate,
                MetaKey = e.MetaKey,
                MetaValue = e.MetaValue,
                DateRange = e.StartDate.ToString("yyyy-MM-dd") + " 至 " + e.EndDate.ToString("yyyy-MM-dd")
            });
            return Json(new { rows = mode, total = pageModelMetaInfo.Total });
        }

        public JsonResult BrokerageDetailList(long accountId, DateTime? startDate, DateTime? endDate, int page, int rows)
        {
            var queryModel = new AccountQuery()
            {
                ShopId = CurrentSellerManager.ShopId,
                StartDate = startDate,
                EndDate = endDate.HasValue ? endDate.Value.AddDays(1) : endDate,
                AccountId = accountId,
                PageSize = rows,
                PageNo = page
            };
            ObsoletePageModel<BrokerageModel> list = _iAccountService.GetBrokerageList(queryModel);
            return Json(new { rows = list.Models, total = list.Total });
        }
    }
}