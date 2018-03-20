using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Himall.Core;
using Himall.CommonModel;

namespace Himall.Model
{
    public partial class FightGroupsInfo
    {
        /// <summary>
        /// 店铺名称
        /// </summary>
        [NotMapped]
        public string ShopName { get; set; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        [NotMapped]
        public string ShopLogo { get; set; }
        /// <summary>
        /// 正常售价
        /// </summary>
        public decimal SalePrice { get; set; }
        /// <summary>
        /// 商品名称
        ///</summary>
        [NotMapped]
        public string ProductName { get; set; }
        /// <summary>
        /// 商品图片目录
        /// </summary>
        [NotMapped]
        public string ProductImgPath { get; set; }
        /// <summary>
        /// 商品默认图片
        [NotMapped]
        /// </summary>
        public string ProductDefaultImage { get; set; }
        /// <summary>
        /// 团长用户名
        /// </summary>
        [NotMapped]
        public string HeadUserName { get; set; }
        /// <summary>
        /// 团长头像
        /// </summary>
        [NotMapped]
        public string HeadUserIcon { get; set; }
        /// <summary>
        /// 团长头像显示
        /// <para>默认头像值补充</para>
        /// </summary>
        [NotMapped]
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
        /// 数据状态 成团中  成功   失败
        ///</summary>
        [NotMapped]
        public FightGroupBuildStatus BuildStatus { get
            {
                return (FightGroupBuildStatus)this.GroupStatus;
            }
        }
        /// <summary>
        /// 拼团订单集
        /// </summary>
        public List<FightGroupOrderInfo> GroupOrders { get; set; }
        /// <summary>
        /// 团组时限（秒）
        /// </summary>
        public int? Seconds { get; set; }
        
    }
}
