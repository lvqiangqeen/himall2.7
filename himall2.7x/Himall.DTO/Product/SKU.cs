using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
	public class SKU
	{
		public new string Id { get; set; }
		public long ProductId { get; set; }
		public string Color { get; set; }
		public string Size { get; set; }
		public string Version { get; set; }
		public string Sku { get; set; }
		public long Stock { get; set; }
		public decimal CostPrice { get; set; }
		public decimal SalePrice { get; set; }
		public long AutoId { get; set; }
		public string ShowPic { get; set; }
		public long? SafeStock { get; set; }
        public string ColorAlias { get; set; }
        public string SizeAlias { get; set; }
        public string VersionAlias { set; get; }
    }
}
