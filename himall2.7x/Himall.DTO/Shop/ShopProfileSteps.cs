using Himall.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Himall.DTO
{
    #region ShopProfileStep1

    /// <summary>
    /// 商家入驻第一步，验证字段必填
    /// </summary>
    public class ShopProfileStep1 : ShopProfileStepT1
    {
        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get { return CompanyNameT; } set { CompanyNameT = value; } }

        /// <summary>
        /// 公司所在地
        /// </summary>
        public int CityRegionId { get { return CityRegionIdT; } set { CityRegionIdT = value; } }

        /// <summary>
        /// 公司详细地址
        /// </summary>
        public string Address { get { return AddressT; } set { AddressT = value; } }

        /// <summary>
        /// 公司电话
        /// </summary>
        public string Phone { get { return PhoneT; } set { PhoneT = value; } }

        /// <summary>
        /// 公司人数
        /// </summary>
        public CompanyEmployeeCount EmployeeCount { get { return EmployeeCountT; } set { EmployeeCountT = value; } }

        /// <summary>
        /// 注册资金
        /// </summary>
        public decimal RegisterMoney { get { return RegisterMoneyT; } set { RegisterMoneyT = value; } }

        /// <summary>
        /// 联系人姓名
        /// </summary>
        public string ContactName { get { return ContactNameT; } set { ContactNameT = value; } }

        /// <summary>
        /// 联系人电话
        /// </summary>
        public string ContactPhone { get { return ContactPhoneT; } set { ContactPhoneT = value; } }

        /// <summary>
        /// 电子邮箱
        /// </summary>
        public string Email { get { return EmailT; } set { EmailT = value; } }

        /// <summary>
        /// 营业执照号
        /// </summary>
        [Remote("CheckBusinessLicenceNumbers", "Shop", "SellerAdmin", ErrorMessage = "该营业执照号已存在")]
        public string BusinessLicenceNumber { get { return BusinessLicenceNumberT; } set { BusinessLicenceNumberT = value; } }

        /// <summary>
        /// 营业执照所在地
        /// </summary>
        public int BusinessLicenceArea { get { return BusinessLicenceAreaT; } set { BusinessLicenceAreaT = value; } }

        /// <summary>
        /// 营业执照起始有效期
        /// </summary>
        public DateTime BusinessLicenceValidStart { get { return BusinessLicenceValidStartT; } set { BusinessLicenceValidStartT = value; } }

        /// <summary>
        /// 营业执照截止有效期
        /// </summary>
        public DateTime BusinessLicenceValidEnd { get { return BusinessLicenceValidEndT; } set { BusinessLicenceValidEndT = value; } }

        /// <summary>
        /// 法定经营范围
        /// </summary>
        public string BusinessSphere { get { return BusinessSphereT; } set { BusinessSphereT = value; } }

        /// <summary>
        /// 营业执照号电子版
        /// </summary>
        public string BusinessLicenceNumberPhoto { get { return BusinessLicenceNumberPhotoT; } set { BusinessLicenceNumberPhotoT = value; } }

        /// <summary>
        /// 组织机构代码
        /// </summary>
        public string OrganizationCode { get { return OrganizationCodeT; } set { OrganizationCodeT = value; } }

        /// <summary>
        /// 组织机构代码证电子版
        /// </summary>
        public string OrganizationCodePhoto { get { return OrganizationCodePhotoT; } set { OrganizationCodePhotoT = value; } }

        /// <summary>
        /// 一般纳税人证明
        /// </summary>
        public string GeneralTaxpayerPhoto { get { return GeneralTaxpayerPhotoT; } set { GeneralTaxpayerPhotoT = value; } }

        /// <summary>
        /// 公司法定代表人
        /// </summary>
        public string legalPerson { get { return legalPersonT; } set { legalPersonT = value; } }

        /// <summary>
        /// 公司成立日期
        /// </summary>
        public DateTime CompanyFoundingDate { get { return CompanyFoundingDateT; } set { CompanyFoundingDateT = value; } }

        /// <summary>
        /// 经营许可类证书
        /// </summary>
        public string BusinessLicenseCert { get { return BusinessLicenseCertT; } set { BusinessLicenseCertT = value; } }

        /// <summary>
        /// 产品类证书
        /// </summary>
        public string ProductCert { set { ProductCertT = value; } get { return ProductCertT; } }

        /// <summary>
        /// 其它证书
        /// </summary>
        public string OtherCert { set { OtherCertT = value; } get { return OtherCertT; } }

        /// <summary>
        /// 税务登记证
        /// </summary>
        public string taxRegistrationCert { get { return taxRegistrationCertT; } set { taxRegistrationCertT = value; } }

        /// <summary>
        /// 营业执照证书
        /// </summary>
        public string BusinessLicenseCert1 { get { return BusinessLicenseCert1T; } set { BusinessLicenseCert1T = value; } }

        /// <summary>
        /// 商品证书
        /// </summary>
        public string ProductCert1 { get { return ProductCert1T; } set { ProductCert1T = value; } }

        /// <summary>
        /// 其他证书
        /// </summary>
        public string OtherCert1 { get { return OtherCert1T; } set { OtherCert1T = value; } }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get { return RealNameT; } set { RealNameT = value; } }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string MemberEmail { get { return MemberEmailT; } set { MemberEmailT = value; } }

        /// <summary>
        /// 邮箱验证码
        /// </summary>
        public string EmailCode { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        public string MemberPhone { get { return MemberPhoneT; } set { MemberPhoneT = value; } }

        /// <summary>
        /// 手机验证码
        /// </summary>
        public string PhoneCode { get; set; }

        /// <summary>
        /// 入驻类型
        /// </summary>
        public Himall.CommonModel.ShopBusinessType BusinessType { get; set; }

        /// <summary>
        /// 字段是否必填
        /// </summary>
        public Himall.DTO.Settled Settled { get; set; }
    }

    /// <summary>
    /// 个人入驻，第一步
    /// </summary>
    public class ShopProfileSteps1 : ShopProfileStepsT1
    {
        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get { return CompanyNameT; } set { CompanyNameT = value; } }

        /// <summary>
        /// 公司所在地
        /// </summary>
        public int CityRegionId { get { return CityRegionIdT; } set { CityRegionIdT = value; } }

        /// <summary>
        /// 公司详细地址
        /// </summary>
        public string Address { get { return AddressT; } set { AddressT = value; } }

        /// <summary>
        /// 身份证号码
        /// </summary>
        public string IDCard { get { return IDCardT; } set { IDCardT = value; } }

        /// <summary>
        /// 身份证正面照
        /// </summary>
        public string IDCardUrl { get { return IDCardUrlT; } set { IDCardUrlT = value; } }

        /// <summary>
        /// 身份证背面照
        /// </summary>
        public string IDCardUrl2 { get { return IDCardUrlT2; } set { IDCardUrlT2 = value; } }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get { return RealNameT; } set { RealNameT = value; } }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string MemberEmail { get { return MemberEmailT; } set { MemberEmailT = value; } }

        /// <summary>
        /// 邮箱验证码
        /// </summary>
        public string EmailCode { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        public string MemberPhone { get { return MemberPhoneT; } set { MemberPhoneT = value; } }

        /// <summary>
        /// 手机验证码
        /// </summary>
        public string PhoneCode { get; set; }

        /// <summary>
        /// 入驻类型
        /// </summary>
        public Himall.CommonModel.ShopBusinessType BusinessType { get; set; }

        /// <summary>
        /// 字段是否必填
        /// </summary>
        public Himall.DTO.Settled Settled { get; set; }
    }
    #endregion

    #region ShopProfileStep2

    public class ShopProfileStep2 : ShopProfileStepT2
    {
        /// <summary>
        /// 银行开户名
        /// </summary>
        public string BankAccountName { get { return BankAccountNameT; } set { BankAccountNameT = value; } }

        /// <summary>
        /// 公司银行账号
        /// </summary>
        public string BankAccountNumber { get { return BankAccountNumberT; } set { BankAccountNumberT = value; } }

        /// <summary>
        /// 开户银行支行名称
        /// </summary>
        public string BankName { get { return BankNameT; } set { BankNameT = value; } }

        /// <summary>
        /// 支行联行号
        /// </summary>
        public string BankCode { get { return BankCodeT; } set { BankCodeT = value; } }

        /// <summary>
        /// 开户银行所在地
        /// </summary>
        public int BankRegionId { get { return BankRegionIdT; } set { BankRegionIdT = value; } }

        /// <summary>
        /// 开户银行许可证电子版
        /// </summary>
        public string BankPhoto { get { return BankPhotoT; } set { BankPhotoT = value; } }

        /// <summary>
        /// 税务登记证号
        /// </summary>
        public string TaxRegistrationCertificate { get { return TaxRegistrationCertificateT; } set { TaxRegistrationCertificateT = value; } }

        /// <summary>
        /// 税务登记证号电子版
        /// </summary>
        public string TaxRegistrationCertificatePhoto { get { return TaxRegistrationCertificatePhotoT; } set { TaxRegistrationCertificatePhotoT = value; } }

        /// <summary>
        /// 纳税人识别号
        /// </summary>
        public string TaxpayerId { get { return TaxpayerIdT; } set { TaxpayerIdT = value; } }

        private string _WeiXinNickName = "";
        /// <summary>
        /// 微信昵称
        /// </summary>
        public string WeiXinNickName { get { return _WeiXinNickName; } set { _WeiXinNickName = value; } }

        private int _WeiXinSex = 0;
        /// <summary>
        /// 微信性别；0、女；1、男
        /// </summary>
        public int WeiXinSex { get { return _WeiXinSex; } set { _WeiXinSex = value; } }

        private string _WeiXinAddress = "";
        /// <summary>
        /// 微信地址
        /// </summary>
        public string WeiXinAddress { get { return _WeiXinAddress; } set { _WeiXinAddress = value; } }

        private string _WeiXinTrueName = "";
        /// <summary>
        /// 微信真实姓名
        /// </summary>
        public string WeiXinTrueName { get { return _WeiXinTrueName; } set { _WeiXinTrueName = value; } }

        private string _WeiXinOpenId = "";
        /// <summary>
        /// 微信OpenId
        /// </summary>
        public string WeiXinOpenId { get { return _WeiXinOpenId; } set { _WeiXinOpenId = value; } }

        /// <summary>
        /// 入驻类型
        /// </summary>
        public Himall.CommonModel.ShopBusinessType BusinessType { get; set; }

        /// <summary>
        /// 字段是否必填
        /// </summary>
        public Himall.DTO.Settled Settled { get; set; }
    }


    #endregion

    #region ShopProfileStep3

    public class ShopProfileStep3
    {
        [Required(ErrorMessage = "必须填写店铺名称")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string ShopName { get; set; }

        public long ShopGrade { get; set; }

        public long[] Categories { get; set; }

        public List<Himall.DTO.BusinessCategory> BusinessCategory { get; set; }

        /// <summary>
        /// 入驻类型
        /// </summary>
        public Himall.CommonModel.ShopBusinessType BusinessType { get; set; }
    }


    #endregion


}