using System;

namespace Himall.Core
{
    /// <summary>
    /// Himall 异常
    /// </summary>
    public class HimallException : ApplicationException
    {
        public HimallException() {
            Log.Info(this.Message, this);
        }

        public HimallException(string message) : base(message) {
            Log.Info(message, this);
        }

        public HimallException(string message, Exception inner) : base(message, inner) {
            Log.Info(message, inner);
        }

    }
}
