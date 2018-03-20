using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 店铺信息表
    /// </summary>
    public class Shop:Himall.Model.ShopInfo
    {
        /// <summary>
        /// 店铺ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { get; set; }

        /// <summary>
        /// 店铺等级
        /// </summary>
        public long GradeId { get; set; }

        /// <summary>
        /// 店铺进度
        /// </summary>
        public Himall.Model.ShopInfo.ShopStage Stage { get; set; }

        /// <summary>
        /// 店铺状态
        /// </summary>
        public Himall.Model.ShopInfo.ShopAuditStatus ShopStatus { get; set; }

        /// <summary>
        /// 公司地址
        /// </summary>
        public string CompanyAddress { get; set; }

        /// <summary>
        /// 营业执照所在地
        /// </summary>
        public int BusinessLicenceRegionId { get; set; }

        /// <summary>
        /// 营业执照号
        /// </summary>
        public string BusinessLicenceNumber { get; set; }

        /// <summary>
        /// 营业执照图片
        /// </summary>
        public string BusinessLicenceNumberPhoto { get; set; }

        /// <summary>
        /// 营业执照有效期
        /// </summary>
        public DateTime BusinessLicenceEnd { get; set; }

        /// <summary>
        /// 营业执照开始日期
        /// </summary>
        public DateTime BusinessLicenceStart { get; set; }

        /// <summary>
        /// 营业执照证书
        /// </summary>
        public string BusinessLicenseCert { get; set; }

        /// <summary>
        /// 法定经营范围
        /// </summary>
        public string BusinessSphere { get; set; }

        /// <summary>
        /// 公司省市区
        /// </summary>
        public int CompanyRegionId { get; set; }

        /// <summary>
        /// 公司详细地址
        /// </summary>
        public string CompanyRegionAddress { get; set; }

        /// <summary>
        /// 公司成立日期
        /// </summary>
        public DateTime CompanyFoundingDate { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 联系人姓名
        /// </summary>
        public string ContactsName { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string ContactsPhone { get; set; }

        /// <summary>
        /// 联系Email
        /// </summary>
        public string ContactsEmail { get; set; }

        /// <summary>
        /// 公司员工数量
        /// </summary>
        public Himall.Model.CompanyEmployeeCount CompanyEmployeeCount { get; set; }

        /// <summary>
        /// 一般纳税人证明
        /// </summary>
        public string GeneralTaxpayerPhot { get; set; }

        /// <summary>
        /// 法人代表
        /// </summary>
        public string legalPerson { get; set; }

        /// <summary>
        /// 组织机构代码
        /// </summary>
        public string OrganizationCode { get; set; }

        /// <summary>
        /// 组织机构代码图片
        /// </summary>
        public string OrganizationCodePhoto { get; set; }

        /// <summary>
        /// 其他证书
        /// </summary>
        public string OtherCert { get; set; }

        /// <summary>
        /// 公司电话
        /// </summary>
        public string CompanyPhone { get; set; }

        /// <summary>
        /// 商品证书
        /// </summary>
        public string ProductCert { get; set; }

        /// <summary>
        /// 公司注册资金
        /// </summary>
        public decimal CompanyRegisteredCapital { get; set; }

        /// <summary>
        /// 税务登记证
        /// </summary>
        public string TaxRegistrationCertificate { get; set; }

        /// <summary>
        /// 审核拒绝原因
        /// </summary>
        public string RefuseReason { get; set; }

        /// <summary>
        /// 银行开户名
        /// </summary>
        public string BankAccountName { get; set; }

        /// <summary>
        /// 公司银行账号
        /// </summary>
        public string BankAccountNumber { get; set; }

        /// <summary>
        /// 支行联行号
        /// </summary>
        public string BankCode { get; set; }

        /// <summary>
        /// 银行名称
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// 开户行证明
        /// </summary>
        public string BankPhoto { get; set; }

        /// <summary>
        /// 开户银行所在地
        /// </summary>
        public int BankRegionId { get; set; }

        /// <summary>
        /// 税务登记证号
        /// </summary>
        public string TaxpayerId { get; set; }
        
        /// <summary>
        /// 纳税人识别号
        /// </summary>
        public string TaxRegistrationCertificatePhoto { get; set; }

        /// <summary>
        /// 支付证明
        /// </summary>
        public string PayPhoto { get; set; }

        /// <summary>
        /// 支付注释
        /// </summary>
        public string PayRemark { get; set; }


        /// <summary>
        /// 商家类别
        /// </summary>
        public Himall.CommonModel.ShopBusinessType? BusinessType { get; set; }

        /// <summary>
        /// 身份证号
        /// </summary>
        public string IDCard { get; set; }

        /// <summary>
        /// 身份证正面照
        /// </summary>
        public string IDCardUrl { get; set; }

        /// <summary>
        /// 身份证背面照
        /// </summary>
        public string IDCardUrl2 { get; set; }

        /// <summary>
        /// 微信昵称
        /// </summary>
        public string WeiXinNickName { get; set; }

        /// <summary>
        /// 微信性别；0、女；1、男
        /// </summary>
        public int WeiXinSex { get; set; }

        /// <summary>
        /// 微信地址
        /// </summary>
        public string WeiXinAddress { get; set; }

        /// <summary>
        /// 微信所在地址
        /// </summary>
        public string WeiXinTrueName { get; set; }

        /// <summary>
        /// 微信头像
        /// </summary>
        public string WeiXinImg { get; set; }

        /// <summary>
        /// 微信唯一标识符
        /// </summary>
        public string WeiXinOpenId { get; set; }

		public System.DateTime? EndDate { get; set; }
		public ShopAuditStatus ShowShopAuditStatus { get; set; }
        /// <summary>
        /// 经营项目
        /// </summary>
        [NotMapped]
        public Dictionary<long, decimal> BusinessCategory { get; set; }

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
