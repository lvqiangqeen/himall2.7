using Hishop.Open.Api;
using System.Net.Http;
using System.Text;
using System.Web.Http.Filters;
using Himall.Core;

namespace Himall.OpenApi
{
    /// <summary>
    /// OpenApi异常处理
    /// </summary>
    public class OpenApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            string jsonstr = OpenApiErrorMessage.ShowErrorMsg(OpenApiErrorCode.System_Error, "");
            if (context.Exception is HimallOpenApiException)
            {
                var curexp = context.Exception as HimallOpenApiException;
                jsonstr = OpenApiErrorMessage.ShowErrorMsg(curexp.ErrorCode, curexp.Message);
            }
            else
            {
                Log.Error(context.Exception.Message, context.Exception);
            }

            result.Content = new StringContent(jsonstr, Encoding.GetEncoding("UTF-8"), "application/json");

            context.Response = result;
        }
    }
}