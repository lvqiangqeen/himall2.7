using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    public enum WXMsgType : int
    {
        /// <summary>
        /// 图文消息
        /// </summary>
        mpnews = 0,
        /// <summary>
        /// 文本
        /// </summary>
        text = 1,
        /// <summary>
        /// 语音
        /// </summary>
        voice = 2,
        /// <summary>
        /// 图片
        /// </summary>
        image = 3,
        /// <summary>
        /// 视频
        /// </summary>
        video = 4,
        /// <summary>
        /// 卡券
        /// </summary>
        wxcard = 5
    }
}
