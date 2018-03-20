using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;

namespace Himall.API
{
    public class HimallApiException : HimallException
    {
        private ApiErrorCode _ErrorCode { get; set; }
        public ApiErrorCode ErrorCode
        {
            get
            {
                return _ErrorCode;
            }
        }

        private string _Message { get; set; }
        public override string Message
        {
            get
            {
                string result = base.Message;
                if (!string.IsNullOrWhiteSpace(_Message))
                {
                    result = _Message;
                }
                else
                {
                    _ErrorCode = ApiErrorCode.System_Error;
                }
                return result;
            }
        }

        /// <summary>
        /// OpenApi统一异常
        /// </summary>
        public HimallApiException() { }
        /// <summary>
        /// OpenApi统一异常
        /// </summary>
        public HimallApiException(ApiErrorCode errorcode, string message) : base(errorcode.ToString()+":"+message)
        {
            this._ErrorCode = errorcode;
            this._Message = message;
        }

        /// <summary>
        /// OpenApi统一异常
        /// </summary>
        public HimallApiException(string message) : base(message) { }

        /// <summary>
        /// OpenApi统一异常
        /// </summary>
        public HimallApiException(string message, Exception inner) : base(message, inner) { }
    }
}
