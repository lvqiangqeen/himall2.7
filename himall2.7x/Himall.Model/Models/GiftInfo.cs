using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Himall.Core;
using Himall.CommonModel;

namespace Himall.Model
{
    public partial class GiftInfo
    {
        /// <summary>
        /// 礼品销售状态
        /// </summary>
        public enum GiftSalesStatus
        {
            /// <summary>
            /// 删除
            /// </summary>
            [Description("删除")]
            IsDelete=-1,
            /// <summary>
            /// 下架
            /// </summary>
            [Description("下架")]
            OffShelves=0,
            /// <summary>
            /// 正常
            /// </summary>
            [Description("正常")]
            Normal = 1,
            /// <summary>
            /// 过期
            /// </summary>
            [Description("过期")]
            HasExpired=2
        }
        /// <summary>
        /// 等级需求
        /// </summary>
        [NotMapped]
        public string NeedGradeName { get; set; }
        /// <summary>
        /// 等级积分要求
        /// </summary>
        public int GradeIntegral { get; set; }
        /// <summary>
        /// 礼品默认显示图片
        /// </summary>
        public string DefaultShowImage { get; set; }

        /// <summary>
        /// 销量总数
        /// <para>实际销量+虚拟销量</para>
        /// </summary>
        public long SumSales
        {
            get
            {
                return VirtualSales + RealSales;
            }
        }

        /// <summary>
        /// 获取图片地址
        /// </summary>
        /// <param name="imageIndex">图片序号</param>
        /// <param name="imageSize">图片尺寸</param>
        /// <returns></returns>
        public string GetImage(ImageSize imageSize, int imageIndex = 1)
        {
            return Core.HimallIO.GetProductSizeImage(ImagePath, imageIndex, (int)imageSize);
        }

        /// <summary>
        /// 图片路径
        /// </summary>
        [NotMapped]
        public string ShowImagePath
        {
            get { return ImageServerUrl + ImagePath; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    ImagePath = value.Replace(ImageServerUrl, "");
                else
                    ImagePath = value;
            }
        }

        /// <summary>
        /// 获取当前销售状态
        /// </summary>
        [NotMapped]
        public GiftInfo.GiftSalesStatus GetSalesStatus
        {
            get
            {
                GiftInfo.GiftSalesStatus result = SalesStatus;
                if (result == GiftInfo.GiftSalesStatus.Normal)
                {
                    if (this.EndDate < DateTime.Now)
                    {
                        result = GiftInfo.GiftSalesStatus.HasExpired;
                    }
                }
                return result;
            }
        }
        /// <summary>
        /// 显示限兑数量
        /// </summary>
        [NotMapped]
        public string ShowLimtQuantity
        {
            get
            {
                string result = "";
                if (this.LimtQuantity == 0)
                {
                    result = "不限";
                }
                else
                {
                    result = this.LimtQuantity.ToString();
                }
                return result;
            }
        }
    }
}
