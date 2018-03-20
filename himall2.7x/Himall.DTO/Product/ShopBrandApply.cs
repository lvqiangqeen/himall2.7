using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
	public class ShopBrandApply
	{
		public int Id { get; set; }
		public long ShopId { get; set; }
		public Nullable<long> BrandId { get; set; }
		public string BrandName { get; set; }
		public string Logo { get; set; }
		public string Description { get; set; }
		public string AuthCertificate { get; set; }
		public int ApplyMode { get; set; }
		public string Remark { get; set; }
		public int AuditStatus { get; set; }
		public System.DateTime ApplyTime { get; set; }
	}
}
