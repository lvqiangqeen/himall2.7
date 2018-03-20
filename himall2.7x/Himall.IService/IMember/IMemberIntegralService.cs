using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{

    public interface IMemberIntegralConversionFactoryService : IService
    {
        IConversionMemberIntegralBase Create(MemberIntegral.IntegralType type, int use = 0);
    }

    public interface IMemberIntegralService: IService
    {
        /// <summary>
        ///  //用户积分记录增加
        /// </summary>
        /// <param name="model"></param>
        /// <param name="conversionMemberIntegralEntity"></param>
        void AddMemberIntegral(MemberIntegralRecord model, IConversionMemberIntegralBase conversionMemberIntegralEntity = null);
        /// <summary>
        /// 通过多个RecordAction，增加用户积分
        /// </summary>
        /// <param name="model"></param>
        /// <param name="conversionMemberIntegralEntity"></param>
        void AddMemberIntegralByRecordAction(MemberIntegralRecord model, IConversionMemberIntegralBase conversionMemberIntegralEntity = null);

        /// <summary>
        /// 获取用户积分列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<MemberIntegral> GetMemberIntegralList(IntegralQuery query);


        /// <summary>
        /// 根据用户ID获取用户的积分信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        MemberIntegral GetMemberIntegral(long userId);


        /// <summary>
        /// 根据用户ID获取用户的积分信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<MemberIntegral> GetMemberIntegrals(IEnumerable<long> userIds);

        /// <summary>
        /// 获取单个用户的积分记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<MemberIntegralRecord> GetIntegralRecordList(IntegralRecordQuery query);

        /// <summary>
        /// 获取单个用户的积分记录,前台使用
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<MemberIntegralRecord> GetIntegralRecordListForWeb(IntegralRecordQuery query);

        /// <summary>
        ///是否有过登录积分记录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool HasLoginIntegralRecord(long userId);
        /// <summary>
        /// 根据订单号获取晒单积分记录
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        List<MemberIntegralRecordAction> GetIntegralRecordAction(IEnumerable<long> virtualItemIds, MemberIntegral.VirtualItemType type);

        /// <summary>
        /// 设置积分规则
        /// </summary>
        /// <param name="info"></param>
        void SetIntegralRule(IEnumerable<MemberIntegralRule> info);

        /// <summary>
        /// 设置积分兑换规则
        /// </summary>
        void SetIntegralChangeRule(MemberIntegralExchangeRules info);

        /// <summary>
        /// 获取积分兑换规则
        /// </summary>
        /// <returns></returns>
        MemberIntegralExchangeRules GetIntegralChangeRule();

        /// <summary>
        /// 获取积分规则
        /// </summary>
        /// <returns></returns>
        IEnumerable<MemberIntegralRule> GetIntegralRule();

        /// <summary>
        /// 获取累计积分分组
        /// </summary>
        /// <returns></returns>
        UserIntegralGroupModel GetUserHistroyIntegralGroup(long userId);
    }
}
