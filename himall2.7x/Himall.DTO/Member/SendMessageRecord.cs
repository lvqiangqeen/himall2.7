using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.CommonModel;

namespace Himall.DTO
{
    public class SendMessageRecord
    {
        public long Id { get; set; }
        public MsgType MessageType { get; set; }
        public DateTime? SendTime { get; set; }
        public string ToUserLabel { get; set; }
        public int? SendState { get; set; }
        public int CurrentCouponCount { get; set; }
        public int CurrentUseCouponCount { get; set; }
    }
}
