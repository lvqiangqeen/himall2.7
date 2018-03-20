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
    /// 拼团订单
    /// </summary>
    public class FightGroupOrderModel
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
        /// 对应商品
        ///</summary>
        public long ProductId { get; set; }
        /// <summary>
        /// 对应规格
        ///</summary>
        public string SkuId { get; set; }
        /// <summary>
        /// 所属拼团
        ///</summary>
        public long GroupId { get; set; }
        /// <summary>
        /// 订单时间
        ///</summary>
        public long OrderId { get; set; }
        /// <summary>
        /// 订单用户编号
        ///</summary>
        public long OrderUserId { get; set; }
        /// <summary>
        /// 是否团首订单
        ///</summary>
        public bool IsFirstOrder { get; set; }
        /// <summary>
        /// 参团时间
        ///</summary>
        public DateTime JoinTime { get; set; }
        /// <summary>
        /// 参团状态 成团中  成功  失败
        ///</summary>
        public FightGroupOrderJoinStatus JoinStatus { get; set; }
        /// <summary>
        /// 结束时间 结束时间 成功或失败的时间
        ///</summary>
        public DateTime OverTime { get; set; }
        /// <summary>
        /// 购买数量
        ///</summary>
        public int Quantity { get; set; }
        /// <summary>
        /// 购买单价
        /// </summary>
        public decimal SalePrice { get; set; }
        /// <summary>
        /// 订单用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 订单用户头像
        /// </summary>
        public string Photo { get; set; }
    }
}
