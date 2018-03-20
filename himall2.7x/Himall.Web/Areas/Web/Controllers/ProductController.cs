using Himall.CommonModel;
using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Areas.Web.Helper;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Himall.Application;

namespace Himall.Web.Areas.Web.Controllers
{
    public class ProductController : BaseWebController
    {
        #region 字段
        private IProductService _iProductService;
        private IFreightTemplateService _iFreightTemplateService;
        private ICommentService _iCommentService;
        private ITypeService _iTypeService;
        #endregion

        public ProductController(IProductService iProductService, IFreightTemplateService iFreightTemplateService, ICommentService iCommentService, ITypeService iTypeService)
        {
            _iProductService = iProductService;
            _iFreightTemplateService = iFreightTemplateService;
            _iCommentService = iCommentService;
            _iTypeService = iTypeService;
        }

        #region Ajax加载商品信息

        #region 商品咨询
        public JsonResult GetConsultationByProduct(long pId, int pageNo, int pageSize = 3)
        {
            var consultations = ServiceHelper.Create<IConsultationService>().GetConsultations(pId);
            if (consultations != null && consultations.Count() > 0)
            {
                var temp = consultations.OrderByDescending(a => a.ConsultationDate).Skip((pageNo - 1) * pageSize).Take(pageSize).ToArray();
                var data = from c in temp
                           select new
                           {
                               UserName = c.UserName,
                               ConsultationContent = c.ConsultationContent,
                               ConsultationDate = c.ConsultationDate.ToString("yyyy-MM-dd HH:mm:ss"),
                               ReplyContent = string.IsNullOrWhiteSpace(c.ReplyContent) ? "暂无回复" : c.ReplyContent,
                               ReplyDate = c.ReplyDate.HasValue ? c.ReplyDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : " ",
                           };
                return Json(new
                {
                    successful = true,
                    consults = data,
                    totalPage = (int)Math.Ceiling((decimal)consultations.Count() / pageSize),
                    currentPage = pageNo,

                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

        }

        #endregion

        #region 商品评价
        /// <summary>
        /// 获取商品评论
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pageNo"></param>
        /// <returns></returns>
        public JsonResult GetCommentByProduct(long pId, int pageNo, int commentType = 0, int pageSize = 3)
        {
            IEnumerable<ProductCommentInfo> result;
            var comments = ServiceHelper.Create<ICommentService>().GetCommentsByProductId(pId);
            // comments = comments.Where(a => a.IsHidden.Value == false);
            if (comments != null && comments.Count() > 0)
            {
                switch (commentType)
                {
                    case 1:
                        result = comments.Where(c => c.ReviewMark >= 4).OrderByDescending(c => c.ReviewMark);
                        break;
                    case 2:
                        result = comments.Where(c => c.ReviewMark == 3).OrderByDescending(c => c.ReviewMark);
                        break;
                    case 3:
                        result = comments.Where(c => c.ReviewMark <= 2).OrderByDescending(c => c.ReviewMark);
                        break;
                    case 4:
                        result = comments.Where(c => c.Himall_ProductCommentsImages.Count > 0);
                        break;
                    case 5:
                        result = comments.Where(c => c.AppendDate.HasValue);
                        break;
                    default:
                        result = comments.OrderByDescending(c => c.ReviewMark);
                        break;
                }
                var temp = result.OrderByDescending(a => a.ReviewDate).Skip((pageNo - 1) * pageSize).Take(pageSize).ToArray();
                var data = temp.Select(c =>
                {
                    ProductTypeInfo typeInfo = ServiceHelper.Create<ITypeService>().GetTypeByProductId(c.ProductId);
                    string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                    return new
                    {
                        UserName = c.UserName,
                        ReviewContent = c.ReviewContent,
                        AppendContent = c.AppendContent,
                        AppendDate = c.AppendDate.HasValue ? c.AppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                        ReplyAppendContent = c.ReplyAppendContent,
                        ReplyAppendDate = c.ReplyAppendDate.HasValue ? c.ReplyAppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                        FinshDate = c.Himall_OrderItems.OrderInfo.FinishDate.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                        Images = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 0).Select(a => new { CommentImage = Core.HimallIO.GetImagePath(a.CommentImage) }),
                        AppendImages = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 1).Select(a => new { CommentImage = Core.HimallIO.GetImagePath(a.CommentImage) }),
                        ReviewDate = c.ReviewDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        ReplyContent = string.IsNullOrWhiteSpace(c.ReplyContent) ? "暂无回复" : c.ReplyContent,
                        ReplyDate = c.ReplyDate.HasValue ? c.ReplyDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : " ",
                        ReviewMark = c.ReviewMark,
                        BuyDate = c.Himall_OrderItems.OrderInfo.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        Color = c.Himall_OrderItems.Color == null ? "" : c.Himall_OrderItems.Color,
                        Version = c.Himall_OrderItems.Version == null ? "" : c.Himall_OrderItems.Version,
                        Size = c.Himall_OrderItems.Size == null ? "" : c.Himall_OrderItems.Size,
                        ColorAlias = colorAlias,
                        SizeAlias = sizeAlias,
                        VersionAlias = versionAlias
                    };
                }).ToArray();

                //var data = from c in temp
                //           select new
                //           {
                //               UserName = c.UserName,
                //               ReviewContent = c.ReviewContent,
                //               AppendContent = c.AppendContent,
                //               AppendDate = c.AppendDate.HasValue ? c.AppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                //               ReplyAppendContent = c.ReplyAppendContent,
                //               ReplyAppendDate = c.ReplyAppendDate.HasValue ? c.ReplyAppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                //               FinshDate = c.Himall_OrderItems.OrderInfo.FinishDate.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                //               Images = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 0).Select(a => new { CommentImage = Core.HimallIO.GetImagePath(a.CommentImage) }),
                //               AppendImages = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 1).Select(a => new { CommentImage = Core.HimallIO.GetImagePath(a.CommentImage) }),
                //               ReviewDate = c.ReviewDate.ToString("yyyy-MM-dd HH:mm:ss"),
                //               ReplyContent = string.IsNullOrWhiteSpace(c.ReplyContent) ? "暂无回复" : c.ReplyContent,
                //               ReplyDate = c.ReplyDate.HasValue ? c.ReplyDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : " ",
                //               ReviewMark = c.ReviewMark,
                //               BuyDate = c.Himall_OrderItems.OrderInfo.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                //               Color = c.Himall_OrderItems.Color == null ? "" : c.Himall_OrderItems.Color,
                //               Version = c.Himall_OrderItems.Version == null ? "" : c.Himall_OrderItems.Version,
                //               Size = c.Himall_OrderItems.Size == null ? "" : c.Himall_OrderItems.Size,

                //           };
                return Json(new
                {
                    successful = true,
                    comments = data,
                    totalPage = (int)Math.Ceiling((decimal)result.Count() / pageSize),
                    pageSize = pageSize,
                    currentPage = pageNo,
                    goodComment = comments.Where(c => c.ReviewMark >= 4).Count(),
                    badComment = comments.Where(c => c.ReviewMark <= 2).Count(),
                    comment = comments.Where(c => c.ReviewMark == 3).Count(),
                    hasAppend = comments.Where(c => c.AppendDate.HasValue).Count(),
                    hasImages = comments.Where(c => c.Himall_ProductCommentsImages.Count > 0).Count(),
                    commentType = commentType

                }, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 店铺分类
        [HttpGet]
        public JsonResult GetShopCate(long gid)
        {
            var ShopCategory = new List<CategoryJsonModel>();
            var product = ServiceHelper.Create<IProductService>().GetProduct(gid);
            var categories = ServiceHelper.Create<IShopCategoryService>().GetShopCategory(product.ShopId).ToList();
            foreach (var main in categories.Where(s => s.ParentCategoryId == 0))
            {
                var topC = new CategoryJsonModel()
                {
                    Name = main.Name,
                    Id = main.Id.ToString(),
                    SubCategory = new List<SecondLevelCategory>()
                };
                foreach (var secondItem in categories.Where(s => s.ParentCategoryId == main.Id).ToList())
                {
                    var secondC = new SecondLevelCategory()
                    {
                        Name = secondItem.Name,
                        Id = secondItem.Id.ToString(),
                    };

                    topC.SubCategory.Add(secondC);
                }
                ShopCategory.Add(topC);
            }
            return Json(ShopCategory, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetShopCateOfficial(long gid)
        {
            var category = new List<CategoryJsonModel>();
            var product = ServiceHelper.Create<IProductService>().GetProduct(gid);
            var productCategory = ServiceHelper.Create<ICategoryService>().GetCategory(product.CategoryId);
            var secondCategory = ServiceHelper.Create<ICategoryService>().GetCategory(productCategory.ParentCategoryId);
            if (secondCategory != null)
            {
                var thridCategories = ServiceHelper.Create<ICategoryService>().GetCategoryByParentId(secondCategory.Id);
                foreach (var thrid in thridCategories)
                {
                    var topC = new CategoryJsonModel()
                    {
                        Name = thrid.Name,
                        Id = thrid.Id.ToString()
                    };
                    category.Add(topC);
                }

            }

            return Json(category, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetBrandOfficial(long gid)
        {
            var brands = new List<CategoryJsonModel>();
            var product = ServiceHelper.Create<IProductService>().GetProduct(gid);
            var productCategory = ServiceHelper.Create<ICategoryService>().GetCategory(product.CategoryId);
            var secondCategory = ServiceHelper.Create<ICategoryService>().GetCategory(productCategory.ParentCategoryId);
            if (secondCategory != null)
            {
                var thridCategories = ServiceHelper.Create<ICategoryService>().GetCategoryByParentId(secondCategory.Id).Select(C => C.Id).ToArray();
                var brandsSecond = ServiceHelper.Create<IBrandService>().GetBrandsByCategoryIds(thridCategories);
                foreach (var brand in brandsSecond)
                {
                    var topC = new CategoryJsonModel()
                    {
                        Name = brand.Name,
                        Id = brand.Id.ToString()
                    };
                    brands.Add(topC);
                }

            }

            return Json(brands, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region  店铺信息
        public JsonResult GetShopInfo(long sid, long productId = 0)
        {
            string cacheKey = CacheKeyCollection.CACHE_SHOPINFO(sid, productId);
            if (Cache.Exists(cacheKey))
                return Cache.Get(cacheKey) as JsonResult;

            string brandLogo = string.Empty;
            long brandId = 0L;
            decimal cashDeposits = 0M;
            if (productId != 0)
            {
                var product = ServiceHelper.Create<IProductService>().GetProduct(productId);
                if (product != null)
                {
                    var brand = ServiceHelper.Create<IBrandService>().GetBrand(product.BrandId);
                    brandLogo = brand == null ? string.Empty : brand.Logo;
                    brandId = brand == null ? brandId : brand.Id;
                }
            }
            var shop = ServiceHelper.Create<IShopService>().GetShop(sid);
            var mark = ShopServiceMark.GetShopComprehensiveMark(sid);
            var cashDepositsInfo = ServiceHelper.Create<ICashDepositsService>().GetCashDepositByShopId(sid);
            if (cashDepositsInfo != null)
                cashDeposits = cashDepositsInfo.CurrentBalance;

            var cashDepositModel = ServiceHelper.Create<ICashDepositsService>().GetCashDepositsObligation(productId);
            var model = new
            {
                CompanyName = shop.CompanyName,
                Id = sid,
                PackMark = mark.PackMark,
                ServiceMark = mark.ServiceMark,
                ComprehensiveMark = mark.ComprehensiveMark,
                Phone = shop.CompanyPhone,
                Name = shop.ShopName,
                Address = ServiceHelper.Create<IRegionService>().GetFullName(shop.CompanyRegionId),
                ProductMark = 3,
                IsSelf = shop.IsSelf,
                BrandLogo = Core.HimallIO.GetImagePath(brandLogo),
                BrandId = brandId,
                CashDeposits = cashDeposits,
                IsSevenDayNoReasonReturn = cashDepositModel.IsSevenDayNoReasonReturn,
                IsCustomerSecurity = cashDepositModel.IsCustomerSecurity,
                TimelyDelivery = cashDepositModel.IsTimelyShip
            };
            JsonResult result = Json(model, true);
            Cache.Insert(cacheKey, result, 600);
            return result;
        }
        #endregion

        #region 热门销售
        [HttpGet]
        public JsonResult GetHotSaleProduct(long sid)
        {
            var HotSaleProducts = new List<HotProductInfo>();
            var sale = ServiceHelper.Create<IProductService>().GetHotSaleProduct(sid, 5);
            if (sale != null)
            {
                foreach (var item in sale.ToArray())
                {
                    HotSaleProducts.Add(new HotProductInfo
                    {
                        ImgPath = Core.HimallIO.GetProductSizeImage(item.RelativePath, 1, (int)ImageSize.Size_220),
                        Name = item.ProductName,
                        Price = item.MinSalePrice,
                        Id = item.Id,
                        SaleCount = (int)item.SaleCounts
                    });
                }
            }
            return Json(HotSaleProducts, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 热门关注
        public JsonResult GetHotConcernedProduct(long sid)
        {
            var HotAttentionProducts = new List<HotProductInfo>();
            var hot = ServiceHelper.Create<IProductService>().GetHotConcernedProduct(sid, 5);
            if (hot != null)
            {
                foreach (var item in hot.ToArray())
                {
                    HotAttentionProducts.Add(new HotProductInfo
                    {
                        ImgPath = Core.HimallIO.GetProductSizeImage(item.RelativePath, 1, (int)ImageSize.Size_220),
                        Name = item.ProductName,
                        Price = item.MinSalePrice,
                        Id = item.Id,
                        SaleCount = (int)item.ConcernedCount
                    });
                }
            }
            return Json(HotAttentionProducts, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 累加浏览次数、 加入历史记录
        [HttpPost]
        public JsonResult LogProduct(long pid)
        {
            if (CurrentUser != null)
            {
                BrowseHistrory.AddBrowsingProduct(pid, CurrentUser.Id);
            }
            else
            {
                BrowseHistrory.AddBrowsingProduct(pid);
            }
            //ServiceHelper.Create<IProductService>().LogProductVisti(pid);
            return Json(null);
        }


        #endregion

        #region 商品属性
        [HttpGet]
        public JsonResult GetProductAttr(long pid)
        {
            List<TypeAttributesModel> ProductAttrs = new List<TypeAttributesModel>();
            var prodAttrs = ServiceHelper.Create<IProductService>().GetProductAttribute(pid).ToList();
            foreach (var attr in prodAttrs)
            {
                if (!ProductAttrs.Any(p => p.AttrId == attr.AttributeId))
                {
                    TypeAttributesModel attrModel = new TypeAttributesModel()
                    {
                        AttrId = attr.AttributeId,
                        AttrValues = new List<TypeAttrValue>(),
                        Name = attr.AttributesInfo.Name
                    };
                    foreach (var attrV in attr.AttributesInfo.AttributeValueInfo.ToList())
                    {
                        if (prodAttrs.Any(p => p.ValueId == attrV.Id))
                        {
                            attrModel.AttrValues.Add(new TypeAttrValue
                            {
                                Id = attrV.Id.ToString(),
                                Name = attrV.Value
                            });
                        }
                    }
                    ProductAttrs.Add(attrModel);
                }
                else
                {
                    var attrTemp = ProductAttrs.FirstOrDefault(p => p.AttrId == attr.AttributeId);

                    if (!attrTemp.AttrValues.Any(p => p.Id == attr.ValueId.ToString()))
                    {
                        var avinfo = attr.AttributesInfo.AttributeValueInfo.FirstOrDefault(a => a.Id == attr.ValueId);
                        if (null != avinfo)
                        {
                            attrTemp.AttrValues.Add(new TypeAttrValue
                            {
                                Id = attr.ValueId.ToString(),
                                Name = avinfo.Value
                            });
                        }
                    }
                }
            }
            return Json(ProductAttrs, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 商品描述
        [HttpGet]
        public JsonResult GetProductDesc(long pid)
        {
            string cacheKey = CacheKeyCollection.CACHE_PRODUCTDESC(pid);
            if (Cache.Exists(cacheKey))
                return Cache.Get(cacheKey) as JsonResult;
            JsonResult result = null;
            var product = ServiceHelper.Create<IProductService>().GetProduct(pid);
            if (product.ProductDescriptionInfo != null)
            {
                var productDescription = product.ProductDescriptionInfo.Description;
                string descriptionPrefix = "", descriptiondSuffix = "";
                if (product.ProductDescriptionInfo.DescriptionPrefixId != 0)
                {
                    var desc = ServiceHelper.Create<IProductDescriptionTemplateService>()
                        .GetTemplate(product.ProductDescriptionInfo.DescriptionPrefixId, product.ShopId);
                    descriptionPrefix = desc == null ? "" : desc.Content;
                }

                if (product.ProductDescriptionInfo.DescriptiondSuffixId != 0)
                {
                    var desc = ServiceHelper.Create<IProductDescriptionTemplateService>()
                        .GetTemplate(product.ProductDescriptionInfo.DescriptiondSuffixId, product.ShopId);
                    descriptiondSuffix = desc == null ? "" : desc.Content;
                }
                result = Json(new
                {
                    ProductDescription = productDescription,
                    DescriptionPrefix = descriptionPrefix,
                    DescriptiondSuffix = descriptiondSuffix
                }, JsonRequestBehavior.AllowGet);

                Cache.Insert(cacheKey, result, 600);
                return result;
            }
            result = Json(new
            {
                ProductDescription = "",
                DescriptionPrefix = "",
                DescriptiondSuffix = ""
            }, JsonRequestBehavior.AllowGet);

            Cache.Insert(cacheKey, result, 600);
            return result;
        }
        #endregion

        #region 获取能否购买信息
        [HttpGet]
        public JsonResult GetEnableBuyInfo(long gid)
        {
            var product = ServiceHelper.Create<IProductService>().GetProduct(gid);
            var hasQuick = CurrentUser == null ? false :
                ServiceHelper.Create<IShippingAddressService>()
                .GetUserShippingAddressByUserId(CurrentUser.Id)
                .Any(s => s.IsQuick);
            return Json(new
            {
                hasQuick = hasQuick ? 1 : 0,
                Logined = (null != CurrentUser) ? 1 : 0,
                hasSKU = product.SKUInfo.Any(s => s.Stock > 0),
                IsOnSale = product.AuditStatus == ProductInfo.ProductAuditStatus.Audited &&
                product.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
            }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region 获取评论数、咨询数
        [HttpGet]
        public JsonResult GetCommentsNumber(long gid)
        {
            var product = ServiceHelper.Create<IProductService>().GetProduct(gid);
            return Json(new
            {
                Comments = product.Himall_ProductComments.Where(a => a.IsHidden.Value == false).Count(),
                Consultations = product.ProductConsultationInfo.Count()
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        [HttpPost]
        public JsonResult AddFavorite(long shopId)
        {
            try
            {
                ServiceHelper.Create<IShopService>().AddFavoriteShop(CurrentUser.Id, shopId);
            }
            catch (HimallException ex)
            {
                return Json(new
                {
                    success = false,
                    msg = ex.Message
                });
            }

            return Json(new
            {
                success = true
            });
        }

        #region 组合购      
        //  [OutputCache(Duration = 60)]
        public ActionResult ProductColloCation(long productId)
        {
            var collocation = ServiceHelper.Create<ICollocationService>().GetCollocationListByProductId(productId);
            List<CollocationProducts> products = null;
            List<ProductCollocationModel> data = new List<ProductCollocationModel>();
            if (collocation != null && collocation.Count > 0)
            {
                int i = 1;
                foreach (var item in collocation)
                {
                    i++;
                    var model = new ProductCollocationModel();
                    products = item.Himall_Collocation.Himall_CollocationPoruducts
                            .Where(a => a.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                            && a.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                            && a.Himall_Products.IsDeleted == false
                            )
                            .Select(a =>
                            new CollocationProducts()
                            {
                                DisplaySequence = a.DisplaySequence.Value,
                                IsMain = a.IsMain,
                                Stock = a.Himall_Products.SKUInfo.Sum(t => t.Stock),
                                MaxCollPrice = a.Himall_CollocationSkus.Max(x => x.Price),
                                MaxSalePrice = a.Himall_CollocationSkus.Max(x => x.SkuPirce).GetValueOrDefault(),
                                MinCollPrice = a.Himall_CollocationSkus.Min(x => x.Price),
                                MinSalePrice = a.Himall_CollocationSkus.Min(x => x.SkuPirce).GetValueOrDefault(),
                                ProductName = a.Himall_Products.ProductName,
                                ProductId = a.ProductId,
                                ColloPid = a.Id,
                                Image = a.Himall_Products.ImagePath
                            }).OrderByDescending(o => o.IsMain).ThenBy(a => a.DisplaySequence).ToList();

                    decimal cheap = 0;
                    if (products != null && products.Count > 1)
                    {
                        cheap = products.Sum(a => a.MaxSalePrice) - products.Sum(a => a.MinCollPrice);
                    }

                    var mainProduct = item.Himall_Collocation.Himall_CollocationPoruducts.FirstOrDefault(p => p.IsMain == true
                            && p.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                            && p.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                            && p.Himall_Products.IsDeleted == false);
                    if (mainProduct != null && products.Count > 1)
                    {
                        model.Id = item.Id;
                        model.ProductId = mainProduct.ProductId;
                        model.ShopId = item.Himall_Collocation.ShopId;
                        model.Products = products;
                        model.Cheap = cheap;
                        if (mainProduct.ProductId != productId)
                        {
                            model.Name = "组合购" + CollocationApplication.GetChineseNumber(i);
                            data.Add(model);
                        }
                        else
                        {
                            model.Name = "组合购" + CollocationApplication.GetChineseNumber(1);
                            i--;
                            //商品为主商品的组合购放第一个
                            data.Insert(0, model);
                        }
                    }
                }
            }
            return PartialView(data);
        }
        #endregion

        /// <summary>
        /// 商品详情
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partnerid">代理用户编号</param>
        /// <returns></returns>
        //#if DEBUG
        //        [ActionPerformance]
        //#endif
        public ActionResult Detail(string id = "", long partnerid = 0)
        {
            var productservice = _iProductService;
            long gid = 0;
            #region 商品Id不合法
            if (long.TryParse(id, out gid))
            {
            }
            if (gid == 0)
            {
                throw new HimallException("很抱歉，您查看的商品不存在，可能被转移。");
            }

            if (!productservice.CheckProductIsExist(gid))
            {
                throw new HimallException("很抱歉，您查看的商品不存在，可能被转移。");
            }

            #endregion         
            var product = productservice.GetProduct(gid);

            if (product == null)
            {
                throw new HimallException("很抱歉，您查看的商品不存在，可能被转移。");
            }

            #region 销售员
            if (partnerid > 0)
            {
                long curuserid = 0;
                if (CurrentUser != null)
                {
                    curuserid = CurrentUser.Id;
                }
                SaveDistributionUserLinkId(partnerid, product.ShopId, curuserid);
            }
            #endregion 

            var iLimitService = ServiceHelper.Create<ILimitTimeBuyService>();

            var ltmbuy = iLimitService.GetLimitTimeMarketItemByProductId(gid);
            if (ltmbuy != null)
            {
                return RedirectToAction("Detail", "LimitTimeBuy", new { id = ltmbuy.Id });
            }

            ProductManagerApplication.GetPCHtml(gid);
            string urlHtml = "/Storage/Products/Statics/" + id + ".html";

            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);
            return File(urlHtml, "text/html");
        }


        /// <summary>
        /// 商品详情(用于生成html页面时调用)
        /// </summary>
        /// <param name="productId"></param>      
        /// <returns></returns>
        public ActionResult Details(long id = 0, long partnerid = 0)
        {
            string price = "";
            var productservice = _iProductService;
            var freightTemplateService = _iFreightTemplateService;
            var typeservice = _iTypeService;

            #region 定义Model和变量

            ProductDetailModelForWeb model = new ProductDetailModelForWeb
            {
                HotAttentionProducts = new List<HotProductInfo>(),
                HotSaleProducts = new List<HotProductInfo>(),
                Product = new Model.ProductInfo(),
                Shop = new ShopInfoModel(),
                ShopCategory = new List<CategoryJsonModel>(),
                Color = new CollectionSKU(),
                Size = new CollectionSKU(),
                Version = new CollectionSKU(),
                BoughtProducts = new List<HotProductInfo>()
            };
            #endregion
            ProductInfo product = null;
            ShopInfoModel shop = new ShopInfoModel();
            #region 商品Id不合法           
            product = productservice.GetProduct(id);

            if (product == null)
            {
                throw new HimallException("很抱歉，您查看的商品不存在，可能被转移。");
            }

            if (product.IsDeleted)
            {
                throw new HimallException("很抱歉，您查看的商品不存在，可能被转移。");
                ////跳转到404页面
                //return RedirectToAction("Error404", "Error", new
                //{
                //    area = "Web"
                //});
            }
            #endregion

            Stopwatch watch = new Stopwatch();

            #region 销售员
            if (partnerid > 0)
            {
                long curuserid = 0;
                if (CurrentUser != null)
                {
                    curuserid = CurrentUser.Id;
                }
                SaveDistributionUserLinkId(partnerid, product.ShopId, curuserid);
            }
            #endregion            

            #region 初始化商品和店铺

            var comment = _iCommentService.GetCommentsByProductId(id);


            #endregion

            #region 店铺信息
            var _shop = ServiceHelper.Create<IShopService>().GetShop(product.ShopId);
            var mark = ShopServiceMark.GetShopComprehensiveMark(_shop.Id);
            model.Shop.Name = _shop.ShopName;
            model.Shop.CompanyName = _shop.CompanyName;
            model.Shop.Id = _shop.Id;
            model.Shop.PackMark = mark.PackMark;
            model.Shop.ServiceMark = mark.ServiceMark;
            model.Shop.ComprehensiveMark = mark.ComprehensiveMark;
            model.Shop.Phone = _shop.CompanyPhone;
            model.Shop.FreeFreight = _shop.FreeFreight;
            //model.Shop.Address = ServiceHelper.Create<IRegionService>().GetRegionFullName(template == null ? 0L : (template.SourceAddress ?? 0L));

            model.Shop.ProductMark = comment.Count() == 0 ? 5 : Convert.ToDecimal(comment.Average(c => c.ReviewMark));

            #endregion

            #region 商品规格

            ProductTypeInfo typeInfo = typeservice.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            model.ColorAlias = colorAlias;
            model.SizeAlias = sizeAlias;
            model.VersionAlias = versionAlias;
            if (product.SKUInfo != null && product.SKUInfo.Count() > 0)
            {
                long colorId = 0, sizeId = 0, versionId = 0;
                foreach (var sku in product.SKUInfo.OrderBy(s => s.AutoId).ToList())
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 0)
                    {
                        if (long.TryParse(specs[1], out colorId))
                        {
                        }
                        if (colorId != 0)
                        {
                            if (sku.Color != null && !model.Color.Any(v => v.Value.Equals(sku.Color)))
                            {
                                var c = product.SKUInfo.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.Stock);
                                model.Color.Add(new ProductSKU
                                {
                                    //Name = "选择颜色",
                                    Name = "选择" + colorAlias,
                                    EnabledClass = c != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Color.Any(c1 => c1.SelectedClass.Equals("selected")) && c != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = colorId,
                                    Value = sku.Color,
                                    Img = sku.ShowPic
                                });
                            }
                        }
                    }
                    if (specs.Count() > 1)
                    {
                        if (long.TryParse(specs[2], out sizeId))
                        {
                        }
                        if (sizeId != 0)
                        {
                            if (sku.Size != null && !model.Size.Any(v => v.Value.Equals(sku.Size)))
                            {
                                var ss = product.SKUInfo.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.Stock);
                                model.Size.Add(new ProductSKU
                                {
                                    //Name = "选择尺码",
                                    Name = "选择" + sizeAlias,
                                    EnabledClass = ss != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Size.Any(s1 => s1.SelectedClass.Equals("selected")) && ss != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = sizeId,
                                    Value = sku.Size

                                });
                            }
                        }
                    }

                    if (specs.Count() > 2)
                    {
                        if (long.TryParse(specs[3], out versionId))
                        {
                        }
                        if (versionId != 0)
                        {
                            if (sku.Version != null && !model.Version.Any(v => v.Value.Equals(sku.Version)))
                            {
                                var v = product.SKUInfo.Where(s => s.Version.Equals(sku.Version)).Sum(s => s.Stock);
                                model.Version.Add(new ProductSKU
                                {
                                    //Name = "选择版本",
                                    Name = "选择" + versionAlias,
                                    EnabledClass = v != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Version.Any(v1 => v1.SelectedClass.Equals("selected")) && v != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = versionId,
                                    Value = sku.Version
                                });
                            }
                        }
                    }

                }
                decimal min = 0, max = 0;
                var skusql = product.SKUInfo.Where(s => s.Stock >= 0);
                if (skusql.Count() > 0)
                {
                    min = skusql.Min(s => s.SalePrice);
                    max = skusql.Max(s => s.SalePrice);
                    if (min == 0 && max == 0)
                    {
                        price = product.MinSalePrice.ToString("f2");
                    }
                }
                else if (max > min)
                {
                    price = string.Format("{0}-{1}", min.ToString("f2"), max.ToString("f2"));
                }
                else
                {
                    price = string.Format("{0}", min.ToString("f2"));
                }

            }
            model.Price = string.IsNullOrWhiteSpace(price) ? product.MinSalePrice.ToString("f2") : price;
            #endregion


            model.Product = product;
            //检查当前产品是否产自官方自营店
            model.IsSellerAdminProdcut = product.Himall_Shops.IsSelf;
            model.CouponCount = ServiceHelper.Create<ICouponService>().GetTopCoupon(product.ShopId).Count();
            model.IsExpiredShop = ServiceHelper.Create<IShopService>().IsExpiredShop(product.ShopId);

            #region 获取店铺的评价统计
            var shopStatisticOrderComments = ServiceHelper.Create<IShopService>().GetShopStatisticOrderComments(product.ShopId);

            var productAndDescription = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescription).FirstOrDefault();
            var sellerServiceAttitude = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitude).FirstOrDefault();
            var sellerDeliverySpeed = shopStatisticOrderComments.Where(c => c.CommentKey ==
                StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeed).FirstOrDefault();

            var productAndDescriptionPeer = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionPeer).FirstOrDefault();
            var sellerServiceAttitudePeer = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudePeer).FirstOrDefault();
            var sellerDeliverySpeedPeer = shopStatisticOrderComments.Where(c => c.CommentKey ==
                StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedPeer).FirstOrDefault();

            var productAndDescriptionMax = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionMax).FirstOrDefault();
            var productAndDescriptionMin = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionMin).FirstOrDefault();

            var sellerServiceAttitudeMax = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudeMax).FirstOrDefault();
            var sellerServiceAttitudeMin = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudeMin).FirstOrDefault();

            var sellerDeliverySpeedMax = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedMax).FirstOrDefault();
            var sellerDeliverySpeedMin = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedMin).FirstOrDefault();

            decimal defaultValue = 5;
            //宝贝与描述
            if (productAndDescription != null && productAndDescriptionPeer != null)
            {
                model.ProductAndDescription = productAndDescription.CommentValue;
                model.ProductAndDescriptionPeer = productAndDescriptionPeer.CommentValue;
                model.ProductAndDescriptionMin = productAndDescriptionMin.CommentValue;
                model.ProductAndDescriptionMax = productAndDescriptionMax.CommentValue;
            }
            else
            {
                model.ProductAndDescription = defaultValue;
                model.ProductAndDescriptionPeer = defaultValue;
                model.ProductAndDescriptionMin = defaultValue;
                model.ProductAndDescriptionMax = defaultValue;
            }
            //卖家服务态度
            if (sellerServiceAttitude != null && sellerServiceAttitudePeer != null)
            {
                model.SellerServiceAttitude = sellerServiceAttitude.CommentValue;
                model.SellerServiceAttitudePeer = sellerServiceAttitudePeer.CommentValue;
                model.SellerServiceAttitudeMax = sellerServiceAttitudeMax.CommentValue;
                model.SellerServiceAttitudeMin = sellerServiceAttitudeMin.CommentValue;
            }
            else
            {
                model.SellerServiceAttitude = defaultValue;
                model.SellerServiceAttitudePeer = defaultValue;
                model.SellerServiceAttitudeMax = defaultValue;
                model.SellerServiceAttitudeMin = defaultValue;
            }
            //卖家发货速度
            if (sellerDeliverySpeedPeer != null && sellerDeliverySpeed != null)
            {
                model.SellerDeliverySpeed = sellerDeliverySpeed.CommentValue;
                model.SellerDeliverySpeedPeer = sellerDeliverySpeedPeer.CommentValue;
                model.SellerDeliverySpeedMax = sellerDeliverySpeedMax != null ? sellerDeliverySpeedMax.CommentValue : 0;
                model.sellerDeliverySpeedMin = sellerDeliverySpeedMin != null ? sellerDeliverySpeedMin.CommentValue : 0;
            }
            else
            {
                model.SellerDeliverySpeed = defaultValue;
                model.SellerDeliverySpeedPeer = defaultValue;
                model.SellerDeliverySpeedMax = defaultValue;
                model.sellerDeliverySpeedMin = defaultValue;
            }
            #endregion

            #region 买过该商品的还买了
            if (CurrentUser != null)
            {
                var queryModel = new OrderQuery()
                {
                    UserName = CurrentUser.UserName,
                    UserId = CurrentUser.Id,
                    PageSize = 20,
                    PageNo = 1
                };
                var orders = ServiceHelper.Create<IOrderService>().GetOrders<OrderInfo>(queryModel).Models.OrderByDescending(c => c.OrderDate).Select(c => c.Id).ToList();
                var orderItems = ServiceHelper.Create<IOrderService>().GetOrderItemsByOrderId(orders).OrderByDescending(c => c.OrderInfo.OrderDate);
                foreach (var orderItem in orderItems)
                {
                    if (model.BoughtProducts.Where(c => c.Id == orderItem.ProductId).Count() > 0)
                        continue;
                    //TODO:[lly]买过该商品的还买了 过滤已删除的商品
                    /* zjt  
					 * productservice 命名应为productService
					 */

                    var productInfo = productservice.GetProduct(orderItem.ProductId);
                    if (productInfo != null)
                    {
                        if (productInfo.IsDeleted)
                        {
                            continue;
                        }
                    }
                    model.BoughtProducts.Add(new HotProductInfo
                    {
                        Id = orderItem.ProductId,
                        Name = orderItem.ProductName,
                        Price = orderItem.SalePrice,
                        ImgPath = orderItem.ThumbnailsUrl
                    });
                    if (model.BoughtProducts.Select(c => c.Id).Distinct().Count() == 20)
                    {
                        break;
                    }
                }
                if (model.BoughtProducts.Count < 5)
                {
                    model.BoughtProducts.Clear();
                }

            }
            #endregion 买过该商品的还买了


            var brandModel = ServiceHelper.Create<IBrandService>().GetBrand(model.Product.BrandId);
            model.Product.BrandName = brandModel == null ? "" : brandModel.Name;

            var map = Core.Helper.QRCodeHelper.Create("http://" + HttpContext.Request.Url.Authority + "/m-wap/product/detail/" + product.Id);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            //  将图片内存流转成base64,图片以DataURI形式显示  
            string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
            ms.Dispose();
            //  显示  
            model.Code = strUrl;
            ViewBag.IsEnableCashOnDelivery = ServiceHelper.Create<IPaymentConfigService>().IsEnable() && model.Shop.Id == 1;
            model.CashDepositsObligation = ServiceHelper.Create<ICashDepositsService>().GetCashDepositsObligation(product.Id);

            //补充当前店铺红包功能
            ViewBag.isShopPage = true;
            ViewBag.CurShopId = product.ShopId;
            TempData["isShopPage"] = true;
            TempData["CurShopId"] = product.ShopId;
            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);
            return View(model);
        }

        public JsonResult GetProductDetails(long id = 0, long shopId = 0)
        {
            var productservice = _iProductService;
            var typeservice = _iTypeService;
            var productId = id;
            if (productId == 0)
                return Json(new { data = false });
            var productInfo = productservice.GetNeedRefreshProductInfo(productId);
            ProductDetailModelForWeb model = new ProductDetailModelForWeb
            {
                Color = new CollectionSKU(),
                Size = new CollectionSKU(),
                Version = new CollectionSKU()
            };
            ProductTypeInfo typeInfo = typeservice.GetType(productInfo.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            var skus = productservice.GetSkuList(productId);
            #region 商品规格
            var price = string.Empty;
            if (skus != null && skus.Count() > 0)
            {
                long colorId = 0, sizeId = 0, versionId = 0;
                foreach (var sku in skus.OrderBy(s => s.AutoId).ToList())
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 0)
                    {
                        if (long.TryParse(specs[1], out colorId))
                        {
                        }
                        if (colorId != 0)
                        {
                            if (!model.Color.Any(v => v.Value.Equals(sku.Color)))
                            {
                                var c = skus.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.Stock);
                                model.Color.Add(new ProductSKU
                                {
                                    //Name = "选择颜色",
                                    Name = "选择" + colorAlias,
                                    EnabledClass = c != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Color.Any(c1 => c1.SelectedClass.Equals("selected")) && c != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = colorId,
                                    Value = sku.Color,
                                    Img = Himall.Core.HimallIO.GetImagePath(sku.ShowPic)
                                });
                            }
                        }
                    }
                    if (specs.Count() > 1)
                    {
                        if (long.TryParse(specs[2], out sizeId))
                        {
                        }
                        if (sizeId != 0)
                        {
                            if (!model.Size.Any(v => v.Value.Equals(sku.Size)))
                            {
                                var ss = skus.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.Stock);
                                model.Size.Add(new ProductSKU
                                {
                                    //Name = "选择尺码",
                                    Name = "选择" + sizeAlias,
                                    EnabledClass = ss != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Size.Any(s1 => s1.SelectedClass.Equals("selected")) && ss != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = sizeId,
                                    Value = sku.Size
                                });
                            }
                        }
                    }

                    if (specs.Count() > 2)
                    {
                        if (long.TryParse(specs[3], out versionId))
                        {
                        }
                        if (versionId != 0)
                        {
                            if (!model.Version.Any(v => v.Value.Equals(sku.Version)))
                            {
                                var v = skus.Where(s => s.Version.Equals(sku.Version)).Sum(s => s.Stock);
                                model.Version.Add(new ProductSKU
                                {
                                    //Name = "选择版本",
                                    Name = "选择" + versionAlias,
                                    EnabledClass = v != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Version.Any(v1 => v1.SelectedClass.Equals("selected")) && v != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = versionId,
                                    Value = sku.Version
                                });
                            }
                        }
                    }

                }
                decimal min = 0, max = 0;
                var skusql = skus.Where(s => s.Stock >= 0);
                if (skusql.Count() > 0)
                {
                    min = skusql.Min(s => s.SalePrice);
                    max = skusql.Max(s => s.SalePrice);
                    if (min == 0 && max == 0)
                    {
                        price = productInfo.MinSalePrice.ToString("f2");
                    }
                }
                else if (max > min)
                {
                    price = string.Format("{0}-{1}", min.ToString("f2"), max.ToString("f2"));
                }
                else
                {
                    price = string.Format("{0}", min.ToString("f2"));
                }
            }
            decimal discount = 1M;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            var shopInfo = ShopApplication.GetShop(productInfo.ShopId);
            if (shopInfo.IsSelf)
            {
                model.Price = string.IsNullOrWhiteSpace(price) ? (productInfo.MinSalePrice * discount).ToString("f2") : (Convert.ToDecimal(price) * discount).ToString("f2");
            }
            else
            {
                model.Price = string.IsNullOrWhiteSpace(price) ? productInfo.MinSalePrice.ToString("f2") : price;
            }
            #endregion
            skus = null;

            //#region 物流信息
            //var template = ServiceHelper.Create<IFreightTemplateService>().GetFreightTemplate(productInfo.FreightTemplateId);
            //decimal freight = 0;
            //int cityId = 0;
            //string shippingAddress = "";
            //string shippingValue = string.Empty;
            //string freightStr = string.Empty;
            //string productAddress = string.Empty;
            //if (template != null)
            //{
            //    if (template.SourceAddress.HasValue)
            //    {
            //        var fullName = RegionApplication.GetFullName(template.SourceAddress.Value);
            //        if (fullName != null)
            //        {
            //            var ass = fullName.Split(' ');
            //            if (ass.Length >= 2)
            //            {
            //                productAddress = ass[0] + " " + ass[1];
            //            }
            //            else
            //            {
            //                productAddress = ass[0];
            //            }
            //        }
            //    }
            //    ShippingAddressInfo defaultAddress = null;
            //    if (CurrentUser != null)
            //    {
            //        defaultAddress = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(CurrentUser.Id);
            //    }
            //    int regionId = 0;
            //    if (defaultAddress != null)
            //    {
            //        regionId = defaultAddress.RegionId;
            //    }
            //    else
            //    {
            //        string curip = Himall.Core.Helper.WebHelper.GetIP();
            //        regionId = (int)RegionApplication.GetRegionByIPInTaobao(curip);
            //    }
            //    if (regionId > 0)
            //    {
            //        shippingValue = RegionApplication.GetRegionPath(regionId);

            //        string addrs = RegionApplication.GetFullName(regionId);

            //        shippingAddress = addrs;
            //        if (string.IsNullOrEmpty(shippingAddress))
            //        {
            //            shippingAddress = "请选择";
            //        }
            //        cityId = regionId;// ServiceHelper.Create<IRegionService>.Create.GetCityId(defaultAddress.RegionIdPath);
            //    }
            //}
            //if (template.IsFree == FreightTemplateType.Free)
            //{
            //    freightStr = "卖家承担运费";
            //}
            //else
            //{
            //    if (CurrentUser != null)
            //    {
            //        if (cityId > 0)
            //        {
            //            List<long> productIds = new List<long>();
            //            List<int> counts = new List<int>();
            //            productIds.Add(id);
            //            counts.Add(1);
            //            freight = ServiceHelper.Create<IProductService>().GetFreight(productIds, counts, cityId);
            //            freightStr = "运费 ￥" + freight;
            //        }
            //    }
            //}

            //if (cityId > 0)
            //{
            //    var shopBranchQuery = new ShopBranchQuery();
            //    shopBranchQuery.ShopId = shopId;
            //    shopBranchQuery.Status = ShopBranchStatus.Normal;
            //    shopBranchQuery.ProductIds = new[] { productId };
            //    var cityRegion = RegionApplication.GetRegion(cityId, CommonModel.Region.RegionLevel.City);
            //    shopBranchQuery.AddressPath = cityRegion.GetIdPath();

            //    model.CanSelfTake = ShopBranchApplication.Exists(shopBranchQuery);
            //}

            //model.ProductAddress = productAddress;
            //model.ShippingAddress = shippingAddress;
            //model.ShippingValue = shippingValue;
            //model.Freight = freightStr;
            //#endregion

            //#region 限时购预热
            //var iLimitService = ServiceHelper.Create<ILimitTimeBuyService>();
            //bool isPreheat = false;
            //model.FlashSale = iLimitService.IsFlashSaleDoesNotStarted(id);
            //model.FlashSaleConfig = iLimitService.GetConfig();
            //var strFlashSaleTime = string.Empty;
            //if (model.FlashSale != null)
            //{
            //    TimeSpan flashSaleTime = DateTime.Parse(model.FlashSale.BeginDate) - DateTime.Now;  //开始时间还剩多久
            //    TimeSpan preheatTime = new TimeSpan(model.FlashSaleConfig.Preheat, 0, 0);  //预热时间是多久
            //    if (preheatTime >= flashSaleTime)  //预热大于开始
            //    {
            //        isPreheat = true;
            //        strFlashSaleTime = Math.Floor(flashSaleTime.TotalHours) + "时" + flashSaleTime.Minutes + "分";
            //    }
            //}

            //#endregion

            //#region  商品评分等级
            //var productMark = _iCommentService.GetProductMark(id);
            //#endregion

            return Json(new
            {
                price = model.Price,
                saleCounts = productInfo.SaleCounts,
                measureUnit = productInfo.MeasureUnit,
                skuColors = model.Color,
                skuSizes = model.Size,
                skuVersions = model.Version,
                FreightTemplateId = productInfo.FreightTemplateId,
                colorAlias = colorAlias,
                sizeAlias = sizeAlias,
                versionAlias = versionAlias
                //productMark = productMark,
                //isPreheat = isPreheat,
                //flashSaleId = model.FlashSale == null ? 0 : model.FlashSale.Id,
                //flashSaleTime =strFlashSaleTime,
                //isNormalPurchase = model.FlashSaleConfig.IsNormalPurchase,
                //shippingValue = model.ShippingValue,
                //shippingAddress = model.ShippingAddress,
                //freight = model.Freight
            });
        }
        public JsonResult GetProductShipAndLimit(long id = 0, long shopId = 0, long templateId = 0)
        {
            var productId = id;
            if (productId == 0)
                return Json(new { data = false });
            ProductDetailModelForWeb model = new ProductDetailModelForWeb
            {
                Color = new CollectionSKU(),
                Size = new CollectionSKU(),
                Version = new CollectionSKU()
            };
            #region 物流信息
            var template = ServiceHelper.Create<IFreightTemplateService>().GetFreightTemplate(templateId);
            decimal freight = 0;
            int cityId = 0;
            string shippingAddress = "";
            string shippingValue = string.Empty;
            string freightStr = string.Empty;
            string productAddress = string.Empty;
            if (template != null)
            {
                if (template.SourceAddress.HasValue)
                {
                    var fullName = RegionApplication.GetFullName(template.SourceAddress.Value);
                    if (fullName != null)
                    {
                        var ass = fullName.Split(' ');
                        if (ass.Length >= 2)
                        {
                            productAddress = ass[0] + " " + ass[1];
                        }
                        else
                        {
                            productAddress = ass[0];
                        }
                    }
                }
                ShippingAddressInfo defaultAddress = null;
                if (CurrentUser != null)
                {
                    defaultAddress = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(CurrentUser.Id);
                }
                int regionId = 0;
                if (defaultAddress != null)
                {
                    regionId = defaultAddress.RegionId;
                }
                else
                {
                    string curip = Himall.Core.Helper.WebHelper.GetIP();
                    regionId = (int)RegionApplication.GetRegionByIPInTaobao(curip);
                }
                if (regionId > 0)
                {
                    shippingValue = RegionApplication.GetRegionPath(regionId);

                    string addrs = RegionApplication.GetFullName(regionId);

                    shippingAddress = addrs;
                    if (string.IsNullOrEmpty(shippingAddress))
                    {
                        shippingAddress = "请选择";
                    }
                    cityId = regionId;// ServiceHelper.Create<IRegionService>.Create.GetCityId(defaultAddress.RegionIdPath);
                }
            }
            if (template.IsFree == FreightTemplateType.Free)
            {
                freightStr = "卖家承担运费";
            }
            else
            {
                if (CurrentUser != null)
                {
                    if (cityId > 0)
                    {
                        List<long> productIds = new List<long>();
                        List<int> counts = new List<int>();
                        productIds.Add(id);
                        counts.Add(1);
                        freight = ServiceHelper.Create<IProductService>().GetFreight(productIds, counts, cityId);
                        freightStr = "运费 ￥" + freight;
                    }
                }
            }

            if (cityId > 0)
            {
                var shopBranchQuery = new ShopBranchQuery();
                shopBranchQuery.ShopId = shopId;
                shopBranchQuery.Status = ShopBranchStatus.Normal;
                shopBranchQuery.ProductIds = new[] { productId };
                var cityRegion = RegionApplication.GetRegion(cityId, CommonModel.Region.RegionLevel.City);
                shopBranchQuery.AddressPath = cityRegion.GetIdPath();

                model.CanSelfTake = ShopBranchApplication.Exists(shopBranchQuery);
            }

            model.ProductAddress = productAddress;
            model.ShippingAddress = shippingAddress;
            model.ShippingValue = shippingValue;
            model.Freight = freightStr;
            #endregion

            #region 限时购预热
            var iLimitService = ServiceHelper.Create<ILimitTimeBuyService>();
            bool isPreheat = false;
            model.FlashSale = iLimitService.IsFlashSaleDoesNotStarted(id);
            model.FlashSaleConfig = iLimitService.GetConfig();
            var strFlashSaleTime = string.Empty;
            if (model.FlashSale != null)
            {
                TimeSpan flashSaleTime = DateTime.Parse(model.FlashSale.BeginDate) - DateTime.Now;  //开始时间还剩多久
                TimeSpan preheatTime = new TimeSpan(model.FlashSaleConfig.Preheat, 0, 0);  //预热时间是多久
                if (preheatTime >= flashSaleTime)  //预热大于开始
                {
                    isPreheat = true;
                    strFlashSaleTime = Math.Floor(flashSaleTime.TotalHours) + "时" + flashSaleTime.Minutes + "分";
                }
            }

            #endregion

            #region  商品评分等级
            var productMark = _iCommentService.GetProductMark(id);
            #endregion

            return Json(new
            {
                productMark = productMark,
                isPreheat = isPreheat,
                flashSaleId = model.FlashSale == null ? 0 : model.FlashSale.Id,
                flashSaleTime = strFlashSaleTime,
                isNormalPurchase = model.FlashSaleConfig.IsNormalPurchase,
                shippingValue = model.ShippingValue,
                shippingAddress = model.ShippingAddress,
                freight = model.Freight
            });
        }
        /// <summary>
        /// 获取登录用户的信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetLoginUserInfo()
        {
            #region 登录信息
            var isLogin = false;
            List<ProductInfo> Concern = new List<ProductInfo>();
            List<ProductInfo> BrowsingProducts = new List<ProductInfo>();
            //var integral = 0;
            if (CurrentUser != null)
            {
                isLogin = true;
                //integral = ServiceHelper.Create<IMemberIntegralService>().GetMemberIntegral(CurrentUser.Id).AvailableIntegrals;

                //关注商品
                var favorites = isLogin ? _iProductService.GetUserAllConcern(CurrentUser.Id) : new List<FavoriteInfo>();
                favorites = favorites.Take(10).ToList();
                foreach (var item in favorites)
                {
                    var p = new ProductInfo();
                    p.Id = item.ProductId;
                    p.ProductName = item.ProductInfo.ProductName;
                    p.ImagePath = Himall.Core.HimallIO.GetProductSizeImage(item.ProductInfo.RelativePath, 1, (int)ImageSize.Size_50);
                    Concern.Add(p);
                }

                //浏览的商品
                var viewHistoryModel = isLogin ? BrowseHistrory.GetBrowsingProducts(10, CurrentUser == null ? 0 : CurrentUser.Id) : new List<ProductBrowsedHistoryModel>();

                foreach (var item in viewHistoryModel)
                {
                    var p = new ProductInfo();
                    p.Id = item.ProductId;
                    p.ProductName = item.ProductName;
                    p.ImagePath = Himall.Core.HimallIO.GetProductSizeImage(item.ImagePath, 1, 50);
                    BrowsingProducts.Add(p);
                }
            }
            var userName = CurrentUser == null ? "" : (string.IsNullOrEmpty(CurrentUser.Nick) ? CurrentUser.UserName : CurrentUser.Nick);
            // return Json(new { isLogin = CurrentUser != null, currentUserName = userName, concern = Concern, browsingProducts = BrowsingProducts, integral = integral });
            return Json(new { isLogin = CurrentUser != null, currentUserName = userName, concern = Concern, browsingProducts = BrowsingProducts });
            #endregion
        }

        public ActionResult UserCoupons()
        {
            var model = new ProductPartialHeaderModel();
            var isLogin = CurrentUser != null;
            List<IBaseCoupon> baseCoupons = new List<IBaseCoupon>();
            var _iCouponService = ServiceHelper.Create<ICouponService>();
            //优惠卷
            var coupons = isLogin ? _iCouponService.GetAllUserCoupon(CurrentUser.Id).ToList() : new List<UserCouponInfo>();
            coupons = coupons == null ? new List<UserCouponInfo>() : coupons;
            baseCoupons.AddRange(coupons);
            //红包
            var shopBonus = isLogin ? ServiceHelper.Create<IShopBonusService>().GetCanUseDetailByUserId(CurrentUser.Id) : new List<ShopBonusReceiveInfo>();
            shopBonus = shopBonus == null ? new List<ShopBonusReceiveInfo>() : shopBonus;
            baseCoupons.AddRange(shopBonus);
            model.BaseCoupon = baseCoupons;
            //用户积分
            model.MemberIntegral = isLogin ? ServiceHelper.Create<IMemberIntegralService>().GetMemberIntegral(CurrentUser.Id).AvailableIntegrals : 0;
            return PartialView(model);
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult CalceFreight(int cityId, long pId, int count)
        {
            List<long> productIds = new List<long>();
            List<int> counts = new List<int>();
            productIds.Add(pId);
            counts.Add(count);
            decimal freight = ServiceHelper.Create<IProductService>().GetFreight(productIds, counts, cityId);
            string freightStr = "运费 ￥" + freight;
            return Json(new Result() { success = true, msg = freightStr });
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult IsCashOnDelivery(long countyId)
        {
            bool result = PaymentConfigApplication.IsCashOnDelivery(countyId);
            if (result)
            {
                return Json(new Result() { success = result, msg = "支持货到付款" });
            }
            return Json(new Result() { success = result, msg = "" });

        }

        public JsonResult GetPrice(string skuId)
        {
            var price = ServiceHelper.Create<IProductService>().GetSku(skuId).SalePrice.ToString("f2");
            return Json(new
            {
                price = price
            }, JsonRequestBehavior.AllowGet);
        }


        [UnAuthorize]
        [HttpPost]
        public ActionResult GetProductActives(long shopId, long productId)
        {
            ProductActives actives = new ProductActives();
            var freeFreight = ServiceHelper.Create<IShopService>().GetShopFreeFreight(shopId);
            actives.freeFreight = freeFreight;
            var bonus = ServiceHelper.Create<IShopBonusService>().GetByShopId(shopId);
            if (bonus != null)
            {
                ProductBonusLableModel model = new ProductBonusLableModel();
                model.Count = bonus.Count;
                model.GrantPrice = bonus.GrantPrice;
                model.RandomAmountStart = bonus.RandomAmountStart;
                model.RandomAmountEnd = bonus.RandomAmountEnd;
                actives.ProductBonus = model;
            }
            var fullDiscount = FullDiscountApplication.GetOngoingActiveByProductId(productId, shopId);
            if (fullDiscount != null)
            {
                actives.FullDiscount = fullDiscount;
            }
            return Json(actives);
        }



        [HttpGet]
        public JsonResult GetStock(string skuId)
        {
            var stock = ServiceHelper.Create<IProductService>().GetSku(skuId).Stock;
            var product = ServiceHelper.Create<IProductService>().GetSku(skuId).ProductInfo;
            var status = 0;
            if (product.AuditStatus == ProductInfo.ProductAuditStatus.Audited && product.SaleStatus == ProductInfo.ProductSaleStatus.OnSale)
            {
                status = 1;
            }
            return Json(new
            {
                Stock = stock,
                Status = status
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSKUInfo(long pId, long colloPid = 0)
        {
            var product = ServiceHelper.Create<IProductService>().GetProduct(pId);
            List<Himall.Model.CollocationSkuInfo> collProduct = null;
            if (colloPid != 0)
            {
                collProduct = ServiceHelper.Create<ICollocationService>().GetProductColloSKU(pId, colloPid);
            }

            decimal discount = 1M;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            var shopInfo = ShopApplication.GetShop(product.ShopId);

            var skuArray = new List<ProductSKUModel>();
            foreach (var sku in product.SKUInfo.Where(s => s.Stock > 0))
            {

                decimal price = 1M;
                if (shopInfo.IsSelf)
                {
                    price = sku.SalePrice * discount;
                }
                else
                {
                    price = sku.SalePrice;
                }
                if (collProduct != null && collProduct.Count > 0)
                {
                    var collsku = collProduct.FirstOrDefault(a => a.SkuID == sku.Id);
                    if (collsku != null)
                        price = collsku.Price;
                }
                skuArray.Add(new ProductSKUModel
                {
                    Price = price,
                    SkuId = sku.Id,
                    Stock = (int)sku.Stock
                });
            }
            //foreach (var item in skuArray)
            //{
            //    var str = item.SKUId.Split('_');
            //    item.SKUId = string.Format("{0};{1};{2}", str[1], str[2], str[3]);
            //}
            return Json(new
            {
                skuArray = skuArray
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddFavoriteProduct(long pid)
        {
            int status = 0;
            ServiceHelper.Create<IProductService>().AddFavorite(pid, CurrentUser.Id, out status);
            if (status == 1)
            {
                return Json(new
                {
                    successful = true,
                    favorited = true,
                    mess = "您已经关注过该商品了."
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new
                {
                    successful = true,
                    favorited = false,
                    mess = "成功关注该商品."
                }, JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult CustmerServices(long shopId)
        {
            //排除掉移动端的客服
            var model = CustomerServiceApplication.GetPreSaleByShopId(shopId).Where(c => c.TerminalType == CustomerServiceInfo.ServiceTerminalType.PC).ToList();
            return View(model);
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult List(long? categoryId, string brandName, string productCode, int? auditStatus, string ids, int page, int rows, string keyWords, string shopName, int? saleStatus)
        {
            var queryModel = new ProductQuery()
            {
                PageSize = rows,
                PageNo = page,
                BrandNameKeyword = brandName,
                KeyWords = keyWords,
                CategoryId = categoryId,
                Ids = string.IsNullOrWhiteSpace(ids) ? null : ids.Split(',').Select(item => long.Parse(item)),
                ShopName = shopName,
                ProductCode = productCode
            };
            if (auditStatus.HasValue)
            {
                queryModel.AuditStatus = new Model.ProductInfo.ProductAuditStatus[] { (Model.ProductInfo.ProductAuditStatus)auditStatus };
                if (auditStatus == (int)Model.ProductInfo.ProductAuditStatus.WaitForAuditing)
                    queryModel.SaleStatus = Model.ProductInfo.ProductSaleStatus.OnSale;
            }
            if (saleStatus.HasValue)
                queryModel.SaleStatus = (Himall.Model.ProductInfo.ProductSaleStatus)saleStatus;

            var products = ProductManagerApplication.GetProducts(queryModel);
            var productDescriptions = ProductManagerApplication.GetProductDescription(products.Models.Select(p => p.Id).ToArray());
            var brands = BrandApplication.GetBrandsByIds(products.Models.Select(p => p.BrandId));
            var categories = CategoryApplication.GetCategories();
            var shops = ShopApplication.GetShops(products.Models.Select(p => p.ShopId));

            var dataGrid = new DataGridModel<Himall.Web.Areas.Admin.Models.Product.ProductModel>();
            dataGrid.rows = products.Models.Select(item => new Himall.Web.Areas.Admin.Models.Product.ProductModel()
            {
                name = item.ProductName,
                brandName = item.BrandId == 0 ? "" : brands.Any(b => b.Id == item.BrandId) ? brands.FirstOrDefault(b => b.Id == item.BrandId).Name : "",
                categoryName = categories.Any(c => c.Id == item.CategoryId) ? categories.FirstOrDefault(c => c.Id == item.CategoryId).Name : "",
                id = item.Id,
                imgUrl = item.GetImage(Himall.CommonModel.ImageSize.Size_50),
                price = item.MinSalePrice,
                state = item.AuditStatus == ProductInfo.ProductAuditStatus.WaitForAuditing ? (item.SaleStatus == ProductInfo.ProductSaleStatus.OnSale ? ProductInfo.ProductAuditStatus.WaitForAuditing.ToDescription() : ProductInfo.ProductSaleStatus.InStock.ToDescription()) : item.AuditStatus.ToDescription(),
                auditStatus = (int)item.AuditStatus,
                url = "",
                auditReason = productDescriptions.Any(pd => pd.ProductId == item.Id) ? productDescriptions.FirstOrDefault(pd => pd.ProductId == item.Id).AuditReason : "",
                shopName = shops.Any(s => s.Id == item.ShopId) ? shops.FirstOrDefault(s => s.Id == item.ShopId).ShopName : "",
                saleStatus = (int)item.SaleStatus,
                productCode = item.ProductCode
            });
            dataGrid.total = products.Total;

            return Json(dataGrid);
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult Browse(long? shopId, long? categoryId, int? auditStatus, string ids, int page, int rows, string keyWords, int? saleStatus, bool? isShopCategory, bool isLimitTimeBuy = false)
        {
            var queryModel = new ProductQuery()
            {
                PageSize = rows,
                PageNo = page,
                KeyWords = keyWords,
                CategoryId = categoryId,
                Ids = string.IsNullOrWhiteSpace(ids) ? null : ids.Split(',').Select(item => long.Parse(item)),
                ShopId = shopId,
                IsLimitTimeBuy = isLimitTimeBuy
            };
            if (auditStatus.HasValue)
                queryModel.AuditStatus = new Model.ProductInfo.ProductAuditStatus[] { (Model.ProductInfo.ProductAuditStatus)auditStatus };

            if (saleStatus.HasValue)
                queryModel.SaleStatus = (Himall.Model.ProductInfo.ProductSaleStatus)saleStatus;


            ObsoletePageModel<Himall.Model.ProductInfo> productEntities = ServiceHelper.Create<IProductService>().GetProducts(queryModel);
            ICategoryService productCategoryService = ServiceHelper.Create<ICategoryService>();
            IBrandService brandService = ServiceHelper.Create<IBrandService>();
            var products = productEntities.Models.ToArray().Select(item => new
            {
                name = item.ProductName,
                brandName = item.BrandId == 0 ? "" : brandService.GetBrand(item.BrandId).Name,
                categoryName = productCategoryService.GetCategory(item.CategoryId).Name,
                id = item.Id,
                imgUrl = item.GetImage(ImageSize.Size_50),
                price = item.MinSalePrice
            });

            var dataGrid = new
            {
                rows = products,
                total = productEntities.Total
            };
            return Json(dataGrid);
        }

        public ActionResult GetCollocationProducts(string productIds, string colloPids)
        {
            var ids = productIds.Split(',').Select(a => long.Parse(a)).ToArray();
            var cids = colloPids.Split(',').Select(a => long.Parse(a)).ToArray();
            var dic = new Dictionary<long, long>();
            for (var i = 0; i < ids.Length; i++)
            {
                dic.Add(ids[i], cids[i]);
            }
            var product = ServiceHelper.Create<IProductService>().GetProductByIds(ids).Where(a => a.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && a.AuditStatus == ProductInfo.ProductAuditStatus.Audited).ToList();


            var products = ids.Select(a => product.Where(t => t.Id == a).FirstOrDefault());


            List<CollocationSkusModel> model = new List<CollocationSkusModel>();
            var index = 0;
            foreach (var p in products)
            {
                model.Add(GetCollocationSku(p, dic.Where(a => a.Key == p.Id).FirstOrDefault().Value));
            }


            return View(model);
        }

        private CollocationSkusModel GetCollocationSku(ProductInfo product, long colloId)
        {
            var typeservice = _iTypeService;
            ProductTypeInfo typeInfo = typeservice.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            CollocationSkusModel model = new CollocationSkusModel();
            model.ProductName = product.ProductName;
            model.ColloProductId = colloId;
            model.ImagePath = product.ImagePath;
            model.ProductId = product.Id;
            model.MeasureUnit = product.MeasureUnit;
            model.Stock = product.SKUInfo.Sum(a => a.Stock);
            model.ColorAlias = colorAlias;
            model.SizeAlias = sizeAlias;
            model.VersionAlias = versionAlias;
            var coll = product.Himall_CollocationSkus.Where(a => a.ColloProductId == colloId);
            if (coll != null)
                model.MinPrice = coll.Min(a => a.Price);
            else
                model.MinPrice = product.SKUInfo.Min(a => a.SalePrice);

            if (product.SKUInfo != null && product.SKUInfo.Count() > 0)
            {
                long colorId = 0, sizeId = 0, versionId = 0;
                foreach (var sku in product.SKUInfo.OrderBy(s => s.AutoId).ToList())
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 0)
                    {
                        if (long.TryParse(specs[1], out colorId))
                        {
                        }
                        if (colorId != 0)
                        {
                            if (!model.Color.Any(v => v.Value.Equals(sku.Color)))
                            {
                                var c = product.SKUInfo.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.Stock);
                                model.Color.Add(new ProductSKU
                                {
                                    //Name = "选择颜色",
                                    Name = "选择" + colorAlias,
                                    EnabledClass = c != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Color.Any(c1 => c1.SelectedClass.Equals("selected")) && c != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = colorId,
                                    Value = sku.Color,
                                    Img = sku.ShowPic
                                });
                            }
                        }
                    }
                    if (specs.Count() > 1)
                    {
                        if (long.TryParse(specs[2], out sizeId))
                        {
                        }
                        if (sizeId != 0)
                        {
                            if (!model.Size.Any(v => v.Value.Equals(sku.Size)))
                            {
                                var ss = product.SKUInfo.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.Stock);
                                model.Size.Add(new ProductSKU
                                {
                                    //Name = "选择尺码",
                                    Name = "选择" + sizeAlias,
                                    EnabledClass = ss != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Size.Any(s1 => s1.SelectedClass.Equals("selected")) && ss != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = sizeId,
                                    Value = sku.Size

                                });
                            }
                        }
                    }

                    if (specs.Count() > 2)
                    {
                        if (long.TryParse(specs[3], out versionId))
                        {
                        }
                        if (versionId != 0)
                        {
                            if (!model.Version.Any(v => v.Value.Equals(sku.Version)))
                            {
                                var v = product.SKUInfo.Where(s => s.Version.Equals(sku.Version)).Sum(s => s.Stock);
                                model.Version.Add(new ProductSKU
                                {
                                    //Name = "选择版本",
                                    Name = "选择" + versionAlias,
                                    EnabledClass = v != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Version.Any(v1 => v1.SelectedClass.Equals("selected")) && v != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = versionId,
                                    Value = sku.Version
                                });
                            }
                        }
                    }

                }
            }
            return model;
        }

        public JsonResult GetHotProduct(long productId, int categoryId)
        {
            var relationProduct = ProductManagerApplication.GetRelationProductByProductId(productId);
            List<DTO.Product.Product> products;
            if (relationProduct == null || ProductManagerApplication.GetProductsByIds(relationProduct.RelationProductIds).Count == 0)
                products = ProductManagerApplication.GetHotSaleProductByCategoryId(categoryId, 10);
            else
                products = ProductManagerApplication.GetProductsByIds(relationProduct.RelationProductIds);

            foreach (var item in products)
            {
                item.ImagePath = item.GetImage(ImageSize.Size_220);
            }

            return Json(products, true);
        }

        public JsonResult CanBuy(long productId, int count)
        {
            if (CurrentUser == null)
                return Json(new { Result = true }, true);

            int reason;
            var msg = new Dictionary<int, string>() { { 0, "" }, { 1, "商品已下架" }, { 2, "商品已删除" }, { 3, "超出商品最大限购数" }, { 9, "商品无货" } };
            var result = ProductManagerApplication.CanBuy(CurrentUser.Id, productId, count, out reason);

            return Json(new
            {
                Result = result,
                Message = msg[reason]
            }, true);
        }
    }
}