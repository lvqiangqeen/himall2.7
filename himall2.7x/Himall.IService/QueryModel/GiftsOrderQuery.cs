using Himall.Model;
using System;
using System.Collections.Generic;

namespace Himall.IServices.QueryModel
{
    public partial class GiftsOrderQuery : QueryBase
    {
        /// <summary>
        /// 关键词
        /// </summary>
        public string skey { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public long? OrderId { get; set; }

        public GiftOrderInfo.GiftOrderStatus? status { get; set; }

        public long? UserId { get; set; }
    }
}
