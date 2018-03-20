using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
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
