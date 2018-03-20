using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class ShopReceiveModel
    {
        public ShopReceiveStatus State { get; set; }

        public decimal Price { get; set; }

        public string UserName { get; set; }

        public long Id { get; set; }
    }

    public enum ShopReceiveStatus
    {
        /// <summary>
        /// 已领取 
        /// </summary>
        Receive = 1 ,

        /// <summary>
        /// 可以领取
        /// </summary>
        CanReceive = 2 ,

        /// <summary>
        /// 可以领取，但没有绑定UserId
        /// </summary>
        CanReceiveNotUser = 3 ,

        /// <summary>
        /// 已经被其他用户取完
        /// </summary>
        HaveNot = 4 ,

        /// <summary>
        /// 失效
        /// </summary>
        Invalid = 5
    }
}
