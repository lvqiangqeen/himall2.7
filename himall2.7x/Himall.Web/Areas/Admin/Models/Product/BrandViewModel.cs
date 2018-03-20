using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Admin.Models.Product
{
    public class BrandViewModel
    {
        public long id { get; set; }
        public bool isChecked { get; set; }
        public string value { get; set; }
    }
}