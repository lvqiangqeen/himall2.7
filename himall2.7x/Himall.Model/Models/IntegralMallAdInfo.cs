using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Himall.Model
{
    public partial class IntegralMallAdInfo:BaseModel
    {
        /// <summary>
        /// 活动类型
        /// </summary>
        public enum AdActivityType
        {
            /// <summary>
            /// 刮刮卡
            /// </summary>
            [Description("刮刮卡")]
            ScratchCard = 1,

            /// <summary>
            /// 大转盘
            /// </summary>
            [Description("大转盘")]
            Roulette = 2

        }
        /// <summary>
        /// 广告显示状态
        /// </summary>
        public enum AdShowStatus
        {
            /// <summary>
            /// 显示
            /// </summary>
            [Description("显示")]
            Show = 1,
            /// <summary>
            /// 隐藏
            /// </summary>
            [Description("隐藏")]
            Hide = 2,
        }
        /// <summary>
        /// 广告显示平台
        /// </summary>
        public enum AdShowPlatform
        {
            /// <summary>
            /// 电脑
            /// </summary>
            PC = 1,
            /// <summary>
            /// APP
            /// </summary>
            APP = 2,
        }
        /// <summary>
        /// 活动类型
        /// </summary>
        [NotMapped]
        public AdActivityType ShowActivityType { get
            {
                AdActivityType result = (AdActivityType)this.ActivityType;
                return result;
            }
            set
            {
                this.ActivityType = value.GetHashCode();
            }
        }
        /// <summary>
        /// 广告显示状态
        /// </summary>
        [NotMapped]
        public AdShowStatus? ShowAdStatus
        {
            get
            {
                AdShowStatus? result = (AdShowStatus?)this.ShowStatus;
                return result;
            }
            set
            {
                if (value.HasValue)
                {
                    this.ShowStatus = value.GetHashCode();
                }
                else
                {
                    this.ShowStatus = null;
                }
            }
        }
        /// <summary>
        /// 广告显示平台
        /// </summary>
        [NotMapped]
        public AdShowPlatform? ShowAdPlatform
        {
            get
            {
                AdShowPlatform? result = (AdShowPlatform?)this.ShowPlatform;
                return result;
            }
            set
            {
                if (value.HasValue)
                {
                    this.ShowPlatform = value.GetHashCode();
                }
                else
                {
                    this.ShowPlatform = null;
                }
            }
        }

        /// <summary>
        /// 链接地址
        /// </summary>
        [NotMapped]
        public string LinkUrl { get; set; }
    }
}
