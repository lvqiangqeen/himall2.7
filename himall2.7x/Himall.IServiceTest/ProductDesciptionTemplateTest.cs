using Himall.IServices;
using Himall.Model;
using Himall.ServiceProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Himall.IServiceTest
{
    [TestClass]
    public class ProductDesciptionTemplateTest
    {
        IProductDescriptionTemplateService service = Instance<IProductDescriptionTemplateService>.Create;

        [TestMethod]
        public void AddTemplateTest()
        {

            var template = new ProductDescriptionTemplateInfo()
            {
                ShopId = 33,
                Position = ProductDescriptionTemplateInfo.TemplatePosition.Top,
                Name = "测试1",
                Content = "这是HTML内容"
            };

            service.AddTemplate(template);

        }

        [TestMethod]
        public void GetTemplateTest()
        {

            var template = new ProductDescriptionTemplateInfo()
            {
                ShopId = 33,
                Position = ProductDescriptionTemplateInfo.TemplatePosition.Top,
                Name = "测试1",
                Content = "这是HTML内容"
            };

            service.AddTemplate(template);

            var actualTemplate = service.GetTemplate(template.Id, template.ShopId);
            Assert.AreEqual(template.Name, actualTemplate.Name);


        }


        [TestMethod]
        public void UpdateTemplateTest()
        {

            var template = new ProductDescriptionTemplateInfo()
            {
                ShopId = 33,
                Position = ProductDescriptionTemplateInfo.TemplatePosition.Top,
                Name = "测试1",
                Content = "这是HTML内容"
            };

            service.AddTemplate(template);

            var newTemplate = new ProductDescriptionTemplateInfo()
            {
                Id=template.Id,
                ShopId = 33,
                Position = ProductDescriptionTemplateInfo.TemplatePosition.Bottom,
                Name = "测试"+System.DateTime.Now.ToString("yyyyMMddHHmmss"),
                Content = "这是HTML内容" + System.DateTime.Now.ToString("yyyyMMddHHmmssffffff")
            };

            service.UpdateTemplate(newTemplate);

            var actualTemplate = service.GetTemplate(template.Id,template.ShopId);

            Assert.AreEqual(newTemplate.Content, actualTemplate.Content);
            Assert.AreEqual(newTemplate.Name, actualTemplate.Name);
        }



        [TestMethod]
        public void DeleteTemplateTest()
        {

            var template = new ProductDescriptionTemplateInfo()
            {
                ShopId = 33,
                Position = ProductDescriptionTemplateInfo.TemplatePosition.Top,
                Name = "测试1",
                Content = "这是HTML内容"
            };
            service.AddTemplate(template);

            var actualTemplate = service.GetTemplate(template.Id, template.ShopId);
            Assert.IsNotNull(actualTemplate);
            service.DeleteTemplate(template.ShopId, template.Id);

            actualTemplate = service.GetTemplate(template.Id, template.ShopId);
            Assert.IsNull(actualTemplate);


        }



    }
}
