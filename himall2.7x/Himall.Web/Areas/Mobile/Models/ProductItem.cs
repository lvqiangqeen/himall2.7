
namespace Himall.Web.Areas.Mobile.Models
{
    public class ProductItem
    {

        public long Id { get; set; }


        public string Name { get; set; }


        public decimal MarketPrice { get; set; }


        public decimal SalePrice { get; set; }

        public string ImageUrl { get; set; }

        public int CommentsCount { get; set; }

    }
}