using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Hishop.Open.Api;

namespace Himall.OpenApi
{
    /// <summary>
    /// OpenApi统一异常
    /// </summary>
    public class HimallOpenApiException: HimallException
    {
        private OpenApiErrorCode _ErrorCode { get; set; }
        public OpenApiErrorCode ErrorCode { get
            {
                return _ErrorCode;
            }
        }

        private string _Message { get; set; }
        public override string Message
        {
            get
            {
                string result= base.Message;
                if(!string.IsNullOrWhiteSpace(_Message))
                {
                    result = _Message;
                }
                else
                {
                    _ErrorCode = OpenApiErrorCode.System_Error;
                }
                return result;
            }
        }

        /// <summary>
        /// OpenApi统一异常
        /// </summary>
        public HimallOpenApiException() { }
        /// <summary>
        /// OpenApi统一异常
        /// </summary>
        public HimallOpenApiException(OpenApiErrorCode errorcode,string message) : base(errorcode.ToString()+":"+message)
        {
            this._ErrorCode = errorcode;
            this._Message = message;
        }

        /// <summary>
        /// OpenApi统一异常
        /// </summary>
        public HimallOpenApiException(string message) : base(message) { }

        /// <summary>
        /// OpenApi统一异常
        /// </summary>
        public HimallOpenApiException(string message, Exception inner) : base(message, inner) { }
    }
}
