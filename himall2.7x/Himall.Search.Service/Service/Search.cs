using Himall.Search.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Search.Service.Model;
using Himall.Search.Service.Data;

namespace Himall.Search.Service
{
    public class Search : ISearch
    {
        /// <summary>
        /// 索引
        /// </summary>
        IndexDao _index;
        /// <summary>
        /// 搜索
        /// </summary>
        SearchDao _search;
        /// <summary>
        /// 分词
        /// </summary>
        Segment _segment;
        public Search(string dictpath)
        {
            _segment = new Segment(dictpath);
            _index = new IndexDao(_segment);
            _search = new SearchDao(_segment);
        }

        public Search()
        {
            var dicPath = Core.Helper.IOHelper.GetMapPath("~/App_Data/Dict/");
            _segment = new Segment(dicPath);
            _index = new IndexDao(_segment);
            _search = new SearchDao(_segment);
        }

        /// <summary>
        /// 根据商品标识删除索引
        /// </summary>
        /// <param name="id">商品标识</param>
        /// <returns>删除索引是否成功</returns>
        public void DeleteIndex(long productId)
        {
            _index.DeleteSegmentWords(productId);
        }


        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="model">索引对象</param>
        /// <returns>创建索引是否成功</returns>
        public void CreateIndex(long productId, string productName)
        {
            List<string> words = _segment.DoSegment(productName);
            _index.InsertSegmentWords(productId, words);
        }

        /// <summary>
        /// 刷新所有商品分词
        /// </summary>
        public void InitSegmentWords(string connection= "") {
            _index.InitSegmentWords(connection);
        }

        /// <summary>
        /// 查询商品
        /// </summary>
        /// <param name="search">查询条件</param>
        /// <returns>商品数据结果</returns>
        public ProductViewResult DoSearch(SearchCondition condition)
        {
            //_index.InitSegmentWords();
            //_search.Query(condition);
            //if(condition.Keyword.Equals("初始化"))
            //    _index.InitSegmentWords();
            return _search.QueryProduct(condition);
        }

        /// <summary>
        /// 查询品牌
        /// </summary>
        /// <param name="search">查询条件</param>
        /// <returns>品牌数据集合</returns>
        public List<BrandView> QueryBrand(SearchCondition condition)
        {
            return _search.QueryBrand(condition);
        }

        /// <summary>
        /// 查询属性集合
        /// </summary>
        /// <param name="search">查询条件</param>
        /// <returns>属性数据集合</returns>
        public List<AttributeView> QueryAttrId(SearchCondition search)
        {
            return _search.QueryAttrId(search);
        }

        /// <summary>
        /// 查询类别
        /// </summary>
        /// <param name="search">查询条件</param>
        /// <returns>类别数据集合</returns>
        public List<CategoryView> QueryCategory(SearchCondition search)
        {
            return _search.QueryCategory(search);
        }

        /// <summary>
        /// 获取联想词
        /// </summary>
        /// <param name="word">查询文本</param>
        /// <returns>可以查询到产品数据的联想词集合</returns>
        public List<string> getAssociationalWord(string word)
        {
            if (string.IsNullOrEmpty(word))
                return null;
            return _search.getAssociationalWord(word);
        }
    }
}


