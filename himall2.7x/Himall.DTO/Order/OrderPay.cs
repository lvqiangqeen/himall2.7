using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
	public class OrderPay
	{
		public long Id { get; set; }
		public long OrderId { get; set; }
		public long PayId { get; set; }
		public bool PayState { get; set; }
		public DateTime? PayTime { get; set; }
	}
}
