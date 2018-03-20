using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Mobile.Models
{
	public class ShopBranchModel
	{
		public int ShopId{get;set;}
		public int RegionId{get;set;}
		public string[] SkuIds{get;set;}
		public int[] Counts { get; set; }
	}
}