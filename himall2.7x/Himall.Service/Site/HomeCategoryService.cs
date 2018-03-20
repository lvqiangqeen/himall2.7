using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Himall.Service
{
    public class HomeCategoryService : ServiceBase, IHomeCategoryService
    {

        #region 首页分类设置

        /// <summary>
        /// 默认分类组数
        /// </summary>
        const int HOME_CATEGORY_SET_COUNT = 14;


        public int TotalRowsCount
        {
            get { return HOME_CATEGORY_SET_COUNT; }
        }

        public IEnumerable<HomeCategorySet> GetHomeCategorySets()
        {
            var homeCategorySet = new HomeCategorySet[HOME_CATEGORY_SET_COUNT];

            if (Cache.Exists(CacheKeyCollection.HomeCategory))
            {
                homeCategorySet = Core.Cache.Get<HomeCategorySet[]>(CacheKeyCollection.HomeCategory);
            }
            else
            {

                //var homeCategoryGroups = context.HomeCategoryInfo.FindAll().GroupBy(item => item.RowNumber);
                var cc =
                    (from C in Context.HomeCategoryInfo
                    .Include("CategoryInfo")
                     where C.CategoryInfo.IsDeleted == false
                     select C).ToList();

                var homeCategoryGroups =
                    (from C in cc
                     group C by C.RowNumber into G
                     select G).ToList();

                foreach (var group in homeCategoryGroups)
                {
                    homeCategorySet[group.Key - 1] = new HomeCategorySet()
                    {
                        RowNumber = group.Key,
                        HomeCategories = group
                    };
                }
                var _HomeCategoryRowInfo = Context.HomeCategoryRowInfo.FindAll().ToList();
                for (int i = 0; i < HOME_CATEGORY_SET_COUNT; i++)
                {
                    if (homeCategorySet[i] == null)
                    {
                        homeCategorySet[i] = new HomeCategorySet()
                        {
                            RowNumber = i + 1,
                            HomeCategories = new List<HomeCategoryInfo>()
                        };
                    }
                    var homeCategoryRows = new List<HomeCategorySet.HomeCategoryTopic>();
                    var categoryRowInfo = _HomeCategoryRowInfo.Where(item => item.RowId == i + 1).FirstOrDefault();
                    if (categoryRowInfo != null)
                    {
                        homeCategoryRows.Add(new HomeCategorySet.HomeCategoryTopic() { Url = categoryRowInfo.Url1, ImageUrl = categoryRowInfo.Image1 });
                        homeCategoryRows.Add(new HomeCategorySet.HomeCategoryTopic() { Url = categoryRowInfo.Url2, ImageUrl = categoryRowInfo.Image2 });
                    }
                    homeCategorySet[i].HomeCategoryTopics = homeCategoryRows;

                }

                var brandService = ObjectContainer.Current.Resolve<IBrandService>();

                for (int i = 0; i < homeCategorySet.Count(); i++)
                {
                    homeCategorySet[i].HomeCategories = homeCategorySet[i].HomeCategories.Select(t => new HomeCategoryInfo()
                    {
                        CategoryId = t.CategoryId,
                        Depth = t.Depth,
                        Id = t.Id,
                        RowNumber = t.RowNumber,
                        CategoryInfo = new CategoryInfo()
                        {
                            Depth = t.CategoryInfo.Depth,
                            Name = t.CategoryInfo.Name,
                            RewriteName = t.CategoryInfo.RewriteName,
                            TypeId = t.CategoryInfo.TypeId,
                            CommisRate = t.CategoryInfo.CommisRate,
                            DisplaySequence = t.CategoryInfo.DisplaySequence,
                            HasChildren = t.CategoryInfo.HasChildren,
                            Icon = t.CategoryInfo.Icon,
                            Path = t.CategoryInfo.Path,
                            Meta_Title = t.CategoryInfo.Meta_Title,
                            ParentCategoryId = t.CategoryInfo.ParentCategoryId,
                            Meta_Keywords = t.CategoryInfo.Meta_Keywords,
                            Meta_Description = t.CategoryInfo.Meta_Description,
                            Id = t.CategoryInfo.Id
                        }
                    }).ToList();
                    homeCategorySet[i].HomeCategoryTopics = homeCategorySet[i].HomeCategoryTopics.Select(t => new HomeCategorySet.HomeCategoryTopic()
                    {
                        ImageUrl = t.ImageUrl,
                        Url = t.Url
                    }).ToList();


                    const int MAX_DISPLAY_BRANDS = 8;//最多显示推荐品牌个数
                    //获取推荐的品牌
                    var singleRowBrand = brandService.GetBrandsByCategoryIds(homeCategorySet[i].HomeCategories.Where(item => item.Depth == 1).Select(item => item.CategoryId).ToArray()).Take(MAX_DISPLAY_BRANDS);
                    BrandInfo[] brands = new BrandInfo[singleRowBrand.Count()];
                    for (int k = 0; k < singleRowBrand.Count(); k++)
                    {
                        brands[k] = new BrandInfo();
                        brands[k].Id = singleRowBrand.ElementAt(k).Id;
                        brands[k].Name = singleRowBrand.ElementAt(k).Name;
                        brands[k].Logo = singleRowBrand.ElementAt(k).Logo;
                    }
                    homeCategorySet[i].HomeBrand = brands;
                }

                Cache.Insert<HomeCategorySet[]>(CacheKeyCollection.HomeCategory, homeCategorySet, 5);
            }
            return homeCategorySet;
        }

        public HomeCategorySet GetHomeCategorySet(int rowNumber)
        {
            if (rowNumber > HOME_CATEGORY_SET_COUNT || rowNumber < 0)
                throw new ArgumentNullException("行号不在取值范围内！取值必须大于0且小于" + HOME_CATEGORY_SET_COUNT);

            var homeCategorySet = new HomeCategorySet();
            homeCategorySet.RowNumber = rowNumber;
            homeCategorySet.HomeCategories = Context.HomeCategoryInfo.FindBy(item => item.RowNumber == rowNumber);
            var categoryRowInfo = Context.HomeCategoryRowInfo.FindBy(item => item.RowId == rowNumber).FirstOrDefault();

            var homeCategoryRows = new List<HomeCategorySet.HomeCategoryTopic>();
            homeCategoryRows.Add(new HomeCategorySet.HomeCategoryTopic() { Url = categoryRowInfo.Url1, ImageUrl = categoryRowInfo.Image1 });
            homeCategoryRows.Add(new HomeCategorySet.HomeCategoryTopic() { Url = categoryRowInfo.Url2, ImageUrl = categoryRowInfo.Image2 });
            homeCategorySet.HomeCategoryTopics = homeCategoryRows;

            return homeCategorySet;
        }

        public void UpdateHomeCategorySet(HomeCategorySet homeCategorySet)
        {
            if (homeCategorySet.HomeCategories == null)
                throw new ArgumentNullException("传入的分类不能为null，但可以是空集合");

            int rowNumber = homeCategorySet.HomeCategories.FirstOrDefault().RowNumber;
            if (rowNumber > HOME_CATEGORY_SET_COUNT || rowNumber < 0)
                throw new ArgumentNullException("行号不在取值范围内！取值必须大于0且小于" + HOME_CATEGORY_SET_COUNT);


            Context.HomeCategoryInfo.Remove(item => item.RowNumber == rowNumber);//移除原所有首页分类

            //填充行号
            foreach (var homeCategory in homeCategorySet.HomeCategories.ToList())
                homeCategory.RowNumber = rowNumber;

            Context.HomeCategoryInfo.AddRange(homeCategorySet.HomeCategories);//添加新的首页分类
            Context.SaveChanges();

        }


        public void UpdateHomeCategorySet(int rowNumber, IEnumerable<long> categoryIds)
        {
            if (rowNumber > HOME_CATEGORY_SET_COUNT || rowNumber < 0)
                throw new ArgumentNullException("行号不在取值范围内！取值必须大于0且小于" + HOME_CATEGORY_SET_COUNT);

            var categoryService = new CategoryService();
            var categoriesCount = categoryIds.Count();
            var homeCategories = new HomeCategoryInfo[categoriesCount];
            long categoryId;
            for (var i = 0; i < categoriesCount; i++)
            {
                categoryId = categoryIds.ElementAt(i);
                homeCategories[i] = new HomeCategoryInfo()
                {
                    RowNumber = rowNumber,
                    CategoryId = categoryId,
                    Depth = categoryService.GetCategory(categoryId).Depth

                };
            }
            Context.HomeCategoryInfo.Remove(item => item.RowNumber == rowNumber);//移除原所有首页分类
            Context.HomeCategoryInfo.AddRange(homeCategories);
            Context.SaveChanges();
        }


        public void UpdateHomeCategorySetSequence(int sourceRowNumber, int destiRowNumber)
        {

            if (sourceRowNumber > HOME_CATEGORY_SET_COUNT || sourceRowNumber < 0)
                throw new ArgumentNullException("原行号不在取值范围内！取值必须大于0且小于" + HOME_CATEGORY_SET_COUNT);
            if (destiRowNumber > HOME_CATEGORY_SET_COUNT || destiRowNumber < 0)
                throw new ArgumentNullException("新行号不在取值范围内！取值必须大于0且小于" + HOME_CATEGORY_SET_COUNT);

            IQueryable<HomeCategoryInfo> sources, destinations;
            sources = Context.HomeCategoryInfo.FindBy(item => item.RowNumber == sourceRowNumber);
            destinations = Context.HomeCategoryInfo.FindBy(item => item.RowNumber == destiRowNumber);
            foreach (var source in sources.ToList())
                source.RowNumber = destiRowNumber;
            foreach (var destination in destinations.ToList())
                destination.RowNumber = sourceRowNumber;

            Context.SaveChanges();
        }


        public void UpdateHomeCategorySet(int rowNumber, IEnumerable<HomeCategorySet.HomeCategoryTopic> homeCategoryTopics)
        {
            if (rowNumber > HOME_CATEGORY_SET_COUNT || rowNumber < 0)
                throw new ArgumentNullException("行号不在取值范围内！取值必须大于0且小于" + HOME_CATEGORY_SET_COUNT);

            HomeCategoryRowInfo rowInfo;
            var oldRowInfo = Context.HomeCategoryRowInfo.FindBy(item => item.RowId == rowNumber).FirstOrDefault();
            if (oldRowInfo == null)
                rowInfo = new HomeCategoryRowInfo() { RowId = rowNumber };
            else
                rowInfo = oldRowInfo;

            int i = 0;
            string[] needToDeleteFiles = new string[2];
            foreach (var item in homeCategoryTopics.ToList())
            {
                if (!string.IsNullOrWhiteSpace(item.Url) && !string.IsNullOrWhiteSpace(item.ImageUrl))
                {
                    if (i++ == 0)
                    {
                        if (rowInfo.Image1 != item.ImageUrl)//当图片有修改时，删除原图片
                            needToDeleteFiles[0] = rowInfo.Image1;

                        rowInfo.Image1 = item.ImageUrl;
                        rowInfo.Url1 = item.Url;
                    }
                    else
                    {
                        if (rowInfo.Image2 != item.ImageUrl)//当图片有修改时，删除原图片
                            needToDeleteFiles[1] = rowInfo.Image2;

                        rowInfo.Image2 = item.ImageUrl;
                        rowInfo.Url2 = item.Url;
                    }
                    if (!string.IsNullOrWhiteSpace(item.ImageUrl))
                        TransferImages(item.ImageUrl);
                }
            }
            if (oldRowInfo == null)
                Context.HomeCategoryRowInfo.Add(rowInfo);
            Context.SaveChanges();

            //删除原图片
            foreach (var file in needToDeleteFiles)
            {
                if (!string.IsNullOrWhiteSpace(file))
                {
                    HimallIO.DeleteFile(file);
                }
            }
        }

        string TransferImages(string oriImageUrl)
        {
            string newDir = "/Storage/Plat/PageSettings/HomeCategory";
            string newFileName = oriImageUrl;
            if (!string.IsNullOrWhiteSpace(newFileName))
            {
                if (newFileName.Contains("/temp/"))
                {
                    string ext = oriImageUrl.Substring(newFileName.LastIndexOf('.'));
                    string newName = Guid.NewGuid().ToString("N") + ext;
                    string oldlogo = newFileName.Substring(newFileName.LastIndexOf("/temp"));
                    string newLogo = newDir + newName;
                    Core.HimallIO.CopyFile(oldlogo, newLogo, true);
                    newFileName = newLogo;
                }
                else if (newFileName.Contains("/Storage/"))
                {
                    newFileName = newFileName.Substring(newFileName.LastIndexOf("/Storage"));
                }
            }
            return newFileName;
        }





        #endregion



    }
}
