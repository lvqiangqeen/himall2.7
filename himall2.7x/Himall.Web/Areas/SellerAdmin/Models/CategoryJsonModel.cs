using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.SellerAdmin.Models
{
	public class CategoryGroup
	{
		public long Id
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public List<CategoryGroup> SubCategorys
		{
			get;
			set;
		}

		public string Path
		{
			get;
			set;
		}

		public long TypeId
		{
			get;
			set;
		}
	}

    public class CategoryJsonModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public List<SecondLevelCategory> SubCategory { get; set; }
    }

    public class SecondLevelCategory
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public List<ThirdLevelCategoty> SubCategory { get; set; }
    }

    public class ThirdLevelCategoty
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class SelectedCategory
    {
        public string Id { get; set; }
        public string Level { get; set; }
    }

    public class ShopProductCategoryModel
    {
        public List<CategoryJsonModel> Data { get; set; }
        public List<SelectedCategory> SelectedCategory { get; set; }
    }
}