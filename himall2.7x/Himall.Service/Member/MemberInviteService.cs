using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Service;
using System.Collections.Generic;
using System.Linq;
using Himall.Entity;
using System;


namespace Himall.Service
{
    public class MemberInviteService : ServiceBase, IMemberInviteService
    {
        public void SetInviteRule(InviteRuleInfo model)
        {
            var m = Context.InviteRuleInfo.FirstOrDefault(a => a.Id == model.Id);
            if (m == null)
            {
                Context.InviteRuleInfo.Add(model);
            }
            else
            {
                m.InviteIntegral = model.InviteIntegral;
                m.RegIntegral = model.RegIntegral;
                m.ShareDesc = model.ShareDesc;
                m.ShareIcon = model.ShareIcon;
                m.ShareRule = model.ShareRule;
                m.ShareTitle = model.ShareTitle;
            }
            Context.SaveChanges();
            if (m != null)
                //转移图片
                m.ShareIcon = MoveImages(model.ShareIcon);
            else
                model.ShareIcon = MoveImages(model.ShareIcon);
            Context.SaveChanges();
        }

        string MoveImages(string image)
        {
            if(string.IsNullOrWhiteSpace(image))
            {
                return "";
            }

            var ext = image.Substring(image.LastIndexOf("."));
            //转移图片
            string relativeDir = "/Storage/Plat/MemberInvite/";
            string fileName = "Invite_Icon" + ext;
            if (image.Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                string temp = image.Substring(image.LastIndexOf("/temp"));
                Core.HimallIO.CopyFile(temp, relativeDir + fileName, true);
                return relativeDir + fileName;
            }  //目标地址
            else
            {
                var fname = image.Substring(image.LastIndexOf("/") + 1);
                return relativeDir + fname;
            }
        }

        public InviteRuleInfo GetInviteRule()
        {
           var inviteRule= Context.InviteRuleInfo.FirstOrDefault();
            if(inviteRule==null)
            {
                InviteRuleInfo ruleInfo = new InviteRuleInfo();
                ruleInfo.InviteIntegral = 0;
                ruleInfo.RegIntegral = 0;
                ruleInfo.ShareDesc = "分享描述";
                ruleInfo.ShareRule = "分享规则";
                ruleInfo.ShareTitle = "分享标题";
                return ruleInfo;
            }
            inviteRule.ShareIcon = Core.HimallIO.GetRomoteImagePath(inviteRule.ShareIcon);
            return inviteRule;
        }


        public bool HasInviteIntegralRecord(long RegId)
        {
            return Context.InviteRecordInfo.Any(a => a.RegUserId == RegId);
        }

        public ObsoletePageModel<InviteRecordInfo> GetInviteList(InviteRecordQuery query)
        {
            int total = 0;
            ObsoletePageModel<InviteRecordInfo> result = new ObsoletePageModel<InviteRecordInfo>();
            var datasql = Context.InviteRecordInfo.AsQueryable();
            if (!string.IsNullOrEmpty(query.userName))
            {
                datasql = datasql.Where(d => d.UserName.Equals(query.userName));
            }
            datasql = datasql.GetPage(out total, query.PageNo, query.PageSize, d => d.OrderByDescending(a => a.Id));
            //数据转换
            result.Models = datasql;
            result.Total = total;
            return result;
        }

        public UserInviteModel GetMemberInviteInfo(long userId)
        {
            UserInviteModel model = new UserInviteModel();
            var query = Context.InviteRecordInfo.Where(a => a.UserId == userId);
            var PersonCount = query.Count();
            var IntergralCount = query.Sum(a => (int?)a.InviteIntegral).GetValueOrDefault();
            model.InviteIntergralCount = IntergralCount;
            model.InvitePersonCount = PersonCount;
            return model;
        }


        public void AddInviteRecord(InviteRecordInfo info)
        {
            Context.InviteRecordInfo.Add(info);
            Context.SaveChanges();
        }


        public void AddInviteIntegel(UserMemberInfo RegMember, UserMemberInfo InviteMember)
        {
            var InviteRule = GetInviteRule();
            if (InviteRule == null)
            {
                return;
            }

            if (!HasInviteIntegralRecord(RegMember.Id)) //没有过邀请得分，加积分
            {
                var factoryService = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create;
                var integralService = ServiceProvider.Instance<IMemberIntegralService>.Create;
                MemberIntegralRecord info = new MemberIntegralRecord();
                info.UserName = RegMember.UserName;
                info.MemberId = RegMember.Id;
                info.RecordDate = DateTime.Now;
                info.ReMark = "被邀请注册";
                info.TypeId = MemberIntegral.IntegralType.Others;
                var memberIntegral = factoryService.Create(MemberIntegral.IntegralType.Others, InviteRule.RegIntegral.Value);
                integralService.AddMemberIntegral(info, memberIntegral);

                MemberIntegralRecord info2 = new MemberIntegralRecord();
                info2.UserName = InviteMember.UserName;
                info2.MemberId = InviteMember.Id;
                info2.RecordDate = DateTime.Now;
                info2.ReMark = "邀请会员";
                info2.TypeId = MemberIntegral.IntegralType.InvitationMemberRegiste;
                var memberIntegral2 = factoryService.Create(MemberIntegral.IntegralType.InvitationMemberRegiste);
                integralService.AddMemberIntegral(info2, memberIntegral2);

                InviteRecordInfo inviteInfo = new InviteRecordInfo();
                inviteInfo.RegIntegral = InviteRule.RegIntegral.GetValueOrDefault();
                inviteInfo.InviteIntegral = InviteRule.InviteIntegral.GetValueOrDefault();
                inviteInfo.RecordTime = DateTime.Now;
                inviteInfo.RegTime = RegMember.CreateDate;
                inviteInfo.RegUserId = RegMember.Id;
                inviteInfo.RegName = RegMember.UserName;
                inviteInfo.UserId = InviteMember.Id;
                inviteInfo.UserName = InviteMember.UserName;
                AddInviteRecord(inviteInfo);
            }
        }
    }
}

