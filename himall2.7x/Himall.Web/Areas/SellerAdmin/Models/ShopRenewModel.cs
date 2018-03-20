using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class ShopRenewModel
    {
        public long ShopId { get; set; }

        public string ShopName { get; set; }

        public string ShopCreateTime { get; set; }

        public string ShopEndTime { get; set; }

        public int ProductLimit { get; set; }

        public int ImageLimit { get; set; }

        public long GradeId { get; set; }

        public string GradeName { get; set; }
    }
}