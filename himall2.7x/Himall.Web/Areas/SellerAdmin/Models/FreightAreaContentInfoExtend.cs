using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public partial class FreightAreaContentInfoExtend 
    {
        public long Id { get; set; }
        public long FreightTemplateId { get; set; }
        public string AreaContent { get; set; }

        public string AreaContentCN { get; set; }
        public Nullable<int> FirstUnit { get; set; }
        public Nullable<float> FirstUnitMonry { get; set; }
        public Nullable<int> AccumulationUnit { get; set; }
        public Nullable<float> AccumulationUnitMoney { get; set; }
        public Nullable<sbyte> IsDefault { get; set; }

    }
}