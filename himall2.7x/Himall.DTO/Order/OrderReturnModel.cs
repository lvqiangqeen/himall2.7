using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 返回实体
    /// </summary>
    public class OrderReturnModel
    {
		public bool Success { get; set; }

		public long[] OrderIds { get; set; }

		public decimal OrderTotal { get; set; }
	}
}

