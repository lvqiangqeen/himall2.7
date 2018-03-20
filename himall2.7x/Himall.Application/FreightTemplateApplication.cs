using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Core;
using Himall.DTO;

namespace Himall.Application
{
    public class FreightTemplateApplication
    {
        private static IFreightTemplateService _iFreightTemplateService = ObjectContainer.Current.Resolve<IFreightTemplateService>();

        public static List<FreightTemplate> GetShopFreightTemplate(long shopId)
        {
            var model = _iFreightTemplateService.GetShopFreightTemplate(shopId);
            var list = AutoMapper.Mapper.Map<List<FreightTemplate>>(model);
            return list;
        }
        /// <summary>
        /// 根据模板ID获取运费模板信息
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public static FreightTemplate GetFreightTemplate(long templateId, long shopId)
        {
            var model = _iFreightTemplateService.GetFreightTemplate(templateId);
            if (model.ShopID != shopId)
            {
                throw new HimallException("此运费模板不存在");
            }
            var areas = _iFreightTemplateService.GetFreightAreaContent(templateId);
            var details = _iFreightTemplateService.GetFreightAreaDetail(templateId);

            model.Himall_FreightAreaContent = areas;
            foreach (var m in model.Himall_FreightAreaContent)
            {
                var t = details.Where(a => a.FreightAreaId == m.Id).ToList();
                m.FreightAreaDetailInfo = t;
            }
            var freightTemplate = AutoMapper.Mapper.Map<FreightTemplate>(model);
            var area = freightTemplate.FreightArea.Where(a => a.IsDefault == 0).ToList();
            foreach (var f in area)
            {
                foreach (var a in f.FreightAreaDetail)
                {
                    a.ProvinceName = RegionApplication.GetRegion(a.ProvinceId).Name;
                    if (a.CityId.HasValue && a.CityId.Value != 0)
                        a.CityName = RegionApplication.GetRegion(a.CityId.Value).Name;
                    if (a.CountyId.HasValue && a.CountyId.Value != 0)
                        a.CountyName = RegionApplication.GetRegion(a.CountyId.Value).Name;
                    if (!string.IsNullOrEmpty(a.TownIds))
                    {
                        var regionNames= RegionApplication.GetAllRegions().Where(x => a.TownIds.Split(',').ToList().Contains(x.Id.ToString())).Select(t => t.Name).ToList();
                        a.TownNames = string.Join(",", regionNames);
                    }
                }
            }
            return freightTemplate;
        }

        /// <summary>
        /// 复制运费模板
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="shopId"></param>
        public static void CopyFreightTemplate(long templateId, long shopId)
        {
            var freight = GetFreightTemplate(templateId, shopId);
            freight.Name = freight.Name +DateTime.Now.ToString("yyyyMMddHHmmss");
            freight.Id = 0;
            AddOrUpdateFreightTemplate(freight);
        }




        /// <summary>
        /// 添加修改运费模板
        /// </summary>
        /// <param name="templateInfo"></param>
        public static void AddOrUpdateFreightTemplate(FreightTemplate templateInfo)
        {
            var freightTemplate = AutoMapper.Mapper.Map<Himall.Model.FreightTemplateInfo>(templateInfo);
            _iFreightTemplateService.UpdateFreightTemplate(freightTemplate);
        }
        /// <summary>
        /// 删除运费模板
        /// </summary>
        /// <param name="templateId"></param>
        public static void DeleteFreightTemplate(long templateId, long shopId)
        {
            bool used = _iFreightTemplateService.IsProductUseFreightTemp(templateId);
            if (used)
            {
                throw new HimallException("此运费模板已使用，不能删除");
            }
            var templateInfo = _iFreightTemplateService.GetFreightTemplate(templateId);
            if (templateInfo.ShopID != shopId)
            {
                throw new HimallException("此运费模板不存在");
            }
            _iFreightTemplateService.DeleteFreightTemplate(templateId);
        }

        /// <summary>
        /// 是否有商品使用过改运费模板
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public static bool IsProductUseFreightTemp(long templateId)
        {
            return _iFreightTemplateService.IsProductUseFreightTemp(templateId);
        }
    }
}
