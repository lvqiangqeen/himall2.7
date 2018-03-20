using Himall.IServices.QueryModel;
using Himall.Model;
using System.Collections.Generic;
using System.Linq;


namespace Himall.IServices
{
    public interface IMemberInviteService : IService
    {
        //设置邀请规则
        void SetInviteRule(InviteRuleInfo model);

        /// <summary>
        /// 获取邀请规则
        /// </summary>
        /// <returns></returns>
        InviteRuleInfo GetInviteRule();

        //分页查询会员邀请积分记录
        ObsoletePageModel<InviteRecordInfo> GetInviteList(InviteRecordQuery query);

        //添加一条邀请记录
        void AddInviteRecord(InviteRecordInfo info);

        //获取单个会员邀请信息
        UserInviteModel GetMemberInviteInfo(long userId);

        //是否获取过邀请积分
        bool HasInviteIntegralRecord(long RegId);

        //添加邀请积分邀请记录
        void AddInviteIntegel(UserMemberInfo RegMember, UserMemberInfo InviteMember);
    }
}

