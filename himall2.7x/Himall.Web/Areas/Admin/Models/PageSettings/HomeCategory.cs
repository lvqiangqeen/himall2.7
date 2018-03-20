using System.Collections.Generic;

namespace Himall.Web.Areas.Admin.Models
{
    public class HomeCategory
    {
        public IEnumerable<string> TopCategoryNames { get; set; }

        public IEnumerable<long> AllCategoryIds { get; set; }

        public int RowNumber { get; set; }

    }
}