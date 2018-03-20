using System;

namespace Himall.Core
{
    /// <summary>
    /// 无效属性异常
    /// </summary>
    public class InvalidPropertyException:HimallException
    {
        /// <summary>
        /// 无效属性异常
        /// </summary>
        public InvalidPropertyException() { }

        /// <summary>
        /// 无效属性异常
        /// </summary>
        public InvalidPropertyException(string message) : base(message) { }

        /// <summary>
        /// 无效属性异常
        /// </summary>
        public InvalidPropertyException(string message, Exception inner) : base(message, inner) { }
    }
}
