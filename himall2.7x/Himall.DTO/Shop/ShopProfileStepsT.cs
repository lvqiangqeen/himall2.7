using Himall.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Himall.DTO
{
    #region ShopProfileStepT1

    /// <summary>
    /// 商家入驻第一步不需要验证实体
    /// </summary>
    public class ShopProfileStepT1
    {
        /// <summary>
        /// 公司名称，验证
        /// </summary>
        [Required(ErrorMessage = "必须填写公司名称")]
        [StringLength(60, ErrorMessage = "最大长度不能超过60")]
        [MinLength(5, ErrorMessage = "公司名称不能小于5个字符")]
        [Remote("CheckCompanyName", "Shop", "SellerAdmin", ErrorMessage = "该公司名已存在")]
        public string CompanyNameT { get; set; }

        /// <summary>
        /// 公司所在地，验证
        /// </summary>
        [Required(ErrorMessage = "必须选择公司所在地")]
        [Range(1, 100000, ErrorMessage = "必须选择公司所在地")]
        public int CityRegionIdT { get; set; }

        /// <summary>
        /// 公司详细地址，验证
        /// </summary>
        [Required(ErrorMessage = "必须填写公司详细地址")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string AddressT { get; set; }

        /// <summary>
        /// 公司电话，验证
        /// </summary>
        [Required(ErrorMessage = "必须填写公司电话")]
        [RegularExpression(@"\d+[\d\,\-]*\d+", ErrorMessage = "请输入正确的公司电话")]
        [StringLength(60, ErrorMessage = "最大长度不能超过60")]
        public string PhoneT { get; set; }

        /// <summary>
        /// 公司人数，验证
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "请选择公司人数")]
        [DisplayName("员工总数")]
        [Required(ErrorMessage = "必须填写员工总数")]
        public CompanyEmployeeCount EmployeeCountT { get; set; }

        /// <summary>
        /// 公司注册资金，验证
        /// </summary>
        [Required(ErrorMessage = "必须填写注册资金")]
        [DataType(DataType.Currency, ErrorMessage = "必须为货币值")]
        [Range(typeof(decimal), "0.00", "10000.00", ErrorMessage = "输入不大于万万级数据")]
        public decimal RegisterMoneyT { get; set; }

        /// <summary>
        /// 联系人姓名，验证
        /// </summary>
        [Required(ErrorMessage = "必须填写联系人姓名")]
        [StringLength(60, ErrorMessage = "最大长度不能超过60")]
        public string ContactNameT { get; set; }

        /// <summary>
        /// 联系人电话，验证
        /// </summary>
        [Required(ErrorMessage = "必须填写联系人电话")]
        [RegularExpression(@"\d+[\d\,\-]*\d+", ErrorMessage = "请输入正确的联系人电话")]
        [StringLength(60, ErrorMessage = "最大长度不能超过60")]
        public string ContactPhoneT { get; set; }

        /// <summary>
        /// 联系邮箱，验证
        /// </summary>
        [Required(ErrorMessage = "必须填写电子邮箱")]
        [EmailAddress(ErrorMessage = "电子邮箱格式不正确")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string EmailT { get; set; }

        /// <summary>
        /// 营业执照，验证
        /// </summary>
        [Required(ErrorMessage = "必须填写营业执照号")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        [Remote("CheckBusinessLicenceNumber", "Shop", "SellerAdmin", ErrorMessage = "该营业执照号已存在")]
        public string BusinessLicenceNumberT { get; set; }

        /// <summary>
        /// 营业执照所在地，验证
        /// </summary>
        [Required(ErrorMessage = "必须填写营业执照所在地")]
        public int BusinessLicenceAreaT { get; set; }

        /// <summary>
        /// 营业执照起始有效期，验证
        /// </summary>
        [Required(ErrorMessage = "必须填写营业执照起始有效期")]
        public DateTime BusinessLicenceValidStartT { get; set; }

        /// <summary>
        /// 营业执照截止有效期，验证
        /// </summary>
        [Required(ErrorMessage = "必须填写营业执照截止有效期")]
        public DateTime BusinessLicenceValidEndT { get; set; }

        /// <summary>
        /// 经营范围，验证
        /// </summary>
        [Required(ErrorMessage = "必须填写法定经营范围")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string BusinessSphereT { get; set; }

        /// <summary>
        /// 营业执照URL，验证
        /// </summary>
        [Required(ErrorMessage = "必须上传营业执照号电子版")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string BusinessLicenceNumberPhotoT { get; set; }

        /// <summary>
        /// 组织机构代码证，验证
        /// </summary>
        [Required(ErrorMessage = "必须填写组织机构代码")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string OrganizationCodeT { get; set; }

        /// <summary>
        /// 住址机构代码证URL，验证
        /// </summary>
        [Required(ErrorMessage = "必须上传组织机构代码证电子版")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string OrganizationCodePhotoT { get; set; }

        /// <summary>
        /// 一般纳税人证明，验证
        /// </summary>
        [Required(ErrorMessage = "必须填写一般纳税人证明")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string GeneralTaxpayerPhotoT { get; set; }

        /// <summary>
        /// 法人代表，验证
        /// </summary>
        [Required(ErrorMessage = "必须填写公司法定代表人")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string legalPersonT { get; set; }

        /// <summary>
        /// 公司成立日期，验证
        /// </summary>
        [Required(ErrorMessage = "必须填写公司成立日期")]
        public DateTime CompanyFoundingDateT { get; set; }

        /// <summary>
        /// 经营许可类证书，验证
        /// </summary>
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string BusinessLicenseCertT { set; get; }

        /// <summary>
        /// 产品类证书，验证
        /// </summary>
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string ProductCertT { set; get; }

        /// <summary>
        /// 其它证书，验证
        /// </summary>
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string OtherCertT { set; get; }

        /// <summary>
        /// 税务登记证，验证
        /// </summary>
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string taxRegistrationCertT { set; get; }

        /// <summary>
        /// 营业执照证书，验证
        /// </summary>
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string BusinessLicenseCert1T { get; set; }

        /// <summary>
        /// 商品证书，验证
        /// </summary>
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string ProductCert1T { get; set; }

        /// <summary>
        /// 其他证书，验证
        /// </summary>
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string OtherCert1T { get; set; }

        /// <summary>
        /// 管理员姓名
        /// </summary>
        [Required(ErrorMessage = "必须填写管理员姓名")]
        [StringLength(60, ErrorMessage = "最大长度不能超过60")]
        public string RealNameT { get; set; }

        private string _MemberEmailT = "";
        /// <summary>
        /// 邮箱
        /// </summary>
        [Required(ErrorMessage = "必须邮箱认证")]
        [StringLength(60, ErrorMessage = "最大长度不能超过60")]
        public string MemberEmailT { get { return _MemberEmailT; } set { if (value != null) { _MemberEmailT = value; } } }

        private string _MemberPhoneT = "";
        /// <summary>
        /// 手机
        /// </summary>
        [Required(ErrorMessage = "必须电话认证")]
        [RegularExpression(@"\d+[\d\,\-]*\d+", ErrorMessage = "请输入正确的联系人电话")]
        [StringLength(60, ErrorMessage = "最大长度不能超过60")]
        public string MemberPhoneT { get { return _MemberPhoneT; } set { if (value != null) { _MemberPhoneT = value; } } }

    }

    /// <summary>
    /// 个人入驻第一步
    /// </summary>
    public class ShopProfileStepsT1
    {
        /// <summary>
        /// 公司名称，验证
        /// </summary>
        [Required(ErrorMessage = "必须填写个人姓名名称")]
        [StringLength(60, ErrorMessage = "最大长度不能超过60")]
        [MinLength(2, ErrorMessage = "个人姓名不能小于2个字符")]
        public string CompanyNameT { get; set; }

        /// <summary>
        /// 公司所在地，验证
        /// </summary>
        [Required(ErrorMessage = "必须选择所在地")]
        [Range(1, 100000, ErrorMessage = "必须选择所在地")]
        public int CityRegionIdT { get; set; }

        /// <summary>
        /// 详细地址，验证
        /// </summary>
        [Required(ErrorMessage = "必须填写详细地址")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string AddressT { get; set; }

        /// <summary>
        /// 身份证号码
        /// </summary>
        [Required(ErrorMessage = "必须填写身份证号码")]
        [StringLength(18, ErrorMessage = "请输入正确的身份证号")]
        [MinLength(18, ErrorMessage = "请输入正确的身份证号")]
        public string IDCardT { get; set; }

        /// <summary>
        /// 身份证正面照
        /// </summary>
        [Required(ErrorMessage = "必须上传身份证电子版")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string IDCardUrlT { get; set; }

        /// <summary>
        /// 身份证背面照
        /// </summary>
        [Required(ErrorMessage = "必须上传身份证电子版")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string IDCardUrlT2 { get; set; }

        /// <summary>
        /// 管理员姓名
        /// </summary>
        [Required(ErrorMessage = "必须填写管理员姓名")]
        [StringLength(10, ErrorMessage = "最大长度不能超过10位")]
        [MinLength(2, ErrorMessage = "管理员姓名不得小于2位")]
        public string RealNameT { get; set; }

        private string _MemberEmailT = "";
        /// <summary>
        /// 邮箱
        /// </summary>
        [Required(ErrorMessage = "必须邮箱认证")]
        [StringLength(60, ErrorMessage = "最大长度不能超过60")]
        public string MemberEmailT { get { return _MemberEmailT; } set { if (value != null) { _MemberEmailT = value; } } }

        private string _MemberPhoneT = "";
        /// <summary>
        /// 手机
        /// </summary>
        [Required(ErrorMessage = "必须电话认证")]
        [RegularExpression(@"\d+[\d\,\-]*\d+", ErrorMessage = "请输入正确的联系人电话")]
        [StringLength(60, ErrorMessage = "最大长度不能超过60")]
        public string MemberPhoneT { get { return _MemberPhoneT; } set { if (value != null) { _MemberPhoneT = value; } } }

    }
    #endregion

    #region ShopProfileStepT2

    public class ShopProfileStepT2
    {
        [Required(ErrorMessage = "必须填写银行开户名")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string BankAccountNameT { get; set; }

        [Required(ErrorMessage = "必须填写公司银行账号")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string BankAccountNumberT { get; set; }

        [Required(ErrorMessage = "必须填写开户银行支行名称")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string BankNameT { get; set; }

        [Required(ErrorMessage = "必须填写支行联行号")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string BankCodeT { get; set; }

        [Required(ErrorMessage = "必须填写开户银行所在地")]
        [Range(1, 100000, ErrorMessage = "必须选择开户银行所在地")]
        public int BankRegionIdT { get; set; }

        [Required(ErrorMessage = "必须上传开户银行许可证电子版")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string BankPhotoT { get; set; }

        [Required(ErrorMessage = "必须填写税务登记证号")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string TaxRegistrationCertificateT { get; set; }

        [Required(ErrorMessage = "必须填写税务登记证号电子版")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string TaxRegistrationCertificatePhotoT { get; set; }

        [Required(ErrorMessage = "必须填写纳税人识别号")]
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string TaxpayerIdT { get; set; }

        private string _WeiXinNickNameT = "";
        /// <summary>
        /// 微信昵称
        /// </summary>
        public string WeiXinNickNameT { get { return _WeiXinNickNameT; } set { _WeiXinNickNameT = value; } }

        private int _WeiXinSexT = 0;
        /// <summary>
        /// 微信性别；0、男；1、女
        /// </summary>
        public int WeiXinSex { get { return _WeiXinSexT; } set { _WeiXinSexT = value; } }

        private string _WeiXinAddressT = "";
        /// <summary>
        /// 微信地址
        /// </summary>
        public string WeiXinAddressT { get { return _WeiXinAddressT; } set { _WeiXinAddressT = value; } }

        private string _WeiXinTrueNameT = "";
        /// <summary>
        /// 微信真实姓名
        /// </summary>
        public string WeiXinTrueNameT { get { return _WeiXinTrueNameT; } set { _WeiXinTrueNameT = value; } }

        private string _WeiXinOpenIdT = "";
        /// <summary>
        /// 微信OpenId
        /// </summary>
        public string WeiXinOpenIdT { get { return _WeiXinOpenIdT; } set { _WeiXinOpenIdT = value; } }

    }


    #endregion


    #region ShopProfileStepT3

    public class ShopProfileStepT3
    {
        [StringLength(100, ErrorMessage = "最大长度不能超过100")]
        public string ShopName { get; set; }

        public long ShopGrade { get; set; }

        public long[] Categories { get; set; }

        public List<BusinessCategoryInfo> BusinessCategory { get; set; }
    }


    #endregion


}