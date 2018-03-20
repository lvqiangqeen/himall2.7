using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Admin.Models.Market
{
    public class BonusReceiveModel
    {
        public string OpenId
        {
            get;
            set;
        }

        public string UserName { get; set; }

        public string ReceiveTime
        {
            get;
            set;
        }

        public decimal Price
        {
            get;
            set;
        }

        public bool IsTransformedDeposit { get; set; }
    }
}