using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
	public class ShopBranchSku
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public int SkuId { get; set; }
		public int ShopId { get; set; }
		public int ShopBranchId { get; set; }
		public int Stock { get; set; }
	}
}
