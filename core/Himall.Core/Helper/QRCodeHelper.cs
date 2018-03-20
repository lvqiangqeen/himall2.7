using com.google.zxing;
using com.google.zxing.common;
using System;
using System.Collections;
using System.Drawing;
using System.Net;

namespace Himall.Core.Helper
{
    public class QRCodeHelper
    {

        public static Bitmap Create(string content)
        {
            //构造二维码写码器
            MultiFormatWriter mutiWriter = new com.google.zxing.MultiFormatWriter();
            ByteMatrix bm = mutiWriter.encode(content, com.google.zxing.BarcodeFormat.QR_CODE, 300, 300);
            Bitmap img = bm.ToBitmap();
            return img;
        }

        /// <summary>
        /// 生成二维码，并显示logo
        /// </summary>
        /// <param name="content"></param>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public static Bitmap Create(string content, string imagePath)
        {
            if (imagePath.Contains("http://") || imagePath.Contains("https://"))
            {
                Uri myUri = new Uri(imagePath);
                WebRequest webRequest = WebRequest.Create(myUri);
                WebResponse webResponse = webRequest.GetResponse();
                var image = new Bitmap(webResponse.GetResponseStream());
                Bitmap qrcode = Create(content, image);
                image.Dispose();
                return qrcode;
            }
            else
            {
                if (!imagePath.Contains(":"))
                    imagePath = Core.Helper.IOHelper.GetMapPath(imagePath);
                var image = Image.FromFile(imagePath);

                Bitmap qrcode = Create(content, image);
                image.Dispose();
                return qrcode;
            }

        }

        public static Bitmap Create(string content, Image centralImage)
        {
            //构造二维码写码器
            MultiFormatWriter mutiWriter = new com.google.zxing.MultiFormatWriter();
            Hashtable hint = new Hashtable();
            hint.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
            hint.Add(EncodeHintType.ERROR_CORRECTION, com.google.zxing.qrcode.decoder.ErrorCorrectionLevel.H);
            //生成二维码
            ByteMatrix bm = mutiWriter.encode(content, com.google.zxing.BarcodeFormat.QR_CODE, 300, 300, hint);
            Bitmap img = bm.ToBitmap();

            //要插入到二维码中的图片
            Image middlImg = centralImage;
            //获取二维码实际尺寸（去掉二维码两边空白后的实际尺寸）
         //  System.Drawing.Size realSize = mutiWriter.GetEncodeSize(content, com.google.zxing.BarcodeFormat.QR_CODE, 300, 300);
            var realSize = mutiWriter.encode(content, com.google.zxing.BarcodeFormat.QR_CODE, 300, 300);
            //计算插入图片的大小和位置
            int middleImgW = Math.Min((int)(realSize.Width / 3.5), middlImg.Width);
            int middleImgH = Math.Min((int)(realSize.Height / 3.5), middlImg.Height);
            int middleImgL = (img.Width - middleImgW) / 2;
            int middleImgT = (img.Height - middleImgH) / 2;

            //将img转换成bmp格式，否则后面无法创建 Graphics对象
            Bitmap bmpimg = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmpimg))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.DrawImage(img, 0, 0);
            }

            //在二维码中插入图片
            System.Drawing.Graphics MyGraphic = System.Drawing.Graphics.FromImage(bmpimg);
            //白底
            MyGraphic.FillRectangle(Brushes.White, middleImgL, middleImgT, middleImgW, middleImgH);
            MyGraphic.DrawImage(middlImg, middleImgL, middleImgT, middleImgW, middleImgH);

            return bmpimg;
        }
    }
}
