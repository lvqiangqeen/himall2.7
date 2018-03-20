using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.DTO
{
	public class SpecificationValue
	{
		public new long Id { get; set; }
		public SpecificationType Specification { get; set; }
		public long TypeId { get; set; }
		public string Value { get; set; }
	}
}
