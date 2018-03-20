using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Himall.Model;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class ShopDistributorAdvPageSetModel
    {
        public long ShopId { get; set; }
        [Required(ErrorMessage = "请填写链接名称")]
        [MaxLength(100, ErrorMessage = "超过100字符")]
        public string DistributorShareName { get; set; }
        [Required(ErrorMessage = "请填写详情描述")]
        [MaxLength(100, ErrorMessage = "超过200字符")]
        public string DistributorShareContent { get; set; }
        [Required(ErrorMessage = "请上传分享Logo")]
        [MaxLength(100, ErrorMessage = "超过100字符")]
        public string DistributorShareLogo { get; set; }
    }
}