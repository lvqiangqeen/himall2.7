using Himall.Core;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Http;
using System.Web.Routing;
using System.Net.Http;
using System.Web;

namespace Himall.OpenApi
{
    [OpenApiExceptionFilter]
    [HimallOpenApiActionFilter]
    public abstract class HiOpenAPIController : ApiController
    {
        //private HiOpenAPIController()
        //{
        //    if (String.IsNullOrEmpty(OpenAPIHelper.HostUrl))
        //        OpenAPIHelper.HostUrl = "http://" + Url.Request.RequestUri.Host;
        //}

        /// <summary>
        /// 获取传递参数转换成字典
        /// </summary>
        /// <returns></returns>
        protected SortedDictionary<string, string> GetSortedParams()
        {
            return OpenAPIHelper.GetSortedParams(Request);
        }
        /// <summary>
        /// 检测参数签名
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void CheckParamsSign(SortedDictionary<string, string> data = null)
        {
            if (data == null)
            {
                data = GetSortedParams();
            }
            OpenAPIHelper.CheckBaseParamsAndSign(data);
        }
    }
}
