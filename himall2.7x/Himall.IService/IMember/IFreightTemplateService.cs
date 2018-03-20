using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface IFreightTemplateService : IService
    {
        /// <summary>
        /// 取卖家所有运费模版
        /// </summary>
        /// <param name="SellerId"></param>
        /// <returns></returns>
        List<FreightTemplateInfo> GetShopFreightTemplate(long ShopID);
        /// <summary>
        /// 根据模版名称取运费模版
        /// </summary>
        /// <param name="TemplateName"></param>
        /// <returns></returns>
        FreightTemplateInfo GetFreightTemplate(long TemplateId);
        /// <summary>
        /// 取模版运送规则
        /// </summary>
        /// <param name="TemplateId"></param>
        /// <returns></returns>
        List<FreightAreaContentInfo> GetFreightAreaContent(long TemplateId);

        /// <summary>
        /// 取模板运送地区
        /// </summary>
        /// <param name="TemplateId"></param>
        /// <returns></returns>
        List<FreightAreaDetailInfo> GetFreightAreaDetail(long TemplateId);


        /// <summary>
        /// 更新运费模版
        /// </summary>
        void UpdateFreightTemplate(FreightTemplateInfo templateInfo);

        void DeleteFreightTemplate(long TemplateId);

        bool IsProductUseFreightTemp(long TemplateId);
    }
}
