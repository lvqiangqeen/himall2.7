using System;
using System.Net.Http;
using System.Text;
using System.ComponentModel;
using System.Web.Http.Filters;
using Himall.Core;

namespace Himall.SmallProgAPI
{
    public class ApiExceptionFilterAttribute: ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            string jsonstr = ShowErrorMsg(ApiErrorCode.System_Error, "");
            if (context.Exception is HimallApiException)
            {
                var curexp = context.Exception as HimallApiException;
                jsonstr = ShowErrorMsg(curexp.ErrorCode, curexp.Message);
            }
            else
            {
                Log.Error(context.Exception.Message, context.Exception);
            }

            result.Content = new StringContent(jsonstr, Encoding.GetEncoding("UTF-8"), "application/json");

            context.Response = result;
        }
        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="enumSubitem"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        private string ShowErrorMsg(Enum enumSubitem, string fields)
        {
            string str = GetEnumDescription(enumSubitem).Replace("_", " ");
            string format = "{{\"code\":\"{0}\",\"Message\":\"{1}\",\"sub_msg\":\"{2}\"}}";
            return string.Format(format, new object[] { enumSubitem.GetHashCode().ToString(), fields, str });
        }
        /// <summary>
        /// 获取枚举描述
        /// </summary>
        /// <param name="enumSubitem"></param>
        /// <returns></returns>
        private string GetEnumDescription(Enum enumSubitem)
        {
            string name = enumSubitem.ToString();
            object[] customAttributes = enumSubitem.GetType().GetField(name).GetCustomAttributes(typeof(DescriptionAttribute), false);
            if ((customAttributes == null) || (customAttributes.Length == 0))
            {
                return name;
            }
            DescriptionAttribute attribute = (DescriptionAttribute)customAttributes[0];
            return attribute.Description;
        }


    }
}
