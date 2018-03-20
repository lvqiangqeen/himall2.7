
namespace Himall.Web.Areas.Admin.Models
{
    public class ArticleCategoryModel
    {
        public string Name { get; set; }


        public long Id { get; set; }

        public long DisplaySequence { get; set; }


        public long ParentId { get; set; }

        public bool HasChild { get; set; }

        public int Depth { get; set; }

        public bool IsDefault { get; set; }
    }
}