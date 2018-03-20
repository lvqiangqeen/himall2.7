using System.Linq;
using Himall.Model;

namespace Himall.IServices
{
    public interface IArticleCategoryService : IService
    {
        /// <summary>
        /// 获取所有文章分类
        /// </summary>
        /// <returns></returns>
        IQueryable<ArticleCategoryInfo> GetCategories();

        /// <summary>
        /// 获取指定父分类下所有子文章分类
        /// </summary>
        /// <param name="parentId">父文章分类，0表示根级</param>
        /// <param name="recursive">是否递归查询所有子分类,默认只查询直接子分类</param>
        /// <returns></returns>
        IQueryable<ArticleCategoryInfo> GetArticleCategoriesByParentId(long parentId, bool recursive = false);

        /// <summary>
        /// 根据id获取文章分类
        /// </summary>
        /// <param name="id">待获取的文章分类id</param>
        /// <returns></returns>
        ArticleCategoryInfo GetArticleCategory(long id);

        /// <summary>
        /// 获取特殊文章分类
        /// </summary>
        /// <param name="categoryType">特殊分类类型</param>
        /// <returns></returns>
        ArticleCategoryInfo GetSpecialArticleCategory(SpecialCategory categoryType);

        /// <summary>
        /// 获取文章分类全路径
        /// </summary>
        /// <param name="id">文章分类编号</param>
        /// <param name="seperator">分隔符</param>
        /// <returns></returns>
        string GetFullPath(long id, string seperator = ",");

        /// <summary>
        /// 增加新文章分类
        /// </summary>
        /// <param name="articleCategory">待增加的文章分类</param>
        void AddArticleCategory(ArticleCategoryInfo articleCategory);

        /// <summary>
        /// 更新指定文章分类的名称
        /// </summary>
        /// <param name="id">文章分类Id</param>
        /// <param name="name">文章分类的名称</param>
        void UpdateArticleCategoryName(long id, string name);

        /// <summary>
        /// 更新指定文章分类的显示顺序
        /// </summary>
        /// <param name="id">文章分类Id</param>
        /// <param name="displaySequence">文章分类的顺序</param>
        void UpdateArticleCategoryDisplaySequence(long id, long displaySequence);

        /// <summary>
        /// 更新文章分类
        /// </summary>
        /// <param name="articleCategory">待更新的文章分类</param>
        void UpdateArticleCategory(ArticleCategoryInfo articleCategory);

        /// <summary>
        /// 删除文章分类
        /// </summary>
        /// <param name="ids">待删除的文章分类编号</param>
        void DeleteArticleCategory(params long[] ids);
        /// <summary>
        /// 重名检查
        /// </summary>
        /// <param name="id">当前编号</param>
        /// <param name="name">待检查名称</param>
        /// <returns></returns>
        bool CheckHaveRename(long id, string name);
    }

    public enum SpecialCategory : int
    {
        /// <summary>
        /// 页脚服务
        /// </summary>
        PageFootService = 1,
        /// <summary>
        /// 系统快报
        /// </summary>
        SystemMeaasge = 2,

        /// <summary>
        ///  资讯中心
        /// </summary>
        InfoCenter = 3,
        /// <summary>
        /// 商家中心的平台公告
        /// </summary>
        PlatformNews = 4



    }
}
