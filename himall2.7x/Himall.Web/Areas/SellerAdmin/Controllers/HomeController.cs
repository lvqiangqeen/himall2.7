using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web;
using Himall.IServices;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Model;
using Himall.IServices.QueryModel;
using System.Configuration;
using Himall.Core.Helper;
using Himall.Application;


namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class HomeController : BaseSellerController
    {
       private IShopService _iShopService;
       private IArticleService _iArticleService;
       private IBrandService _iBrandService;
       private ICommentService _iCommentService;
       private IProductService _iProductService;
       private IManagerService _iManagerService;
       private IStatisticsService _iStatisticsService;
        public HomeController(
            IShopService iShopService,
            IArticleService iArticleService,
            IBrandService iBrandService,
            ICommentService iCommentService,
            IProductService iProductService,
            IManagerService iManagerService,
            IStatisticsService iStatisticsService
            )
        {
            _iShopService = iShopService;
            _iArticleService = iArticleService;
            _iBrandService = iBrandService;
            _iCommentService = iCommentService;
            _iProductService = iProductService;
            _iManagerService = iManagerService;
            _iStatisticsService = iStatisticsService;

        }
        private bool IsInstalled()
        {
            var t = ConfigurationManager.AppSettings["IsInstalled"];
            return null == t || bool.Parse(t);
        }

        //[UnAuthorize]
        //public JsonResult GetsellerAdminMessage()
        //{
        //    #region 查询提醒消息

        //    var commentQuery = new CommentQuery() { PageNo = 1, PageSize = 100001, ShopID = base.CurrentSellerManager.ShopId, IsReply = false };
        //    var UnReplyComments = _iCommentService.GetComments(commentQuery).Models.Count();

        //    var consultationsQuery = new ConsultationQuery { PageNo = 1, PageSize = 10000, ShopID = base.CurrentSellerManager.ShopId, IsReply = false };
        //    var UnReplyConsultations = ServiceHelper.Create<IConsultationService>().GetConsultations(consultationsQuery).Models.Count();



        //    var UnPayOrder = ServiceHelper.Create<IOrderService>().GetOrders<OrderInfo>(
        //        new OrderQuery
        //        {
        //            PageNo = 1,
        //            PageSize = 10000,
        //            Status = Model.OrderInfo.OrderOperateStatus.WaitPay,
        //            ShopId = base.CurrentSellerManager.ShopId
        //        }).Models.Count();
        //    var UnDeliveryOrder = ServiceHelper.Create<IOrderService>().GetOrders<OrderInfo>(
        //        new OrderQuery
        //        {
        //            PageNo = 1,
        //            PageSize = 10000,
        //            Status = Model.OrderInfo.OrderOperateStatus.WaitDelivery,
        //            ShopId = base.CurrentSellerManager.ShopId
        //        }).Models.Count();

        //    var UnComplaints = ServiceHelper.Create<IComplaintService>().GetOrderComplaints(
        //        new ComplaintQuery
        //        {
        //            PageNo = 1,
        //            PageSize = 10000,
        //            ShopId = CurrentSellerManager.ShopId,
        //            Status = Model.OrderComplaintInfo.ComplaintStatus.WaitDeal
        //        }).Models.Count();




        //    var AllMessageCount = (UnReplyConsultations + UnReplyComments + UnPayOrder + UnComplaints + UnDeliveryOrder);

        //    #endregion

        //    return Json(new
        //    {
        //        UnReplyConsultations = UnReplyConsultations,
        //        UnReplyComments = UnReplyComments,
        //        UnPayOrder = UnPayOrder,
        //        UnComplaints = UnComplaints,
        //        UnDeliveryOrder = UnDeliveryOrder,
        //        AllMessageCount = AllMessageCount
        //    }, JsonRequestBehavior.AllowGet);
        //}

        [UnAuthorize]
        public ActionResult Index()
        {
            //var t = ConfigurationManager.AppSettings["IsInstalled"];
            //if (!(null == t || bool.Parse(t)))
            //{
            //    return RedirectToAction("Agreement", "Installer", new { area = "Web" });
            //}

            //var shopInfo= _iShopService.GetShopBasicInfo(CurrentSellerManager.ShopId);
            //ViewBag.IsSellerAdmin = shopInfo.IsSelf;
            //ViewBag.ShopId = CurrentSellerManager.ShopId;
            //ViewBag.Name = CurrentSellerManager.UserName;
            //ViewBag.Rights = string.Join(",", CurrentSellerManager.SellerPrivileges.Select(a => (int)a).OrderBy(a => a));
            //ViewBag.SiteName = CurrentSiteSetting.SiteName;
            //ViewBag.Logo = Himall.Core.HimallIO.GetImagePath(CurrentSiteSetting.MemberLogo);
            //ViewBag.EndDate = shopInfo.EndDate.Value.ToString("yyyy-MM-dd");

            //var cache = CacheKeyCollection.isPromptKey(CurrentSellerManager.ShopId);
            //var cacheCode = Core.Cache.Get(cache);
            //if (cacheCode == null)
            //{
            //    Core.Cache.Insert(cache, "0", DateTime.Parse(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd")));//一天只提醒一次
            //    ViewBag.isPrompt = shopInfo.EndDate.Value < DateTime.Now.AddDays(15) ? 1 : 0;//到期前15天提示
            //}
            //else
            //{
            //    ViewBag.isPrompt = 0;
            //}
            //return View(CurrentSellerManager);
            return RedirectToAction("Console");
        }
        [UnAuthorize]
        public ActionResult Console()
        {
            //新首页开始
            HomeModel model = new HomeModel();
            model.SellerConsoleModel = _iShopService.GetSellerConsoleModel(CurrentSellerManager.ShopId);
            /*公告*/
            model.Articles = _iArticleService.GetTopNArticle<ArticleInfo>(6, 4);
            /*店铺信息*/
            var shop = _iShopService.GetShop(CurrentSellerManager.ShopId);
            if (shop != null)
            {
                ViewBag.Logo = CurrentSiteSetting.MemberLogo;
                model.ShopId = shop.Id;
                model.ShopLogo = shop.Logo;
                model.ShopName = shop.ShopName;
                model.ShopEndDate = shop.EndDate.HasValue ? shop.EndDate.Value.ToString("yyyy-MM-dd") : string.Empty;
                var shopGrade = _iShopService.GetShopGrades().Where(c => c.Id == shop.GradeId).FirstOrDefault();
                model.ShopGradeName = shopGrade != null ? shopGrade.Name : string.Empty;

                var shopStatisticOrderComments = _iShopService.GetShopStatisticOrderComments(CurrentSellerManager.ShopId);

                var productAndDescription = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescription).FirstOrDefault();

                var sellerServiceAttitude = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitude).FirstOrDefault();
                var sellerDeliverySpeed = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeed).FirstOrDefault();
                var defaultValue = "5";
                //宝贝与描述
                model.ProductAndDescription = productAndDescription != null ? string.Format("{0:F}", productAndDescription.CommentValue) : defaultValue;
                //卖家服务态度
                model.SellerServiceAttitude = sellerServiceAttitude != null ? string.Format("{0:F}", sellerServiceAttitude.CommentValue) : defaultValue;
                //卖家发货速度
                model.SellerDeliverySpeed = sellerDeliverySpeed != null ? string.Format("{0:F}", sellerDeliverySpeed.CommentValue) : defaultValue;
                //所有商品数
                model.ProductsNumberIng = model.SellerConsoleModel.ProductLimit.ToString();
                //发布商品数量
                model.ProductsNumber = model.SellerConsoleModel.ProductsCount.ToString();

                //使用空间
                model.UseSpace = model.SellerConsoleModel.ImageLimit.ToString();
                //正使用的空间
                model.UseSpaceing = model.SellerConsoleModel.ProductImages.ToString();

                //商品咨询
                model.OrderProductConsultation = model.SellerConsoleModel.ProductConsultations.ToString();
                //订单总数
                model.OrderCounts = model.SellerConsoleModel.OrderCounts.ToString();
                //待买家付款
                model.OrderWaitPay = model.SellerConsoleModel.WaitPayTrades.ToString();
                //待发货
                model.OrderWaitDelivery = model.SellerConsoleModel.WaitDeliveryTrades.ToString();

                //待回复评价
                model.OrderReplyComments = _iCommentService.GetComments(new CommentQuery
                {
                    PageNo = 1,
                    PageSize = int.MaxValue,
                    IsReply = false,
                    ShopID = CurrentSellerManager.ShopId
                }).Total.ToString();
                //待处理投诉
                model.OrderHandlingComplaints = model.SellerConsoleModel.Complaints.ToString();
                //待处理退款
                model.OrderWithRefund = model.SellerConsoleModel.RefundTrades.ToString();
                //待处理退货
                model.OrderWithRefundAndRGoods = model.SellerConsoleModel.RefundAndRGoodsTrades.ToString();
                //商品评价
                model.ProductsEvaluation = model.SellerConsoleModel.ProductComments.ToString();
                //授权品牌
                model.ProductsBrands =_iBrandService.GetShopBrandApplys(CurrentSellerManager.ShopId).Where(c => c.AuditStatus == 1).Count().ToString();
                //出售中
                model.ProductsOnSale = model.SellerConsoleModel.OnSaleProducts.ToString();
                //草稿箱
                model.ProductsInDraft = _iProductService.GetProducts(new ProductQuery
                {
                    PageNo = 1,
                    PageSize = int.MaxValue,
                    ShopId = CurrentSellerManager.ShopId,
                    SaleStatus = ProductInfo.ProductSaleStatus.InDraft
                }).Total.ToString();
                //待审核
                model.ProductsWaitForAuditing = model.SellerConsoleModel.WaitForAuditingProducts.ToString();
                //审核未通过
                model.ProductsAuditFailed = model.SellerConsoleModel.AuditFailureProducts.ToString();
                //违规下架
                model.ProductsInfractionSaleOff = model.SellerConsoleModel.InfractionSaleOffProducts.ToString();
                //仓库中
                model.ProductsInStock = model.SellerConsoleModel.InStockProducts.ToString();
                //警戒库存数
                model.OverSafeStockProducts = ProductManagerApplication.GetOverSafeStockProducts(CurrentSellerManager.ShopId).ToString();
                DateTime startDate = DateTime.Now.AddDays(-1).Date;
                DateTime endDate = startDate.AddDays(1).AddMilliseconds(-1);
                var statistic = StatisticApplication.GetShopTradeStatistic(startDate, endDate, CurrentSellerManager.ShopId);
                IList<EchartsData> lstEchartsData = new List<EchartsData>();
                if (statistic != null)
                {
                    ViewBag.VistiCounts = statistic.VistiCounts;
                    ViewBag.OrderCounts = statistic.OrderCount;
                    ViewBag.SaleAmounts = statistic.SaleAmounts;
                }
                else
                {
                    string zero = decimal.Zero.ToString();
                    ViewBag.VistiCounts = zero;
                    ViewBag.OrderCounts = zero;
                    ViewBag.SaleAmounts = zero;
                }
            }

            //新首页结束
            return View(model);
        }

        [UnAuthorize]
        [HttpGet]
        public ActionResult ProductRecentMonthSaleRank()
        {
            var shopId = CurrentSellerManager.ShopId;
            var model = _iStatisticsService.GetRecentMonthSaleRankChart(shopId);
            return Json(new { successful = true, chart = model }, JsonRequestBehavior.AllowGet);
        }

        [UnAuthorize]
        [HttpGet]
        public ActionResult AnalysisEffectShop()
        {
            DateTime startDate = DateTime.Now.AddDays(-1).Date;
            DateTime endDate = startDate.AddDays(1).AddMilliseconds(-1);
            ShopInfo.ShopVistis shopVistis = _iShopService.GetShopVistiInfo(startDate, endDate, CurrentSellerManager.ShopId);
            IList<EchartsData> lstEchartsData = new List<EchartsData>();
            if (shopVistis != null)
            {
                lstEchartsData.Add(new EchartsData { name = "访问次数", value = shopVistis.VistiCounts.ToString() });
                lstEchartsData.Add(new EchartsData { name = "下单次数", value = shopVistis.OrderCounts.ToString() });
                lstEchartsData.Add(new EchartsData { name = "支付金额", value = shopVistis.SaleAmounts.ToString() });
            }
            else
            {
                string zero = decimal.Zero.ToString();
                lstEchartsData.Add(new EchartsData { name = "访问次数", value = zero });
                lstEchartsData.Add(new EchartsData { name = "下单次数", value = zero });
                lstEchartsData.Add(new EchartsData { name = "支付金额", value = zero });
            }

            return Json(new { successful = true, chart = lstEchartsData }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult ChangePassword(string oldpassword, string password)
        {
            if (string.IsNullOrWhiteSpace(oldpassword) || string.IsNullOrWhiteSpace(password))
            {
                return Json(new Result() { success = false, msg = "密码不能为空！" });
            }
            var model = CurrentSellerManager;
            var pwd = SecureHelper.MD5(SecureHelper.MD5(oldpassword) + model.PasswordSalt);
            if (pwd == model.Password)
            {
                _iManagerService.ChangeSellerManagerPassword(CurrentSellerManager.Id, CurrentSellerManager.ShopId, password, CurrentSellerManager.RoleId);
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
            var model = CurrentSellerManager;
            var pwd = SecureHelper.MD5(SecureHelper.MD5(password) + model.PasswordSalt);
            if (model.Password == pwd)
            {
                return Json(new Result() { success = true });
            }
            return Json(new Result() { success = false });
        }
    }
}