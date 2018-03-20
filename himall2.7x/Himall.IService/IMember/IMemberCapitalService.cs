using Himall.Model;
using Himall.IServices.QueryModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface IMemberCapitalService : IService
    {
        /// <summary>
        /// 取多个会员资产信息
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        ObsoletePageModel<CapitalInfo> GetCapitals(CapitalQuery query);
        /// <summary>
        /// 取会员资产
        /// </summary>
        /// <param name="userid">会员ID</param>
        /// <returns></returns>
        CapitalInfo GetCapitalInfo(long userid);
        /// <summary>
        /// 取多个会员资产明细
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<CapitalDetailInfo> GetCapitalDetails(CapitalDetailQuery query);

        /// <summary>
        /// 取多个会员资产明细
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        List<CapitalDetailInfo> GetCapitalDetailList(CapitalDetailQuery query);

        /// <summary>
        /// 取资产明细
        /// </summary>
        /// <param name="id">明细ID</param>
        /// <returns></returns>
        CapitalDetailInfo GetCapitalDetailInfo(long id);

        /// <summary>
        /// 取申请提现记录
        /// </summary>
        /// <param name="MemId"></param>
        /// <returns></returns>
        ObsoletePageModel<ApplyWithDrawInfo> GetApplyWithDraw(ApplyWithDrawQuery query);

        ApplyWithDrawInfo GetWithDraw(long id);

        /// <summary>
        /// 设置支付密码
        /// </summary>
        /// <param name="memid"></param>
        /// <param name="pwd"></param>
        void SetPayPwd(long memid,string pwd);
        /// <summary>
        /// 审核会员申请提现
        /// </summary>
        /// <param name="id"></param>
        void ConfirmApplyWithDraw(ApplyWithDrawInfo info);
        /// <summary>
        /// 拒绝会员提现申请
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="opuser"></param>
        /// <param name="remark"></param>
        void RefuseApplyWithDraw(long id, ApplyWithDrawInfo.ApplyWithDrawStatus status, string opuser, string remark);
        /// <summary>
        /// 添加提现申请
        /// </summary>
        /// <param name="memid"></param>
        /// <param name="amount"></param>
        /// <param name="nickname"></param>
        void AddWithDrawApply(ApplyWithDrawInfo model);
        /// <summary>
        /// 添加充值记录
        /// </summary>
        /// <param name="model"></param>
        long AddChargeApply(ChargeDetailInfo model);
        /// <summary>
        /// 取充值记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ChargeDetailInfo GetChargeDetail(long id);

        /// <summary>
        /// 取充值列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<ChargeDetailInfo> GetChargeLists(ChargeQuery query);

        void UpdateChargeDetail(ChargeDetailInfo model);
        /// <summary>
        /// 添加会员资产
        /// </summary>
        /// <param name="capitalModel"></param>
        void AddCapital(CapitalDetailModel model);
        /// <summary>
        /// 更新会员资产金额
        /// </summary>
        /// <param name="memid"></param>
        /// <param name="amount">可用金额</param>
        /// <param name="freezeAmount">冻结金额</param>
        /// <param name="chargeAmount">支付金额</param>
        void UpdateCapitalAmount(long memid,decimal amount,decimal freezeAmount,decimal chargeAmount);
        
        /// <summary>
        /// 根据不同类型生成单号(充值单号、提现单号)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        long CreateCode(CapitalDetailInfo.CapitalDetailType type);

        /// <summary>
        /// 添加店铺充值流水
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        long AddChargeDetailShop(ChargeDetailShopInfo model);

        /// <summary>
        /// 修改店铺充值流水
        /// </summary>
        /// <param name="model"></param>
        void UpdateChargeDetailShop(ChargeDetailShopInfo model);

        /// <summary>
        /// 获取店铺充值流水信息
        /// </summary>
        /// <param name="Id">流水ID</param>
        /// <returns></returns>
        ChargeDetailShopInfo GetChargeDetailShop(long Id);

		/// <summary>
		/// 充值成功
		/// </summary>
		/// <param name="chargeDetailId"></param>
		void ChargeSuccess(long chargeDetailId);
    }
}
