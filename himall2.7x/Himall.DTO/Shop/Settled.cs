using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 商家入驻设置
    /// </summary>
    public class Settled
    {
        private long _ID = 0;
        /// <summary>
        /// 设置主键ID
        /// </summary>
        public long ID { get { return _ID; } set { _ID = value; } }

        private Himall.CommonModel.BusinessType _BusinessType = Himall.CommonModel.BusinessType.Enterprise;
        /// <summary>
        /// 商家类型
        /// </summary>
        public Himall.CommonModel.BusinessType BusinessType { get { return _BusinessType; } set { _BusinessType = value; } }

        private Himall.CommonModel.SettleAccountsType _SettlementAccountType = Himall.CommonModel.SettleAccountsType.SettleBank;
        /// <summary>
        /// 商家结算类型
        /// </summary>
        public Himall.CommonModel.SettleAccountsType SettlementAccountType { get { return _SettlementAccountType; } set { _SettlementAccountType = value; } }

        private int _TrialDays = 0;
        /// <summary>
        /// 试用天数
        /// </summary>
        public int TrialDays { get { return _TrialDays; } set { _TrialDays = value; } }

        private Himall.CommonModel.VerificationStatus _IsCity = Himall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 地址必填
        /// </summary>
        public Himall.CommonModel.VerificationStatus IsCity { get { return _IsCity; } set { _IsCity = value; } }

        private Himall.CommonModel.VerificationStatus _IsPeopleNumber = Himall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 人数必填
        /// </summary>
        public Himall.CommonModel.VerificationStatus IsPeopleNumber { get { return _IsPeopleNumber; } set { _IsPeopleNumber = value; } }

        private Himall.CommonModel.VerificationStatus _IsAddress = Himall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 详细地址必填
        /// </summary>
        public Himall.CommonModel.VerificationStatus IsAddress { get { return _IsAddress; } set { _IsAddress = value; } }

        private Himall.CommonModel.VerificationStatus _IsBusinessLicenseCode = Himall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 营业执照号必填
        /// </summary>
        public Himall.CommonModel.VerificationStatus IsBusinessLicenseCode { get { return _IsBusinessLicenseCode; } set { _IsBusinessLicenseCode = value; } }

        private Himall.CommonModel.VerificationStatus _IsBusinessScope = Himall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 经营范围必填
        /// </summary>
        public Himall.CommonModel.VerificationStatus IsBusinessScope { get { return _IsBusinessScope; } set { _IsBusinessScope = value; } }

        private Himall.CommonModel.VerificationStatus _IsBusinessLicense = Himall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 营业执照必填
        /// </summary>
        public Himall.CommonModel.VerificationStatus IsBusinessLicense { get { return _IsBusinessLicense; } set { _IsBusinessLicense = value; } }

        private Himall.CommonModel.VerificationStatus _IsAgencyCode = Himall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 机构代码必填
        /// </summary>
        public Himall.CommonModel.VerificationStatus IsAgencyCode { get { return _IsAgencyCode; } set { _IsAgencyCode = value; } }

        private Himall.CommonModel.VerificationStatus _IsAgencyCodeLicense = Himall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 机构代码证必填
        /// </summary>
        public Himall.CommonModel.VerificationStatus IsAgencyCodeLicense { get { return _IsAgencyCodeLicense; } set { _IsAgencyCodeLicense = value; } }


        private Himall.CommonModel.VerificationStatus _IsTaxpayerToProve = Himall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 纳税人证明必填
        /// </summary>
        public Himall.CommonModel.VerificationStatus IsTaxpayerToProve { get { return _IsTaxpayerToProve; } set { _IsTaxpayerToProve = value; } }

        private Himall.CommonModel.VerificationType _CompanyVerificationType = Himall.CommonModel.VerificationType.VerifyPhone;
        /// <summary>
        /// 验证类型
        /// </summary>
        public Himall.CommonModel.VerificationType CompanyVerificationType { get { return _CompanyVerificationType; } set { _CompanyVerificationType = value; } }


        private Himall.CommonModel.VerificationStatus _IsSName = Himall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 个人姓名必填
        /// </summary>
        public Himall.CommonModel.VerificationStatus IsSName { get { return _IsSName; } set { _IsSName = value; } }

        private Himall.CommonModel.VerificationStatus _IsSCity = Himall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 个人地址必填
        /// </summary>
        public Himall.CommonModel.VerificationStatus IsSCity { get { return _IsSCity; } set { _IsSCity = value; } }

        private Himall.CommonModel.VerificationStatus _IsSAddress = Himall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 个人详细地址必填
        /// </summary>
        public Himall.CommonModel.VerificationStatus IsSAddress { get { return _IsSAddress; } set { _IsSAddress = value; } }

        private Himall.CommonModel.VerificationStatus _IsSIDCard = Himall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 个人身份证必填
        /// </summary>
        public Himall.CommonModel.VerificationStatus IsSIDCard { get { return _IsSIDCard; } set { _IsSIDCard = value; } }

        private Himall.CommonModel.VerificationStatus _IsSIdCardUrl = Himall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 个人身份证上传
        /// </summary>
        public Himall.CommonModel.VerificationStatus IsSIdCardUrl { get { return _IsSIdCardUrl; } set { _IsSIdCardUrl = value; } }

        private Himall.CommonModel.VerificationType _SelfVerificationType = Himall.CommonModel.VerificationType.VerifyPhone;
        /// <summary>
        /// 个人验证类型
        /// </summary>
        public Himall.CommonModel.VerificationType SelfVerificationType { get { return _SelfVerificationType; } set { _SelfVerificationType = value; } }
    }
}
