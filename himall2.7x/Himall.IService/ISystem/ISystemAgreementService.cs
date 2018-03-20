using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface ISystemAgreementService : IService
    {
        /// <summary>
        /// 获取协议信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        AgreementInfo GetAgreement(Himall.Model.AgreementInfo.AgreementTypes type);
        /// <summary>
        /// 添加协议
        /// </summary>
        /// <param name="model"></param>
        void AddAgreement(AgreementInfo model);
        /// <summary>
        /// 修改协议
        /// </summary>
        /// <param name="model"></param>
        bool UpdateAgreement(AgreementInfo model);

    }
}
