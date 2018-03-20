using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web;

namespace Himall.OpenApi
{
    /// <summary>
    /// Action过滤器-过滤基础参数与签名
    /// </summary>
    public class HimallOpenApiActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (String.IsNullOrEmpty(OpenAPIHelper.HostUrl))
            {
                //装载当前URI
                OpenAPIHelper.HostUrl = actionContext.Request.RequestUri.Scheme+"://"+ actionContext.Request.RequestUri.Authority;
            }

            var data = OpenAPIHelper.GetSortedParams(actionContext.Request);
            OpenAPIHelper.CheckBaseParamsAndSign(data);
            base.OnActionExecuting(actionContext);
        }
    }
}
