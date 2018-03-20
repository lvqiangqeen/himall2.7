using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Admin.Models
{
    public class IntegralRule
    {
        public int Reg { set; get; }
        public int BindWX { set; get; }

        public int Login { set; get; }

        public int Comment { set; get; }
        public int Share { set; get; }

        public int MoneyPerIntegral { set; get; }
    }
}