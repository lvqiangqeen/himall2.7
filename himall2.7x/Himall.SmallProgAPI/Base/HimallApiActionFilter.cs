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
using Himall.SmallProgAPI.Helper;

namespace Himall.SmallProgAPI
{
    /// <summary>
    /// Action过滤器-过滤基础参数与签名
    /// </summary>
    public class HimallApiActionFilter: ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (String.IsNullOrEmpty(ApiHelper.HostUrl))
            {
                //装载当前URI
                ApiHelper.HostUrl = actionContext.Request.RequestUri.Scheme + "://" + actionContext.Request.RequestUri.Authority;
            }

            var data = ApiHelper.GetSortedParams(actionContext.Request);
            ApiHelper.CheckBaseParamsAndSign(data);
            base.OnActionExecuting(actionContext);
        }
    }
}
