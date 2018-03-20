using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
	public class Brand
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public long DisplaySequence { get; set; }
		private string logo { get; set; }
		public string RewriteName { get; set; }
		public string Description { get; set; }
		public string Meta_Title { get; set; }
		public string Meta_Description { get; set; }
		public string Meta_Keywords { get; set; }
		public bool IsRecommend { get; set; }
		public bool IsDeleted { get; set; }

		/// <summary>
		/// 品牌Logo
		/// </summary>
		public string Logo { get; set; }
	}
}
