using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;

/**
 * 注册有奖service实现
 * 2016-05-23
 * **/
namespace Himall.Service
{
    /// <summary>
    /// 注册有奖
    /// </summary>
    public class CouponSendByRegisterService : ServiceBase, ICouponSendByRegisterService
    {
        /// <summary>
        /// 新增设置
        /// </summary>
        /// <param name="mCouponSendByRegister"></param>
        public void AddCouponSendByRegister(Himall.Model.CouponSendByRegisterInfo mCouponSendByRegister)
        {
            var model = Context.CouponSendByRegisterInfo.FirstOrDefault();
            if (model == null)
            {
                Context.CouponSendByRegisterInfo.Add(mCouponSendByRegister);
                Context.SaveChanges();
            }
        }

        /// <summary>
        /// 更新设置
        /// </summary>
        /// <param name="mCouponSendByRegister"></param>
        public void UpdateCouponSendByRegister(Himall.Model.CouponSendByRegisterInfo mCouponSendByRegister)
        {
            var model = Context.CouponSendByRegisterInfo.FirstOrDefault(s => s.Id == mCouponSendByRegister.Id);


            var topicModuleList = new List<Himall.Model.CouponSendByRegisterDetailedInfo>();
            foreach (var item in mCouponSendByRegister.Himall_CouponSendByRegisterDetailed)
            {
                topicModuleList.Add(new Himall.Model.CouponSendByRegisterDetailedInfo()
                {
                    CouponId = item.CouponId,
                    CouponRegisterId = mCouponSendByRegister.Id
                });
            }
            //删除设置详情
            Context.CouponSendByRegisterDetailedInfo.Remove(item => item.CouponRegisterId == mCouponSendByRegister.Id);//先删除
            Context.SaveChanges();

            model.Himall_CouponSendByRegisterDetailed = topicModuleList;
            model.Status = mCouponSendByRegister.Status;
            model.Link = mCouponSendByRegister.Link;
            Context.SaveChanges();
        }

        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <returns></returns>
        public Himall.Model.CouponSendByRegisterInfo GetCouponSendByRegister()
        {
            return Context.CouponSendByRegisterInfo.FirstOrDefault();
        }
    }
}
