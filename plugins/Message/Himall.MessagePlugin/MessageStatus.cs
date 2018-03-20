using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.MessagePlugin
{
    public class MessageStatus
    {
        public int OrderCreated { set; get; }

        public int OrderPay { set; get; }

        public int OrderShipping { set; get; }

        public int OrderRefund { set; get; }

        public int FindPassWord { set; get; }

        public int ShopAudited { set; get; }

        public int ShopSuccess { set; get; }
        public int ShopHaveNewOrder { set; get; }
        public int ReceiveBonus { set; get; }
        public int LimitTimeBuy { set; get; }
        public int SubscribeLimitTimeBuy { set; get; }
        public int RefundDeliver { set; get; }
        public int FightGroupOpenSuccess { set; get; }
        public int FightGroupJoinSuccess { set; get; }
        public int FightGroupNewJoin { set; get; }
        public int FightGroupFailed { set; get; }
        public int FightGroupSuccess { set; get; }

        public int SendCouponSuccess { set; get; }
    }
}
