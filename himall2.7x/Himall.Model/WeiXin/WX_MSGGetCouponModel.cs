using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    /// <summary>
    /// 微信模板消息-领取红包通知
    /// </summary>
    public class WX_MSGGetCouponModel
    {
        public WX_MSGGetCouponModel()
        {
            this.first = new WX_MSGItemBaseModel();
            this.toName = new WX_MSGItemBaseModel();
            this.gift = new WX_MSGItemBaseModel();
            this.time = new WX_MSGItemBaseModel();
            this.remark = new WX_MSGItemBaseModel();
        }
        public WX_MSGItemBaseModel first { get; set; }
        public WX_MSGItemBaseModel toName { get; set; }
        public WX_MSGItemBaseModel gift { get; set; }
        public WX_MSGItemBaseModel time { get; set; }
        public WX_MSGItemBaseModel remark { get; set; }
    }
}
