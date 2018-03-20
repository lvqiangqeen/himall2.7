using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    /// <summary>
    /// 分销业绩
    /// </summary>
    public class DistributionFeatController : BaseSellerController
    {
        private IDistributionService _iDistributionService;
        private IMemberService _iMemberService;
        private long curshopid = 0;
        private ShopDistributorSettingInfo distributorConfig;
        public DistributionFeatController(IDistributionService iDistributionService, IMemberService iMemberService)
        {
            _iDistributionService = iDistributionService;
            _iMemberService = iMemberService;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            curshopid = CurrentSellerManager.ShopId;
            distributorConfig = _iDistributionService.getShopDistributorSettingInfo(curshopid);
        }

        #region 订单业绩
        /// <summary>
        /// 分销业绩(订单)
        /// </summary>
        /// <returns></returns>
        public ActionResult Order()
        {

            return View();
        }

        public JsonResult GetOrderList(long? orderid = null, OrderInfo.OrderOperateStatus? ordstate = null, int? sstate = null, long? salesid = null, DateTime? stime = null, DateTime? etime = null, int rows = 10, int page = 1)
        {
            List<DistributionFeatModel> datalist = new List<DistributionFeatModel>();
            DistributionUserBillQuery query = new DistributionUserBillQuery { ShopId = curshopid, PageNo = page, PageSize = rows };
            List<BrokerageIncomeInfo.BrokerageStatus> states = new List<BrokerageIncomeInfo.BrokerageStatus>();
            if (sstate.HasValue)
            {
                switch (sstate)
                {
                    case 1:
                        states.Add(BrokerageIncomeInfo.BrokerageStatus.Settled);
                        break;
                    case 0:
                        states.Add(BrokerageIncomeInfo.BrokerageStatus.NotAvailable);
                        states.Add(BrokerageIncomeInfo.BrokerageStatus.NotSettled);
                        break;
                }
            }
            query.SettleState = states;
            if (stime.HasValue)
            {
                query.StartTime = stime.Value.Date;
            }
            if (etime.HasValue)
            {
                query.EndTime = etime.Value.Date;
            }
            if (orderid.HasValue)
            {
                query.OrderId = orderid.Value;
            }
            if (salesid.HasValue)
            {
                query.UserId = salesid.Value;
            }
            if (ordstate.HasValue)
            {
                query.OrderState = ordstate.Value;
            }

            var datasql = _iDistributionService.GetUserBillList(query);
            datalist = datasql.Models.ToList();
            var siteconfig = this.CurrentSiteSetting;
            int SalesRefundTimeout = siteconfig.SalesReturnTimeout;
            int NoReceivingTimeout = siteconfig.NoReceivingTimeout;
            //需要计算维权期
            foreach (var item in datalist)
            {
                if (SalesRefundTimeout < 0)
                {
                    SalesRefundTimeout = 0;
                }
                if (!item.LastRightsTime.HasValue)
                {
                    item.LastRightsTime = item.CreateTime;
                    item.LastRightsTime = item.LastRightsTime.Value.AddDays(SalesRefundTimeout);
                }
                item.LastRightsTime = item.LastRightsTime.Value.AddDays(SalesRefundTimeout);
            }

            var result = new { rows = datalist, total = datasql.Total };
            return Json(result);
        }
        #endregion

        [HttpPost]
        public JsonResult GetMembers(string keyWords)
        {
            var after = _iMemberService.GetMembers(false, keyWords).Where(a => a.Himall_Promoter.Count() > 0);
            var values = after.Select(item => new { key = item.Id, value = item.UserName });
            return Json(values);
        }

        #region 商品业绩
        /// <summary>
        /// 商品分销明细
        /// </summary>
        /// <returns></returns>
        public ActionResult Product()
        {
            return View();
        }

        public JsonResult GetProductDataList(string skey = "", int? status = null, int rows = 10, int page = 1)
        {
            DistributionQuery query = new DistributionQuery();
            query.PageNo = page;
            query.PageSize = rows;
            query.ProductName = skey;
            query.ShopId = curshopid;
            if (status.HasValue && status.Value != -1)
                query.Status = (Himall.Model.ProductBrokerageInfo.ProductBrokerageStatus)status.Value;
            var m = _iDistributionService.GetDistributionlist(query);
            var model = m.Models.ToList();
            var result = new { rows = model, total = m.Total };
            return Json(result);
        }
        #endregion
    }
}