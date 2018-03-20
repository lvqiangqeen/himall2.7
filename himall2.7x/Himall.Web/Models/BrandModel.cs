using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Himall.Web.Models
{
    public class BrandModel
    {
        public long ID { set; get; }

        /// <summary>
        /// 品牌名称
        /// </summary>
        /// 
        [Required(ErrorMessage="品牌必须填写")]
        [RegularExpression("^[a-zA-Z0-9_\u4e00-\u9fa5]+$",ErrorMessage="品牌名称必须是中文，字母，数字和下划线")]
        [MaxLength(20,ErrorMessage="品牌名称不能超过20个字符")]
        public string BrandName { set; get; }

        /// <summary>
        /// 品牌英文名
        /// </summary>
        public string BrandEnName { set; get; }

        /// <summary>
        /// 是否推荐
        /// </summary>
        public bool IsRecommend { set; get; }

        /// <summary>
        /// 品牌描述
        /// </summary>
        [MaxLength(200,ErrorMessage="品牌描述不能超过200个字符")]
        public string BrandDesc { set; get; }

        /// <summary>
        /// 品牌LOGO
        /// </summary>
        [Required(ErrorMessage="品牌图片不能为空！")]
        public string BrandLogo { set; get; }

        /// <summary>
        /// SEO标题
        /// </summary>
        [MaxLength(500, ErrorMessage = "SEO标题不能超过500个字符")]
        public string MetaTitle { set; get; }

        /// <summary>
        /// SEO关键字
        /// </summary>
        [MaxLength(500, ErrorMessage = "SEO关键字不能超过500个字符")]
        public string MetaKeyWord { set; get; }

        /// <summary>
        /// SEO描述
        /// </summary>
        [MaxLength(500, ErrorMessage = "SEO描述不能超过500个字符")]
        public string MetaDescription { set; get; }

        /// <summary>
        /// 显示顺序
        /// </summary>
        public int DisplaySequence { set; get; }

    }
}