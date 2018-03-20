using System.Collections.Generic;

namespace Himall.Web.Areas.Web.Models
{
    public class PageFootServiceModel
    {
        public string CateogryName { get; set; }

        public IEnumerable<Himall.Model.ArticleInfo> Articles { get; set; }

    }
}