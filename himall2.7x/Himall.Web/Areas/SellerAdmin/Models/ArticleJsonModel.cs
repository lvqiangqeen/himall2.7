using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class ArticleJsonModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string AddDate { get; set; }
    }
}