using System;

namespace Himall.Core
{
    public class CacheRegisterException : HimallException
    {
        public CacheRegisterException() { }

        public CacheRegisterException(string message) : base(message) { }

        public CacheRegisterException(string message, Exception inner) : base(message, inner) { }
    }
}
