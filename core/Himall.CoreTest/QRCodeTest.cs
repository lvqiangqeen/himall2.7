using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Himall.CoreTest
{
    [TestClass]
    public class QRCodeTest
    {
        [TestMethod]
        public void CreatImageQRCodeTest()
        {
            string imagePath = "hishop.png";
            string content = "http://www.qq.com";
            var image = Core.Helper.QRCodeHelper.Create(content, System.Drawing.Image.FromFile(imagePath));
            image.Save("qrcode.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        [TestMethod]
        public void CreatImageQRCodeFromPathTest()
        {
            string imagePath = "hishop.png";
            string content = "http://www.qq.com";
            var image = Core.Helper.QRCodeHelper.Create(content, imagePath);
            image.Save("qrcode.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }


        [TestMethod]
        public void CreatTextQRCodeTest()
        {
            string content = "http://www.qq.com";
            var image = Core.Helper.QRCodeHelper.Create(content);
            image.Save("qrcode.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }

    }
}
