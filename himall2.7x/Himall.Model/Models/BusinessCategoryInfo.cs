using System.ComponentModel.DataAnnotations.Schema;

namespace Himall.Model
{
    public partial class BusinessCategoryInfo
    {
        [NotMapped]
        public string CategoryName { get; set; }
    }
}
