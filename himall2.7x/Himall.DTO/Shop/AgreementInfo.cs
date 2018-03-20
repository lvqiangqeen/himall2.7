using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 协议
    /// </summary>
    public class AgreementInfo
    {
        long _id = 0;
        /// <summary>
        /// 协议ID
        /// </summary>
        public new long Id { get { return _id; } set { _id = value; } }
        /// <summary>
        /// 协议类别
        /// </summary>
        public Himall.Model.AgreementInfo.AgreementTypes AgreementType { get; set; }
        /// <summary>
        /// 协议内容
        /// </summary>
        public string AgreementContent { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public System.DateTime LastUpdateTime { get; set; }
    }
}
