using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Hishop.Open.Api;

namespace Himall.OpenApi.Model.Parameter
{
    /// <summary>
    /// 基础传入参数
    /// </summary>
    public class BaseParameterModel
    {
        public string app_key { get; set;}
        public string timestamp{get; set;}
        public string sign{get; set;}

        /// <summary>
        /// 检测参数完整性与合法性
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckParameter()
        {
            bool result = false;
            if(string.IsNullOrWhiteSpace(app_key))
            {
                throw new HimallOpenApiException(Hishop.Open.Api.OpenApiErrorCode.Missing_App_Key,"app_key");
            }
            if (string.IsNullOrWhiteSpace(timestamp))
            {
                throw new HimallOpenApiException(Hishop.Open.Api.OpenApiErrorCode.Missing_Timestamp, "timestamp");
            }
            if (!OpenApiSign.CheckTimeStamp(timestamp))
            {
                throw new HimallOpenApiException(Hishop.Open.Api.OpenApiErrorCode.Invalid_Timestamp, "timestamp");
            }
            if (string.IsNullOrWhiteSpace(sign))
            {
                throw new HimallOpenApiException(Hishop.Open.Api.OpenApiErrorCode.Missing_Signature, "sign");
            }
            return result;
        }
        /// <summary>
        /// 值初始
        /// </summary>
        public virtual void ValueInit()
        {

        }
    }
}
