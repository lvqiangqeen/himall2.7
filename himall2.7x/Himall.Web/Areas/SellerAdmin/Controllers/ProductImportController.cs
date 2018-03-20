using Himall.Core;
using Himall.Core.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using LumenWorks.Framework.IO.Csv;
using Himall.CommonModel;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class ProductImportController : BaseSellerController
    {
        private ICategoryService _iCategoryService;
        private IShopService _iShopService;
        private IBrandService _iBrandService;
        public ProductImportController(
            ICategoryService iCategoryService,
            IShopService iShopService,
            IBrandService iBrandService
            )
            : this()
        {
            _iBrandService = iBrandService;
            _iShopService = iShopService;
            _iCategoryService = iCategoryService;

        }
        private long _shopid = 0;
        private long _userid = 0;
        public ProductImportController()
            : base()
        {
            //退出登录后，直接访问页面异常处理
            if (CurrentSellerManager != null)
            {
                _shopid = CurrentSellerManager.ShopId;
                _userid = CurrentSellerManager.Id;
            }
        }
        public ActionResult ImportManage()
        {
            long lngCount = 0, lngTotal = 0;
            int intSuccess = 0;
            //从缓存取用户导入商品数量
            GetImportCountFromCache(out lngCount, out lngTotal);

            if (lngTotal == lngCount && lngTotal > 0)
            {
                intSuccess = 1;
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
                    Text = item.Name + "【" + item.ValuationMethod.ToDescription() + "】",
                    Value = item.Id.ToString()
                });
            }
            ViewBag.FreightTemplates = freightList;
            ViewBag.Count = lngCount;
            ViewBag.Total = lngTotal;
            ViewBag.Success = intSuccess;
            ViewBag.shopid = _shopid;
            ViewBag.userid = _userid;
            return View();
        }

        /// <summary>
        /// 取平台类目
        /// </summary>
        /// lly 2015-02-06
        /// <param name="key"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        [UnAuthorize]
        [HttpPost]
        public JsonResult GetPlatFormCategory(long? key = null, int? level = -1)
        {
            if (level == -1)
                key = 0;

            if (key.HasValue)
            {
                var categories = _iCategoryService.GetCategoryByParentId(key.Value);
                if (key == 0)
                {
                    var service = _iShopService;
                    var shopinfo = service.GetShop(CurrentSellerManager.ShopId) ?? new ShopInfo { };
                    if (!shopinfo.IsSelf)
                    {
                        var shopcategories = service.GetBusinessCategory(CurrentSellerManager.ShopId).Select(e => e.CategoryId);
                        categories = _iCategoryService.GetTopLevelCategories(shopcategories);
                    }
                }
                var cateoriesPair = categories.Select(item => new KeyValuePair<long, string>(item.Id, item.Name));
                return Json(cateoriesPair);
            }
            else
                return Json(new object[] { });
        }
        /// <summary>
        /// 取店铺品牌
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [UnAuthorize]
        [HttpPost]
        public JsonResult GetShopBrand(long categoryId)
        {
            var brands = _iBrandService.GetBrandsByCategoryIds(CurrentSellerManager.ShopId, categoryId);
            var brandsPair = brands.Select(item => new KeyValuePair<long, string>(item.Id, item.Name));
            return Json(brandsPair);
        }

        /// <summary>
        /// 取导入记录条数
        /// </summary>
        /// <returns></returns>
        public JsonResult GetImportCount()
        {
            long lngCount = 0, lngTotal = 0;
            int intSuccess = 0;
            //从缓存取用户导入商品数量
            GetImportCountFromCache(out lngCount, out lngTotal);

            if (lngTotal == lngCount && lngTotal > 0)
            {
                intSuccess = 1;
            }
            return Json(new
            {
                Count = lngCount,
                Total = lngTotal,
                Success = intSuccess
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 取正在进行导入的人数
        /// </summary>
        /// <returns></returns>
        public JsonResult GetImportOpCount()
        {
            long lngCount = 0;
            //
            object opcount = Core.Cache.Get(CacheKeyCollection.UserImportOpCount);
            if (opcount != null)
            {
                lngCount = string.IsNullOrEmpty(opcount.ToString()) ? 0 : long.Parse(opcount.ToString());
            }

            return Json(new
            {
                Count = lngCount
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 从缓存读取导入记录数
        /// </summary>
        /// <param name="count"></param>
        /// <param name="total"></param>
        private void GetImportCountFromCache(out long count, out long total)
        {
            object objCount = Core.Cache.Get(CacheKeyCollection.UserImportProductCount(_userid));
            object objTotal = Core.Cache.Get(CacheKeyCollection.UserImportProductTotal(_userid));
            count = objCount == null ? 0 : long.Parse(objCount.ToString());
            total = objTotal == null ? 0 : long.Parse(objTotal.ToString());
            if (count == total && total > 0)
            {
                Core.Cache.Remove(CacheKeyCollection.UserImportProductCount(_userid));
                Core.Cache.Remove(CacheKeyCollection.UserImportProductTotal(_userid));
            }
        }
    }

    public class AsyncProductImportController : BaseAsyncController
    {
        private ICategoryService _iCategoryService;
        private IProductService _iProductService;
        private ISearchProductService _iSearchProductService;
        public AsyncProductImportController(ICategoryService iCategoryService, IProductService iProductService,
            ISearchProductService iSearchProductService)
        {
            _iCategoryService = iCategoryService;
            _iProductService = iProductService;
            _iSearchProductService = iSearchProductService;
        }

        [UnAuthorize]
        [HttpGet]
        public JsonResult ImportProductJson(long paraCategory, long paraShopCategory, long? paraBrand, int paraSaleStatus, long _shopid, long _userid, long freightId, string file)
        {
            /*
             产品ID/主图
             产品ID/Details/明细图片
            */
            string filePath = Server.MapPath("/temp/" + file);
            string imgpath1 = string.Format(@"/Storage/Shop/{0}/Products", _shopid);
            string imgpath2 = Server.MapPath(imgpath1);
            long brand = 0;
            if (paraBrand.HasValue)
                brand = paraBrand.Value;
            JsonResult result = new JsonResult();
            if (System.IO.File.Exists(filePath))
            {
                ZipHelper.ZipInfo zipinfo = ZipHelper.UnZipFile(filePath);
                if (zipinfo.Success)
                {
                    try
                    {
                        int intCnt = ProcessProduct(paraCategory, paraShopCategory, brand, paraSaleStatus, _shopid, _userid, freightId, zipinfo.UnZipPath, imgpath1, imgpath2);
                        if (intCnt > 0)
                        {
                            result = Json(new { success = true, message = "成功导入【" + intCnt.ToString() + "】件商品" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            result = Json(new { success = false, message = "导入【0】件商品，请检查数据包，是否是重复导入" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Error("导入商品异常：" + ex.Message);
                        Core.Cache.Remove(CacheKeyCollection.UserImportProductCount(_userid));
                        Core.Cache.Remove(CacheKeyCollection.UserImportProductTotal(_userid));
                        result = Json(new { success = false, message = "导入商品异常:" + ex.Message }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    Core.Log.Error("解压文件异常：" + zipinfo.InfoMessage);
                    result = Json(new { success = false, message = "解压出现异常,请检查压缩文件格式" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                result = Json(new { success = false, message = "上传文件不存在" }, JsonRequestBehavior.AllowGet);
            }
            object opcount = Core.Cache.Get(CacheKeyCollection.UserImportOpCount);
            if (opcount != null)
            {
                Core.Cache.Insert(CacheKeyCollection.UserImportOpCount, int.Parse(opcount.ToString()) - 1);
            }
            return result;
        }
        /// <summary>
        /// 异步导入商品
        /// </summary>
        /// <param name="paraCategory"></param>
        /// <param name="paraShopCategory"></param>
        /// <param name="paraBrand"></param>
        /// <param name="paraSaleStatus"></param>
        /// <param name="_shopid"></param>
        /// <param name="_userid"></param>
        /// <param name="file">压缩文件名</param>
        /// <returns></returns>
        public void ImportProductAsync(long paraCategory, long paraShopCategory, long? paraBrand, int paraSaleStatus, long _shopid, long _userid, long freightId, string file)
        {
            /*
             产品ID/主图
             产品ID/Details/明细图片
            */
            AsyncManager.OutstandingOperations.Increment();
            Task.Factory.StartNew(() =>
            {
                string filePath = Server.MapPath("/temp/" + file);
                string imgpath1 = string.Format(@"/Storage/Shop/{0}/Products", _shopid);
                string imgpath2 = Server.MapPath(imgpath1);
                long brand = 0;
                if (paraBrand.HasValue)
                    brand = paraBrand.Value;
                JsonResult result = new JsonResult();
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        ZipHelper.ZipInfo zipinfo = ZipHelper.UnZipFile(filePath);
                        if (zipinfo.Success)
                        {

                            int intCnt = ProcessProduct(paraCategory, paraShopCategory, brand, paraSaleStatus, _shopid, _userid, freightId, zipinfo.UnZipPath, imgpath1, imgpath2);
                            if (intCnt > 0)
                            {
                                AsyncManager.Parameters["success"] = true;
                                AsyncManager.Parameters["message"] = "成功导入【" + intCnt.ToString() + "】件商品";
                            }
                            else
                            {
                                Core.Cache.Remove(CacheKeyCollection.UserImportProductCount(_userid));
                                Core.Cache.Remove(CacheKeyCollection.UserImportProductTotal(_userid));
                                AsyncManager.Parameters["success"] = false;
                                AsyncManager.Parameters["message"] = "导入【0】件商品，请检查数据包格式，或是否重复导入";
                            }

                        }
                        else
                        {
                            Core.Log.Error("解压文件异常：" + zipinfo.InfoMessage);
                            AsyncManager.Parameters["success"] = false;
                            AsyncManager.Parameters["message"] = "解压出现异常,请检查压缩文件格式";
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Error("导入商品异常：" + ex.Message);
                        Core.Cache.Remove(CacheKeyCollection.UserImportProductCount(_userid));
                        Core.Cache.Remove(CacheKeyCollection.UserImportProductTotal(_userid));
                        AsyncManager.Parameters["success"] = false;
                        AsyncManager.Parameters["message"] = "导入商品异常:" + ex.Message;
                    }
                }
                else
                {
                    AsyncManager.Parameters["success"] = false;
                    AsyncManager.Parameters["message"] = "上传文件不存在";
                }
                AsyncManager.OutstandingOperations.Decrement();
                object opcount = Core.Cache.Get(CacheKeyCollection.UserImportOpCount);
                if (opcount != null)
                {
                    Core.Cache.Insert(CacheKeyCollection.UserImportOpCount, int.Parse(opcount.ToString()) - 1);
                }
            });
        }
        public JsonResult ImportProductCompleted(bool success, string message)
        {
            return Json(new { success = success, message = message }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 商品明细处理
        /// </summary>
        /// <param name="paraCategory"></param>
        /// <param name="paraShopCategory"></param>
        /// <param name="paraBrand"></param>
        /// <param name="paraSaleStatus"></param>
        /// <param name="_shopid"></param>
        /// <param name="_userid"></param>
        /// <param name="mainpath">压缩文件的路径</param>
        /// <param name="imgpath1">虚拟相对路径</param>
        /// <param name="imgpath2">绝对路径(mappath)包含</param>
        /// <returns></returns>
        private int ProcessProduct(long paraCategory, long paraShopCategory, long paraBrand, int paraSaleStatus, long _shopid, long _userid, long freightId, string mainpath, string imgpath1, string imgpath2)
        {
            int result = 0;
            string strPath = mainpath;
            //var iProcudt = _iProductService;
            var category = _iCategoryService.GetCategory(paraCategory);

            if (Directory.Exists(strPath))
            {
                string[] csvfiles = Directory.GetFiles(strPath, "*.csv", SearchOption.AllDirectories);
                string line = string.Empty;
                List<string> cells = new List<string>();
                for (int i = 0; i < csvfiles.Length; i++)
                {
                    StreamReader reader = new StreamReader(csvfiles[i], Encoding.Unicode);
                    string str2 = reader.ReadToEnd();
                    reader.Close();
                    str2 = str2.Substring(str2.IndexOf('\n') + 1);
                    str2 = str2.Substring(str2.IndexOf('\n') + 1);
                    StreamWriter writer = new StreamWriter(csvfiles[i], false, Encoding.Unicode);
                    writer.Write(str2);
                    writer.Close();
                    using (CsvReader reader2 = new CsvReader(new StreamReader(csvfiles[i], Encoding.UTF8), true, '\t'))
                    {
                        int num = 0;
                        while (reader2.ReadNextRecord())
                        {
                            num++;
                            int columnCount = reader2.FieldCount;
                            //string[] heads = reader2.GetFieldHeaders();
                            string strProductName = reader2["宝贝名称"].Replace("\"", "");
                            ProductQuery productQuery = new ProductQuery();
                            productQuery.CategoryId = category.Id;
                            productQuery.ShopId = _shopid;
                            productQuery.KeyWords = strProductName;
                            var iProcudt = _iProductService;
                            ObsoletePageModel<ProductInfo> products = iProcudt.GetProducts(productQuery);
                            if (products.Total > 0)
                            {//当前店铺、分类已经存在相同编码的商品
                                result++;
                                Core.Log.Debug(strProductName + " : 商品不能重复导入");
                                Core.Cache.Insert(CacheKeyCollection.UserImportProductCount(_userid), result);
                                continue;
                            }
                            long pid = iProcudt.GetNextProductId();
                            long lngStock = 0;
                            decimal price = decimal.Parse(reader2["宝贝价格"] == string.Empty ? "0" : reader2["宝贝价格"]);
                            var p = new ProductInfo()
                            {
                                Id = pid,
                                TypeId = category.TypeId,
                                AddedDate = DateTime.Now,
                                BrandId = paraBrand,
                                CategoryId = category.Id,
                                CategoryPath = category.Path,
                                MarketPrice = price,
                                ShortDescription = string.Empty,
                                ProductCode = reader2["商家编码"].Replace("\"", ""),
                                ImagePath = "",
                                DisplaySequence = 1,
                                ProductName = strProductName,
                                MinSalePrice = price,
                                ShopId = _shopid,
                                HasSKU = false,//判断是否有多规格才能赋值
                                ProductAttributeInfo = new List<ProductAttributeInfo>(),
                                Himall_ProductShopCategories = paraShopCategory == 0 ? null : new List<ProductShopCategoryInfo>() { 
                                            new ProductShopCategoryInfo(){ ProductId=pid, ShopCategoryId=paraShopCategory}
                                        },
                                ProductDescriptionInfo = new ProductDescriptionInfo
                                {
                                    AuditReason = "",
                                    Description = reader2["宝贝描述"],//.Replace("\"", ""),//不能纯去除
                                    DescriptiondSuffixId = 0,
                                    DescriptionPrefixId = 0,
                                    Meta_Description = string.Empty,
                                    Meta_Keywords = string.Empty,
                                    Meta_Title = string.Empty,
                                    ProductId = pid
                                },
                                SKUInfo = new List<SKUInfo>() { new SKUInfo()
                                        { Id=string.Format("{0}_{1}_{2}_{3}" , pid , "0" , "0" , "0"), 
                                          Stock=long.TryParse(reader2["宝贝数量"],out lngStock)?lngStock:0,
                                          SalePrice=price,
                                          CostPrice=price
                                        }
                                        },
                                SaleStatus = paraSaleStatus == 1 ? ProductInfo.ProductSaleStatus.OnSale : ProductInfo.ProductSaleStatus.InStock,
                                AuditStatus = ProductInfo.ProductAuditStatus.WaitForAuditing,
                                FreightTemplateId = freightId
                            };
                            //图片处理
                            p.ImagePath = imgpath1 + "//" + p.Id.ToString();
                            if (reader2["新图片"] != string.Empty)
                            {
                                ImportProductImg(p.Id, _shopid, csvfiles[i], reader2["新图片"]);
                            }
                            iProcudt.AddProduct(p);
                            _iSearchProductService.AddSearchProduct(p.Id);
                            result++;
                            Core.Log.Debug(strProductName);
                            Core.Cache.Insert(CacheKeyCollection.UserImportProductCount(_userid), result);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 导入主图
        /// </summary>
        /// <param name="pid">产品ID</param>
        /// <param name="path">主图目录</param>
        /// <param name="filenames">主图文件信息</param>
        private void ImportProductImg(long pid, long _shopid, string path, string filenames)
        {
            //ff50d4ebbbe59def9faa2672d7538f44:1:0:|;1cd65d2a2d6b8c5bf1818151e5c982e6:1:1:|;54845d82ec3db3731fd63ebf5568b82d:2:0:1627207:107121|;
            path = path.Replace(Path.GetExtension(path), string.Empty);
            filenames = filenames.Replace("\"", string.Empty);
            string despath = string.Format(@"/Storage/Shop/{0}/Products/{1}", _shopid, pid);
            string[] arrFiles = new string[] { };
            string strDesfilename = string.Empty;
            int intDesfileCnt = 0;
            int intImgCnt = 0;
            filenames.Split(';').ToList().ForEach(item =>
            {
                if (item != string.Empty)
                {
                    string[] strArray = item.Split(':');
                    if (strArray.Length > 0)
                    {
                        arrFiles = Directory.GetFiles(path, strArray[0] + ".*", SearchOption.AllDirectories);

                        intImgCnt += 1;

                        try
                        {
                            string dest = string.Format("{0}\\{1}.png", despath, intImgCnt);

                            //读取文件流
                            FileStream fileStream = new FileStream(arrFiles[0], FileMode.Open, FileAccess.Read);

                            int byteLength = (int)fileStream.Length;
                            byte[] fileBytes = new byte[byteLength];
                            fileStream.Read(fileBytes, 0, byteLength);

                            //文件流关閉,文件解除锁定
                            fileStream.Close();

                            MemoryStream stream = new MemoryStream(fileBytes);

                            //using (Image image = Image.FromFile(arrFiles[0]))
                            //{
                            //    MemoryStream stream = new MemoryStream();

                            //    image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                            Core.HimallIO.CreateFile(dest, stream, FileCreateType.Create);
                            //}

                            var imageSizes = EnumHelper.ToDictionary<ImageSize>().Select(t => t.Key);
                            foreach (var imageSize in imageSizes)
                            {
                                string size = string.Format("{0}/{1}_{2}.png", despath, intImgCnt, imageSize);
                                Core.HimallIO.CreateThumbnail(dest, size, imageSize, imageSize);
                            }
                        }
                        catch (FileNotFoundException fex)
                        {
                            Core.Log.Error("导入商品处理图片时，没有找到文件", fex);
                        }
                        catch (System.Runtime.InteropServices.ExternalException eex)
                        {
                            Core.Log.Error("导入商品处理图片时，ExternalException异常", eex);
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Error("导入商品处理图片时，Exception异常", ex);
                        }
                        //IOHelper.CopyFile(source, Server.MapPath(dest), true);

                    }
                }
            });

        }
    }
}