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
    public class MemberIntegralApplication
    {
        private static IMemberIntegralService _iMemberIntegralService = ObjectContainer.Current.Resolve<IMemberIntegralService>();

        /// <summary>
        ///  //用户积分记录增加
        /// </summary>
        /// <param name="model"></param>
        /// <param name="conversionMemberIntegralEntity"></param>
        public static void AddMemberIntegral(MemberIntegralRecord model, IConversionMemberIntegralBase conversionMemberIntegralEntity = null)
        {
            _iMemberIntegralService.AddMemberIntegral(model, conversionMemberIntegralEntity);
        }
        /// <summary>
        /// 通过多个RecordAction，增加用户积分
        /// </summary>
        /// <param name="model"></param>
        /// <param name="type"></param>
        public static void AddMemberIntegralByEnum(MemberIntegralRecord model, MemberIntegral.IntegralType type)
        {
            var conversionService= ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create;
            var conversionMemberIntegralEntity = conversionService.Create(MemberIntegral.IntegralType.Share);
            _iMemberIntegralService.AddMemberIntegralByRecordAction(model, conversionMemberIntegralEntity);
        }
        /// <summary>
        /// 获取用户积分列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static ObsoletePageModel<MemberIntegral> GetMemberIntegralList(IntegralQuery query)
        {
            return _iMemberIntegralService.GetMemberIntegralList(query);
        }


        /// <summary>
        /// 根据用户ID获取用户的积分信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static MemberIntegral GetMemberIntegral(long userId)
        {
            return _iMemberIntegralService.GetMemberIntegral(userId);
        }


        /// <summary>
        /// 根据用户ID获取用户的积分信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<Himall.DTO.UserIntegral> GetMemberIntegrals(IEnumerable<long> userIds)
        {
            var model = _iMemberIntegralService.GetMemberIntegrals(userIds);
            var reslut = AutoMapper.Mapper.Map<List<Himall.DTO.UserIntegral>>(model);
            return reslut;
        }

        /// <summary>
        /// 获取单个用户的积分记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static ObsoletePageModel<MemberIntegralRecord> GetIntegralRecordList(IntegralRecordQuery query)
        {
            return _iMemberIntegralService.GetIntegralRecordList(query);
        }

        /// <summary>
        /// 获取单个用户的积分记录,前台使用
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static ObsoletePageModel<MemberIntegralRecord> GetIntegralRecordListForWeb(IntegralRecordQuery query)
        {
            return _iMemberIntegralService.GetIntegralRecordListForWeb(query);
        }

        /// <summary>
        ///是否有过登录积分记录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool HasLoginIntegralRecord(long userId)
        {
            return _iMemberIntegralService.HasLoginIntegralRecord(userId);
        }

        /// <summary>
        /// 设置积分规则
        /// </summary>
        /// <param name="info"></param>
        public static void SetIntegralRule(IEnumerable<MemberIntegralRule> info)
        {
            _iMemberIntegralService.SetIntegralRule(info);
        }

        /// <summary>
        /// 设置积分兑换规则
        /// </summary>
        public static void SetIntegralChangeRule(MemberIntegralExchangeRules info)
        {
            _iMemberIntegralService.SetIntegralChangeRule(info);
        }

        /// <summary>
        /// 获取积分兑换规则
        /// </summary>
        /// <returns></returns>
        public static MemberIntegralExchangeRules GetIntegralChangeRule()
        {
            return _iMemberIntegralService.GetIntegralChangeRule();
        }

        /// <summary>
        /// 获取积分规则
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<MemberIntegralRule> GetIntegralRule()
        {
            return _iMemberIntegralService.GetIntegralRule();
        }

        /// <summary>
        /// 获取累计积分分组
        /// </summary>
        /// <returns></returns>
        public static UserIntegralGroupModel GetUserHistroyIntegralGroup(long userId)
        {
            return _iMemberIntegralService.GetUserHistroyIntegralGroup(userId);
        }

        /// <summary>
        /// 订单是否已经分享
        /// </summary>
        /// <param name="orderid"></param>
        /// <returns>true:已经分享过</returns>
        public static bool OrderIsShared(IEnumerable<long> orderids)
        {
            var recordAction = _iMemberIntegralService.GetIntegralRecordAction(orderids, MemberIntegral.VirtualItemType.ShareOrder);
            if (recordAction.Count > 0)//有分享记录，就认为已经分享过（不管分享的订单个数）
                return true;
            return false;
        }
    }
}
