using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Model
{
    public class AreaMapExportModel
    {
        public List<AreaMapExportSubModel> Details { get; set; }

        public int MaxMemberCount { get; set; }

        public int MinMemberCount { get; set; }

        public decimal MaxOrderMoney { get; set; }

        public decimal MinOrderMoney { get; set; }
    }

    public class AreaMapExportSubModel
    {
        public int RegionID { get; set; }

        public string RegionName { get; set; }

        public int MemberCount { get; set; }

        public decimal OrderMoney { get; set; }
    }
}