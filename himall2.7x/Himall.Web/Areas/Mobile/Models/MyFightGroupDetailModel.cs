using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.DTO;
using Himall.Web.Models;

namespace Himall.Web.Areas.Mobile.Models
{
    public class MyFightGroupDetailModel
    {
        /// <summary>
        /// 活动数据
        /// </summary>
        public FightGroupActiveModel ActiveData { get; set; }
        /// <summary>
        /// 拼团数据
        /// </summary>
        public FightGroupsModel GroupsData { get; set; }
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
        /// <summary>
        /// 微信分享参数
        /// </summary>
        public CommonModel.Model.WeiXinShareArgs WeiXinShareArgs { get; set; }
    }
}