using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.DTO;
using Himall.CommonModel;
using Himall.Web.Models;
using Himall.Model;

namespace Himall.Web.Areas.Mobile.Models
{
    public class OrderSubmitGroupModel
    {
        public long GroupActionId { get; set; }
        public long? GroupId { get; set; }
        public MobileOrderDetailConfirmModel ConfirmModel { get; set; }
        /// <summary>
        /// 是否为微信端
        /// </summary>
        public bool IsWeiXin { get; set; }
    }
}