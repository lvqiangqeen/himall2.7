using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Himall.Model
{
    public partial class ShopInfo
    {
        /// <summary>
        /// 显示店铺状态
        /// </summary>
        [NotMapped]
        public ShopAuditStatus ShowShopAuditStatus
        {
            get
            {
                ShopAuditStatus result = ShopAuditStatus.Unusable;
                if (this != null)
                {
                    result = this.ShopStatus;
                    if (this.EndDate != null && this.ShopStatus == ShopInfo.ShopAuditStatus.Open)
                    {
                        DateTime endd = this.EndDate.Value.Date.AddDays(1).AddSeconds(-1);
                        if ((endd - DateTime.Now).TotalSeconds < 0)
                        {
                            result = ShopInfo.ShopAuditStatus.HasExpired;
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 店铺状态
        /// </summary>
        public enum ShopAuditStatus
        {
            /// <summary>
            /// 不可用
            /// </summary>
            [Description("不可用")]
            Unusable = 1,

            /// <summary>
            /// 待审核
            /// </summary>
            [Description("待审核")]
            WaitAudit = 2,

            /// <summary>
            /// 待付款
            /// </summary>
            [Description("待付款")]
            WaitPay = 3,

            /// <summary>
            /// 被拒绝
            /// </summary>
            [Description("被拒绝")]
            Refuse = 4,

            /// <summary>
            /// 待确认
            /// </summary>
            [Description("待确认")]
            WaitConfirm = 5,

            /// <summary>
            /// 冻结
            /// </summary>
            [Description("冻结")]
            Freeze = 6,

            /// <summary>
            /// 开启
            /// </summary>
            [Description("开启")]
            Open = 7,
            /// <summary>
            /// 已过期
            /// </summary>
            [Description("已过期")]
            HasExpired = -1
        }

        [NotMapped]
        public string ShopAccount { get; set; }

        [NotMapped]
        public Dictionary<long, decimal> BusinessCategory { get; set; }

        [NotMapped]
        public string CompanyRegionAddress { get; set; }

        [NotMapped]
        public int Sales
        {
            get;
            set;
        }

        [NotMapped]
        public string ProductAndDescription
        {
            get;
            set;
        }

        [NotMapped]
        public string SellerServiceAttitude
        {
            get;
            set;
        }

        [NotMapped]
        public string SellerDeliverySpeed
        {
            get;
            set;
        }

        /// <summary>
        /// 店铺进度
        /// </summary>
        public enum ShopStage
        {
            /// <summary>
            /// 许可协议
            /// </summary>
            [Description("许可协议")]
            Agreement,

            /// <summary>
            /// 公司信息
            /// </summary>
            [Description("公司信息")]
            CompanyInfo,

            /// <summary>
            /// 财务信息
            /// </summary>
            [Description("财务信息")]
            FinancialInfo,

            /// <summary>
            /// 店铺信息
            /// </summary>
            [Description("店铺信息")]
            ShopInfo,

            /// <summary>
            /// 上传支付凭证
            /// </summary>
            [Description("上传支付凭证")]
            UploadPayOrder,

            /// <summary>
            /// 完成
            /// </summary>
            [Description("完成")]
            Finish
        }

        public class ShopVistis
        {
            public decimal VistiCounts { get; set; }

            public decimal SaleCounts { get; set; }

            public decimal SaleAmounts { get; set; }

            public decimal OrderCounts { get; set; }
        }

        /// <summary>
        /// 店铺欢迎语
        /// </summary>
        public string WelcomeTitle { get; set; }

        /// <summary>
        /// 坐标
        /// </summary>
        public decimal Lng { get; set; }

        /// <summary>
        /// 坐标
        /// </summary>
        public decimal Lat { get; set; }

        /// <summary>
        /// 行业
        /// </summary>
        public string Industry { get; set; }

        /// <summary>
        /// 营业环境图片，imgpath1;imgpath2
        /// </summary>
        public string BranchImage { get; set; }

        /// <summary>
        /// 营业时间
        /// </summary>
        public string OpeningTime { get; set; }

        /// <summary>
        /// 店铺描述
        /// </summary>
        public string ShopDescription { get; set; }

        /// <summary>
        /// 申请人职位
        /// </summary>
        public string ContactsPosition { get; set; }
    }
}
