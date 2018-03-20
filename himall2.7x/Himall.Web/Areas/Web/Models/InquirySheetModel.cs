using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Web.Models
{
    public class InquirySheetModel
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string PublishDate { get; set; }

        public string EndDate { get; set; }

        public string Status { get; set; }
    }
}