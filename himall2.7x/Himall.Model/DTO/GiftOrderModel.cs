using Himall.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class GiftOrderModel
    {
        public GiftOrderModel()
        {
            PlatformType = PlatformType.PC;
        }
        public PlatformType PlatformType { set; get; }
        public UserMemberInfo CurrentUser { set; get; }

        public ShippingAddressInfo ReceiveAddress { get; set; }
        /// <summary>
        /// 用户备注
        /// </summary>
        public string UserRemark { get; set; }
        public IEnumerable<GiftOrderItemModel> Gifts { set; get; }
    }

    public class GiftOrderItemModel
    {
        public long GiftId { get; set; }
        public int Counts { get; set; }
    }
}
