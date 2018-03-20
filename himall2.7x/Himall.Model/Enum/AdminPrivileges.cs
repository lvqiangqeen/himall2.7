using Himall.CommonModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public enum AdminPrivilege
    {
        //所有权限
        //  [Privilege("", "","1,2",""]        /* 首页*
        // ConSole = 1001,
        //[Description("在线更新")]
        //OnlineUpdate = 1002,
        //[Description("版权声明")]
        //CopyRight = 1002,
        //[Description("关于我们")]
        //AboutUs = 1003,

        /*商品*/
        [Privilege("商品", "商品管理", 2001, "product/management", "product")]
        ProductManage = 2001,
        [Privilege("商品", "分类管理", 2002, "category/management", "category")]
        CategoryManage = 2002,
        [Privilege("商品", "品牌管理", 2003, "brand/Management", "brand")]

        BrandManage = 2003,
        [Privilege("商品", "类型管理", 2004, "ProductType/management", "ProductType")]
        ProductTypeManage = 2004,
        [Privilege("商品", "咨询管理", 2006, "ProductConsultation/management", "productconsultation")]
        ConsultationManage = 2006,
        [Privilege("商品", "评论管理", 2007, "ProductComment/management", "ProductComment")]
        CommentManage = 2007,
        ///*交易*/

        [Privilege("交易", "订单管理", 3001, "Order/management", "order")]
        OrderManage = 3001,
        [Privilege("交易", "退款处理", 3002, "OrderRefund/management?showtype=2", "orderrefund")]
        ReturnRefundManage = 3002,
        [Privilege("交易", "退货处理", 3009, "OrderRefund/management?showtype=3", "ReturnGoodsManage")]
        ReturnGoodsManage = 3009,
        [Privilege("交易", "交易评价", 3003, "OrderComment/management", "ordercomment")]
        OrderComment = 3003,
        [Privilege("交易", "交易投诉", 3004, "OrderComplaint/management", "ordercomplaint")]
        OrderComplaint = 3004,
        [Privilege("交易", "支付方式", 3005, "Payment/management", "payment")]
        PaymentManage = 3005,
        [Privilege("交易", "快递单模板", 3006, "ExpressTemplate/management", "ExpressTemplate")]
        ExpressTemplate = 3006,
        [Privilege("交易", "交易设置", 3007, "AdvancePayment/edit", "AdvancePayment")]
        PaymentManageSet = 3007,
        [Privilege("交易", "发票管理", 3008, "Order/InvoiceContext", "InvoiceContext")]
        InvoiceContextManage = 3008,
        ///*会员*/
        [Privilege("会员", "会员管理", 4001, "member/management", "member")]
        MemberManage = 4001,

        [Privilege("会员", "会员分组", 4002, "member/MemberGroup", "member")]
        MemberGroupManage = 4002,

        [Privilege("会员", "标签管理", 4009, "Label/management", "Label")]
        LabelManage = 4009,
        [Privilege("会员", "会员营销", 4010, "MessageGroup/WXGroupMessage", "MessageGroup")]
        MarketingManage = 4010,
        [Privilege("会员", "会员积分", 4003, "MemberIntegral/search", "MemberIntegral")]
        MemberIntegral = 4003,
        [Privilege("会员", "积分规则", 4004, "IntegralRule/management", "IntegralRule")]
        IntegralRule = 4004,
        [Privilege("会员", "会员等级", 4005, "MemberGrade/management", "MemberGrade")]
        MemberGrade = 4005,
        [Privilege("会员", "信任登录", 4006, "OAuth/Management", "OAuth")]
        OAuth = 4006,
        [Privilege("会员", "会员推广", 4007, "MemberInvite/Setting", "MemberInvite")]
        MemberInvite = 4007,
        [Privilege("会员", "预付款管理", 4008, "Capital/Index", "Capital")]
        Capital = 4008,

        ///*店铺*/
        [Privilege("店铺", "店铺管理", 5001, "shop/management?type=Auditing", "Shop")]
        ShopManage = 5001,
        [Privilege("店铺", "店铺套餐", 5002, "ShopGrade/management", "ShopGrade")]
        ShopPackage = 5002,
        [Privilege("店铺", "财务管理", 5003, "Billing/index", "Billing")]
        SettlementManage = 5003,
        [Privilege("店铺", "保证金管理", 5004, "CashDeposit/Management", "CashDeposit")]
        CashDepositManagement = 5004,
        [Privilege("店铺", "商家提现管理", 5005, "ShopWithDraw/Management", "ShopWithDraw")]
        ShopWithDrawManagement = 5004,

        ///*统计*/
        ////[Description("流量统计")]
        ////TrafficStatistics = 6001,
        [Privilege("统计", "会员统计", 6002, "Statistics/Member", "statistics", "member")]
        MemberStatistics = 6002,
        [Privilege("统计", "店铺统计", 6003, "Statistics/NewShop", "statistics", "newshop")]
        ShopStatistics = 6003,
        [Privilege("统计", "商品统计", 6004, "Statistics/ProductSaleStatistic", "statistics")]
        ProductSaleStatistic = 6004,
        [Privilege("统计", "交易统计", 6005, "Statistics/TradeStatistic", "statistics")]
        TradeStatistic = 6005,

        ///*网站*/
        [Privilege("网站", "页面设置", 7001, "PageSettings", "PageSettings")]
        [Privilege("网站", "页面设置", 7001, "PageSettings", "SlideAd")]
        PageSetting = 7001,
        [Privilege("网站", "文章管理", 7002, "Article/management", "article")]
        ArticleManage = 7002,
        [Privilege("网站", "文章分类", 7003, "ArticleCategory/management", "articlecategory")]
        ArticleCategoryManage = 7003,
        [Privilege("网站", "主题设置", 7004, "Theme/", "theme")]
        ThemeSet = 7004,


        [Privilege("分销", "分销市场管理", 7101, "DistributionMarket/management", "DistributionMarket")]
        DistributionMarket = 7101,
        [Privilege("分销", "销售员管理", 7102, "Promoter/management", "Promoter")]
        Promoter = 7102,
        [Privilege("分销", "分销业绩管理", 7103, "Proformance/management", "Proformance")]
        Proformance = 7103,
        [Privilege("分销", "分销首页设置", 7104, "DistributionShop/HomePageSetting", "DistributionShop")]
        DistributionShop = 7104,




        ///*系统*/
        [Privilege("系统", "站点设置", 8001, "SiteSetting/Edit", "SiteSetting")]
        [Privilege("系统", "站点设置", 8001, "SiteSetting/Edit", "Navigation")]
        SiteSetting = 8001,
        [Privilege("系统", "管理员", 8002, "Manager/management", "Manager")]
        AdminManage = 8002,
        [Privilege("系统", "权限组", 8003, "Privilege/management", "privilege")]
        PrivilegesManage = 8003,
        [Privilege("系统", "操作日志", 8004, "OperationLog/management", "OperationLog")]
        OperationLog = 8004,
        [Privilege("系统", "消息设置", 8005, "Message/management", "Message")]
        MessageSetting = 8005,
        [Privilege("系统", "入驻设置", 8006, "Agreement/Settled", "Agreement")]
        Agreement = 8006,

        [Privilege("系统", "区域管理", 8007, "RegionArea/management", "RegionArea")]
        RegionArea = 8007,
        //[Privilege("系统", "敏感关键词", 8007, "SensitiveWord/Management", "SensitiveWord")]
        //SensitiveWord = 8007,

        ///*营销*/
        [Privilege("营销", "营销中心", 9020, "Marketing/Management", "Marketing")]
        Marketing = 9020,

        [Privilege("营销", "限时购", 9001, "LimitTimeBuy/management", "LimitTimeBuy", "", AdminCatalogType.Internal)]
        LimitTimeBuy = 9001,
        [Privilege("营销", "优惠券", 9002, "Coupon/management", "Coupon", "", AdminCatalogType.Internal)]
        Coupon = 9002,
        [Privilege("营销", "注册送优惠券", 9013, "CouponActivity/Index", "CouponActivity", "", AdminCatalogType.Internal)]
        CouponActivity = 9013,
        [Privilege("营销", "组合购", 9003, "Collocation/management", "Collocation", "", AdminCatalogType.Internal)]
        Collocation = 9003,
        [Privilege("营销", "吸粉红包", 9004, "Bonus/management", "Bonus", "", AdminCatalogType.Internal)]
        Bonus = 9004,
        [Privilege("营销", "代金红包", 9005, "ShopBonus/management", "ShopBonus", "", AdminCatalogType.Internal)]
        ShopBonus = 9005,
        [Privilege("营销", "礼品管理", 9006, "Gift/management", "gift", "", AdminCatalogType.Internal)]
        GiftManage = 9006,
        [Privilege("营销", "礼品兑换列表", 9007, "Gift/Order", "giftorder", "", AdminCatalogType.Internal)]
        GiftOrder = 9007,
        [Privilege("营销", "签到", 9008, "SignIn/Setting", "SignIn", "", AdminCatalogType.Internal)]
        SignIn = 9008,
        [Privilege("营销", "拼团", 9011, "FightGroup/Index", "FightGroup", "", AdminCatalogType.Internal)]
        FullDiscount = 9011,
        [Privilege("营销", "满额减", 9015, "FullDiscount/BoughtList", "FullDiscount", "", AdminCatalogType.Internal)]
        FightGroup = 9015,
        [Privilege("营销", "刮刮卡管理", 9012, "WeiActivity/Management", "WeiActivity", "", AdminCatalogType.Internal)]
        WeiActivity = 9012,
        [Privilege("营销", "大转盘管理", 9014, "WeiBigWheel/Management", "WeiBigWheel", "", AdminCatalogType.Internal)]
        WeiBigWheel = 9014,
        /*专题*/
        [Privilege("营销", "移动端专题", 9009, "MobileTopic/management", "MobileTopic", "", AdminCatalogType.Internal)]
        MobileTopic = 9009,
        [Privilege("营销", "PC端专题", 9010, "Topic/management", "Topic", "", AdminCatalogType.Internal)]
        PCTopic = 9010,
        [Privilege("营销", "APP积分商城", 9016, "Gift/AppManage", "AppManage", "", AdminCatalogType.Internal)]
        AppIntegralManage = 9016,

        ///*微商城*/
        [Privilege("微商城", "商城首页设置", 10001, "VTemplate/VHomepage", "VTemplate")]
        [Privilege("微商城", "商城首页设置", 10001, "VTemplate/VHomepage", "MobileHomeProducts")]
        [Privilege("微商城", "商城首页设置", 10001, "VTemplate/VHomepage", "TemplateVisualizationAjax")]
        Vshop = 10001,
        [Privilege("微商城", "微店管理", 10002, "Vshop/VShopManagement", "Vshop")]
        VshopManage = 10002,
        [Privilege("微商城", "菜单设置", 10003, "Weixin/MenuManage", "WeiXin")]
        VshopMenu = 10003,
        [Privilege("微商城", "公众号设置", 10004, "Weixin/BasicSettings", "WeiXin")]
        VshopBasicSetting = 10004,
        [Privilege("微商城", "素材管理", 10005, "Weixin/WXMsgTemplateManage", "WeiXin")]
        WXMsgTemplateManage = 10005,

        [Privilege("APP", "APP首页配置", 12001, "APPShop/HomePageSetting", "APPShop")]
        APPShop = 12001,
        [Privilege("APP", "关于我们", 12002, "APPShop/Management", "APPShop")]
        APPAboutUs = 12002,
        [Privilege("APP", "APP引导页", 12004, "APPShop/APPGuidePages", "APPShop")]
        APPGuidePages = 12004,

        [Privilege("小程序", "商品配置", 13002, "WXSmallProgram/ProductSetting", "WXSmallProgram")]
        ProductSetting = 13002,
        [Privilege("小程序", "首页配置", 13001, "WXSmallProgram/HomePageSetting", "WXSmallProgram", LinkTarget = CommonModel.TargetType.Blank)]
        WXSmallProgramHomePageSetting = 13001,
        
        [Privilege("小程序", "消息配置", 13003, "WXSmallProgram/EditWXMessage", "WXSmallProgram")]
        EditWXMessage = 13003
    }
}
