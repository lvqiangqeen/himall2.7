using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Dapper;
using Himall.Model;
using Himall.Entity;
using Himall.IServices;
using Himall.CommonModel;
using Himall.Core;

namespace Himall.Service
{
    public class SearchProductService : ServiceBase, ISearchProductService
    {

        public void AddSearchProduct(long productId)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("INSERT INTO `himall_searchproducts` (`ProductId`, `ProductName`, `ShopId`, `ShopName`, `BrandId`, `BrandName`, `FirstCateId`, `FirstCateName`, `SecondCateId`, `SecondCateName`, `ThirdCateId`, `ThirdCateName`, `AttrValues`, `Comments`, `SaleCount`, `SalePrice`, `OnSaleTime`, `ImagePath`, `CanSearch`, `BrandLogo`) ");
            sql.Append("select a.Id,a.ProductName,a.ShopId,b.ShopName,a.BrandId,c.`Name` as BrandName,SUBSTRING_INDEX(a.CategoryPath, '|', 1) AS FirstCateId, d.Name as FirstCateName,SUBSTRING_INDEX(SUBSTRING_INDEX(CategoryPath, '|', 2), '|', -1) AS SecondCateId, e.Name as SecondCateName,SUBSTRING_INDEX(CategoryPath, '|', -1) AS ThirdCateId, f.Name as ThirdCateName,g.AttrValues,0,0,a.MinSalePrice,a.AddedDate,a.ImagePath,(case when a.SaleStatus=1 and a.AuditStatus=2 and a.IsDeleted=0 then 1 else 0 end) as CanSearch,c.Logo from himall_products a ");
            sql.Append("left join himall_shops b on a.ShopId = b.Id ");
            sql.Append("left join himall_brands c on a.BrandId = c.Id ");
            sql.Append("left join himall_categories d on SUBSTRING_INDEX(a.CategoryPath, '|', 1) = d.Id ");
            sql.Append("left join himall_categories e on SUBSTRING_INDEX(SUBSTRING_INDEX(a.CategoryPath, '|', 2), '|', -1) = e.Id ");
            sql.Append("left join himall_categories f on SUBSTRING_INDEX(a.CategoryPath,'|', -1) = f.Id ");
            sql.Append("left join (select ProductId, group_concat(ValueId) as AttrValues from himall_productattributes group by productId) g on a.Id = g.ProductId ");
            sql.Append("where a.Id=@ProductId");
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                conn.Execute(sql.ToString(), new { ProductId = productId });
            }
        }

        public void UpdateSearchProduct(long productId)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("update himall_searchproducts a ");
            sql.Append("left join himall_products b on a.ProductId = b.Id ");
            sql.Append("left join himall_categories c on SUBSTRING_INDEX(b.CategoryPath, '|', 1) = c.Id ");
            sql.Append("left join himall_categories d on SUBSTRING_INDEX(SUBSTRING_INDEX(b.CategoryPath, '|', 2), '|', -1) = d.Id ");
            sql.Append("left join himall_categories e on SUBSTRING_INDEX(b.CategoryPath, '|', -1) = e.Id ");
            sql.Append("left join (select ProductId, group_concat(ValueId) as AttrValues from himall_productattributes group by productId) f on a.ProductId = f.ProductId ");
            sql.Append("left join himall_shops g on b.ShopId = g.Id ");
            sql.Append("left join himall_brands h on b.BrandId = h.Id ");
            sql.Append("set a.ProductName = b.ProductName,a.ShopName = g.ShopName,a.BrandId=h.Id,a.BrandName = h.`Name`,a.BrandLogo = h.Logo,a.FirstCateId = c.Id,a.FirstCateName = c.`Name`, ");
            sql.Append("a.SecondCateId = d.Id,a.SecondCateName = d.`Name`,a.ThirdCateId = e.Id,a.ThirdCateName = e.`Name`,a.AttrValues = f.AttrValues,a.SalePrice = b.MinSalePrice,a.ImagePath = b.ImagePath, ");
            sql.Append("a.CanSearch = (case when b.SaleStatus =1 and b.AuditStatus = 2 and b.IsDeleted=0 then 1 else 0 end) ");
            sql.Append("where a.ProductId=@ProductId ");
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                conn.Execute(sql.ToString(), new { ProductId = productId });
            }
        }

        public void UpdateShop(long shopId, string shopName)
        {
            string sql = "update himall_searchProducts set ShopName=@ShopName where ShopId=@ShopId";
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                conn.Execute(sql, new { ShopName = shopName, ShopId = shopId });
            }
        }

        public void UpdateSearchStatusByProduct(long productId)
        {
            string sql = "update himall_searchproducts a left join himall_products b on a.productid=b.id set a.cansearch=(case when b.SaleStatus=1 and b.AuditStatus=2 and b.IsDeleted=0 then 1 else 0 end) where a.productid=@ProductId";
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                conn.Execute(sql, new { ProductId = productId });
            }
        }

        public void UpdateSearchStatusByProducts(List<long> productIds)
        {
            string sql = "update himall_searchproducts a left join himall_products b on a.productid=b.id set a.cansearch=(case when b.SaleStatus=1 and b.AuditStatus=2 and b.IsDeleted=0 then 1 else 0 end) where a.productid in @ProductId";
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                conn.Execute(sql, new { ProductId = productIds.ToArray() });
            }
        }
        public void UpdateSearchStatusByShop(long shopId)
        {
            string sql = string.Empty;

            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                sql = "update himall_searchproducts a left join himall_products b on a.productid = b.id set CanSearch = 0 where b.shopid = @id and cansearch=1; ";
                sql += "update himall_searchproducts a left join himall_products b on a.productid = b.id set CanSearch = 1 where a.cansearch=0 and a.shopid = @id and b.AuditStatus = 2 and b.SaleStatus = 1 and b.IsDeleted = 0; ";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", shopId);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateBrand(BrandInfo brand)
        {
            string sql = "update himall_searchProducts set BrandName=@BrandName,BrandLogo=@BrandLogo where BrandId=@BrandId";
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                conn.Execute(sql, new { BrandName = brand.Name, BrandLogo = brand.Logo });
            }
        }
        public void UpdateCategory(CategoryInfo category)
        {
            string sql = "update himall_searchProducts set {0}CateName=@CateName where {0}CateId=@CateId";
            switch (category.Depth)
            {
                case 1:
                    sql = string.Format(sql, "First");
                    break;
                case 2:
                    sql = string.Format(sql, "Second");
                    break;
                case 3:
                    sql = string.Format(sql, "Third");
                    break;
            }
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                conn.Execute(sql, new { CateName = category.Name, CateId = category.Id });
            }
        }

        /// <summary>
        /// 商品搜索
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public SearchProductResult SearchProduct(SearchProductQuery query)
        {
            SearchProductResult result = new SearchProductResult();
            DynamicParameters parms = new DynamicParameters();
            string countsql = "select count(1) from himall_searchproducts ";
            string sql = "select ProductId,ProductName,SalePrice,ImagePath,ShopId,ShopName,SaleCount,ThirdCateId,Comments from himall_searchproducts ";
            string where = GetSearchWhere(query, parms);
            string order = GetSearchOrder(query);
            string index = GetForceIndex(query);
            string page = GetSearchPage(query);
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                result.Data = conn.Query<ProductView>(string.Concat(sql, index, where, order, page), parms).ToList();
                result.Total = int.Parse(conn.ExecuteScalar(string.Concat(countsql, where), parms).ToString());                
            }

            return result;
        }

        /// <summary>
        /// 商品属性、分类、品牌搜索
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public SearchProductFilterResult SearchProductFilter(SearchProductQuery query)
        {

            try
            {
                SearchProductFilterResult result = new SearchProductFilterResult();
                DynamicParameters parms = new DynamicParameters();
                string sql = "select DISTINCT Id,FirstCateId,FirstCateName,SecondCateId,SecondCateName,ThirdCateId,ThirdCateName,BrandId,BrandName,BrandLogo,AttrValues from himall_searchproducts  ps ";
                string where = GetSearchWhere(query, parms);
                string order = GetSearchOrder(query);
                string index = GetForceIndex(query);
                string page = string.Empty;
                string AttrValueIds = string.Empty;
                bool hasAttrCache = false;
                List<dynamic> data = null;
                List<AttributeInfo> listAttr = new List<AttributeInfo>();
                List<AttributeValueInfo> listAttrVal = new List<AttributeValueInfo>();
                if (Cache.Exists(CacheKeyCollection.CACHE_ATTRIBUTE_LIST) && Cache.Exists(CacheKeyCollection.CACHE_ATTRIBUTEVALUE_LIST))
                {
                    hasAttrCache = true;
                    listAttr = Cache.Get<List<AttributeInfo>>(CacheKeyCollection.CACHE_ATTRIBUTE_LIST);
                    listAttrVal = Cache.Get<List<AttributeValueInfo>>(CacheKeyCollection.CACHE_ATTRIBUTEVALUE_LIST);
                }

                using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
                {
                    data = conn.Query(string.Concat(sql, index, where, order, page), parms).ToList();
                    foreach (dynamic o in data)
                        AttrValueIds += o.AttrValues;

                    if (!hasAttrCache)
                    {
                        sql = "SELECT * FROM HiMall_Attributes";
                        listAttr = conn.Query<AttributeInfo>(sql).ToList();
                        sql = "SELECT * FROM himall_attributevalues";
                        listAttrVal = conn.Query<AttributeValueInfo>(sql).ToList();
                    }
                }
                if (!hasAttrCache)
                {
                    Cache.Insert<List<AttributeInfo>>(CacheKeyCollection.CACHE_ATTRIBUTE_LIST, listAttr);
                    Cache.Insert<List<AttributeValueInfo>>(CacheKeyCollection.CACHE_ATTRIBUTEVALUE_LIST, listAttrVal);
                }

                List<string> ValueIds = AttrValueIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                listAttrVal = listAttrVal.Where(r => ValueIds.Contains(r.Id.ToString())).ToList();
                listAttr = listAttr.Where(r => listAttrVal.Select(z => z.AttributeId).Contains(r.Id)).ToList();

                result.Attribute = listAttr.Select(r => new AttributeView()
                {
                    AttrId = r.Id,
                    Name = r.Name,
                    AttrValues = listAttrVal.Where(z => z.AttributeId == r.Id).Select(s =>
                                  new AttributeValue()
                                  {
                                      Id = s.Id,
                                      Name = s.Value
                                  }).ToList()
                }).ToList();

                result.Brand = data.Where((x, i) => data.FindIndex(z => z.BrandId == x.BrandId && z.BrandId > 0) == i)
                                   .Select(p => new BrandView() { Id = p.BrandId, Name = p.BrandName, Logo = p.BrandLogo }).ToList();

                result.Category = data.Where((x, i) => data.FindIndex(z => z.FirstCateId == x.FirstCateId) == i) //去重
                                      .Select(f => new CategoryView()
                                      {
                                          Id = f.FirstCateId,
                                          Name = f.FirstCateName,
                                          SubCategory = data.Where((x, i) => data.FindIndex(z => z.SecondCateId == x.SecondCateId) == i) //二级去重
                                                            .Where(r => r.FirstCateId == f.FirstCateId) //查找指定一级分类的下级
                                                            .Select(s => new CategoryView()
                                                            {
                                                                Id = s.SecondCateId,
                                                                Name = s.SecondCateName,
                                                                SubCategory = data.Where((x, i) => data.FindIndex(z => z.ThirdCateId == x.ThirdCateId) == i) //三级去重
                                                                                  .Where(r => r.SecondCateId == s.SecondCateId) //查找指定二级分类的下级
                                                                                  .Select(t => new CategoryView()
                                                                                  {
                                                                                      Id = t.ThirdCateId,
                                                                                      Name = t.ThirdCateName
                                                                                  }).ToList()
                                                            }).ToList()
                                      }).ToList();
                return result;
            }
            catch (Exception ex)
            {

                Log.Error("搜索不出来了：", ex);
                return new SearchProductFilterResult();
            }
        }

        #region 组装sql
        /// <summary>
        /// 获取搜索过滤sql
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        private string GetSearchWhere(SearchProductQuery query, DynamicParameters parms)
        {
            StringBuilder where = new StringBuilder();
            where.Append("WHERE CanSearch=1 ");
            #region 过滤条件
            if (query.ShopId != 0)
            {
                where.Append("AND ShopId=@ShopId ");
                parms.Add("@ShopId", query.ShopId);
            }

            if (query.VShopId != 0)
            {
                where.Append(" AND ShopId IN (SELECT ShopId FROM himall_vshop where Id=@VShopId) ");
                parms.Add("@VShopId", query.VShopId);
            }

            if ((query.VShopId != 0 || query.ShopId != 0) && query.ShopCategoryId != 0)
            {
                where.Append(" AND ProductId IN (select ProductId from himall_productshopcategories where ShopCategoryId in(select id from Himall_ShopCategories where ShopId = @ShopId and(id = @ShopCategoryId or ParentCategoryId = @ShopCategoryId)))");
                parms.Add("@ShopCategoryId", query.ShopCategoryId);

            }

            if (query.BrandId != 0)
            {
                where.Append("AND BrandId=@BrandId ");
                parms.Add("@BrandId", query.BrandId);

            }

            if (query.FirstCateId != 0)
            {
                where.Append("AND FirstCateId=@FirstCateId ");
                parms.Add("@FirstCateId", query.FirstCateId);

            }
            else if (query.SecondCateId != 0)
            {
                where.Append("AND SecondCateId=@SecondCateId ");
                parms.Add("@SecondCateId", query.SecondCateId);

            }
            else if (query.ThirdCateId != 0)
            {
                where.Append("AND ThirdCateId=@ThirdCateId ");
                parms.Add("@ThirdCateId", query.ThirdCateId);

            }

            if (query.StartPrice > 0)
            {
                where.Append(" AND SalePrice>=@StartPrice ");
                parms.Add("@StartPrice", query.StartPrice);

            }

            if (query.EndPrice > 0 && query.EndPrice >= query.StartPrice)
            {
                where.Append(" AND SalePrice <= @EndPrice ");
                parms.Add("@EndPrice", query.EndPrice);

            }

            if (query.AttrValIds.Count > 0)
            {
                where.Append(" AND ProductId IN (SELECT DISTINCT ProductId FROM Himall_ProductAttributes WHERE ValueId IN @ValueIds) ");
                parms.Add("@ValueIds", query.AttrValIds.ToArray());

            }

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                if (!query.IsLikeSearch)
                {
                    where.Append("AND MATCH(ProductName) AGAINST(@ProductName IN BOOLEAN MODE) ");
                    parms.Add("@ProductName", string.Concat(query.Keyword, "*").Replace(" ", "*"));

                }
                else
                {
                    where.Append("AND ProductName like @ProductName ");
                    parms.Add("@ProductName", "%" + query.Keyword + "%");

                }
            }

            return where.ToString();
            #endregion
        }
        /// <summary>
        /// 获取搜索排序sql
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private string GetSearchOrder(SearchProductQuery query)
        {
            string order = string.Empty;
            switch (query.OrderKey)
            {
                case 2:
                    order = " ORDER BY SaleCount ";
                    break;
                case 3:
                    order = " ORDER BY SalePrice ";
                    break;
                case 4:
                    order = " ORDER BY Comments ";
                    break;
                case 5:
                    order = " ORDER BY OnSaleTime ";
                    break;
                default:
                    order = " ORDER BY Id ";
                    break;
            }
            if (!query.OrderType)
                order += " DESC ";
            else
                order += " ASC ";

            return order;
        }

        /// <summary>
        /// 非主键排序时强制使用索引
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private string GetForceIndex(SearchProductQuery query)
        {
            if (!string.IsNullOrEmpty(query.Keyword))
                return string.Empty;

            string index = string.Empty;
            switch (query.OrderKey)
            {
                case 2:
                    index = " FORCE INDEX(IX_SaleCount) ";
                    break;
                case 3:
                    index = " FORCE INDEX(IX_SalePrice)  ";
                    break;
                case 4:
                    index = " FORCE INDEX(IX_Comments) ";
                    break;
                case 5:
                    index = " FORCE INDEX(IX_OnSaleTime) ";
                    break;
            }

            return index;
        }
        /// <summary>
        /// 获取搜索商品分页sql
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private string GetSearchPage(SearchProductQuery query)
        {
            return string.Format(" LIMIT {0},{1} ", (query.PageNumber - 1) * query.PageSize, query.PageSize);
        }
        #endregion

        #region 小程序商品查询

        /// <summary>
        /// 商品搜索
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public SearchProductResult SearchAppletProduct(SearchProductQuery query)
        {
            SearchProductResult result = new SearchProductResult();
            DynamicParameters parms = new DynamicParameters();
            string countsql = "select count(1) from himall_searchproducts ps ";
            string sql = "select ps.ProductId,ps.ProductName,ps.SalePrice,ps.ImagePath,ps.ShopId,ps.ShopName,ps.SaleCount,ps.ThirdCateId,ps.Comments,pt.HasSKU,pt.MinSalePrice,(select id from Himall_SKUs where ProductId=ps.ProductId order by id desc limit 1) as SkuId,IFNULL((select Sum(Quantity) from Himall_ShoppingCarts cs where cs.ProductId=ps.ProductId),0) as cartquantity  from himall_searchproducts ps left join Himall_Products pt on ps.ProductId=pt.Id ";
            string where = GetAppletSearchWhere(query, parms);
            string order = GetAppletSearchOrder(query);
            string index = GetForceIndex(query);
            string page = GetSearchPage(query);
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                result.Data = conn.Query<ProductView>(string.Concat(sql, index, where, order, page), parms).ToList();
                result.Total = int.Parse(conn.ExecuteScalar(string.Concat(countsql, where), parms).ToString());
            }

            return result;
        }
        /// <summary>
        /// 获取搜索过滤sql
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        private string GetAppletSearchWhere(SearchProductQuery query, DynamicParameters parms)
        {
            StringBuilder where = new StringBuilder();
            where.Append("WHERE CanSearch=1 ");
            #region 过滤条件
            if (query.ShopId != 0)
            {
                where.Append("AND ps.ShopId=@ShopId ");
                parms.Add("@ShopId", query.ShopId);
            }

            if (query.VShopId != 0)
            {
                where.Append(" AND ps.ShopId IN (SELECT ShopId FROM himall_vshop where Id=@VShopId) ");
                parms.Add("@VShopId", query.VShopId);
            }

            if ((query.VShopId != 0 || query.ShopId != 0) && query.ShopCategoryId != 0)
            {
                where.Append(" AND ps.ProductId IN (select ProductId from himall_productshopcategories where ShopCategoryId in(select id from Himall_ShopCategories where ShopId = @ShopId and(id = @ShopCategoryId or ParentCategoryId = @ShopCategoryId)))");
                parms.Add("@ShopCategoryId", query.ShopCategoryId);

            }

            if (query.BrandId != 0)
            {
                where.Append("AND ps.BrandId=@BrandId ");
                parms.Add("@BrandId", query.BrandId);

            }

            if (query.FirstCateId != 0)
            {
                where.Append("AND ps.FirstCateId=@FirstCateId ");
                parms.Add("@FirstCateId", query.FirstCateId);

            }
            else if (query.SecondCateId != 0)
            {
                where.Append("AND ps.SecondCateId=@SecondCateId ");
                parms.Add("@SecondCateId", query.SecondCateId);

            }
            else if (query.ThirdCateId != 0)
            {
                where.Append("AND ps.ThirdCateId=@ThirdCateId ");
                parms.Add("@ThirdCateId", query.ThirdCateId);

            }

            if (query.StartPrice > 0)
            {
                where.Append(" AND ps.SalePrice>=@StartPrice ");
                parms.Add("@StartPrice", query.StartPrice);

            }

            if (query.EndPrice > 0 && query.EndPrice >= query.StartPrice)
            {
                where.Append(" AND ps.SalePrice <= @EndPrice ");
                parms.Add("@EndPrice", query.EndPrice);

            }

            if (query.AttrValIds.Count > 0)
            {
                where.Append(" AND ps.ProductId IN (SELECT DISTINCT ProductId FROM Himall_ProductAttributes WHERE ValueId IN @ValueIds) ");
                parms.Add("@ValueIds", query.AttrValIds.ToArray());

            }

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                if (!query.IsLikeSearch)
                {
                    where.Append("AND MATCH(ps.ProductName) AGAINST(@ProductName IN BOOLEAN MODE) ");
                    parms.Add("@ProductName", string.Concat(query.Keyword, "*").Replace(" ", "*"));

                }
                else
                {
                    where.Append("AND ps.ProductName like @ProductName ");
                    parms.Add("@ProductName", "%" + query.Keyword + "%");

                }
            }

            return where.ToString();
            #endregion
        }
        /// <summary>
        /// 获取搜索排序sql
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private string GetAppletSearchOrder(SearchProductQuery query)
        {
            string order = string.Empty;
            switch (query.OrderKey)
            {
                case 2:
                    order = " ORDER BY ps.SaleCount ";
                    break;
                case 3:
                    order = " ORDER BY ps.SalePrice ";
                    break;
                case 4:
                    order = " ORDER BY ps.Comments ";
                    break;
                case 5:
                    order = " ORDER BY ps.OnSaleTime ";
                    break;
                default:
                    order = " ORDER BY ps.Id ";
                    break;
            }
            if (!query.OrderType)
                order += " DESC ";
            else
                order += " ASC ";

            return order;
        }
        #endregion
    }
}
