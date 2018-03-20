using Himall.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Himall.ViewModel
{
    public class LabelModel 
    {
        /// <summary>
        /// 标签会员数量
        /// </summary>
        public long MemberNum { get; set; }
        [Required(ErrorMessage = "标签名称为必填项")]
        [MaxLength(15, ErrorMessage = "标签名称最多15个字符")]
        public string LabelName { get; set; }
        public long Id { get; set; }
    }
}