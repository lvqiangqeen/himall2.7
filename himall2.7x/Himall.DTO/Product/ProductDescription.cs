using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
	public class ProductDescription
	{
		public new long Id { get; set; }
		public long ProductId { get; set; }
		public string Description { get; set; }
		public long DescriptionPrefixId { get; set; }
		public long DescriptiondSuffixId { get; set; }
		public string Meta_Title { get; set; }
		public string Meta_Description { get; set; }
		public string Meta_Keywords { get; set; }
		public string AuditReason { get; set; }
		public string MobileDescription { get; set; }
		public string ShowMobileDescription { get; set; }
	}
}
