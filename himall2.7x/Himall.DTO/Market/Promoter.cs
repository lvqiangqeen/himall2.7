using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.DTO
{
	public class Promoter
	{
		public new long Id { get; set; }
		public Nullable<long> UserId { get; set; }
		public string ShopName { get; set; }
		public PromoterInfo.PromoterStatus Status { get; set; }
		public DateTime? ApplyTime { get; set; }
		public DateTime? PassTime { get; set; }
		public string Remark { get; set; }
	}
}
