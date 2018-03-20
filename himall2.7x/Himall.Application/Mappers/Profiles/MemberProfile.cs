using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Himall.CommonModel;

namespace Himall.Application.Mappers.Profiles
{
	public class MemberProfile:Profile
	{
		protected override void Configure()
		{
			base.Configure();

            CreateMap<Himall.Model.MemberGrade, Himall.DTO.MemberGrade>();
            CreateMap<Himall.DTO.MemberGrade, Himall.Model.MemberGrade>();

            CreateMap<Model.UserMemberInfo, DTO.Members>();
			CreateMap<DTO.Members, Model.UserMemberInfo>();
            CreateMap<QueryPageModel<Himall.Model.UserMemberInfo>, QueryPageModel<Himall.DTO.Members>>();

            CreateMap<Model.MemberIntegral, DTO.UserIntegral>();
            CreateMap<DTO.UserIntegral, Model.MemberIntegral>();



            //CreateMap<Himall.Model.LabelInfo, Himall.DTO.Labels>();
            //CreateMap<Himall.DTO.Labels, Himall.Model.LabelInfo>();


            //  CreateMap<Model.MemberConsumeStatisticsInfo, DTO.MemberConsumeStatistics>();
            //  CreateMap<DTO.MemberConsumeStatistics, Model.MemberConsumeStatisticsInfo>();

            CreateMap<Himall.Model.UserMemberInfo, Himall.DTO.MemberPurchasingPower>();
            CreateMap<Model.MemberOpenIdInfo, DTO.MemberOpenId>();
			CreateMap<DTO.MemberOpenId, Model.MemberOpenIdInfo>();

			CreateMap<Model.ChargeDetailInfo, DTO.ChargeDetail>();
			CreateMap<DTO.ChargeDetail, Model.ChargeDetailInfo>();

			CreateMap<Model.PromoterInfo, DTO.Promoter>();
			CreateMap<DTO.Promoter, Model.PromoterInfo>();

            CreateMap<Model.SendMessageRecordInfo, DTO.SendMessageRecord>();
            CreateMap<DTO.SendMessageRecord, Model.SendMessageRecordInfo>();
		}
	}
}
