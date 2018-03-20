using System;
namespace Himall.Core
{
	public class HimallIOException : HimallException
	{
		public HimallIOException()
		{
		}
		public HimallIOException(string message) : base(message)
		{
		}
        public HimallIOException(string message, Exception inner)
            : base(message, inner)
		{
		}
	}
}
