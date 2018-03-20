using System;
using System.Collections.Generic;
using Himall.Core;
using Himall.Model;
using Himall.CommonModel;

namespace Himall.DTO.Product
{
	public class Product
	{
		public long Id { get; set; }
		public long ShopId { get; set; }
		public long CategoryId { get; set; }
		public string CategoryPath { get; set; }
        /// <summary>
        ///  一级分类ID
        /// </summary>
        public long CategoryTopId
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.CategoryPath))
                {
                    return 0;
                }
                long mainId = 0;
                if (long.TryParse( this.CategoryPath.Split('|')[0],out mainId))
                {
                    return mainId;
                }
                return 0;
            }

        }
		public long TypeId { get; set; }
		public long BrandId { get; set; }
		public string ProductName { get; set; }
		public string ProductCode { get; set; }
		public string ShortDescription { get; set; }
		public ProductInfo.ProductSaleStatus SaleStatus { get; set; }
		public System.DateTime AddedDate { get; set; }
		public long DisplaySequence { get; set; }
		public decimal MarketPrice { get; set; }
		public decimal MinSalePrice { get; set; }
		public bool HasSKU { get; set; }
		public long VistiCounts { get; set; }
		public long SaleCounts { get; set; }
		public ProductInfo.ProductAuditStatus AuditStatus { get; set; }
		public long FreightTemplateId { get; set; }
		public Nullable<decimal> Weight { get; set; }
		public Nullable<decimal> Volume { get; set; }
		public Nullable<int> Quantity { get; set; }
		public string MeasureUnit { get; set; }
		public ProductInfo.ProductEditStatus EditStatus { get; set; }

		public int ConcernedCount { get; set; }

		/// <summary>
		/// 图片路径
		/// </summary>
		public string ImagePath
		{
			get;
			set;
		}

		public string RelativePath
		{
			get;
			set;
		}

		/// <summary>
		/// 最大购买数
		/// </summary>
		public int MaxBuyCount { get; set; }

		/// <summary>
		/// 成交数
		/// </summary>
		public long OrderCounts { get; set; }

		/// <summary>
		/// 商品地址
		/// </summary>
		public string Address { get; set; }

		/// <summary>
		/// 店铺名称
		/// </summary>
		public string ShopName { get; set; }

		public string BrandName
		{
			get;
			set;
		}

		#region 表单传参用
		public string CategoryNames { get; set; }

		public int IsCategory { get; set; }

		public long TopId { get; set; }

		public long BottomId { get; set; }
		#endregion

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

		public DTO.ProductDescription Description
		{
			get;
			set;
		}

		public DTO.SKU[] SKUS
		{
			get;
			set;
		}

		public DTO.ProductAttribute[] Attributes
		{
			get;
			set;
		}

		public DTO.SpecificationValue[] Specifications
		{
			get;
			set;
		}

		/// <summary>
		/// 获取图片地址 YZY修改获取不同尺寸图片的方法
		/// </summary>
		/// <param name="imageIndex">图片序号</param>
		/// <param name="imageSize">图片尺寸</param>
		/// <returns></returns>
		public string GetImage(ImageSize imageSize, int imageIndex = 1)
		{
			// return string.Format(ImagePath + "/{0}_{1}.png", imageIndex, (int)imageSize);
			return Core.HimallIO.GetProductSizeImage(RelativePath, imageIndex, (int)imageSize);
		}
	}
}
