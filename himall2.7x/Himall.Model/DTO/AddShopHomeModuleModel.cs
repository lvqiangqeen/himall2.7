using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model.DTO
{
    public class AddShopHomeModuleModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public List<AddShopHomeModuleProductModel> Products { get; set; }

        public List<AddShopHomeModuleTopImgModel> TopImgs { get; set; }

        public long ShopId { get; set; }
    }

    public class AddShopHomeModuleProductModel 
    {
        public int DisplaySequence { get; set; }

        public long ProductId { get; set; }
    }

    public class AddShopHomeModuleTopImgModel
    {
        public int DisplaySequence { get; set; }

        public string ImgPath { get; set; }

        public string Url { get; set; }
    } 
}
