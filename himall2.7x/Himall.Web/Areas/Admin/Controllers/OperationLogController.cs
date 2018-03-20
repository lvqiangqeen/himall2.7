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


namespace Himall.Web.Areas.Admin.Controllers
{
    public class OperationLogController : BaseAdminController
    {
        private IOperationLogService _iOperationLogService;

        private IManagerService _iManagerService;
        public OperationLogController(IOperationLogService iOperationLogService,IManagerService iManagerService)
        {
            _iOperationLogService = iOperationLogService;
            _iManagerService = iManagerService;
        }
        [Description("平台日志管理页面")]
        // GET: Admin/OperationLog
        public ActionResult Management()
        {
            return View();
        }

        [UnAuthorize]
        [Description("分页获取日志的JSON数据")]
        public JsonResult List(int page, string userName, int rows, DateTime? startDate,DateTime?endDate)
        {
            var query = new OperationLogQuery() { UserName = userName, PageNo = page, PageSize = rows, StartDate = startDate, EndDate = endDate };

            var result = _iOperationLogService.GetPlatformOperationLogs(query);
            var logs = result.Models.ToList().Select(item => new 
            {
               Id=item.Id,
               UserName=item.UserName,
               PageUrl=item.PageUrl,
               Description=item.Description,
               Date=item.Date.ToString("yyyy-MM-dd HH:mm"),
               IPAddress=item.IPAddress
            });
            var model = new { rows =logs, total = result.Total };
            return Json(model);
        }

        [UnAuthorize]
        [Description("关键字获取管理员用户名列表")]
        public JsonResult GetManagers(string keyWords)
        {
            var after = _iManagerService.GetManagers(keyWords).Where(item => item.ShopId == 0);
            var values = after.Select(item => new { key = item.Id,value = item.UserName });
            return Json(values);
        }

    }
}