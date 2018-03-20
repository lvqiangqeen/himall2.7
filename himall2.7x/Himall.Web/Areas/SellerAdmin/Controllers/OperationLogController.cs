using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;

using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class OperationLogController : BaseSellerController
    {
        private IManagerService _iManagerService;
        private IOperationLogService _iOperationLogService;


        public OperationLogController(IManagerService iManagerService,IOperationLogService iOperationLogService)
        {
            _iManagerService = iManagerService;
            _iOperationLogService = iOperationLogService;
        }

        [Description("商家日志管理页面")]
        // GET: Admin/OperationLog
        public ActionResult Management()
        {
            return View();
        }

        [Description("分页获取日志的JSON数据")]
        [UnAuthorize]
        public JsonResult List(int page, string userName, int rows, DateTime? startDate, DateTime? endDate)
        {
            var query = new OperationLogQuery() { UserName = userName, PageNo = page, ShopId = CurrentSellerManager.ShopId, PageSize = rows, StartDate = startDate, EndDate = endDate };
            var result = _iOperationLogService.GetPlatformOperationLogs(query);
            var logs = result.Models.ToList().Select(item => new
            {
                Id = item.Id,
                UserName = item.UserName,
                PageUrl = item.PageUrl,
                Description = item.Description,
                Date = item.Date.ToString("yyyy-MM-dd HH:mm"),
                IPAddress = item.IPAddress
            });
            var model = new { rows = logs, total = result.Total };
            return Json(model);
        }

        [Description("关键字获取管理员用户名列表")]
        [UnAuthorize]
        public JsonResult GetManagers(string keyWords)
        {
            var shopId = CurrentSellerManager.ShopId;
            var after = _iManagerService.GetManagers(keyWords).Where(item => item.ShopId == shopId);
            var values = after.Select(item => new { key = item.Id, value = item.UserName });
            return Json(values);
        }

    }
}