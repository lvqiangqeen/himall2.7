using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Himall.Web.Controllers
{
    public class ExpressDataController : BaseController
    {
        private IExpressService _iExpressService;
        public ExpressDataController(IExpressService iExpressService)
        {
            _iExpressService = iExpressService;
        }
        // GET: ExpressData
        public JsonResult Search(string expressCompanyName, string shipOrderNumber)
        {

            var expressData = _iExpressService.GetExpressData(expressCompanyName, shipOrderNumber);

            if (expressData != null)
            {
                if (expressData.Success)
                    expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列
                var json = new
                {
                    success = expressData.Success,
                    msg = expressData.Message,
                    data = expressData.ExpressDataItems.Select(item => new
                    {
                        time = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                        content = item.Content
                    })
                };
                return Json(json);
            }
            else
            {
                var json = new
                {
                    success = false,
                    msg = "无物流信息"
                };
                return Json(json);
            }
        }


        // GET: ExpressData
        public JsonResult SearchTop(string expressCompanyName, string shipOrderNumber)
        {
            var expressData = _iExpressService.GetExpressData(expressCompanyName, shipOrderNumber);

            if (expressData.Success)
                expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列

            var json = new
            {
                success = expressData.Success,
                msg = expressData.Message,
                data = expressData.ExpressDataItems.Take(1).Select(item => new
                {
                    time = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    content = item.Content
                })
            };

            return Json(json);
        }

        [HttpPost]
        public JsonResult SaveExpressData(string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                return Json(new { result = false, returnCode = 500, message = "服务器错误" });
            }
            try
            {
                var ReturnModel = new
                {
                    status = string.Empty,
                    message = string.Empty,
                    lastResult = new
                    {
                        message = string.Empty,
                        state = string.Empty,
                        status = string.Empty,
                        ischeck = string.Empty,
                        com = string.Empty,
                        nu = string.Empty
                    }
                };
                var obj = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(param, ReturnModel);
                OrderExpressInfo model = new OrderExpressInfo();
                model.DataContent = param;
                model.CompanyCode = obj.lastResult.com;
                model.ExpressNumber = obj.lastResult.nu;
                _iExpressService.SaveExpressData(model);
                return Json(new { result = true, returnCode = 200, message = "成功" });
            }
            catch (Exception ex)
            {
                Core.Log.Error("保存快递信息错误：" + ex.Message + param);
                return Json(new { result = false, returnCode = 500, message = "服务器错误" + ex.Message });
            }
        }
    }
}