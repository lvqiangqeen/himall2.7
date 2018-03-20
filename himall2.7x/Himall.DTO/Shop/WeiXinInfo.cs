using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    public class WeiXinInfo
    {
        /// <summary>
        /// 微信识别码
        /// </summary>
        public string OpenId { get; set; }

        /// <summary>
        /// 微信昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public string sex { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        public string province { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string city { get; set; }

        /// <summary>
        /// 微信头像
        /// </summary>
        public string headimgurl { get; set; }
    }
}
