using System.ComponentModel.DataAnnotations;
using Himall.Model;
using Himall.Core;
using System;
using System.Collections.Generic;
using Himall.IServices;
using Himall.Web.Framework;
using System.ComponentModel;
using System.Web.Mvc;

namespace Himall.Web.Models
{
    public class CategoryKeyVal
    {
        public string Name { get; set; }
        public decimal CommisRate { get; set; }
    }
    public class ShopModel
    {
        //店铺相关
        public long Id { get; set; }

        [Required(ErrorMessage = "店铺名称为必填项")]
        [MaxLength(20, ErrorMessage = "店铺名称最多20个字符")]
        public string Name { get; set; }
        public string Account { get; set; }

        [Required(ErrorMessage = "店铺套餐为必填项")]
        public string ShopGrade { get; set; }

        [Required(ErrorMessage = "有效期为必填项")]
        public string EndDate { get; set; }
        public string Status { get; set; }
        public bool IsSelf { get; set; }
        public List<CategoryKeyVal> BusinessCategory { get; set; }
        //公司及联系人信息

        public string CompanyName { get; set; }
        public string CompanyRegion { get; set; }

        public string CompanyAddress { get; set; }

        public string CompanyPhone { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "人数必须为大于0的整数")]
        [DisplayName("员工总数")]
        [Required(ErrorMessage = "必须填写员工总数")]
        public CompanyEmployeeCount CompanyEmployeeCount { get; set; }
        [Required(ErrorMessage = "必须填写注册资金")]
        [DataType(DataType.Currency, ErrorMessage = "必须为货币值")]
        [Range(typeof(decimal), "0.00", "10000.00", ErrorMessage = "输入不大于万万级数据")]
        public decimal CompanyRegisteredCapital { get; set; }

        public string ContactsName { get; set; }

        public string ContactsPhone { get; set; }

        public string ContactsEmail { get; set; }

        //营业执照信息
        public string BusinessLicenceNumber { get; set; }
        public string BusinessLicenceNumberPhoto { get; set; }
        public string BusinessLicenceRegionId { get; set; }
        public System.DateTime? BusinessLicenceStart { get; set; }
        public System.DateTime? BusinessLicenceEnd { get; set; }
        public string BusinessSphere { get; set; }

        //组织机构代码证
        public string OrganizationCode { get; set; }
        public string OrganizationCodePhoto { get; set; }

        //一般纳税人证明
        public string GeneralTaxpayerPhot { get; set; }

        //结算账号信息
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string BankRegionId { get; set; }
        public string BankPhoto { get; set; }

        //税务登记证
        public string TaxRegistrationCertificate { get; set; }
        public string TaxpayerId { get; set; }
        public string TaxRegistrationCertificatePhoto { get; set; }

        //付款凭证
        public string PayPhoto { get; set; }
        public string PayRemark { get; set; }

        /// <summary>
        /// 证书
        /// </summary>
        public string BusinessLicenseCert { set; get; }
        public string ProductCert { set; get; }
        public string OtherCert { set; get; }
        public string legalPerson { get; set; }
        public DateTime? CompanyFoundingDate { get; set; }


        public int NewCompanyRegionId { get; set; }
        public int NewBankRegionId { get; set; }
        public ShopModel()
        { }

        /// <summary>
        /// 入驻类别
        /// </summary>
        public Himall.CommonModel.ShopBusinessType BusinessType { get; set; }

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

        public ShopModel(ShopInfo m)
            : this()
        {
            this.Id = m.Id;
            this.Account = m.ShopAccount;
            this.Name = m.ShopName;
            var obj = ServiceHelper.Create<IShopService>().GetShopGrade(m.GradeId);
            this.ShopGrade = obj == null ? "" : obj.Name;
            this.Status = m.ShopStatus.ToDescription();
            this.EndDate = m.EndDate.HasValue ? m.EndDate.Value.ToString("yyyy-MM-dd") : "";
            this.IsSelf = m.IsSelf;

            //公司及联系人信息
            this.CompanyName = m.CompanyName;
            this.NewCompanyRegionId = m.CompanyRegionId;
            this.CompanyRegion = ServiceHelper.Create<IRegionService>().GetFullName(m.CompanyRegionId);
            this.CompanyAddress = m.CompanyAddress;
            this.CompanyPhone = m.CompanyPhone;
            this.CompanyEmployeeCount = m.CompanyEmployeeCount;
            this.CompanyRegisteredCapital = m.CompanyRegisteredCapital;
            this.ContactsName = m.ContactsName;
            this.ContactsPhone = m.ContactsPhone;
            this.ContactsEmail = m.ContactsEmail;

            //证书 经营许可类证书  产品类证书  其它证书
            this.BusinessLicenseCert = m.BusinessLicenseCert;
            this.ProductCert = m.ProductCert;
            this.OtherCert = m.OtherCert;

            //营业执照信息
            this.BusinessLicenceNumber = m.BusinessLicenceNumber;
            this.BusinessLicenceNumberPhoto = m.BusinessLicenceNumberPhoto;
            this.BusinessLicenceRegionId = ServiceHelper.Create<IRegionService>().GetFullName(m.BusinessLicenceRegionId);
            this.BusinessLicenceStart = m.BusinessLicenceStart;
            this.BusinessLicenceEnd = m.BusinessLicenceEnd;
            this.BusinessSphere = m.BusinessSphere;

            //组织机构代码证
            this.OrganizationCode = m.OrganizationCode;
            this.OrganizationCodePhoto = m.OrganizationCodePhoto;

            //一般纳税人证明
            this.GeneralTaxpayerPhot = m.GeneralTaxpayerPhot;

            //结算账号信息
            this.BankAccountName = m.BankAccountName;
            this.BankAccountNumber = m.BankAccountNumber;
            this.BankName = m.BankName;
            this.BankCode = m.BankCode;
            this.BankRegionId = ServiceHelper.Create<IRegionService>().GetFullName(m.BankRegionId);
            this.NewBankRegionId = m.BankRegionId;
            this.BankPhoto = m.BankPhoto;

            //税务登记证
            this.TaxRegistrationCertificate = m.TaxRegistrationCertificate;
            this.TaxpayerId = m.TaxpayerId;
            this.TaxRegistrationCertificatePhoto = m.TaxRegistrationCertificatePhoto;

            //付款凭证
            this.PayPhoto = m.PayPhoto;
            this.PayRemark = m.PayRemark;

            this.legalPerson = m.legalPerson;
            this.CompanyFoundingDate = m.CompanyFoundingDate.HasValue ? m.CompanyFoundingDate.Value : DateTime.Now;

            //类别
            this.BusinessType = m.BusinessType == null ? Himall.CommonModel.ShopBusinessType.Enterprise : m.BusinessType.Value;

            //个人入驻
            this.IDCard = m.IDCard == null ? "" : m.IDCard;
			this.IDCardUrl = m.IDCardUrl == null ? "" : m.IDCardUrl;
			this.IDCardUrl2 = m.IDCardUrl2 == null ? "" : m.IDCardUrl2;

            //微信账户
            this.WeiXinAddress = m.WeiXinAddress == null ? "" : m.WeiXinAddress;
            this.WeiXinNickName = m.WeiXinNickName == null ? "" : m.WeiXinNickName;
            this.WeiXinOpenId = m.WeiXinOpenId == null ? "" : m.WeiXinOpenId;
            this.WeiXinSex = m.WeiXinSex == null ? 0 : m.WeiXinSex.Value;
            this.WeiXinTrueName = m.WeiXinTrueName == null ? "" : m.WeiXinTrueName;
            this.WeiXinImg = m.WeiXinImg == null ? "" : m.WeiXinImg;
        }

        public static implicit operator ShopInfo(ShopModel m)
        {
            return new ShopInfo
            {
                Id = m.Id,
                ShopName = m.Name,
                GradeId = int.Parse(m.ShopGrade),
                EndDate = DateTime.Parse(m.EndDate),
                ShopStatus = (ShopInfo.ShopAuditStatus)(int.Parse(m.Status)),
                BankRegionId = m.NewBankRegionId,
                CompanyRegionId = m.NewCompanyRegionId
            };
        }

		public decimal Balance { get; set; }
	}

    /// <summary>
    /// 个人入驻信息编辑
    /// </summary>
    public class ShopPersonal
    {
        //店铺相关
        public long Id { get; set; }

        [Required(ErrorMessage = "店铺名称为必填项")]
        [MaxLength(20, ErrorMessage = "店铺名称最多20个字符")]
        public string Name { get; set; }
        public string Account { get; set; }

        [Required(ErrorMessage = "店铺套餐为必填项")]
        public string ShopGrade { get; set; }

        [Required(ErrorMessage = "有效期为必填项")]
        public string EndDate { get; set; }

        [Required(ErrorMessage = "必须填写姓名")]
        [MinLength(2, ErrorMessage = "姓名不能小于2个字符")]
        [StringLength(60, ErrorMessage = "公司名称最大长度不能超过60")]
        public string CompanyName { get; set; }
        public string CompanyRegion { get; set; }
        [Required(ErrorMessage = "必须填写详细地址")]
        public string CompanyAddress { get; set; }

        /// <summary>
        /// 身份证号
        /// </summary>
        public string IDCard { get; set; }

        public int NewCompanyRegionId { get; set; }

        /// <summary>
        /// 身份证URL
        /// </summary>
        public string IDCardUrl { get; set; }

		/// <summary>
		/// 身份证背面照
		/// </summary>
		public string IDCardUrl2 { get; set; }
    }
}