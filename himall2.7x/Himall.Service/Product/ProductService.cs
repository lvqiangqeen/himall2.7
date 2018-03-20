using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EntityFramework.Extensions;
using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.CommonModel;
using System.Transactions;
using MySql.Data.MySqlClient;
using Dapper;

namespace Himall.Service
{
    public class ProductService : ServiceBase, IProductService
    {
        #region 平台服务
        public ObsoletePageModel<ProductInfo> GetProducts(ProductQuery productQueryModel)
        {

            int total;
            var products = GetProductsByQueryModel(productQueryModel, out total);
            ObsoletePageModel<ProductInfo> pageModel = new ObsoletePageModel<ProductInfo>()
            {
                Models = products,
                Total = total
            };
            return pageModel;
        }

        /// <summary>
        /// 判断商品是否存在
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public bool CheckProductIsExist(long productId)
        {
            object result = null;
            string sql = "SELECT 1 FROM HiMall_Products WHERE Id=@Id AND IsDeleted=0";
            using (var conn = new MySqlConnection(Connection.ConnectionString))
            {
                result = conn.ExecuteScalar(sql, new { Id = productId });
            }

            if (result == null) return false;
            return result.ToString().Equals("1");
        }

        /// <summary>
        /// 根据条件获取商品
        /// </summary>
        /// <param name="productQueryModel">商品查询模型</param>
        /// <param name="total">总记录数</param>
        /// <returns></returns>
        IQueryable<ProductInfo> GetProductsByQueryModel(ProductQuery productQueryModel, out int total)
        {
            var products = Context.ProductInfo.Where(item => true);

            if (productQueryModel.ShopId.HasValue)//过滤店铺
                products = products.Where(item => item.ShopId == productQueryModel.ShopId);

            //过滤已删除的商品
            products = products.Where(item => item.IsDeleted == false);

            if (productQueryModel.Ids != null && productQueryModel.Ids.Count() > 0)//条件 编号
                products = products.Where(item => productQueryModel.Ids.Contains(item.Id));

            //排除某些商品的ID
            if (productQueryModel.ExceptIds != null && productQueryModel.ExceptIds.Count() > 0)
            {
                products = products.Where(item => !productQueryModel.ExceptIds.Contains(item.Id));
            }

            if (!string.IsNullOrWhiteSpace(productQueryModel.ProductCode))
                products = products.Where(item => item.ProductCode == productQueryModel.ProductCode);


            if (productQueryModel.OverSafeStock)
                products = products.Where(p => p.SKUInfo.Any(sku => sku.SafeStock >= sku.Stock));//有一个sku库存不足则显示

            if (productQueryModel.AuditStatus != null)//条件 审核状态
                products = products.Where(item => productQueryModel.AuditStatus.Contains(item.AuditStatus));

            if (productQueryModel.SaleStatus.HasValue)
                products = products.Where(item => item.SaleStatus == productQueryModel.SaleStatus);

            if (productQueryModel.CategoryId.HasValue)//条件 分类编号
                products = products.Where(item => ("|" + item.CategoryPath + "|").Contains("|" + productQueryModel.CategoryId.Value + "|"));

            if (productQueryModel.NotIncludedInDraft)
                products = products.Where(item => item.SaleStatus != ProductInfo.ProductSaleStatus.InDraft);

            if (productQueryModel.StartDate.HasValue)//添加日期筛选
                products = products.Where(item => item.AddedDate >= productQueryModel.StartDate);
            if (productQueryModel.EndDate.HasValue)//添加日期筛选
            {
                var end = productQueryModel.EndDate.Value.Date.AddDays(1);
                products = products.Where(item => item.AddedDate < end);
            }
            if (!string.IsNullOrWhiteSpace(productQueryModel.KeyWords))// 条件 关键字
                products = products.Where(item => item.ProductName.Contains(productQueryModel.KeyWords));

            if (!string.IsNullOrWhiteSpace(productQueryModel.ShopName))//查询商家关键字
            {
                var shopIds = Context.ShopInfo.FindBy(item => item.ShopName.Contains(productQueryModel.ShopName)).Select(item => item.Id);
                products = products.Where(item => shopIds.Contains(item.ShopId));
            }
            if (productQueryModel.IsLimitTimeBuy)
            {
                var limits = Context.LimitTimeMarketInfo.Where(l => l.AuditStatus == LimitTimeMarketInfo.LimitTimeMarketAuditStatus.Ongoing).Select(l => l.ProductId);
                products = products.Where(p => !limits.Contains(p.Id));
            }

            if (!string.IsNullOrEmpty(productQueryModel.BrandNameKeyword))
            {
                var brandIds = Context.BrandInfo.Where(p => p.Name.Contains(productQueryModel.BrandNameKeyword) && p.IsDeleted == false).Select(p => p.Id);
                products = products.Where(p => brandIds.Contains(p.BrandId));
            }

            if (productQueryModel.ShopCategoryId.HasValue && productQueryModel.ShopId.HasValue)
            {
                var shopCategoryId = productQueryModel.ShopCategoryId.Value;
                var shopId = productQueryModel.ShopId.Value;
                var shopCategoryIds = Context.ShopCategoryInfo.Where(p => p.ShopId == shopId && (p.Id == shopCategoryId || p.ParentCategoryId == shopCategoryId)).Select(p => p.Id);
                var productIds = Context.ProductShopCategoryInfo.Where(p => shopCategoryIds.Contains(p.ShopCategoryId)).Select(p => p.ProductId);
                products = products.Where(p => productIds.Contains(p.Id));
            }

            switch (productQueryModel.OrderKey)
            {
                case 2:
                    if (!productQueryModel.OrderType)
                        products = products.OrderByDescending(p => p.AddedDate);
                    else
                        products = products.OrderBy(p => p.AddedDate);
                    break;
                case 3:
                    if (!productQueryModel.OrderType)
                        products = products.OrderByDescending(p => p.SaleCounts);
                    else
                        products = products.OrderBy(p => p.SaleCounts);
                    break;
                default:
                    if (!productQueryModel.OrderType)
                        products = products.OrderByDescending(p => p.Id);
                    else
                        products = products.OrderBy(p => p.Id);
                    break;
            }

            return products.GetPage(out total, productQueryModel.PageNo, productQueryModel.PageSize);
        }

        public void AuditProduct(long productId, ProductInfo.ProductAuditStatus auditStatus, string message)
        {
            ProductInfo product = Context.ProductInfo.FindById(productId);
            if (product == null)
                throw new HimallException("此商品不存在");
            if (product.IsDeleted)
                throw new HimallException("此商品已被删除");
            AuditProducts(new long[] { productId }, auditStatus, message);
        }

        public void AuditProducts(IEnumerable<long> productIds, ProductInfo.ProductAuditStatus auditStatus, string message)
        {
            var products = Context.ProductInfo.Where(item => productIds.Contains(item.Id) && item.IsDeleted == false);
            foreach (var product in products.ToList())
            {
                if (product.IsDeleted == false)
                {
                    product.AuditStatus = auditStatus;
                    switch (auditStatus)
                    {
                        case ProductInfo.ProductAuditStatus.Audited:
                            product.EditStatus = (int)ProductInfo.ProductEditStatus.Normal;   //申核通过  置位修改生效
                            break;

                        case ProductInfo.ProductAuditStatus.InfractionSaleOff:
                            product.EditStatus = (int)ProductInfo.ProductEditStatus.CompelPendingAudit;   //违规下架  置位强制需审核
                            break;
                    }
                    if (!string.IsNullOrWhiteSpace(message))//有说明信息，则填充
                    {
                        if (product.ProductDescriptionInfo == null)
                            product.ProductDescriptionInfo = new ProductDescriptionInfo();
                        product.ProductDescriptionInfo.AuditReason = message;
                    }
                }
            }
            Context.SaveChanges();
        }


        #endregion

        #region 商家服务

        public void UpdateProductImagePath(long pId, string path)
        {
            var product = Context.ProductInfo.FindById(pId);
            if (null != product)
            {
                product.ImagePath = path;
                //主表添加
                //转移外站图片，去除script脚本,防止注入
                product.ProductDescriptionInfo.Description = HTMLProcess(product.ProductDescriptionInfo.Description, product.RelativePath);
                product.ProductDescriptionInfo.MobileDescription = HTMLProcess(product.ProductDescriptionInfo.MobileDescription, product.RelativePath);
            }
            Context.SaveChanges();
        }

        public void AddSKU(ProductInfo pInfo)
        {
            var skus = Context.SKUInfo.Where(s => s.ProductId == pInfo.Id);
            Context.SKUInfo.RemoveRange(skus);
            Context.SKUInfo.AddRange(pInfo.SKUInfo);
            Context.SaveChanges();
        }

        /// <summary>
        /// 是否有规格
        /// </summary>
        /// <param name="id">产品编号</param>
        /// <returns></returns>
        public bool HasSKU(long id)
        {
            bool result = false;
            result = Context.SKUInfo.Any(d => d.ProductId == id &&
                ((d.Color != null && d.Color != "") || (d.Version != null && d.Version != "") || (d.Size != null && d.Size != ""))
                );
            ;
            return result;
        }

        public IQueryable<ProductAttributeInfo> GetProductAttribute(long productId)
        {
            var re = from p in Context.ProductAttributeInfo
                     .Include("AttributesInfo")
                     .Include("AttributesInfo.AttributeValueInfo")
                     .Include("AttributesInfo.ProductAttributesInfo")
                     where p.ProductId == productId && p.ValueId != 0
                     select p;
            return re;
        }

        public IQueryable<SellerSpecificationValueInfo> GetSellerSpecifications(long shopId, long typeId)
        {
            return Context.SellerSpecificationValueInfo.FindBy(p => p.ShopId == shopId && p.TypeId == typeId);
        }

        /// <summary>
        /// 转移外站图片，去除script脚本
        /// </summary>
        /// <param name="content">html内容</param>
        /// <param name="id"></param>
        /// <returns></returns>
        string HTMLProcess(string content, string path)
        {
            var details = Path.Combine(path, "Details").Replace("\\", "/");

            try
            {

                string imageRealtivePath = details;
                content = Core.Helper.HtmlContentHelper.TransferToLocalImage(content, "/", imageRealtivePath, Core.HimallIO.GetImagePath(imageRealtivePath) + "/");
                content = Core.Helper.HtmlContentHelper.RemoveScriptsAndStyles(content);
            }
            catch
            {
            }
            return content;
        }

        public void AddProduct(ProductInfo model)
        {
            model.EditStatus = (short)ProductInfo.ProductEditStatus.EditedAndPending;  //初始修改状态
            Context.ProductInfo.Add(model);
            Context.SaveChanges();
            //商品上架
            if (model.SaleStatus == ProductInfo.ProductSaleStatus.OnSale)
            {
                ApplyForSale(model.Id);
            }
        }

        public void AddProduct(long shopId, ProductInfo product, string[] pics, SKUInfo[] skus, ProductDescriptionInfo description, ProductAttributeInfo[] attributes, long[] goodsCategory, SellerSpecificationValueInfo[] sellerSpecifications)
        {
            //设置商品基本属性
            product.AddedDate = DateTime.Now;
            if (product.SaleStatus == ProductInfo.ProductSaleStatus.RawState)
                product.SaleStatus = ProductInfo.ProductSaleStatus.OnSale;
            product.AuditStatus = ProductInfo.ProductAuditStatus.WaitForAuditing;
            product.DisplaySequence = 1;
            product.ShopId = shopId;
            product.HasSKU = skus != null && skus.Length > 0 && (skus.Length > 1 || !string.IsNullOrEmpty(skus[0].Color) || !string.IsNullOrEmpty(skus[0].Size) || !string.IsNullOrEmpty(skus[0].Version));

            this.AddProduct(product);

            //转移商品图片
            product.ImagePath = this.ProductImageToStorageAndCreateThumbnail(product.ShopId, product.Id, pics);

            this.ProcessSKU(shopId, product.Id, skus);

            product.SKUInfo = skus.ToArray();

            if (description != null)
            {
                description.ProductId = product.Id;
                //处理html

                description.Description = ProcessHtml(product.ImagePath, description.Description);
                description.MobileDescription = ProcessHtml(product.ImagePath, description.MobileDescription);

                product.ProductDescriptionInfo = description;
            }

            if (attributes != null && attributes.Length > 0)
            {
                foreach (var item in attributes)
                {
                    item.ProductId = product.Id;
                    product.ProductAttributeInfo.Add(item);
                }
            }

            if (goodsCategory != null && goodsCategory.Where(p => p > 0).Any())
            {
                product.Himall_ProductShopCategories = goodsCategory.Where(p => p > 0).Select(id => new ProductShopCategoryInfo()
                {
                    ProductId = product.Id,
                    ShopCategoryId = id
                }).ToList();
            }

            if (sellerSpecifications != null && sellerSpecifications.Length > 0)
                this.SaveSellerSpecifications(sellerSpecifications.ToList());
            else
                Context.SaveChanges();
        }

        private void UpdateCommon(ProductInfo model)
        {
            var product = Context.ProductInfo.FindById(model.Id);
            var productAuditONoff = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().ProdutAuditOnOff;
            if (productAuditONoff == 0 && product.AuditStatus == ProductInfo.ProductAuditStatus.InfractionSaleOff)
            {
                throw new HimallException("违规下架的商品不能执行此操作！");
            }

            product.BrandId = model.BrandId;
            product.CategoryId = model.CategoryId;
            product.CategoryPath = model.CategoryPath;
            //product.SaleStatus = model.SaleStatus;    修改商品时不修正销售状态

            product.MarketPrice = model.MarketPrice;
            product.MinSalePrice = model.MinSalePrice;
            product.ProductCode = model.ProductCode;
            product.ProductName = model.ProductName;
            product.ShortDescription = model.ShortDescription;
            product.TypeId = model.TypeId;
            product.FreightTemplateId = model.FreightTemplateId;
            product.Volume = model.Volume;
            product.Weight = model.Weight;
            product.MeasureUnit = model.MeasureUnit;
            product.ImagePath = model.RelativePath;
            //商品信息修改状态
            product.EditStatus = model.EditStatus;

            var productDesc = Context.ProductDescriptionInfo.FirstOrDefault(p => p.ProductId == model.Id);
            productDesc.Description = HTMLProcess(model.ProductDescriptionInfo.Description, model.RelativePath);//转移外站图片，去除script脚本,防止注入 
            productDesc.DescriptionPrefixId = model.ProductDescriptionInfo.DescriptionPrefixId;
            productDesc.DescriptiondSuffixId = model.ProductDescriptionInfo.DescriptiondSuffixId;
            productDesc.Meta_Description = model.ProductDescriptionInfo.Meta_Description;
            productDesc.Meta_Keywords = model.ProductDescriptionInfo.Meta_Keywords;
            productDesc.Meta_Title = model.ProductDescriptionInfo.Meta_Title;

            productDesc.MobileDescription = HTMLProcess(model.ProductDescriptionInfo.MobileDescription, model.RelativePath);

            Context.SaveChanges();

            //商品上架
            if (model.SaleStatus == ProductInfo.ProductSaleStatus.OnSale)
            {
                ApplyForSale(product.Id);
            }
        }

        /// <summary>
        /// 申请商品上架
        /// </summary>
        /// <param name="id">商品编号</param>
        /// <returns></returns>
        /// Add:DZY[150715]
        public bool ApplyForSale(long id)
        {
            bool result = false;

            var product = Context.ProductInfo.FindById(id);

            this.ApplyForSale(product);

            Context.SaveChanges();
            result = true;

            return result;
        }

        public void ApplyForSale(ProductInfo product)
        {
            bool isCanAuditPass = true;
            //处理修改状态
            if (product.AuditStatus == ProductInfo.ProductAuditStatus.InfractionSaleOff)
            {
                //违规下架 必审
                product.EditStatus = (int)ProductInfo.ProductEditStatus.CompelPendingAudit;
                isCanAuditPass = false;
            }

            if (product.IsDeleted)
            {
                //删除重新上架 必审
                product.EditStatus = (int)ProductInfo.ProductEditStatus.CompelPendingAudit;
                isCanAuditPass = false;
            }

            #region 免审上架处理
            var productAuditONoff = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().ProdutAuditOnOff == 0;  //是否免审核上架

            if (productAuditONoff)
            {
                if (product.AuditStatus == ProductInfo.ProductAuditStatus.InfractionSaleOff)
                {
                    throw new HimallException("违规下架的商品不能申请免审核上架！");
                    //return false;   //强制退出
                }
                if (isCanAuditPass)
                {
                    if (product.EditStatus < 4)
                    {
                        product.EditStatus = (int)ProductInfo.ProductEditStatus.Normal;   //免审上架 置位修改状态为已生效
                    }
                }
            }
            #endregion

            if (product.SaleStatus != ProductInfo.ProductSaleStatus.InDraft)
            {
                //如果原状态为草稿箱，则不做修改，仍然为草稿箱
                product.SaleStatus = ProductInfo.ProductSaleStatus.OnSale;
            }
            if (product.EditStatus == (int)ProductInfo.ProductEditStatus.EditedAndPending
                || product.EditStatus == (int)ProductInfo.ProductEditStatus.PendingAudit
                || product.EditStatus == (int)ProductInfo.ProductEditStatus.CompelPendingAudit
                || product.EditStatus == (int)ProductInfo.ProductEditStatus.CompelPendingHasEdited
                )
            {
                product.AuditStatus = ProductInfo.ProductAuditStatus.WaitForAuditing;
            }
            else
            {
                product.AuditStatus = ProductInfo.ProductAuditStatus.Audited;
            }
        }

        private void UpdateSKUs(ProductInfo model)
        {
            var skus = Context.SKUInfo.Where(s => s.ProductId == model.Id);
            Context.SKUInfo.RemoveRange(skus);
            Context.SKUInfo.AddRange(model.SKUInfo);
            Context.SaveChanges();
        }

        private void UpdateAttr(ProductInfo model)
        {
            var attr = Context.ProductAttributeInfo.Where(s => s.ProductId == model.Id);
            Context.ProductAttributeInfo.RemoveRange(attr);
            Context.ProductAttributeInfo.AddRange(model.ProductAttributeInfo);
            Context.SaveChanges();
        }

        /// <summary>
        /// 更新商品分类服务
        /// </summary>
        /// <param name="model"></param>
        private void UpdateCategory(ProductInfo model)
        {
            var cate = Context.ProductShopCategoryInfo.Where(s => s.ProductId == model.Id);
            Context.ProductShopCategoryInfo.RemoveRange(cate);
            Context.ProductShopCategoryInfo.AddRange(model.Himall_ProductShopCategories);
            Context.SaveChanges();
        }
        /// <summary>
        /// 更新商品服务
        /// </summary>
        /// <param name="model">商品实体</param>
        public void UpdateProduct(ProductInfo model)
        {
            //更新试商品基本信息
            UpdateCommon(model);

            //更新商品SKU
            //UpdateSKUs(model);

            //更新商品属性
            UpdateAttr(model);

            //更新商品分类
            UpdateCategory(model);
        }

        public void UpdateProduct(ProductInfo product, string[] pics, SKUInfo[] skus, ProductDescriptionInfo description, ProductAttributeInfo[] attributes, long[] goodsCategory, SellerSpecificationValueInfo[] sellerSpecifications)
        {
            var context = this.Context;
            product.HasSKU = skus != null && skus.Length > 0 && (skus.Length > 1 || !string.IsNullOrEmpty(skus[0].Color) || !string.IsNullOrEmpty(skus[0].Size) || !string.IsNullOrEmpty(skus[0].Version));

            //转移商品图片
            if (pics != null)
            {
                if (pics.Any(path => string.IsNullOrWhiteSpace(path) || !path.StartsWith(product.ImagePath)))
                {
                    this.ProductImageToStorageAndCreateThumbnail(product.ShopId, product.Id, pics);
                }
            }

            //处理html
            if (description != null)
            {
                description.Description = ProcessHtml(product.ImagePath, description.Description);
                description.MobileDescription = ProcessHtml(product.ImagePath, description.MobileDescription);

                var descriptionInfo = context.ProductDescriptionInfo.FirstOrDefault(p => p.ProductId == product.Id);
                if (description.Description != descriptionInfo.Description || description.MobileDescription != descriptionInfo.MobileDescription)
                    product.EditStatus = (int)(product.EditStatus > (int)ProductInfo.ProductEditStatus.EditedAndPending ? ProductInfo.ProductEditStatus.CompelPendingHasEdited : ProductInfo.ProductEditStatus.EditedAndPending);

                descriptionInfo.Description = description.Description;
                descriptionInfo.MobileDescription = description.MobileDescription;
                descriptionInfo.DescriptionPrefixId = description.DescriptionPrefixId;
                descriptionInfo.DescriptiondSuffixId = description.DescriptiondSuffixId;
            }

            this.ApplyForSale(product);

            this.ProcessSKU(product.ShopId, product.Id, skus);
            context.SKUInfo.RemoveRange(context.SKUInfo.Where(p => p.ProductId == product.Id));
            context.SKUInfo.AddRange(skus);

            //商品属性
            if (attributes != null && attributes.Length > 0)
            {
                context.ProductAttributeInfo.RemoveRange(context.ProductAttributeInfo.Where(p => p.ProductId == product.Id));
                context.ProductAttributeInfo.AddRange(attributes);
            }

            //商家分类
            if (goodsCategory != null && goodsCategory.Where(p => p > 0).Any())
            {
                var temp = goodsCategory.Where(p => p > 0).Select(shopCategoryId => new ProductShopCategoryInfo()
                {
                    ProductId = product.Id,
                    ShopCategoryId = shopCategoryId
                }).ToArray();

                context.ProductShopCategoryInfo.RemoveRange(context.ProductShopCategoryInfo.Where(p => p.ProductId == product.Id));
                context.ProductShopCategoryInfo.AddRange(temp);
            }

            //保存商家规格
            if (sellerSpecifications != null && sellerSpecifications.Length > 0)
                this.SaveSellerSpecifications(sellerSpecifications.ToList());
            context.SaveChanges();
        }

        private string ProcessHtml(string productImagePath, string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            var descriptionImagePath = productImagePath + "/remark";
            html = Core.Helper.HtmlContentHelper.TransferToLocalImage(html, "/", descriptionImagePath, Core.HimallIO.GetImagePath(descriptionImagePath) + "/");
            html = Core.Helper.HtmlContentHelper.RemoveScriptsAndStyles(html);
            return html;
        }

        public ProductInfo GetProduct(long id)
        {
            var re = (from P in Context.ProductInfo
                          .Include("SKUInfo")
                      where P.Id == id
                      select P).FirstOrDefault();
            return re;
        }

        public ProductInfo GetProductCache(long id)
        {
            string cacheKey = CacheKeyCollection.CACHE_PRODUCT(id);
            if (Cache.Exists(cacheKey))
                return Cache.Get<ProductInfo>(cacheKey);
            ProductInfo result = null;
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                string sql = "select * from himall_Products where Id=@Id  AND IsDeleted=0 ";
                result = conn.QueryFirstOrDefault<ProductInfo>(sql, new { Id = id });
            }
            Cache.Insert<ProductInfo>(cacheKey, result, 300);
            return result;
        }

        /// <summary>
        /// 获取商品详情页需要及时刷新的信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ProductInfo GetNeedRefreshProductInfo(long id)
        {
            ProductInfo p = new ProductInfo();
            string sql = "select MinSalePrice, SaleCounts, MeasureUnit,SaleStatus,AuditStatus,FreightTemplateId,ShopId,TypeId from himall_products where Id=@Id";
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                p = conn.QueryFirstOrDefault<ProductInfo>(sql, new { Id = id });
            }
            return p;

            //var re = Context.ProductInfo.Where(a => a.Id == id).Select(a => new { MinSalePrice = a.MinSalePrice, SaleCounts = a.SaleCounts, MeasureUnit = a.MeasureUnit, SaleStatus = a.SaleStatus, AuditStatus = a.AuditStatus, FreightTemplateId = a.FreightTemplateId, ShopId = a.ShopId }).FirstOrDefault();
            //if (re == null) return null;
            //p.MinSalePrice = re.MinSalePrice;
            //p.SaleCounts = re.SaleCounts;
            //p.MeasureUnit = re.MeasureUnit;
            //p.SaleStatus = re.SaleStatus;
            //p.AuditStatus = re.AuditStatus;
            //p.FreightTemplateId = re.FreightTemplateId;
            //p.ShopId = re.ShopId;
            //return p;
        }

        /// <summary>
        /// 获取商品描述
        /// </summary>
        /// <param name="id">商品编号</param>
        /// <returns></returns>
        public ProductDescriptionInfo GetProductDescription(long id)
        {
            var re = Context.ProductDescriptionInfo.FirstOrDefault(d => d.ProductId == id);
            return re;
        }

        /// <summary>
        /// 获取商品描述
        /// </summary>
        /// <param name="ids">商品编号</param>
        /// <returns></returns>
        public List<ProductDescriptionInfo> GetProductDescriptions(IEnumerable<long> ids)
        {
            return Context.ProductDescriptionInfo.Where(p => ids.Contains(p.ProductId)).ToList();
        }

        /// <summary>
        /// 获取商品的评论数
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public int GetProductCommentCount(long productId)
        {
            return this.Context.ProductCommentInfo.Count(p => p.ProductId == productId);
        }

        public AttributeInfo GetAttributeInfo(long attrId)
        {
            AttributeInfo attr = null;
            if (Cache.Exists(CacheKeyCollection.CACHE_ATTRIBUTE_LIST) && Cache.Exists(CacheKeyCollection.CACHE_ATTRIBUTEVALUE_LIST))
            {
                attr = Cache.Get<List<AttributeInfo>>(CacheKeyCollection.CACHE_ATTRIBUTE_LIST).FirstOrDefault(r => r.Id == attrId);
                if (attr != null)
                    attr.AttributeValueInfo = Cache.Get<List<AttributeValueInfo>>(CacheKeyCollection.CACHE_ATTRIBUTEVALUE_LIST).Where(r => r.AttributeId == attrId).ToList();

                return attr;
            }

            List<AttributeInfo> listAttr = new List<AttributeInfo>();
            List<AttributeValueInfo> listAttrVal = new List<AttributeValueInfo>();

            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                string sql = "SELECT * FROM HiMall_Attributes";
                listAttr = conn.Query<AttributeInfo>(sql).ToList();
                sql = "SELECT * FROM himall_attributevalues";
                listAttrVal = conn.Query<AttributeValueInfo>(sql).ToList();
            }

            attr = listAttr.FirstOrDefault(r => r.Id == attrId);
            attr.AttributeValueInfo = listAttrVal.Where(r => r.AttributeId == attrId).ToList();
            Cache.Insert<List<AttributeInfo>>(CacheKeyCollection.CACHE_ATTRIBUTE_LIST, listAttr);
            Cache.Insert<List<AttributeValueInfo>>(CacheKeyCollection.CACHE_ATTRIBUTEVALUE_LIST, listAttrVal);
            return attr;
        }


        public void SaleOff(long id, long shopid)
        {
            var product = Context.ProductInfo.FindById(id);
            if (product.ShopId != shopid)
                throw new HimallException("只能下架指定店铺的商品");

            //标记为下架状态
            product.SaleStatus = ProductInfo.ProductSaleStatus.InStock;
            Context.SaveChanges();
        }


        public void SaleOff(IEnumerable<long> ids, long shopid)
        {
            var products = Context.ProductInfo.Where(item => ids.Contains(item.Id) && item.IsDeleted == false).ToArray();
            if (products.Count(item => item.ShopId != shopid) > 0)
                throw new HimallException("只能下架指定店铺的商品");

            //标记为下架状态
            foreach (var product in products)
            {
                product.SaleStatus = ProductInfo.ProductSaleStatus.InStock;
                product.AuditStatus = ProductInfo.ProductAuditStatus.WaitForAuditing;  //修正为待审状态
            }
            Context.SaveChanges();
        }


        public void OnSale(long id, long shopId)
        {
            OnSale(new long[] { id }, shopId);
        }

        public void OnSale(IEnumerable<long> ids, long shopId)
        {
            var products = Context.ProductInfo.Where(item => ids.Contains(item.Id) && item.IsDeleted == false);
            if (products.Any(item => item.ShopId != shopId))
                throw new HimallException("只能上架指定店铺的商品");
            //标记为上架架状态
            foreach (var item in products)
            {
                item.SaleStatus = ProductInfo.ProductSaleStatus.OnSale;
                ApplyForSale(item);
            }
            Context.SaveChanges();
        }


        public void DeleteProduct(IEnumerable<long> ids, long shopId)
        {
            //this.Context.ProductInfo.Where(item => ids.Contains(item.Id) && item.ShopId == shopId)
            //    .Update(p => new ProductInfo { IsDeleted = true, EditStatus = (int)ProductInfo.ProductEditStatus.CompelPendingAudit });

            ////清理微店推荐商品
            //this.Context.MobileHomeProductsInfo.Where(d => ids.Contains(d.ProductId) && d.ShopId == shopId).Delete();//批量删除

            ////清理专题
            //this.Context.ModuleProductInfo.Where(d => ids.Contains(d.ProductId) && d.ProductInfo.ShopId == shopId).Delete();//批量删除


            using (TransactionScope scope = new TransactionScope())
            {
                var products = Context.ProductInfo.Where(item => ids.Contains(item.Id) && item.IsDeleted == false).ToList();
                //if (products.Count(item => item.ShopId != shopId) > 0)
                //    throw new HimallException("只能下架指定店铺的商品");
                //context.ProductInfo.RemoveRange(products);
                //context.SaveChanges();
                foreach (var product in products)
                {
                    product.IsDeleted = true;
                    product.EditStatus = (int)ProductInfo.ProductEditStatus.CompelPendingAudit;
                }

                #region
                //清理微店推荐商品
                var mplist = Context.MobileHomeProductsInfo.Where(d => ids.Contains(d.ProductId)).ToList();
                Context.MobileHomeProductsInfo.RemoveRange(mplist);   //批量删除
                                                                      //清理专题
                var mtplist = Context.ModuleProductInfo.Where(d => ids.Contains(d.ProductId)).ToList();
                Context.ModuleProductInfo.RemoveRange(mtplist);   //批量删除

                //清理购物车
                var cartlist = Context.ShoppingCartItemInfo.Where(d => ids.Contains(d.ProductId)).ToList();
                Context.ShoppingCartItemInfo.RemoveRange(cartlist);
                #endregion

                //删除门店商品
                Context.Database.ExecuteSqlCommand("delete from himall_shopbranchskus where ProductId in(" + string.Join(",", ids) + ")");

                Context.SaveChanges();
                scope.Complete();
            }

            Context.SaveChanges();
        }

        public IQueryable<SKUInfo> GetSKUs(long productId)
        {
            //return Context.SKUInfo.FindBy(s => s.ProductId == productId);
            var sku = Context.SKUInfo.FindBy(s => s.ProductId == productId).ToList().AsQueryable();
            foreach (var item in sku)
            {
                ProductTypeInfo typeInfo = (ProductTypeInfo)Context.ProductTypeInfo.Join(Context.ProductInfo.Where(d => d.Id == item.ProductId), x => x.Id, y => y.TypeId, (x, y) => x).ToList().FirstOrDefault();
                item.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                item.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                item.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            }
            return sku;
        }

        public List<SKUInfo> GetSkuList(long productId)
        {
            List<SKUInfo> list = new List<SKUInfo>();
            string sql = "SELECT * FROM Himall_SKUs WHERE ProductId=@ProductId";
            using (var conn = new MySqlConnection(Connection.ConnectionString))
            {
                list = conn.Query<SKUInfo>(sql, new { ProductId = productId }).ToList();
            }
            return list;
        }

        public SKUInfo GetSku(string skuId)
        {
            return Context.SKUInfo.Include("ProductInfo").FindBy(s => s.Id == skuId).FirstOrDefault();
        }

        public string GetSkuString(string skuId)
        {
            var sku = Context.SKUInfo.FirstOrDefault(p => p.Id == skuId);
            if (sku != null)
            {
                ProductTypeInfo typeInfo = (ProductTypeInfo)Context.ProductTypeInfo.Join(Context.ProductInfo.Where(d => d.Id == sku.ProductId), x => x.Id, y => y.TypeId, (x, y) => x).ToList().FirstOrDefault();
                string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                string str = "";
                List<string> arrstr = new List<string>();
                if (!string.IsNullOrEmpty(sku.Color))
                    arrstr.Add(" " + colorAlias + "：" + sku.Color);
                if (!string.IsNullOrEmpty(sku.Size))
                    arrstr.Add(" " + sizeAlias + "：" + sku.Size);
                if (!string.IsNullOrEmpty(sku.Version))
                    arrstr.Add(" " + versionAlias + "：" + sku.Version);
                if (arrstr.Count > 0)
                {
                    str = string.Join(";", arrstr.ToArray());
                }
                return str;
            }

            return string.Empty;
        }

        public void SaveSellerSpecifications(List<SellerSpecificationValueInfo> info)
        {
            if (null != info && info.Count() > 0)
            {
                long shopId = info[0].ShopId, typeId = info[0].TypeId;

                var source = Context.SellerSpecificationValueInfo.Where(s => s.ShopId == shopId && s.TypeId == typeId);
                foreach (var item in info)
                {
                    //修改已有的
                    if (source.Any(s => s.ValueId == item.ValueId))
                    {
                        source.FirstOrDefault(s => s.ValueId == item.ValueId).Value = item.Value;
                    }
                    //添加
                    else
                    {
                        item.Specification = Context.SpecificationValueInfo.FindById(item.ValueId).Specification;
                        Context.SellerSpecificationValueInfo.Add(item);
                    }
                }
                Context.SaveChanges();
            }
        }


        public IQueryable<ProductShopCategoryInfo> GetProductShopCategories(long productId)
        {
            return Context.ProductShopCategoryInfo.FindBy(p => p.ProductId == productId);
        }

        public void BindTemplate(long? topTemplateId, long? bottomTemplateId, IEnumerable<long> productIds)
        {
            var products = Context.ProductInfo.Where(item => productIds.Contains(item.Id) && item.IsDeleted == false).ToArray();
            foreach (var product in products)
            {
                if (product.ProductDescriptionInfo == null)
                    product.ProductDescriptionInfo = new ProductDescriptionInfo();
                if (topTemplateId.HasValue)
                    product.ProductDescriptionInfo.DescriptionPrefixId = topTemplateId.Value;
                if (bottomTemplateId.HasValue)
                    product.ProductDescriptionInfo.DescriptiondSuffixId = bottomTemplateId.Value;
            }
            Context.SaveChanges();
        }

        public IQueryable<ProductInfo> GetProductByIds(IEnumerable<long> ids)
        {
            var products = Context.ProductInfo.Where(item => ids.Contains(item.Id) && item.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && item.AuditStatus == ProductInfo.ProductAuditStatus.Audited && item.IsDeleted == false);
            return products;
        }
        public List<ProductInfo> GetAllProductByIds(IEnumerable<long> ids)
        {
            string sql = "select * from himall_products where Id in @ProductId  AND IsDeleted=0";
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                return conn.Query<ProductInfo>(sql, new { ProductId = ids.ToArray() }).ToList();
            }
        }

        public IEnumerable<ProductInfo> GetAllStatusProductByIds(IEnumerable<long> ids)
        {
            var products = Context.ProductInfo.Where(item => ids.Contains(item.Id));
            return products;
        }

        public long GetNextProductId()
        {
            if (Context.ProductInfo.Count() == 0)
                return 1;
            return Context.ProductInfo.Max(p => p.Id) + 1;
        }

        public int GetShopAllProducts(long shopId)
        {
            int productsNum = Context.ProductInfo.Where(item => item.ShopId == shopId && item.IsDeleted == false).Count();
            return productsNum;
        }

        public int GetShopOnsaleProducts(long shopId)
        {
            return Context.ProductInfo.Where(item => item.ShopId == shopId && item.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && item.AuditStatus == ProductInfo.ProductAuditStatus.Audited && item.IsDeleted == false).Count();

        }
        #endregion

        #region   前台页面

        public ObsoletePageModel<ProductInfo> SearchProduct(ProductSearch search)
        {

            var result = from item in Context.ProductInfo
                         where item.IsDeleted == false
                         select new
                         {
                             Id = item.Id,
                             ProductName = item.ProductName,
                             AuditStatus = item.AuditStatus,
                             SaleStatus = item.SaleStatus,
                             Himall_ProductShopCategories = item.Himall_ProductShopCategories,
                             ShopId = item.ShopId,
                             CategoryPath = item.CategoryPath,
                             CategoryId = item.CategoryId,
                             BrandId = item.BrandId,
                             MinSalePrice = item.MinSalePrice,
                             SaleCounts = item.SaleCounts,
                             ProductConsultationInfo = item.ProductConsultationInfo,
                             AddedDate = item.AddedDate,
                             Himall_ProductVistis = item.Himall_ProductVistis,
                             Himall_ProductComments = item.Himall_ProductComments
                         };

            StringBuilder sqlQuery = new StringBuilder();

            //shopId
            if (search.shopId != 0)
            {
                result = result.Where(p => p.ShopId.Equals(search.shopId)
                    && p.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                    && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale);
            }
            else
            {
                result = result.Where(p => p.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                    && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale);
            }


            //Category
            if (search.CategoryId > 0)
            {
                var categoryId = search.CategoryId;
                //下面有一个按店铺分类查询的条件
                //if (search.shopId > 0)
                //{
                //    result = result.Where(p => p.Himall_ProductShopCategories.
                //        Any(s => s.ShopCategoryId.Equals(search.CategoryId)));
                //}
                //else
                //{
                result = result.Where(p => ("|" + p.CategoryPath + "|").
                    Contains("|" + search.CategoryId.ToString() + "|")).OrderBy(s => s.CategoryId);

                //}
            }
            //ShopCategory
            if (search.ShopCategoryId.HasValue && search.ShopCategoryId > 0)
            {
                IEnumerable<long> productIds = new long[] { };
                productIds = Context.ProductShopCategoryInfo
                    .Where(
                          item => item.ShopCategoryInfo.Id == search.ShopCategoryId ||
                                  item.ShopCategoryInfo.ParentCategoryId == search.ShopCategoryId).Select(item => item.ProductId);

                result = result.Where(p => productIds.Contains(p.Id));
            }


            //Brand
            if (search.BrandId > 0)
            {
                result = result.Where(p => p.BrandId.Equals(search.BrandId));
            }

            //Attrbuite
            foreach (var attr in search.AttrIds)
            {
                long attrId = 0;
                long.TryParse(attr.Split('_')[0], out attrId);

                if (attr.Split('_').Length <= 1)
                {
                    continue;
                }
                long attrValueId = 0;
                long.TryParse(attr.Split('_')[1], out attrValueId);

                var pIds = Context.ProductAttributeInfo.Where(p => p.AttributeId == attrId && p.ValueId == attrValueId).
                    Select(p => p.ProductId);
                result = result.Where(p => pIds.Contains(p.Id));
            }

            //Keyword
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keywords = search.Keyword.Replace("\t", " ").Replace("　", " ").Split(' ');//分隔多个关键字
                var products = result;
                var validProducts = result;
                bool first = true;//第一次查询标志
                foreach (var keyword in keywords)
                {
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        if (first)
                        {
                            validProducts = result.Where(p => p.ProductName.ToUpper().Contains(keyword.ToUpper()));
                        }
                        else
                        {
                            validProducts = validProducts.Concat(products.Where(p => p.ProductName.ToUpper().Contains(keyword.ToUpper())));
                        }
                        first = false;
                    }
                }
                result = validProducts.AsQueryable();
            }
            if (search.startPrice >= 0 && search.EndPrice > search.startPrice && search.EndPrice != decimal.MaxValue)
            {
                result = result.Where(r => r.MinSalePrice >= search.startPrice && r.MinSalePrice <= search.EndPrice);
            }

            if (search.StartDate.HasValue)
            {
                //添加日期筛选
                result = result.Where(item => item.AddedDate >= search.StartDate);
            }

            if (search.EndDate.HasValue)
            {
                //添加日期筛选
                result = result.Where(item => item.AddedDate <= search.EndDate);
            }

            //Ex_Keyword
            if (!string.IsNullOrWhiteSpace(search.Ex_Keyword))
            {
                result = result.Where(p => p.ProductName.Contains(search.Ex_Keyword));
            }
            if (search.shopBranchId.HasValue && search.shopBranchId.Value != 0)
            {//过滤门店已选商品
                var pid = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == search.shopBranchId.Value).Select(item => item.ProductId).Distinct();
                result = result.Where(e => !pid.Any(id => id == e.Id));
            }

            //判断商品所在店铺是否已经过期

            //ServiceProvider.Instance<IShopService>.Create.GetShops
            //var shopsevice = ServiceProvider.Instance<IShopService>.Create;

            //result = result.Where(p => shopsevice.IsExpiredShop(p.ShopId) == false);

            var searchProduct = from p in result
                                join shop in Context.ShopInfo on p.ShopId equals shop.Id
                                where shop.EndDate.Value > DateTime.Now && shop.ShopStatus != ShopInfo.ShopAuditStatus.Freeze
                                select p;

            //int total = searchProduct.Count();





            var proudctIds = searchProduct.OrderBy(p => p.Id).Select(item => item.Id).Distinct();
            var productsResult = from item in Context.ProductInfo
                                 where proudctIds.Contains(item.Id)
                                 select item;

            int total = productsResult.Count();
            //日龙添加（判断分页超出）
            if (search.PageSize == 0)
            {
                search.PageSize = 10;
            }
            int maxpage = (int)Math.Ceiling(productsResult.Count() / (double)search.PageSize);
            if (maxpage < search.PageNumber)
            {
                //search.PageNumber = 1;
                //分页超出返回null
                //Edit：DZY[150707]

                ObsoletePageModel<ProductInfo> noDataResult = new ObsoletePageModel<ProductInfo>()
                {
                    Total = total,
                    Models = (new ProductInfo[] { }).AsQueryable()
                };
                return noDataResult;
            }

            //end


            switch (search.OrderKey)
            {
                case 2:
                    if (!search.OrderType)
                        productsResult = productsResult.OrderByDescending(p => p.Himall_ProductVistis.FirstOrDefault().OrderCounts);
                    else
                        productsResult = productsResult.OrderBy(p => p.Himall_ProductVistis.FirstOrDefault().OrderCounts);
                    break;
                case 3:
                    if (!search.OrderType)
                        productsResult = productsResult.OrderByDescending(p => p.MinSalePrice);
                    else
                        productsResult = productsResult.OrderBy(p => p.MinSalePrice);
                    break;
                case 4:
                    if (!search.OrderType)
                        productsResult = productsResult.OrderByDescending(p => p.Himall_ProductComments.Count());
                    else
                        productsResult = productsResult.OrderBy(p => p.Himall_ProductComments.Count());
                    break;
                case 5:
                    if (!search.OrderType)
                        productsResult = productsResult.OrderByDescending(p => p.AddedDate);
                    else
                        productsResult = productsResult.OrderBy(p => p.AddedDate);
                    break;
                default:
                    productsResult = productsResult.OrderByDescending(item => item.Id);
                    break;
            }
            productsResult = productsResult.Skip((search.PageNumber - 1) * search.PageSize).Take(search.PageSize);

            //TODO LRL 2015/09/08 加入店铺名称和地址信息
            foreach (var p in productsResult.ToArray())
            {
                long freightTemplateId = p.FreightTemplateId;
                p.ShopName = p.Himall_Shops.ShopName;
                //TODO:直接取商品表里的销售数量
                if (p.Himall_ProductVistis == null)
                {
                    p.SaleCounts = 0;
                }
                else
                {
                    p.SaleCounts = p.Himall_ProductVistis.Sum(s => s.OrderCounts.HasValue ? s.OrderCounts.Value : 0);
                }
                p.Address = "";

                int? regionId = 0;
                if (freightTemplateId != 0)
                {
                    try
                    {
                        regionId = ServiceProvider.Instance<IFreightTemplateService>.Create.GetFreightTemplate(freightTemplateId).SourceAddress;
                        if (regionId.HasValue)
                        {
                            var region = ServiceProvider.Instance<IRegionService>.Create.GetRegion(regionId.Value);
                            if (region != null)
                                p.Address = region.Name;
                        }
                    }
                    catch
                    {
                        p.Address = "";
                        Core.Log.Error(string.Format("获取地区名字出错，参数 freightTemplateId={0},regionId={1}", freightTemplateId, regionId));
                    }
                }
            }

            ObsoletePageModel<ProductInfo> pageModel = new ObsoletePageModel<ProductInfo>()
            {
                Total = total,
                Models = productsResult
            };
            return pageModel;
        }


        public ObsoletePageModel<ProductInfo, SearchProductModel> SearchProductAndOtherModel(ProductSearch search)
        {
            var result = from item in Context.ProductInfo.Where(d=>d.IsDeleted==false)
                         select item;

            StringBuilder sqlQuery = new StringBuilder();

            //shopId
            if (search.shopId != 0)
            {
                result = result.Where(p => p.ShopId.Equals(search.shopId)
                    && p.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                    && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale);
            }
            else
            {
                result = result.Where(p => p.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                    && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale);
            }


            //Category
            if (search.CategoryId > 0)
            {
                var categoryId = search.CategoryId;

                if (search.shopId > 0)
                {
                    result = result.Where(p => p.Himall_ProductShopCategories.
                        Any(s => s.ShopCategoryId.Equals(search.CategoryId)));
                }
                else
                {
                    result = result.Where(p => ("|" + p.CategoryPath + "|").
                        Contains("|" + search.CategoryId.ToString() + "|")).OrderBy(s => s.CategoryId);

                }
            }
            //ShopCategory
            if (search.ShopCategoryId.HasValue && search.ShopCategoryId > 0)
            {
                IEnumerable<long> productIds = new long[] { };
                productIds = Context.ProductShopCategoryInfo
                    .Where(
                          item => item.ShopCategoryInfo.Id == search.ShopCategoryId ||
                                  item.ShopCategoryInfo.ParentCategoryId == search.ShopCategoryId).Select(item => item.ProductId);

                result = result.Where(p => productIds.Contains(p.Id));
            }


            //Brand
            if (search.BrandId > 0)
            {
                result = result.Where(p => p.BrandId.Equals(search.BrandId));
            }

            //Attrbuite
            foreach (var attr in search.AttrIds)
            {
                long attrId = 0;
                long.TryParse(attr.Split('_')[0], out attrId);

                if (attr.Split('_').Length <= 1)
                {
                    continue;
                }
                long attrValueId = 0;
                long.TryParse(attr.Split('_')[1], out attrValueId);

                var pIds = Context.ProductAttributeInfo.Where(p => p.AttributeId == attrId && p.ValueId == attrValueId).
                    Select(p => p.ProductId);
                result = result.Where(p => pIds.Contains(p.Id));
            }

            //Keyword
            if (search.CategoryId == 0 && !string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keywords = search.Keyword.Replace("\t", " ").Replace("　", " ").Split(' ');//分隔多个关键字
                var products = result;
                var validProducts = result;
                bool first = true;//第一次查询标志
                foreach (var keyword in keywords)
                {
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        if (first)
                        {
                            validProducts = result.Where(p => p.ProductName.ToUpper().Contains(keyword.ToUpper()));
                        }
                        else
                        {
                            validProducts = validProducts.Concat(products.Where(p => p.ProductName.ToUpper().Contains(keyword.ToUpper())));
                        }
                        first = false;
                    }
                }
                result = validProducts.AsQueryable();
            }
            if (search.startPrice >= 0 && search.EndPrice > search.startPrice && search.EndPrice != decimal.MaxValue)
            {
                result = result.Where(r => r.MinSalePrice >= search.startPrice && r.MinSalePrice <= search.EndPrice);
            }

            //Ex_Keyword
            if (!string.IsNullOrWhiteSpace(search.Ex_Keyword))
            {
                result = result.Where(p => p.ProductName.Contains(search.Ex_Keyword));
            }


            //判断商品所在店铺是否已经过期

            //ServiceProvider.Instance<IShopService>.Create.GetShops
            //var shopsevice = ServiceProvider.Instance<IShopService>.Create;

            //result = result.Where(p => shopsevice.IsExpiredShop(p.ShopId) == false);

            var searchProduct = from p in result
                                join shop in Context.ShopInfo on p.ShopId equals shop.Id
                                where shop.EndDate.Value > DateTime.Now && shop.ShopStatus != ShopInfo.ShopAuditStatus.Freeze
                                select p;

            //int total = searchProduct.Count();





            //var proudctIds = searchProduct.OrderBy(p => p.Id).Select(item => item.Id).Distinct();
            //var productsResult = searchProduct;

            var proudctIds = searchProduct.OrderBy(p => p.Id).Select(item => item.Id).Distinct();
            var productsResult = from item in Context.ProductInfo
                                 where proudctIds.Contains(item.Id)
                                 select item;

            int total = productsResult.Count();
            //日龙添加（判断分页超出）
            if (search.PageSize == 0)
            {
                search.PageSize = 10;
            }
            int maxpage = (int)Math.Ceiling(total / (double)search.PageSize);
            if (maxpage < search.PageNumber)
            {
                //search.PageNumber = 1;
                //分页超出返回null
                //Edit：DZY[150707]

                ObsoletePageModel<ProductInfo, SearchProductModel> noDataResult = new ObsoletePageModel<ProductInfo, SearchProductModel>()
                {
                    Total = total,
                    Models = (new ProductInfo[] { }).AsQueryable(),
                    TotalData = null
                };
                return noDataResult;
            }

            //end


            switch (search.OrderKey)
            {
                case 2:
                    if (!search.OrderType)
                        productsResult = productsResult.OrderByDescending(p => p.Himall_ProductVistis.FirstOrDefault().OrderCounts);
                    else
                        productsResult = productsResult.OrderBy(p => p.Himall_ProductVistis.FirstOrDefault().OrderCounts);
                    break;
                case 3:
                    if (!search.OrderType)
                        productsResult = productsResult.OrderByDescending(p => p.MinSalePrice);
                    else
                        productsResult = productsResult.OrderBy(p => p.MinSalePrice);
                    break;
                case 4:
                    if (!search.OrderType)
                        productsResult = productsResult.OrderByDescending(p => p.Himall_ProductComments.Count());
                    else
                        productsResult = productsResult.OrderBy(p => p.Himall_ProductComments.Count());
                    break;
                case 5:
                    if (!search.OrderType)
                        productsResult = productsResult.OrderByDescending(p => p.AddedDate);
                    else
                        productsResult = productsResult.OrderBy(p => p.AddedDate);
                    break;
                default:
                    productsResult = productsResult.OrderByDescending(item => item.Id);
                    break;
            }
            //全部商品的品牌（未分页）
            List<BrandInfo> brandArray = new List<BrandInfo>();
            var brandIds = productsResult.Where(a => a.BrandId != 0).Select(a => a.BrandId).Distinct();
            var _brand = Context.BrandInfo.Where(e => brandIds.Contains(e.Id) && e.IsDeleted == false);
            brandArray = _brand.ToList();

            var productAttrs = GetAttributes(productsResult);

            productsResult = productsResult.Skip((search.PageNumber - 1) * search.PageSize).Take(search.PageSize);

            var prolist = productsResult.ToList();

            //TODO LRL 2015/09/08 加入店铺名称和地址信息
            foreach (var p in prolist)
            {
                long freightTemplateId = p.FreightTemplateId;
                p.ShopName = p.Himall_Shops.ShopName;
                //TODO:直接取商品表里的销售数量
                //if (p.Himall_ProductVistis == null)
                //{
                //    p.SaleCounts = 0;
                //}
                //else
                //{
                //    p.SaleCounts = p.Himall_ProductVistis.Sum(s => s.OrderCounts.HasValue ? s.OrderCounts.Value : 0);
                //}
                p.Address = "";

                long? regionId = 0;
                if (freightTemplateId != 0)
                {
                    try
                    {
                        regionId = ServiceProvider.Instance<IFreightTemplateService>.Create.GetFreightTemplate(freightTemplateId).SourceAddress;
                        if (regionId.HasValue)
                        {
                            var region = ServiceProvider.Instance<IRegionService>.Create.GetRegion(regionId.Value);
                            if (region != null)
                                p.Address = region.Name;
                        }
                    }
                    catch
                    {
                        p.Address = "";
                        Core.Log.Error(string.Format("获取地区名字出错，参数 freightTemplateId={0},regionId={1}", freightTemplateId, regionId));
                    }
                }
            }

            ObsoletePageModel<ProductInfo, SearchProductModel> pageModel = new ObsoletePageModel<ProductInfo, SearchProductModel>()
            {
                Total = total,
                Models = productsResult,
                TotalData = new SearchProductModel
                {
                    Brands = brandArray,
                    ProductAttrs = productAttrs
                }
            };
            return pageModel;
        }
        #endregion

        private List<TypeAttributesModel> GetAttributes(IQueryable<ProductInfo> products)
        {
            List<TypeAttributesModel> ProductAttrs = new List<TypeAttributesModel>();
            var list = from pp in
                           (from p in products
                            group p by p.CategoryId into G
                            select new
                            {
                                G.Key,
                                Count = G.Count()
                            })
                       orderby pp.Count descending
                       select pp;
            var _cid = list.ToList()[0].Key;
            if (list.Count() <= 20)
            {
                var prods = products.Where(p => p.CategoryId.Equals(_cid)).ToArray();
                var allProdsAttrs = (from p in prods
                                     join a in Context.ProductAttributeInfo on p.Id equals a.ProductId
                                     where a.ValueId != 0


                                     select a).ToArray();
                foreach (var prod in prods)
                {
                    var prodAttrs = allProdsAttrs.Where(e => e.ProductId == prod.Id);
                    foreach (var attr in prodAttrs)
                    {
                        if (!ProductAttrs.Any(p => p.AttrId == attr.AttributeId))
                        {
                            TypeAttributesModel attrModel = new TypeAttributesModel()
                            {
                                AttrId = attr.AttributeId,
                                AttrValues = new List<TypeAttrValue>(),
                                Name = attr.AttributesInfo.Name
                            };
                            foreach (var attrV in attr.AttributesInfo.AttributeValueInfo)
                            {
                                if (prodAttrs.Any(p => p.ValueId == attrV.Id))
                                {
                                    attrModel.AttrValues.Add(new TypeAttrValue
                                    {
                                        Id = attrV.Id.ToString(),
                                        Name = attrV.Value
                                    });
                                }
                            }
                            ProductAttrs.Add(attrModel);
                        }
                        else
                        {
                            var attrTemp = ProductAttrs.FirstOrDefault(p => p.AttrId == attr.AttributeId);

                            if (!attrTemp.AttrValues.Any(p => p.Id == attr.ValueId.ToString()))
                            {
                                var avinfo = attr.AttributesInfo.AttributeValueInfo.FirstOrDefault(a => a.Id == attr.ValueId);
                                if (null != avinfo)
                                {
                                    attrTemp.AttrValues.Add(new TypeAttrValue
                                    {
                                        Id = attr.ValueId.ToString(),
                                        Name = attr.AttributesInfo.AttributeValueInfo.FirstOrDefault(a => a.Id == attr.ValueId).Value
                                    });
                                }
                            }
                        }
                    }
                }
            }
            return ProductAttrs;
        }

        #region 获取店铺热销的前N件商品
        public IQueryable<ProductInfo> GetHotSaleProduct(long shopId, int count = 5)
        {
            string CACHE_MANAGER_KEY = CacheKeyCollection.HotSaleProduct(shopId);
            if (Cache.Exists(CACHE_MANAGER_KEY))
            {
                return Core.Cache.Get<List<ProductInfo>>(CACHE_MANAGER_KEY).AsQueryable().OrderByDescending(h => h.SaleCounts);
            }
            else
            {
                var data =
                                (from p in Context.ProductInfo
                                 .Include("Himall_ProductVistis")
                                 where p.ShopId.Equals(shopId) && p.SaleStatus == Himall.Model.ProductInfo.ProductSaleStatus.OnSale && p.AuditStatus == ProductInfo.ProductAuditStatus.Audited && p.IsDeleted == false
                                 select p
                                );
                var result = data.OrderByDescending(s => s.SaleCounts).Take(count).ToList();
                #region 注释
                //result = result.Select(r => new ProductInfo() {
                //     Id = r.Id, ImagePath = r.ImagePath,  HasSKU = r.HasSKU, AddedDate = r.AddedDate, Address = r.Address, AuditStatus = r.AuditStatus,  BottomId = r.BottomId, BrandId = r.BrandId, BrandName=r.BrandName,
                //      CategoryId = r.CategoryId , CategoryNames = r.CategoryNames, CategoryPath = r.CategoryPath, ConcernedCount = r.ConcernedCount, DisplaySequence = r.DisplaySequence, EditStatus = r.EditStatus,
                //       FreightTemplateId = r.FreightTemplateId, IsCategory = r.IsCategory, MarketPrice = r.MarketPrice, MeasureUnit = r.MeasureUnit, MinSalePrice = r.MinSalePrice, OrderCounts = r.OrderCounts,
                //        ProductCode = r.ProductCode, Quantity = r.Quantity, SaleStatus = r.SaleStatus, ShopId = r.ShopId, SaleCounts = r.SaleCounts, ProductName = r.ProductName, ShopName = r.ShopName, Weight = r.Weight,
                //         ShortDescription = r.ShortDescription,
                //    Himall_Categories = new CategoryInfo {  CommisRate = r.Himall_Categories.CommisRate, Depth=r.Himall_Categories.Depth, HasChildren=r.Himall_Categories.HasChildren, DisplaySequence = r.Himall_Categories.DisplaySequence, Icon = r.Himall_Categories.Icon, Meta_Description = r.Himall_Categories.Meta_Description, Id = r.Himall_Categories.Id,
                //        Meta_Title = r.Himall_Categories.Meta_Title, Name = r.Himall_Categories.Name, ParentCategoryId = r.Himall_Categories.ParentCategoryId, Path = r.Himall_Categories.Path, RewriteName = r.Himall_Categories.RewriteName, TypeId = r.Himall_Categories.TypeId, Meta_Keywords = r.Himall_Categories.Meta_Keywords},  
                //    Himall_AgentProducts = r.Himall_AgentProducts.Select(t => new AgentProductsInfo()
                //    {
                //        AddTime = t.AddTime,
                //        Id = t.Id,
                //        ProductId = t.ProductId,
                //        ShopId = t.ShopId,
                //        UserId = t.UserId
                //    }).ToList(),
                //    Himall_BrowsingHistory = r.Himall_BrowsingHistory.Select(t => new BrowsingHistoryInfo()
                //    {
                //        BrowseTime = t.BrowseTime,
                //        Id = t.Id,
                //        MemberId = t.MemberId,
                //        ProductId = t.ProductId
                //    }).ToList(),
                //    Himall_CollocationSkus = r.Himall_CollocationSkus.Select(t => new CollocationSkuInfo()
                //    {
                //        ColloProductId = t.ColloProductId,
                //        Id = t.Id,
                //        Price = t.Price,
                //        ProductId = t.ProductId,
                //        SkuID = t.SkuID
                //    }).ToList(),
                //    Himall_Favorites = r.Himall_Favorites.Select(t => new FavoriteInfo()
                //    {
                //        Date = t.Date,
                //        Id = t.Id,
                //        ProductId = t.ProductId,
                //        UserId = t.UserId
                //    }).ToList(),
                //    Himall_FlashSale = r.Himall_FlashSale.Select(t => new FlashSaleInfo()
                //    {
                //        BeginDate = t.BeginDate,
                //        CategoryName = t.CategoryName,
                //        EndDate = t.EndDate,
                //        Id = t.Id,
                //        ImagePath = t.ImagePath,
                //        MinPrice = t.MinPrice,
                //        LimitCountOfThePeople = t.LimitCountOfThePeople,
                //        ProductId = t.ProductId,
                //        SaleCount = t.SaleCount,
                //        ShopId = t.ShopId,
                //        Status = t.Status,
                //        Title = t.Title
                //    }).ToList(),
                //    Himall_FloorProducts = r.Himall_FloorProducts.Select(t => new FloorProductInfo()
                //    {
                //        FloorId = t.FloorId,
                //        Id = t.Id,
                //        ProductId = t.ProductId,
                //        Tab = t.Tab
                //    }).ToList(),
                //Himall_FloorTablDetails = r.Himall_FloorTablDetails.Select(t => new FloorTablDetailsInfo()
                //{
                //    Id = t.Id,
                //    ProductId = t.ProductId,
                //    TabId = t.TabId
                //}).ToList(),
                //    Himall_MobileHomeProducts = r.Himall_MobileHomeProducts.Select(t => new MobileHomeProductsInfo()
                //    {
                //        Id = t.Id,
                //        PlatFormType = t.PlatFormType,
                //        ProductId = t.ProductId,
                //        Sequence = t.Sequence,
                //        ShopId = t.ShopId
                //    }).ToList(),
                //    Himall_ModuleProducts = r.Himall_ModuleProducts.Select(t => new ModuleProductInfo()
                //    {
                //        DisplaySequence = t.DisplaySequence,
                //        Id = t.Id,
                //        ModuleId = t.ModuleId,
                //        ProductId = t.ProductId
                //    }).ToList(),
                //    Himall_ProductBrokerage = r.Himall_ProductBrokerage.Select(t => new ProductBrokerageInfo()
                //    {
                //        Id = t.Id,
                //        AgentNum = t.AgentNum,
                //        BrokerageAmount = t.BrokerageAmount,
                //        BrokerageTotal = t.BrokerageTotal,
                //        CategoryName = t.CategoryName,
                //        CreateTime = t.CreateTime,
                //        ForwardNum = t.ForwardNum,
                //        ProductId = t.ProductId,
                //        rate = t.rate,
                //        saleAmount = t.saleAmount,
                //        SaleNum = t.SaleNum,
                //        ShopId = t.ShopId,
                //        Sort = t.Sort,
                //        Status = t.Status
                //    }).ToList(),
                //    Himall_ProductComments = r.Himall_ProductComments.Select(t => new ProductCommentInfo()
                //    {
                //        AppendContent = t.AppendContent,
                //        AppendDate = t.AppendDate,
                //        Email = t.Email,
                //        Id = t.Id,
                //        IsHidden = t.IsHidden,
                //        ProductId = t.ProductId,
                //        ReplyAppendContent = t.ReplyAppendContent,
                //        ReplyAppendDate = t.ReplyAppendDate,
                //        ReplyContent = t.ReplyContent,
                //        ReplyDate = t.ReplyDate,
                //        ReviewContent = t.ReviewContent,
                //        ReviewDate = t.ReviewDate,
                //        ShopId = t.ShopId,
                //        ShopName = t.ShopName,
                //        SubOrderId = t.SubOrderId,
                //        UserId = t.UserId,
                //        UserName = t.UserName
                //    }).ToList(),
                //    Himall_ProductShopCategories = r.Himall_ProductShopCategories.Select(t => new ProductShopCategoryInfo()
                //    {
                //        Id = t.Id,
                //        ProductId = t.ProductId,
                //        ShopCategoryId = t.ShopCategoryId
                //    }).ToList(),
                //    Himall_ProductVistis = r.Himall_ProductVistis.Select(t => new ProductVistiInfo()
                //    {
                //        Id = t.Id,
                //        Date = t.Date,
                //        OrderCounts = t.OrderCounts,
                //        ProductId = t.ProductId,
                //        SaleAmounts = t.SaleAmounts,
                //        SaleCounts = t.SaleCounts,
                //        VistiCounts = t.VistiCounts
                //    }).ToList(),
                //    Himall_ShopHomeModuleProducts = r.Himall_ShopHomeModuleProducts.Select(t => new ShopHomeModuleProductInfo()
                //    {
                //        DisplaySequence = t.DisplaySequence,
                //        HomeModuleId = t.HomeModuleId,
                //        Id = t.Id,
                //        ProductId = t.ProductId
                //    }).ToList(),
                //    Himall_ShoppingCarts = r.Himall_ShoppingCarts.Select(t => new ShoppingCartItemInfo()
                //    {
                //        Id = t.Id,
                //        AddTime = t.AddTime,
                //        ProductId = t.ProductId,
                //        Quantity = t.Quantity,
                //        SkuId = t.SkuId,
                //        UserId = t.UserId
                //    }).ToList(),
                //Himall_Shops = new ShopInfo() {  BankAccountName = r.Himall_Shops.BankAccountName, BankAccountNumber= r.Himall_Shops.BankAccountNumber, BankCode =r.Himall_Shops.BankCode, BankName=r.Himall_Shops.BankName, BankPhoto = r.Himall_Shops.BankPhoto, BankRegionId = r.Himall_Shops.BankRegionId,
                // BusinessLicenceEnd=r.Himall_Shops.BusinessLicenceEnd, BusinessLicenceNumber = r.Himall_Shops.BusinessLicenceNumber, BusinessLicenceNumberPhoto = r.Himall_Shops.BusinessLicenceNumberPhoto, BusinessLicenceRegionId = r.Himall_Shops.BusinessLicenceRegionId, BusinessLicenceStart = r.Himall_Shops.BusinessLicenceStart,
                // BusinessLicenseCert =r.Himall_Shops.BusinessLicenseCert, BusinessSphere= r.Himall_Shops.BusinessSphere, CompanyAddress = r.Himall_Shops.BusinessSphere, CompanyFoundingDate= r.Himall_Shops.CompanyFoundingDate, CompanyName = r.Himall_Shops.CompanyName, CompanyPhone = r.Himall_Shops.CompanyPhone, CompanyRegionId = r.Himall_Shops.CompanyRegionId,
                // CompanyRegionAddress = r.Himall_Shops.CompanyRegionAddress, CompanyRegisteredCapital = r.Himall_Shops.CompanyRegisteredCapital, ContactsEmail = r.Himall_Shops.ContactsEmail, ContactsName = r.Himall_Shops.ContactsName, ContactsPhone = r.Himall_Shops.ContactsPhone, CreateDate = r.Himall_Shops.CreateDate, EndDate = r.Himall_Shops.EndDate, FreeFreight = r.Himall_Shops.FreeFreight,
                // Freight =r.Himall_Shops.Freight, GradeId = r.Himall_Shops.GradeId, GeneralTaxpayerPhot = r.Himall_Shops.GeneralTaxpayerPhot, Id = r.Himall_Shops.Id, IsSelf=r.Himall_Shops.IsSelf, legalPerson=r.Himall_Shops.legalPerson}
                //}).ToList();
                #endregion
                result = result.Select(t => new ProductInfo()
                {
                    ImagePath = t.ImagePath,
                    ProductName = t.ProductName,
                    MinSalePrice = t.MinSalePrice,
                    Id = t.Id,
                    SaleCounts = t.SaleCounts
                }).ToList();
                Cache.Insert(CACHE_MANAGER_KEY, result, DateTime.Now.AddMinutes(5));
                return result.AsQueryable();
            }

        }
        #endregion

        public IQueryable<ProductInfo> GetPlatHotSaleProduct(int count = 3)
        {
            return Context.ProductInfo.Where(a => a.SaleStatus == Himall.Model.ProductInfo.ProductSaleStatus.OnSale && a.AuditStatus == ProductInfo.ProductAuditStatus.Audited).OrderByDescending(p => p.SaleCounts).Take(count);
        }

        #region 获取店铺最新上架的前N件商品
        public IQueryable<ProductInfo> GetNewSaleProduct(long shopId, int count = 5)
        {
            string CACHE_MANAGER_KEY = CacheKeyCollection.NewSaleProduct(shopId);
            if (Cache.Exists(CACHE_MANAGER_KEY))
            {
                return ((List<ProductInfo>)Core.Cache.Get(CACHE_MANAGER_KEY)).AsQueryable();
            }
            else
            {
                var data =
                                (from p in Context.ProductInfo
                                 where p.ShopId.Equals(shopId) && p.SaleStatus == Himall.Model.ProductInfo.ProductSaleStatus.OnSale && p.AuditStatus == ProductInfo.ProductAuditStatus.Audited && p.IsDeleted == false
                                 select p
                                ).OrderByDescending(p => p.AddedDate).Take(count);
                if (data.Count() == 0)
                    return null;
                //  var result = context.ProductInfo.Where(p => data.Any(d => d.Id.Equals(p.Id)));
                //Cache.Insert(CACHE_MANAGER_KEY, data.ToList(), DateTime.Now.AddMinutes(5));
                return data;
            }
        }
        #endregion

        #region 获取店铺最受关注的前N件商品
        public IQueryable<ProductInfo> GetHotConcernedProduct(long shopId, int count = 5)
        {
            string CACHE_MANAGER_KEY = CacheKeyCollection.HotConcernedProduct(shopId);
            if (Cache.Exists((CACHE_MANAGER_KEY)))
            {
                return ((List<ProductInfo>)Core.Cache.Get(CACHE_MANAGER_KEY)).AsQueryable().OrderByDescending(p => p.ConcernedCount);
            }
            else
            {
                var data = (
                    from p in Context.FavoriteInfo
                    where p.ProductInfo.ShopId == shopId
                    && p.ProductInfo.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                    && p.ProductInfo.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                    && p.ProductInfo.IsDeleted==false
                    group p by new
                    {
                        p.ProductInfo
                    } into g
                    select new
                    {
                        Product = g.Key.ProductInfo,
                        Count = g.Count()
                    }
                    ).OrderByDescending(c => c.Count).Take(count);
                foreach (var item in data)
                {
                    item.Product.ConcernedCount = item.Count;
                }
                var result = data.Select(c => c.Product).AsQueryable<ProductInfo>();
                //Cache.Insert(CACHE_MANAGER_KEY, result.ToList(), DateTime.Now.AddMinutes(5));
                return result;
                #region
                //var productsAudited = context.ProductInfo.Where(s => s.ShopId.Equals(shopId) && s.SaleStatus != ProductInfo.ProductSaleStatus.InDraft && s.AuditStatus == ProductInfo.ProductAuditStatus.Audited).ToList();

                //var products = (from p in productsAudited
                //                join f in favorites on p.Id equals f.ProductId
                //                where p.ShopId == shopId
                //                select new
                //                {
                //                    Id = p.Id,
                //                    Name = p.ProductName,
                //                    Price = p.MinSalePrice,
                //                    ImgPath = p.ImagePath
                //                }).ToList();
                //var data = (from v in products
                //            group v by new { v.Id, v.ImgPath, v.Name, v.Price } into G
                //            select new
                //            {
                //                Id = G.FirstOrDefault().Id,
                //                Name = G.FirstOrDefault().Name,
                //                Price = G.FirstOrDefault().Price,
                //                ImgPath = G.FirstOrDefault().ImgPath,
                //                Count = G.Count()
                //            }
                //            ).OrderByDescending(p => p.Count).Take(count).ToList();

                //if (data.Count() == 0) return null;
                //List<ProductInfo> result = new List<ProductInfo>();
                //foreach (var item in context.ProductInfo)
                //{
                //    if (data.Any(d => d.Id.Equals(item.Id)))
                //    {
                //        result.Add(item);
                //    }
                //}
                //foreach (var item in result)
                //{
                //    item.ConcernedCount = data.FirstOrDefault(d => d.Id.Equals(item.Id)).Count;
                //}
                //Cache.Insert(CACHE_MANAGER_KEY, result, DateTime.Now.AddMinutes(5));
                //Cache.Insert(CACHE_MANAGER_KEY, data, DateTime.Now.AddMinutes(5));
                //return result.AsQueryable().OrderByDescending(p => p.ConcernedCount);
                //return data;
                #endregion
            }
        }
        #endregion

        #region 获取用户关注的商品
        public ObsoletePageModel<FavoriteInfo> GetUserConcernProducts(long userId, int pageNo, int pageSize)
        {
            int total = 0;
            var favorite = Context.FavoriteInfo.FindBy(a => a.UserId == userId, pageNo, pageSize, out total, a => a.Date, false);
            ObsoletePageModel<FavoriteInfo> pageModel = new ObsoletePageModel<FavoriteInfo>()
            {
                Models = favorite,
                Total = total
            };
            return pageModel;
        }

        public List<FavoriteInfo> GetUserAllConcern(long userId)
        {
            var model = Context.FavoriteInfo.Where(p => p.UserId == userId).ToList();
            if (model == null)
            {
                return new List<FavoriteInfo>();
            }
            return model;
        }
        #endregion

        #region 取消用户关注的商品
        public void CancelConcernProducts(IEnumerable<long> ids, long userId)
        {
            Context.FavoriteInfo.Remove(a => a.UserId == userId && ids.Contains(a.Id));
            Context.SaveChanges();
        }

        public void DeleteFavorite(long productId, long userId)
        {
            Context.FavoriteInfo.Remove(item => item.UserId == userId && item.ProductId == productId);
            Context.SaveChanges();
        }
        #endregion

        #region 累加商品浏览次数 /*作废*/
        public void LogProductVisti(long productId)
        {
            /*
			var date = DateTime.Now;
			long shopId = 0;
			var productV = (from p in Context.ProductVistiInfo.Include("ProductInfo")
							where p.ProductId.Equals(productId)
							 && p.Date.Year.Equals(date.Year)
							 && p.Date.Month.Equals(date.Month)
							 && p.Date.Day.Equals(date.Day)
							select p).FirstOrDefault();

			if (null != productV && productV.ProductId.Equals(productId))
			{
				productV.VistiCounts += 1;
				shopId = productV.ProductInfo.ShopId;
			}
			else
			{
				shopId = Context.ProductInfo.FirstOrDefault(p => p.Id == productId).ShopId;
				Context.ProductVistiInfo.Add(new ProductVistiInfo
				{
					ProductId = productId,
					Date = DateTime.Now,
					VistiCounts = 1,
					SaleAmounts = 0,
					SaleCounts = 0
				});
			}



			ShopVistiInfo shopVisti = Context.ShopVistiInfo.FindBy(
					item => item.ShopId == shopId && item.Date.Year == date.Year
						&& item.Date.Month == date.Month && item.Date.Day == date.Day).FirstOrDefault();
			if (shopVisti == null)
			{
				shopVisti = new ShopVistiInfo();
				shopVisti.ShopId = shopId;
				shopVisti.Date = DateTime.Now.Date;
				Context.ShopVistiInfo.Add(shopVisti);
			}
			shopVisti.VistiCounts += 1;

			Context.SaveChanges();
			 * */
        }
        #endregion

        #region 我的收藏
        public void AddFavorite(long productId, long userId, out int status)
        {
            var fav = Context.FavoriteInfo.FirstOrDefault(f => f.ProductId.Equals(productId) && f.UserId.Equals(userId));
            if (fav != null && fav.ProductId == productId)
            {
                status = 1;
            }
            else
            {
                Context.FavoriteInfo.Add(new FavoriteInfo
                {
                    Date = DateTime.Now,
                    ProductId = productId,
                    UserId = userId,
                    Tags = ""
                });
                status = 0;
            }
            Context.SaveChanges();

        }

        public bool IsFavorite(long productId, long userId)
        {
            bool isFav = false;
            if (userId <= 0)
                throw new Himall.Core.HimallException("用户ID不存在！");
            var fav = Context.FavoriteInfo.FindBy(f => f.ProductId.Equals(productId) && f.UserId.Equals(userId)).Count();
            if (fav >= 1)
                isFav = true;
            return isFav;
        }
        #endregion
        #region 更新库存
        public void UpdateStock(string skuId, long stockChange)
        {
            var sku = Context.SKUInfo.FirstOrDefault(item => item.Id == skuId);
            if (sku != null)
            {
                sku.Stock += stockChange;
                if (sku.Stock < 0)
                    throw new HimallException("商品库存不足");
            }
            Context.SaveChanges();
        }
        public void SetSkusStock(IEnumerable<string> skuIds, IEnumerable<int> stock)
        {
            var skus = Context.SKUInfo.Where(e => skuIds.Any(s => s == e.Id));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            var i = 0;
            var stk = stock.ToArray();
            foreach (var sku in skus)
            {
                sku.Stock = stk[i];
                i++;
            }
            Context.SaveChanges();
        }
        public void SetMoreProductToOneStock(IEnumerable<long> pids, int stock)
        {
            var skus = Context.SKUInfo.Where(e => pids.Any(s => s == e.ProductId));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            foreach (var sku in skus)
            {
                sku.Stock = stock;
            }
            Context.SaveChanges();
        }
        public void AddSkuStock(IEnumerable<string> skuIds, IEnumerable<int> stock)
        {
            var skus = Context.SKUInfo.Where(e => skuIds.Any(s => s == e.Id));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            var i = 0;
            var stk = stock.ToArray();
            foreach (var sku in skus)
            {
                sku.Stock = sku.Stock + stk[i];
            }
            Context.SaveChanges();
        }
        public void AddProductStock(IEnumerable<long> pids, int stock)
        {
            var skus = Context.SKUInfo.Where(e => pids.Any(s => s == e.ProductId));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            foreach (var sku in skus)
            {
                sku.Stock = sku.Stock + stock;
            }
            Context.SaveChanges();
        }
        public void ReduceSkuStock(IEnumerable<string> skuIds, IEnumerable<int> stock)
        {
            var skus = Context.SKUInfo.Where(e => skuIds.Any(s => s == e.Id));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            var i = 0;
            var stk = stock.ToArray();
            foreach (var sku in skus)
            {
                sku.Stock = (sku.Stock - stk[i]) > 0 ? sku.Stock - stk[i] : 0;
            }
            Context.SaveChanges();
        }
        public void ReduceProductStock(IEnumerable<long> pids, int stock)
        {
            var skus = Context.SKUInfo.Where(e => pids.Any(s => s == e.ProductId));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            foreach (var sku in skus)
            {
                sku.Stock = (sku.Stock - stock) > 0 ? sku.Stock - stock : 0;
            }
            Context.SaveChanges();
        }
        #endregion

        #region 更新商品销售数量

        public void UpdateSalesCount(string skuId, int addSalesCount)
        {
            var product = Context.SKUInfo.FirstOrDefault(item => item.Id == skuId).ProductInfo;
            if (product != null)
            {
                product.SaleCounts += addSalesCount;
                Context.SaveChanges();
            }
        }

        #endregion

        #region 添加浏览记录
        public void AddBrowsingProduct(BrowsingHistoryInfo info)
        {
            var model = Context.BrowsingHistoryInfo.FirstOrDefault(a => a.ProductId == info.ProductId && a.MemberId == info.MemberId);
            if (model == null)
            {
                if (Context.BrowsingHistoryInfo.Count(a => a.MemberId == info.MemberId) < 20)
                {
                    Context.BrowsingHistoryInfo.Add(info);
                }
                else
                {
                    var remove = Context.BrowsingHistoryInfo.Where(a => a.MemberId == info.MemberId).OrderBy(a => a.BrowseTime).FirstOrDefault();
                    Context.BrowsingHistoryInfo.Remove(remove);
                    Context.BrowsingHistoryInfo.Add(info);
                }
            }
            else
            {
                model.BrowseTime = info.BrowseTime;
            }
            Context.SaveChanges();
        }
        #endregion

        #region 获取用户浏览记录
        public IQueryable<BrowsingHistoryInfo> GetBrowsingProducts(long userId)
        {
            var list = from p in Context.BrowsingHistoryInfo
                       .Include("Himall_Products")
                       where p.MemberId == userId
                       select p;
            return list;
        }
        #endregion


        public ProductVistiInfo GetProductVistInfo(long pId, ICollection<ProductVistiInfo> pInfo = null)
        {
            var result = new ProductVistiInfo();
            if (pInfo == null)
            {
                var xinfo = Context.ProductVistiInfo.FirstOrDefault(v => v.ProductId == pId);
                if (xinfo != null)
                {
                    result.ProductId = pId;
                    result.SaleAmounts = xinfo.SaleAmounts;
                    result.SaleCounts = xinfo.SaleCounts;
                    result.Date = DateTime.Now;
                }
            }
            else
            {
                var v = (from pv in pInfo
                         group pv by pv.ProductId into G
                         select new
                         {
                             SaleCount = G.Sum(v1 => v1.SaleCounts),
                             SaleAmounts = G.Sum(v1 => v1.SaleAmounts),
                             ProductId = G.Key
                         }).FirstOrDefault();
                if (null != v && v.ProductId == pId)
                {
                    result.ProductId = pId;
                    result.SaleAmounts = v.SaleAmounts;
                    result.SaleCounts = v.SaleCount;
                    result.Date = DateTime.Now;
                }
            }
            return result;
        }

        #region 获取运费

        public decimal GetFreight(IEnumerable<long> productIds, IEnumerable<int> counts, int cityId)
        {
            return 0;
            //CheckWhenGetFreight(productIds, counts, cityId);
            //decimal freight = 0;
            //productIds = productIds.ToArray();
            //counts = counts.ToArray();

            //IProductService service = ServiceProvider.Instance<IProductService>.Create;
            //IFreightTemplateService ftservice = ServiceProvider.Instance<IFreightTemplateService>.Create;
            //IRegionService regionService = ServiceProvider.Instance<IRegionService>.Create;
            //int i = 0;
            //var products = productIds.Select(productId =>
            //{
            //    var product = service.GetProductCache(productId);
            //    return new
            //    {
            //        Product = product,
            //        Quantity = counts.ElementAt(i++),
            //    };

            //}).ToList();

            //var freightProductGroup = products.GroupBy(item => item.Product.FreightTemplateId);//根据运费模版将产品分组
            //var region = regionService.GetRegion(cityId);
            //foreach (var freightProduct in freightProductGroup)
            //{
            //    FreightTemplateInfo template = ftservice.GetFreightTemplate(freightProduct.Key);
            //    var freightAreas = template.Himall_FreightAreaContent.ToList();
            //    var freightAreaDetail = ftservice.GetFreightAreaDetail(freightProduct.Key).ToList();
            //    if (template != null && region != null)
            //    {
            //        if (template.IsFree == FreightTemplateType.SelfDefine)//是否包邮
            //        {
            //            //  FreightAreaContentInfo freightAreaContent = template.Himall_FreightAreaContent.Where(item => item.AreaContent.Split(',').Contains(cityId.ToString()) == true).FirstOrDefault();

            //            FreightAreaContentInfo freightAreaContent = null;
            //            FreightAreaDetailInfo detail = null;
            //            //从最后一级乡镇查找
            //            if (region.Level == Region.RegionLevel.Town)//如果传过来ID是区级的话
            //            {
            //                detail = freightAreaDetail.Where(a => a.TownIds != "" && a.TownIds != null && a.TownIds.Split(',').Contains(cityId.ToString())).FirstOrDefault();
            //            }
            //            var pid = regionService.GetRegion(region.Id, Region.RegionLevel.Province).Id;//省级
            //            var cid = regionService.GetRegion(region.Id, Region.RegionLevel.City).Id;//市级          

            //            if (detail == null)
            //            {
            //                if (region.Level == Region.RegionLevel.County || region.Level == Region.RegionLevel.Town)
            //                {
            //                    var countyId = regionService.GetRegion(region.Id, Region.RegionLevel.County).Id;//区级
            //                    detail = freightAreaDetail.Where(a => a.CountyId == countyId && (a.TownIds == "" || a.TownIds == null)).FirstOrDefault();
            //                }
            //            }
            //            if (detail == null)
            //            {
            //                detail = freightAreaDetail.Where(a => a.CityId == cid && (!a.CountyId.HasValue || a.CountyId.Value == 0)).FirstOrDefault();
            //            }
            //            if (detail == null)
            //            {
            //                detail = freightAreaDetail.Where(a => a.ProvinceId == pid && (!a.CityId.HasValue || a.CityId.Value == 0)).FirstOrDefault();
            //            }
            //            if (detail != null)
            //            {
            //                freightAreaContent = freightAreas.Where(a => a.Id == detail.FreightAreaId).FirstOrDefault();
            //            }

            //            if (freightAreaContent == null)
            //            {
            //                //配送地址不包含当前城市则使用默认运费规则
            //                freightAreaContent = freightAreas.Where(item => item.IsDefault == 1).FirstOrDefault();
            //            }

            //            if (template.ValuationMethod == ValuationMethodType.Weight)//按重量
            //            {
            //                decimal weight = freightProduct.Sum(item => item.Product.Weight != null ? ((decimal)item.Product.Weight * item.Quantity) : 0); //总重量
            //                freight += GetFreight2(weight, (int)freightAreaContent.FirstUnit, (decimal)freightAreaContent.FirstUnitMonry, (int)freightAreaContent.AccumulationUnit, (decimal)freightAreaContent.AccumulationUnitMoney);
            //            }
            //            else if (template.ValuationMethod == ValuationMethodType.Bulk)//按体积
            //            {
            //                decimal volume = freightProduct.Sum(item => item.Product.Volume != null ? ((decimal)item.Product.Volume * item.Quantity) : 0);//总体积
            //                freight += GetFreight2(volume, (int)freightAreaContent.FirstUnit, (decimal)freightAreaContent.FirstUnitMonry, (int)freightAreaContent.AccumulationUnit, (decimal)freightAreaContent.AccumulationUnitMoney);
            //            }
            //            else //按数量
            //            {
            //                int count = freightProduct.Sum(item => item.Quantity);//总数量
            //                freight += GetFreight2(count, (int)freightAreaContent.FirstUnit, (decimal)freightAreaContent.FirstUnitMonry, (int)freightAreaContent.AccumulationUnit, (decimal)freightAreaContent.AccumulationUnitMoney);
            //            }
            //        }

            //    }
            //}

            //return freight;
        }

        /// <summary>
        /// 获取运费（超出部分 超出多少个按多少个算）
        /// </summary>
        /// <param name="count">总数量/重量/体积</param>
        /// <param name="firstUnit">首件/重/体积</param>
        /// <param name="firstUnitMonry">首费</param>
        /// <param name="accumulationUnit">续件/重/体积</param>
        /// <param name="accumulationUnitMoney">续费</param>
        /// <returns></returns>
        decimal GetFreight1(decimal count, int firstUnit, decimal firstUnitMonry, int accumulationUnit, decimal accumulationUnitMoney)
        {
            decimal freight = 0;
            if (count <= firstUnit)
            {
                freight = firstUnitMonry;
            }
            else
            {
                decimal size = (count - firstUnit) / accumulationUnit;  //续件个数
                freight = firstUnitMonry + (size * accumulationUnitMoney);
            }
            return freight;
        }

        /// <summary>
        /// 获取运费（超出部分 不足1个按1个算）
        /// </summary>
        /// <param name="count">总数量/重量/体积</param>
        /// <param name="firstUnit">首件/重/体积</param>
        /// <param name="firstUnitMonry">首费</param>
        /// <param name="accumulationUnit">续件/重/体积</param>
        /// <param name="accumulationUnitMoney">续费</param>
        /// <returns></returns>
        decimal GetFreight2(decimal count, int firstUnit, decimal firstUnitMonry, int accumulationUnit, decimal accumulationUnitMoney)
        {
            decimal freight = 0;
            if (count <= firstUnit)
            {
                freight = firstUnitMonry;
            }
            else
            {
                decimal size = (count - firstUnit) / accumulationUnit;  //续件个数
                decimal prefix = Math.Truncate(size); //续件个数整数部分
                decimal suffix = size - prefix; //续件个数小数部分
                decimal p1 = prefix * accumulationUnitMoney; //续件个数整数部分金额
                decimal p2 = 0;
                if (suffix > 0)
                    p2 = 1 * accumulationUnitMoney; //续件个数小数部分金额 按超过1个算

                freight = firstUnitMonry + p1 + p2;
            }
            return freight;
        }

        void CheckWhenGetFreight(IEnumerable<string> skuIds, IEnumerable<int> counts, int cityId)
        {
            if (skuIds == null || skuIds.Count() == 0)
                throw new InvalidPropertyException("待计算运费的商品不能这空");
            if (counts == null || counts.Count() == 0)
                throw new InvalidPropertyException("待计算运费的商品数量不能这空");
            if (counts.Count(item => item <= 0) > 0)
                throw new InvalidPropertyException("待计算运费的商品数量必须都大于0");
            if (skuIds.Count() != counts.Count())
                throw new InvalidPropertyException("商品数量不一致");
            if (cityId <= 0)
                throw new InvalidPropertyException("收货地址无效");


            var productService = ServiceProvider.Instance<IProductService>.Create;
            for (var i = 0; i < skuIds.Count(); i++)
            {
                var sku = productService.GetSku(skuIds.ElementAt(i));
                if (sku == null)
                    throw new HimallException("未找到" + sku + "对应的商品");
            }
        }

        void CheckWhenGetFreight(IEnumerable<long> productIds, IEnumerable<int> counts, int cityId)
        {
            if (productIds == null || productIds.Count() == 0)
                throw new InvalidPropertyException("待计算运费的商品不能这空");
            if (counts == null || counts.Count() == 0)
                throw new InvalidPropertyException("待计算运费的商品数量不能这空");
            if (counts.Count(item => item <= 0) > 0)
                throw new InvalidPropertyException("待计算运费的商品数量必须都大于0");
            if (productIds.Count() != counts.Count())
                throw new InvalidPropertyException("商品数量不一致");
            if (cityId <= 0)
                throw new InvalidPropertyException("收货地址无效");


            //var productService = ServiceProvider.Instance<IProductService>.Create;
            //for (var i = 0; i < productIds.Count(); i++)
            //{
            //    var product = productService.GetProduct(productIds.ElementAt(i));
            //    if (product == null)
            //        throw new HimallException("未找到" + product.ProductName + "对应的商品");
            //    //if (sku.Stock < counts.ElementAt(i))
            //    //{
            //    //    var product = productService.GetProduct(sku.ProductId);
            //    //    throw new HimallException("商品“" + product.ProductName + "”库存不够，仅剩" + sku.Stock + "件");
            //    //}
            //}
        }

        public List<decimal> GetFreights(IEnumerable<string> skuIds, IEnumerable<int> counts, int cityId)
        {
            CheckWhenGetFreight(skuIds, counts, cityId);
            List<decimal> freights = new List<decimal>();
            decimal freight = 0;
            skuIds = skuIds.ToArray();
            counts = counts.ToArray();

            IProductService service = ServiceProvider.Instance<IProductService>.Create;
            IFreightTemplateService ftservice = ServiceProvider.Instance<IFreightTemplateService>.Create;
            int i = 0;
            var products = skuIds.Select(skuId =>
            {
                var skuInfo = service.GetSku(skuId);
                var product = service.GetProduct(skuInfo.ProductId);
                return new
                {
                    SKU = skuInfo,
                    Product = product,
                    Quantity = counts.ElementAt(i++),
                };
            }
            ).ToList();

            var freightProductGroup = products.GroupBy(item => item.Product.FreightTemplateId);//根据运费模版将产品分组
            foreach (var freightProduct in freightProductGroup)
            {
                FreightTemplateInfo template = ftservice.GetFreightTemplate(freightProduct.Key);
                if (template != null)
                {
                    if (template.IsFree == FreightTemplateType.SelfDefine)//是否包邮
                    {

                        FreightAreaContentInfo freightAreaContent = template.Himall_FreightAreaContent.Where(item => item.AreaContent.Split(',').Contains(cityId.ToString()) == true).FirstOrDefault();
                        if (freightAreaContent == null)
                        {
                            //配送地址不包含当前城市则使用默认运费规则
                            freightAreaContent = template.Himall_FreightAreaContent.Where(item => item.IsDefault == 1).FirstOrDefault();
                        }
                        if (template.ValuationMethod == ValuationMethodType.Weight)//按重量
                        {
                            decimal weight = freightProduct.Sum(item => item.Product.Weight != null ? ((decimal)item.Product.Weight * item.Quantity) : 0); //总重量
                            freight = GetFreight2(weight, (int)freightAreaContent.FirstUnit, (decimal)freightAreaContent.FirstUnitMonry, (int)freightAreaContent.AccumulationUnit, (decimal)freightAreaContent.AccumulationUnitMoney);
                        }
                        else if (template.ValuationMethod == ValuationMethodType.Bulk)//按体积
                        {
                            decimal volume = freightProduct.Sum(item => item.Product.Volume != null ? ((decimal)item.Product.Volume * item.Quantity) : 0);//总体积
                            freight = GetFreight2(volume, (int)freightAreaContent.FirstUnit, (decimal)freightAreaContent.FirstUnitMonry, (int)freightAreaContent.AccumulationUnit, (decimal)freightAreaContent.AccumulationUnitMoney);
                        }
                        else //按数量
                        {
                            int count = freightProduct.Sum(item => item.Quantity);//总数量
                            freight = GetFreight2(count, (int)freightAreaContent.FirstUnit, (decimal)freightAreaContent.FirstUnitMonry, (int)freightAreaContent.AccumulationUnit, (decimal)freightAreaContent.AccumulationUnitMoney);
                        }
                    }

                }
                freights.Add(freight);
            }

            return freights;
        }

        #endregion


        public IQueryable<ProductInfo> GetPlatHotSaleProductByNearShop(int count, long userId, bool isRecommend = false)
        {
            IQueryable<ProductInfo> result;
            var order = Context.OrderInfo.Where(c => c.UserId == userId).OrderByDescending(c => c.Id).FirstOrDefault();
            if (order == null)
            {
                if (isRecommend)
                {
                    result = Context.ProductInfo.Where(c => c.IsDeleted == false)
                .OrderByDescending(c => c.SaleCounts).Take(count);
                }
                else
                {
                    result = Context.ProductInfo.Where(c => 1 == 2);
                }
                return result;
            }
            result = Context.ProductInfo.Where(c => c.ShopId == order.ShopId
                && c.IsDeleted == false)
                .OrderByDescending(c => c.SaleCounts).Take(count);
            return result;
        }

        /// <summary>
        /// 取数据修改状态
        /// <para>不包含图片修改判断</para>
        /// </summary>
        /// <param name="id">用于比对的商品编号</param>
        /// <param name="model">当前商品数据</param>
        /// <returns></returns>
        /// Add:DZY[150714]
        public ProductInfo.ProductEditStatus GetEditStatus(long id, ProductInfo model)
        {
            ProductInfo.ProductEditStatus result = ProductInfo.ProductEditStatus.PendingAudit;
            var data = Context.ProductInfo.FirstOrDefault(d => d.Id == id);
            if (data != null)
            {

                result = (ProductInfo.ProductEditStatus)data.EditStatus;

                #region 已修改待审核
                if (data.ProductName != model.ProductName)  //标题
                {
                    if ((int)result > 3)
                    {
                        result = ProductInfo.ProductEditStatus.CompelPendingHasEdited;
                    }
                    else
                    {
                        result = ProductInfo.ProductEditStatus.EditedAndPending;
                    }
                }
                if (data.ProductDescriptionInfo.Description != model.ProductDescriptionInfo.Description)  //描述
                {
                    if ((int)result > 3)
                    {
                        result = ProductInfo.ProductEditStatus.CompelPendingHasEdited;
                    }
                    else
                    {
                        result = ProductInfo.ProductEditStatus.EditedAndPending;
                    }
                }
                //TODO:DZY[150729] 移动端描述需要审核
                /* zjt  
				 * TODO可移除，保留注释即可
				 */
                if (data.ProductDescriptionInfo.MobileDescription != model.ProductDescriptionInfo.MobileDescription)  //移动端描述
                {
                    if ((int)result > 3)
                    {
                        result = ProductInfo.ProductEditStatus.CompelPendingHasEdited;
                    }
                    else
                    {
                        result = ProductInfo.ProductEditStatus.EditedAndPending;
                    }
                }
                if (data.ShortDescription != model.ShortDescription)  //广告语
                {
                    if ((int)result > 3)
                    {
                        result = ProductInfo.ProductEditStatus.CompelPendingHasEdited;
                    }
                    else
                    {
                        result = ProductInfo.ProductEditStatus.EditedAndPending;
                    }
                }
                #endregion

            }
            return result;
        }

        public IQueryable<SKUInfo> GetSKUs(IEnumerable<long> productIds)
        {
            return Context.SKUInfo.FindBy(s => productIds.Contains(s.ProductId));
        }

        /// <summary>
        /// 根据sku id 获取sku信息
        /// </summary>
        /// <param name="skuIds"></param>
        /// <returns></returns>
        public List<SKUInfo> GetSKUs(IEnumerable<string> skuIds)
        {
            string id = string.Join(",", skuIds);
            string sql = "select * from himall_skus where Id in @SkuId";
            List<SKUInfo> list = new List<SKUInfo>();
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                list = conn.Query<SKUInfo>(sql, new { SkuId = skuIds.ToArray() }).ToList();
            }
            return list;
        }

        /// <summary>
        /// 是否为限时购商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsLimitBuy(long id)
        {
            bool result = false;
            var now = DateTime.Now;
            result = Context.FlashSaleInfo.Any(m => m.ProductId == id && m.Status == FlashSaleInfo.FlashSaleStatus.Ongoing && m.BeginDate <= now
                && m.EndDate > now);
            return result;
        }

        /// <summary>
        /// 修改推荐商品
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="relationProductIds"></param>
        public void UpdateRelationProduct(long productId, string relationProductIds)
        {
            if (relationProductIds == null)
                relationProductIds = "";

            var data = this.Context.ProductRelationProductInfo.FirstOrDefault(p => p.ProductId == productId);

            if (data == null)
            {
                this.Context.ProductRelationProductInfo.Add(new ProductRelationProductInfo
                {
                    ProductId = productId,
                    Relation = relationProductIds
                });
            }
            else
                data.Relation = relationProductIds;

            this.Context.SaveChanges();
        }

        /// <summary>
        /// 获取商品的推荐商品
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public ProductRelationProductInfo GetRelationProductByProductId(long productId)
        {
            return this.Context.ProductRelationProductInfo.FirstOrDefault(p => p.ProductId == productId);
        }

        /// <summary>
		/// 获取商品所有状态的推荐商品
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public List<ProductRelationProductInfo> GetRelationProductByProductIds(IEnumerable<long> productIds)
        {
            return this.Context.ProductRelationProductInfo.Where(p => productIds.Contains(p.ProductId)).ToList();
        }

        /// <summary>
        /// 获取指定类型下面热销的前N件商品
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<ProductInfo> GetHotSaleProductByCategoryId(int categoryId, int count)
        {
            return this.Context.ProductInfo.Where(p => p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && p.AuditStatus == ProductInfo.ProductAuditStatus.Audited && p.CategoryId == categoryId && p.IsDeleted == false)
                .OrderByDescending(p => p.SaleCounts).Take(count).ToList();
        }

        #region 私有方法
        /// <summary>
        /// 设置sku id，转移sku图片等
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="productId"></param>
        /// <param name="skus"></param>
        private void ProcessSKU(long shopId, long productId, SKUInfo[] skus)
        {
            var skuInfoShowPics = new Dictionary<string, string>();
            foreach (var item in skus)
            {
                item.Id = string.Format(item.Id, productId);
                item.AutoId = item.Id.GetHashCode();
                item.ProductId = productId;
                if (!string.IsNullOrWhiteSpace(item.ShowPic))
                {
                    if (!skuInfoShowPics.ContainsKey(item.ShowPic))
                    {
                        var temp = this.SKUImageToStorage(shopId, productId, item.Id, item.ShowPic);
                        skuInfoShowPics.Add(item.ShowPic, temp);//处理重复图片路径
                        item.ShowPic = temp;
                    }
                    else
                        item.ShowPic = skuInfoShowPics[item.ShowPic];
                }
            }
        }

        private string ProductImageToStorageAndCreateThumbnail(long shopId, long productId, string[] paths)
        {
            if (paths == null || paths.Length == 0)
                throw new ArgumentNullException("paths");

            var destFileName = string.Format("/Storage/Shop/{0}/Products/{1}", shopId, productId);

            for (int i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                var dest = string.Format("{0}/{1}.png", destFileName, i + 1);
                if (string.IsNullOrWhiteSpace(path) || path.Contains(destFileName))
                {
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        try
                        {
                            Core.HimallIO.DeleteFile(dest);
                        }
                        catch
                        { }
                    }
                    continue;
                }

                try
                {
                    Himall.Core.HimallIO.CopyFile(path, dest, true);

                    var imageSizes = EnumHelper.ToDictionary<ImageSize>().Select(t => t.Key);

                    foreach (var imageSize in imageSizes)
                    {
                        string size = string.Format("{0}/{1}_{2}.png", destFileName, i + 1, imageSize);
                        Core.HimallIO.CreateThumbnail(dest, size, imageSize, imageSize);
                    }
                }
                catch (FileNotFoundException fex)
                {
                    Core.Log.Error("没有找到文件", fex);
                }
                catch (System.Runtime.InteropServices.ExternalException eex)
                {
                    Core.Log.Error("ExternalException异常", eex);
                }
                catch (Exception ex)
                {
                    Core.Log.Error("Exception异常", ex);
                }
            }

            return destFileName;
        }

        private string SKUImageToStorage(long shopId, long productId, string skuId, string path)
        {
            if (string.IsNullOrWhiteSpace(skuId))
                throw new ArgumentNullException("skuId");

            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            var destFileName = string.Format("/Storage/Shop/{0}/Products/{1}/skus/{2}.png", shopId, productId, skuId);

            if (path.EndsWith(destFileName))
                return destFileName;

            try
            {
                Core.HimallIO.CopyFile(path, destFileName, true);
            }
            catch (Exception e)
            {
                Core.Log.Error("Exception异常", e);
            }

            return destFileName;
        }
        #endregion

        #region IProductService 成员


        public void SetProductOverSafeStock(IEnumerable<long> pids, long stock)
        {
            var skus = Context.SKUInfo.Where(e => pids.Contains(e.ProductId));
            foreach (var sku in skus)
            {
                sku.SafeStock = stock;
            }
            Context.SaveChanges();
        }

        #endregion

        #region 门店首页商品列表
        public QueryPageModel<ProductInfo> GetStoreHomeProducts(ProductQuery productQueryModel)
        {
            int total;
            var products = GetStoreHomeProductsByQueryModel(productQueryModel, out total);

            QueryPageModel<ProductInfo> pageModel = new QueryPageModel<ProductInfo>()
            {
                Models = products.ToList(),
                Total = total
            };
            return pageModel;
        }
        IQueryable<ProductInfo> GetStoreHomeProductsByQueryModel(ProductQuery productQueryModel, out int total)
        {
            //var products = Context.ProductInfo.Where(item => true);
            var products = Context.ProductInfo.Where(item => item.IsDeleted==false);//过滤删除了的商品
            if (productQueryModel.ShopId.HasValue)//过滤商家
                products = products.Where(item => item.ShopId == productQueryModel.ShopId);

            if (productQueryModel.ShopCategoryId.HasValue && productQueryModel.ShopId.HasValue)//过滤商家分类，包括下面子类
            {
                var shopCategoryId = productQueryModel.ShopCategoryId.Value;
                var shopId = productQueryModel.ShopId.Value;
                var shopCategoryIds = Context.ShopCategoryInfo.Where(p => p.ShopId == shopId && (p.Id == shopCategoryId || p.ParentCategoryId == shopCategoryId)).Select(p => p.Id);
                var productIds = Context.ProductShopCategoryInfo.Where(p => shopCategoryIds.Contains(p.ShopCategoryId)).Select(p => p.ProductId);
                products = products.Where(p => productIds.Contains(p.Id));
            }

            if (productQueryModel.ShopBranchId.HasValue)//过滤出商家下门店商品
            {
                var shopBranchProducts = this.Context.ShopBranchSkusInfo.Where(p => p.ShopId == productQueryModel.ShopId && p.ShopBranchId == productQueryModel.ShopBranchId)
                   .GroupBy(p => new { p.ProductId })
                   .GroupBy(p => p.Key.ProductId)
                   .Select(p => p.Key);

                products = products.Where(p => shopBranchProducts.Contains(p.Id));
            }

            return products.GetPage(out total, productQueryModel.PageNo, productQueryModel.PageSize);
        }
        #endregion
    }
}
