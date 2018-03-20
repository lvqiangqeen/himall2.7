using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model
{
    public class ProductDetailCommentModel
    {
        public ProductDetailCommentModel()
        {
            Images = new List<string>();
            AppendImages = new List<string>();
        }
        public string Sku { get; set; }
        public string UserName { get; set; }
        public string ReviewContent { get; set; }
        public string AppendContent { get; set; }
        public DateTime? AppendDate { get; set; }
        public string ReplyAppendContent { get; set; }
        public DateTime? ReplyAppendDate { get; set; }
        public DateTime? FinshDate { get; set; }
        public List<string> Images { get; set; }
        public List<string> AppendImages { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string ReplyContent { get; set; }
        public DateTime? ReplyDate { get; set; }
        public int ReviewMark { get; set; }
        public DateTime? BuyDate { get; set; }
    }
}
