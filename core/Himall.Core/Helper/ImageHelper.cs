using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;

namespace Himall.Core.Helper
{
    public class ImageHelper
    {

        #region  水印,缩略图

        //是否已经加载了JPEG编码解码器
        private static bool _isloadjpegcodec = false;
        //当前系统安装的JPEG编码解码器
        private static ImageCodecInfo _jpegcodec = null;


        /// <summary>
        /// 获得当前系统安装的JPEG编码解码器
        /// </summary>
        /// <returns></returns>
        public static ImageCodecInfo GetJPEGCodec()
        {
            if (_isloadjpegcodec == true)
                return _jpegcodec;

            ImageCodecInfo[] codecsList = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecsList)
            {
                if (codec.MimeType.IndexOf("jpeg") > -1)
                {
                    _jpegcodec = codec;
                    break;
                }

            }
            _isloadjpegcodec = true;
            return _jpegcodec;
        }

        /// <summary>
        /// 为指定的图片生成缩略图
        /// </summary>
        /// <param name="sourceFilename">图片源文件的完整文件名</param>
        /// <param name="destFilename">缩略图文件的完整文件名</param>
        /// <param name="width">缩略图宽度(px)</param>
        /// <param name="height">缩略图高度(px)</param>
        public static void CreateThumbnail(string sourceFilename, string destFilename, int width, int height)
        {
            Image imageFrom = Image.FromFile(sourceFilename);
            if (imageFrom.Width <= width && imageFrom.Height <= height)
            {
                // 如果源图的大小没有超过缩略图指定的大小，则直接把源图复制到目标文件
                File.Copy(sourceFilename, destFilename, true);
                imageFrom.Dispose();

                return;
            }

            // 源图宽度及高度 
            int imageFromWidth = imageFrom.Width;
            int imageFromHeight = imageFrom.Height;

            float scale = height / (float)imageFromHeight;

            if ((width / (float)imageFromWidth) < scale)
                scale = width / (float)imageFromWidth;

            width = (int)(imageFromWidth * scale);
            height = (int)(imageFromHeight * scale);

            // 创建画布 
            Image bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);

            g.Clear(Color.White);

            // 指定高质量的双三次插值法。执行预筛选以确保高质量的收缩。此模式可产生质量最高的转换图像。 
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            // 指定高质量、低速度呈现。 
            g.SmoothingMode = SmoothingMode.HighQuality;

            // 在指定位置并且按指定大小绘制指定的 Image 的指定部分。 
            g.DrawImage(imageFrom, new Rectangle(0, 0, width, height), new Rectangle(0, 0, imageFromWidth, imageFromHeight), GraphicsUnit.Pixel);

            //配置JPEG压缩引擎
            EncoderParameters encoderParams = new EncoderParameters();
            EncoderParameter encoderParam = new EncoderParameter(Encoder.Quality, 100L);
            encoderParams.Param[0] = encoderParam;

            ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo jpegICI = null;

            for (int x = 0; x < arrayICI.Length; x++)
            {
                if (arrayICI[x].FormatDescription.Equals("JPEG"))
                {
                    jpegICI = arrayICI[x];
                    break;
                }
            }

            bmp.Save(destFilename, jpegICI, encoderParams);

            //显示释放资源 
            encoderParams.Dispose();
            encoderParam.Dispose();
            imageFrom.Dispose();
            bmp.Dispose();
            g.Dispose();
        }
        ///// <summary>
        ///// 生成缩略图
        ///// </summary>
        ///// <param name="imagePath">图片路径</param>
        ///// <param name="thumbPath">缩略图路径</param>
        ///// <param name="width">缩略图宽度</param>
        ///// <param name="height">缩略图高度</param>
        ///// <param name="mode">生成缩略图的方式</param>   
        //public static void GenerateThumb(string imagePath, string thumbPath, int width, int height, string mode)
        //{
        //    Image image = Image.FromFile(imagePath);

        //    string extension = imagePath.Substring(imagePath.LastIndexOf(".")).ToLower();
        //    ImageFormat imageFormat = null;
        //    switch (extension)
        //    {
        //        case ".jpg":
        //        case ".jpeg":
        //            imageFormat = ImageFormat.Jpeg;
        //            break;
        //        case ".bmp":
        //            imageFormat = ImageFormat.Bmp;
        //            break;
        //        case ".png":
        //            imageFormat = ImageFormat.Png;
        //            break;
        //        case ".gif":
        //            imageFormat = ImageFormat.Gif;
        //            break;
        //        default:
        //            imageFormat = ImageFormat.Jpeg;
        //            break;
        //    }

        //    int toWidth = width > 0 ? width : image.Width;
        //    int toHeight = height > 0 ? height : image.Height;

        //    int x = 0;
        //    int y = 0;
        //    int ow = image.Width;
        //    int oh = image.Height;

        //    switch (mode)
        //    {
        //        case "HW"://指定高宽缩放（可能变形）           
        //            break;
        //        case "W"://指定宽，高按比例             
        //            toHeight = image.Height * width / image.Width;
        //            break;
        //        case "H"://指定高，宽按比例
        //            toWidth = image.Width * height / image.Height;
        //            break;
        //        case "Cut"://指定高宽裁减（不变形）           
        //            if ((double)image.Width / (double)image.Height > (double)toWidth / (double)toHeight)
        //            {
        //                oh = image.Height;
        //                ow = image.Height * toWidth / toHeight;
        //                y = 0;
        //                x = (image.Width - ow) / 2;
        //            }
        //            else
        //            {
        //                ow = image.Width;
        //                oh = image.Width * height / toWidth;
        //                x = 0;
        //                y = (image.Height - oh) / 2;
        //            }
        //            break;
        //        default:
        //            break;
        //    }

        //    //新建一个bmp
        //    Image bitmap = new Bitmap(toWidth, toHeight);

        //    //新建一个画板
        //    Graphics g = Graphics.FromImage(bitmap);

        //    //设置高质量插值法
        //    g.InterpolationMode = InterpolationMode.High;

        //    //设置高质量,低速度呈现平滑程度
        //    g.SmoothingMode = SmoothingMode.HighQuality;

        //    //清空画布并以透明背景色填充
        //    g.Clear(Color.Transparent);

        //    //在指定位置并且按指定大小绘制原图片的指定部分
        //    g.DrawImage(image,
        //                new Rectangle(0, 0, toWidth, toHeight),
        //                new Rectangle(x, y, ow, oh),
        //                GraphicsUnit.Pixel);

        //    try
        //    {
        //        bitmap.Save(thumbPath, imageFormat);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        if (g != null)
        //            g.Dispose();
        //        if (bitmap != null)
        //            bitmap.Dispose();
        //        if (image != null)
        //            image.Dispose();
        //    }
        //}

        /// <summary>
        /// 生成图片水印
        /// </summary>
        /// <param name="originalPath">源图路径</param>
        /// <param name="watermarkPath">水印图片路径</param>
        /// <param name="targetPath">保存路径</param>
        /// <param name="position">位置</param>
        /// <param name="opacity">透明度</param>
        /// <param name="quality">质量</param>
        public static void GenerateImageWatermark(string originalPath, string watermarkPath, string targetPath, int position, int opacity, int quality)
        {
            Image originalImage = null;
            Image watermarkImage = null;
            //图片属性
            ImageAttributes attributes = null;
            //画板
            Graphics g = null;
            try
            {

                originalImage = Image.FromFile(originalPath);
                watermarkImage = new Bitmap(watermarkPath);

                if (watermarkImage.Height >= originalImage.Height || watermarkImage.Width >= originalImage.Width)
                {
                    originalImage.Save(targetPath);
                    return;
                }

                if (quality < 0 || quality > 100)
                    quality = 80;

                //水印透明度
                float iii;
                if (opacity > 0 && opacity <= 10)
                    iii = (float)(opacity / 10.0F);
                else
                    iii = 0.5F;

                //水印位置
                int x = 0;
                int y = 0;
                switch (position)
                {
                    case 1:
                        x = (int)(originalImage.Width * (float).01);
                        y = (int)(originalImage.Height * (float).01);
                        break;
                    case 2:
                        x = (int)((originalImage.Width * (float).50) - (watermarkImage.Width / 2));
                        y = (int)(originalImage.Height * (float).01);
                        break;
                    case 3:
                        x = (int)((originalImage.Width * (float).99) - (watermarkImage.Width));
                        y = (int)(originalImage.Height * (float).01);
                        break;
                    case 4:
                        x = (int)(originalImage.Width * (float).01);
                        y = (int)((originalImage.Height * (float).50) - (watermarkImage.Height / 2));
                        break;
                    case 5:
                        x = (int)((originalImage.Width * (float).50) - (watermarkImage.Width / 2));
                        y = (int)((originalImage.Height * (float).50) - (watermarkImage.Height / 2));
                        break;
                    case 6:
                        x = (int)((originalImage.Width * (float).99) - (watermarkImage.Width));
                        y = (int)((originalImage.Height * (float).50) - (watermarkImage.Height / 2));
                        break;
                    case 7:
                        x = (int)(originalImage.Width * (float).01);
                        y = (int)((originalImage.Height * (float).99) - watermarkImage.Height);
                        break;
                    case 8:
                        x = (int)((originalImage.Width * (float).50) - (watermarkImage.Width / 2));
                        y = (int)((originalImage.Height * (float).99) - watermarkImage.Height);
                        break;
                    case 9:
                        x = (int)((originalImage.Width * (float).99) - (watermarkImage.Width));
                        y = (int)((originalImage.Height * (float).99) - watermarkImage.Height);
                        break;
                }

                //颜色映射表
                ColorMap colorMap = new ColorMap();
                colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
                colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
                ColorMap[] newColorMap = { colorMap };

                //颜色变换矩阵,iii是设置透明度的范围0到1中的单精度类型
                float[][] newColorMatrix ={ 
                                            new float[] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f},
                                            new float[] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f},
                                            new float[] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f},
                                            new float[] {0.0f,  0.0f,  0.0f,  iii, 0.0f},
                                            new float[] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}
                                           };
                //定义一个 5 x 5 矩阵
                ColorMatrix matrix = new ColorMatrix(newColorMatrix);

                //图片属性
                attributes = new ImageAttributes();
                attributes.SetRemapTable(newColorMap, ColorAdjustType.Bitmap);
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                //画板
                g = Graphics.FromImage(originalImage);
                //绘制水印
                g.DrawImage(watermarkImage, new Rectangle(x, y, watermarkImage.Width, watermarkImage.Height), 0, 0, watermarkImage.Width, watermarkImage.Height, GraphicsUnit.Pixel, attributes);
                //保存图片
                EncoderParameters encoderParams = new EncoderParameters();
                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, new long[] { quality });
                if (GetJPEGCodec() != null)
                    originalImage.Save(targetPath, _jpegcodec, encoderParams);
                else
                    originalImage.Save(targetPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (g != null)
                    g.Dispose();
                if (attributes != null)
                    attributes.Dispose();
                if (watermarkImage != null)
                    watermarkImage.Dispose();
                if (originalImage != null)
                    originalImage.Dispose();
            }
        }

        /// <summary>
        /// 生成文字水印
        /// </summary>
        /// <param name="originalPath">源图路径</param>
        /// <param name="targetPath">保存路径</param>
        /// <param name="text">水印文字</param>
        /// <param name="textSize">文字大小</param>
        /// <param name="textFont">文字字体</param>
        /// <param name="position">位置</param>
        /// <param name="quality">质量</param>
        public static void GenerateTextWatermark(string originalPath, string targetPath, string text, int textSize, string textFont, int position, int quality)
        {
            Image originalImage = null;
            //画板
            Graphics g = null;
            try
            {
                originalImage = Image.FromFile(originalPath);
                //画板
                g = Graphics.FromImage(originalImage);
                if (quality < 0 || quality > 100)
                    quality = 80;

                Font font = new Font(textFont, textSize, FontStyle.Regular, GraphicsUnit.Pixel);
                SizeF sizePair = g.MeasureString(text, font);

                float x = 0;
                float y = 0;

                switch (position)
                {
                    case 1:
                        x = (float)originalImage.Width * (float).01;
                        y = (float)originalImage.Height * (float).01;
                        break;
                    case 2:
                        x = ((float)originalImage.Width * (float).50) - (sizePair.Width / 2);
                        y = (float)originalImage.Height * (float).01;
                        break;
                    case 3:
                        x = ((float)originalImage.Width * (float).99) - sizePair.Width;
                        y = (float)originalImage.Height * (float).01;
                        break;
                    case 4:
                        x = (float)originalImage.Width * (float).01;
                        y = ((float)originalImage.Height * (float).50) - (sizePair.Height / 2);
                        break;
                    case 5:
                        x = ((float)originalImage.Width * (float).50) - (sizePair.Width / 2);
                        y = ((float)originalImage.Height * (float).50) - (sizePair.Height / 2);
                        break;
                    case 6:
                        x = ((float)originalImage.Width * (float).99) - sizePair.Width;
                        y = ((float)originalImage.Height * (float).50) - (sizePair.Height / 2);
                        break;
                    case 7:
                        x = (float)originalImage.Width * (float).01;
                        y = ((float)originalImage.Height * (float).99) - sizePair.Height;
                        break;
                    case 8:
                        x = ((float)originalImage.Width * (float).50) - (sizePair.Width / 2);
                        y = ((float)originalImage.Height * (float).99) - sizePair.Height;
                        break;
                    case 9:
                        x = ((float)originalImage.Width * (float).99) - sizePair.Width;
                        y = ((float)originalImage.Height * (float).99) - sizePair.Height;
                        break;
                }

                g.DrawString(text, font, new SolidBrush(Color.White), x + 1, y + 1);
                g.DrawString(text, font, new SolidBrush(Color.Black), x, y);

                //保存图片
                EncoderParameters encoderParams = new EncoderParameters();
                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, new long[] { quality });
                if (GetJPEGCodec() != null)
                    originalImage.Save(targetPath, _jpegcodec, encoderParams);
                else
                    originalImage.Save(targetPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (g != null)
                    g.Dispose();
                if (originalImage != null)
                    originalImage.Dispose();
            }
        }

        #endregion

        #region 生成验证码

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="checkCode"></param>
        /// <returns></returns>
        public static MemoryStream GenerateCheckCode(out string checkCode)
        {
            checkCode = string.Empty;
            //颜色列表，用于验证码、噪线、噪点 
            Color color = ColorTranslator.FromHtml("#1AE61A");

            //验证码的字符集，去掉了一些容易混淆的字符 
            char[] character = { '2', '3', '4', '5', '6', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'W', 'X', 'Y' };
            Random rnd = new Random();
            //生成验证码字符串 
            for (int i = 0; i < 4; i++)
            {
                checkCode += character[rnd.Next(character.Length)];
            }

            int iwidth = 85;//(int)(checkCode.Length * 11.5);
            System.Drawing.Bitmap image = new System.Drawing.Bitmap(iwidth, 30);
            Graphics g = Graphics.FromImage(image);
            Random random = new Random(System.DateTime.Now.Millisecond);

            Brush b = new System.Drawing.SolidBrush(Color.FromArgb(4683611));

            g.Clear(ColorTranslator.FromHtml("#EBFDDF"));
            using (StringFormat format = new StringFormat())
            {

                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                format.FormatFlags = StringFormatFlags.NoWrap;
                Matrix matrix = new Matrix();
                float offsetx = -25, offsety = 0;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                for (int i = 0; i < checkCode.Length; i++)
                {
                    int fsize = random.Next(20, 24);
                    Font f = CreateFont(IOHelper.GetMapPath("/fonts/checkCode.ttf"), fsize, FontStyle.Bold, GraphicsUnit.Point, 0);
                 //   Font f = new Font("黑体", fsize);
                    SizeF size = g.MeasureString(checkCode[i].ToString(), f);
                    matrix.RotateAt(random.Next(-15, 10), new PointF(offsetx + size.Width / 2, offsety + size.Height / 2));
                    g.Transform = matrix;

                    g.DrawString(
                                 checkCode[i].ToString(),
                                 f,
                                 Brushes.Green,
                                 new RectangleF(
                                 offsetx,
                                 offsety,
                                 image.Width,
                                 image.Height),
                                format);
                    //g.DrawString(checkCode, f, b, 3, 3);
                    offsetx += size.Width * 3 / 5;
                    offsety += -0;
                    g.RotateTransform(0);
                    matrix.Reset();
                    f.Dispose();
                }
            }


            Pen blackPen = new Pen(Color.Black, 0);
            //将验证码图片写入内存流，并将其以 "image/Png" 格式输出 
            MemoryStream ms = new MemoryStream();
            try
            {
                image.Save(ms, ImageFormat.Png);
                return ms;
            }
            finally
            {
                //显式释放资源 
                image.Dispose();
                g.Dispose();
            }
        }


        public static Font CreateFont(string fontFile, float fontSize, FontStyle fontStyle, GraphicsUnit graphicsUnit, byte gdiCharSet)
        {
            PrivateFontCollection  pfc = new PrivateFontCollection();
            pfc.AddFontFile(fontFile);
            return new Font(pfc.Families[0], fontSize, fontStyle, graphicsUnit, gdiCharSet);
        }

        #endregion

        #region 图片格式转换

        /// <summary>
        /// 图片格式转换
        /// </summary>
        /// <param name="originalImagePath">原始图片地址</param>
        /// <param name="newFormatImagePath">新格式图片地址</param>
        /// <param name="fortmat">待转换的格式</param>
        public static void TranserImageFormat(string originalImagePath, string newFormatImagePath, ImageFormat fortmat)
        {
            Bitmap myBitmap = new Bitmap(originalImagePath);
            myBitmap.Save(newFormatImagePath, ImageFormat.Jpeg);
        }

        #endregion

    }
}
