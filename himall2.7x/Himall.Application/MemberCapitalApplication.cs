using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;

namespace Himall.Application
{
    public class MemberCapitalApplication
    {
        private static IMemberCapitalService _iMemberCapitalService = ObjectContainer.Current.Resolve<IMemberCapitalService>();
        /// <summary>
        /// 取多个会员资产信息
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static ObsoletePageModel<CapitalInfo> GetCapitals(CapitalQuery query)
        {
            return _iMemberCapitalService.GetCapitals(query);
        }
        /// <summary>
        /// 取会员资产
        /// </summary>
        /// <param name="userid">会员ID</param>
        /// <returns></returns>
        public static CapitalInfo GetCapitalInfo(long userid)
        {
            return _iMemberCapitalService.GetCapitalInfo(userid);
        }
        /// <summary>
        /// 取多个会员资产明细
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static ObsoletePageModel<CapitalDetailInfo> GetCapitalDetails(CapitalDetailQuery query)
        {
            return _iMemberCapitalService.GetCapitalDetails(query);
        }

        /// <summary>
        /// 取资产明细
        /// </summary>
        /// <param name="id">明细ID</param>
        /// <returns></returns>
        public static CapitalDetailInfo GetCapitalDetailInfo(long id)
        {
           return  _iMemberCapitalService.GetCapitalDetailInfo(id);
        }

        /// <summary>
        /// 取申请提现记录
        /// </summary>
        /// <param name="MemId"></param>
        /// <returns></returns>
        public static ObsoletePageModel<ApplyWithDrawInfo> GetApplyWithDraw(ApplyWithDrawQuery query)
        {
            return  _iMemberCapitalService.GetApplyWithDraw(query);
        }


        /// <summary>
        /// 设置支付密码
        /// </summary>
        /// <param name="memid"></param>
        /// <param name="pwd"></param>
        public static void SetPayPwd(long memid, string pwd)
        {
            _iMemberCapitalService.SetPayPwd(memid, pwd);
        }
        /// <summary>
        /// 审核会员申请提现
        /// </summary>
        /// <param name="id"></param>
        public static void ConfirmApplyWithDraw(ApplyWithDrawInfo info)
        {
            _iMemberCapitalService.ConfirmApplyWithDraw(info);
        }
        /// <summary>
        /// 拒绝会员提现申请
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="opuser"></param>
        /// <param name="remark"></param>
        public static void RefuseApplyWithDraw(long id, ApplyWithDrawInfo.ApplyWithDrawStatus status, string opuser, string remark)
        {
            _iMemberCapitalService.RefuseApplyWithDraw(id, status, opuser, remark);
        }
        /// <summary>
        /// 添加提现申请
        /// </summary>
        /// <param name="memid"></param>
        /// <param name="amount"></param>
        /// <param name="nickname"></param>
        public static void AddWithDrawApply(ApplyWithDrawInfo model)
        {
            _iMemberCapitalService.AddWithDrawApply(model);
        }
        /// <summary>
        /// 添加充值记录
        /// </summary>
        /// <param name="model"></param>
        public static long AddChargeApply(DTO.ChargeDetail model)
        {
			return _iMemberCapitalService.AddChargeApply(AutoMapper.Mapper.Map<Model.ChargeDetailInfo>(model));
        }
        /// <summary>
        /// 取充值记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ChargeDetailInfo GetChargeDetail(long id)
        {
           return  _iMemberCapitalService.GetChargeDetail(id);
        }

		/// <summary>
		/// 充值成功
		/// </summary>
		/// <param name="chargeDetailId"></param>
		public static void ChargeSuccess(long chargeDetailId)
		{
			_iMemberCapitalService.ChargeSuccess(chargeDetailId);
		}

        /// <summary>
        /// 取充值列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static ObsoletePageModel<ChargeDetailInfo> GetChargeLists(ChargeQuery query)
        {
          return   _iMemberCapitalService.GetChargeLists(query);
        }

        public static void UpdateChargeDetail(ChargeDetailInfo model)
        {
            _iMemberCapitalService.UpdateChargeDetail(model);
        }
        /// <summary>
        /// 添加会员资产
        /// </summary>
        /// <param name="capitalModel"></param>
        public static void AddCapital(CapitalDetailModel model)
        {
            _iMemberCapitalService.AddCapital(model);
        }
        /// <summary>
        /// 更新会员资产金额
        /// </summary>
        /// <param name="memid"></param>
        /// <param name="amount">可用金额</param>
        /// <param name="freezeAmount">冻结金额</param>
        /// <param name="chargeAmount">支付金额</param>
        public static void UpdateCapitalAmount(long memid, decimal amount, decimal freezeAmount, decimal chargeAmount)
        {
            _iMemberCapitalService.UpdateCapitalAmount(memid, amount, freezeAmount, chargeAmount);
        }

        /// <summary>
        /// 根据不同类型生成单号(充值单号、提现单号)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static long CreateCode(CapitalDetailInfo.CapitalDetailType type)
        {
          return   _iMemberCapitalService.CreateCode(type);
        }

        /// <summary>
        /// 添加店铺充值流水
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static long AddChargeDetailShop(ChargeDetailShopInfo model)
        {
          return   _iMemberCapitalService.AddChargeDetailShop(model);
        }

        /// <summary>
        /// 修改店铺充值流水
        /// </summary>
        /// <param name="model"></param>
        public static void UpdateChargeDetailShop(ChargeDetailShopInfo model)
        {
            _iMemberCapitalService.UpdateChargeDetailShop(model);
        }

        /// <summary>
        /// 获取店铺充值流水信息
        /// </summary>
        /// <param name="Id">流水ID</param>
        /// <returns></returns>
        public static ChargeDetailShopInfo GetChargeDetailShop(long Id)
        {
          return   _iMemberCapitalService.GetChargeDetailShop(Id);
        }
    }
}
