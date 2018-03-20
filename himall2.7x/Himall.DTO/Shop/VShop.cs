using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.DTO
{
	public class VShop
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public long ShopId { get; set; }
		public System.DateTime CreateTime { get; set; }
		public int VisitNum { get; set; }
		public int buyNum { get; set; }
		public VShopInfo.VshopStates State { get; set; }
		private string logo { get; set; }
		private string backgroundImage { get; set; }
		public string Description { get; set; }
		public string Tags { get; set; }
		public string HomePageTitle { get; set; }
		public string WXLogo { get; set; }

		/// <summary>
		/// Logo路径
		/// </summary>
		public string Logo { get; set; }

		/// <summary>
		/// 背景图片路径
		/// </summary>
		public string BackgroundImage { get; set; }

		/// <summary>
		/// 显示排序，需数据补充
		/// </summary>
		public int? ShowSequence { get; set; }
	}
}
