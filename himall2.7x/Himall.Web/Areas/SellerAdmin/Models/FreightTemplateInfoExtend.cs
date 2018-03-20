using Himall.CommonModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class FreightTemplateInfoExtend 
    {
        public FreightTemplateInfoExtend()
        {
            this.FreightAreaContent = new HashSet<FreightAreaContentInfoExtend>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public Nullable<int> SourceAddress { get; set; }
        public string SourceAddressStr { get; set; }
        public string SendTime { get; set; }
        public FreightTemplateType IsFree { get; set; }
        public ValuationMethodType ValuationMethod { get; set; }
        public Nullable<int> ShippingMethod { get; set; }
        public long ShopID { get; set; }

        public virtual IEnumerable<FreightAreaContentInfoExtend> FreightAreaContent { get; set; }
    }
    public class FreightTemplateInfoModel
    {
        public FreightTemplateInfoModel()
        {
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public long ShopID { get; set; }

        public string ValuationMethod { set; get; }

        public bool IsFree { set; get; }

    }
}