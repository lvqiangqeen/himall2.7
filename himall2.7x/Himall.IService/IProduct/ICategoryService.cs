
using Himall.Model;
using System.Collections.Generic;
using System.Linq;
namespace Himall.IServices
{
    public interface ICategoryService : IService
    {

        string GetEffectCategoryName(long shopId, long typeId);

        /// <summary>
        /// 获取所有主分类
        /// </summary>
        /// <returns></returns>
        IEnumerable<CategoryInfo> GetMainCategory();

        /// <summary>
        /// 获取指定分类下面的子级分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IEnumerable<CategoryInfo> GetCategoryByParentId(long id);

        /// <summary>
        /// 获取可以做为经营类目的子级分类
        /// </summary>
        /// <param name="id">父级Id</param>
        /// <returns></returns>
        IEnumerable<CategoryInfo> GetValidBusinessCategoryByParentId(long id);

        /// <summary>
        /// 添加一个分类
        /// </summary>
        /// <param name="model"></param>
        void AddCategory(CategoryInfo model);

        /// <summary>
        /// 获取一个分类信息
        /// </summary>
        /// <param name="id">分类Id</param>
        /// <returns></returns>
        CategoryInfo GetCategory(long id);

        /// <summary>
        /// 获取所有分类
        /// </summary>
        /// <returns></returns>
        List<CategoryInfo> GetCategories();


        /// <summary>
        /// 更新指定分类的名称
        /// </summary>
        /// <param name="id">分类Id</param>
        /// <param name="name">分类的名称</param>
        void UpdateCategoryName(long id, string name);
        /// <summary>
        /// 更新分类佣金比率
        /// </summary>
        /// <param name="id"></param>
        /// <param name="commis"></param>
        void UpdateCategoryCommis(long id, decimal commis);
        /// <summary>
        /// 更新指定分类的显示顺序
        /// </summary>
        /// <param name="id">分类Id</param>
        /// <param name="displaySequence">分类的顺序</param>
        void UpdateCategoryDisplaySequence(long id, long displaySequence);

        /// <summary>
        /// 获取所有一二级节点分类
        /// </summary>
        /// <returns></returns>
        IEnumerable<CategoryInfo> GetFirstAndSecondLevelCategories();

        /// <summary>
        /// 获取指定一级分类下所有二三级节点分类
        /// </summary>
        /// <param name="ids">指定的一级分类</param>
        /// <returns></returns>
        IEnumerable<CategoryInfo> GetSecondAndThirdLevelCategories(params long[] ids);

        /// <summary>
        /// 获取传入分类的所有一级分类
        /// </summary>
        /// <param name="categoryIds">传入分类的id</param>
        /// <returns></returns>
        IEnumerable<CategoryInfo> GetTopLevelCategories(IEnumerable<long> categoryIds);


        /// <summary>
        /// 获取数据库中Id最大的分类
        /// </summary>
        /// <returns></returns>
        long GetMaxCategoryId();
        /// <summary>
        /// 根据Category模型更新
        /// </summary>
        /// <param name="model"></param>
        void UpdateCategory(CategoryInfo model);
        /// <summary>
        /// 根据ID删除分类（递归删除子分类）
        /// </summary>
        /// <param name="id"></param>
        void DeleteCategory(long id);

        /// <summary>
        /// 根据分类的名称获取分类
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        ObsoletePageModel<CategoryInfo> GetCategoryByName(string name, int pageNo, int pageSize);
    }
}
