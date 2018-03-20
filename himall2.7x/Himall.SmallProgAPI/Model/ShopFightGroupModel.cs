using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
{
    public class ShopFightGroupModel
    {
        /// <summary>
        /// 拼团编号
        /// </summary>
        public long? Id
        {
            get;set;
        }
        /// <summary>
        /// 团长编号
        /// </summary>
        public long? HeadUserId
        {
            get;set;
        }
        /// <summary>
        /// 活动编号
        /// </summary>
        public long? ActiveId
        {
            get;set;
        }
        /// <summary>
        /// 当前拼团人数
        /// </summary>
        public int? JoinedNumber
        {
            get;set;
        }
        /// <summary>
        /// 限定拼团人数
        /// </summary>
        public int? LimitedNumber
        {
            get;set;
        }
        /// <summary>
        /// 拼团状态
        /// </summary>
        public int? GroupStatus
        {
            get;set;
        }
        /// <summary>
        /// 头像
        /// </summary>
        public string Photo
        {
            get;set;
        }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Nick
        {
            get;set;
        }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName
        {
            get;set;
        }
        /// <summary>
        /// 商品图片
        /// </summary>
        public string IconUrl
        {
            get;set;
        }
        /// <summary>
        /// 每单奖励
        /// </summary>
        public decimal? ReturnMoney
        {
            get;set;
        }
        /// <summary>
        /// 开团奖励
        /// </summary>
        public decimal? OpenGroupReward
        {
            get;set;
        }
        /// <summary>
        /// 邀请奖励
        /// </summary>
        public decimal? InvitationReward
        {
            get;set;
        }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime
        {
            get;set;
        }
        /// <summary>
        /// 拼团活动售价
        /// </summary>
        public decimal? ActivePrice
        {
            get;set;
        }
        /// <summary>
        /// 实际结算总价
        /// </summary>
        public decimal? TotalPrice
        {
            get;set;
        }
        /// <summary>
        /// 实际结算单价
        /// </summary>
        public decimal? SalePrice
        {
            get;set;
        }
    }
}
