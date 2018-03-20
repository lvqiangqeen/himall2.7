using System;
using System.Collections.Generic;

namespace Himall.SmallProgAPI.Model
{
    public class OrderCommentModel
    {
        public long UserId { get; set; }

        public string UserName { get; set; }

        public string UserEmail { get; set; }
        public DateTime ReviewDate { get; set; }
        public long ProductId { get; set; }
        public string ReviewText { get; set; }
        public string OrderId { get; set; }
        public string SkuId { get; set; }
        public string SkuContent { get; set; }
        public int Score { get; set; }
        public string ImageUrl1 { get; set; }
        public string ImageUrl2 { get; set; }
        public string ImageUrl3 { get; set; }
        public string ImageUrl4 { get; set; }
        public string ImageUrl5 { get; set; }
        /// <summary>
        /// 回复内容
        /// </summary>
        public string ReplyText { get; set; }
        /// <summary>
        /// 回复时间
        /// </summary>
        public DateTime? ReplyDate { get; set; }
    }
}