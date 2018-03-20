using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
	public class ProductComment
	{
		public long Id { get; set; }
		public long ProductId { get; set; }
		public long ShopId { get; set; }
		public string ShopName { get; set; }
		public long UserId { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string ReviewContent { get; set; }
		public System.DateTime ReviewDate { get; set; }
		public string ReplyContent { get; set; }
		public Nullable<System.DateTime> ReplyDate { get; set; }
		public int ReviewMark { get; set; }
		public Nullable<long> SubOrderId { get; set; }
		public string AppendContent { get; set; }
		public Nullable<System.DateTime> AppendDate { get; set; }
		public string ReplyAppendContent { get; set; }
		public Nullable<System.DateTime> ReplyAppendDate { get; set; }
		public Nullable<bool> IsHidden { get; set; }

		public List<ProductCommentImage> Images { get; set; }
	}

	public class ProductCommentImage
	{
		public long Id { get; set; }
		public string CommentImage { get; set; }
		public long CommentId { get; set; }
		public int CommentType { get; set; }
	}
}
