
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using Himall.Core;
using Himall.CommonModel;

namespace Himall.Model
{
    public partial class ProductInfo
    {

        /// <summary>
        /// 销售状态
        /// </summary>
        public enum ProductSaleStatus
        {
            /// <summary>
            /// 原始状态
            /// <para>此状态不可入库，需要取出原数据的销售状态重新补充数据</para>
            /// </summary>
            [Description("原始状态")]
            RawState = 0,
            /// <summary>
            /// 出售中
            /// </summary>
            [Description("出售中")]
            OnSale = 1,

            /// <summary>
            /// 仓库中
            /// </summary>
            [Description("仓库中")]
            InStock = 2,
            /// <summary>
            /// 草稿箱
            /// </summary>
            [Description("草稿箱")]
            InDraft = 3
        }

        /// <summary>
        /// 审核状态
        /// </summary>
        public enum ProductAuditStatus
        {
            /// <summary>
            /// 待审核
            /// </summary>
            [Description("待审核")]
            WaitForAuditing = 1,

            /// <summary>
            /// 销售中
            /// </summary>
            [Description("销售中")]
            Audited,

            /// <summary>
            /// 未通过(审核失败)
            /// </summary>
            [Description("未通过")]
            AuditFailed,

            /// <summary>
            /// 违规下架
            /// </summary>
            [Description("违规下架")]
            InfractionSaleOff,

            /// <summary>
            /// 未审核
            /// </summary>
            [Description("未审核")]
            UnAudit
        }

        /// <summary>
        /// 修改状态
        /// </summary>
        public enum ProductEditStatus
        {
            /// <summary>
            /// 正常
            /// <para>修改已生效</para>
            /// </summary>
            [Description("正常")]
            Normal = 0,

            /// <summary>
            /// 已修改
            /// </summary>
            [Description("已修改")]
            Edited = 1,

            /// <summary>
            /// 待审核
            /// </summary>
            [Description("待审核")]
            PendingAudit = 2,

            /// <summary>
            /// 已修改待审核
            /// <para>已修改+待审核</para>
            /// </summary>
            [Description("已修改待审核")]
            EditedAndPending = 3,
            /// <summary>
            /// 强制待审核
            /// <para>免审上架也需要审核</para>
            /// </summary>
            [Description("强制待审核")]
            CompelPendingAudit = 4,
            /// <summary>
            /// 强制待审已修改
            /// <para>免审上架也需要审核</para>
            /// </summary>
            [Description("强制待审已修改")]
            CompelPendingHasEdited = 5,

        }

        

        public int ConcernedCount { get; set; }

        /// <summary>
        /// 获取图片地址 YZY修改获取不同尺寸图片的方法
        /// </summary>
        /// <param name="imageIndex">图片序号</param>
        /// <param name="imageSize">图片尺寸</param>
        /// <returns></returns>
        public string GetImage(ImageSize imageSize, int imageIndex = 1)
        {
           // return string.Format(ImagePath + "/{0}_{1}.png", imageIndex, (int)imageSize);
            return Core.HimallIO.GetProductSizeImage(imagePath, imageIndex, (int)imageSize);
        }


        /// <summary>
        /// 图片路径
        /// </summary>
        [NotMapped]
        public string ImagePath
        {
          //  get { return ImageServerUrl + imagePath; }

            get { return Core.HimallIO.GetImagePath(imagePath); }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    imagePath = value.Replace(ImageServerUrl, "");
                else
                    imagePath = value;
            }
        }

        public string RelativePath
        {
            get { return imagePath; }
        }

        /// <summary>
        /// 成交数
        /// </summary>
        [NotMapped]
        public long OrderCounts { get; set; }

        /// <summary>
        /// 商品地址
        /// </summary>
        [NotMapped]
        public string Address { get; set; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        [NotMapped]
        public string ShopName { get; set; }

        [NotMapped]
        public string BrandName
        {
            get;
            set;
        }

        #region 表单传参用
        [NotMapped]
        public string CategoryNames { get; set; }

        [NotMapped]
        public int IsCategory { get; set; }

        [NotMapped]
        public long TopId { get; set; }

        [NotMapped]
        public long BottomId { get; set; }
        #endregion

        [NotMapped]
        public string ShowProductState
        {
            get
            {
                string result = "错误数据";
                if (this != null)
                {
                    if (this.AuditStatus == ProductInfo.ProductAuditStatus.WaitForAuditing)
                    {
                        result = (this.SaleStatus == ProductInfo.ProductSaleStatus.OnSale ? ProductInfo.ProductAuditStatus.WaitForAuditing.ToDescription() :
                ProductInfo.ProductSaleStatus.InStock.ToDescription());
                    }
                    else
                    {
                        result = this.AuditStatus.ToDescription();
                    }
                }
                return result;
            }
        }
    }
}
