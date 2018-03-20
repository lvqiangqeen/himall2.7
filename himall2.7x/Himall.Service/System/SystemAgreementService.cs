using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Entity;

namespace Himall.Service
{
    public class SystemAgreementService : ServiceBase, ISystemAgreementService
    {
        /// <summary>
        /// 获取协议信息
        /// </summary>
        /// <param name="Id">协议类型</param>
        /// <returns></returns>
        public AgreementInfo GetAgreement(Himall.Model.AgreementInfo.AgreementTypes type)
        {
            return Context.AgreementInfo.Where(b => b.AgreementType == (int)type).FirstOrDefault();
        }
        /// <summary>
        /// 添加协议信息
        /// </summary>
        /// <param name="model">协议信息</param>
        public void AddAgreement(AgreementInfo model)
        {
            Context.AgreementInfo.Add(model);
            Context.SaveChanges();
        }

        /// <summary>
        /// 修改协议信息
        /// </summary>
        /// <param name="model">协议信息</param>
        public bool UpdateAgreement(AgreementInfo model)
        {
            AgreementInfo agreement = GetAgreement((Himall.Model.AgreementInfo.AgreementTypes)model.AgreementType);
            agreement.AgreementType = model.AgreementType;
            agreement.AgreementContent = model.AgreementContent;
            agreement.LastUpdateTime = DateTime.Now;
            if (Context.SaveChanges() > 0)
                return true;
            else
                return false;
        }
    }
}
