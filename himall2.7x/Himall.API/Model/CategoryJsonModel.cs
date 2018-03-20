using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model
{

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
