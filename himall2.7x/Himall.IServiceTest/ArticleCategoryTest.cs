using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Himall.Model;
using Himall.IServices;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Himall.IServices.QueryModel;

namespace Himall.IServiceTest
{
    [TestClass]
    public class ArticleCategoryTest
    {
        [TestMethod]
        public void ArticleCategoryAdd_Coreect_Test()
        {

            ArticleCategoryInfo articleCategory = new ArticleCategoryInfo()
            {
                ParentCategoryId = 0,
                Name = "这是文章分类" + DateTime.Now.ToString("yyyyMMddHHmmss")
            };
            var service = ServiceProvider.Instance<IArticleCategoryService>.Create;
            service.AddArticleCategory(articleCategory);
            Assert.IsTrue(articleCategory.Id > 0);
        }

        [TestMethod]
        public void Test()
        {
            var service = ServiceProvider.Instance<IDistributionService>.Create;
            ProformanceQuery query = new ProformanceQuery();
            query.PageNo = 1;
            query.PageSize = 9;
           var model= service.GetPerformanceList(query);
            Assert.IsTrue( model.Total> 0);
        }



        [TestMethod]
        public void ArticleCategoryAdd_ParentIdError_Test()
        {

            ArticleCategoryInfo articleCategory = new ArticleCategoryInfo()
            {
                ParentCategoryId = -1,
                Name = "这是文章分类" + DateTime.Now.ToString("yyyyMMddHHmmss")
            };
            var service = ServiceProvider.Instance<IArticleCategoryService>.Create;

            try
            {
                service.AddArticleCategory(articleCategory);
                Assert.Fail();
            }
            catch (Exception )
            {
                
            }
        }


        [TestMethod]
        public void ArticleCategoryAdd_NameIsEmpty_Test()
        {

            ArticleCategoryInfo articleCategory = new ArticleCategoryInfo()
            {
                ParentCategoryId = 0,
            };
            var service = ServiceProvider.Instance<IArticleCategoryService>.Create;

            try
            {
                service.AddArticleCategory(articleCategory);
                Assert.Fail();
            }
            catch (Exception )
            {

            }
        }


        [TestMethod]
        public void ArticleCategoryGetTest()
        {
             var service = ServiceProvider.Instance<IArticleCategoryService>.Create;
             var articleCategories = service.GetArticleCategoriesByParentId(0);
             Assert.IsTrue(articleCategories.Count() > 0);
        }


        [TestMethod]
        public void ArticleCategoryUpdateTest()
        {
            var service = ServiceProvider.Instance<IArticleCategoryService>.Create;
            ArticleCategoryInfo articleCategory = new ArticleCategoryInfo()
            {
                ParentCategoryId = 0,
                Name = "这是文章分类" + DateTime.Now.ToString("yyyyMMddHHmmss")
            };
            service.AddArticleCategory(articleCategory);

            string newName = "修改后" + DateTime.Now.ToString("yyyyMMddHHmmss");
            articleCategory.Name = newName;
            service.UpdateArticleCategory(articleCategory);

            ArticleCategoryInfo newArticleCategory = service.GetArticleCategory(articleCategory.Id);
            Assert.AreEqual(newName, newArticleCategory.Name);
        }


        [TestMethod]
        public void ArticleCategoryDeleteTest()
        {
            var service = ServiceProvider.Instance<IArticleCategoryService>.Create;
            ArticleCategoryInfo articleCategory = new ArticleCategoryInfo()
            {
                ParentCategoryId = 0,
                Name = "这是文章分类" + DateTime.Now.ToString("yyyyMMddHHmmss")
            };
            service.AddArticleCategory(articleCategory);

            service.DeleteArticleCategory(articleCategory.Id);
            
            ArticleCategoryInfo newArticleCategory = service.GetArticleCategory(articleCategory.Id);
            Assert.IsNull(newArticleCategory);
        }

         [TestMethod]
        public void TestTime()
        {
             //订单生成算法
            /*
             * 订单号一共12位，由年（1位），随机数（3位），天（2位），月（2位）随机数+当前订单数（4位）
             * 
             * 3位是随机数说明：
             *       会生成一个8为的随机数，取随机数的前3位
             *       
             * 4位是随机数+当前订单数的说明：
             *       这里一共是4位，也就是说一天一个店铺最大支持9999个订单，如果订单数不足4位，就是用随机数补齐。
             *       但是对于超过1万做了处理，最大能支持39999.
             */
            var year = DateTime.Now.Year.ToString().Substring(3,1);
            var _day = DateTime.Now.Day;
            var day = _day < 10 ? "0" + _day.ToString() : _day.ToString();
            var _month = DateTime.Now.Month;
            var month = _month < 10 ? "0" + _month.ToString() : _month.ToString();
            var list = new List<string>();
            string randomStr = "";
            string orderCountStr = "";

            for (int i = 1; i <20000; i++)
            {

                long tick = DateTime.Now.Ticks;
                Random ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
                var ranString = ran.Next(10000000, 99999999).ToString();


                randomStr = ranString.Substring(0, 3);
                orderCountStr = GetOrderCountStr(list.Count(), ranString);
                switch (list.Count() % 3) {
                    case 0: list.Add(string.Format("{0}{1}{2}{3}{4}", year, day, randomStr, month, orderCountStr)); break;
                    case 1: list.Add(string.Format("{0}{1}{2}{3}{4}", day, year, month, orderCountStr, randomStr)); break;
                    case 2: list.Add(string.Format("{0}{1}{2}{3}{4}", month, orderCountStr, year, day, randomStr)); break;

            }


                Thread.Sleep(2);
            }
            var hh = from f in
                         (from x in list
                          group x by x into G
                          select new
                          {
                              count = G.Count(),
                              key = G.Key
                          })
                     where f.count > 1
                     select f;
            var t = hh.ToList();

            Assert.AreEqual(true, t.Count() < 1);
            
        }

         private string GetOrderCountStr(int orderCount, string ranString)
         {
             string orderCountStr = "";
             switch (orderCount.ToString().Length)
             {
                 case 1:
                     orderCountStr = string.Format("000{0}", orderCount);
                     break;
                 case 2:
                     orderCountStr = string.Format("00{0}",  orderCount);
                     break;
                 case 3:
                     orderCountStr = string.Format("0{0}", orderCount);
                     break;
                 case 4:
                     orderCountStr = string.Format("{0}",  orderCount);
                     break;
                 case 5:
                     ranString = orderCount % 2 == 0 ? ranString.Substring(2, 4) : ranString.Substring(4, 4);
                     orderCountStr = string.Format("{0}", GetOrderCountStr(orderCount % 10000, ranString));
                     break;
             }
             return orderCountStr;

         }

    }
}
