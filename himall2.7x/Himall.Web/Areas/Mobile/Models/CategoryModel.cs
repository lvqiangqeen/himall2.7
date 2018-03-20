using System.Collections.Generic;

namespace Himall.Web.Areas.Mobile
{
    public class CategoryModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Image { get; set; }

        public int Depth { get; set; }
        public long DisplaySequence { get; set; }

        public IEnumerable<CategoryModel> SubCategories { get; set; }

    }
}