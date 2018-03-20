using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices.QueryModel;
using Himall.Core.Plugins.Message;


namespace Himall.IServices
{
    public interface IWXSmallProgramService : IService
    {
        /// <summary>
        /// 获取所有商品
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<ProductInfo> GetWXSmallProducts(int page,int rows);

        /// <summary>
        /// 添加商品
        /// </summary>
        void AddWXSmallProducts(WXSmallChoiceProductsInfo model);

        /// <summary>
        /// 批量商品信息
        /// </summary>
        /// <param name="productIds">分销商品ids</param>
        /// <returns></returns>
        List<WXSmallChoiceProductsInfo> GetWXSmallProductInfo(IEnumerable<long> productIds);

        /// <summary>
        /// 获取所有商品
        /// </summary>
        /// <returns></returns>
        List<WXSmallChoiceProductsInfo> GetWXSmallProducts();
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="Id"></param>
        void DeleteWXSmallProductById(long Id);
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="ids"></param>
        void DeleteWXSmallProductByIds(long[] ids);

        IQueryable<ProductInfo> GetWXSmallHomeProducts();
    }
}
