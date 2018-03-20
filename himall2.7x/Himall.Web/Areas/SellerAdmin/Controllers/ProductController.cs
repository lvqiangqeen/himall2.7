using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Mvc;
using Himall.Application;
using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.App_Code;
using Himall.Web.App_Code.Common;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;
using Himall.Web.Models;
using Himall.CommonModel;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class ProductController : BaseSellerController
    {
        #region 字段
        private IShopCategoryService _iShopCategoryService;
        private IProductService _iProductService;
        private ITypeService _iTypeService;
        private ICategoryService _iCategoryService;
        private IShopService _iShopService;
        private ISearchProductService _iSearchProductService;
        private long shopId = 2;
        #endregion

        #region 构造函数
        public ProductController(IShopCategoryService iShopCategoryService,
            IProductService iProductService,
            ITypeService iTypeService,
            ICategoryService iCategoryService,
            IShopService iShopService,
            ISearchProductService iSearchProductService
            )
            : this()
        {
            _iShopCategoryService = iShopCategoryService;
            _iProductService = iProductService;
            _iTypeService = iTypeService;
            _iCategoryService = iCategoryService;
            _iShopService = iShopService;
            _iSearchProductService = iSearchProductService;
        }


        public ProductController()
            : base()
        {
            if (CurrentSellerManager != null)
                shopId = CurrentSellerManager.ShopId;
        }
        #endregion

        #region Action
        public ActionResult Create()
        {
            var model = InitCreateModel();

            string message;
            if (!CanCreate(out message))
                this.SendMessage(message);

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(ProductCreateModel model)
        {
            model.MinSalePrice = model.SKUExs.Min(p => p.SalePrice);//model.SKUInfoExs一定有值,当用户没有设置规格时将会有默认值

            if (!ModelState.IsValid)
            {
                var errlist = ModelState.Values;
                foreach (var item in errlist)
                {
                    if (item.Errors.Count > 0)Core.Log.Error(item.Errors[0].ErrorMessage);
                }
                return ValidError();
            }

            string message;
            if (!CanCreate(out message))
                return Json(false, message);

            var product = AutoMapper.Mapper.DynamicMap<DTO.Product.Product>(model);
            var success = false;
            try
            {
                var skus = model.SKUExs.Select(p =>
                {
                    p.Id = p.CreateId(null);
                    return p;
                }).ToArray();

                var descriptionInfo = model.Description;

                var attributes = model.GetProductAttribute(model.Id);
                var sellerSpecs = model.GetSellerSpecification(this.CurrentSellerManager.ShopId, product.TypeId);

                var productDto = Himall.Application.ProductManagerApplication.AddProduct(this.CurrentShop.Id, product, model.Pics, skus, descriptionInfo, attributes, model.GoodsCategory, sellerSpecs);
                _iSearchProductService.AddSearchProduct(productDto.Id);

                success = true;
            }
            catch (Exception e)
            {
                Core.Log.Error("创建商品失败", e);
            }

            try
            {
                //添加商家操作日志
                ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(new LogInfo
                {
                    Date = DateTime.Now,
                    Description = string.Format("商家发布商品，Id={0}, 名称={1} [{2}]", product.Id, product.ProductName, success ? "成功" : "失败"),
                    IPAddress = Request.UserHostAddress,
                    PageUrl = "/Product/Create",
                    UserName = CurrentSellerManager.UserName,
                    ShopId = CurrentSellerManager.ShopId
                });
            }
            catch
            {
            }

            if (success)
                return Json();
            return Json(false, "发布商品出错");
        }

        public ActionResult Edit(long id)
        {
            var product = ProductManagerApplication.GetProduct(id);
            if (product == null)
                return HttpNotFound();

            var model = InitEditModel(product);

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(ProductCreateModel model)
        {
            model.MinSalePrice = model.SKUExs.Min(p => p.SalePrice);//model.SKUInfoExs一定有值,当用户没有设置规格时将会有默认值

            if (!ModelState.IsValid)
                return ValidError();
            foreach (var item in model.SKUExs)
            {
                if (string.IsNullOrWhiteSpace(item.Color) && string.IsNullOrWhiteSpace(item.Size) && string.IsNullOrWhiteSpace(item.Version))
                {
                    item.ColorId = 0;
                    item.SizeId = 0;
                    item.VersionId = 0;
                    item.Id = model.Id + "_0_0_0";
                }
            }
            var skus = model.SKUExs.Select(sku =>
            {
                sku.ProductId = model.Id;
                sku.Id = sku.CreateId(model.Id);
                return sku;
            }).ToArray();

            //处理限时购 逻辑完全copy之前的
            #region 更改限时购数据
            using (ILimitTimeBuyService ilimit = ServiceHelper.Create<ILimitTimeBuyService>())
            {
                FlashSaleModel fsi = ilimit.GetFlaseSaleByProductId(model.Id);
                if (fsi != null)
                {
                    if (DateTime.Parse(fsi.EndDate) > DateTime.Now)
                        foreach (var sku in skus)
                        {
                            LimitOrderHelper.ModifyLimitStock(sku.Id, (int)sku.Stock, DateTime.Parse(fsi.EndDate));
                        }
                    using (Entity.Entities dbcontext = new Entity.Entities())
                    {
                        foreach (var sku in skus)
                        {
                            var fsd = dbcontext.FlashSaleDetailInfo.FirstOrDefault(t => t.SkuId == sku.Id);
                            if (fsd == null)
                            {
                                dbcontext.FlashSaleDetailInfo.Add(new FlashSaleDetailInfo
                                {
                                    FlashSaleId = fsi.Id,
                                    Price = sku.SalePrice,
                                    ProductId = fsi.ProductId,
                                    SkuId = sku.Id
                                });
                            }
                        }
                        dbcontext.SaveChanges();
                    }
                }
            }
            #endregion 更改限时购数据

            if (model.SaleStatus != ProductInfo.ProductSaleStatus.InDraft)
            {
                model.SaleStatus = ProductInfo.ProductSaleStatus.OnSale;
            }

            var descriptionInfo = model.Description;
            var product = model.Map<DTO.Product.Product>();
            var attributes = model.GetProductAttribute(model.Id);
            var sellerSpecs = model.GetSellerSpecification(this.CurrentSellerManager.ShopId, model.TypeId);
            bool success = false;
            try
            {
                Himall.Application.ProductManagerApplication.UpdateProduct(product, model.Pics, skus, descriptionInfo, attributes, model.GoodsCategory, sellerSpecs);
                _iSearchProductService.UpdateSearchProduct(product.Id);
                var curpro = ProductManagerApplication.GetProduct(product.Id);
                //修正门店规格
                ShopBranchApplication.CorrectBranchProductSkus(curpro.Id, curpro.ShopId);
                if (curpro.SaleStatus!= ProductInfo.ProductSaleStatus.OnSale || curpro.AuditStatus!= ProductInfo.ProductAuditStatus.Audited)
                {
                    //处理拼团
                    var fg = FightGroupApplication.GetActiveByProductId(curpro.Id);
                    if (fg != null)
                    {
                        fg.EndTime = DateTime.Now.AddMinutes(-1);
                        FightGroupApplication.UpdateActive(fg);
                    }
                    //处理门店
                    ShopBranchApplication.UnSaleProduct(curpro.Id);
                }
                success = true;
            }
            catch (Exception e)
            {
                Core.Log.Error("编辑商品异常", e);
            }

            try
            {
                //添加商家操作日志
                ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(new LogInfo
                {
                    Date = DateTime.Now,
                    Description = string.Format("商家修改商品，Id={0}, 名称={1} [{2}]", model.Id, model.ProductName, success ? "成功" : "失败"),
                    IPAddress = Request.UserHostAddress,
                    PageUrl = "/Product/Edit",
                    UserName = CurrentSellerManager.UserName,
                    ShopId = CurrentSellerManager.ShopId
                });
            }
            catch
            {
            }

            if (success)
                return Json();
            return Json(false, "操作失败！");
        }

        public ActionResult CategoryBrands(long categoryId)
        {
            var brands = ServiceHelper.Create<IBrandService>().GetBrandsByCategoryIds(this.CurrentShop.Id, categoryId).Select(p => new
            {
                p.Id,
                p.Name
            }).ToList();

            return Json(brands, JsonRequestBehavior.AllowGet);
        }

        // GET: SellerAdmin/Product
        public ActionResult PublicStepOne()
        {
            return View();
        }

        [UnAuthorize]
        private List<CategoryJsonModel> GetShopCategoryJson(long shopId)
        {
            var categories = _iShopCategoryService.GetShopCategory(shopId).ToArray();
            var json = new List<CategoryJsonModel>();
            foreach (var main in categories.Where(s => s.ParentCategoryId == 0))
            {
                var topC = new CategoryJsonModel()
                {
                    Name = main.Name,
                    Id = main.Id.ToString(),
                    SubCategory = new List<SecondLevelCategory>()
                };
                foreach (var secondItem in categories.Where(s => s.ParentCategoryId == main.Id))
                {
                    var secondC = new SecondLevelCategory()
                    {
                        Name = secondItem.Name,
                        Id = secondItem.Id.ToString(),
                    };

                    topC.SubCategory.Add(secondC);
                }
                json.Add(topC);
            }
            return json;
        }

        [UnAuthorize]
        public JsonResult GetShopProductCategory(long productId = 0)
        {
            var json = new ShopProductCategoryModel()
            {
                SelectedCategory = new List<SelectedCategory>()
            };
            json.Data = GetShopCategoryJson(shopId);
            if (0 != productId)
            {
                var cates = _iProductService.GetProductShopCategories(productId);
                foreach (var top in json.Data)
                {
                    var id = long.Parse(top.Id);
                    if (cates.Any(c => c.ShopCategoryId == id))
                        json.SelectedCategory.Add(new SelectedCategory
                        {
                            Id = top.Id,
                            Level = "1"
                        });
                    foreach (var second in top.SubCategory)
                    {
                        id = long.Parse(second.Id);
                        if (cates.Any(c => c.ShopCategoryId == id))
                            json.SelectedCategory.Add(new SelectedCategory
                            {
                                Id = second.Id,
                                Level = "2"
                            });
                    }
                }
            }
            return Json(new
            {
                json
            }, JsonRequestBehavior.AllowGet);
        }

        private List<TypeAttributesModel> GetPlateformAttr(long categoryId)
        {
            var cate = _iCategoryService.GetCategory(categoryId);
            var type = _iTypeService.GetType(cate.TypeId);
            var json = new List<TypeAttributesModel>();
            foreach (var attr in type.AttributeInfo)
            {
                var attrItem = new TypeAttributesModel()
                {
                    Name = attr.Name,
                    AttrId = attr.Id,
                    Selected = "",
                    IsMulti = attr.IsMulti,
                    AttrValues = new List<TypeAttrValue>()
                };
                foreach (var value in attr.AttributeValueInfo)
                {
                    attrItem.AttrValues.Add(new TypeAttrValue
                    {
                        Id = value.Id.ToString(),
                        Name = value.Value
                    });
                }
                json.Add(attrItem);
            }
            return json;
        }

        [UnAuthorize]
        public JsonResult GetAttributes(long categoryId, long productId = 0, long isCategoryId = 0)
        {
            Dictionary<long, string> AttrMap = new Dictionary<long, string>();
            if (productId > 0)
            {
                var product = _iProductService.GetProduct(productId);
                if (product != null)
                {
                    //修改商品分类时获取分类属性
                    if (product.CategoryId != categoryId)
                    {
                        isCategoryId = 1;
                    }
                }
            }
            //如果是发布商品
            if (isCategoryId == 1)
            {
                var json = GetPlateformAttr(categoryId);
                return Json(new
                {
                    json
                }, JsonRequestBehavior.AllowGet);
            }
            //如果是编辑商品
            else
            {
                var json = new List<TypeAttributesModel>();

                //获取商品已经有的属性
                var attrs = _iProductService.GetProductAttribute(productId);
                if (null == attrs || attrs.Count() == 0)
                {
                    json = GetPlateformAttr(categoryId);
                    return Json(new
                    {
                        json
                    }, JsonRequestBehavior.AllowGet);
                }
                foreach (var item in attrs.ToArray())
                {
                    //使用字典处理同一属性的多个属性值被选中
                    if (!AttrMap.ContainsKey(item.AttributeId))
                        AttrMap.Add(item.AttributeId, item.ValueId.ToString());
                    else
                    {
                        AttrMap[item.AttributeId] = AttrMap[item.AttributeId] + "," + item.ValueId.ToString();
                        continue;
                    }

                    var attr = item.AttributesInfo;
                    //获取平台与该属性对应的所有属性值
                    var attrValue = _iTypeService.
                        GetType(attr.TypeId).
                        AttributeInfo.FirstOrDefault(a => a.Id == attr.Id).
                        AttributeValueInfo;
                    var attrItem = new TypeAttributesModel()
                    {
                        Name = attr.Name,
                        AttrId = item.AttributeId,
                        Selected = "",
                        IsMulti = attr.IsMulti,
                        AttrValues = new List<TypeAttrValue>()
                    };
                    //添加属性值
                    foreach (var value in attrValue.ToArray())
                    {
                        attrItem.AttrValues.Add(new TypeAttrValue
                        {
                            Id = value.Id.ToString(),
                            Name = value.Value
                        });
                    }
                    categoryId = _iProductService.GetProduct(productId).CategoryId;
                    json.Add(attrItem);
                }
                var platAttr = GetPlateformAttr(categoryId);
                //更新选中属性值
                foreach (var item in json)
                {
                    item.Selected = AttrMap[item.AttrId];
                    platAttr.Remove(platAttr.FirstOrDefault(a => a.AttrId == item.AttrId));
                }
                json.AddRange(platAttr);
                return Json(new
                {
                    json
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private SpecJosnModel GetPlatformSpec(long categoryId, long productId = 0)
        {
            var data = new SpecJosnModel
            {
                json = new List<TypeSpecificationModel>(),
                tableData = new tableDataModel()
                {
                    cost = new List<SKUSpecModel>(),
                    mallPrice = new List<SKUSpecModel>(),
                    productId = productId,
                    sku = new List<SKUSpecModel>(),
                    stock = new List<SKUSpecModel>()
                }
            };
            var cate = _iCategoryService.GetCategory(categoryId);
            var type = _iTypeService.GetType(cate.TypeId);

            foreach (SpecificationType spec in Enum.GetValues(typeof(SpecificationType)))
            {
                bool isreaddata = true;
                if (productId == 0)
                {
                    isreaddata = false;
                    switch (spec)
                    {
                        case SpecificationType.Color:
                            if (type.IsSupportColor)
                            {
                                isreaddata = true;
                            }
                            break;
                        case SpecificationType.Size:
                            if (type.IsSupportSize)
                            {
                                isreaddata = true;
                            }
                            break;
                        case SpecificationType.Version:
                            if (type.IsSupportVersion)
                            {
                                isreaddata = true;
                            }
                            break;
                    }
                }
                if (isreaddata)
                {
                    var specItem = new TypeSpecificationModel()
                    {
                        Name = Enum.GetNames(typeof(SpecificationType))[(int)spec - 1],
                        Values = new List<Specification>(),
                        SpecId = (int)spec
                    };
                    foreach (var value in type.SpecificationValueInfo.Where(s => (int)s.Specification == (int)spec).OrderBy(s => s.Value))
                    {
                        specItem.Values.Add(new Specification
                        {
                            Id = value.Id.ToString(),
                            Name = value.Value,
                            isPlatform = true,
                            Selected = false
                        });
                    }
                    data.json.Add(specItem);
                }
            }
            var skus = _iProductService.GetSKUs(productId).OrderBy(s => s.Color).ThenBy(s => s.Size).ThenBy(s => s.Version);

            InitialTableData(skus, data);

            return data;
        }

        private void InitialTableData(IQueryable<SKUInfo> skus, SpecJosnModel data)
        {
            if (skus.Count() == 0)
                return;
            var specType = 0;
            var value = "";
            var skuArray = skus.ToArray();
            if (!string.IsNullOrWhiteSpace(skuArray[0].Version))
            {
                specType = 2;
                value = skuArray[0].Version;
            }
            if (!string.IsNullOrWhiteSpace(skuArray[0].Size))
            {
                specType = 1;
                value = skuArray[0].Size;
            }
            if (!string.IsNullOrWhiteSpace(skuArray[0].Color))
            {
                specType = 0;
                value = skuArray[0].Color;
            }
            if (string.IsNullOrWhiteSpace(value))
                return;
            SKUSpecModel cost = new SKUSpecModel()
            {
                ValueSet = new List<string>()
            };
            SKUSpecModel stock = new SKUSpecModel()
            {
                ValueSet = new List<string>()
            };
            SKUSpecModel sku = new SKUSpecModel()
            {
                ValueSet = new List<string>()
            };
            SKUSpecModel mallPrice = new SKUSpecModel()
            {
                ValueSet = new List<string>()
            };
            foreach (var s in skus)
            {
                var specValue = "";
                switch (specType)
                {
                    case 0:
                        specValue = s.Color;
                        break;

                    case 1:
                        specValue = s.Size;
                        break;

                    case 2:
                        specValue = s.Version;
                        break;
                }
                if (specValue.Equals(value))
                {
                    cost.ValueSet.Add(s.CostPrice == 0 ? "" : s.CostPrice.ToString("f2"));
                    cost.index = specValue;

                    stock.ValueSet.Add(s.Stock == 0 ? "" : s.Stock.ToString("f2"));
                    stock.index = specValue;

                    sku.ValueSet.Add(s.Sku);
                    sku.index = specValue;

                    mallPrice.ValueSet.Add(s.SalePrice == 0 ? "" : s.SalePrice.ToString("f2"));
                    mallPrice.index = specValue;
                }
                else
                {
                    data.tableData.cost.Add(DeepClone(cost));
                    data.tableData.stock.Add(DeepClone(stock));
                    data.tableData.sku.Add(DeepClone(sku));
                    data.tableData.mallPrice.Add(DeepClone(mallPrice));

                    cost = new SKUSpecModel()
                    {
                        ValueSet = new List<string>()
                    };
                    stock = new SKUSpecModel()
                    {
                        ValueSet = new List<string>()
                    };
                    sku = new SKUSpecModel()
                    {
                        ValueSet = new List<string>()
                    };
                    mallPrice = new SKUSpecModel()
                    {
                        ValueSet = new List<string>()
                    };
                    cost.ValueSet.Add(s.CostPrice == 0 ? "" : s.CostPrice.ToString("f2"));
                    cost.index = specValue;

                    stock.ValueSet.Add(s.Stock == 0 ? "" : s.Stock.ToString("f2"));
                    stock.index = specValue;

                    sku.ValueSet.Add(s.Sku);
                    sku.index = specValue;

                    mallPrice.ValueSet.Add(s.SalePrice == 0 ? "" : s.SalePrice.ToString("f2"));
                    mallPrice.index = specValue;
                    value = specValue;
                }
            }
            data.tableData.cost.Add(DeepClone(cost));
            data.tableData.stock.Add(DeepClone(stock));
            data.tableData.sku.Add(DeepClone(sku));
            data.tableData.mallPrice.Add(DeepClone(mallPrice));
        }

        private SKUSpecModel DeepClone(SKUSpecModel obj)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, obj);
                memory.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(memory) as SKUSpecModel;
            }
        }

        private List<SKUInfo> GetProducrSpec(List<SKUInfo> skuList)
        {
            List<SKUInfo> temp = new List<SKUInfo>();
            foreach (var sku in skuList)
            {
                var array = string.IsNullOrWhiteSpace(sku.Id) ? new string[1] { "" } : sku.Id.Split('_');
                temp.Add(new SKUInfo
                {
                    Color = array.Length >= 2 ? array[1] : "",
                    Size = array.Length >= 3 ? array[2] : "",
                    Version = array.Length >= 4 ? array[3] : "",
                    Id = sku.Id
                });
            }
            return temp;
        }

        [UnAuthorize]
        public JsonResult GetSpecifications(long categoryId, long productId = 0, long isCategoryId = 0)
        {
            Dictionary<long, string> SpecMap = new Dictionary<long, string>();
            var cate = _iCategoryService.GetCategory(categoryId);

            //平台规格
            var platfromSpec = GetPlatformSpec(categoryId, productId);

            //商家规格
            var sellerSpec = _iProductService
                .GetSellerSpecifications(shopId, cate.TypeId);

            var SKUs = _iProductService.GetSKUs(productId).ToList();

            //商品规格
            var productSpec = GetProducrSpec(SKUs);
            var skusList = SKUs.ToList();
            //遍历平台规格，使用商家规格或者商家规格覆盖
            foreach (var item in platfromSpec.json)
            {
                var specs = sellerSpec.Where(s => (int)s.Specification == item.SpecId);
                Specification updateSpec = null;

                foreach (var pspec in specs)
                {
                    updateSpec = item.Values.FirstOrDefault(s => s.Id == pspec.ValueId.ToString());

                    #region 使用商家的数据覆盖(使用商家规格值)

                    if (null != updateSpec && updateSpec.Id == pspec.ValueId.ToString())
                    {
                        updateSpec.Name = pspec.Value;
                        updateSpec.isPlatform = false;
                    }

                    //判断规格是否选中(使用商家规格值)
                    if (skusList.Any(s => s.Color.Equals(pspec.Value)) ||
                        skusList.Any(s => s.Size.Equals(pspec.Value)) ||
                        skusList.Any(s => s.Version.Equals(pspec.Value)))
                        updateSpec.Selected = true;

                    #endregion 使用商家的数据覆盖(使用商家规格值)
                }

                #region 使用商品SKU的规格值覆盖

                foreach (var val in item.Values)
                {
                    if (item.Name == "Color"
                    && productSpec.Any(s => val.Id == s.Color))
                    {
                        var sku = productSpec.FirstOrDefault(s => val.Id == s.Color);
                        val.Name = SKUs.FirstOrDefault(p => p.Id == sku.Id).Color;
                        val.isPlatform = false;
                        val.Selected = true;
                    }
                    if (item.Name == "Size"
                        && productSpec.Any(s => val.Id == s.Size))
                    {
                        var sku = productSpec.FirstOrDefault(s => val.Id == s.Size);
                        val.Name = SKUs.FirstOrDefault(p => p.Id == sku.Id).Size;
                        val.isPlatform = false;
                        val.Selected = true;
                    }
                    if (item.Name == "Version"
                        && productSpec.Any(s => val.Id == s.Version))
                    {
                        var sku = productSpec.FirstOrDefault(s => val.Id == s.Version);
                        val.Name = SKUs.FirstOrDefault(p => p.Id == sku.Id).Version;
                        val.isPlatform = false;
                        val.Selected = true;
                    }
                }

                #endregion 使用商品SKU的规格值覆盖
            }
            return Json(new
            {
                data = platfromSpec
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Specifications(long categoryId, long productId = 0)
        {
            Dictionary<Dictionary<SpecificationType, long>, SKUInfo> skus = null;
            if (productId > 0)
                skus = _iProductService.GetSKUs(productId).ToDictionary(p => SKUEx.SplitId(p.Id), p => p);

            List<SpecificationValueInfo> specifications = ObjectContainer.Current.Resolve<ISpecificationService>().GetSpecification(categoryId, this.CurrentShop.Id);
            SpecificationValueInfo info = new SpecificationValueInfo();
            if (specifications != null && specifications.Count > 0) info = specifications[0];
            var newspecifications = specifications.GroupBy(item => item.Specification).Select(item => new
            {
                Specification = new
                {
                    Value = (int)item.Key,
                    Name = item.Key.ToString(),
                    Alias = GetSpecificationTypeAlias(item.Key, info),
                    Text = item.Key.ToDescription(),
                    NeedPic = item.Key == SpecificationType.Color//是否需要设置图片
                },
                Values = item.Select(model =>
                    {
                        var value = model.Value;
                        if (skus != null)
                        {
                            var key = skus.Keys.FirstOrDefault(dic => dic.ContainsKey(item.Key) && dic[item.Key] == model.Id);
                            if (key != null)
                            {
                                string _tmpvalue = "";
                                switch (item.Key)
                                {
                                    case SpecificationType.Color:
                                        _tmpvalue = skus[key].Color;
                                        break;
                                    case SpecificationType.Size:
                                        _tmpvalue = skus[key].Size;
                                        break;
                                    default:
                                        _tmpvalue = skus[key].Version;
                                        break;
                                }
                                if (!string.IsNullOrWhiteSpace(_tmpvalue))
                                {
                                    value = _tmpvalue;
                                }
                            }
                        }

                        return new
                        {
                            model.Id,
                            Value = value
                        };
                    }).ToArray()
            });

            return Json(newspecifications, JsonRequestBehavior.AllowGet);
        }

        private string GetSpecificationTypeAlias(SpecificationType typeId, SpecificationValueInfo item)
        {
            string alias = "";
            switch (typeId)
            {
                case SpecificationType.Color:
                    alias = item.TypeInfo.ColorAlias;
                    break;
                case SpecificationType.Size:
                    alias = item.TypeInfo.SizeAlias;
                    break;
                default:
                    alias = item.TypeInfo.VersionAlias;
                    break;
            }
            return alias;
        }

        [UnAuthorize]
        public ActionResult PublicStepTwo(string categoryNames = "", long categoryId = 0, long productId = 0)
        {
            var iProduct = _iProductService;
            var iCategory = _iCategoryService;
            string sale = "0", stock = "0", cost = "0";
            bool isChangeCate = false;
            ProductInfo product = new ProductInfo();
            if (productId != 0)
            {
                product = iProduct.GetProduct(productId);
                if (product == null || product.ShopId != CurrentSellerManager.ShopId)
                {
                    throw new HimallException(productId + ",该商品已删除或者不属于该店铺");
                }
                if (product.SKUInfo.Count() > 0)
                {
                    var tempS = product.SKUInfo.Where(s => s.SalePrice > 0);
                    var tempC = product.SKUInfo.Where(s => s.CostPrice > 0);
                    sale = tempS.Count() == 0 ? product.MinSalePrice.ToString("f3") : tempS.Min(s => s.SalePrice).ToString();
                    stock = product.SKUInfo.Sum(s => s.Stock).ToString();
                    cost = tempC.Count() == 0 ? cost : tempC.Min(s => s.CostPrice).ToString();
                }
                if (string.IsNullOrWhiteSpace(categoryNames))
                {
                    var cateArray = product.CategoryPath.Split('|');
                    for (int i = 0; i < cateArray.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(cateArray[i]))
                        {
                            var cobj = iCategory.GetCategory(long.Parse(cateArray[i]));
                            categoryNames += string.Format("{0} {1} ",
                                cobj == null ? "" : cobj.Name,
                                (i == cateArray.Length - 1) ? "" : " > ");
                        }
                    }
                }
                if (categoryId == 0)
                {
                    categoryId = product.CategoryId;
                }
                else
                {
                    if (categoryId != product.CategoryId)
                    {
                        isChangeCate = true;
                    }
                }
                product.CategoryNames = categoryNames;
                if (isChangeCate)
                {
                    product.SKUInfo = null;
                    product.CategoryId = categoryId;
                }
            }
            else
            {
                product = new ProductInfo();
                product.CategoryId = categoryId;
                product.CategoryNames = categoryNames;
            }

            var brands = ServiceHelper.Create<IBrandService>().GetBrandsByCategoryIds(shopId, categoryId);
            List<SelectListItem> brandList = new List<SelectListItem> { new SelectListItem
                {
                    Selected = false,
                    Text ="请选择品牌...",
                    Value = "0"
                }};
            foreach (var item in brands)
            {
                brandList.Add(new SelectListItem
                {
                    Selected = productId != 0 && product != null && product.BrandId == item.Id,
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }

            var freightTemplates = ServiceHelper.Create<IFreightTemplateService>().GetShopFreightTemplate(CurrentSellerManager.ShopId);
            List<SelectListItem> freightList = new List<SelectListItem> { new SelectListItem
                {
                    Selected = false,
                    Text ="请选择运费模板...",
                    Value = "0"
                }};
            foreach (var item in freightTemplates)
            {
                freightList.Add(new SelectListItem
                {
                    Selected = productId != 0 && product != null && product.FreightTemplateId == item.Id,
                    Text = item.Name + "【" + item.ValuationMethod.ToDescription() + "】",
                    Value = item.Id.ToString()
                });
            }
            product.IsCategory = productId == 0 ? 1 : 0;
            product.ShopId = shopId;
            product.TopId = 0 == product.Id ? 0 : product.ProductDescriptionInfo.DescriptionPrefixId;
            product.BottomId = 0 == product.Id ? 0 : product.ProductDescriptionInfo.DescriptiondSuffixId;

            ViewBag.FreightTemplates = freightList;
            ViewBag.BrandDrop = brandList;
            ViewBag.SalePrice = sale;
            ViewBag.Stock = stock;
            ViewBag.CostPrice = cost;

            return View(product);
        }

        public JsonResult GetFreightTemplate()
        {
            var freightTemplates = ServiceHelper.Create<IFreightTemplateService>().GetShopFreightTemplate(CurrentSellerManager.ShopId);
            List<SelectListItem> freightList = new List<SelectListItem> { new SelectListItem
                {
                    Selected = false,
                    Text ="请选择运费模板...",
                    Value = "0"
                }};
            foreach (var item in freightTemplates)
            {
                freightList.Add(new SelectListItem
                {
                    Text = item.Name + "【" + item.ValuationMethod.ToDescription() + "】",
                    Value = item.Id.ToString()
                });
            }
            return Json(new
            {
                success = true,
                model = freightList.Where(f => !f.Value.Equals("0")).ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Management()
        {
            var auditOnOff = ServiceHelper.Create<ISiteSettingService>().GetSiteSettings().ProdutAuditOnOff;
            ViewBag.AuditOnOff = auditOnOff;
            return View();
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult List(ProductQuery queryModel, string auditStatuses, int page, int rows)
        {
            queryModel.PageSize = rows;
            queryModel.PageNo = page;
            queryModel.ShopId = CurrentSellerManager.ShopId;

            var products = ProductManagerApplication.GetProducts(queryModel);
            var productDescriptions = ProductManagerApplication.GetProductDescription(products.Models.Select(p => p.Id).ToArray());
            var categories = CategoryApplication.GetCategories();
            var brands = BrandApplication.GetBrandsByIds(products.Models.Select(p => p.BrandId));
            var relationProducts = ProductManagerApplication.GetRelationProductByProductIds(products.Models.Select(p => p.Id));
            //取超出安全库存的商品ID
            var overSafeStockPids = ProductManagerApplication.GetOverSafeStockProductIds(products.Models.Select(p => p.Id));

            var skus = ProductManagerApplication.GetSKU(products.Models.Select(p => p.Id));

            var dataGrid = new DataGridModel<Himall.Web.Areas.SellerAdmin.Models.ProductModel>();
            dataGrid.total = products.Total;
            dataGrid.rows = products.Models.ToArray().Select(item =>
            {
                var cate = _iShopCategoryService.GetCategoryByProductId(item.Id);
                return new Himall.Web.Areas.SellerAdmin.Models.ProductModel()
                {
                    Name = item.ProductName,
                    Id = item.Id,
                    Image = item.GetImage(ImageSize.Size_50),
                    Price = item.MinSalePrice,
                    Url = "",
                    PublishTime = item.AddedDate.ToString("yyyy-MM-dd HH:mm"),
                    SaleState = (int)item.SaleStatus,
                    CategoryId = item.CategoryId,
                    BrandId = item.BrandId,
                    AuditState = (int)item.AuditStatus,
                    AuditReason = productDescriptions.Any(pd => pd.ProductId == item.Id) ? productDescriptions.FirstOrDefault(pd => pd.ProductId == item.Id).AuditReason : "",
                    ProductCode = item.ProductCode,
                    QrCode = GetQrCodeImagePath(item.Id),
                    SaleCount = item.SaleCounts,
                    Unit = item.MeasureUnit,
                    Uid = CurrentSellerManager.Id,
                    CategoryName = cate == null ? "" : cate.Name,
                    BrandName = item.BrandId == 0 || !brands.Any(b => b.Id == item.BrandId) ? "" : brands.FirstOrDefault(b => b.Id == item.BrandId).Name,
                    RelationProducts = relationProducts.Any(p => p.ProductId == item.Id) ? relationProducts.FirstOrDefault(p => p.ProductId == item.Id).Relation : "",
                    IsOverSafeStock = overSafeStockPids.Any(e => e == item.Id),
                    Stock = skus.Where(sku => sku.ProductId == item.Id).Sum(sku => sku.Stock),
                    MaxBuyCount = item.MaxBuyCount
                };
            }).ToList();

            return Json(dataGrid);
        }

        public ActionResult ExportToExcel(long? categoryId = null, string productCode = "", string brandName = "", int? auditStatus = null, string auditStatuses = null, int? saleStatus = null, string ids = "", string keyWords = "", DateTime? startDate = null, DateTime? endDate = null)
        {
            #region 获取查询数据

            var queryModel = new ProductQuery()
            {
                SaleStatus = (Model.ProductInfo.ProductSaleStatus?)saleStatus,
                PageSize = int.MaxValue,
                PageNo = 1,
                BrandNameKeyword = brandName,
                KeyWords = keyWords,
                ShopCategoryId = categoryId,
                Ids = string.IsNullOrWhiteSpace(ids) ? null : ids.Split(',').Select(item => long.Parse(item)),
                ShopId = CurrentSellerManager.ShopId,
                StartDate = startDate,
                EndDate = endDate,
                ProductCode = productCode
            };
            if (!string.IsNullOrWhiteSpace(auditStatuses))
            {
                queryModel.AuditStatus = auditStatuses.Split(',').Select(item => (Model.ProductInfo.ProductAuditStatus)(long.Parse(item))).ToArray();
                if (auditStatuses == "1,3" || auditStatuses == "1")//查询待审核时，仅查出售中状态的
                    queryModel.SaleStatus = Model.ProductInfo.ProductSaleStatus.OnSale;
            }

            if (saleStatus.HasValue && saleStatus == (int)Model.ProductInfo.ProductSaleStatus.InStock && auditStatus != (int)Model.ProductInfo.ProductAuditStatus.InfractionSaleOff)
                queryModel.AuditStatus = new Model.ProductInfo.ProductAuditStatus[] { Model.ProductInfo.ProductAuditStatus.Audited, Model.ProductInfo.ProductAuditStatus.UnAudit, Model.ProductInfo.ProductAuditStatus.WaitForAuditing };

            if (auditStatus.HasValue)
                queryModel.AuditStatus = new Model.ProductInfo.ProductAuditStatus[] { (Model.ProductInfo.ProductAuditStatus)auditStatus };

            ObsoletePageModel<Himall.Model.ProductInfo> productEntities = _iProductService.GetProducts(queryModel);
            IShopCategoryService shopCategoryService = _iShopCategoryService;

            IBrandService brandService = ServiceHelper.Create<IBrandService>();
            IEnumerable<ProductInfoForExportModel> products = productEntities.Models.ToArray().Select(item =>
            {
                var brand = brandService.GetBrand(item.BrandId);
                var cate = shopCategoryService.GetCategoryByProductId(item.Id);
                return new Himall.Web.Areas.SellerAdmin.Models.ProductInfoForExportModel()
                {
                    Id = item.Id,
                    CategoryName = cate == null ? "" : cate.Name,
                    BrandName = item.BrandId == 0 || brand == null ? "" : brand.Name,
                    ProductName = item.ProductName,
                    MarketPrice = item.MarketPrice,
                    MinSalePrice = item.MinSalePrice,
                    ProductCode = item.ProductCode,
                    ShortDescription = item.ShortDescription,
                    SaleStatus = item.SaleStatus,
                    AddedDate = item.AddedDate,
                    HasSKU = (item.SKUInfo != null && item.SKUInfo.Count() == 1 && item.SKUInfo.FirstOrDefault().Id.Contains("0_0_0")) ? false : true,
                    VistiCounts = item.VistiCounts,
                    SaleCounts = item.SaleCounts,
                    AuditStatus = item.AuditStatus,
                    AuditReason = item.ProductDescriptionInfo != null ? item.ProductDescriptionInfo.AuditReason : "",
                    Quantity = item.Quantity,
                    MeasureUnit = item.MeasureUnit,
                    SKUInfo = item.SKUInfo
                };
            }).ToList();



            #endregion 获取查询数据

            #region 构建Excel文档

            ViewData.Model = products;
            string viewHtml = RenderPartialViewToString(this, "ExportProductinfo");
            return File(System.Text.Encoding.UTF8.GetBytes(viewHtml), "application/ms-excel", string.Format("店铺商品信息_{0}.xls", DateTime.Now.ToString("yyyy-MM-dd")));

            #endregion
        }

        [NonAction]
        protected string RenderPartialViewToString(Controller controller, string partialViewName)
        {
            IView view = ViewEngines.Engines.FindPartialView(controller.ControllerContext, partialViewName).View;
            using (StringWriter writer = new StringWriter())
            {
                ViewContext viewContext = new ViewContext(controller.ControllerContext, view, controller.ViewData, controller.TempData, writer);
                viewContext.View.Render(viewContext, writer);
                return writer.ToString();
            }
        }

        public string GetQrCodeImagePath(long productId)
        {
            var map = Core.Helper.QRCodeHelper.Create("http://" + HttpContext.Request.Url.Authority + "/m-wap/product/detail/" + productId);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            //  将图片内存流转成base64,图片以DataURI形式显示
            string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
            return strUrl;
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult Browse(long? categoryId, int? auditStatus, string ids, int page, int rows, string keyWords,
            int? saleStatus, bool? isShopCategory, bool isLimitTimeBuy = false, bool showSku = false, long[] exceptProductIds = null)
        {
            var queryModel = new ProductQuery()
            {
                PageSize = rows,
                PageNo = page,
                KeyWords = keyWords,
                CategoryId = isShopCategory.GetValueOrDefault() ? null : categoryId,
                ShopCategoryId = isShopCategory.GetValueOrDefault() ? categoryId : null,
                Ids = string.IsNullOrWhiteSpace(ids) ? null : ids.Split(',').Select(item => long.Parse(item)),
                ShopId = CurrentSellerManager.ShopId,
                IsLimitTimeBuy = isLimitTimeBuy,
                ExceptIds = exceptProductIds
            };
            if (auditStatus.HasValue)
                queryModel.AuditStatus = new Model.ProductInfo.ProductAuditStatus[] { (Model.ProductInfo.ProductAuditStatus)auditStatus };

            if (saleStatus.HasValue)
                queryModel.SaleStatus = (Himall.Model.ProductInfo.ProductSaleStatus)saleStatus;

            ObsoletePageModel<Himall.Model.ProductInfo> productEntities = _iProductService.GetProducts(queryModel);
            ICategoryService productCategoryService = _iCategoryService;
            IBrandService brandService = ServiceHelper.Create<IBrandService>();
            var products = productEntities.Models.ToArray().Select(item =>
                {
                    var brand = brandService.GetBrand(item.BrandId);
                    var cate = productCategoryService.GetCategory(item.CategoryId);
                    return new
                    {
                        name = item.ProductName,
                        brandName = item.BrandId == 0 || brand == null ? "" : brand.Name,
                        categoryName = brand == null ? "" : cate.Name,
                        id = item.Id,
                        imgUrl = item.GetImage(ImageSize.Size_50),
                        price = item.MinSalePrice,
                        skus = !showSku ? null : item.SKUInfo.Select(a => new SKUModel()
                        {
                            Id = a.Id,
                            SalePrice = a.SalePrice,
                            Size = a.Size,
                            Stock = a.Stock,
                            Version = a.Version,
                            Color = a.Color,
                            Sku = a.Sku,
                            AutoId = a.AutoId,
                            ProductId = a.ProductId
                        })
                    };
                });

            var dataGrid = new
            {
                rows = products,
                total = productEntities.Total
            };
            return Json(dataGrid);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult BatchSaleOff(string ids)
        {
            IEnumerable<long> ids_long = ids.Split(',').Select(item => long.Parse(item));

            _iProductService.SaleOff(ids_long, CurrentSellerManager.ShopId);
            ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(
           new LogInfo
           {
               Date = DateTime.Now,
               Description = "商家商品批量下架，Ids=" + ids,
               IPAddress = Request.UserHostAddress,
               PageUrl = "/Product/BatchSaleOff",
               UserName = CurrentSellerManager.UserName,
               ShopId = CurrentSellerManager.ShopId
           });

            _iSearchProductService.UpdateSearchStatusByProducts(ids_long.ToList());
            foreach (var item in ids_long)
            {
                //处理门店
                ShopBranchApplication.UnSaleProduct(item);
            }

            return Json(new
            {
                success = true
            });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult BatchOnSale(string ids)
        {
            IEnumerable<long> ids_long = ids.Split(',').Select(item => long.Parse(item));

            _iProductService.OnSale(ids_long, CurrentSellerManager.ShopId);
            ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(
           new LogInfo
           {
               Date = DateTime.Now,
               Description = "商家商品批量上架，Ids=" + ids,
               IPAddress = Request.UserHostAddress,
               PageUrl = "/Product/BatchOnSale",
               UserName = CurrentSellerManager.UserName,
               ShopId = CurrentSellerManager.ShopId
           });
            #region 更新搜索状态
            _iSearchProductService.UpdateSearchStatusByProducts(ids_long.ToList());
            #endregion
            return Json(new
            {
                success = true
            });
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult Delete(string ids)
        {
            try
            {
                IEnumerable<long> ids_long = ids.Split(',').Select(item => long.Parse(item));
                //状态改为 已删除
                ProductManagerApplication.DeleteProduct(ids_long, CurrentSellerManager.ShopId);

                _iSearchProductService.UpdateSearchStatusByProducts(ids_long.ToList());

                foreach (var id in ids_long)
                {
                    string path = (string.Format(@"/Storage/Shop/{0}/Products/{1}", CurrentSellerManager.ShopId, id));
                    if (HimallIO.ExistDir(path))
                        Core.HimallIO.DeleteDir(path, true);
                }

                ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(
                new LogInfo
                {
                    Date = DateTime.Now,
                    Description = "商家删除商品，Ids=" + ids,
                    IPAddress = Request.UserHostAddress,
                    PageUrl = "/Product/Delete",
                    UserName = CurrentSellerManager.UserName,
                    ShopId = CurrentSellerManager.ShopId
                });
                return Json(new
                {
                    success = true
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    msg = e.Message
                });
            }
        }

        /// <summary>
        /// 删除指定路径文件夹
        /// </summary>
        /// <param name="path"></param>
        private void DeleteDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Exists)
            {
                DirectoryInfo[] childs = dir.GetDirectories();
                foreach (DirectoryInfo child in childs)
                {
                    child.Delete(true);
                }
                dir.Delete(true);
            }
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult BindTemplates(string ids, long? topTemplateId, long? bottomTemplateId)
        {
            IEnumerable<long> productIds = ids.Split(',').Select(item => long.Parse(item));
            _iProductService.BindTemplate(topTemplateId, bottomTemplateId, productIds);
            foreach (var pid in productIds)
            {
                string cacheKey = CacheKeyCollection.CACHE_PRODUCTDESC(pid);
                Cache.Remove(cacheKey);
            }
            return Json(new
            {
                success = true
            });
        }

        public JsonResult Recommend(long productId, string productIds)
        {
            bool success = false;
            try
            {
                Application.ProductManagerApplication.UpdateRelationProduct(productId, productIds);
                success = true;
            }
            catch
            { }

            return Json(success);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SetProductOverSafeStock(string ids, long stock)
        {
            if (stock == 0)
            {
                throw new HimallException("库存不能为0");
            }
            IEnumerable<long> productIds = ids.Split(',').Select(item => long.Parse(item));
            ProductManagerApplication.SetProductOverSafeStock(productIds, stock);
            return Json(new
            {
                success = true
            });
        }

        #endregion

        #region 私有方法
        private JsonResult Json(bool success = true, string message = null)
        {
            return Json(new
            {
                Success = success,
                Message = message
            }, true);
        }

        private JsonResult ValidError()
        {
            var errors = ModelState.Where(p => p.Value.Errors.Count > 0).Select(p => new
            {
                p.Key,
                p.Value.Errors[0].ErrorMessage
            }).ToArray();

            return Json(new
            {
                success = false,
                errors,
                message = "验证失败"
            });
        }

        private bool CanCreate(out string message)
        {
            if (ServiceHelper.Create<IShopService>().GetShopSpaceUsage(this.CurrentSellerManager.ShopId) == -1)
            {
                message = "存储图片空间不足,不能发布商品!";
                return false;
            }

            var grade = ServiceHelper.Create<IShopService>().GetShopGrade(this.CurrentShop.GradeId);
            if (grade != null)
            {
                var count = _iProductService.GetShopAllProducts(this.CurrentSellerManager.ShopId);
                if (count >= grade.ProductLimit)
                {
                    message = "此店铺等级最多只能发布" + grade.ProductLimit + "件商品";
                    return false;
                }
            }

            message = "";
            return true;
        }

        private ProductCreateModel InitCreateModel(ProductCreateModel model = null)
        {
            if (model == null)
                model = new ProductCreateModel();

            var shopId = this.CurrentShop.Id;
            var categories = _iShopCategoryService.GetBusinessCategory(shopId, this.CurrentShop.IsSelf);
            var categoryGroups = this.GroupCategory(categories.ToList());

            model.CategoryGroups = categoryGroups;
            model.FreightTemplates = FreightTemplateApplication.GetShopFreightTemplate(shopId);
            model.ShopCategorys = ShopCategoryApplication.GetShopCategory(shopId);
            model.DescriptionTemplates = ProductManagerApplication.GetDescriptionTemplatesByShopId(shopId);

            return model;
        }

        private ProductCreateModel InitEditModel(Himall.DTO.Product.Product product, ProductCreateModel model = null)
        {
            if (model == null)
            {
                model = AutoMapper.Mapper.DynamicMap<ProductCreateModel>(product);
                model.GoodsCategory = ProductManagerApplication.GetProductShopCategoriesByProductId(product.Id).Select(item => item.ShopCategoryId).ToArray();
                model.SKUExs = SKUApplication.GetByProductIds(new[] { product.Id }).Select(p => p.DynamicMap<SKUEx>()).ToArray();
                model.Description = ProductManagerApplication.GetProductDescription(product.Id);

                model.SelectAttributes = ProductManagerApplication.GetProductAttribute(product.Id).GroupBy(p => p.AttributeId).Select(p => new AttrSelectData()
                {
                    AttributeId = p.Key,
                    ValueId = string.Join(",", p.Select(item => item.ValueId))
                }).ToArray();
            }

            model.Stock = model.SKUExs.Sum(p => p.Stock);
            model.SafeStock = model.SKUExs.Min(p => p.SafeStock);

            InitCreateModel(model);

            return model;
        }

        private List<CategoryGroup> GroupCategory(List<CategoryInfo> categorys, long pid = 0)
        {
            var result = new List<CategoryGroup>();
            var parents = categorys.Where(p => p.ParentCategoryId == pid);

            foreach (var parent in parents)
            {
                var item = new CategoryGroup();
                item.Id = parent.Id;
                item.Name = parent.Name;
                item.Path = parent.Path;
                item.TypeId = parent.TypeId;
                item.SubCategorys = GroupCategory(categorys, item.Id);

                result.Add(item);
            }

            return result;
        }
        #endregion
    }
}