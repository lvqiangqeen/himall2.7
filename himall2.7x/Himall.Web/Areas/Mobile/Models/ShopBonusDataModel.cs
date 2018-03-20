using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Mobile.Models
{
    public class ShopBonusDataModel
    {
        public long GrantId { get; set; }

        public string ShareHref { get; set; }

        public string HeadImg { get; set; }

        public string ShopAddress { get; set; }

        public string UserName { get; set; }

        public decimal Price { get; set; }

        public string OpenId { get; set; }

        public string ShopName { get; set; }
    }
}