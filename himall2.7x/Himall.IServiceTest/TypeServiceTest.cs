using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Himall.Service;
using Himall.IServices;
using System.Linq;
using Himall.Model;
using System.Text;

namespace Himall.IServiceTest
{
    public class AttrComparerTest : IEqualityComparer<AttributeInfo>
    {
        public bool Equals(AttributeInfo x, AttributeInfo y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id;
        }
        public int GetHashCode(AttributeInfo attr)
        {
            if (Object.ReferenceEquals(attr, null)) return 0;
            int Id = (int)(attr.Id ^ attr.TypeId);
            return Id;
        }
    }
    [TestClass]
    public class TypeServiceTest : ServiceBase
    {
        [TestMethod]
        public void MethodTest()
        {
            List<AttributeInfo> olds = new List<AttributeInfo> {
                new AttributeInfo{ Id=1, TypeId=2, Name="OK"},
                 new AttributeInfo{ Id=4, TypeId=2, Name="OK"},
                 new AttributeInfo{ Id=6, TypeId=2, Name="OK"},
            };

            List<AttributeInfo> news = new List<AttributeInfo> { 
                 new AttributeInfo{ Id=11, TypeId=2, Name="OK"},
                 new AttributeInfo{ Id=1, TypeId=2, Name="OK"},
                 new AttributeInfo{ Id=4, TypeId=2, Name="OK"},
                 new AttributeInfo{ Id=15, TypeId=2, Name="OK"},
            };
            var deleteNewAttr = olds.Except(news, new AttrComparerTest());
            var addNewAttr = news.Except(olds, new AttrComparerTest());

        }

        [TestMethod]
        public void GetBusinessCategoryTest()
        {
            var IShop = Himall.ServiceProvider.Instance<IShopService>.Create;
            var shop = IShop.GetShopGrade(2);
        }

        [TestMethod]
        public void CreateTypeMethodTest()
        {
            var type = new ProductTypeInfo
            {
                IsSupportColor = true,
                IsSupportSize = true,
                IsSupportVersion = false,
                Name = "鞋子",
                SpecificationValueInfo = new List<SpecificationValueInfo> { 
                    new SpecificationValueInfo{ Specification= SpecificationType.Color, Value="红色,蓝色"} ,
                    new SpecificationValueInfo{ Specification= SpecificationType.Size, Value="S,M,XX"}  
                },
                AttributeInfo = new List<AttributeInfo> {
                    new AttributeInfo{ Name="a", IsMulti=true, DisplaySequence=1, AttributeValueInfo=new List<AttributeValueInfo>
                    {
                        new AttributeValueInfo{ Value="aa", DisplaySequence=1},
                        new AttributeValueInfo{ Value="aaa", DisplaySequence=1},
                    }}
                },
                TypeBrandInfo = new List<TypeBrandInfo>
                {
                    new TypeBrandInfo{ BrandId=1},
                    new TypeBrandInfo{ BrandId=11}
                }
            };
            ServiceProvider.Instance<ITypeService>.Create.AddType(type);
        }

        [TestMethod]
        public void DeleteTypeMethodTest()
        {
            ServiceProvider.Instance<ITypeService>.Create.DeleteType(2);
        }

        [TestMethod]
        public void GetTypeMethodTest()
        {
            var auct = ServiceProvider.Instance<ITypeService>.Create.GetType(3);
            Assert.AreEqual(3, auct.Id);

        }

        [TestMethod]
        public void UpdateTypeMethodTest()
        {
            var type = new ProductTypeInfo
            {
                Id = 3,
                IsSupportColor = false,
                IsSupportSize = false,
                IsSupportVersion = false,
                //VersionValue = "4G,8G",
                //ColorValue = "",
                //SizeValue = "",
                Name = "TestType"

            };
            ServiceProvider.Instance<ITypeService>.Create.UpdateType(type);
        }

        [TestMethod]
        public void GetTypesMethodTest()
        {
            try
            {
                var list = ServiceProvider.Instance<ITypeService>.Create.GetTypes("T", 1, 1);
                Assert.AreEqual("T", list.Models.FirstOrDefault().Name);

                //Assert.AreEqual(2, list.Models.ToList().Count());
            }
            catch (System.Reflection.TargetInvocationException )
            {

            }
        }


        [TestMethod]
        public void TestM()
        {
            var time = GetStartDayOfWeeks(2014, 12);
            Console.WriteLine(time);
        }

        public string GetStartDayOfWeeks(int year, int month)
        {
            if (year < 1600 || year > 9999)
            {
                return "";
            }
            if (month < 0 || month > 12)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            for (int index = 1; index < 5; index++)
            {
                DateTime startMonth = new DateTime(year, month, 1);  //该月第一天  
                int dayOfWeek = 7;
                if (Convert.ToInt32(startMonth.DayOfWeek.ToString("d")) > 0)
                    dayOfWeek = Convert.ToInt32(startMonth.DayOfWeek.ToString("d"));  //该月第一天为星期几  
                DateTime startWeek = startMonth.AddDays(1 - dayOfWeek);  //该月第一周开始日期  
                //DateTime startDayOfWeeks = startWeek.AddDays((index - 1) * 7);  //index周的起始日期  
                DateTime startDayOfWeeks = startWeek.AddDays(index * 7);  //index周的起始日期  
                if ((startDayOfWeeks - startMonth.AddMonths(1)).Days > 0)  //startDayOfWeeks不在该月范围内  
                {
                    return "";
                }
                sb.Append(startDayOfWeeks.ToString("yyyy-MM-dd"));
                sb.Append(" ~ ");
                sb.Append(startDayOfWeeks.AddDays(6).ToString("yyyy-MM-dd"));
                sb.Append(Environment.NewLine);

            }
            return sb.ToString();
        }

    }
}
