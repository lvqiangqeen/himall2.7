using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Himall.Core.Helper;
using System.IO;

namespace Himall.CoreTest
{
    [TestClass]
    public class IOHelperTest
    {
        [TestMethod]
        public void Core_GetCurrentDirectoryTest()
        {

           string dir = IOHelper.GetMapPath("Images");
           /// 
          //  var length= GetDirectoryLength(dir);

        }

      
    }
}
