using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web;
using Himall.IServices;
using Himall.IServices.QueryModel;
using System.Configuration;
using Himall.Model;
using Himall.Core.Helper;
using Himall.Core.Plugins.Payment;


namespace Himall.Web.Areas.Admin.Controllers
{
    public class HomeController : BaseAdminController
    {
        IShopService _iShopService;
        IStatisticsService _iStatisticsService;
        IManagerService _iManagerService;
        public HomeController(IShopService iShopService, IStatisticsService iStatisticsService, IManagerService iManagerService)
        {
            _iShopService = iShopService;
            _iStatisticsService = iStatisticsService;
            _iManagerService = iManagerService;
        }

        [HttpGet]
        //public JsonResult GetPlatfromMessage()
        //{
        //    #region 查询提醒消息

        //    var queryModel = new ProductQuery()
        //    {
        //        PageSize = 10000,
        //        PageNo = 1,
        //        AuditStatus = new Model.ProductInfo.ProductAuditStatus[] { Model.ProductInfo.ProductAuditStatus.WaitForAuditing },
        //        SaleStatus = Model.ProductInfo.ProductSaleStatus.OnSale
        //    };

        //    var ProductWaitForAuditing = ServiceHelper.Create<IProductService>().GetProducts(queryModel).Models.Count();

        //    var shopQueryModel = new ShopQuery()
        //    {
        //         Status = 0,
        //        PageSize = 10000,
        //        PageNo = 1
        //    };

        //    var ShopWaitAudit = ServiceHelper.Create<IShopService>().GetShops(shopQueryModel).Models.Where(
        //        s=>s.ShopStatus== ShopInfo.ShopAuditStatus.WaitAudit
        //        || s.ShopStatus== ShopInfo.ShopAuditStatus.WaitConfirm 
        //        || s.ShopStatus== ShopInfo.ShopAuditStatus.WaitPay).Count();


        //    var BrandAudit = ServiceHelper.Create<IBrandService>().
        //        GetShopBrandApplys(null,0, 1, 10000,"").Models.Count();


        //    var complaintQueryModel = new ComplaintQuery()
        //    {

        //        Status = Model.OrderComplaintInfo.ComplaintStatus.Dispute,
        //        PageSize = 10000,
        //        PageNo = 1
        //    };

        //    var ComplaintDispute = ServiceHelper.Create<IComplaintService>().GetOrderComplaints(complaintQueryModel).Models.Count();

        //    var orderQueryModel = new RefundQuery()
        //    {

        //         ConfirmStatus = OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm,
        //          AuditStatus= Model.OrderRefundInfo.OrderRefundAuditStatus.Audited,
        //        PageSize = 10000,
        //        PageNo = 1
        //    };
        //    var RefundWaitAudit = ServiceHelper.Create<IRefundService>().GetOrderRefunds(orderQueryModel).Models.Count();

        //    var AllMessageCount = (ProductWaitForAuditing + ShopWaitAudit + BrandAudit + ComplaintDispute + RefundWaitAudit);

        //    #endregion

        //    return Json(new
        //    {
        //        ProductWaitForAuditing = ProductWaitForAuditing,
        //        ShopWaitAudit = ShopWaitAudit,
        //        BrandAudit = BrandAudit,
        //        ComplaintDispute = ComplaintDispute,
        //        RefundWaitAudit = RefundWaitAudit,
        //        AllMessageCount = AllMessageCount
        //    }, JsonRequestBehavior.AllowGet);
        //}

        // GET: Admin/Home
        [UnAuthorize]
        public ActionResult Index()
        {
            //var t = ConfigurationManager.AppSettings["IsInstalled"];
            //if (!(null == t || bool.Parse(t)))
            //{
            //    return RedirectToAction("Agreement", "Installer", new { area = "Web" });
            //}
            //ViewBag.Name = CurrentManager.UserName;
            //ViewBag.Rights = string.Join(",", CurrentManager.AdminPrivileges.Select(a => (int)a).OrderBy(a => a));

            //return View();
            return RedirectToAction("Console");
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult ChangePassword(string oldpassword, string password)
        {
            if (string.IsNullOrWhiteSpace(oldpassword) || string.IsNullOrWhiteSpace(password))
            {
                return Json(new Result() { success = false, msg = "密码不能为空！" });
            }
            var model = CurrentManager;
            var pwd = SecureHelper.MD5(SecureHelper.MD5(oldpassword) + model.PasswordSalt);
            if (pwd == model.Password)
            {
                _iManagerService.ChangePlatformManagerPassword(model.Id, password, 0);
                return Json(new Result() { success = true, msg = "修改成功" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "旧密码错误" });
            }
        }


        [UnAuthorize]
        public JsonResult CheckOldPassword(string password)
        {
            var model = CurrentManager;
            var pwd = SecureHelper.MD5(SecureHelper.MD5(password) + model.PasswordSalt);
            if (model.Password == pwd)
            {
                return Json(new Result() { success = true });
            }
            return Json(new Result() { success = false });
        }

        [UnAuthorize]
        public ActionResult Copyright()
        {
            return View();
        }

        [UnAuthorize]
        public ActionResult About()
        {
            return View();
        }
        [UnAuthorize]
        public ActionResult Console()
        {
            var model = _iShopService.GetPlatConsoleMode();
            return View(model);
        }

        [HttpGet]
        [UnAuthorize]
        public ActionResult ProductRecentMonthSaleRank()
        {
            var model = _iStatisticsService.GetRecentMonthSaleRankChart();
            return Json(new { successful = true, chart = model }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [UnAuthorize]
        public ActionResult GetRecentMonthShopSaleRankChart()
        {
            var model = _iStatisticsService.GetRecentMonthShopSaleRankChart();
            return Json(new { successful = true, chart = model }, JsonRequestBehavior.AllowGet);
        }
    }
}