using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
{
    public class FightGroupActiveDetailModel : Himall.DTO.FightGroupActiveModel
    {
        /// <summary>
        /// 分享图片
        /// </summary>
        public string ShareImage { get; set; }
        /// <summary>
        /// 分享标题
        /// </summary>
        public string ShareTitle { get; set; }
        /// <summary>
        /// 分享链接
        /// </summary>
        public string ShareUrl { get; set; }
        /// <summary>
        /// 分享描述
        /// </summary>
        public string ShareDesc { get; set; }
    }
}
