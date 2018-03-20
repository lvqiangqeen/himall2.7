using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Himall.ServiceProvider;
using Himall.IServices;
using Himall.Model;
using System.Collections.Generic;
using System.Linq;

namespace Himall.IServiceTest
{
    [TestClass]
    public class ProductCategoryServiceUnitTest
    {
        [TestMethod]
        public void AddCategoryTest()
        {
            var ICategory = Instance<ICategoryService>.Create;
            var id = ICategory.GetMaxCategoryId();
            var model = new CategoryInfo
            {
                Depth = 1,
                Path = "1",
                Name = "TEst",
                ParentCategoryId = 0,
                DisplaySequence = 1,
                HasChildren = false,
                Icon = "",
                Meta_Description = "D",
                Meta_Keywords = "K",
                Meta_Title = "T",
                RewriteName = "RName",
                TypeId = 1
            };
            ICategory.AddCategory(model);
            model.Id = id;
            var expect = model;
            expect.ProductTypeInfo = null;
            var actual = ICategory.GetCategory(id);
            actual.ProductTypeInfo = null;

            Assert.Equals(expect, actual);
            Assert.AreNotEqual(null, actual);


            actual = ICategory.GetCategory(id - 1);
            actual.ProductTypeInfo = null;
            Assert.AreEqual(expect, actual);
            Assert.AreNotEqual(null, actual);

        }


        [TestMethod]
        public void GetFirstLevelCategoryTest()
        {


            var ICategory = Instance<ICategoryService>.Create;
            var categories = ICategory.GetMainCategory();
            foreach (var item in categories)
            {
                Assert.AreEqual(item.ParentCategoryId, 0);
                Assert.AreEqual(item.Depth, 1);
                Assert.AreEqual(item.HasChildren, true);
            }
        }
        

        [TestMethod]
        public void GetNonLeafCategoryListTest()
        {
            var ICategory = Instance<ICategoryService>.Create;
            var categories = ICategory.GetFirstAndSecondLevelCategories();

            //这里测试比较复杂，后面补上
        }

        [TestMethod]
        public void GetCategoryByParentIdTest()
        {


            //  假设存在如下数据
            //
            //
            //
            //{  
            //    Id=1,sub{ Id=2, Id=12, {Id=23,sub{Id=19}}}
            //    Id=2,sub{ Id=45 }
            //};


            var ICategory = Instance<ICategoryService>.Create;
            //id==1 的情况
            var categories = ICategory.GetCategoryByParentId(1);
            Assert.AreNotEqual(null, categories);

            Assert.AreEqual(3, categories.Count());

            var id = 23;
            Assert.AreEqual(id, categories.FirstOrDefault(c => c.Id.Equals(id)).Id);
            //后续
        }

        [TestMethod]
        public void GetCategoryByIdTest()
        {
            //  假设存在如下数据
            //
            //
            //
            //{
            //    Id=12,
            //    Depth = 1,
            //    Path = "1",
            //    Name = "TEst",
            //    ParentCategoryId = 0,
            //    DisplaySequence = 1,
            //    HasChildren = true,
            //    Icon = "",
            //    Meta_Description = "D",
            //    Meta_Keywords = "K",
            //    Meta_Title = "T",
            //    RewriteName = "RName",
            //    TypeId = 1
            //};
            var ICategory = Instance<ICategoryService>.Create;
            var category = ICategory.GetCategory(12);

            Assert.AreNotEqual(null, category);
            Assert.AreEqual(12, category.Id);
            Assert.AreEqual("RName", category.RewriteName);
            Assert.AreEqual(1, category.Depth);
            Assert.AreEqual(0, category.ParentCategoryId);
            Assert.AreEqual(1, category.DisplaySequence);

        }

        [TestMethod]
        public void UpdateCategoryNameTest()
        {
            //  假设存在如下数据
            //
            //
            //
            //{
            //    Id=12,
            //    Depth = 1,
            //    Path = "1",
            //    Name = "TEst",
            //    ParentCategoryId = 0,
            //    DisplaySequence = 1,
            //    HasChildren = true,
            //    Icon = "",
            //    Meta_Description = "D",
            //    Meta_Keywords = "K",
            //    Meta_Title = "T",
            //    RewriteName = "RName",
            //    TypeId = 1
            //};
            var ICategory = Instance<ICategoryService>.Create;
            ICategory.UpdateCategoryName(12, "TestName");

            var actual = ICategory.GetCategory(12);
            Assert.AreEqual("TestName", actual.Name);


        }

        [TestMethod]
        public void UpdateCategoryDisplaySequenceTest()
        {
            //  假设存在如下数据
            //
            //
            //
            //{
            //    Id=12,
            //    Depth = 1,
            //    Path = "1",
            //    Name = "TEst",
            //    ParentCategoryId = 0,
            //    DisplaySequence = 1,
            //    HasChildren = true,
            //    Icon = "",
            //    Meta_Description = "D",
            //    Meta_Keywords = "K",
            //    Meta_Title = "T",
            //    RewriteName = "RName",
            //    TypeId = 1
            //};
            var ICategory = Instance<ICategoryService>.Create;
            var orderOK = 8;
            ICategory.UpdateCategoryDisplaySequence(12, orderOK);

            var actual = ICategory.GetCategory(12);
            Assert.AreEqual(orderOK, actual.DisplaySequence);

            var orderError = -2;
            ICategory.UpdateCategoryDisplaySequence(12, orderError);
            Assert.AreEqual(orderError, actual.DisplaySequence);


        }

        [TestMethod]
        public void DeleteCategoryByIdTest()
        {
            //  假设存在如下数据
            //
            //
            //
            //{  
            //    Id=1,sub{ Id=24, Id=12, {Id=23,sub{Id=19}}}
            //    Id=2,sub{ Id=45 }
            //};

            //string msg="";
            var ICategory = Instance<ICategoryService>.Create;
            ICategory.DeleteCategory(1);

            Assert.AreEqual(null, ICategory.GetCategory(24));
            Assert.AreEqual(null, ICategory.GetCategory(12));
            Assert.AreEqual(null, ICategory.GetCategory(23));



        }
    }
}
