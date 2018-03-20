
namespace Himall.IServices
{

    using Himall.Model;
    using System.Linq;
	using Himall.CommonModel;

    public interface ITypeService : IService
    {
        /// <summary>
        /// 获取所有的商品类型列表，包括分页信息
        /// search是搜索条件，如果search为空即显示全部
        /// </summary>
        /// <param name="search">搜索条件</param>
        /// <param name="page">页码</param>
        /// <param name="rows">每页行数</param>
        /// <param name="count">总行数</param>
        /// <returns></returns>
        QueryPageModel<ProductTypeInfo> GetTypes(string search, int pageNo, int pageSize);

        /// <summary>
        /// 获取所有的商品类型列表
        /// </summary>
        /// <returns></returns>
        IQueryable<ProductTypeInfo> GetTypes();

        /// <summary>
        /// 根据Id获取商品类型实体
        /// </summary>
        /// <param name="id">类型Id</param>
        /// <returns></returns>
        ProductTypeInfo GetType(long id);

        /// <summary>
        /// 根据ProductId获取商品类型实体
        /// </summary>
        /// <param name="productId">ProductId</param>
        /// <returns></returns>
        ProductTypeInfo GetTypeByProductId(long productId);

        /// <summary>
        /// 更新商品类型
        /// </summary>
        /// <param name="model"></param>
        void UpdateType(ProductTypeInfo model);

        /// <summary>
        /// 删除商品类型
        /// </summary>
        /// <param name="id"></param>
        void DeleteType(long id);

        /// <summary>
        /// 创建商品类型
        /// </summary>
        /// <param name="model"></param>
        void AddType(ProductTypeInfo model);

    }
}
