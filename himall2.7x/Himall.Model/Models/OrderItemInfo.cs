namespace Himall.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class OrderItemInfo:BaseModel
    {
        public string ProductCode { get; set; }

        public long FreightId { set; get; }

        //总共的运费
        public decimal Freight { set; get; }

        public string ColorAlias { get; set; }
        public string SizeAlias { get; set; }
        public string VersionAlias { get; set; }
    }
}
