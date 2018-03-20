using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
	public class ProductAttribute
	{
		public long Id { get; set; }
		public long ProductId { get; set; }
		public long AttributeId { get; set; }
		public long ValueId { get; set; }
	}
}
