using Himall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
///发关消息
namespace Himall.Model
{
    /// <summary>
    /// 微信群发消息
    /// </summary>
    public class SendMsgInfo {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string MediaId { get; set; }
        public string Content { get; set; }

        public SendToUserLabel ToUserLabel { get; set; }

        public WXMsgType MsgType { get; set; }
        public string ToUserDesc { get; set; }
    }
    public class SendToUserLabel {
        public long[] LabelIds { get; set; }
        public Himall.CommonModel.SexType? Sex { get; set; }
        public long? ProvinceId { get; set; }
    }
    public class SendInfoResult
    {
        
        /// <summary>
        /// 微信接口错误码
        /// </summary>
        public string errCode { get; set; }
        /// <summary>
        /// 微信接口错误信息
        /// </summary>
        public string errMsg { get; set; }
    }
}
