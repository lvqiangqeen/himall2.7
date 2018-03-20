using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Himall.Web.Framework
{
    //可以给controller和Action打标记，不允许指定多个
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method,AllowMultiple = false, Inherited = false)]
    public class GZipAttribute : ActionFilterAttribute
    {
        private bool isEnableCompression = true;
        /// <summary>
        /// GZIP压缩筛选器
        /// </summary>
        /// <param name="context">ActionExecutingContext</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
             object[] actionFilter = filterContext.ActionDescriptor.GetCustomAttributes(typeof(NoCompress), false);
            object[] controllerFilter = filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(NoCompress), false);
            if (controllerFilter.Length == 1 || actionFilter.Length == 1)
            {  
               isEnableCompression = false;
            } 
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
         {

            if (filterContext.Exception != null||!isEnableCompression)
             {
                 return;
             }
            var response = filterContext.HttpContext.Response;
            //判断IIS或者其他承载设备是是否启用了GZip或DeflateStream
            if (response.Filter is GZipStream || response.Filter is DeflateStream)
                return;

	          var acceptEncoding =filterContext.HttpContext.Request.Headers["Accept-Encoding"];
            if (!string.IsNullOrEmpty(acceptEncoding))
            {
                acceptEncoding = acceptEncoding.ToLower();
               
                if (acceptEncoding.Contains("gzip") && response.Filter!=null)
                {
                    response.AppendHeader("Content-Encoding", "gzip");
                    response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
                }
                else if (acceptEncoding.Contains("deflate") && response.Filter != null)
                {
                    response.AppendHeader("Content-Encoding", "deflate");
                    response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
                }
            }
         }
    }

    /// <summary>
   /// 不启用压缩特性
   /// </summary>
   [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
   public class NoCompress : Attribute
   {
       public NoCompress()
       {
       }
   }
}
