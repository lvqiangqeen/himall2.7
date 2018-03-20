using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
	public class ShopCategory
	{
		public long Id { get; set; }
		public long ShopId { get; set; }
		public long ParentCategoryId { get; set; }
		public string Name { get; set; }
		public long DisplaySequence { get; set; }
		public bool IsShow { get; set; }
	}
}
