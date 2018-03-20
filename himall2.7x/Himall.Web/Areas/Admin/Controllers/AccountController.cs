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

namespace Himall.Web.Areas.Admin.Controllers
{
    public class AccountController : BaseAdminController
    {
        private IAccountService _iAccountService;
        private ISiteSettingService _iSiteSettingService;


        public AccountController(IAccountService iAccountService, ISiteSettingService iSiteSettingService)
        {
            this._iAccountService = iAccountService;
            this._iSiteSettingService = iSiteSettingService;
        }


        public ActionResult Management()
        {
            return View();
        }

        [UnAuthorize]
        public ActionResult Detail(long id)
        {
            AccountInfo account = _iAccountService.GetAccount(id);

            return View(account);
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult List(int status, string shopName, int page, int rows)
        {
            var queryModel = new AccountQuery()
            {
                Status = (Model.AccountInfo.AccountStatus?)status,
                ShopName = shopName,
                PageSize = rows,
                PageNo = page
            };

            ObsoletePageModel<AccountInfo> accounts = _iAccountService.GetAccounts(queryModel);
            IList<AccountModel> models = new List<AccountModel>();
            foreach (var item in accounts.Models.ToArray())
            {
                AccountModel model = new AccountModel();
                model.Id = item.Id;
                model.ShopId = item.ShopId;
                model.ShopName = item.ShopName;
                model.AccountDate = item.AccountDate.ToString();
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


        public FileResult ExportExcel(int status, string shopName)
        {
            var queryModel = new AccountQuery()
            {
                Status = (Model.AccountInfo.AccountStatus?)status,
                ShopName = shopName,
                PageSize = int.MaxValue,
                PageNo = 1
            };

            ObsoletePageModel<AccountInfo> accounts = _iAccountService.GetAccounts(queryModel);
            IList<AccountModel> models = new List<AccountModel>();
            foreach (var item in accounts.Models.ToArray())
            {
                AccountModel model = new AccountModel();
                model.Id = item.Id;
                model.ShopId = item.ShopId;
                model.ShopName = item.ShopName;
                model.AccountDate = item.AccountDate.ToString();
                model.StartDate = item.StartDate;
                model.EndDate = item.EndDate;
                model.Status = (int)item.Status;
                model.ProductActualPaidAmount = item.ProductActualPaidAmount;
                model.FreightAmount = item.FreightAmount;
                model.CommissionAmount = item.CommissionAmount;
                model.RefundAmount = item.RefundAmount;
                model.RefundCommissionAmount = item.RefundCommissionAmount;
                model.BrokerageAmount = item.Brokerage;
                model.ReturnBrokerageAmount = item.ReturnBrokerage;
                model.AdvancePaymentAmount = item.AdvancePaymentAmount;
                model.PeriodSettlement = item.PeriodSettlement;
                model.Remark = item.Remark;
                model.TimeSlot = string.Format("{0} 至 {1}", model.StartDate.Date.ToString("yyyy-MM-dd"), model.EndDate.Date.ToString("yyyy-MM-dd"));
                models.Add(model);

            }

            //创建Excel文件的对象
            NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            //添加一个sheet
            NPOI.SS.UserModel.Sheet sheet1 = book.CreateSheet("Sheet1");
            //获取list数据

            //给sheet1添加第一行的头部标题

            NPOI.SS.UserModel.Row row1 = sheet1.CreateRow(0);
            row1.CreateCell(0).SetCellValue("店铺名称");
            row1.CreateCell(1).SetCellValue("时间段");
            row1.CreateCell(2).SetCellValue("商品实付总额");
            row1.CreateCell(3).SetCellValue("运费");
            row1.CreateCell(4).SetCellValue("佣金");
            row1.CreateCell(5).SetCellValue("退款金额");
            row1.CreateCell(6).SetCellValue("退还佣金");
            row1.CreateCell(7).SetCellValue("分销佣金");
            row1.CreateCell(8).SetCellValue("退还分销佣金");
            row1.CreateCell(9).SetCellValue("营销费用总额");
            row1.CreateCell(10).SetCellValue("本期应结");
            row1.CreateCell(11).SetCellValue("出账日期");
            //将数据逐步写入sheet1各个行
            for (int i = 0; i < models.Count; i++)
            {
                NPOI.SS.UserModel.Row rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.CreateCell(0).SetCellValue(models[i].ShopName);
                rowtemp.CreateCell(1).SetCellValue(models[i].TimeSlot);
                rowtemp.CreateCell(2).SetCellValue(models[i].ProductActualPaidAmount.ToString());
                rowtemp.CreateCell(3).SetCellValue(models[i].FreightAmount.ToString());
                rowtemp.CreateCell(4).SetCellValue(models[i].CommissionAmount.ToString());
                rowtemp.CreateCell(5).SetCellValue(models[i].RefundAmount.ToString());
                rowtemp.CreateCell(6).SetCellValue(models[i].RefundCommissionAmount.ToString());
                rowtemp.CreateCell(7).SetCellValue(models[i].BrokerageAmount.ToString());
                rowtemp.CreateCell(8).SetCellValue(models[i].ReturnBrokerageAmount.ToString());
                rowtemp.CreateCell(9).SetCellValue(models[i].AdvancePaymentAmount.ToString());
                rowtemp.CreateCell(10).SetCellValue(models[i].PeriodSettlement.ToString());
                rowtemp.CreateCell(11).SetCellValue(models[i].AccountDate.ToString());
            }

            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "application/vnd.ms-excel", "结算管理.xls");


        }

        [OperationLog(Message = "确认结算")]
        [UnAuthorize]
        [HttpPost]
        public JsonResult ConfirmAccount(long id, string remark)
        {
            Result result = new Result();
            try
            {
                _iAccountService.ConfirmAccount(id, remark);
                result.success = true;
            }
            catch (Exception ex)
            {
                result.msg = ex.Message;
            }
            return Json(result);
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
                PageNo = page
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
                                           p.BrokerageAmount,
                                           p.ReturnBrokerageAmount,
                                           p.RefundTotalAmount,
                                           Date = p.Date.ToString(),
                                           OrderDate = p.OrderDate.ToString(),
                                           p.OrderRefundsDates
                                       });
            return Json(new { rows = accountDetailsModel, total = accountDetails.Total });
        }



        public FileResult DetailListExportExcel(long accountId, int enumOrderTypeId, DateTime? startDate, DateTime? endDate)
        {
            var queryModel = new AccountQuery()
            {
                StartDate = startDate,
                EndDate = endDate.HasValue ? endDate.Value.AddDays(1) : endDate,
                AccountId = accountId,
                PageSize = int.MaxValue,
                EnumOrderType = (AccountDetailInfo.EnumOrderType)enumOrderTypeId,
                PageNo = 1
            };
            var accountDetails = _iAccountService.GetAccountDetails(queryModel);

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
                                       }).ToList();
            //创建Excel文件的对象
            NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            //添加一个sheet
            NPOI.SS.UserModel.Sheet sheet1 = book.CreateSheet("Sheet1");
            //获取list数据

            //给sheet1添加第一行的头部标题
            string titleFlag = string.Empty;
            NPOI.SS.UserModel.Row row1 = sheet1.CreateRow(0);
            if (enumOrderTypeId == 1)
            {
                titleFlag = "订单列表";
                row1.CreateCell(0).SetCellValue("类型");
                row1.CreateCell(1).SetCellValue("订单编号");
                row1.CreateCell(2).SetCellValue("商品实付金额");
                row1.CreateCell(3).SetCellValue("运费");
                row1.CreateCell(4).SetCellValue("佣金");
                row1.CreateCell(5).SetCellValue("下单日期");
                row1.CreateCell(6).SetCellValue("成交日期");
                sheet1.SetColumnWidth(0, 550 * 5);
                sheet1.SetColumnWidth(1, 550 * 20);
                sheet1.SetColumnWidth(2, 550 * 8);
                sheet1.SetColumnWidth(3, 550 * 5);
                sheet1.SetColumnWidth(4, 550 * 5);
                sheet1.SetColumnWidth(5, 550 * 15);
                sheet1.SetColumnWidth(6, 550 * 15);
            }
            else if (enumOrderTypeId == 0)
            {
                titleFlag = "退单列表";
                row1.CreateCell(0).SetCellValue("类型");
                row1.CreateCell(1).SetCellValue("订单编号");
                row1.CreateCell(2).SetCellValue("商品实付金额");
                row1.CreateCell(3).SetCellValue("运费");
                row1.CreateCell(4).SetCellValue("退款金额");
                row1.CreateCell(5).SetCellValue("退还佣金");
                row1.CreateCell(6).SetCellValue("退单日期");
                sheet1.SetColumnWidth(0, 550 * 5);
                sheet1.SetColumnWidth(1, 550 * 20);
                sheet1.SetColumnWidth(2, 550 * 8);
                sheet1.SetColumnWidth(3, 550 * 5);
                sheet1.SetColumnWidth(4, 550 * 8);
                sheet1.SetColumnWidth(5, 550 * 5);
                sheet1.SetColumnWidth(6, 550 * 15);
            }

            //将数据逐步写入sheet1各个行
            for (int i = 0; i < accountDetailsModel.Count(); i++)
            {
                NPOI.SS.UserModel.Row rowtemp = sheet1.CreateRow(i + 1);
                if (enumOrderTypeId == 1)
                {
                    rowtemp.CreateCell(0).SetCellValue(titleFlag);
                    rowtemp.CreateCell(1).SetCellValue(accountDetailsModel[i].OrderId.ToString());
                    rowtemp.CreateCell(2).SetCellValue(accountDetailsModel[i].ProductActualPaidAmount.ToString());
                    rowtemp.CreateCell(3).SetCellValue(accountDetailsModel[i].FreightAmount.ToString());
                    rowtemp.CreateCell(4).SetCellValue(accountDetailsModel[i].CommissionAmount.ToString());
                    rowtemp.CreateCell(5).SetCellValue(accountDetailsModel[i].OrderDate.ToString());
                    rowtemp.CreateCell(6).SetCellValue(accountDetailsModel[i].Date.ToString());
                }
                else if (enumOrderTypeId == 0)
                {
                    rowtemp.CreateCell(0).SetCellValue(titleFlag);
                    rowtemp.CreateCell(1).SetCellValue(accountDetailsModel[i].OrderId.ToString());
                    rowtemp.CreateCell(2).SetCellValue(accountDetailsModel[i].ProductActualPaidAmount.ToString());
                    rowtemp.CreateCell(3).SetCellValue(accountDetailsModel[i].FreightAmount.ToString());
                    rowtemp.CreateCell(4).SetCellValue(accountDetailsModel[i].RefundTotalAmount.ToString());
                    rowtemp.CreateCell(5).SetCellValue(accountDetailsModel[i].RefundCommisAmount.ToString());
                    rowtemp.CreateCell(6).SetCellValue(accountDetailsModel[i].OrderRefundsDates.ToString());
                }

            }

            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "application/vnd.ms-excel", string.Format("结算详情-{0}.xls", titleFlag));
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
                StartDate = startDate,
                EndDate = endDate.HasValue ? endDate.Value.AddDays(1) : endDate,
                AccountId = accountId,
                PageSize = rows,
                PageNo = page
            };
            ObsoletePageModel<BrokerageModel> list = _iAccountService.GetBrokerageList(queryModel);
            return Json(new { rows = list.Models, total = list.Total });
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult ExecutSettlement()
        {
            //Himall.Service.Job.AccountJob job = new Service.Job.AccountJob();
            //job.Execute(null);
            //Himall.Service.Job.OrderCommentsJob commentjob = new Service.Job.OrderCommentsJob();
            //commentjob.Execute(null);
            return Json(new { success = true });
        }

        public FileResult AgreementDetailListExportExcel(long accountId, int enumOrderTypeId, DateTime? startDate, DateTime? endDate)
        {
           

            var queryModel = new AccountQuery()
            {
                StartDate = startDate,
                EndDate = endDate.HasValue ? endDate.Value.AddDays(1) : endDate,
                AccountId = accountId,
                PageSize = int.MaxValue,
                PageNo = 1
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
            }).ToList();
       
            //创建Excel文件的对象
            NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            //添加一个sheet
            NPOI.SS.UserModel.Sheet sheet1 = book.CreateSheet("Sheet1");
            //获取list数据

            //给sheet1添加第一行的头部标题

            NPOI.SS.UserModel.Row row1 = sheet1.CreateRow(0);

            row1.CreateCell(0).SetCellValue("类型");
            row1.CreateCell(1).SetCellValue("营销类型");
            row1.CreateCell(2).SetCellValue("费用");
            row1.CreateCell(3).SetCellValue("服务周期");
            sheet1.SetColumnWidth(0, 550 * 5);
            sheet1.SetColumnWidth(1, 550 * 20);
            sheet1.SetColumnWidth(2, 550 * 8);
            sheet1.SetColumnWidth(3, 550 * 15);

            //将数据逐步写入sheet1各个行
            for (int i = 0; i < mode.Count(); i++)
            {
                NPOI.SS.UserModel.Row rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.CreateCell(0).SetCellValue("营销服务费");
                rowtemp.CreateCell(1).SetCellValue(mode[i].MetaKey);
                rowtemp.CreateCell(2).SetCellValue(mode[i].MetaValue);
                rowtemp.CreateCell(3).SetCellValue(mode[i].DateRange);
            }

            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "application/vnd.ms-excel", "结算详情-营销服务费列表.xls");
        }

        public ActionResult SetSettlementWeek()
        {
            WeekSettlementModel model = new WeekSettlementModel();
            var settings = _iSiteSettingService.GetSiteSettings();
            model.CurrentWeekSettlement = settings.WeekSettlement;
            return View(model);
        }

        [HttpPost]
        public ActionResult UpdateSettlementWeek(WeekSettlementModel weekSettlementModel)
        {
            _iSiteSettingService.SaveSetting("WeekSettlement", weekSettlementModel.NewWeekSettlement);
            return RedirectToAction("SetSettlementWeek");
        }
    }
}