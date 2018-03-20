using Himall.IServices;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Model;
using Himall.Core;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Areas.Admin.Models.Product;
using System.IO;
using Himall.DTO;
using Himall.Application;
using Himall.CommonModel;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class PageSettingsController : BaseAdminController
    {
        ISiteSettingService _iSiteSettingService;
        ISlideAdsService _iSlideAdsService;
        IBrandService _iBrandService;
        ITypeService _iTypeService;
        IHomeCategoryService _iHomeCategoryService;
        ICategoryService _iCategoryService;
        IFloorService _iFloorService;

        public PageSettingsController(
        ISiteSettingService iSiteSettingService,
        ISlideAdsService iSlideAdsService,
        IBrandService iBrandService,
        ITypeService iTypeService,
        IHomeCategoryService iHomeCategoryService,
        ICategoryService iCategoryService,
        IFloorService iFloorService
            )
        {
            _iSiteSettingService = iSiteSettingService;
            _iSlideAdsService = iSlideAdsService;
            _iBrandService = iBrandService;
            _iTypeService = iTypeService;
            _iHomeCategoryService = iHomeCategoryService;
            _iCategoryService = iCategoryService;
            _iFloorService = iFloorService;

        }
        // GET: Admin/PageSettings
        public ActionResult Index()
        {
            var settings = _iSiteSettingService.GetSiteSettings();
            ViewBag.Logo = settings.Logo;
            ViewBag.Keyword = settings.Keyword;
            ViewBag.Hotkeywords = settings.Hotkeywords;

            var imageAds = _iSlideAdsService.GetImageAds(0).ToList();

            ViewBag.ImageAdsTop = imageAds.Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.BannerAds).ToList();
            ViewBag.HeadAds = imageAds.Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.HeadRightAds).ToList();
            ViewBag.CenterAds = imageAds.Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.Customize).ToList();
            ViewBag.ShopAds = imageAds.Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.BrandsAds).ToList();
            ViewBag.BottomPic = CurrentSiteSetting.PCBottomPic;
			ViewBag.AdvertisementUrl=CurrentSiteSetting.AdvertisementUrl;
			ViewBag.AdvertisementImagePath=CurrentSiteSetting.AdvertisementImagePath;
			ViewBag.AdvertisementState=CurrentSiteSetting.AdvertisementState;

            var imageAdsz = imageAds.Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.Single).ToArray();
            return View(imageAdsz);
        }


        [UnAuthorize]
        public JsonResult GetBrandsAjax(long id = 0)
        {
            HomeFloorInfo homeFloor = null;
            IEnumerable<HomeFloorDetail.Brand> _Floorbrands = null;
            var brands = _iBrandService.GetBrands("");
            if (id != 0)
            {
                homeFloor = _iFloorService.GetHomeFloor(id);
                _Floorbrands = homeFloor.FloorBrandInfo.Select(
                    item => new HomeFloorDetail.Brand()
                    {
                        Id = item.BrandId,
                        Name = item.BrandInfo.Name
                    }
                    );
            }

            var data = new List<BrandViewModel>();
            foreach (var brand in brands)
            {
                data.Add(new Models.Product.BrandViewModel
                {
                    id = brand.Id,
                    isChecked = null == _Floorbrands ? false : _Floorbrands.Any(b => b.Id.Equals(brand.Id)),
                    value = brand.Name
                });
            }
            return Json(new
            {
                data = data
            }, JsonRequestBehavior.AllowGet);
        }

        #region 商品分类设置

        public ActionResult HomeCategory()
        {
            var homeCategorySets = _iHomeCategoryService.GetHomeCategorySets().ToArray();
            var models = homeCategorySets.Select(item => new Models.HomeCategory()
            {
                RowNumber = item.RowNumber,
                TopCategoryNames = GetTopLevelCategoryNames(item.HomeCategories.Select(category => category.CategoryId)),
                AllCategoryIds = item.HomeCategories.Select(category => category.CategoryId)
            }).OrderBy(item => item.RowNumber);
            return View(models);
        }

        IEnumerable<string> GetTopLevelCategoryNames(IEnumerable<long> categoryIds)
        {
            var productCategoryService = _iCategoryService;
            var topLevelCateogries = productCategoryService.GetTopLevelCategories(categoryIds);
            return topLevelCateogries.Select(item => item.Name);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SaveHomeCategory(string categoryIds, int rowNumber)
        {
            IEnumerable<long> ids;
            if (string.IsNullOrWhiteSpace(categoryIds))
                ids = new List<long>();
            else
                ids = categoryIds.Split(',').Select(item => long.Parse(item));
            _iHomeCategoryService.UpdateHomeCategorySet(rowNumber, ids);
            return Json(new
            {
                success = true
            });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult ChangeSequence(int oriRowNumber, int newRowNumber)
        {
            _iHomeCategoryService.UpdateHomeCategorySetSequence(oriRowNumber, newRowNumber);
            return Json(new
            {
                success = true
            });
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult SaveHomeTopic(int rowNumber, string url1, string imgUrl1, string url2, string imgUrl2)
        {
            var homeCategoryTopics = new HomeCategorySet.HomeCategoryTopic[] {
                 new HomeCategorySet.HomeCategoryTopic(){
                  Url = url1,
                  ImageUrl = imgUrl1
                 },
                 new HomeCategorySet.HomeCategoryTopic(){
                  ImageUrl = imgUrl2,
                   Url = url2
                 }
            };
            _iHomeCategoryService.UpdateHomeCategorySet(rowNumber, homeCategoryTopics);
            return Json(new
            {
                success = true
            });
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult GetHomeCategoryTopics(int rowNumber)
        {
            var homeCategorySet = _iHomeCategoryService.GetHomeCategorySet(rowNumber);
            int topicsCount = homeCategorySet.HomeCategoryTopics == null ? 0 : homeCategorySet.HomeCategoryTopics.Count();
            var topic1 = topicsCount > 0 ? homeCategorySet.HomeCategoryTopics.ElementAt(0) : null;
            var topic2 = topicsCount > 1 ? homeCategorySet.HomeCategoryTopics.ElementAt(1) : null;

            var model = new
            {
                imageUrl1 = topic1 == null ? "" : topic1.ImageUrl,
                url1 = topic1 == null ? "" : topic1.Url,
                imageUrl2 = topic2 == null ? "" : topic2.ImageUrl,
                url2 = topic2 == null ? "" : topic2.Url
            };

            return Json(model);
        }



        #endregion



        #region 楼层设置


        #region 基本信息

        public ActionResult HomeFloor()
        {
            var floors = _iFloorService.GetAllHomeFloors();
            var xxx = floors.ToList();
            var floorModels = floors.Select(item => new HomeFloor()
            {
                DisplaySequence = item.DisplaySequence,
                Enable = item.IsShow,
                Id = item.Id,
                Name = item.FloorName,
                StyleLevel = item.StyleLevel
            });
            return View(floorModels);
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult FloorChangeSequence(int oriRowNumber, int newRowNumber)
        {
            _iFloorService.UpdateHomeFloorSequence(oriRowNumber, newRowNumber);
            return Json(new
            {
                success = true
            });
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult FloorEnableDisplay(long id, bool enable)
        {
            _iFloorService.EnableHomeFloor(id, enable);
            return Json(new
            {
                success = true
            });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult DeleteFloor(long id)
        {
            _iFloorService.DeleteHomeFloor(id);
            return Json(new
            {
                success = true
            });
        }



        public ActionResult AddHomeFloor(long id = 0)
        {
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
                homeFloor = new HomeFloorInfo();
            ViewBag.TopLevelCategories = _iCategoryService.GetCategoryByParentId(0);
            return View(homeFloor);
        }


        public ActionResult AddFloorChoose()
        {

            return View();
        }


        [UnAuthorize]
        [HttpPost]
        public JsonResult SaveHomeFloorBasicInfo(long id, string name, string categoryIds)
        {
            string msg = string.Empty;
            bool success = false;
            if (string.IsNullOrWhiteSpace(name))
                msg = "楼层名称不能为空";
            else if (name.Trim().Length > 4)
                msg = "楼层名称长度不能超过4个字";
            else
            {
                name = name.Trim();
                try
                {
                    IEnumerable<long> categoryIds_long = categoryIds.Split(',').Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => long.Parse(item));
                    if (id > 0)
                        _iFloorService.UpdateFloorBasicInfo(id, name, categoryIds_long);
                    else
                    {
                        var basicInfo = _iFloorService.AddHomeFloorBasicInfo(name, categoryIds_long);
                        id = basicInfo.Id;
                    }
                    success = true;
                }
                catch (FormatException)
                {
                    msg = "商品分类编号有误";
                }

            }
            return Json(new
            {
                success = success,
                msg = msg,
                id = id
            });
        }

        #endregion


        #region 详细设置

        /// <summary>
        /// 楼层一
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 6; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;

                //填充文字链接
                homeFloorDetail.TextLinks = homeFloor.FloorTopicInfo.Where(item => item.TopicType == Position.Top).Select(item => new HomeFloorDetail.TextLink()
                {
                    Id = item.Id,
                    Name = item.TopicName,
                    Url = item.Url
                });

                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url,
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);
            }


            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }

        /// <summary>
        /// 楼层2
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail2(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 11; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.StyleLevel = 1;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;
                homeFloorDetail.DefaultTabName = homeFloor.DefaultTabName;

                //填充文字链接
                homeFloorDetail.TextLinks = homeFloor.FloorTopicInfo.Where(item => item.TopicType == Position.Top).Select(item => new HomeFloorDetail.TextLink()
                {
                    Id = item.Id,
                    Name = item.TopicName,
                    Url = item.Url
                });

                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url,
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);

                homeFloorDetail.Tabs = homeFloor.Himall_FloorTabls
                    .Where(item => item.FloorId == homeFloor.Id).OrderBy(p => p.Id)
                    .Select(item => new HomeFloorDetail.Tab()
                    {
                        Id = item.Id,
                        Detail = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id)
                        .Select(p => new HomeFloorDetail.ProductDetail()
                        {
                            Id = p.Id,
                            ProductId = p.ProductId
                        }),
                        Name = item.Name,
                        Count = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Count(),
                        Ids = ArrayToString(item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Select(p => p.ProductId).ToArray())
                    });
            }

            if (homeFloorDetail.Tabs == null)
            {
                homeFloorDetail.Tabs = new List<HomeFloorDetail.Tab>();
            }
            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }

        /// <summary>
        /// 楼层3
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail3(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 1; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.StyleLevel = 2;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;
                homeFloorDetail.DefaultTabName = homeFloor.DefaultTabName;

                //图片
                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url,
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);


                //商品
                homeFloorDetail.ProductModules = homeFloor.FloorProductInfo
                    .Where(item => item.FloorId == homeFloor.Id).OrderBy(p => p.Id)
                    .Select(item => new HomeFloorDetail.ProductModule()
                    {
                        Id = item.Id,
                        price = item.ProductInfo.MinSalePrice,
                        ProductId = item.ProductId,
                        productImg = HimallIO.GetProductSizeImage(item.ProductInfo.ImagePath, 1, (int)ImageSize.Size_50),
                        productName = item.ProductInfo.ProductName,
                        Tab = item.Tab
                    });
            }

            if (homeFloorDetail.Tabs == null)
            {
                homeFloorDetail.Tabs = new List<HomeFloorDetail.Tab>();
            }
            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }

        /// <summary>
        /// 楼层四
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail4(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 10; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.StyleLevel = 3;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;

                //填充文字链接
                homeFloorDetail.TextLinks = homeFloor.FloorTopicInfo.Where(item => item.TopicType == Position.Top).Select(item => new HomeFloorDetail.TextLink()
                {
                    Id = item.Id,
                    Name = item.TopicName,
                    Url = item.Url
                });

                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url,
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);
            }


            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }

        /// <summary>
        /// 默认楼层
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail5(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 12; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.StyleLevel = 1;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;
                homeFloorDetail.DefaultTabName = homeFloor.DefaultTabName;

                //填充文字链接
                homeFloorDetail.TextLinks = homeFloor.FloorTopicInfo.Where(item => item.TopicType == Position.Top).Select(item => new HomeFloorDetail.TextLink()
                {
                    Id = item.Id,
                    Name = item.TopicName,
                    Url = item.Url
                });

                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url,
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);

                homeFloorDetail.Tabs = homeFloor.Himall_FloorTabls
                    .Where(item => item.FloorId == homeFloor.Id).OrderBy(p => p.Id)
                    .Select(item => new HomeFloorDetail.Tab()
                    {
                        Id = item.Id,
                        Detail = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id)
                        .Select(p => new HomeFloorDetail.ProductDetail()
                        {
                            Id = p.Id,
                            ProductId = p.ProductId
                        }),
                        Name = item.Name,
                        Count = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Count(),
                        Ids = ArrayToString(item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Select(p => p.ProductId).ToArray())
                    });
            }

            if (homeFloorDetail.Tabs == null)
            {
                homeFloorDetail.Tabs = new List<HomeFloorDetail.Tab>();
            }
            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }
        /// <summary>
        /// 楼层五
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail6(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 12; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.StyleLevel = 5;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;
                homeFloorDetail.DefaultTabName = homeFloor.DefaultTabName;

                //填充文字链接
                homeFloorDetail.TextLinks = homeFloor.FloorTopicInfo.Where(item => item.TopicType == Position.Top).Select(item => new HomeFloorDetail.TextLink()
                {
                    Id = item.Id,
                    Name = item.TopicName,
                    Url = item.Url
                });

                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url,
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);

                homeFloorDetail.Tabs = homeFloor.Himall_FloorTabls
                    .Where(item => item.FloorId == homeFloor.Id).OrderBy(p => p.Id)
                    .Select(item => new HomeFloorDetail.Tab()
                    {
                        Id = item.Id,
                        Detail = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id)
                        .Select(p => new HomeFloorDetail.ProductDetail()
                        {
                            Id = p.Id,
                            ProductId = p.ProductId
                        }),
                        Name = item.Name,
                        Count = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Count(),
                        Ids = ArrayToString(item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Select(p => p.ProductId).ToArray())
                    });
            }

            if (homeFloorDetail.Tabs == null)
            {
                homeFloorDetail.Tabs = new List<HomeFloorDetail.Tab>();
            }
            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }
        /// <summary>
        /// 楼层六
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail7(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 16; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.StyleLevel = 1;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;
                homeFloorDetail.DefaultTabName = homeFloor.DefaultTabName;

                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url,
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);


                homeFloorDetail.Tabs = homeFloor.Himall_FloorTabls
                    .Where(item => item.FloorId == homeFloor.Id).OrderBy(p => p.Id)
                    .Select(item => new HomeFloorDetail.Tab()
                    {
                        Id = item.Id,
                        Detail = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id)
                        .Select(p => new HomeFloorDetail.ProductDetail()
                        {
                            Id = p.Id,
                            ProductId = p.ProductId
                        }),
                        Name = item.Name,
                        Count = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Count(),
                        Ids = ArrayToString(item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Select(p => p.ProductId).ToArray())
                    });
            }

            if (homeFloorDetail.Tabs == null)
            {
                homeFloorDetail.Tabs = new List<HomeFloorDetail.Tab>();
            }
            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }
        /// <summary>
        /// 楼层七
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail8(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 15; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.StyleLevel = 1;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;
                homeFloorDetail.DefaultTabName = homeFloor.DefaultTabName;

                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url,
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);
                

                homeFloorDetail.Tabs = homeFloor.Himall_FloorTabls
                    .Where(item => item.FloorId == homeFloor.Id).OrderBy(p => p.Id)
                    .Select(item => new HomeFloorDetail.Tab()
                    {
                        Id = item.Id,
                        Detail = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id)
                        .Select(p => new HomeFloorDetail.ProductDetail()
                        {
                            Id = p.Id,
                            ProductId = p.ProductId
                        }),
                        Name = item.Name,
                        Count = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Count(),
                        Ids = ArrayToString(item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Select(p => p.ProductId).ToArray())
                    });
            }

            if (homeFloorDetail.Tabs == null)
            {
                homeFloorDetail.Tabs = new List<HomeFloorDetail.Tab>();
            }
            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail9(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 13; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.StyleLevel = 1;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;
                homeFloorDetail.DefaultTabName = homeFloor.DefaultTabName;

                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url,
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);

                //填充文字链接
                homeFloorDetail.TextLinks = homeFloor.FloorTopicInfo.Where(item => item.TopicType == Position.Top).Select(item => new HomeFloorDetail.TextLink()
                {
                    Id = item.Id,
                    Name = item.TopicName,
                    Url = item.Url
                });
                homeFloorDetail.Tabs = homeFloor.Himall_FloorTabls
                    .Where(item => item.FloorId == homeFloor.Id).OrderBy(p => p.Id)
                    .Select(item => new HomeFloorDetail.Tab()
                    {
                        Id = item.Id,
                        Detail = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id)
                        .Select(p => new HomeFloorDetail.ProductDetail()
                        {
                            Id = p.Id,
                            ProductId = p.ProductId
                        }),
                        Name = item.Name,
                        Count = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Count(),
                        Ids = ArrayToString(item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Select(p => p.ProductId).ToArray())
                    });
            }

            if (homeFloorDetail.Tabs == null)
            {
                homeFloorDetail.Tabs = new List<HomeFloorDetail.Tab>();
            }
            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }
        private string ArrayToString(long[] array)
        {
            string ids = string.Empty;
            foreach (long id in array)
            {
                ids += id + ",";
            }
            return ids.Substring(0, ids.Length - 1);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SaveHomeFloorDetail(string floorDetail)
        {
            var floorDetailObj = Newtonsoft.Json.JsonConvert.DeserializeObject<HomeFloorDetail>(floorDetail);

            var homeFloor = new Model.HomeFloorInfo()
            {
                Id = floorDetailObj.Id,
                FloorName = floorDetailObj.Name,
                SubName = floorDetailObj.SubName,
                DefaultTabName = floorDetailObj.DefaultTabName,
                StyleLevel = floorDetailObj.StyleLevel,
                FloorBrandInfo = floorDetailObj.Brands.Select(item => new FloorBrandInfo()
                {
                    BrandId = item.Id,
                    FloorId = floorDetailObj.Id,
                }).ToList(),
                FloorTopicInfo = floorDetailObj.TextLinks.Select(i => new FloorTopicInfo
                {
                    FloorId = floorDetailObj.Id,
                    TopicImage = "",
                    TopicName = i.Name,
                    Url = i.Url,
                    TopicType = Position.Top
                }).ToList(),

            };
            var productLink = floorDetailObj.ProductLinks.Select(i => new FloorTopicInfo
                {
                    FloorId = floorDetailObj.Id,
                    TopicImage = String.IsNullOrWhiteSpace(i.Name) ? "" : i.Name,
                    TopicName = "",
                    Url = i.Url,
                    TopicType = (Position)i.Id
                }).ToList();

            foreach (var item in productLink)
            {
                homeFloor.FloorTopicInfo.Add(item);
            }

            if (floorDetailObj.Tabs != null)
            {
                homeFloor.Himall_FloorTabls = floorDetailObj.Tabs.Select(item => new FloorTablsInfo()
               {
                   Id = item.Id,
                   Name = item.Name,
                   FloorId = floorDetailObj.Id,
                   Himall_FloorTablDetails = item.Detail.Select(d => new FloorTablDetailsInfo()
                   {
                       Id = d.Id,
                       TabId = item.Id,
                       ProductId = d.ProductId
                   }).ToList()
               }).ToList();
            }

            if (floorDetailObj.ProductModules != null)
            {
                homeFloor.FloorProductInfo = floorDetailObj.ProductModules.Select(item => new FloorProductInfo()
                {
                    Id = item.Id,
                    FloorId = floorDetailObj.Id,
                    ProductId = item.ProductId,
                    Tab = item.Tab
                }).ToList();
            }

            _iFloorService.UpdateHomeFloorDetail(homeFloor);

            return Json(new
            {
                success = true
            });
        }


        #endregion




        #endregion

        #region LOGO图片设置

        /// <summary>
        /// LOGO图片设置
        /// </summary>
        /// <param name="logo"></param>
        /// <returns></returns>
        [HttpPost]
        [UnAuthorize]
        public JsonResult SetLogo(string logo)
        {
            string image = logo;
            if (image.Contains("/temp/"))
            {
                string source = image.Substring(image.LastIndexOf("/temp"));
                string dest = @"/Storage/Plat/Site/";
                var ext = Path.GetExtension(source);

                image = dest + "logo" + ext;
                Core.HimallIO.CopyFile(source, image, true);
            }
            else if (image.Contains("/Storage/"))
            {
                image = image.Substring(image.LastIndexOf("/Storage/"));
            }
            _iSiteSettingService.SaveSetting("Logo", image);
            return Json(new
            {
                success = true,
                logo = HimallIO.GetImagePath(image)
            });
        }

		/// <summary>
		/// 设置广告
		/// </summary>
		/// <param name="img"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		[HttpPost]
		[UnAuthorize]
		public JsonResult SetAdvertisement(string img, string url,bool open)
		{
			img = MoveImages(img, "Advertisement", "advertisement");
			_iSiteSettingService.SaveSetting("AdvertisementImagePath", img);
			_iSiteSettingService.SaveSetting("AdvertisementUrl", url);
			_iSiteSettingService.SaveSetting("AdvertisementState", open);

			return Json(new
			{
				success = true,
				img = HimallIO.GetImagePath(img)
			});
		}

        /// <summary>
        /// 头部广告图设置
        /// </summary>
        /// <param name="logo"></param>
        /// <returns></returns>
        [HttpPost]
        [UnAuthorize]
        public JsonResult SetHeadArea(string img)
        {
            img = MoveImages(img,"HeadArea","logo");
            _iSiteSettingService.SaveSetting("HeadArea", img);
            return Json(new
            {
                success = true,
                logo = HimallIO.GetImagePath(img)
            });
        }


        /// <summary>
        /// 转移LOGO图片
        /// </summary>
        /// <param name="id"></param>
        /// <param name="image"></param>
        /// <returns></returns>
		private string MoveImages(string image, string directoryName,string fileName)
        {
            //文件转移
            if (image.Contains("/temp/"))
            {
                string source = image.Substring(image.LastIndexOf("/temp"));
                string dest = @"/Storage/Plat/ImageAd/";
                var ext = Path.GetExtension(source);

				image = dest + directoryName + "/" + fileName + ext;
                Core.HimallIO.CopyFile(source, image, true);
            }
            else if (image.Contains("/Storage/"))
            {
                image = image.Substring(image.LastIndexOf("/Storage/"));
            }

            return image;
        }
        #endregion

        #region 底部服务图片

        [HttpPost]
        [UnAuthorize]
        public JsonResult SetBottomPic(string pic)
        {
            //文件转移
            if (pic.Contains("/temp/"))
            {
                string source = pic.Substring(pic.LastIndexOf("/temp"));
                string dest = @"/Storage/Plat/ImageAd/";
                var ext = Path.GetExtension(source);

                pic = dest + "PCBottomPic" + ext;
                Core.HimallIO.CopyFile(source, pic, true);
            }
            else if (pic.Contains("/Storage/"))
            {
                pic = pic.Substring(pic.LastIndexOf("/Storage/"));
            }

            _iSiteSettingService.SaveSetting("PCBottomPic", pic);
            return Json(new
            {
                success = true,
                pic = pic
            });
        }

        #endregion

        #region 关键字设置
        [HttpPost]
        [UnAuthorize]
        public JsonResult SetKeyWords(string keyword, string hotkeywords)
        {
            var sitesetting = _iSiteSettingService;
            var setting = sitesetting.GetSiteSettings();
            setting.Hotkeywords = hotkeywords;
            setting.Keyword = keyword;
            sitesetting.SetSiteSettings(setting);
            return Json(new Result
            {
                success = true
            });
        }
        #endregion

        public ViewResult Limittime()
        {
            var setting = _iSiteSettingService.GetSiteSettings();
            ViewBag.Limittime = setting.Limittime;
            return View();
        }

        /// <summary>
        /// 设置首页是否显示限时购
        /// </summary>
        /// <param name="Limittime">是否显示</param>
        /// <returns></returns>
        [HttpPost]
        [UnAuthorize]
        public JsonResult SetLimittime(bool Limittime)
        {
            var sitesetting = _iSiteSettingService;
            var setting = sitesetting.GetSiteSettings();
            setting.Limittime = Limittime;
            sitesetting.SetSiteSettings(setting);
            return Json(new Result
            {
                success = true
            });
        }

        #region 页脚设置

        [ValidateInput(false)]
        [UnAuthorize]
        [HttpPost]
        public JsonResult SetPageFoot(string content)
        {
            content = HTMLProcess(content);
            _iSiteSettingService.SaveSetting("PageFoot", content);
            return Json(new
            {
                success = true
            });
        }


        public ActionResult SetPageFoot()
        {
            var settings = _iSiteSettingService.GetSiteSettings();
            ViewBag.PageFoot = settings.PageFoot;
            return View();
        }


        /// <summary>
        /// 转移外站图片，去除script脚本
        /// </summary>
        /// <param name="content">html内容</param>
        /// <param name="id"></param>
        /// <returns></returns>
        string HTMLProcess(string content)
        {
            // string imageRealtivePath = details;
            //  content = Core.Helper.HtmlContentHelper.TransferToLocalImage(content, "/", imageRealtivePath, Core.HimallIO.GetImagePath(imageRealtivePath) + "/");
            // content = Core.Helper.HtmlContentHelper.RemoveScriptsAndStyles(content);
            string imageRealtivePath = "/Storage/Plat/PageSettings/PageFoot";
            string imageDirectory = Core.Helper.IOHelper.GetMapPath(imageRealtivePath);
            content = Core.Helper.HtmlContentHelper.TransferToLocalImage(content, "/", imageRealtivePath, Core.HimallIO.GetImagePath(imageRealtivePath) + "/");
            content = Core.Helper.HtmlContentHelper.RemoveScriptsAndStyles(content);
            return content;
        }



        #endregion


        #region  主题设置
        public ActionResult SetTheme()
        {
            return View(ThemeApplication.getTheme());
        }

        /// <summary>
        /// 修改主题设置
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <param name="typeId">0、默认；1、自定义主题</param>
        /// <param name="MainColor">主色</param>
        /// <param name="SecondaryColor">商城辅色</param>
        /// <param name="WritingColor">字体颜色</param>
        /// <param name="FrameColor">边框颜色</param>
        /// <param name="ClassifiedsColor">边框栏颜色</param>
        /// <returns></returns>
        public JsonResult updateTheme(long id, int typeId, string MainColor = "", string SecondaryColor = "", string WritingColor = "", string FrameColor = "", string ClassifiedsColor = "")
        {
            Theme mVTheme = new Theme()
            {
                ThemeId = id,
                TypeId = (Himall.CommonModel.ThemeType)typeId,
                MainColor = MainColor,
                SecondaryColor = SecondaryColor,
                WritingColor = WritingColor,
                FrameColor = FrameColor,
                ClassifiedsColor = ClassifiedsColor
            };

            ThemeApplication.SetTheme(mVTheme);

            return Json(new
            {
                status = 1
            });
        }
        #endregion
    }
}