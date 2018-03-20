using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    public partial class SlideAdModel
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
        public long DisplaySequence { get; set; }
        public Model.SlideAdInfo.SlideAdType TypeId { get; set; }
        public string Description { get; set; }
    }
}
