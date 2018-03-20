using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.CommonModel;

namespace Himall.Web.Areas.Mobile.Models
{
    /// <summary>
    /// 我参与的拼团列表
    /// </summary>
    public class MyFightGroupPostJoinGroupsModel
    {
        /// <summary>
        /// 编号
        ///</summary>
        public long Id { get; set; }
        /// <summary>
        /// 对应活动
        ///</summary>
        public long ActiveId { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public long OrderId { get; set; }
        /// <summary>
        /// 火拼价
        /// <para>单价</para>
        /// </summary>
        public decimal GroupPrice { get; set; }
        /// <summary>
        /// 商品名称
        ///</summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 商品图片目录
        /// </summary>
        public string ProductImgPath { get; set; }
        /// <summary>
        /// 商品默认图片
        /// </summary>
        public string ProductDefaultImage { get; set; }
        /// <summary>
        /// 还差人数
        ///</summary>
        public int NeedNumber { get; set; }
        /// <summary>
        /// 数据状态 成团中  成功   失败
        ///</summary>
        public FightGroupBuildStatus BuildStatus { get; set; }
        /// <summary>
        /// 成团最后时间
        /// </summary>
        public DateTime GroupEndTime { get; set; }

        public string ShowGroupEndTime { get
            {
                string result = "";
                if (GroupEndTime.Date == DateTime.Now.Date)
                {
                    result = GroupEndTime.ToString("HH:mm:ss");
                }
                else
                {
                    result = GroupEndTime.ToString("yyyy-MM-dd");
                }
                return result;
            }
        }
        /// <summary>
        /// 拼团的用户头像集
        /// </summary>
        public List<string> UserIcons { get; set; }
    }
}