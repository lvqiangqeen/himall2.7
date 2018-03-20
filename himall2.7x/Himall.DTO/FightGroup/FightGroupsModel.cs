using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;
using Himall.CommonModel;

namespace Himall.DTO
{
    /// <summary>
    /// 拼团组团详情
    /// </summary>
    public class FightGroupsModel
    {
        /// <summary>
        /// 编号
        ///</summary>
        public long Id { get; set; }
        /// <summary>
        /// 团长用户编号
        ///</summary>
        public long HeadUserId { get; set; }
        /// <summary>
        /// 当前订单的用户编号
        /// </summary>
        //public long OrderUserId { get; set; }
        /// <summary>
        /// 对应活动
        ///</summary>
        public long ActiveId { get; set; }
        /// <summary>
        /// 店铺编号
        ///</summary>
        public long ShopId { get; set; }
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { get; set; }
        public string ShopLogo { get; set; }
        /// <summary>
        /// 商品编号
        ///</summary>
        public long ProductId { get; set; }
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
        /// 参团人数限制
        ///</summary>
        public int LimitedNumber { get; set; }
        /// <summary>
        /// 时间限制
        ///</summary>
        public decimal LimitedHour { get; set; }
        /// <summary>
        /// 已参团人数
        ///</summary>
        public int JoinedNumber { get; set; }
        /// <summary>
        /// 是否异常
        ///</summary>
        public bool IsException { get; set; }
        /// <summary>
        /// 数据状态 成团中  成功   失败
        ///</summary>
        public FightGroupBuildStatus BuildStatus { get; set; }
        /// <summary>
        /// 开团时间
        ///</summary>
        public DateTime AddGroupTime { get; set; }
        /// <summary>
        /// 结束时间 成功或失败的时间
        ///</summary>
        public DateTime? OverTime { get; set; }
        /// <summary>
        /// 团长用户名
        /// </summary>
        public string HeadUserName { get; set; }
        /// <summary>
        /// 团长头像
        /// </summary>
        public string HeadUserIcon { get; set; }
        /// <summary>
        /// 团长头像显示
        /// <para>默认头像值补充</para>
        /// </summary>
        public string ShowHeadUserIcon
        {
            get
            {
                string defualticon = "";
                string result = HeadUserIcon;
                if (string.IsNullOrWhiteSpace(result))
                {
                    result = defualticon;
                }
                return result;
            }
        }
        /// <summary>
        /// 拼团结束时间
        /// </summary>
        public DateTime GroupEndTime
        {
            get
            {
                DateTime result =this.AddGroupTime.AddHours((double)this.LimitedHour);
                return result;
            }
        }
        /// <summary>
        /// 拼团订单集
        /// </summary>
        public List<FightGroupOrderModel> GroupOrders { get; set; }
    }
}
