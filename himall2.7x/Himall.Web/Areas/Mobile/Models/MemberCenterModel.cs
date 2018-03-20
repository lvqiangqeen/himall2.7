using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Mobile.Models
{
    public class MemberCenterModel
    {
        public int WaitingForComments { get; set; }

        public Model.UserMemberInfo Member { get; set; }

        public int AllOrders { get; set; }

        public int WaitingForRecieve { get; set; }

        public int WaitingForPay { get; set; }
        /// <summary>
        /// 待发货
        /// </summary>
        public int WaitingForDelivery { get; set; }

        public int RefundOrders { get; set; }

        public decimal Capital { get; set; }

        public string GradeName { get; set; }

        public int CouponsCount { get; set; }

        public int CollectionShop { get; set; }
        /// <summary>
        /// 是否已开启签到功能
        /// </summary>
        public bool SignInIsEnable { get; set; }
        /// <summary>
        /// 是否可以签到
        /// </summary>
        public bool CanSignIn { get; set; }

        //是否开启分销
        public bool CanDistribution { set; get; }
        /// <summary>
        /// 是否开启拼团
        /// </summary>
        public bool CanFightGroup { get; set; }
        /// <summary>
        /// 组团数
        /// </summary>
        public int BulidFightGroupNumber { get; set; }
        /// <summary>
        /// 用户可用积分
        /// </summary>
        public int MemberAvailableIntegrals { get; set; }
        public Himall.Model.UserMemberInfo userMemberInfo { get; set; }
    }
}