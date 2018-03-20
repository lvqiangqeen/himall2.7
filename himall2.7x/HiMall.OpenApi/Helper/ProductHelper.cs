using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Himall.Core;
using Himall.Model;
using Himall.IServices;
using Himall.OpenApi.Model;
using Hishop.Open.Api;
using Himall.CommonModel;

namespace Himall.OpenApi
{
    /// <summary>
    /// 商品辅助工具
    /// </summary>
    public class ProductHelper
    {
        private IProductService _iProductService;
        private IRegionService _iRegionService;
        private long shopId = 0;
        private ShopHelper _shophelper;
        private ITypeService _iTypeService;
        public ProductHelper()
        {
            _iProductService = Himall.ServiceProvider.Instance<IProductService>.Create;
            _iRegionService = Himall.ServiceProvider.Instance<IRegionService>.Create;
            _iTypeService = Himall.ServiceProvider.Instance<ITypeService>.Create;
        }

        /// <summary>
        /// 获取指定商品的详情信息
        /// </summary>
        /// <param name="num_iid"></param>
        /// <param name="app_key"></param>
        /// <returns></returns>
        public product_item_model GetProduct(int num_iid, string app_key)
        {
            InitShopInfo(app_key);

            long proid = num_iid;
            var prodata = _iProductService.GetProduct(proid);
            if(prodata.ShopId!=shopId)
            {
                throw new HimallOpenApiException(OpenApiErrorCode.Invalid_App_Key, "app_key");
            }
            var result = ProductInfoMapChange(prodata);
            return result;
        }

        /// <summary>
        /// 获取当前商家的商品列表
        /// </summary>
        /// <param name="start_modified"></param>
        /// <param name="end_modified"></param>
        /// <param name="approve_status"></param>
        /// <param name="q"></param>
        /// <param name="order_by"></param>
        /// <param name="page_no"></param>
        /// <param name="page_size"></param>
        /// <param name="app_key"></param>
        /// <returns></returns>
        public ObsoletePageModel<product_list_model> GetSoldProducts(DateTime? start_modified, DateTime? end_modified, string approve_status, string q, string order_by, int page_no, int page_size, string app_key)
        {
            InitShopInfo(app_key);

            ObsoletePageModel<product_list_model> result = new ObsoletePageModel<product_list_model>()
            {
                Models = null,
                Total = 0
            };
            List<product_list_model> resultdata = new List<product_list_model>();

            #region 构建查询条件
            Himall.IServices.QueryModel.ProductQuery pq = new IServices.QueryModel.ProductQuery()
            {
                PageSize = page_size,
                PageNo = page_no,
                KeyWords = q
            };
            pq.ShopId = shopId;
            if (start_modified != null)
            {
                pq.StartDate = start_modified;
            }
            if (end_modified != null)
            {
                pq.EndDate = end_modified;
            }
            ProductStatus queryps = ProductStatus.In_Stock;
            if (!string.IsNullOrWhiteSpace(approve_status))
            {
                if (Enum.TryParse(approve_status, true, out queryps))
                {
                    switch (queryps)
                    {
                        case ProductStatus.In_Stock:
                            pq.SaleStatus = ProductInfo.ProductSaleStatus.InStock;
                            break;
                        case ProductStatus.On_Sale:
                            pq.SaleStatus = ProductInfo.ProductSaleStatus.OnSale;
							pq.AuditStatus = new[] { ProductInfo.ProductAuditStatus.Audited };
                            break;
                        case ProductStatus.Un_Sale:
							pq.AuditStatus = new[] { ProductInfo.ProductAuditStatus.WaitForAuditing ,ProductInfo.ProductAuditStatus.InfractionSaleOff};
                            break;
                        default:
                            throw new HimallOpenApiException(OpenApiErrorCode.Product_ApproveStatus_Faild, "approve_status");
                            break;
                    }
                }
                else
                {
                    throw new HimallOpenApiException(OpenApiErrorCode.Product_ApproveStatus_Faild, "approve_status");
                }
            }
            if (!string.IsNullOrWhiteSpace(order_by))
            {
                bool _orderasc = true;
                if (order_by.IndexOf(":desc") > 0)
                {
                    _orderasc = false;
                }
                order_by = order_by.Split(':')[0];
                pq.OrderKey = 1;
                pq.OrderType = _orderasc;
                switch (order_by)
                {
                    case "create_time":
                        pq.OrderKey = 2;
                        break;
                    case "sold_quantity":
                        pq.OrderKey = 3;
                        break;
                }
            }
            #endregion

            var proqlist = _iProductService.GetProducts(pq);
            result.Total = proqlist.Total;
            if (proqlist.Total > 0)
            {
                var datalist = proqlist.Models.ToList();
                resultdata = ProductInfoListMapChange(datalist);
                result.Models = resultdata.AsQueryable();
            }
            return result;
        }

        /// <summary>
        /// 商品/SKU库存修改(提供按照全量或增量形式修改宝贝/SKU库存
        /// </summary>
        /// <param name="num_iid"></param>
        /// <param name="sku_id"></param>
        /// <param name="quantity"></param>
        /// <param name="type"></param>
        /// <param name="app_key"></param>
        /// <returns></returns>
        public product_item_model UpdateProductQuantity(int num_iid, string sku_id, int quantity, int type, string app_key)
        {
            long proid = num_iid;
            var prodata = _iProductService.GetProduct(proid);
            if (prodata == null)
            {
                throw new HimallOpenApiException(OpenApiErrorCode.Invalid_Arguments, "num_iid");
            }
            InitShopInfo(app_key);
            if (prodata.ShopId != shopId)
            {
                throw new HimallOpenApiException(OpenApiErrorCode.Invalid_App_Key, "app_key");
            }
            SKUInfo skudata;
            if (!string.IsNullOrWhiteSpace(sku_id))
            {
                skudata = _iProductService.GetSku(sku_id);
                long oldstock = skudata.Stock;
                long nowstock = GetStockQuantity(oldstock, quantity, type);
                _iProductService.UpdateStock(sku_id, nowstock - oldstock);   //只有增量更新接口
            }
            else
            {
                if (prodata.SKUInfo.Count > 0)
                {
                    foreach (var item in prodata.SKUInfo)
                    {
                        item.Stock = GetStockQuantity(item.Stock, quantity, type);
                    }
                    _iProductService.UpdateProduct(prodata);   //直接修改整个商品数据
                }
                else
                {
                    if (!prodata.Quantity.HasValue)
                    {
                        prodata.Quantity = 0;
                    }
                    //不做操作
                    //prodata.Quantity = GetStockQuantity(prodata.Quantity.Value, quantity, type);
                }
            }
            //获取商品新数据
            prodata = _iProductService.GetProduct(proid);
            var result = ProductInfoMapChange(prodata);
            return result;
        }

        /// <summary>
        /// 修改商品销售状态 (上架， 下架， 入库)
        /// </summary>
        /// <param name="num_iid"></param>
        /// <param name="approve_status"></param>
        /// <param name="app_key"></param>
        /// <returns></returns>
        public product_item_model UpdateProductApproveStatus(int num_iid, string approve_status, string app_key)
        {
            if (string.IsNullOrWhiteSpace(approve_status))
            {
                throw new HimallOpenApiException(OpenApiErrorCode.Product_ApproveStatus_Faild, "approve_status");
            }
            InitShopInfo(app_key);
            ProductStatus dataps = ProductStatus.In_Stock;
            if (Enum.TryParse(approve_status, true, out dataps))
            {
                try {
                    switch (dataps)
                    {
                        case ProductStatus.In_Stock:
                           
                            _iProductService.SaleOff(num_iid, shopId);
                            break;
                        case ProductStatus.On_Sale:
                            _iProductService.OnSale(num_iid, shopId);
                            break;
                        case ProductStatus.Un_Sale:
                            _iProductService.SaleOff(num_iid, shopId);
                            break;
                    }
                }catch(Exception ex)
                {
                    throw new HimallOpenApiException(OpenApiErrorCode.Product_Status_is_Invalid, "approve_status");
                }
            }
            else
            {
                throw new HimallOpenApiException(OpenApiErrorCode.Product_Status_is_Invalid, "approve_status");
            }

            long proid = num_iid;
            var prodata = _iProductService.GetProduct(proid);
            var result = ProductInfoMapChange(prodata);
            return result;
        }

        #region 私有
        /// <summary>
        /// 初始获取店铺信息
        /// </summary>
        /// <param name="app_key"></param>
        private void InitShopInfo(string app_key)
        {
            _shophelper = new ShopHelper(app_key);
            shopId = _shophelper.ShopId;
        }
        /// <summary>
        /// 商品信息转换
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private product_item_model ProductInfoMapChange(ProductInfo data)
        {
            var prodata = data;
            if (prodata == null)
            {
                throw new HimallOpenApiException(OpenApiErrorCode.Product_Not_Exists, "num_iid");
            }
            product_item_model result = new product_item_model();

            #region 装配信息
            result.num_iid = (int)prodata.Id;
            result.outer_id = prodata.ProductCode;
            result.brand_id = (int)prodata.BrandId;
            result.brand_name = prodata.BrandName;
            result.cid = (int)prodata.CategoryId;
            result.type_id = (int)prodata.TypeId;
            if (prodata.Himall_Categories != null)
            {
                result.cat_name = prodata.Himall_Categories.Name;
                if (prodata.Himall_Categories.ProductTypeInfo != null)
                {
                    result.type_name = prodata.Himall_Categories.ProductTypeInfo.Name;
                }
            }
            result.title = prodata.ProductName.Trim();
            result.list_time = prodata.AddedDate;
            result.modified = prodata.AddedDate;
            result.display_sequence = (int)prodata.DisplaySequence;
            result.sold_quantity = (int)prodata.SaleCounts;
            result.desc = prodata.ProductDescriptionInfo.Description;
            result.wap_desc = prodata.ProductDescriptionInfo.MobileDescription;
            result.pic_url.Add(System.IO.Path.Combine(OpenAPIHelper.HostUrl,prodata.GetImage(ImageSize.Size_350, 1)));
            ProductStatus ps = GetProductStatus(prodata);
            result.approve_status = ps.ToString();

            #region 商品属性填充
            var prodAttrs = _iProductService.GetProductAttribute(prodata.Id).ToList();
            var prodAttrids = prodAttrs.Select(d => d.AttributeId).Distinct().ToList();
            result.props_name = "";
            if (prodAttrids.Count > 0)
            {
                List<string> propslst = new List<string>();
                List<string> propsvallst = new List<string>();
                foreach (var curattid in prodAttrids)
                {
                    var item = prodAttrs.FirstOrDefault(d => d.AttributeId == curattid);
                    propsvallst.Clear();
                    foreach (var attrV in item.AttributesInfo.AttributeValueInfo.ToList())
                    {
                        if (prodAttrs.Any(p => p.ValueId == attrV.Id))
                        {
                            propsvallst.Add(attrV.Value);
                        }
                    }
                    propslst.Add(item.AttributesInfo.Name + "#cln#[" + string.Join(",", propsvallst.ToArray()) + "]");
                }
                result.props_name = string.Join("#scln#", propslst.ToArray());
            }
            #endregion

            #region  发货地区
            var prolocid = prodata.Himall_FreightTemplate.SourceAddress;
            result.location = "";
            if (prolocid.HasValue)
            {
                var locpath = _iRegionService.GetFullName(prolocid.Value, ",");
                result.location = "{'city':'#c#', 'state':'#p#'}";
                if (!string.IsNullOrWhiteSpace(locpath))
                {
                    var _tmparr = locpath.Split(',');
                    result.location = result.location.Replace("#p#", _tmparr[0]);
                    if (_tmparr.Length > 1)
                    {
                        result.location = result.location.Replace("#c#", _tmparr[1]);
                    }
                    else
                    {
                        result.location = result.location.Replace("#c#", "");
                    }
                }
            }
            #endregion

            #region SKUS

            ProductTypeInfo typeInfo = _iTypeService.GetType(data.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            result.skus = new List<product_sku_model>();
            foreach (var item in prodata.SKUInfo)
            {
                product_sku_model skudata = new product_sku_model();
                skudata.sku_id = item.Id;
                skudata.outer_sku_id = item.Sku;
                skudata.price = item.SalePrice;
                skudata.quantity = (int)item.Stock;
                //skudata.sku_properties_name = "颜色:" + item.Color + "尺寸:" + item.Size + "版本:" + item.Version;
                skudata.sku_properties_name = colorAlias + ":" + item.Color + sizeAlias + ":" + item.Size + versionAlias + ":" + item.Version;
                string sku_properties_name = item.Color + item.Size + item.Version;
                if (string.IsNullOrWhiteSpace(sku_properties_name))
                {
                    skudata.sku_properties_name = "";
                }
                if (!string.IsNullOrWhiteSpace(skudata.sku_properties_name))
                {
                    result.skus.Add(skudata);
                }
            }
            #endregion

            #endregion

            return result;
        }
        /// <summary>
        /// 商品列表信息项转换
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private product_list_model ProductInfoListMapChange(ProductInfo data)
        {
            product_list_model dresult = null;
            var prodata = data;
            dresult = new product_list_model();

            if (prodata == null)
            {
                throw new HimallOpenApiException(OpenApiErrorCode.Product_Not_Exists, "num_iid");
            }

            #region 装配信息
            dresult.num_iid = (int)prodata.Id;
            dresult.outer_id = prodata.ProductCode;
            dresult.brand_id = (int)prodata.BrandId;
            dresult.brand_name = prodata.BrandName;
            dresult.cid = (int)prodata.CategoryId;
            dresult.type_id = (int)prodata.TypeId;
            if (prodata.Himall_Categories != null)
            {
                dresult.cat_name = prodata.Himall_Categories.Name;
                if (prodata.Himall_Categories.ProductTypeInfo != null)
                {
                    dresult.type_name = prodata.Himall_Categories.ProductTypeInfo.Name;
                }
            }
            dresult.price = prodata.MinSalePrice;
            if (prodata.SKUInfo.Count > 0)
            {
                dresult.num = (int)prodata.SKUInfo.Sum(d => d.Stock);
            }
            else
            {
                dresult.num = 0;
                if (prodata.Quantity.HasValue)
                {
                    dresult.num = prodata.Quantity.Value;
                }
            }
            dresult.title = prodata.ProductName.Trim();
            dresult.list_time = prodata.AddedDate;
            dresult.modified = prodata.AddedDate;
            dresult.sold_quantity = (int)prodata.SaleCounts;
            dresult.pic_url = new System.Collections.ArrayList();
            dresult.pic_url.Add(System.IO.Path.Combine(OpenAPIHelper.HostUrl, prodata.GetImage(ImageSize.Size_350, 1)));
            ProductStatus ps = GetProductStatus(prodata);
            dresult.approve_status = ps.ToString();
            #endregion

            return dresult;
        }
        /// <summary>
        /// 商品列表信息转换
        /// </summary>
        /// <param name="datalist"></param>
        /// <returns></returns>
        private List<product_list_model> ProductInfoListMapChange(List<ProductInfo> datalist)
        {
            List<product_list_model> result = new List<product_list_model>();
            foreach(var item in datalist)
            {
                result.Add(ProductInfoListMapChange(item));
            }
            return result;
        }

        /// <summary>
        /// 获取商品状态
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private ProductStatus GetProductStatus(ProductInfo data)
        {
            ProductStatus result = ProductStatus.Un_Sale;
            if (data.AuditStatus == ProductInfo.ProductAuditStatus.Audited && data.SaleStatus == ProductInfo.ProductSaleStatus.OnSale)
            {
                result = ProductStatus.On_Sale;
            }
            if (data.SaleStatus == ProductInfo.ProductSaleStatus.InStock)
            {
                result = ProductStatus.In_Stock;
            }
            return result;
        }

        /// <summary>
        /// 获取并计算库存
        /// </summary>
        /// <param name="currentQuantity">当前库存</param>
        /// <param name="quantity">库存计算量</param>
        /// <param name="type">1全量更新 2增量更新</param>
        /// <returns></returns>
        private long GetStockQuantity(long currentQuantity, long quantity, int type)
        {
            long result = 0;
            switch (type)
            {
                case 2:
                    result = quantity;
                    break;
                default:
                    result = currentQuantity + quantity;
                    break;
            }
            if (result < 0)
            {
                throw new HimallOpenApiException(OpenApiErrorCode.Product_UpdateeQuantity_Faild, "quantity");
            }
            return result;
        }
        #endregion
    }
}
