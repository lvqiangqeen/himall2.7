using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface IShopCategoryService:IService
    {
		IQueryable<CategoryInfo> GetBusinessCategory(long shopId);
		IQueryable<CategoryInfo> GetBusinessCategory(long shopId, bool isSelf);
        IQueryable<ShopCategoryInfo> GetShopCategory(long shopId);

        /// <summary>
        /// 获取所有主分类
        /// </summary>
        /// <returns></returns>
        IEnumerable<ShopCategoryInfo> GetMainCategory(long shop);

        /// <summary>
        /// 获取指定分类下面的子级分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IEnumerable<ShopCategoryInfo> GetCategoryByParentId(long id);

        /// <summary>
        /// 添加一个分类
        /// </summary>
        /// <param name="model"></param>
        void AddCategory(ShopCategoryInfo model);

        /// <summary>
        /// 获取一个分类信息
        /// </summary>
        /// <param name="id">分类Id</param>
        /// <returns></returns>
        ShopCategoryInfo GetCategory(long id);
        /// <summary>
        /// 通过商品编号取得店铺所属分类
        /// </summary>
        /// <param name="id">商品编号</param>
        /// <returns></returns>
        ShopCategoryInfo GetCategoryByProductId(long id);


        /// <summary>
        /// 更新指定分类的名称
        /// </summary>
        /// <param name="id">分类Id</param>
        /// <param name="name">分类的名称</param>
        void UpdateCategoryName(long id, string name);

        /// <summary>
        /// 更新指定分类的显示顺序
        /// </summary>
        /// <param name="id">分类Id</param>
        /// <param name="displaySequence">分类的顺序</param>
        void UpdateCategoryDisplaySequence(long id, long displaySequence);


        /// <summary>
        /// 根据Category模型更新
        /// </summary>
        /// <param name="model"></param>
        void UpdateCategory(ShopCategoryInfo model);


        /// <summary>
        /// 根据ID删除分类（递归删除子分类）
        /// </summary>
        /// <param name="id"></param>
        void DeleteCategory(long id, long shopId);

        /// <summary>
        /// 根据父级Id获取商品分类
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        IEnumerable<ShopCategoryInfo> GetCategoryByParentId(long id, long shopId);
    }
}
