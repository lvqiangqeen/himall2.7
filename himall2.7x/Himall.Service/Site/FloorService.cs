using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Himall.Service
{
    public class FloorService : ServiceBase, IFloorService
    {



        #region 首页楼层设置

        /// <summary>
        /// 显示顺序锁
        /// </summary>
        static object DisplaySequenceLocker = new object();

        public HomeFloorInfo AddHomeFloorBasicInfo(string name, IEnumerable<long> topLevelCategoryIds)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("楼层名称不能为空");
            if (topLevelCategoryIds == null || topLevelCategoryIds.Count() == 0)
                throw new ArgumentNullException("至少要选择一个商品分类");

            var homeFloorInfo = new HomeFloorInfo()
            {
                FloorName = name,
            };

            lock (DisplaySequenceLocker)//防止添加重复的顺序号
            {
                homeFloorInfo.DisplaySequence = GetMaxHomeFloorSequence() + 1;//设置显示序号

                foreach (var categoryId in topLevelCategoryIds.ToList())//添加楼层分类信息
                {
                    var floorCategory = new FloorCategoryInfo()
                    {
                        CategoryId = categoryId,
                        Depth = 1 //基本信息只保存一级分类
                    };
                    homeFloorInfo.FloorCategoryInfo.Add(floorCategory);
                }

                Context.HomeFloorInfo.Add(homeFloorInfo);
                Context.SaveChanges();
            }
            return homeFloorInfo;
        }

        public void UpdateFloorBasicInfo(long homeFloorId, string name, IEnumerable<long> topLevelCategoryIds)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("楼层名称不能为空");
            if (topLevelCategoryIds == null || topLevelCategoryIds.Count() == 0)
                throw new ArgumentNullException("至少要选择一个商品分类");

            var homeFloorBasicInfo = Context.HomeFloorInfo.FindById(homeFloorId);

            homeFloorBasicInfo.FloorName = name;

            topLevelCategoryIds = topLevelCategoryIds.Distinct();


            var allTopCategoryids = homeFloorBasicInfo.FloorCategoryInfo.Select(item => item.CategoryId);//当前楼层所有一级商品分类id
            var deletedTopLevelCategoryIds = allTopCategoryids.Where(item => !topLevelCategoryIds.Contains(item));//当前楼层待删除的一级商品分类id
            var newTopLevelCategoryIds = topLevelCategoryIds.Where(item => !allTopCategoryids.Contains(item));//待添加的一级商品分类id

            var newFloorCategories = new FloorCategoryInfo[newTopLevelCategoryIds.Count()];

            //构造新的楼层分类
            int i = 0;
            foreach (var categoryId in newTopLevelCategoryIds.ToList())
            {
                newFloorCategories[i++] = new FloorCategoryInfo()
                {
                    FloorId = homeFloorId,
                    CategoryId = categoryId,
                    Depth = 1
                };
            }

            Context.FloorCategoryInfo.Remove(item => item.FloorId == homeFloorId && deletedTopLevelCategoryIds.Contains(item.CategoryId));//删除待删除的
            Context.FloorCategoryInfo.AddRange(newFloorCategories);//添加新的楼层分类
            Context.SaveChanges();
        }

        public void UpdateHomeFloorSequence(long sourceSequence, long destiSequence)
        {
            var source = Context.HomeFloorInfo.FirstOrDefault(item => item.DisplaySequence == sourceSequence);
            var destination = Context.HomeFloorInfo.FirstOrDefault(item => item.DisplaySequence == destiSequence);

            source.DisplaySequence = destiSequence;
            destination.DisplaySequence = sourceSequence;
            Context.SaveChanges();
        }

        public void EnableHomeFloor(long homeFloorId, bool enable)
        {
            var homeFloor = Context.HomeFloorInfo.FindById(homeFloorId);

            homeFloor.IsShow = enable;
            Context.SaveChanges();
        }

        public IQueryable<HomeFloorInfo> GetAllHomeFloors()
        {
            var result = Context.HomeFloorInfo.Include("FloorTopicInfo")
                    .Include("FloorBrandInfo")
                    .Include("FloorBrandInfo.BrandInfo")
                    .OrderBy(a => a.DisplaySequence);

            return result;
        }

        public IQueryable<HomeFloorInfo> GetHomeFloors()
        {
            var result = Context.HomeFloorInfo.Include("FloorTopicInfo")
                .Include("FloorBrandInfo")
                .Include("FloorBrandInfo.BrandInfo")
                .Where(a => a.IsShow).OrderBy(a => a.DisplaySequence);
            //var result = (from F in context.HomeFloorInfo
            //                 .Include("FloorProductInfo.ProductInfo")
            //                 .Include("FloorCategoryInfo.CategoryInfo")
            //                 .Include("FloorProductInfo")
            //                 .Include("FloorCategoryInfo")
            //                 .Include("FloorTopicInfo")

            //              orderby F.DisplaySequence
            //              where F.IsShow
            //              select F).ToList();
            //var result1 = (from F in context.HomeFloorInfo
            //                 .Include("FloorBrandInfo.BrandInfo")
            //                 .Include("FloorBrandInfo")
            //               orderby F.DisplaySequence
            //               where F.IsShow
            //               select F).ToList();
            //foreach (var item in result1)
            //{
            //    foreach (var b in item.FloorBrandInfo)
            //    {
            //        result.FirstOrDefault(i => i.Id == item.Id).FloorBrandInfo.Add(b);

            //    }
            //}
            return result;
        }


        public HomeFloorInfo GetHomeFloor(long id)
        {
            return Context.HomeFloorInfo.FindById(id);
        }

        /// <summary>
        /// 获取最大首页楼层序号
        /// </summary>
        /// <returns></returns>
        long GetMaxHomeFloorSequence()
        {
            long maxSequence = 0;
            if (Context.HomeFloorInfo.Count() > 0)
                maxSequence = Context.HomeFloorInfo.Max(item => item.DisplaySequence);
            return maxSequence;
        }



        public void DeleteHomeFloor(long id)
        {
            var tabs = Context.FloorTablsInfo.Where(p => p.FloorId == id).Select(p => p.Id).ToList();
            foreach (var tabid in tabs)
            {
                Context.FloorTablDetailsInfo.RemoveRange(Context.FloorTablDetailsInfo.Where(p => p.TabId == tabid));
            }
            Context.FloorTablsInfo.RemoveRange(Context.FloorTablsInfo.Where(p => p.FloorId == id));
            Context.HomeFloorInfo.Remove(id);
            Context.SaveChanges();
            RemoveImage(id);//删除图片
        }


        public void UpdateHomeFloorDetail(HomeFloorInfo homeFloor)
        {
            if (homeFloor.Id == 0)
            {
                long? sequence = 0;
                var floor = Context.HomeFloorInfo.Select(p => p.DisplaySequence);
                if (floor.Count() > 0)
                    sequence = floor.Max();
                if (!sequence.HasValue)
                    sequence = 0;
                homeFloor.DisplaySequence = sequence.Value + 1;
                homeFloor.IsShow = true;
                Context.HomeFloorInfo.Add(homeFloor);
            }
            else
            {
                var f = Context.HomeFloorInfo.FindById(homeFloor.Id);
                f.DefaultTabName = homeFloor.DefaultTabName;
                f.FloorName = homeFloor.FloorName;
                f.SubName = homeFloor.SubName;
            }
            //UpdateCategory(homeFloor.Id, homeFloor.FloorCategoryInfo);

            if (homeFloor.StyleLevel == 1 || homeFloor.StyleLevel == 4 || homeFloor.StyleLevel == 5 || homeFloor.StyleLevel == 6 || homeFloor.StyleLevel == 7)
            {
                UpdateProducts(homeFloor.Id, homeFloor.Himall_FloorTabls);
            }


            UpdateBrand(homeFloor.Id, homeFloor.FloorBrandInfo);

            //UpdateCategoryImage(homeFloor.Id, homeFloor.FloorTopicInfo.FirstOrDefault(item => item.TopicType == Position.Top));

            UpdateTextLink(homeFloor.Id, homeFloor.FloorTopicInfo.Where(item => item.TopicType == Position.Top));

            UpdataProductLink(homeFloor.Id, homeFloor.FloorTopicInfo.Where(item => item.TopicType != Position.Top));
            UpdateProductModule(homeFloor);
            Context.Configuration.ValidateOnSaveEnabled = false;
            Context.SaveChanges();//保存更改
            Context.Configuration.ValidateOnSaveEnabled = true;
        }

        private void UpdateProducts(long floorId, IEnumerable<FloorTablsInfo> tabs)
        {
            //var ids = tabs.Where( item => item.FloorId == floorId ).Select( item => item.Id );
            //获取该删除的所有选项卡
            var ids = Context.FloorTablsInfo.Where(item => item.FloorId == floorId).Select(item => item.Id);
            //先删除所有选项卡的关联商品
            Context.FloorTablDetailsInfo.Remove(item => ids.Contains(item.TabId));
            //再删除选项卡
            Context.FloorTablsInfo.Remove(item => item.FloorId == floorId);
            Context.FloorTablsInfo.AddRange(tabs);

            foreach (FloorTablsInfo tab in tabs)
            {
                Context.FloorTablDetailsInfo.AddRange(tab.Himall_FloorTablDetails);
            }

        }

        private void UpdataProductLink(long floorId, IEnumerable<FloorTopicInfo> productLink)
        {
            //删除原文字链接
            Context.FloorTopicInfo.Remove(item => item.FloorId == floorId && item.TopicType != Position.Top);
            foreach (var item in productLink)
            {
                item.TopicImage = TransferImage(item.TopicImage, floorId, item.TopicType.ToString());
            }
            //重新添加
            Context.FloorTopicInfo.AddRange(productLink);
        }


        /// <summary>
        /// 更新 楼层所包含的分类
        /// </summary>
        /// <param name="floorCategories"></param>
        void UpdateCategory(long floorId, IEnumerable<FloorCategoryInfo> floorCategories)
        {
            var oriCategories = Context.FloorCategoryInfo.FindBy(item => item.FloorId == floorId && item.Depth >= 2);//查询该楼层下所有二级分类

            var oriCategoryIds = oriCategories.Select(item => item.CategoryId);//原所有分类id
            var newCategoryIds = floorCategories.Select(item => item.CategoryId);//新的分类id

            //查出要删除的分类并删除 
            var needToDelete = oriCategories.Where(item => !newCategoryIds.Contains(item.CategoryId));
            Context.FloorCategoryInfo.RemoveRange(needToDelete);

            //查出要添加的分类并添加
            var needToAdd = floorCategories.Where(item => !oriCategoryIds.Contains(item.CategoryId));
            Context.FloorCategoryInfo.AddRange(needToAdd);

        }

        /// <summary>
        /// 保存品牌
        /// </summary>
        /// <param name="floorBrands"></param>
        void UpdateBrand(long floorId, IEnumerable<FloorBrandInfo> floorBrands)
        {
            var oriBrands = Context.FloorBrandInfo.FindBy(item => item.FloorId == floorId);//查询该楼层下所有品牌

            var oriBrandIds = oriBrands.Select(item => item.BrandId);//原所有品牌id
            var newBrandIds = floorBrands.Select(item => item.BrandId);//新的品牌id

            //查出要删除的分类并删除 
            var needToDelete = oriBrands.Where(item => !newBrandIds.Contains(item.BrandId));
            Context.FloorBrandInfo.RemoveRange(needToDelete);

            //查出要添加的分类并添加
            var needToAdd = floorBrands.Where(item => !oriBrandIds.Contains(item.BrandId));
            Context.FloorBrandInfo.AddRange(needToAdd);
        }

        /// <summary>
        /// 保存分类图片
        /// </summary>
        void UpdateCategoryImage(long floorId, FloorTopicInfo categoryImage)
        {
            if (categoryImage != null)
            {
                bool isUpdate = false;
                var oriCategoryImage = Context.FloorTopicInfo.FirstOrDefault(item => item.FloorId == floorId && item.TopicType == Position.Top);
                if (oriCategoryImage == null)
                    oriCategoryImage = new FloorTopicInfo()
                    {
                        TopicName = "",
                        FloorId = floorId
                    };
                else
                    isUpdate = true;
                oriCategoryImage.TopicType = Position.Top;
                oriCategoryImage.Url = categoryImage.Url;
                oriCategoryImage.TopicImage = TransferImage(categoryImage.TopicImage, floorId, Position.Top.ToString());
                if (!isUpdate)
                    Context.FloorTopicInfo.Add(oriCategoryImage);
            }
        }

        /// <summary>
        /// 移动图片
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <returns></returns>
        string TransferImage(string sourceFile, long floorId, string type)
        {
            if (!string.IsNullOrWhiteSpace(sourceFile) && !sourceFile.Contains("/Storage/Plat/"))
            {
                string newDir = "/Storage/Plat/PageSettings/HomeFloor/" + floorId + "/";

                string ext = sourceFile.Substring(sourceFile.LastIndexOf('.'));//得到扩展名
                string newName = "floor_" + type + "_" + System.DateTime.Now.ToString("yyyyMMddHHmmssffffff") + ext;//得到新的文件名

                if (!string.IsNullOrWhiteSpace(sourceFile))
                {
                    if (sourceFile.Contains("/temp/"))
                    {
                        string logoname = sourceFile.Substring(sourceFile.LastIndexOf('/') + 1);
                        string oldlogo = sourceFile.Substring(sourceFile.LastIndexOf("/temp"));
                        string newLogo = newDir + newName;
                        Core.HimallIO.CopyFile(oldlogo, newLogo, true);
                        sourceFile = newLogo;
                        return sourceFile;//返回新的文件路径
                    }
                    else if (sourceFile.Contains("/Storage/"))
                    {
                        sourceFile = sourceFile.Substring(sourceFile.LastIndexOf("/Storage"));
                    }
                }
            }
            else if (sourceFile.Contains("/Storage/"))
            {
                sourceFile = sourceFile.Substring(sourceFile.LastIndexOf("/Storage"));
            }

            return sourceFile;
        }

        void RemoveImage(long id, string type)
        {
            string filePath = "/Storage/Plat/PageSettings/HomeFloor/" + id+"/";
            var files = Core.HimallIO.GetDirAndFiles(filePath);
            foreach (var file in files)
            {
                if (file.IndexOf("floor_" + type + "_",StringComparison.OrdinalIgnoreCase) > -1)
                {
                    try
                    {
                        System.IO.File.Delete(file);
                    }
                    catch
                    {
                    }
                }
            }
        }

        void RemoveImage(long id)
        {
            try
            {
                string path = "/Storage/Plat/PageSettings/HomeFloor/" + id+"/";

                Core.HimallIO.DeleteDir(path,true);
            }
            catch { }
        }

        /// <summary>
        /// 保存文字链接
        /// </summary>
        /// <param name="floorId"></param>
        /// <param name="textLinks"></param>
        void UpdateTextLink(long floorId, IEnumerable<FloorTopicInfo> textLinks)
        {
            //删除原文字链接
            Context.FloorTopicInfo.Remove(item => item.FloorId == floorId && item.TopicType == Position.Top);

            //重新添加
            Context.FloorTopicInfo.AddRange(textLinks);

        }


        void UpdateProductModule(HomeFloorInfo homeFloor)
        {
            //保存商品信息
            string[] tabNames = new string[] { "特价商品" };
            Context.FloorProductInfo.Remove(item => item.FloorId == homeFloor.Id);
            Context.FloorProductInfo.AddRange(homeFloor.FloorProductInfo);
            var oriHomeFloor = Context.HomeFloorInfo.FindById(homeFloor.Id);
            int i = 0;

            //保存专题信息
            //context.FloorTopicInfo.Remove(item => item.FloorId == homeFloor.Id && item.TopicType == Position.MiddleOne);

            //var productTopics = homeFloor.FloorTopicInfo.Where(item => item.TopicType == Position.MiddleOne).ToArray();
            //foreach (var topic in productTopics)
            //{
            //    topic.TopicImage = TransferImage(topic.TopicImage, homeFloor.Id, Position.MiddleOne.ToString());
            //}
            //context.FloorTopicInfo.AddRange(productTopics);

        }

        #endregion


    }
}
