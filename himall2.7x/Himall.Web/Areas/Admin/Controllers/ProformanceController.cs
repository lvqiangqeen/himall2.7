using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class ProformanceController : BaseAdminController
    {
        IDistributionService _iDistributionService;
        IMemberService _iMemberService;
        ISiteSettingService _iSiteSettingService;
        public ProformanceController(IDistributionService iDistributionService, IMemberService iMemberService, ISiteSettingService iSiteSettingService)
        {
            _iMemberService = iMemberService;
            _iDistributionService = iDistributionService;
            _iSiteSettingService = iSiteSettingService;
        }
        // GET: Admin/Proformance
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Management()
        {
            return View();
        }


        public ActionResult Detail(long id)
        {
            ViewBag.Id = id;
            return View();
        }


        [HttpPost]
        public JsonResult UserPerformanceList(int page, int rows,long userid, int? days, long? orderId)
        {

            UserProformanceQuery query = new UserProformanceQuery();
            query.UserId = userid;
            query.OrderId = orderId;
            query.PageNo = page;
            query.PageSize = rows;
            if (days.HasValue)
            {
                if (days == 3)
                {
                    query.startTime = DateTime.Now.AddDays(-3);
                }
                else if (days == 7)
                {
                    query.startTime = DateTime.Now.AddDays(-7);
                }
                else if (days == 30)
                {
                    query.startTime = DateTime.Now.AddDays(-30);
                }
            }
            var m = _iDistributionService.GetPerformanceDetail(query);
            var model = m.Models.ToList();
            var expried = _iSiteSettingService.GetSiteSettings().SalesReturnTimeout;
            foreach (var t in model)
            {
                if (t.OrderStatus == Model.OrderInfo.OrderOperateStatus.Finish && t.FinshedTime.Value.AddDays(expried) <DateTime.Now)
                    t.Expired = true;
            }
            var dataGrid = new { rows = model, total = m.Total };
            return Json(dataGrid);
        }

        [HttpPost]
        public JsonResult GetMembers(string keyWords)
        {
            if (!string.IsNullOrEmpty(keyWords))
            {
                keyWords = keyWords.Trim();
            }
            var after = _iMemberService.GetMembers(false, keyWords).Where(a => a.Himall_Promoter.Count() > 0);
            var values = after.Select(item => new { key = item.Id, value = item.UserName });
            return Json(values);
        }


        [HttpPost]
        public JsonResult List(int page, int rows, long? userId, int? days)
        {
            ProformanceQuery query = new ProformanceQuery();
            query.PageNo = page;
            query.PageSize = rows;
            query.UserId = userId;
            if (days.HasValue)
            {
                if (days == 3)
                {
                    query.startTime = DateTime.Now.AddDays(-3);
                }
                else if (days == 7)
                {
                    query.startTime = DateTime.Now.AddDays(-7);
                }
                else if (days == 30)
                {
                    query.startTime = DateTime.Now.AddDays(-30);
                }
            }
            var model = _iDistributionService.GetPerformanceList(query);
            var dataGrid = new { rows = model.Models, total = model.Total };
            return Json(dataGrid);
        }
    }
}