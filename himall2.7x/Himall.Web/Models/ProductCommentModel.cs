using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Models
{
    public class ProductCommentModel
    {
        /// <summary>
        /// 商品评论
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 产品ID
        /// </summary>
        public long ProductId { get; set; }
        /// <summary>
        /// 店铺ID
        /// </summary>
        /// 
        public long ShopId { get; set; }

        /// <summary>
        /// 产品图片地址
        /// </summary>
        public string ImagePath { set; get; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { set; get; }

        //产品图片
        public string ProductPic { set; get; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户邮件
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 评论内容
        /// </summary>
        public string CommentContent { get; set; }


  /// <summary>
        /// 追加内容
        /// </summary>
        public string AppendContent { get; set; }

        /// <summary>
        /// 追加时间
        /// </summary>
        public DateTime? AppendDate { get; set; }

        /// <summary>
        /// 评论时间
        /// </summary>
        public System.DateTime CommentDate { get; set; }

        public string CommentDateStr
        {
            get
            {
                return CommentDate.ToString( "yyyy-MM-dd HH:mm" );
            }
        }

        public string AppendDateStr
        {
            get
            {
                if (AppendDate.HasValue)
                    return AppendDate.Value.ToString("yyyy-MM-dd HH:mm");
                else
                    return "";
            }
        }

        /// <summary>
        /// 隐藏
        /// </summary>
        public bool IsHidden { set; get; }

        /// <summary>
        /// 回复内容
        /// </summary>
        public string ReplyContent { get; set; }
        /// <summary>
        /// 回复时间
        /// </summary>
        public System.DateTime? ReplyDate { get; set; }


        /// 回复时间
        /// </summary>
        public System.DateTime? ReplyAppendDate { get; set; }

        /// <summary>
        /// 回复状态
        /// </summary>
        public bool Status { get {
            return (ReplyDate.HasValue && !AppendDate.HasValue)||(ReplyAppendDate.HasValue);
        
        } }

        /// <summary>
        /// 评价分数
        /// </summary>
        public int CommentMark { get; set; }

        public string Date
        {
            get
            {
                return CommentDate.ToString( "yyyy-MM-dd HH:mm" );
            }
        }

        //TODO LRL 2015/08/06 添加规格属性
        public string Color { get; set; }
        public string Size { get; set; }
        public string Version { get; set; }

        public string ColorAlias { get; set; }
        public string SizeAlias { get; set; }
        public string VersionAlias { get; set; }
        public Nullable<long> OderItemId { get; set; }

        /// <summary>
        /// 订单ID
        /// </summary>
        public long OrderId { get; set; }
        

        public string UserPhone { get; set; }
    }
}