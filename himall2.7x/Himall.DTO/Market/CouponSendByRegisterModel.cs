using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * 2016-05-23
 * 注册有礼优惠券设置
 * **/
namespace Himall.DTO
{
    /// <summary>
    /// 注册有礼优惠券设置
    /// </summary>
    public class CouponSendByRegisterModel
    {
        public CouponSendByRegisterModel()
        {
            this.CouponIds = new List<CouponModel>();
        }

        private long _id = 0;
        /// <summary>
        /// 设置ID
        /// </summary>
        public long Id { get { return _id; } set { _id = value; } }

        private Himall.CommonModel.CouponSendByRegisterStatus _status = Himall.CommonModel.CouponSendByRegisterStatus.Shut;
        /// <summary>
        /// 开启状态
        /// </summary>
        public Himall.CommonModel.CouponSendByRegisterStatus Status { get { return _status; } set { _status = value; } }

        /// <summary>
        /// 优惠券
        /// </summary>
        public List<CouponModel> CouponIds { get; set; }

        /// <summary>
        /// 活动URL
        /// </summary>
        public string Link { get; set; }
        
        /// <summary>
        /// 优惠券剩余总数
        /// </summary>
        public int total { get; set; }

        /// <summary>
        /// 优惠券总金额
        /// </summary>
        public decimal price { get; set; }
    }
}
