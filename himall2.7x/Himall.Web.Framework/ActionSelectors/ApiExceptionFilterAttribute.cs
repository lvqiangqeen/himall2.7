using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;


namespace Himall.Web.Framework
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            if(context.Exception is Himall.Core.HimallException)
            {
                result.Content = new StringContent("Success = false, Error = 101, ErrorMsg =" + context.Exception.Message, Encoding.GetEncoding("UTF-8"), "application/json");
            }
            else
            {
                result.Content = new StringContent("Success = false, Error = 102, ErrorMsg =" + context.Exception.Message, Encoding.GetEncoding("UTF-8"), "application/json"); 
            }
            context.Response = result;

        }
    }
}


