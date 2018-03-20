using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
///微信素材
namespace Himall.Model
{
    public class WXMaterialInfo
    {
       /// <summary>
       /// 标题
       /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 图文消息的封面图片素材id
        /// </summary>
        public string thumb_media_id { get; set; }
        /// <summary>
        /// 作者
        /// </summary>
        public string author { get; set; }
        /// <summary>
        /// 图文消息的摘要(多图文)
        /// </summary>
        public string digest { get; set; }
        /// <summary>
        /// 是否显示封面(0/1)
        /// </summary>
        public string show_cover_pic { get; set; }
        /// <summary>
        /// 图文消息的具体内容，支持HTML标签
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 图文消息的原文地址
        /// </summary>
        public string content_source_url { get; set; }

        public string url { get; set; }
    }

    public class MediaNewsItemList
    {
        public MediaNewsItemList()
        {

        }

        public IEnumerable<MediaNewsItem> content { get; set; }
        /// <summary>
        /// 素材总数
        /// </summary>
        public int total_count { get; set; }
        /// <summary>
        /// 获取的素材数量
        /// </summary>
        public int count { get; set; }
        /// <summary>
        /// 微信接口错误码
        /// </summary>
        public string errCode { get; set; }
        /// <summary>
        /// 微信接口错误信息
        /// </summary>
        public string errMsg { get; set; }
    }

    public class MediaNewsItem
    {
        public MediaNewsItem()
        {

        }

        public IEnumerable<WXMaterialInfo> items { get; set; }
        public string media_id { get; set; }
        public string update_time { get; set; } 
    }

    public class MediaItemCount
    {
        /// <summary>
        /// 语音总数量
        /// </summary>
        public int voice_count { get; set; }
        /// <summary>
        /// 视频总数量
        /// </summary>
        public int video_count { get; set; }
        /// <summary>
        /// 图片总数量
        /// </summary>
        public int image_count { get; set; }
        /// <summary>
        /// 图文总数量
        /// </summary>
        public int news_count { get; set; }
        /// <summary>
        /// 微信接口错误码
        /// </summary>
        public string errCode { get; set; }
        /// <summary>
        /// 微信接口错误信息
        /// </summary>
        public string errMsg { get; set; }
    }

    public class WXUploadNewsResult : WxJsonResult
    {
        public string media_id { get; set; }
    }
    public class WxJsonResult
    {
        public string errmsg { get; set; }
    }
   

}
