using System.ComponentModel.DataAnnotations;

namespace Himall.Web.Models
{
    public class SiteSettingModel
    {

        /// <summary>
        /// 站点名称
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// ICP编号
        /// </summary>
        public string ICPNubmer { get; set; }

        /// <summary>
        /// 客服联系电话
        /// </summary>
        public string CustomerTel { get; set; }

        /// <summary>
        /// 站点
        /// </summary>
        public bool SiteIsOpen { get; set; }

        /// <summary>
        /// 注册方式
        /// </summary>
        public Himall.Model.SiteSettingsInfo.RegisterTypes RegisterType { get; set; }
        /// <summary>
        /// 手机是否需验证
        /// </summary>
        public bool MobileVerifOpen { set; get; }
        /// <summary>
        /// 邮箱是否必填
        /// </summary>
        public bool RegisterEmailRequired { get; set; }
        /// <summary>
        /// 邮箱是否需要验证
        /// </summary>
        public bool EmailVerifOpen { set; get; }


        /// <summary>
        /// 微信AppId
        /// </summary>
        public string WeixinAppId { get; set; }

        /// <summary>
        /// 微信AppSecret
        /// </summary>
        public string WeixinAppSecret { get; set; }

        /// <summary>
        /// 微信token
        /// </summary>
        public string WeixinToKen { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string WeixinPartnerID { get; set; }

        public string WeixinPartnerKey { get; set; }

        public string WeixinLoginUrl { get; set; }

        public bool IsValidationService { get; set; }

        /// <summary>
        /// 商家入住协议
        /// </summary>
        public string SellerAdminAgreement { get; set; }

        /// <summary>
        /// 预付款百分比
        /// </summary>
        public decimal AdvancePaymentPercent { get; set; }

        /// <summary>
        /// 预付款上限
        /// </summary>
        public decimal AdvancePaymentLimit { get; set; }


        public string Logo
        {
            get;
            set;
        }

        public string MemberLogo
        {
            get;
            set;
        }
        /// <summary>
        /// 微信Logo
        /// <para>用于微信卡券，100*100，小于1M</para>
        /// </summary>
        public string WXLogo
        {
            get;
            set;
        }
        /// <summary>
        /// PC登录页左侧图片
        /// </summary>
        public string PCLoginPic
        {
            get;
            set;
        }

        public string QRCode
        {
            get;
            set;
        }

        public string FlowScript
        {
            get;
            set;
        }

        public string Site_SEOTitle
        {
            get;
            set;
        }

        public string Site_SEOKeywords
        {
            get;
            set;
        }

        public string Site_SEODescription
        {
            get;
            set;
        }

        /// <summary>
        /// 未付款超时(小时)
        /// </summary>        
        public int UnpaidTimeout { get; set; }

        /// <summary>
        /// 确认收货超时(天数)
        /// </summary>        
        public int NoReceivingTimeout { get; set; }
        /// <summary>
        /// 关闭评价通道时限(天数)
        /// </summary>
        public int OrderCommentTimeout { get; set; }

        /// <summary>
        /// 确认收货后可退货周期(天数)
        /// </summary>
        public int SalesReturnTimeout { get; set; }
        /// <summary>
        /// 售后-商家自动确认售后时限(天数)
        /// <para>到期未审核，自动认为同意售后</para>
        /// </summary>
        public int AS_ShopConfirmTimeout { get; set; }
        /// <summary>
        /// 售后-用户发货限时(天数)
        /// <para>到期未发货自动关闭售后</para>
        /// </summary>
        public int AS_SendGoodsCloseTimeout { get; set; }
        /// <summary>
        /// 售后-商家确认到货时限(天数)
        /// <para>到期未收货，自动收货</para>
        /// </summary>
        public int AS_ShopNoReceivingTimeout { get; set; }


        /// <summary>
        /// 收到红包微信提醒模板编号
        /// </summary>
        public string WX_MSGGetCouponTemplateId { get; set; }
        /// <summary>
        /// app版本号
        /// </summary>
        public string AppVersion { get; set; }
        /// <summary>
        /// 安卓下载地址
        /// </summary>
        public string AndriodDownLoad { set; get; }
        /// <summary>
        /// APP更新说明
        /// </summary>
        public string AppUpdateDescription { set; get; }
        /// <summary>
        /// IOS下载地址
        /// </summary>
        public string IOSDownLoad { set; get; }

        /// <summary>
        /// 是否提供下载
        /// </summary>
        public bool CanDownload { set; get; }

        /// <summary>
        /// 快递100Key
        /// </summary>
        public string Kuaidi100Key { set; get; }

        /// <summary>
        /// 客服电话
        /// </summary>
        public string SitePhone { get; set; }

    }
}