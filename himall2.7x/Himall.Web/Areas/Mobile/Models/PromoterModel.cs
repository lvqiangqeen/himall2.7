using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;

namespace Himall.Web.Areas.Mobile
{
    public class PromoterModel
    {
        public Himall.Model.UserMemberInfo Member { set; get; }

        public Himall.Model.RecruitSettingInfo RecruitSetting { set; get; }

        public string RegionPath { set; get; }

        public bool IsBindMobile { set; get; }

        public bool IsRefused { set; get; }

        public string ShopName { set; get; }
        public PromoterInfo.PromoterStatus Status { get; set; }
        /// <summary>
        ///  是否提交过资料
        /// </summary>
        public bool IsHavePostData { get; set; }
    }
}