using Himall.Core.Helper;
using Himall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Framework
{
    //可以给controller和Action打标记，不允许指定多个
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class OperationLogAttribute : ActionFilterAttribute
    {
        public string Message { get; set; }
        /// <summary>
        /// 参数名称列表,可以用, | 分隔
        /// </summary>
        public string ParameterNameList;

        public OperationLogAttribute()
        {

        }
        public OperationLogAttribute(string message, string parameterNameList = "")
        {
            this.Message = message;
            this.ParameterNameList = parameterNameList;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //filterContext.HttpContext.Request.Form;//form表单提交的数据
            //filterContext.HttpContext.Request.QueryString;//Url 参数
            if (filterContext.Exception != null) return;
            var controllerName = filterContext.RouteData.Values["controller"].ToString();
            var actionName = filterContext.RouteData.Values["action"].ToString();
            var areaName = filterContext.RouteData.Values["area"];
            StringBuilder logContent = new StringBuilder();
            logContent.Append(Message + ",操作记录:");
            if (!string.IsNullOrEmpty(ParameterNameList))
            {
                Dictionary<string, string> parmsObj = new Dictionary<string, string>();

                foreach (var item in ParameterNameList.Split(',', '|'))
                {
                    var valueProviderResult = filterContext.Controller.ValueProvider.GetValue(item);

                    if (valueProviderResult != null && !parmsObj.ContainsKey(item))
                    {
                        parmsObj.Add(item, valueProviderResult.AttemptedValue);
                    }
                }
                foreach (KeyValuePair<string, string> kvp in parmsObj)
                {
                    logContent.AppendFormat("{0}:{1} ", kvp.Key, kvp.Value);
                }
            }
            var model = new Himall.Model.LogInfo()
            {
                Date = DateTime.Now,
                IPAddress = WebHelper.GetIP(),
                UserName = (filterContext.Controller as BaseAdminController).CurrentManager.UserName,
                PageUrl = controllerName + "/" + actionName,
                Description = logContent.ToString()
            };
            Task.Factory.StartNew(() =>
            {
                Himall.ServiceProvider.Instance<IOperationLogService>.Create.AddPlatformOperationLog(model);
            });
            base.OnActionExecuted(filterContext);
        }
    }
}