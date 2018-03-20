using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Mobile.Models
{
    /// <summary>
    /// 商品评价部份显示
    /// </summary>
    public class ProductCommentShowModel
    {
        public long ProductId { get; set; }
        public int CommentCount { get; set; }
        public bool IsShowColumnTitle { get; set; }
        public bool IsShowCommentList { get; set; }
        public List<ProductDetailCommentModel> CommentList { get; set; }
    }
}