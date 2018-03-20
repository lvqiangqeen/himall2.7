using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Himall.ServiceProvider;
using Himall.IServices;
using Himall.IServices.QueryModel;
using System.Linq;

namespace Himall.IServiceTest
{
    [TestClass]
    public class ProductConsultationTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var IConsultation = Instance<IConsultationService>.Create;
            var query = new ConsultationQuery { PageNo = 1, PageSize = 3, KeyWords = "", ShopID = 1, IsReply = true };
            var result = IConsultation.GetConsultations(query).Models.Select(A=>new { 
            productNAME=A.ProductInfo.ProductName,
            ReplyText=A.ReplyContent  
            });
            //Assert.Equals( "小米1");
        }
    }
}
