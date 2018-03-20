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
    public class ShopOperationLogAttribute : ActionFilterAttribute
    {
        public ShopOperationLogAttribute(string message,string parameterNameList="")
        {
            this.Message = message;
            this.ParameterNameList = parameterNameList;
        }
        public ShopOperationLogAttribute()
        {
         
        }

        public string Message { get; set; }
        public string ParameterNameList { set; get; }
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var controllerName = filterContext.RouteData.Values["controller"].ToString();
            var actionName = filterContext.RouteData.Values["action"].ToString();
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
                UserName = (filterContext.Controller as BaseSellerController).CurrentSellerManager.UserName,
                PageUrl = controllerName + "/" + actionName,
                Description = logContent.ToString(),
                ShopId = (filterContext.Controller as BaseSellerController).CurrentSellerManager.ShopId,
            };
            Task.Factory.StartNew(() =>
            {
                Himall.ServiceProvider.Instance<IOperationLogService>.Create.AddSellerOperationLog(model);
            });
        }
    }
}