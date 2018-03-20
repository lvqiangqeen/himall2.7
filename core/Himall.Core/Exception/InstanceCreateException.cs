using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Core
{
    /// <summary>
    /// 实例创建异常
    /// </summary>
    public class InstanceCreateException : HimallException
    {
        public InstanceCreateException() { }

        public InstanceCreateException(string message) : base(message) { }

        public InstanceCreateException(string message, Exception inner) : base(message, inner) { }
    }
}
