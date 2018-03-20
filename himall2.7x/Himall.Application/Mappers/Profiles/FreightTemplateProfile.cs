using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Application.Mappers.Profiles
{
    public class FreightTemplateProfile : Profile
    {
        protected override void Configure()
        {
            base.Configure();
            //模板基本信息
            CreateMap<Model.FreightTemplateInfo, DTO.FreightTemplate>().ForMember(e=>e.FreightArea,(map)=>map.MapFrom(m=>m.Himall_FreightAreaContent));
            CreateMap<DTO.FreightTemplate, Model.FreightTemplateInfo>().ForMember(e => e.Himall_FreightAreaContent, (map) => map.MapFrom(m => m.FreightArea));
            //模板地区
            CreateMap<Model.FreightAreaContentInfo, DTO.FreightArea>().ForMember(e => e.FreightAreaDetail, (map) => map.MapFrom(m => m.FreightAreaDetailInfo)); ;
            CreateMap<DTO.FreightArea, Model.FreightAreaContentInfo>().ForMember(e => e.FreightAreaDetailInfo, (map) => map.MapFrom(m => m.FreightAreaDetail));
            //模板地区详情
            CreateMap<Model.FreightAreaDetailInfo, DTO.FreightAreaDetail>();
            CreateMap<DTO.FreightAreaDetail, Model.FreightAreaDetailInfo>();
        }
    }
}
