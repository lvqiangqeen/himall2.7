using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Himall.Service
{
    public class ProductDescriptionTemplateService : ServiceBase, IProductDescriptionTemplateService
    {
        /// <summary>
        /// 模板所在目录
        /// </summary>
        const string TEMPLATE_DIRECTORY = "/Storage/Shop/{0}/templates/{1}";

        public ObsoletePageModel<ProductDescriptionTemplateInfo> GetTemplates(long shopId,int pageNumber,int pageSize,string name = null, ProductDescriptionTemplateInfo.TemplatePosition? position = null)
        {
            var templates = Context.ProductDescriptionTemplateInfo.FindBy(item => item.ShopId == shopId);
            if (position.HasValue)
                templates = templates.Where(item => item.Position == position.Value);

            if (!string.IsNullOrWhiteSpace(name))
                templates = templates.Where(item => item.Name.Contains(name));


            var pageMode = new ObsoletePageModel<ProductDescriptionTemplateInfo>()
            {
                Total = templates.Count(),
                Models = templates.OrderByDescending(item=>item.Id).Skip(pageSize * (pageNumber - 1)).Take(pageSize)
            };

            return pageMode;
        }

        public IQueryable<ProductDescriptionTemplateInfo> GetTemplates(long shopId)
        {
            return Context.ProductDescriptionTemplateInfo.FindBy(t=>t.ShopId.Equals(shopId));
        }

        public Model.ProductDescriptionTemplateInfo GetTemplate(long id, long shopId)
        {
            return Context.ProductDescriptionTemplateInfo.FindBy(item => item.Id == id && item.ShopId == shopId).FirstOrDefault();
        }

        public void AddTemplate(ProductDescriptionTemplateInfo template)
        {
            CheckProperty(template);//检查属性合法性
            template.Content = HTMLProcess(template.Content, template.Id, template.ShopId);//获取外站图片及去除script脚本,防止注入
            template.MobileContent = HTMLProcess(template.MobileContent, template.Id, template.ShopId);
            Context.ProductDescriptionTemplateInfo.Add(template);
            Context.SaveChanges();
        }

        public void UpdateTemplate(ProductDescriptionTemplateInfo template)
        {
            CheckProperty(template);//检查属性合法性

            var oldTemplate = Context.ProductDescriptionTemplateInfo.FindById(template.Id);
            oldTemplate.Name = template.Name;
            oldTemplate.Position = template.Position;
            oldTemplate.Content = HTMLProcess(template.Content, template.Id, template.ShopId);//获取外站图片及去除script脚本,防止注入
            oldTemplate.MobileContent = HTMLProcess(template.MobileContent, template.Id, template.ShopId);
            Context.SaveChanges();
        }

        public void DeleteTemplate(long shopId, params long[] ids)
        {

            var templates = Context.ProductDescriptionTemplateInfo.Where(item => ids.Contains(item.Id));
            if (templates.Count(item => item.ShopId != shopId) > 0)
                throw new HimallException("不能删除非本店铺的商品描述模板");

            IEnumerable<string> templateDirs = templates.Select(item => item.Id.ToString()).ToArray();

            templateDirs = templateDirs.Select(item => string.Format(TEMPLATE_DIRECTORY, shopId, item));

            //删除图片
            foreach (var dir in templateDirs)
            {
                if (Core.HimallIO.ExistDir(dir))
                {
                    Core.HimallIO.DeleteDir(dir, true);
                }
            }

            Context.ProductDescriptionTemplateInfo.Remove(item => ids.Contains(item.Id));
            Context.SaveChanges();

        }



        void CheckProperty(ProductDescriptionTemplateInfo template)
        {
            if (string.IsNullOrWhiteSpace(template.Content))
                throw new InvalidPropertyException("模板内容不可空");
            if (string.IsNullOrWhiteSpace(template.Name))
                throw new InvalidPropertyException("模板名称不可空");
            if(template.ShopId==0)
                throw new InvalidPropertyException("店铺id不可空");
        }

        /// <summary>
        /// 转移外站图片，去除script脚本
        /// </summary>
        /// <param name="content">html内容</param>
        /// <param name="id"></param>
        /// <returns></returns>
        string HTMLProcess(string content, long id,long shopId)
        {
            string imageRealtivePath = string.Format(TEMPLATE_DIRECTORY, shopId, id);
            content = Core.Helper.HtmlContentHelper.TransferToLocalImage(content, "/", imageRealtivePath, Core.HimallIO.GetImagePath(imageRealtivePath) + "/");
            content = Core.Helper.HtmlContentHelper.RemoveScriptsAndStyles(content);
            return content;
        }




    }
}
