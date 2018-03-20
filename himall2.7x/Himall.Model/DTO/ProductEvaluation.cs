using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Model
{
    public class ProductEvaluation : BaseModel
    {



        /// <summary>
        /// 订单详情ID
        /// </summary>
        public new long Id { set; get; }


        /// <summary>
        /// 评论的ID
        /// </summary>
        public long CommentId { set; get; }

        /// <summary>
        /// 商品ID
        /// </summary>
        public long ProductId { set; get; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { set; get; }

        /// <summary>
        /// 商品缩略图
        /// </summary>
        public string ThumbnailsUrl { set; get; }

        /// <summary>
        /// 购买时间
        /// </summary>
        public DateTime BuyTime { set; get; }

        /// <summary>
        /// 评价状态
        /// </summary>
        public bool EvaluationStatus { set; get; }

        /// <summary>
        /// 评价内容
        /// </summary>
        public string EvaluationContent { set; get; }


        public string AppendContent { set; get; }


        public DateTime? AppendTime { set; get; }

        /// <summary>
        /// 评价时间
        /// </summary>
        public DateTime EvaluationTime { set; get; }


        /// <summary>
        /// 回复时间
        /// </summary>
        public DateTime ?ReplyTime { set; get; }

        /// <summary>
        /// 回复内容
        /// </summary>
        public string ReplyContent { set; get; }


        /// <summary>
        /// 回复时间
        /// </summary>
        public DateTime? ReplyAppendTime { set; get; }

        /// <summary>
        /// 回复内容
        /// </summary>
        public string ReplyAppendContent { set; get; }


        /// <summary>
        /// 评分
        /// </summary>
        public int EvaluationRank { set; get; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public long OrderId { get; set; }

        public ProductCommentInfo ProductComment { get; set; }

        /// <summary>
        /// 评论图片
        /// </summary>
        public List<ProductCommentsImagesInfo> CommentImages { set; get; }

        //日龙添加
        //7.16号， 显示出商品的规格
        /// <summary>
        /// 颜色
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// 尺寸
        /// </summary>
        public string Size { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; }

        public string ColorAlias { get; set; }
        public string SizeAlias { get; set; }
        public string VersionAlias { get; set; }

        public string SkuId { get; set; }

        public decimal Price { get; set; }
       
    }
}