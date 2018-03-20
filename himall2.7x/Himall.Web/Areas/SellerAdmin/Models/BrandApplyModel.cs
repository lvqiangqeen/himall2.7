using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class BrandApplyModel
    {
        public long Id { set; get; }

        public long ShopId { set; get; }

        public long BrandId { set; get; }

        /// <summary>
        /// 品牌名称
        /// </summary>
        /// 
        [Required(ErrorMessage = "品牌必须填写")]
        public string BrandName { set; get; }

        /// <summary>
        /// 品牌描述
        /// </summary>
        public string BrandDesc { set; get; }

        /// <summary>
        /// 品牌LOGO
        /// </summary>
        [Required(ErrorMessage = "品牌图片不能为空！")]
        public string BrandLogo { set; get; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 品牌授权证书
        /// </summary>
        [Required(ErrorMessage = "品牌授权证书不能为空！")]
        public string BrandAuthPic { get; set; }

        /// <summary>
        /// 申请类型
        /// </summary>
        [Required(ErrorMessage = "申请类型不能为空！")]
        public int BrandMode { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>    
        public int AuditStatus { set; get; }

        public string ApplyTime { get; set; }

        public string ShopName { get; set; }
    }
}