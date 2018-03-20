using System.ComponentModel;
using System.Configuration;

namespace Himall.Model
{
    public partial class VShopInfo
    {

        /// <summary>
        /// 微店状态
        /// </summary>
        public enum VshopStates
        {

            /// <summary>
            /// 未审核
            /// </summary>
            [Description("未审核")]
            NotAudit = 1,

            /// <summary>
            /// 审核通过
            /// </summary>
            [Description("审核通过")]
            Normal = 2,

            /// <summary>
            /// 审核拒绝
            /// </summary>
            [Description("审核拒绝")]
            Refused = 3,

            /// <summary>
            /// 开启微店第一步
            /// </summary>
            [Description("开启微店第一步")]
            Step1 = 4,

            /// <summary>
            /// 开启微店第二步
            /// </summary>
            [Description("开启微店第二步")]
            Step2 = 5,

            /// <summary>
            /// 开启微店第三步
            /// </summary>
            [Description("开启微店第三步")]
            Step3 = 6,

            /// <summary>
            /// 已关闭
            /// </summary>
            [Description("已关闭")]
            Close = 99,

        }

        /// <summary>
        /// Logo路径
        /// </summary>
        public string Logo
        {
            get { return ImageServerUrl + logo; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    logo = value.Replace(ImageServerUrl, "");
                else
                    logo = value;
            }
        }

        /// <summary>
        /// 背景图片路径
        /// </summary>
        public string BackgroundImage
        {
            get { return ImageServerUrl + backgroundImage; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    backgroundImage = value.Replace(ImageServerUrl, "");
                else
                    backgroundImage = value;
            }
        }
        /// <summary>
        /// 显示排序，需数据补充
        /// </summary>
        public int? ShowSequence { get; set; }

    }
}
