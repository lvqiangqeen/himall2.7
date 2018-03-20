using Himall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Service
{
    public class SettledService : ServiceBase, ISettledService
    {
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="mSettledInfo"></param>
        public void AddSettled(Himall.Model.SettledInfo mSettledInfo)
        {
            var data = Context.SettledInfo.ToList();
            if (data.Count == 0)
            {
                Context.SettledInfo.Add(mSettledInfo);
                Context.SaveChanges();
                Core.Cache.Remove(CacheKeyCollection.Settled);//清除缓存
            }
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="mSettledInfo"></param>
        public void UpdateSettled(Himall.Model.SettledInfo mSettledInfo)
        {
            var model = Context.SettledInfo.FirstOrDefault(s => s.ID == mSettledInfo.ID);
            model.BusinessType = mSettledInfo.BusinessType;
            model.CompanyVerificationType = mSettledInfo.CompanyVerificationType;
            model.IsAddress = mSettledInfo.IsAddress;
            model.IsAgencyCode = mSettledInfo.IsAgencyCode;
            model.IsAgencyCodeLicense = mSettledInfo.IsAgencyCodeLicense;
            model.IsBusinessLicense = mSettledInfo.IsBusinessLicense;
            model.IsBusinessLicenseCode = mSettledInfo.IsBusinessLicenseCode;
            model.IsBusinessScope = mSettledInfo.IsBusinessScope;
            model.IsCity = mSettledInfo.IsCity;
            model.IsPeopleNumber = mSettledInfo.IsPeopleNumber;
            model.IsSAddress = mSettledInfo.IsSAddress;
            model.IsSCity = mSettledInfo.IsSCity;
            model.IsSIDCard = mSettledInfo.IsSIDCard;
            model.IsSIdCardUrl = mSettledInfo.IsSIdCardUrl;
            model.IsSName = mSettledInfo.IsSName;
            model.IsTaxpayerToProve = mSettledInfo.IsTaxpayerToProve;
            model.SelfVerificationType = mSettledInfo.SelfVerificationType;
            model.SettlementAccountType = mSettledInfo.SettlementAccountType;
            model.TrialDays = mSettledInfo.TrialDays;
            Context.SaveChanges();
            Core.Cache.Remove(CacheKeyCollection.Settled);//清除缓存
        }

        /// <summary>
        /// 查询,获取第一条数据
        /// </summary>
        /// <returns></returns>
        public Himall.Model.SettledInfo GetSettled()
        {
            Himall.Model.SettledInfo mSettledInfo;
            if (Core.Cache.Exists(CacheKeyCollection.Settled))//如果存在缓存，则从缓存中读取
                mSettledInfo = Core.Cache.Get<Himall.Model.SettledInfo>(CacheKeyCollection.Settled);
            else
            {
                mSettledInfo = Context.SettledInfo.FirstOrDefault();
                Core.Cache.Insert(CacheKeyCollection.Settled, mSettledInfo);
            }

            return mSettledInfo;
        }
    }
}
