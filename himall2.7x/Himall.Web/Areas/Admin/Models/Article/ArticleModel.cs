using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Admin.Models
{
    public class ArticleModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "文章分类必选")]
        public long? CategoryId { get; set; }


        [Required(ErrorMessage="文章标题必填")]
        [MaxLength(25,ErrorMessage="最多25个字符")]
        [MinLength(3,ErrorMessage="最少3个字符")]
        public string Title { get; set; }

        public string IconUrl { get; set; }

        //[MaxLength(20000, ErrorMessage = "最多20000个字符")]
        [Required(ErrorMessage = "品牌简介必填")]
        public string Content { get; set; }
        public string Meta_Title { get; set; }
        public string Meta_Description { get; set; }
        public string Meta_Keywords { get; set; }
        public bool IsRelease { get; set; }


        public string ArticleCategoryFullPath { get; set; }
    }
}