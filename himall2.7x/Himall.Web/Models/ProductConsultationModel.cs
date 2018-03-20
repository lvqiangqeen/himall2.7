using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Models
{
    public class ProductConsultationModel
    {
        /// <summary>
        /// 产品咨询ID
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 产品ID
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { set; get; }

        /// <summary>
        /// 产品图片地址
        /// </summary>
        public string ImagePath { set; get; }

        //产品图片
        public string ProductPic { set; get; }

        /// <summary>
        /// 店铺id
        /// </summary>
        public long ShopId { get; set; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户邮箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 咨询内容
        /// </summary>
        public string ConsultationContent { get; set; }
        /// <summary>
        /// 咨询时间
        /// </summary>
        public System.DateTime ConsultationDate { get; set; }
        public string ConsultationDateStr
        {
            get;
            set;
        }
        /// <summary>
        /// 回复内容
        /// </summary>
        public string ReplyContent { get; set; }
        /// <summary>
        /// 回复时间
        /// </summary>
        public System.DateTime? ReplyDate { get; set; }

        /// <summary>
        /// 回复状态
        /// </summary>
        public bool Status { get { return ReplyDate.HasValue; } }

        public string Date
        {
            get; 
            set;
        }


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
    }
}