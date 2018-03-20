using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;

namespace Himall.CoreTest
{
    [TestClass]
    public class CheckCodeTest
    {
        [TestMethod]
        public void Core_CheckCode_CreateTest()
        {
            string checkCode;
            using (System.IO.MemoryStream ms = Himall.Core.Helper.ImageHelper.GenerateCheckCode(out checkCode))
            {
                Bitmap bmp = new Bitmap(ms);
                bmp.Save("checkCode.png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
