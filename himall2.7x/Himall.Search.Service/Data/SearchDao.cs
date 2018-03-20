using Himall.Search.Service.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Search.Service.Data
{
    public class SearchDao
    {
        private Segment _segment;
        public SearchDao(Segment segment)
        {
            _segment = segment;
        }

        public ProductViewResult QueryProduct(SearchCondition search)
        {
            ProductViewResult result = new ProductViewResult();
            MySqlConnection connection = Connection.GetConnection();
            MySqlCommand cmd = new MySqlCommand();

            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT SQL_CALC_FOUND_ROWS _p.Id,_p.ProductName,_p.MinSalePrice,_p.ShopId,_p.CategoryId,_p.ImagePath,_p.SaleCounts,_f.SourceAddress,_s.ShopName,_c.CommentCount FROM Himall_Products _p ");
            sql.Append("LEFT JOIN Himall_FreightTemplate _f ON _p.FreightTemplateId=_f.Id ");
            sql.Append("LEFT JOIN Himall_Shops _s ON _p.ShopId=_s.Id ");
            sql.Append("LEFT JOIN (SELECT COUNT(1) AS CommentCount,ProductId FROM Himall_ProductComments GROUP BY ProductId) _c ON _p.Id=_c.ProductId ");
            sql.Append("WHERE _p.IsDeleted=0 AND _p.AuditStatus=2 AND _p.SaleStatus=1 ");
            #region 店铺过滤
            if (search.shopId > 0)
            {
                sql.Append(" AND _p.ShopId=@ShopId ");
                cmd.Parameters.AddWithValue("@ShopId", search.shopId);
            }
            #endregion

            #region 品牌过滤
            if (search.BrandId > 0)
            {
                sql.Append(" AND _p.BrandId=@BrandId ");
                cmd.Parameters.AddWithValue("@BrandId", search.BrandId);
            }
            #endregion

            #region 分类过滤
            if (search.CategoryId > 0)
            {
                if (search.shopId > 0)
                {
                    sql.Append(" AND _p.Id IN (SELECT DISTINCT ProductId FROM Himall_ProductShopCategories WHERE ShopCategoryId = @ShopCategoryId)");
                    cmd.Parameters.AddWithValue("@ShopCategoryId", search.ShopCategoryId);
                }
                else
                    sql.AppendFormat("AND _p.CategoryId IN (SELECT Id FROM Himall_Categories WHERE CONCAT('|',Path,'|') LIKE '%|{0}|%')", search.CategoryId);
            }
            #endregion

            #region 属性过滤
            if (search.AttrIds.Count > 0)
            {
                string sqlAttrWhere = string.Empty;
                for (int i = 0; i < search.AttrIds.Count; i++)
                {
                    if (search.AttrIds[i].Contains("_"))
                    {
                        string attrId = search.AttrIds[i].Split(new char[] { '_' })[0];
                        string valId = search.AttrIds[i].Split(new char[] { '_' })[1];

                        sqlAttrWhere += string.Format("(AttributeId=@AttrId{0} AND ValueId=@ValId{0}) OR", i.ToString());
                        cmd.Parameters.AddWithValue(string.Format("@AttrId{0}", i.ToString()), attrId);
                        cmd.Parameters.AddWithValue(string.Format("@ValId{0}", i.ToString()), valId);
                    }
                }

                if (!string.IsNullOrEmpty(sqlAttrWhere))
                {
                    sqlAttrWhere = sqlAttrWhere.Substring(0, sqlAttrWhere.Length - 3);
                    sql.AppendFormat(" AND _p.Id IN (SELECT DISTINCT ProductId FROM Himall_ProductAttributes WHERE {0}) ", sqlAttrWhere);

                }
            }
            #endregion

            #region 价格区间
            if (search.StartPrice > 0)
            {
                sql.Append(" AND _p.MinSalePrice>=@StartPrice ");
                cmd.Parameters.AddWithValue("@StartPrice", search.StartPrice);
            }
            if (search.EndPrice > 0 && search.EndPrice >= search.StartPrice)
            {
                sql.Append(" AND _p.MinSalePrice <= @EndPrice ");
                cmd.Parameters.AddWithValue("@EndPrice", search.EndPrice);
            }
            #endregion

            #region 关键词处理
            if (!string.IsNullOrEmpty(search.Keyword))
            {
                //分隔关键词
                List<string> words = new List<string>();
                List<string> ws = _segment.DoSegment(search.Keyword);
                foreach (string s in ws)
                {
                    if (!words.Contains(s))
                        words.Add(s);
                }

                if (search.Ex_Keyword != null && !string.IsNullOrEmpty(search.Ex_Keyword.Trim()) && search.Ex_Keyword.Trim().Length <= 5)
                    words.Add(search.Ex_Keyword.Trim());
                else
                {
                    if (!string.IsNullOrEmpty(search.Ex_Keyword))
                    {
                        ws = _segment.DoSegment(search.Ex_Keyword);
                        foreach (string s in ws)
                        {
                            if (!words.Contains(s))
                                words.Add(s);
                        }
                    }
                }

                //分词搜索
                if (words.Count > 0)
                {
                    MySqlCommand IdCmd = new MySqlCommand();
                    string sqlWhere = "SELECT Id FROM Himall_Products WHERE ProductName=@ProductName UNION ALL SELECT a.ProductId FROM Himall_ProductWords a INNER JOIN Himall_SegmentWords b ON a.WordId=b.Id WHERE b.Word IN (SELECT WORD FROM ({0}) AS _WordTable) OR MATCH(b.Word) AGAINST(@WordList IN BOOLEAN MODE)";
                    string wordList = string.Empty;
                    string wordInList = string.Empty;
                    string IdList = string.Empty;
                    for (int i = 0; i < words.Count; i++)
                    {
                        wordList += words[i] + "*,";
                        wordInList += string.Format("SELECT @word{0} AS WORD {1} ", i.ToString(), (i == words.Count - 1 ? string.Empty : " UNION ALL "));
                        IdCmd.Parameters.AddWithValue(string.Format("@word{0}", i.ToString()), words[i]);
                    }
                    IdCmd.Parameters.AddWithValue("@WordList", wordList.TrimEnd(new char[] { ',' }));
                    IdCmd.Parameters.AddWithValue("@ProductName", search.Keyword.Trim());
                    IdCmd.CommandText = string.Format(sqlWhere, wordInList);
                    IdCmd.Connection = connection;
                    if (IdCmd.Connection.State != ConnectionState.Open)
                        connection.Open();

                    MySqlDataReader IdReader = IdCmd.ExecuteReader();
                    while (IdReader.Read())
                    {
                        IdList += IdReader["Id"].ToString() + ",";
                    }
                    IdReader.Dispose();

                    sql.AppendFormat(" AND _p.Id IN ({0})", string.IsNullOrEmpty(IdList) ? "0" : IdList.TrimEnd(new char[] { ',' }));
                    result.Keys = words;
                }
            }
            #endregion

            #region 处理排序
            switch (search.OrderKey)
            {
                case 2:
                    sql.Append(" ORDER BY _p.SaleCounts ");
                    break;
                case 3:
                    sql.Append(" ORDER BY _p.MinSalePrice ");
                    break;
                case 4:
                    sql.Append(" ORDER BY _c.CommentCount ");
                    break;
                case 5:
                    sql.Append(" ORDER BY _p.AddedDate ");
                    break;
                default:
                    sql.Append(" ORDER BY _p.Id ");
                    break;
            }
            if (!search.OrderType)
                sql.Append(" DESC ");
            else
                sql.Append(" ASC ");
            #endregion

            #region 处理分页
            sql.AppendFormat(" LIMIT {0},{1}", (search.PageNumber - 1) * search.PageSize, search.PageSize);
            #endregion
            cmd.Connection = connection;
            if (cmd.Connection.State != ConnectionState.Open)
                connection.Open();

            cmd.CommandText = sql.ToString();

            MySqlDataReader reader = cmd.ExecuteReader();
            List<ProductView> products = new List<ProductView>();
            while (reader.Read())
            {
                ProductView model = new ProductView();
                model.Id = long.Parse(reader["Id"].ToString());
                model.Address = reader["SourceAddress"].ToString();
                model.ImagePath = reader["ImagePath"].ToString();
                model.Price = decimal.Parse(reader["MinSalePrice"].ToString());
                model.ShopName = reader["ShopName"].ToString();
                model.ProductName = reader["ProductName"].ToString();
                model.CategoryId = long.Parse(reader["CategoryId"].ToString());
                model.SaleCount = int.Parse(reader["SaleCounts"].ToString());
                model.Comments = int.Parse(string.IsNullOrEmpty(reader["CommentCount"].ToString()) ? "0" : reader["CommentCount"].ToString());
                model.ShopId = long.Parse(reader["ShopId"].ToString());
                products.Add(model);
            }

            reader.Dispose();

            MySqlCommand total = new MySqlCommand("select found_rows()", connection);
            result.Total = int.Parse(total.ExecuteScalar().ToString());
            connection.Close();
            connection.Dispose();
            result.Data = products;
            return result;
        }

        /// <summary>
        /// 查询商品
        /// </summary>
        /// <param name="search">查询条件</param>
        /// <returns>商品数据结果</returns>
        public ProductViewResult Query(SearchCondition search)
        {
            string sql = null;
            ProductViewResult result = new ProductViewResult();
            List<ProductView> products = new List<ProductView>();
            //主属性条件 + 商品名索引
            List<string> gsql = getMasterAndNameIndexSql(search);
            sql = gsql[0];
            result.Keys = gsql.Skip(1).ToList();

            //目录分类索引
            if (search.CategoryId > 0)
            {
                if (search.shopId > 0)
                    sql = string.Format("select pname.* from({0}) pname inner join himall_productshopcategories c on pname.id = c.ProductId where c.ShopCategoryId = {1}", sql, search.ShopCategoryId);
                else
                    sql = string.Format("select pname.* from({0}) pname inner join himall_indexcategory c on pname.id = c.id where c.CategoryId = {1}", sql, search.CategoryId);
            }

            //属性索引
            if (search.AttrIds.Count > 0)
            {
                sql = string.Format(" select pcategory.* from({0}) pcategory  inner join ({1}) a on pcategory.id = a.id ", sql, getAttrSql(search.AttrIds));
            }
            //排序
            sql = string.Format("select SQL_CALC_FOUND_ROWS presult.ShopId,presult.ImagePath,presult.Id,presult.Address,presult.Price,presult.ShopName,presult.ProductName,presult.CategoryId,presult.SaleCount,presult.Comments from({0}) presult ", sql);
            switch (search.OrderKey)
            {
                case 2:
                    if (!search.OrderType)
                        sql = string.Format("{0}order by SaleCount desc", sql);
                    else
                        sql = string.Format("{0}order by SaleCount asc", sql);
                    break;
                case 3:
                    if (!search.OrderType)
                        sql = string.Format("{0}order by Price desc", sql);
                    else
                        sql = string.Format("{0}order by Price asc", sql);
                    break;
                case 4:
                    if (!search.OrderType)
                        sql = string.Format("{0}order by Comments desc", sql);
                    else
                        sql = string.Format("{0}order by Comments asc", sql);
                    break;
                case 5:
                    if (!search.OrderType)
                        sql = string.Format("{0}order by AddedDate desc", sql);
                    else
                        sql = string.Format("{0}order by AddedDate asc", sql);
                    break;
                default:
                    sql = string.Format("{0}order by Id desc ", sql);
                    break;
            }
            //分页
            sql = string.Format("{0} limit {1},{2}", sql, (search.PageNumber - 1) * search.PageSize, search.PageSize);
            MySqlConnection connection = Connection.GetConnection();
            connection.Open();
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ProductView model = new ProductView();
                model.Id = long.Parse(reader["Id"].ToString());
                model.Address = reader["Address"].ToString();
                model.ImagePath = reader["ImagePath"].ToString();
                model.Price = decimal.Parse(reader["Price"].ToString());
                model.ShopName = reader["ShopName"].ToString();
                model.ProductName = reader["ProductName"].ToString();
                model.CategoryId = long.Parse(reader["CategoryId"].ToString());
                model.SaleCount = int.Parse(reader["SaleCount"].ToString());
                model.Comments = int.Parse(reader["Comments"].ToString());
                model.ShopId = long.Parse(reader["ShopId"].ToString());
                products.Add(model);
            }
            reader.Dispose();
            MySqlCommand total = new MySqlCommand("select found_rows()", connection);
            result.Total = int.Parse(total.ExecuteScalar().ToString());
            connection.Close();
            connection.Dispose();
            result.Data = products;
            return result;
        }

        /// <summary>
        /// 根据主索引条件和产品名称条件产生sql语句
        /// </summary>
        /// <param name="search">查询条件</param>
        /// <returns>sql语句</returns>
        private List<string> getMasterAndNameIndexSql(SearchCondition search)
        {
            List<string> result = new List<string>();
            string sql = "select p.* from himall_indexproduct p ";
            //产品名称索引  
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                List<string> words = new List<string>();

                List<string> ws = _segment.DoSegment(search.Keyword);
                foreach (string s in ws)
                {
                    if (!words.Contains(s))
                        words.Add(s);
                }

                if (search.Ex_Keyword != null && !string.IsNullOrEmpty(search.Ex_Keyword.Trim()) && search.Ex_Keyword.Trim().Length <= 5)
                    words.Add(search.Ex_Keyword.Trim());
                else
                {
                    if (!string.IsNullOrEmpty(search.Ex_Keyword))
                    {
                        ws = _segment.DoSegment(search.Ex_Keyword);
                        foreach (string s in ws)
                        {
                            if (!words.Contains(s))
                                words.Add(s);
                        }
                    }
                }
                result = words;
                string insql = "";
                if (words.Count > 0)
                {
                    insql = "";
                    for (int i = 0; i < words.Count(); i++)
                    {
                        insql = string.Format("{0}SELECT DISTINCT id FROM himall_indexname m  WHERE word = '{1}'", insql, words[i]);
                        if (i < words.Count - 1)
                            insql = string.Format("{0} UNION ALL ", insql);
                    }
                    insql = string.Format("select id from ({0}) m ", insql);
                    insql = string.Format("{0} GROUP BY m.id HAVING COUNT(*) = {1}", insql, words.Count());
                }
                sql = string.Format("{0}inner join ({1}) n on p.id = n.id where ", sql, insql);
            }
            else
                sql = string.Format("{0} where ", sql);
            //主索引其它条件
            if (search.BrandId > 0)
                sql = string.Format("{0} p.BrandId = {1} and ", sql, search.BrandId);
            if (search.shopId > 0)
                sql = string.Format("{0} p.shopId = {1} and ", sql, search.shopId);
            if (search.StartPrice >= 0 && search.EndPrice > search.StartPrice)
                sql = string.Format("{0} p.Price >= {1} and p.price <= {2} and ", sql, search.StartPrice, search.EndPrice);
            sql = string.Format("{0} p.EndDate>now() ", sql);
            result.Insert(0, sql);
            return result;
        }

        /// <summary>
        /// 获取属性交集sql
        /// </summary>
        /// <param name="AttrIds">属性ID集合</param>
        /// <returns>sql语句</returns>
        private string getAttrSql(List<string> AttrIds)
        {
            string insql = " SELECT m.id FROM (";
            for (int i = 0; i < AttrIds.Count(); i++)
            {
                insql = string.Format("{0}SELECT DISTINCT id FROM himall_indexAttr  WHERE AttrValueId = '{1}' ", insql, AttrIds[i]);
                if (i < AttrIds.Count - 1)
                    insql = string.Format("{0} UNION ALL ", insql);
            }
            insql = string.Format("{0}) m GROUP BY m.id HAVING COUNT(*) = {1}", insql, AttrIds.Count());
            return insql;
        }

        /// <summary>
        /// 查询品牌
        /// </summary>
        /// <param name="search">查询条件</param>
        /// <returns>品牌数据集合</returns>
        public List<BrandView> QueryBrand(SearchCondition search)
        {
            List<BrandView> result = new List<BrandView>();
            MySqlConnection connection = Connection.GetConnection();
            MySqlCommand cmd = new MySqlCommand();

            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT Id,Name,Logo FROM Himall_Brands WHERE Id IN (SELECT DISTINCT BrandId FROM Himall_Products _p ");
            sql.Append("WHERE _p.IsDeleted=0 AND _p.AuditStatus=2 AND _p.SaleStatus=1 ");
            #region 店铺过滤
            if (search.shopId > 0)
            {
                sql.Append(" AND _p.ShopId=@ShopId ");
                cmd.Parameters.AddWithValue("@ShopId", search.shopId);
            }
            #endregion

            #region 品牌过滤
            if (search.BrandId > 0)
            {
                sql.Append(" AND _p.BrandId=@BrandId ");
                cmd.Parameters.AddWithValue("@BrandId", search.BrandId);
            }
            #endregion

            #region 分类过滤
            if (search.CategoryId > 0)
            {
                if (search.shopId > 0)
                {
                    sql.Append(" AND _p.Id IN (SELECT DISTINCT ProductId FROM Himall_ProductShopCategories WHERE ShopCategoryId = @ShopCategoryId)");
                    cmd.Parameters.AddWithValue("@ShopCategoryId", search.ShopCategoryId);
                }
                else
                    sql.AppendFormat("AND _p.CategoryId IN (SELECT Id FROM Himall_Categories WHERE CONCAT('|',Path,'|') LIKE '%|{0}|%')", search.CategoryId);
            }
            #endregion

            #region 属性过滤
            if (search.AttrIds.Count > 0)
            {
                string sqlAttrWhere = string.Empty;
                for (int i = 0; i < search.AttrIds.Count; i++)
                {
                    if (search.AttrIds[i].Contains("_"))
                    {
                        string attrId = search.AttrIds[i].Split(new char[] { '_' })[0];
                        string valId = search.AttrIds[i].Split(new char[] { '_' })[1];

                        sqlAttrWhere += string.Format("(AttributeId=@AttrId{0} AND ValueId=@ValId{0}) OR", i.ToString());
                        cmd.Parameters.AddWithValue(string.Format("@AttrId{0}", i.ToString()), attrId);
                        cmd.Parameters.AddWithValue(string.Format("@ValId{0}", i.ToString()), valId);
                    }
                }

                if (!string.IsNullOrEmpty(sqlAttrWhere))
                {
                    sqlAttrWhere = sqlAttrWhere.Substring(0, sqlAttrWhere.Length - 3);
                    sql.AppendFormat(" AND _p.Id IN (SELECT DISTINCT ProductId FROM Himall_ProductAttributes WHERE {0}) ", sqlAttrWhere);

                }
            }
            #endregion

            #region 价格区间
            if (search.StartPrice > 0)
            {
                sql.Append(" AND _p.MinSalePrice>=@StartPrice ");
                cmd.Parameters.AddWithValue("@StartPrice", search.StartPrice);
            }
            if (search.EndPrice > 0 && search.EndPrice >= search.StartPrice)
            {
                sql.Append(" AND _p.MinSalePrice <= @EndPrice ");
                cmd.Parameters.AddWithValue("@EndPrice", search.EndPrice);
            }
            #endregion

            #region 关键词处理
            if (!string.IsNullOrEmpty(search.Keyword))
            {
                //分隔关键词
                List<string> words = new List<string>();
                List<string> ws = _segment.DoSegment(search.Keyword);
                foreach (string s in ws)
                {
                    if (!words.Contains(s))
                        words.Add(s);
                }

                if (search.Ex_Keyword != null && !string.IsNullOrEmpty(search.Ex_Keyword.Trim()) && search.Ex_Keyword.Trim().Length <= 5)
                    words.Add(search.Ex_Keyword.Trim());
                else
                {
                    if (!string.IsNullOrEmpty(search.Ex_Keyword))
                    {
                        ws = _segment.DoSegment(search.Ex_Keyword);
                        foreach (string s in ws)
                        {
                            if (!words.Contains(s))
                                words.Add(s);
                        }
                    }
                }

                //分词搜索
                if (words.Count > 0)
                {
                    MySqlCommand IdCmd = new MySqlCommand();
                    string sqlWhere = "SELECT Id FROM Himall_Products WHERE ProductName=@ProductName UNION ALL SELECT a.ProductId FROM Himall_ProductWords a INNER JOIN Himall_SegmentWords b ON a.WordId=b.Id WHERE b.Word IN (SELECT WORD FROM ({0}) AS _WordTable) OR MATCH(b.Word) AGAINST(@WordList IN BOOLEAN MODE)";
                    string wordList = string.Empty;
                    string wordInList = string.Empty;
                    string IdList = string.Empty;
                    for (int i = 0; i < words.Count; i++)
                    {
                        wordList += words[i] + "*,";
                        wordInList += string.Format("SELECT @word{0} AS WORD {1} ", i.ToString(), (i == words.Count - 1 ? string.Empty : " UNION ALL "));
                        IdCmd.Parameters.AddWithValue(string.Format("@word{0}", i.ToString()), words[i]);
                    }
                    IdCmd.Parameters.AddWithValue("@WordList", wordList.TrimEnd(new char[] { ',' }));
                    IdCmd.Parameters.AddWithValue("@ProductName", search.Keyword.Trim());
                    IdCmd.CommandText = string.Format(sqlWhere, wordInList);
                    IdCmd.Connection = connection;
                    if (IdCmd.Connection.State != ConnectionState.Open)
                        connection.Open();

                    MySqlDataReader IdReader = IdCmd.ExecuteReader();
                    while (IdReader.Read())
                    {
                        IdList += IdReader["Id"].ToString() + ",";
                    }
                    IdReader.Dispose();

                    sql.AppendFormat(" AND _p.Id IN ({0})", string.IsNullOrEmpty(IdList) ? "0" : IdList.TrimEnd(new char[] { ',' }));
                }
            }
            #endregion

            sql.Append(")");

            cmd.Connection = connection;
            if (cmd.Connection.State != ConnectionState.Open)
                connection.Open();

            cmd.CommandText = sql.ToString();
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                BrandView temp = new BrandView();
                temp.Id = long.Parse(reader["Id"].ToString());
                temp.Name = reader["Name"].ToString();
                temp.Logo = reader["Logo"].ToString();
                result.Add(temp);
            }
            reader.Dispose();
            connection.Close();
            connection.Dispose();
            return result;
        }

        /// <summary>
        /// 查询属性集合
        /// </summary>
        /// <param name="search">查询条件</param>
        /// <returns>属性数据集合</returns>
        public List<AttributeView> QueryAttrId(SearchCondition search)
        {
            List<AttributeView> result = new List<AttributeView>();
            MySqlConnection connection = Connection.GetConnection();
            MySqlCommand cmd = new MySqlCommand();

            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT a.Id AS AttrId,a.`Name`,b.Id AS ValueId,b.`Value` FROM Himall_Attributes a ");
            sql.Append("INNER JOIN Himall_AttributeValues b on a.Id=b.AttributeId ");
            sql.Append("INNER JOIN (SELECT DISTINCT _pa.AttributeId,_pa.ValueId FROM Himall_Products _p ");
            sql.Append("INNER JOIN Himall_ProductAttributes _pa on _p.Id = _pa.ProductId ");
            sql.Append("WHERE _p.IsDeleted=0 AND _p.AuditStatus=2 AND _p.SaleStatus=1 ");
            #region 店铺过滤
            if (search.shopId > 0)
            {
                sql.Append(" AND _p.ShopId=@ShopId ");
                cmd.Parameters.AddWithValue("@ShopId", search.shopId);
            }
            #endregion

            #region 品牌过滤
            if (search.BrandId > 0)
            {
                sql.Append(" AND _p.BrandId=@BrandId ");
                cmd.Parameters.AddWithValue("@BrandId", search.BrandId);
            }
            #endregion

            #region 分类过滤
            if (search.CategoryId > 0)
            {
                if (search.shopId > 0)
                {
                    sql.Append(" AND _p.Id IN (SELECT DISTINCT ProductId FROM Himall_ProductShopCategories WHERE ShopCategoryId = @ShopCategoryId)");
                    cmd.Parameters.AddWithValue("@ShopCategoryId", search.ShopCategoryId);
                }
                else
                    sql.AppendFormat("AND _p.CategoryId IN (SELECT Id FROM Himall_Categories WHERE CONCAT('|',Path,'|') LIKE '%|{0}|%')", search.CategoryId);
            }
            #endregion

            #region 属性过滤
            if (search.AttrIds.Count > 0)
            {
                string sqlAttrWhere = string.Empty;
                for (int i = 0; i < search.AttrIds.Count; i++)
                {
                    if (search.AttrIds[i].Contains("_"))
                    {
                        string attrId = search.AttrIds[i].Split(new char[] { '_' })[0];
                        string valId = search.AttrIds[i].Split(new char[] { '_' })[1];

                        sqlAttrWhere += string.Format("(AttributeId=@AttrId{0} AND ValueId=@ValId{0}) OR", i.ToString());
                        cmd.Parameters.AddWithValue(string.Format("@AttrId{0}", i.ToString()), attrId);
                        cmd.Parameters.AddWithValue(string.Format("@ValId{0}", i.ToString()), valId);
                    }
                }

                if (!string.IsNullOrEmpty(sqlAttrWhere))
                {
                    sqlAttrWhere = sqlAttrWhere.Substring(0, sqlAttrWhere.Length - 3);
                    sql.AppendFormat(" AND _p.Id IN (SELECT DISTINCT ProductId FROM Himall_ProductAttributes WHERE {0}) ", sqlAttrWhere);

                }
            }
            #endregion

            #region 价格区间
            if (search.StartPrice > 0)
            {
                sql.Append(" AND _p.MinSalePrice>=@StartPrice ");
                cmd.Parameters.AddWithValue("@StartPrice", search.StartPrice);
            }
            if (search.EndPrice > 0 && search.EndPrice >= search.StartPrice)
            {
                sql.Append(" AND _p.MinSalePrice <= @EndPrice ");
                cmd.Parameters.AddWithValue("@EndPrice", search.EndPrice);
            }
            #endregion

            #region 关键词处理
            if (!string.IsNullOrEmpty(search.Keyword))
            {
                //分隔关键词
                List<string> words = new List<string>();
                List<string> ws = _segment.DoSegment(search.Keyword);
                foreach (string s in ws)
                {
                    if (!words.Contains(s))
                        words.Add(s);
                }

                if (search.Ex_Keyword != null && !string.IsNullOrEmpty(search.Ex_Keyword.Trim()) && search.Ex_Keyword.Trim().Length <= 5)
                    words.Add(search.Ex_Keyword.Trim());
                else
                {
                    if (!string.IsNullOrEmpty(search.Ex_Keyword))
                    {
                        ws = _segment.DoSegment(search.Ex_Keyword);
                        foreach (string s in ws)
                        {
                            if (!words.Contains(s))
                                words.Add(s);
                        }
                    }
                }

                //分词搜索
                if (words.Count > 0)
                {
                    MySqlCommand IdCmd = new MySqlCommand();
                    string sqlWhere = "SELECT Id FROM Himall_Products WHERE ProductName=@ProductName UNION ALL SELECT a.ProductId FROM Himall_ProductWords a INNER JOIN Himall_SegmentWords b ON a.WordId=b.Id WHERE b.Word IN (SELECT WORD FROM ({0}) AS _WordTable) OR MATCH(b.Word) AGAINST(@WordList IN BOOLEAN MODE)";
                    string wordList = string.Empty;
                    string wordInList = string.Empty;
                    string IdList = string.Empty;
                    for (int i = 0; i < words.Count; i++)
                    {
                        wordList += words[i] + "*,";
                        wordInList += string.Format("SELECT @word{0} AS WORD {1} ", i.ToString(), (i == words.Count - 1 ? string.Empty : " UNION ALL "));
                        IdCmd.Parameters.AddWithValue(string.Format("@word{0}", i.ToString()), words[i]);
                    }
                    IdCmd.Parameters.AddWithValue("@WordList", wordList.TrimEnd(new char[] { ',' }));
                    IdCmd.Parameters.AddWithValue("@ProductName", search.Keyword.Trim());
                    IdCmd.CommandText = string.Format(sqlWhere, wordInList);
                    IdCmd.Connection = connection;
                    if (IdCmd.Connection.State != ConnectionState.Open)
                        connection.Open();

                    MySqlDataReader IdReader = IdCmd.ExecuteReader();
                    while (IdReader.Read())
                    {
                        IdList += IdReader["Id"].ToString() + ",";
                    }
                    IdReader.Dispose();

                    sql.AppendFormat(" AND _p.Id IN ({0})", string.IsNullOrEmpty(IdList) ? "0" : IdList.TrimEnd(new char[] { ',' }));
                }
            }
            #endregion

            sql.Append(") AS _r ON b.AttributeId=_r.AttributeId AND b.Id=_r.ValueId ");

            cmd.Connection = connection;
            if (cmd.Connection.State != ConnectionState.Open)
                connection.Open();

            cmd.CommandText = sql.ToString();
            MySqlDataReader reader = cmd.ExecuteReader();
            AttributeView attr;
            while (reader.Read())
            {
                bool isfind = true;
                attr = result.Find(t => t.AttrId == long.Parse(reader["AttrId"].ToString()));
                if (attr == null)//新属性构造
                {
                    isfind = false;
                    attr = new AttributeView();
                    attr.AttrId = long.Parse(reader["AttrId"].ToString());
                    attr.Name = reader["Name"].ToString();
                }
                //属性下的值构造
                AttributeValue v = new AttributeValue();
                v.Id = long.Parse(reader["ValueId"].ToString());
                v.Name = reader["Value"].ToString();
                if (attr.AttrValues.Find(t => t.Id == v.Id && t.Name == v.Name) == null)
                    attr.AttrValues.Add(v);
                if (!isfind)
                    result.Add(attr);
            }
            reader.Dispose();
            connection.Close();
            connection.Dispose();
            return result;
        }

        /// <summary>
        /// 查询类别
        /// </summary>
        /// <param name="search">查询条件</param>
        /// <returns>类别数据集合</returns>
        public List<CategoryView> QueryCategory(SearchCondition search)
        {
            List<CategoryView> result = new List<CategoryView>();
            MySqlConnection connection = Connection.GetConnection();
            MySqlCommand cmd = new MySqlCommand();

            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT DISTINCT CategoryPath FROM Himall_Products _p ");
            sql.Append("WHERE _p.IsDeleted=0 AND _p.AuditStatus=2 AND _p.SaleStatus=1 ");
            #region 店铺过滤
            if (search.shopId > 0)
            {
                sql.Append(" AND _p.ShopId=@ShopId ");
                cmd.Parameters.AddWithValue("@ShopId", search.shopId);
            }
            #endregion

            #region 品牌过滤
            if (search.BrandId > 0)
            {
                sql.Append(" AND _p.BrandId=@BrandId ");
                cmd.Parameters.AddWithValue("@BrandId", search.BrandId);
            }
            #endregion

            #region 分类过滤
            if (search.CategoryId > 0)
            {
                if (search.shopId > 0)
                {
                    sql.Append(" AND _p.Id IN (SELECT DISTINCT ProductId FROM Himall_ProductShopCategories WHERE ShopCategoryId = @ShopCategoryId)");
                    cmd.Parameters.AddWithValue("@ShopCategoryId", search.ShopCategoryId);
                }
                else
                    sql.AppendFormat("AND _p.CategoryId IN (SELECT Id FROM Himall_Categories WHERE CONCAT('|',Path,'|') LIKE '%|{0}|%')", search.CategoryId);
            }
            #endregion

            #region 属性过滤
            if (search.AttrIds.Count > 0)
            {
                string sqlAttrWhere = string.Empty;
                for (int i = 0; i < search.AttrIds.Count; i++)
                {
                    if (search.AttrIds[i].Contains("_"))
                    {
                        string attrId = search.AttrIds[i].Split(new char[] { '_' })[0];
                        string valId = search.AttrIds[i].Split(new char[] { '_' })[1];

                        sqlAttrWhere += string.Format("(AttributeId=@AttrId{0} AND ValueId=@ValId{0}) OR", i.ToString());
                        cmd.Parameters.AddWithValue(string.Format("@AttrId{0}", i.ToString()), attrId);
                        cmd.Parameters.AddWithValue(string.Format("@ValId{0}", i.ToString()), valId);
                    }
                }

                if (!string.IsNullOrEmpty(sqlAttrWhere))
                {
                    sqlAttrWhere = sqlAttrWhere.Substring(0, sqlAttrWhere.Length - 3);
                    sql.AppendFormat(" AND _p.Id IN (SELECT DISTINCT ProductId FROM Himall_ProductAttributes WHERE {0}) ", sqlAttrWhere);

                }
            }
            #endregion

            #region 价格区间
            if (search.StartPrice > 0)
            {
                sql.Append(" AND _p.MinSalePrice>=@StartPrice ");
                cmd.Parameters.AddWithValue("@StartPrice", search.StartPrice);
            }
            if (search.EndPrice > 0 && search.EndPrice >= search.StartPrice)
            {
                sql.Append(" AND _p.MinSalePrice <= @EndPrice ");
                cmd.Parameters.AddWithValue("@EndPrice", search.EndPrice);
            }
            #endregion

            #region 关键词处理
            if (!string.IsNullOrEmpty(search.Keyword))
            {
                //分隔关键词
                List<string> words = new List<string>();
                List<string> ws = _segment.DoSegment(search.Keyword);
                foreach (string s in ws)
                {
                    if (!words.Contains(s))
                        words.Add(s);
                }

                if (search.Ex_Keyword != null && !string.IsNullOrEmpty(search.Ex_Keyword.Trim()) && search.Ex_Keyword.Trim().Length <= 5)
                    words.Add(search.Ex_Keyword.Trim());
                else
                {
                    if (!string.IsNullOrEmpty(search.Ex_Keyword))
                    {
                        ws = _segment.DoSegment(search.Ex_Keyword);
                        foreach (string s in ws)
                        {
                            if (!words.Contains(s))
                                words.Add(s);
                        }
                    }
                }

                //分词搜索
                if (words.Count > 0)
                {
                    MySqlCommand IdCmd = new MySqlCommand();
                    string sqlWhere = "SELECT Id FROM Himall_Products WHERE ProductName=@ProductName UNION ALL SELECT a.ProductId FROM Himall_ProductWords a INNER JOIN Himall_SegmentWords b ON a.WordId=b.Id WHERE b.Word IN (SELECT WORD FROM ({0}) AS _WordTable) OR MATCH(b.Word) AGAINST(@WordList IN BOOLEAN MODE)";
                    string wordList = string.Empty;
                    string wordInList = string.Empty;
                    string IdList = string.Empty;
                    for (int i = 0; i < words.Count; i++)
                    {
                        wordList += words[i] + "*,";
                        wordInList += string.Format("SELECT @word{0} AS WORD {1} ", i.ToString(), (i == words.Count - 1 ? string.Empty : " UNION ALL "));
                        IdCmd.Parameters.AddWithValue(string.Format("@word{0}", i.ToString()), words[i]);
                    }
                    IdCmd.Parameters.AddWithValue("@WordList", wordList.TrimEnd(new char[] { ',' }));
                    IdCmd.Parameters.AddWithValue("@ProductName", search.Keyword.Trim());
                    IdCmd.CommandText = string.Format(sqlWhere, wordInList);
                    IdCmd.Connection = connection;
                    if (IdCmd.Connection.State != ConnectionState.Open)
                        connection.Open();

                    MySqlDataReader IdReader = IdCmd.ExecuteReader();
                    while (IdReader.Read())
                    {
                        IdList += IdReader["Id"].ToString() + ",";
                    }
                    IdReader.Dispose();

                    sql.AppendFormat(" AND _p.Id IN ({0})", string.IsNullOrEmpty(IdList) ? "0" : IdList.TrimEnd(new char[] { ',' }));
                }
            }
            #endregion

            cmd.Connection = connection;
            if (cmd.Connection.State != ConnectionState.Open)
                connection.Open();

            cmd.CommandText = sql.ToString();
            MySqlDataReader reader = cmd.ExecuteReader();
            List<string> lstCategoryPath = new List<string>();
            while (reader.Read())
                lstCategoryPath.Add(reader["CategoryPath"].ToString());
            reader.Dispose();

            string allPath = string.Empty;
            foreach (string path in lstCategoryPath)
                allPath += string.Format("|{0}|", path);

            List<string> lstCategoryId = allPath.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();

            sql.Clear();
            sql.Append("SELECT Id,Name,Depth AS level FROM Himall_Categories WHERE Id IN(");
            foreach (string id in lstCategoryId)
            {
                if (!string.IsNullOrEmpty(id))
                    sql.AppendFormat("{0},", id);
            }
            cmd.CommandText = sql.ToString().TrimEnd(new char[] { ',' }) + ")";
            cmd.Parameters.Clear();
            reader = cmd.ExecuteReader();
            CategoryView top = null;
            while (reader.Read())//构造树型结构
            {
                CategoryView category;
                int level = int.Parse(reader["level"].ToString());
                long id = long.Parse(reader["Id"].ToString());
                if (level == 1)//根结点处理
                {
                    if (result.Find(t => t.Id == id) != null)
                    {
                        top = null;
                        continue;
                    }
                    category = new CategoryView();
                    category.Id = id;
                    category.Name = reader["Name"].ToString();
                    top = category;
                    result.Add(category);
                }
                else
                {
                    if (top == null)
                        continue;
                    if (top.SubCategory == null || top.SubCategory.Count == 0)//二级结点处理
                    {
                        CategoryView sub = new CategoryView();
                        sub.Id = id;
                        sub.Name = reader["Name"].ToString();
                        top.SubCategory.Add(sub);
                    }
                    else//三级结点处理
                    {
                        CategoryView temp = top.SubCategory.Find(t => t.Id == id);
                        CategoryView sub = new CategoryView();
                        sub.Id = id;
                        sub.Name = reader["Name"].ToString();
                        if (temp != null)
                            temp.SubCategory.Add(sub);
                        else
                            top.SubCategory.Add(sub);
                    }
                }
            }
            reader.Dispose();
            connection.Close();
            connection.Dispose();
            return result;
        }

        /// <summary>
        /// 获取联想词
        /// </summary>
        /// <param name="word">查询文本</param>
        /// <returns>可以查询到产品数据的联想词集合</returns>
        public List<string> getAssociationalWord(string word)
        {
            List<string> result = new List<string>();
            MySqlConnection connection = Connection.GetConnection();
            connection.Open();
            string sql = string.Format("SELECT Word FROM Himall_SegmentWords WHERE Word LIKE '{0}%'", word);
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(reader["word"].ToString());
            }
            connection.Close();
            reader.Dispose();
            connection.Dispose();
            return result;
        }
    }
}
