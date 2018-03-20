using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.DTO.Product;
using Himall.Model;
using Himall.Core;
using Himall.IServices;
using Himall.Application.Mappers;
using Himall.IServices.QueryModel;
using Himall.DTO;
using Himall.CommonModel;
using System.Net;
using System.IO;

namespace Himall.Application
{
	public class ProductManagerApplication
	{
		#region 字段
		private static IServices.IProductService _productService;
		private static IServices.IProductDescriptionTemplateService _productDescriptionTemplateService;
        private static IServices.ISearchProductService _searchProductService;
		#endregion

		#region 构造函数
		static ProductManagerApplication()
		{
			_productService = Himall.Core.ObjectContainer.Current.Resolve<Himall.IServices.IProductService>();
            _productDescriptionTemplateService = Himall.Core.ObjectContainer.Current.Resolve<IServices.IProductDescriptionTemplateService>();
            _searchProductService = Himall.Core.ObjectContainer.Current.Resolve<IServices.ISearchProductService>();
        }
		#endregion

		#region 方法
		/// <summary>
		/// 添加商品
		/// </summary>
		/// <param name="shopId">店铺id</param>
		/// <param name="product">商品信息</param>
		/// <param name="pics">需要转移的商品图片地址</param>
		/// <param name="skus">skus，至少要有一项</param>
		/// <param name="description">描述</param>
		/// <param name="attributes">商品属性</param>
		/// <param name="goodsCategory">商家分类</param>
		/// <param name="sellerSpecifications">商家自定义规格</param>
		public static Product AddProduct(long shopId, Product product, string[] pics, SKU[] skus, ProductDescription description, ProductAttribute[] attributes, long[] goodsCategory, SellerSpecificationValue[] sellerSpecifications)
		{
			var productInfo = product.Map<ProductInfo>();
			var skuInofs = skus.Map<SKUInfo[]>();
			var descriptionInfo = description.Map<ProductDescriptionInfo>();
			var attributeInfos = attributes.Map<ProductAttributeInfo[]>();
			var sellerSpecificationInfos = sellerSpecifications.Map<SellerSpecificationValueInfo[]>();
			_productService.AddProduct(shopId, productInfo, pics, skuInofs, descriptionInfo, attributeInfos, goodsCategory, sellerSpecificationInfos);
            CreateHtml(productInfo.Id);
          //  DTO.Product.Product p = new Product();
            return  AutoMapper.Mapper.Map<Product>(productInfo);
		}

		/// <summary>
		/// 更新商品
		/// </summary>
		/// <param name="product">修改后的商品</param>
		/// <param name="pics">需要转移的商品图片地址</param>
		/// <param name="skus">skus，至少要有一项</param>
		/// <param name="description">描述</param>
		/// <param name="attributes">商品属性</param>
		/// <param name="goodsCategory">商家分类</param>
		/// <param name="sellerSpecifications">商家自定义规格</param>
		public static void UpdateProduct(Product product, string[] pics, SKU[] skus, ProductDescription description, ProductAttribute[] attributes, long[] goodsCategory, SellerSpecificationValue[] sellerSpecifications)
		{
			var productInfo = _productService.GetProduct(product.Id);
			if (productInfo == null)
				throw new HimallException("指定id对应的数据不存在");

			var editStatus = (ProductInfo.ProductEditStatus)productInfo.EditStatus;

			if (product.ProductName != productInfo.ProductName)
				editStatus = GetEditStatus(editStatus);
			if (product.ShortDescription != productInfo.ShortDescription)
				editStatus = GetEditStatus(editStatus);

			product.AddedDate = productInfo.AddedDate;
            if (productInfo.SaleStatus != ProductInfo.ProductSaleStatus.InDraft)
            {
                product.SaleStatus = productInfo.SaleStatus;
            }
			product.AuditStatus = productInfo.AuditStatus;
			product.DisplaySequence = productInfo.DisplaySequence;
			product.ShopId = productInfo.ShopId;
			product.HasSKU = productInfo.HasSKU;
			product.ImagePath = productInfo.ImagePath;
            product.SaleCounts = productInfo.SaleCounts;

            if (pics != null)
			{
				if (pics.Any(path => string.IsNullOrWhiteSpace(path) || !path.StartsWith(productInfo.ImagePath)))//有任何修改过的图片
				{
					editStatus = GetEditStatus(editStatus);
				}
			}

			product.DynamicMap(productInfo);
			productInfo.EditStatus = (int)editStatus;

			var skuInofs = skus.Map<SKUInfo[]>();
			var descriptionInfo = description.Map<ProductDescriptionInfo>();
			var attributeInfos = attributes.Map<ProductAttributeInfo[]>();
			var sellerSpecificationInfos = sellerSpecifications.Map<SellerSpecificationValueInfo[]>();
			_productService.UpdateProduct(productInfo, pics, skuInofs, descriptionInfo, attributeInfos, goodsCategory, sellerSpecificationInfos);
            CreateHtml(product.Id);
        }


        /// <summary>
        /// 生成指定商品详情html
        /// </summary>
        public static void CreateHtml(long productId)
        {
            WebClient wc = new WebClient();
            var preUrl = System.Configuration.ConfigurationManager.AppSettings["CurDomainUrl"];
            string url = preUrl + "/Product/Details/" + productId;
            string wapurl = preUrl + "/m-wap/Product/Details/" + productId+ "?nojumpfg = 1";
            string urlHtml = "/Storage/Products/Statics/" + productId + ".html";
            string wapHtml = "/Storage/Products/Statics/" + productId + "-wap.html";

            var data = wc.DownloadData(url);
            var wapdata = wc.DownloadData(wapurl);
            MemoryStream memoryStream = new MemoryStream(data);
            MemoryStream wapMemoryStream = new MemoryStream(wapdata);
            HimallIO.CreateFile(urlHtml, memoryStream, FileCreateType.Create);
            HimallIO.CreateFile(wapHtml, wapMemoryStream, FileCreateType.Create);
        }

        static void CreatPCHtml(long productId)
        {
            WebClient wc = new WebClient();
            var preUrl = System.Configuration.ConfigurationManager.AppSettings["CurDomainUrl"];
            string url = preUrl + "/Product/Details/" + productId;
            string urlHtml = "/Storage/Products/Statics/" + productId + ".html";
            var data = wc.DownloadData(url);
            MemoryStream memoryStream = new MemoryStream(data);
            HimallIO.CreateFile(urlHtml, memoryStream, FileCreateType.Create);
        }

        static void CreatWAPHtml(long productId)
        {
            WebClient wc = new WebClient();
            var preUrl = System.Configuration.ConfigurationManager.AppSettings["CurDomainUrl"];
            string wapurl = preUrl + "/m-wap/Product/Details/" + productId + "?nojumpfg=1";
            string wapHtml = "/Storage/Products/Statics/" + productId + "-wap.html";
            var wapdata = wc.DownloadData(wapurl);
            MemoryStream wapMemoryStream = new MemoryStream(wapdata);
            HimallIO.CreateFile(wapHtml, wapMemoryStream, FileCreateType.Create);
        }

        /// <summary>
        /// 获取指定商品详情html
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static void GetPCHtml(long productId)
        {
            string pcUrlHtml = "/Storage/Products/Statics/" + productId + ".html";
            string fuleUrl = Core.Helper.IOHelper.GetMapPath(pcUrlHtml);
            if (File.Exists(fuleUrl))
            {
                TimeSpan ts = DateTime.Now - File.GetLastWriteTime(fuleUrl);
                if (ts.TotalMinutes > 20)
                    RefreshLocalProductHtml(productId, pcUrlHtml, fuleUrl);              
            }
            else
                RefreshLocalProductHtml(productId, pcUrlHtml, fuleUrl);
        }

        /// <summary>
        /// 获取指定商品详情html
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static void GetWAPHtml(long productId)
        {
            string wapUrlHtml = "/Storage/Products/Statics/" + productId + "-wap.html";
            string fuleUrl = Core.Helper.IOHelper.GetMapPath(wapUrlHtml);
            if (File.Exists(fuleUrl))
            {
                TimeSpan ts = DateTime.Now - File.GetLastWriteTime(fuleUrl);
                if (ts.TotalMinutes > 20)
                    RefreshWAPLocalProductHtml(productId, wapUrlHtml, fuleUrl);
            }
            else
                RefreshWAPLocalProductHtml(productId, wapUrlHtml, fuleUrl);
        }

        /// <summary>
        /// 刷新本地缓存商品html文件 
        /// </summary>     
        /// <param name="targetFilename">本地待生成的html文件名</param>
        static void RefreshWAPLocalProductHtml(long productId, string htmlUrl, string targetFilename)
        {
            lock (htmlUrl)
            {
                if (!File.Exists(targetFilename) || (DateTime.Now - File.GetLastWriteTime(targetFilename)).TotalMinutes > 20)
                {
                    var dirFullname = Core.Helper.IOHelper.GetMapPath("/Storage/Products/Statics");
                    if (!Directory.Exists(dirFullname))
                        Directory.CreateDirectory(dirFullname);
                    if (!HimallIO.ExistFile(htmlUrl))
                        RefreshWapProductHtmlInRemote(productId);
                    byte[] test = HimallIO.GetFileContent(htmlUrl);
                    File.WriteAllBytes(targetFilename, HimallIO.GetFileContent(htmlUrl));
                }
            }
        }

        /// <summary>
        /// 刷新本地缓存商品html文件 
        /// </summary>
        /// <param name="htmlUrl">远程html文件地址</param>
        /// <param name="targetFilename">本地待生成的html文件名</param>
        static void RefreshLocalProductHtml(long productId, string htmlUrl, string targetFilename)
        {
            lock (htmlUrl)
            {
                if (!File.Exists(targetFilename) || (DateTime.Now - File.GetLastWriteTime(targetFilename)).TotalMinutes > 20)
                {
                    var dirFullname = Core.Helper.IOHelper.GetMapPath("/Storage/Products/Statics");
                    if (!Directory.Exists(dirFullname))
                        Directory.CreateDirectory(dirFullname);
                    if (!HimallIO.ExistFile(htmlUrl))
                        RefreshProductHtmlInRemote(productId);
                    byte[] test = HimallIO.GetFileContent(htmlUrl);
                    File.WriteAllBytes(targetFilename, HimallIO.GetFileContent(htmlUrl));
                }
            }
        }

        /// <summary>
        /// 刷新html文件
        /// </summary>
        /// <param name="productId"></param>
        static void RefreshProductHtmlInRemote(long productId)
        {
            string urlHtml = "/Storage/Products/Statics/" + productId + ".html";
            //using (Cache.GetCacheLocker(urlHtml))
            //{
                if (!HimallIO.ExistFile(urlHtml))
                    CreatPCHtml(productId);
            //}
        }

        /// <summary>
        /// 刷新移动端html文件
        /// </summary>
        /// <param name="productId"></param>
        static void RefreshWapProductHtmlInRemote(long productId)
        {
            string urlHtml = "/Storage/Products/Statics/" + productId + "-wap.html";
            //using (Cache.GetCacheLocker(urlHtml))
            //{
                if (!HimallIO.ExistFile(urlHtml))
                    CreatWAPHtml(productId);
            //}
        }


        /// <summary>
        /// 获取一个商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Product GetProduct(long id)
		{
			return _productService.GetProduct(id).Map<Product>();
		}
		/// <summary>
		/// 根据多个ID取多个商品信息
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public static List<Product> GetProductsByIds(IEnumerable<long> ids)
		{
			var productsInfo = _productService.GetProductByIds(ids);
			return productsInfo.ToList().Map<List<Product>>();
		}
        /// <summary>
        /// 根据多个ID，取商品信息（所有状态）
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<Product> GetAllStatusProductByIds(IEnumerable<long> ids)
        {
            var productsInfo = _productService.GetAllStatusProductByIds(ids);
            return productsInfo.ToList().Map<List<Product>>();
        }
		public static QueryPageModel<Product> GetProducts(ProductSearch query)
		{
			var data= _productService.SearchProduct(query);

			return new QueryPageModel<Product>() 
			{
				Models=data.Models.ToList().Map<List<Product>>(),
				Total=data.Total
			};
		}

		public static QueryPageModel<Product> GetProducts(ProductQuery query)
		{
			var data = _productService.GetProducts(query);

			return new QueryPageModel<Product>()
			{
				Models = data.Models.ToList().Map<List<Product>>(),
				Total = data.Total
			};
		}

		/// <summary>
		/// 根据商品id获取属性
		/// </summary>
		/// <param name="id">商品id</param>
		/// <returns></returns>
		public static List<DTO.ProductAttribute> GetProductAttribute(long id)
		{
			var entities = _productService.GetProductAttribute(id).ToList();
			return AutoMapper.Mapper.Map<List<DTO.ProductAttribute>>(entities);
		}

		/// <summary>
		/// 根据商品id获取描述
		/// </summary>
		/// <param name="id">商品id</param>
		/// <returns></returns>
		public static DTO.ProductDescription GetProductDescription(long id)
		{
			var description = _productService.GetProductDescription(id);
			return AutoMapper.Mapper.Map<DTO.ProductDescription>(description);
		}

		/// <summary>
		/// 根据商品id获取描述
		/// </summary>
		/// <param name="ids">商品ids</param>
		/// <returns></returns>
		public static List<DTO.ProductDescription> GetProductDescription(long[] ids)
		{
			var description = _productService.GetProductDescriptions(ids);
			return AutoMapper.Mapper.Map<List<DTO.ProductDescription>>(description);
		}

		public static List<ProductShopCategory> GetProductShopCategoriesByProductId(long productId)
		{
			return _productService.GetProductShopCategories(productId).ToList().Map<List<ProductShopCategory>>();
		}

		/// <summary>
		/// 根据商品id获取SKU
		/// </summary>
		/// <param name="id">商品id</param>
		/// <returns></returns>
		public static List<DTO.SKU> GetSKU(long id)
		{
			var skus = _productService.GetSKUs(id).ToList();
			return AutoMapper.Mapper.Map<List<DTO.SKU>>(skus);
		}

		/// <summary>
		/// 根据商品id获取SKU
		/// </summary>
		/// <param name="productIds">商品id</param>
		/// <returns></returns>
		public static List<DTO.SKU> GetSKU(IEnumerable<long> productIds)
		{
			var skus = _productService.GetSKUs(productIds).ToList();
			return AutoMapper.Mapper.Map<List<DTO.SKU>>(skus);
		}

		/// <summary>
		/// 根据sku id 获取sku信息
		/// </summary>
		/// <param name="skuIds"></param>
		/// <returns></returns>
		public static List<DTO.SKU> GetSKUs(IEnumerable<string> skuIds)
		{
			var list = _productService.GetSKUs(skuIds);
			return list.Map<List<DTO.SKU>>();
		}

		/// <summary>
		/// 获取商品的评论数
		/// </summary>
		/// <param name="productId"></param>
		/// <returns></returns>
		public static int GetProductCommentCount(long productId)
		{
			return _productService.GetProductCommentCount(productId);
		}
        /// <summary>
        /// 取店铺超出安全库存的商品数
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
		public static long GetOverSafeStockProducts(long shopid)
		{
			var products = _productService.GetProducts(new IServices.QueryModel.ProductQuery
			{
				ShopId = shopid,
				OverSafeStock = true
			});
			return products.Total;

		}
        /// <summary>
        /// 取超出警戒库存的商品ID
        /// </summary>
        /// <param name="pids"></param>
        /// <returns></returns>
        public static IEnumerable<long> GetOverSafeStockProductIds(IEnumerable<long> pids)
        {
            var skus = _productService.GetSKUs(pids).ToList();
            var overStockPids = skus.Where(e => e.SafeStock >= e.Stock).Select(e => e.ProductId).Distinct();
            return overStockPids;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pids"></param>
        /// <param name="stock"></param>
        public static void SetProductOverSafeStock(IEnumerable<long> pids,long stock)
        {
            _productService.SetProductOverSafeStock(pids, stock);
        }
		/// <summary>
		/// 删除门店对应的商品
		/// </summary>
		/// <param name="ids"></param>
		/// <param name="shopId"></param>
		public static void DeleteProduct(IEnumerable<long> ids, long shopId)
		{
			_productService.DeleteProduct(ids, shopId);
		}

		/// <summary>
		/// 修改推荐商品
		/// </summary>
		/// <param name="productId"></param>
		/// <param name="relationProductIds"></param>
		public static void UpdateRelationProduct(long productId, string relationProductIds)
		{
			_productService.UpdateRelationProduct(productId, relationProductIds);
		}

		/// <summary>
		/// 获取商品的推荐商品
		/// </summary>
		/// <param name="productId"></param>
		/// <returns></returns>
		public static DTO.ProductRelationProduct GetRelationProductByProductId(long productId)
		{
			return _productService.GetRelationProductByProductId(productId).Map<DTO.ProductRelationProduct>();
		}

		/// <summary>
		/// 获取商品的推荐商品
		/// </summary>
		/// <param name="productId"></param>
		/// <returns></returns>
		public static List<ProductRelationProduct> GetRelationProductByProductIds(IEnumerable<long> productIds)
		{
			return _productService.GetRelationProductByProductIds(productIds).Map<List<DTO.ProductRelationProduct>>();
		}

		/// <summary>
		/// 获取指定类型下面热销的前N件商品
		/// </summary>
		/// <param name="categoryId"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static List<Product> GetHotSaleProductByCategoryId(int categoryId, int count)
		{
			return _productService.GetHotSaleProductByCategoryId(categoryId, count).Map<List<Product>>();
		}

		/// <summary>
		/// 获取商家所有商品描述模板
		/// </summary>
		/// <param name="shopId"></param>
		/// <returns></returns>
		public static List<ProductDescriptionTemplate> GetDescriptionTemplatesByShopId(long shopId)
		{
			return _productDescriptionTemplateService.GetTemplates(shopId).ToList().Map<List<ProductDescriptionTemplate>>();
		}
        /// <summary>
        /// 批量下架商品
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="shopid"></param>
        public static void BatchSaleOff(IEnumerable<long> ids,long shopid)
        {
            _productService.SaleOff(ids, shopid);
        }
        /// <summary>
        /// 批量上架商品
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="shopid"></param>
        public static void BatchOnSale(IEnumerable<long> ids, long shopid)
        {
            _productService.OnSale(ids, shopid);
        }

        /// <summary>
        /// 设置SKU库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="skuIds"></param>
        /// <param name="stock"></param>
        /// <param name="opType"></param>
        public static void SetSkuStock(IEnumerable<string> skuIds, IEnumerable<int> stock, CommonModel.StockOpType opType)
        {
            switch (opType)
            {
                case StockOpType.Normal:
                    _productService.SetSkusStock( skuIds, stock);
                    break;
                case StockOpType.Add:
                    _productService.AddSkuStock(skuIds, stock);
                    break;
                case StockOpType.Reduce:
                    _productService.ReduceSkuStock( skuIds, stock);
                    break;
            }
        }
        /// <summary>
        /// 设置商品库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="pids"></param>
        /// <param name="stock"></param>
        /// <param name="opType"></param>
        public static void SetProductStock(IEnumerable<long> pids, int stock, CommonModel.StockOpType opType)
        {
            switch (opType)
            {
                case StockOpType.Normal:
                    _productService.SetMoreProductToOneStock(pids, stock);
                    break;
                case StockOpType.Add:
                    _productService.AddProductStock(pids, stock);
                    break;
                case StockOpType.Reduce:
                    _productService.ReduceProductStock(pids, stock);
                    break;
            }
        }

		public static bool CanBuy(long userId, long productId, int count, out int reason)
		{
			var product = _productService.GetProduct(productId);
			if (product.SaleStatus != ProductInfo.ProductSaleStatus.OnSale || product.AuditStatus != ProductInfo.ProductAuditStatus.Audited)
			{
				reason = 1;
				return false;
			}
            long stock = product.SKUInfo.Sum(p => p.Stock);
            if (stock == 0)
            {
                reason = 9;
                return false;
            }
			if (product.IsDeleted)
			{
				reason = 2;
				return false;
			}

			if (product.MaxBuyCount <= 0)
			{
				reason = 0;
				return true;
			}

			var buyedCounts = OrderApplication.GetProductBuyCount(userId, new long[] { productId });
			if (product.MaxBuyCount < count + (buyedCounts.ContainsKey(productId) ? buyedCounts[productId] : 0))
			{
				reason = 3;
				return false;
			}

			reason = 0;
			return true;
		}

        public static void BindTemplates(IEnumerable<long> productIds, long? topTemplateId, long? bottomTemplateId)
        {
            _productService.BindTemplate(topTemplateId, bottomTemplateId, productIds);
            
        }
        #endregion

        #region 私有方法
        private static ProductInfo.ProductEditStatus GetEditStatus(ProductInfo.ProductEditStatus status)
		{
			if (status > ProductInfo.ProductEditStatus.EditedAndPending)
				return ProductInfo.ProductEditStatus.CompelPendingHasEdited;
			return ProductInfo.ProductEditStatus.EditedAndPending;
		}
        #endregion
    }
}
