using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{

    public class SearchProductModel
    {
        public List<BrandInfo> Brands { get; set; }

        public List<TypeAttributesModel> ProductAttrs { get; set; }
    }
}
