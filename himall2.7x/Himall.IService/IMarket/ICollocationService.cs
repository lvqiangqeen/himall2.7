using Himall.Core;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface ICollocationService : IService
    {

        /// <summary>
        /// 商家添加一个组合购
        /// </summary>
        /// <param name="info"></param>
        void AddCollocation(CollocationInfo info);


        /// <summary>
        /// 商家修改一个组合购
        /// </summary>
        /// <param name="info"></param>
        void EditCollocation(CollocationInfo info);



        //使组合购失效
        void CancelCollocation(long CollocationId, long shopId);
        /// <summary>

          /// <summary>
        /// 获取商家添加的组合购列表
        /// </summary>
        /// <returns></returns>
        ObsoletePageModel<CollocationInfo> GetCollocationList(CollocationQuery query);


        /// <summary>
        /// 根据商品ID获取组合购信息
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        CollocationInfo GetCollocationByProductId(long productId);
        List<CollocationPoruductInfo> GetCollocationListByProductId(long productId);
        /// <summary>
        /// 根据组合购ID获取组合购信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        CollocationInfo GetCollocation(long Id);

        /// <summary>
        /// 根据组合商品获取组合SKU信息
        /// </summary>
        /// <param name="colloPid"></param>
        /// <param name="skuid"></param>
        /// <returns></returns>
         CollocationSkuInfo GetColloSku(long colloPid, string skuid);

        //获取一个商品的组合购SKU信息
         List<CollocationSkuInfo> GetProductColloSKU(long productid, long colloPid);
    }
}
