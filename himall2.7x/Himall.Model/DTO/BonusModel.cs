using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class ReceiveModel
    {
        public ReceiveStatus State { get; set; }

        public decimal Price { get; set; }
    }

    public enum ReceiveStatus
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
        /// 已经被其他用户取完
        /// </summary>
        HaveNot = 3 ,

        /// <summary>
        /// 未关注
        /// </summary>
        NotAttention = 4 ,

        /// <summary>
        /// 失效
        /// </summary>
        Invalid = 5 ,

        /// <summary>
        /// 可以领取但没有关注
        /// </summary>
        CanReceiveNotAttention = 6
    }
}
