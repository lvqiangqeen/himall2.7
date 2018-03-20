using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class InviteRuleModel
    {
        public long ID { set; get; }

        [Required(ErrorMessage = "邀请积分不能为空")]
        [RegularExpression(@"\d+", ErrorMessage = "积分必须是数字")]
        [Range(0, int.MaxValue, ErrorMessage = "积分必须大于等于0")]
        public Nullable<int> InviteIntegral { get; set; }
        [RegularExpression(@"\d+", ErrorMessage = "积分必须是数字")]
        [Range(0, int.MaxValue, ErrorMessage = "积分必须大于等于0")]
        public Nullable<int> RegIntegral { get; set; }
        [Required(ErrorMessage = "分享标题不能为空")]
        public string ShareTitle { get; set; }

        [Required(ErrorMessage = "分享描述不能为空")]
        [MaxLength(70, ErrorMessage = "最多70字")]
        public string ShareDesc { get; set; }
        public string ShareIcon { get; set; }

        [MaxLength(70,ErrorMessage="最多70字")]
        public string ShareRule { get; set; }
    }
}
