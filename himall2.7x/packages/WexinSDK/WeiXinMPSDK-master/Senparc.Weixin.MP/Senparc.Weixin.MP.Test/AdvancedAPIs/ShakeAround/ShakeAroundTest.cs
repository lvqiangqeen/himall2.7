using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.Weixin.MP.AdvancedAPIs.ShakeAround;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Test.CommonAPIs;

namespace Senparc.Weixin.MP.Test.AdvancedAPIs
{
    [TestClass]
    public class ShakeAroundTest : CommonApiTest
    {
        [TestMethod]
        public void DeviceApplyTest()
        {
            var accessToken = AccessTokenContainer.GetToken( _appId );

            var result = ShakeAroundApi.DeviceApply( accessToken , 1 , "测试" , "测试" );
            Assert.IsNotNull( result );
            Assert.AreEqual( result.errcode , ReturnCode.请求成功 );
        }

        [TestMethod]
        public void DeviceListTest()
        {
            var accessToken = AccessTokenContainer.GetToken( _appId );

            var result = ShakeAroundApi.SearchDeviceByRange( accessToken , 0 , 50 );
            Assert.IsNotNull( result );
            Assert.AreEqual( result.errcode , ReturnCode.请求成功 );
        }

        [TestMethod]
        public void DeviceByIdTest()
        {
            var accessToken = AccessTokenContainer.GetToken( _appId );

            DeviceApply_Data_Device_Identifiers d = new DeviceApply_Data_Device_Identifiers();
            d.device_id = 1015959;
            var result = ShakeAroundApi.SearchDeviceById( accessToken , new List<DeviceApply_Data_Device_Identifiers>
            {
                d
            } );
            Assert.IsNotNull( result );
            Assert.AreEqual( result.errcode , ReturnCode.请求成功 );
        }


        [TestMethod]
        public void UploadImageTest()
        {
            var accessToken = AccessTokenContainer.GetToken( _appId );

            string file = @"E:\测试.jpg";

            var result = ShakeAroundApi.UploadImage( accessToken , file );
            Assert.IsNotNull( result );
            Assert.AreEqual( result.errcode , ReturnCode.请求成功 );
        }

        [TestMethod]
        public void GetPagesTest()
        {
            var accessToken = AccessTokenContainer.GetToken( _appId );
             
            var result = ShakeAroundApi.SearchPagesByRange( accessToken , 0 , 49 );
            Assert.IsNotNull( result );
            Assert.AreEqual( result.errcode , ReturnCode.请求成功 );
        }


    }
}
