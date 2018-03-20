using Himall.Model;
using System.Linq;

namespace Himall.IServices
{
    /// <summary>
    /// 商品描述模板
    /// </summary>
    public interface IProductDescriptionTemplateService : IService
    {

        /// <summary>
        /// 获取满足条件的所有模板
        /// </summary>
        /// <param name="name">条件：模板名称</param>
        /// <param name="position">条件：模板位置</param>
        /// <param name="shopId">店铺id</param>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">一页行数</param>
        /// <returns></returns>
        ObsoletePageModel<ProductDescriptionTemplateInfo> GetTemplates(long shopId,int pageNumber,int pageSize, string name = null, ProductDescriptionTemplateInfo.TemplatePosition? position = null);


        /// <summary>
        /// 获取满足条件的所有模板
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <returns></returns>
        IQueryable<ProductDescriptionTemplateInfo> GetTemplates(long shopId);

        /// <summary>
        /// 获取指定id的模板
        /// </summary>
        /// <param name="id">模板id</param>
        /// <param name="shopId">店铺id</param>
        /// <returns></returns>
        ProductDescriptionTemplateInfo GetTemplate(long id,long shopId);

        /// <summary>
        /// 添加模板
        /// </summary>
        /// <param name="template">模板内容</param>
        void AddTemplate(ProductDescriptionTemplateInfo template);

        /// <summary>
        /// 修改模板
        /// </summary>
        /// <param name="template">模板内容</param>
        void UpdateTemplate(ProductDescriptionTemplateInfo template);

        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="ids">待删除的模板id</param>
        /// <param name="shopId">店铺id</param>
        void DeleteTemplate(long shopId,params long [] ids);
    }
}
