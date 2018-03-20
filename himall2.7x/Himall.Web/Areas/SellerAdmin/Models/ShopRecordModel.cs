using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class ShopRecordModel
    {
        public int Id { get; set; }
        public string OperateDate { get; set; }

        public string OperateType { get; set; }

        public decimal Amount { get; set; }

        public string Content { get; set; }

        public string Operate { get; set; }
    }
}