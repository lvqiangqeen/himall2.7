using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Web.Models
{
    /// <summary>
    /// 品牌
    /// </summary>
    public class WebFloorBrand
    {
        public long Id { get; set; }

        public string Url { get; set; }
        public string Img { get; set; }
        public string Name
        {
            get;
            set;
        }
    }
}