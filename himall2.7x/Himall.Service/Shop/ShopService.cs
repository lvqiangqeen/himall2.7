using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.Entity;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using Himall.CommonModel;
using MySql.Data.MySqlClient;
using Dapper;

namespace Himall.Service
{
    public class ShopService : ServiceBase, IShopService
    {

        #region 实现店铺服务
        public ObsoletePageModel<ShopInfo> GetAuditingShops(ShopQuery shopQueryModel)
        {
            return null;
        }

        public ObsoletePageModel<ShopInfo> GetSellers(SellerQuery sellerQueryModel)
        {
            IQueryable<ShopInfo> shops = Context.ShopInfo;
            if (!string.IsNullOrEmpty(sellerQueryModel.ShopName))
                shops = shops.Where(item => item.ShopName.Contains(sellerQueryModel.ShopName));
            if (sellerQueryModel.RegionId.HasValue)
                shops = shops.Where(item => item.CompanyRegionId >= sellerQueryModel.RegionId.Value);
            if (sellerQueryModel.NextRegionId.HasValue)
                shops = shops.Where(item => item.CompanyRegionId < sellerQueryModel.NextRegionId.Value);
            if (sellerQueryModel.Ids != null && sellerQueryModel.Ids.Count() > 0)
                shops = shops.Where(item => sellerQueryModel.Ids.Contains(item.Id));
            if (sellerQueryModel.ShopId.HasValue)
                shops = shops.Where(item => item.Id != sellerQueryModel.ShopId.Value);
            int total = 0;
            total = shops.Count();
            shops = shops.FindBy(item => item.ShopStatus == ShopInfo.ShopAuditStatus.Open && item.Stage == ShopInfo.ShopStage.Finish, sellerQueryModel.PageNo, sellerQueryModel.PageSize, out total, item => item.Id, false);
            ObsoletePageModel<ShopInfo> pageModel = new ObsoletePageModel<ShopInfo>()
            {
                Models = shops,
                Total = total
            };
            return pageModel;
        }

		public QueryPageModel<ShopInfo> GetShops(ShopQuery shopQueryModel)
        {
            IQueryable<ShopInfo> shops = Context.ShopInfo.AsQueryable();
            shops = shops.Where(item => item.GradeId > 0);  //去除未完成信息
            if (shopQueryModel.ShopGradeId > 0)
            {
                shops = shops.Where(item => item.GradeId == (shopQueryModel.ShopGradeId.Value));
            }
            if (shopQueryModel.BrandId > 0)
            {
                shops = shops.Where(item => item.Himall_ShopBrands.Any(a => a.BrandId == shopQueryModel.BrandId));
            }
            if (shopQueryModel.CategoryId > 0)
            {
                shops = shops.Where(item => item.Himall_Products.Any(a => a.CategoryId == shopQueryModel.CategoryId));
            }
            if (!string.IsNullOrWhiteSpace(shopQueryModel.ShopName))
            {
                shops = shops.Where(item => item.ShopName.Contains(shopQueryModel.ShopName));
            }
            if (!string.IsNullOrWhiteSpace(shopQueryModel.ShopAccount))
            {
                shops = shops.Where(item => Context.ManagerInfo.Any(m => m.UserName.Equals(shopQueryModel.ShopAccount) && m.ShopId != 0 && m.RoleId == 0));
            }
            if (shopQueryModel.Status.HasValue)
            {
                if ((int)shopQueryModel.Status != 0)
                {
                    var _stwhere = shops.GetDefaultPredicate(false);
                    var _status = shopQueryModel.Status.Value;
                    var edv = DateTime.Now.Date.AddSeconds(-1);
                    switch (_status)
                    {
                        case ShopInfo.ShopAuditStatus.Open:
                            shops = shops.Where(d => d.ShopStatus == ShopInfo.ShopAuditStatus.Open && d.EndDate > edv);
                            break;
                        case ShopInfo.ShopAuditStatus.HasExpired:
                            shops = shops.Where(d => d.ShopStatus == ShopInfo.ShopAuditStatus.Open && d.EndDate < edv);
                            break;
                        default:
                            _stwhere = _stwhere.Or(d => d.ShopStatus == _status);
                            if (shopQueryModel.MoreStatus != null && shopQueryModel.MoreStatus.Count > 0)
                            {
                                foreach (var qsitem in shopQueryModel.MoreStatus)
                                {
                                    _stwhere = _stwhere.Or(d => d.ShopStatus == qsitem);
                                }
                            }
                            shops = shops.Where(_stwhere);
                            break;
                    }
                }
            }

            //TODO:ZJT 增加按销量排序
            Func<IQueryable<ShopInfo>, IOrderedQueryable<ShopInfo>> orderBy = null;
            if (shopQueryModel.OrderBy != 1)
            {
                orderBy = d => d.OrderByDescending(o => o.ShopStatus);
            }
            else
            {
                var orders = Context.OrderInfo;
                orderBy = d => d.OrderByDescending(o => orders.Where(p => p.ShopId == o.Id && p.OrderStatus == OrderInfo.OrderOperateStatus.Finish).Count());
            }

            int total;
            shops = shops.GetPage(out total, shopQueryModel.PageNo, shopQueryModel.PageSize, orderBy);
            foreach (var item in shops.ToList())
            {
                var manager = Context.ManagerInfo.FirstOrDefault(m => m.ShopId.Equals(item.Id));
                item.ShopAccount = manager == null ? "" : manager.UserName;
            }

			var pageModel = new QueryPageModel<ShopInfo>()
            {
                Models = shops.ToList(),
                Total = total
            };
            return pageModel;
        }


        /// <summary>
        /// 检测并初始店铺模板
        /// </summary>
        /// <param name="shopId"></param>
        public void CheckInitTemplate(long shopId)
        {
            CopyTemplate(shopId);
        }

        //TODO:ZJT 获取店铺有效的订单总销量
        public int GetSales(long id)
        {
            return Context.OrderInfo.Where(p => p.ShopId == id && p.OrderStatus == OrderInfo.OrderOperateStatus.Finish).Count();
        }

        public ShopInfo GetShop(long id, bool businessCategoryOn = false)
        {
            if (Cache.Exists(CacheKeyCollection.CACHE_SHOP(id, businessCategoryOn)))
                return Cache.Get<ShopInfo>(CacheKeyCollection.CACHE_SHOP(id, businessCategoryOn));

            var shop = Context.ShopInfo.FindById(id);
            if (null == shop)
                return null;
            var manage = Context.ManagerInfo.FirstOrDefault(m => m.ShopId.Equals(shop.Id));
            shop.ShopAccount = manage == null ? "" : manage.UserName;
            if (businessCategoryOn)
            {
                shop.BusinessCategory = new Dictionary<long, decimal>();
                var businessCategory = GetBusinessCategory(id);
                foreach (var item in businessCategory.ToList())
                {
                    if (!shop.BusinessCategory.ContainsKey(item.CategoryId))
                    {
                        shop.BusinessCategory.Add(item.CategoryId, item.CommisRate);
                    }
                }
            }
            Cache.Insert<ShopInfo>(CacheKeyCollection.CACHE_SHOP(id, businessCategoryOn), shop, 600);
            return shop;
        }

        /// <summary>
        /// 通过app_key获取店铺信息
        /// </summary>
        /// <param name="appkey"></param>
        /// <returns></returns>
        public ShopInfo GetShop(string appkey)
        {
            ShopInfo result = null;
            var app = Context.ShopOpenApiSettingInfo.SingleOrDefault(d => d.AppKey == appkey);
            if (app == null)
            {
                throw new HimallException("错误的appkey");
            }
            result = Context.ShopInfo.SingleOrDefault(d => d.Id == app.ShopId);
            return result;
        }

		/// <summary>
		/// 根据id获取门店
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public List<ShopInfo> GetShops(IEnumerable<long> ids)
		{
			return Context.ShopInfo.Where(p => ids.Contains(p.Id)).ToList();
		}

        public string GetShopName(long id)
        {
            return Context.ShopInfo.Where(p => p.Id == id).Select(p => p.ShopName).FirstOrDefault();
        }

		/// <summary>
		/// 获取商家名称
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public Dictionary<long,string> GetShopNames(IEnumerable<long> ids)
		{
			return Context.ShopInfo.Where(p => ids.Contains(p.Id)).Select(p => new { p.Id, p.ShopName }).ToDictionary(p => p.Id, p => p.ShopName);
		}

        public ShopInfo GetShopBasicInfo(long id)
        {
            var shop = Context.ShopInfo.FindById(id);
            return shop;
        }

        public void DeleteShop(long id)
        {
            Context.ShopInfo.Remove(Context.ShopInfo.FindById(id));
            Context.SaveChanges();
        }

        public void UpdateShop(ShopInfo shop)
        {
            //说明：
            //     店铺的更新只会影响如下字段
            //     1. ShopName     (店铺名称)
            //     2. ShopGradeId  (店铺等级)
            //     3. EndDate      (有效期)
            //     4. ShopStatus   (店铺状态)

            var actual = Context.ShopInfo.FindById(shop.Id);
            actual.ShopName = shop.ShopName ?? actual.ShopName;
            actual.GradeId = shop.GradeId == 0 ? actual.GradeId : shop.GradeId;
            actual.EndDate = shop.EndDate ?? actual.EndDate;
            actual.ShopStatus = shop.ShopStatus == 0 ? actual.ShopStatus : shop.ShopStatus;
            actual.Stage = shop.Stage ?? actual.Stage;
            actual.BankAccountName = shop.BankAccountName ?? actual.BankAccountName;
            actual.BankAccountNumber = shop.BankAccountNumber ?? actual.BankAccountNumber;
            actual.BankCode = shop.BankCode ?? actual.BankCode;
            actual.BankName = shop.BankName ?? actual.BankName;
            //    actual.BankPhoto = shop.BankPhoto ?? actual.BankPhoto; //开户银行许可证电子版
            actual.BankRegionId = shop.BankRegionId == 0 ? actual.BankRegionId : shop.BankRegionId;
            actual.CompanyRegionId = shop.CompanyRegionId == 0 ? actual.CompanyRegionId : shop.CompanyRegionId;
            actual.CompanyRegisteredCapital = shop.CompanyRegisteredCapital == 0 ? actual.CompanyRegisteredCapital : shop.CompanyRegisteredCapital;
            actual.BusinessLicenceEnd = shop.BusinessLicenceEnd ?? actual.BusinessLicenceEnd;
            actual.BusinessLicenceStart = shop.BusinessLicenceStart ?? actual.BusinessLicenceStart;
            actual.legalPerson = shop.legalPerson ?? actual.legalPerson;

            if (shop.CompanyFoundingDate.HasValue)
                actual.CompanyFoundingDate = shop.CompanyFoundingDate.HasValue ? shop.CompanyFoundingDate.Value : actual.CompanyFoundingDate.Value;


            actual.Logo = shop.Logo ?? actual.Logo;
            actual.SubDomains = shop.SubDomains ?? actual.SubDomains;
            actual.Theme = shop.Theme ?? actual.Theme;
            actual.BusinessLicenceRegionId = shop.BusinessLicenceRegionId == 0 ? actual.BusinessLicenceRegionId : shop.BusinessLicenceRegionId;
            actual.CompanyEmployeeCount = shop.CompanyEmployeeCount == 0 ? actual.CompanyEmployeeCount : shop.CompanyEmployeeCount;
            actual.BusinessLicenceNumber = shop.BusinessLicenceNumber ?? actual.BusinessLicenceNumber;
            // actual.BusinessLicenceNumberPhoto = shop.BusinessLicenceNumberPhoto ?? actual.BusinessLicenceNumberPhoto;//营业执照
            actual.BusinessSphere = shop.BusinessSphere ?? actual.BusinessSphere;
            actual.CompanyAddress = shop.CompanyAddress ?? actual.CompanyAddress;
            actual.CompanyName = shop.CompanyName ?? actual.CompanyName;
            actual.CompanyPhone = shop.CompanyPhone ?? actual.CompanyPhone;
            actual.CompanyRegionAddress = shop.CompanyRegionAddress ?? actual.CompanyRegionAddress;
            actual.ContactsEmail = shop.ContactsEmail ?? actual.ContactsEmail;
            actual.ContactsName = shop.ContactsName ?? actual.ContactsName;
            actual.ContactsPhone = shop.ContactsPhone ?? actual.ContactsPhone;
            actual.OrganizationCode = shop.OrganizationCode ?? actual.OrganizationCode;
            //  actual.OrganizationCodePhoto = shop.OrganizationCodePhoto ?? actual.OrganizationCodePhoto;//组织机构代码证
            // actual.GeneralTaxpayerPhot = shop.GeneralTaxpayerPhot ?? actual.GeneralTaxpayerPhot;//一般纳税人证明
            actual.TaxpayerId = shop.TaxpayerId ?? actual.TaxpayerId;
            actual.TaxRegistrationCertificate = shop.TaxRegistrationCertificate ?? actual.TaxRegistrationCertificate;
            //   actual.TaxRegistrationCertificatePhoto = shop.TaxRegistrationCertificatePhoto ?? actual.TaxRegistrationCertificatePhoto; 
            //actual.PayPhoto = shop.PayPhoto ?? actual.PayPhoto;
            actual.PayRemark = shop.PayRemark ?? actual.PayRemark;
            actual.SenderAddress = shop.SenderAddress ?? actual.SenderAddress;
            actual.SenderName = shop.SenderName ?? actual.SenderName;
            actual.SenderPhone = shop.SenderPhone ?? actual.SenderPhone;


            //个人认证
            actual.BusinessType = shop.BusinessType ?? actual.BusinessType;
            actual.IDCard = shop.IDCard ?? actual.IDCard;
            actual.WeiXinNickName = shop.WeiXinNickName ?? actual.WeiXinNickName;
            actual.WeiXinSex = shop.WeiXinSex ?? actual.WeiXinSex;
            actual.WeiXinAddress = shop.WeiXinAddress ?? actual.WeiXinAddress;
            actual.WeiXinTrueName = shop.WeiXinTrueName ?? actual.WeiXinTrueName;
            actual.WeiXinOpenId = shop.WeiXinOpenId ?? actual.WeiXinOpenId;
            actual.WeiXinImg = shop.WeiXinImg ?? shop.WeiXinImg;
            actual.AutoAllotOrder = shop.AutoAllotOrder;//商家是否开启订单自动分配门店

            //TODO:DZY[160225]  防联动修改提前保存值
            var BusinessLicenseCert = shop.BusinessLicenseCert;
            var ProductCert = shop.ProductCert;
            var OtherCert = shop.OtherCert;

            //TODO:DZY[160225]  此处如果传进来的shop不是一个new对象，而是从ef获取数据就会形成联动修改。

            if (!string.IsNullOrEmpty(shop.BusinessLicenseCert))
                actual.BusinessLicenseCert = "/Storage/Shop/" + shop.Id + "/Cert/BusinessLicenseCert";//经营许可类证书
            if (!string.IsNullOrEmpty(shop.ProductCert))
                actual.ProductCert = "/Storage/Shop/" + shop.Id + "/Cert/ProductCert";//产品类证书
            if (!string.IsNullOrEmpty(shop.OtherCert))
                actual.OtherCert = "/Storage/Shop/" + shop.Id + "/Cert/OtherCert";//其它证书
            if (!string.IsNullOrEmpty(shop.TaxRegistrationCertificatePhoto))
				actual.TaxRegistrationCertificatePhoto = MoveImages(shop.Id, shop.TaxRegistrationCertificatePhoto, "TaxRegistrationCertificatePhoto", 1); //税务登记证
            if (!string.IsNullOrEmpty(shop.BusinessLicenceNumberPhoto))
				actual.BusinessLicenceNumberPhoto = MoveImages(shop.Id, shop.BusinessLicenceNumberPhoto, "BusinessLicenceNumberPhoto", 1);//营业执照
            if (!string.IsNullOrEmpty(shop.OrganizationCodePhoto))
				actual.OrganizationCodePhoto = MoveImages(shop.Id, shop.OrganizationCodePhoto, "OrganizationCodePhoto", 1);//组织机构代码证
            if (!string.IsNullOrEmpty(shop.GeneralTaxpayerPhot))
				actual.GeneralTaxpayerPhot = MoveImages(shop.Id, shop.GeneralTaxpayerPhot, "GeneralTaxpayerPhoto", 1); //一般纳税人证明
            if (!string.IsNullOrEmpty(shop.BankPhoto))
				actual.BankPhoto = MoveImages(shop.Id, shop.BankPhoto, "BankPhoto", 1); //一般纳税人证明
            if (!string.IsNullOrEmpty(shop.PayPhoto))
				actual.PayPhoto = MoveImages(shop.Id, shop.PayPhoto, "PayPhoto", 1); //付款凭证

            if (!string.IsNullOrEmpty(shop.IDCardUrl))
				actual.IDCardUrl = MoveImages(shop.Id, shop.IDCardUrl, "IDCardUrl", 1);
            if (!string.IsNullOrEmpty(shop.IDCardUrl2))
				actual.IDCardUrl2 = MoveImages(shop.Id, shop.IDCardUrl2, "IDCardUrl", 2);


            actual.Lat = shop.Lat;
            actual.Lng = shop.Lng;
            actual.OpeningTime = shop.OpeningTime ?? shop.OpeningTime;
            actual.ShopDescription = shop.ShopDescription ?? shop.ShopDescription;
            actual.WelcomeTitle = shop.WelcomeTitle ?? shop.WelcomeTitle;
            actual.Industry = shop.Industry ?? shop.Industry;
            actual.BranchImage = shop.BranchImage ?? shop.BranchImage;
            actual.ContactsPosition = shop.ContactsPosition ?? shop.ContactsPosition;

            Context.SaveChanges();
            //过期店铺下架所有商品
            if (actual.EndDate < DateTime.Now.AddDays(1).Date)
            {
                SaleOffAllProduct(actual.Id);
                CloseAllMarketingAction(actual.Id);
            }

            if (!string.IsNullOrEmpty(BusinessLicenseCert))
            {
                var arr = BusinessLicenseCert.Split(',');
                var index = 0;
                foreach (var i in arr)
                {
                    index++;
                    MoveImages(shop.Id, i, "BusinessLicenseCert", index);
                }

            }
            if (!string.IsNullOrEmpty(ProductCert))
            {
                var arr = ProductCert.Split(',');
                var index = 0;
                foreach (var i in arr)
                {
                    index++;
                    MoveImages(shop.Id, i, "ProductCert", index);
                }
            }
            if (!string.IsNullOrEmpty(OtherCert))
            {
                var arr = OtherCert.Split(',');
                var index = 0;
                foreach (var i in arr)
                {
                    index++;
                    MoveImages(shop.Id, i, "OtherCert", index);
                }
            }

            var msg = ServiceProvider.Instance<IMessageService>.Create;
            var email = PluginsManagement.GetPlugins<IEmailPlugin>().FirstOrDefault();
            if (!String.IsNullOrWhiteSpace(actual.ContactsEmail))
            {
                msg.UpdateMemberContacts(new MemberContactsInfo()
                {
                    Contact = actual.ContactsEmail,
                    ServiceProvider = email.PluginInfo.PluginId,
                    UserId = actual.Id,
                    UserType = MemberContactsInfo.UserTypes.ShopManager
                });
            }

            var sms = PluginsManagement.GetPlugins<ISMSPlugin>().FirstOrDefault();
            if (sms != null && !String.IsNullOrWhiteSpace(actual.ContactsPhone))
            {
                msg.UpdateMemberContacts(new MemberContactsInfo()
                {
                    Contact = actual.ContactsPhone,
                    ServiceProvider = sms.PluginInfo.PluginId,
                    UserId = actual.Id,
                    UserType = MemberContactsInfo.UserTypes.ShopManager
                });
            }
            //更新操作要处理相关缓存
            Cache.Remove(CacheKeyCollection.CACHE_SHOP(shop.Id, true));
            Cache.Remove(CacheKeyCollection.CACHE_SHOP(shop.Id, false));
            Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(shop.Id, true));
            Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(shop.Id, false));
        }

        string MoveImages(long shopId, string image, string name, int index = 1)
        {
            if (string.IsNullOrEmpty(image))
            {
                return "";
            }
            string OriUrl = image;
            var ext = ".png";
            string ImageDir = string.Empty;

            var path = "/Storage/Shop/" + shopId + "/Cert/";
            //转移图片

            var fileName = name + index + ext;

            if (image.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                var img = image.Substring(image.LastIndexOf("/temp"));
                Core.HimallIO.CopyFile(img, path + fileName, true);
            }  //目标地址
            return path + fileName;
        }




        public void UpdateShop(ShopInfo shop, IEnumerable<long> categoryIds)
        {
            categoryIds = categoryIds.Distinct();
            categoryIds = GetThirdLevelCategories(categoryIds);//获取所有三级分类，分佣精确到三级分类
            using (TransactionScope scope = new TransactionScope())
            {
                shop.ShopStatus = ShopInfo.ShopAuditStatus.WaitAudit;
                UpdateShop(shop);
                UpdateBusinessCategory(shop.Id, categoryIds);
                scope.Complete();
            }
            Cache.Remove(CacheKeyCollection.CACHE_SHOP(shop.Id, true));
            Cache.Remove(CacheKeyCollection.CACHE_SHOP(shop.Id, false));
            Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(shop.Id, true));
            Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(shop.Id, false));
        }

        IEnumerable<long> GetThirdLevelCategories(IEnumerable<long> categoryIds)
        {
            var categoryService = ServiceProvider.Instance<ICategoryService>.Create;
            var allCateogries = categoryService.GetCategories().ToArray();
            List<long> validCategoryIds = new List<long>();
            foreach (var categoryId in categoryIds.ToList())
            {
                var category = allCateogries.FirstOrDefault(item => item.Id == categoryId);
                if (category.Depth == 1)
                {
                    var secondCategoryIds = allCateogries.Where(item => item.ParentCategoryId == category.Id).Select(item => item.Id);
                    validCategoryIds.AddRange(allCateogries.Where(item => secondCategoryIds.Contains(item.ParentCategoryId)).Select(item => item.Id));
                }
                else if (category.Depth == 2)
                    validCategoryIds.AddRange(allCateogries.Where(item => item.ParentCategoryId == category.Id).Select(item => item.Id));
                else
                    validCategoryIds.Add(categoryId);
            }
            return validCategoryIds;
        }

        public long AddShop(ShopInfo shop)
        {
            Context.ShopInfo.Add(shop);
            Context.SaveChanges();
            return shop.Id;
        }

        private string GetCategoryNameByPath(long id)
        {
            var c = Context.CategoryInfo.FindById(id);
            if (c.Depth == 1 && c.ParentCategoryId == 0)
                return c.Name;
            return GetCategoryNameByPath(c.ParentCategoryId) + " > " + c.Name;
        }


        public IQueryable<BusinessCategoryInfo> GetBusinessCategory(long id)
        {
            var bcategory = Context.BusinessCategoryInfo.FindBy(b => b.ShopId.Equals(id));
            foreach (var item in bcategory.ToList())
            {
                item.CategoryName = GetCategoryNameByPath(item.CategoryId);
            }
            return bcategory;
        }

        public void UpdateBusinessCategory(long shopId, IEnumerable<long> categoryIds)
        {
            var bcategory = Context.BusinessCategoryInfo.FindBy(b => b.ShopId.Equals(shopId));
            Context.BusinessCategoryInfo.RemoveRange(bcategory);
            Context.SaveChanges();
            categoryIds = categoryIds.Distinct();
            var categories = Context.CategoryInfo.FindBy(item => categoryIds.Contains(item.Id)).ToArray();
            var businessCategoies = categoryIds.Select(item =>
            {
                var category = categories.FirstOrDefault(t => t.Id == item);
                return new BusinessCategoryInfo()
                {
                    CategoryId = item,
                    ShopId = shopId,
                    CommisRate = category.CommisRate
                };
            });
            Context.BusinessCategoryInfo.AddRange(businessCategoies);
            Context.SaveChanges();
        }

        /// <summary>
        /// 是否可以删除经营类目
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="bCategoryId"></param>
        /// <returns></returns>
        public bool CanDeleteBusinessCategory(long shopId, long bCategoryId)
        {
            bool result = true;
            var bcategory = Context.BusinessCategoryInfo.FirstOrDefault(b => b.ShopId.Equals(shopId) && b.CategoryId == bCategoryId);
            if (bcategory != null)
            {
                result = !(Context.OrderItemInfo.Any(d => d.ShopId == shopId && Context.ProductInfo.Where(p => p.CategoryId == bCategoryId).Select(p => p.Id).Contains(d.ProductId)));
            }
            return result;
        }


        public void SaveBusinessCategory(long id, Dictionary<long, decimal> bCategoryList)
        {
            var bcategory = Context.BusinessCategoryInfo.FindBy(b => b.ShopId.Equals(id));
            foreach (var item in bcategory.ToList())
            {
                Context.BusinessCategoryInfo.Remove(item);
            }

            foreach (var item in bCategoryList)
            {
                Context.BusinessCategoryInfo.Add(new BusinessCategoryInfo
                {
                    CategoryId = item.Key,
                    CommisRate = item.Value,
                    ShopId = id
                });
            }
            Context.SaveChanges();
        }

        private void CopyFolder(string from, string to)
        {
            if (!Directory.Exists(to))
                Directory.CreateDirectory(to);

            // 子文件夹
            foreach (string sub in Directory.GetDirectories(from))
                CopyFolder(sub + "\\", to + Path.GetFileName(sub) + "\\");

            // 文件
            foreach (string file in Directory.GetFiles(from))
                File.Copy(file, to + Path.GetFileName(file), true);
        }

        private void CopyTemplate(long shopId)
        {
            string templatePath = HttpContext.Current.Server.MapPath(string.Format("/Areas/SellerAdmin/Templates/vshop/{0}/", shopId));

            bool isExist = Directory.Exists(templatePath);
            if (!isExist)
            {
                Directory.CreateDirectory(templatePath);
                CopyFolder(HttpContext.Current.Server.MapPath("/Template"), templatePath);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="status"></param>
        /// <param name="comments"></param>
        public void UpdateShopStatus(long shopId, ShopInfo.ShopAuditStatus status, string comments = "", int TrialDays = 0)
        {
            var shop = Context.ShopInfo.FindById(shopId);
            shop.ShopStatus = status;
            if (!string.IsNullOrWhiteSpace(comments))
                shop.RefuseReason = comments;
            //官方自营永不过期
            if (shop.IsSelf) status = ShopInfo.ShopAuditStatus.Open;

            //设置店铺的结束日期
            if (status == ShopInfo.ShopAuditStatus.Open)
            {
                //TODO:DZY[150729] 官方自营店到期自动延期
                /* zjt  
                 * TODO可移除，保留注释即可
                 */
                if (shop.IsSelf)
                {
                    shop.ShopStatus = ShopInfo.ShopAuditStatus.Open; //开启官方自营店
                    shop.EndDate = DateTime.Now.AddYears(10);
                }
                else
                {
                    shop.EndDate = DateTime.Now.AddDays(TrialDays);
                }

                //账户初始
                if (!Context.ShopAccountInfo.Any(a => a.ShopId == shopId))
                {
                    ShopAccountInfo model = new ShopAccountInfo()
                    {
                        ShopId = shopId,
                        ShopName = shop.ShopName,
                        Settled = 0,
                        PendingSettlement = 0,
                        Balance = 0
                    };
                    Context.ShopAccountInfo.Add(model);
                    Context.SaveChanges();
                }

                //发送通知消息
                var shopMessage = new MessageShopInfo();
                shop.Stage = ShopInfo.ShopStage.Finish;
                shopMessage.ShopId = shopId;
                shopMessage.ShopName = shop.ShopName;
                shopMessage.SiteName = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().SiteName;
                long uid = this.GetShopManagers(shopId);
                var member = Context.UserMemberInfo.FindById(uid);
                shopMessage.UserName = member.UserName;
                Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnShopAudited(uid, shopMessage));
                CopyTemplate(shopId);
            }
            if (status == ShopInfo.ShopAuditStatus.WaitPay)
            {
                //发送通知消息
                //var shopMessage = new MessageShopInfo();
                //shopMessage.ShopId = shopId;
                //shopMessage.ShopName = shop.ShopName;
                //shopMessage.SiteName = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().SiteName;
                //Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnShopSuccess(shopId, shopMessage));
            }
            if (status == ShopInfo.ShopAuditStatus.Refuse)
            {
                shop.Stage = ShopInfo.ShopStage.CompanyInfo;
                //shop.ShopName = "";
            }
            shop.CreateDate = DateTime.Now;
            Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_SHOP(shopId, true));
            Cache.Remove(CacheKeyCollection.CACHE_SHOP(shopId, false));
            Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(shopId, true));
            Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(shopId, false));
        }

        public void UpdateShopFreight(long shopId, decimal freight, decimal freeFreight)
        {
            var shop = Context.ShopInfo.FindById(shopId);
            shop.Freight = freight;
            shop.FreeFreight = freeFreight;
            Context.SaveChanges();
        }

        public PlatConsoleModel GetPlatConsoleMode()
        {
            var shopinfo = Context.ShopInfo;
            var orders = Context.OrderInfo;
            var products = Context.ProductInfo.Where(item => item.IsDeleted == false);
            var orderRefund = Context.OrderRefundInfo;
            var comment = Context.ProductCommentInfo;
            var consultations = Context.ProductConsultationInfo;
            var complaints = Context.OrderComplaintInfo;
            var member = Context.UserMemberInfo;
            PlatConsoleModel model = new PlatConsoleModel();
            var Today = DateTime.Now.Date;
            var Yesterday = DateTime.Now.Date.AddDays(-1);
            var brandAuditStatus = (int)ShopBrandApplysInfo.BrandAuditStatus.UnAudit;
            model.TodaySaleAmount = orders.Where(a => (a.OrderStatus != OrderInfo.OrderOperateStatus.Close && a.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay) && a.PayDate >= Today).Sum(a => (decimal?)(a.ProductTotalAmount + a.Freight + a.Tax - a.DiscountAmount)).GetValueOrDefault();
            model.TodayMemberIncrease = member.Where(a => a.CreateDate >= Today).Count();
            model.TodayShopIncrease = shopinfo.Where(a => a.CreateDate >= Today && a.ShopStatus == ShopInfo.ShopAuditStatus.Open && a.Stage == ShopInfo.ShopStage.Finish).Count();
            model.YesterdayShopIncrease = shopinfo.Where(a => a.CreateDate >= Yesterday && a.CreateDate < Today && a.ShopStatus == ShopInfo.ShopAuditStatus.Open && a.Stage == ShopInfo.ShopStage.Finish).Count();
            model.WaitAuditShops = shopinfo.Where(a => a.GradeId > 0 && (a.ShopStatus == ShopInfo.ShopAuditStatus.WaitAudit)).Count();
            //model.WaitConfirmShops = shopinfo.Where(a => a.GradeId > 0 && (a.ShopStatus == ShopInfo.ShopAuditStatus.WaitConfirm)).Count();
            model.ExpiredShops = shopinfo.Where(a => a.EndDate < Today).Count();
            model.ShopNum = shopinfo.Count(a => a.ShopStatus == ShopInfo.ShopAuditStatus.Open && a.Stage == ShopInfo.ShopStage.Finish);
            model.WaitForAuditingBrands = Context.ShopBrandApplysInfo.Count(a => a.AuditStatus == brandAuditStatus);
            model.ProductNum = products.Count();
            model.OnSaleProducts = products.Where(a => a.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && a.AuditStatus == ProductInfo.ProductAuditStatus.Audited).Count();
            model.WaitForAuditingProducts = products.Where(a => a.AuditStatus == ProductInfo.ProductAuditStatus.WaitForAuditing && a.SaleStatus == ProductInfo.ProductSaleStatus.OnSale).Count();
            model.ProductComments = comment.Count();
            model.ProductConsultations = consultations.Count();
            model.WaitPayTrades = orders.Where(a => a.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay).Count();
            model.RefundTrades = orderRefund.Where(a => a.RefundMode != OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund && a.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited && a.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm).Count();
            model.OrderWithRefundAndRGoods = orderRefund.Where(a => a.RefundMode == OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund && (a.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited && a.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm)).Count();

            var _ordswhere = orders.GetDefaultPredicate(true);
            _ordswhere = _ordswhere.And(d => d.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery);
            var fgordids = Context.FightGroupOrderInfo.Where(d => d.JoinStatus != 4).Select(d => d.OrderId);
            _ordswhere = _ordswhere.And(d => !fgordids.Contains(d.Id));
            model.WaitDeliveryTrades = orders.Where(_ordswhere).Count();

            model.Complaints = complaints.Count(a => a.Status == OrderComplaintInfo.ComplaintStatus.Dispute);
            model.OrderCounts = orders.Where(a => a.OrderStatus == OrderInfo.OrderOperateStatus.Finish).Count();
            model.Cash = Context.ApplyWithDrawInfo.Where(a => a.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.WaitConfirm).Count();
            model.GiftSend = Context.GiftOrderInfo.Where(a => a.OrderStatus == GiftOrderInfo.GiftOrderStatus.WaitDelivery).Count();
            model.ShopCashNumber = Context.ShopWithDrawInfo.Where(a => a.Status == CommonModel.WithdrawStaus.WatingAudit).Count();
            return model;
        }

        public SellerConsoleModel GetSellerConsoleModel(long shopId)
        {
            var shopinfo = Context.ShopInfo.Where(a => a.Id == shopId).FirstOrDefault();
            var shopG = Context.ShopGradeInfo.FindById(shopinfo.GradeId);
            var orders = Context.OrderInfo.Where(a => a.ShopId == shopId);
            var products = Context.ProductInfo.Where(a => a.ShopId == shopId && a.IsDeleted == false);
            var brands = Context.ShopBrandApplysInfo.Where(a => a.ShopId == shopId);
            var orderRefund = Context.OrderRefundInfo.Where(a => a.ShopId == shopId && a.RefundMode != OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund && (a.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit || a.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving));
            var orderRefundAndRGoods = Context.OrderRefundInfo.Where(a => a.ShopId == shopId && a.RefundMode == OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund && (a.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit || a.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving));
            var comment = Context.ProductCommentInfo.Where(a => a.ShopId == shopId);
            var consultations = Context.ProductConsultationInfo.Where(a => a.ShopId == shopId);
            var complaints = Context.OrderComplaintInfo.Where(a => a.ShopId == shopId && a.Status == OrderComplaintInfo.ComplaintStatus.WaitDeal);
            SellerConsoleModel model = new SellerConsoleModel();
            model.ShopName = shopinfo.ShopName;
            model.ShopGrade = shopG.Name;
            model.ShopEndDate = shopinfo.EndDate.Value;
            model.ShopFreight = shopinfo.Freight;
            model.ProductsCount = products.Count();
            model.OnSaleProducts = products.Where(a => a.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && a.AuditStatus == ProductInfo.ProductAuditStatus.Audited).Count();
            model.AuditFailureProducts = products.Where(a => a.AuditStatus == ProductInfo.ProductAuditStatus.AuditFailed).Count();
            model.InfractionSaleOffProducts = products.Where(a => a.AuditStatus == ProductInfo.ProductAuditStatus.InfractionSaleOff).Count();
            model.InStockProducts = products.Where(a => a.SaleStatus == ProductInfo.ProductSaleStatus.InStock).Count();
            model.WaitForAuditingProducts = products.Where(a => a.AuditStatus == ProductInfo.ProductAuditStatus.WaitForAuditing && a.SaleStatus == ProductInfo.ProductSaleStatus.OnSale).Count();
            model.BrandApply = brands.Where(a => a.AuditStatus == (int)ShopBrandApplysInfo.BrandAuditStatus.UnAudit).Count();
            model.WaitPayTrades = orders.Where(a => a.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay).Count();
            model.ProductComments = comment.Count();
            model.ProductConsultations = consultations.Count();
            model.ProductLimit = shopG.ProductLimit;
            var SpaceUsage = GetShopSpaceUsage2(shopId);
            model.ProductsCount = products.Count();
            model.ImageLimit = shopG.ImageLimit;
            model.ProductImages = SpaceUsage;
            //拼团
            //var fgordids = Context.FightGroupOrderInfo.Where(d => d.JoinStatus != 4).Select(d => d.OrderId);
            //model.WaitDeliveryTrades = orders.Where(a => a.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery && !fgordids.Contains(a.Id)).Count();
            var _ordswhere = orders.GetDefaultPredicate(true);
            _ordswhere = _ordswhere.And(d => d.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery);
            var fgordids = Context.FightGroupOrderInfo.Where(d => d.JoinStatus != 4).Select(d => d.OrderId);
            _ordswhere = _ordswhere.And(d => !fgordids.Contains(d.Id));
            model.WaitDeliveryTrades = orders.Where(_ordswhere).Count();
            
            model.RefundTrades = orderRefund.Count();
            model.RefundAndRGoodsTrades = orderRefundAndRGoods.Count();
            model.Complaints = complaints.Count();
            model.OrderCounts = orders.Count();
            return model;
        }

        public bool IsExpiredShop(long shopId)
        {
            var date = DateTime.Now.Date;
            var shopinfo = Context.ShopInfo.FindById(shopId);
            if (shopinfo.EndDate <= date)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 是否冻结
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public bool IsFreezeShop(long shopId)
        {
            bool result = true;
            var shopinfo = Context.ShopInfo.FindById(shopId);
            if (shopinfo != null)
            {
                if (shopinfo.ShopStatus != ShopInfo.ShopAuditStatus.Freeze)
                {
                    result = false;
                }
            }
            return result;
        }
        /// <summary>
        /// 是否官方自营店
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public bool IsSelfShop(long shopId)
        {
            bool result = false;
            var shopinfo = Context.ShopInfo.FindById(shopId);
            if (shopinfo != null)
            {
                result = shopinfo.IsSelf;
            }

            return result;
        }
        #endregion

        #region 实现店铺等级服务

        public IQueryable<ShopGradeInfo> GetShopGrades()
        {
            return Context.ShopGradeInfo.FindAll();
        }

        public ShopGradeInfo GetShopGrade(long id)
        {
            return Context.ShopGradeInfo.FindById(id);
        }

        public void AddShopGrade(ShopGradeInfo shopGrade)
        {
            Context.ShopGradeInfo.Add(shopGrade);
            Context.SaveChanges();
        }

        public void DeleteShopGrade(long id, out string msg)
        {
            msg = "";
            if (Context.ShopInfo.Any(s => s.GradeId == id))
            {
                msg = "删除失败，因为该套餐和店铺有关联，所以不能删除.";
                return;
            }
            Context.ShopGradeInfo.Remove(Context.ShopGradeInfo.FindById(id));
            Context.SaveChanges();
        }

        public void UpdateShopGrade(ShopGradeInfo shopGrade)
        {
            var actual = Context.ShopGradeInfo.FindById(shopGrade.Id);
            actual.Name = shopGrade.Name;
            actual.ProductLimit = shopGrade.ProductLimit;
            actual.ImageLimit = shopGrade.ImageLimit;
            actual.ChargeStandard = shopGrade.ChargeStandard;

            Context.SaveChanges();
        }
        #endregion

        #region 店铺信息
        public long GetShopSpaceUsage(long shopId)
        {
            var path = string.Format("/Storage/Shop/{0}/Products/", shopId);
            if (!HimallIO.ExistDir(path))
            {
                HimallIO.CreateDir(path);
            }
            var useage = Core.HimallIO.GetDirMetaInfo(path).ContentLength / 1024 / 1024;
            var shop = Context.ShopInfo.Where(a => a.Id == shopId).FirstOrDefault();
            var limit = Context.ShopGradeInfo.FindById(shop.GradeId).ImageLimit;
            return useage > limit ? -1 : useage;
        }

        public long GetShopSpaceUsage2(long shopId)
        {
            var path = string.Format("/Storage/Shop/{0}/Products/", shopId);
            if (!Core.HimallIO.ExistDir(path))
            {
                Core.HimallIO.CreateDir(path);
            }
            var useage = Core.HimallIO.GetDirMetaInfo(path).ContentLength / 1024 / 1024;
            return useage;
        }


        public long GetShopConcernedCount(long shopId)
        {
            long ShopConcernedCount = 0;
            string CACHE_MANAGER_KEY = CacheKeyCollection.ShopConcerned(shopId);

            if (Cache.Exists(CACHE_MANAGER_KEY))
            {
                ShopConcernedCount = (long)Core.Cache.Get(CACHE_MANAGER_KEY);
            }
            else
            {
                ShopConcernedCount = Context.FavoriteShopInfo.Where(a => a.ShopId == shopId).Count();
                Cache.Insert(CACHE_MANAGER_KEY, ShopConcernedCount, DateTime.Now.AddMinutes(5));
            }
            return ShopConcernedCount;
        }

		/// <summary>
		/// 获取店铺账户信息
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public List<ShopAccountInfo> GetShopAccounts(IEnumerable<long> ids)
		{
			return this.Context.ShopAccountInfo.Where(p => ids.Contains(p.ShopId)).ToList();
		}

        public ObsoletePageModel<FavoriteShopInfo> GetUserConcernShops(long userId, int pageNo, int pageSize)
        {
            int total = 0;
            var favorite = Context.FavoriteShopInfo.FindBy(a => a.UserId == userId, pageNo, pageSize, out total, a => a.Id, false);
            ObsoletePageModel<FavoriteShopInfo> pageModel = new ObsoletePageModel<FavoriteShopInfo>()
            {
                Models = favorite,
                Total = total
            };
            return pageModel;
        }

        public void CancelConcernShops(IEnumerable<long> ids, long userId)
        {
            Context.FavoriteShopInfo.Remove(a => a.UserId == userId && ids.Contains(a.Id));
            Context.SaveChanges();
        }

        public void CancelConcernShops(long shopId, long userId)
        {
            Context.FavoriteShopInfo.Remove(a => a.UserId == userId && a.ShopId == shopId);
            Context.SaveChanges();
        }

        public void LogShopVisti(long shopId)
        {
            //TODO:取消实时统计
            //var date = DateTime.Now;
            //var shop = Context.ShopVistiInfo.FirstOrDefault(s => s.ShopId.Equals(shopId)
            //    && s.Date.Year.Equals(date.Year) && s.Date.Month.Equals(date.Month) && s.Date.Day.Equals(date.Day));
            //if (null != shop && shop.ShopId.Equals(shopId))
            //{
            //    shop.VistiCounts += 1;
            //}
            //else
            //{
            //    Context.ShopVistiInfo.Add(new ShopVistiInfo
            //    {
            //        ShopId = shopId,
            //        Date = DateTime.Now,
            //        VistiCounts = 1,
            //        SaleAmounts = 0,
            //        SaleCounts = 0
            //    });
            //}
            //Context.SaveChanges();
        }


        public ShopInfo CreateEmptyShop()
        {
            ShopInfo shopInfo = new ShopInfo()
            {
                ShopName = "",
                GradeId = 0,
                IsSelf = false,
                ShopStatus = ShopInfo.ShopAuditStatus.Unusable,
                CreateDate = DateTime.Now,
                CompanyRegionId = 0,
                CompanyEmployeeCount = CompanyEmployeeCount.LessThanFive,
                CompanyRegisteredCapital = 0,
                BusinessLicenceNumberPhoto = "",
                BusinessLicenceRegionId = 0,
                BankRegionId = 0,
                FreeFreight = 0,
                Freight = 0,
                Stage = ShopInfo.ShopStage.CompanyInfo
            };
            Context.ShopInfo.Add(shopInfo);
            Context.SaveChanges();
            return shopInfo;
        }


        public void AddFavoriteShop(long memberId, long shopId)
        {
            var exist = Context.FavoriteShopInfo.FirstOrDefault(item => item.UserId == memberId && item.ShopId == shopId);
            if (exist == null)
            {
                FavoriteShopInfo favoriteShopInfo = new FavoriteShopInfo()
                {
                    ShopId = shopId,
                    UserId = memberId,
                    Date = DateTime.Now,
                    Tags = ""
                };
                Context.FavoriteShopInfo.Add(favoriteShopInfo);
                Context.SaveChanges();
            }
            else
            {
                throw new HimallException("您已经关注过该店铺");
            }
        }

        public bool IsFavoriteShop(long memberId, long shopId)
        {
            bool isFav = false;
            if (memberId <= 0)
                throw new Himall.Core.HimallException("用户ID不存在！");
            var fav = Context.FavoriteShopInfo.FindBy(f => f.ShopId.Equals(shopId) && f.UserId.Equals(memberId)).Count();
            if (fav >= 1)
                isFav = true;
            return isFav;
        }

        public IQueryable<FavoriteShopInfo> GetFavoriteShopInfos(long memberId)
        {
            var favouriteShopInfos = Context.FavoriteShopInfo.FindBy(item => item.UserId == memberId);
            return favouriteShopInfos;
        }


        public void SaveBusinessCategory(long id, decimal commisRate)
        {
            if (commisRate > 100)
                throw new InvalidPropertyException("分佣比例不能大于100");
            else if (commisRate < 0)
                throw new InvalidPropertyException("分佣比例不能小于0");

            var businessCategory = Context.BusinessCategoryInfo.FirstOrDefault(item => item.Id == id);
            if (businessCategory == null)
                throw new HimallException("未找到" + id + "对应的经营类目");
            businessCategory.CommisRate = commisRate;
            Context.SaveChanges();
        }


        public void UpdateLogo(long shopId, string img)
        {
            Context.ShopInfo.FindById(shopId).Logo = img;
            Context.SaveChanges();
        }


        public void UpdateShopSenderInfo(long shopId, int regionId, string address, string senderName, string senderPhone)
        {
            var shop = Context.ShopInfo.FirstOrDefault(item => item.Id == shopId);
            if (shop == null)
                throw new InvalidPropertyException("未找到对应的商铺");

            shop.SenderRegionId = regionId;
            shop.SenderAddress = address;
            shop.SenderName = senderName;
            shop.SenderPhone = senderPhone;

            Context.SaveChanges();
        }


        public bool ExistShop(string shopName, long shopId = 0)
        {
            ShopInfo shop = Context.ShopInfo.Where(p => p.ShopName.Equals(shopName) && p.Id != shopId).FirstOrDefault();
            if (shop == null)
            {
                return false;
            }
            else
            {
                return true;
            }

            //return context.ShopInfo.Any(s => s.ShopName.Equals(shopName) && s.Id != shopId);
        }


        public IQueryable<StatisticOrderCommentsInfo> GetShopStatisticOrderComments(long shopId)
        {
            return Context.StatisticOrderCommentsInfo.Where(c => c.ShopId == shopId);
        }


        public ShopInfo.ShopVistis GetShopVistiInfo(DateTime startDate, DateTime endDate, long shopId)
        {
            ShopInfo.ShopVistis shopVistis = (
                from p in Context.ShopVistiInfo
                where p.ShopId == shopId && p.Date >= startDate && p.Date <= endDate
                group p by p.ShopId into g
                select new ShopInfo.ShopVistis
                {
                    SaleAmounts = g.Sum(c => c.SaleAmounts),
                    SaleCounts = g.Sum(c => c.SaleCounts),
                    VistiCounts = g.Sum(c => c.VistiCounts),
                    OrderCounts = decimal.Zero
                }
                    ).FirstOrDefault();
            if (shopVistis != null)
            {
                shopVistis.OrderCounts = Context.OrderInfo.Where(c => c.OrderDate >= startDate && c.OrderDate <= endDate && c.ShopId == shopId).Count();
            }
            return shopVistis;
        }


        public bool ExistCompanyName(string companyName, long shopId = 0)
        {
            return Context.ShopInfo.Any(s => s.CompanyName.Equals(companyName) && s.Id != shopId);
        }
        public bool ExistBusinessLicenceNumber(string BusinessLicenceNumber, long shopId = 0)
        {
            if (!string.IsNullOrWhiteSpace(BusinessLicenceNumber))
                return Context.ShopInfo.Any(s => s.BusinessLicenceNumber.Equals(BusinessLicenceNumber) && s.Id != shopId);
            else
                return false;
        }

        public void AddShopRenewRecord(ShopRenewRecord record)
        {
            Context.ShopRenewRecord.Add(record);
            Context.SaveChanges();
            //入驻缴费（升级和续费）
            var PlatAccount = Context.PlatAccountInfo.FirstOrDefault();
            PlatAccountItemInfo pinfo = new PlatAccountItemInfo();
            pinfo.IsIncome = true;
            pinfo.DetailId = record.Id.ToString();
            if (string.IsNullOrEmpty(record.TradeNo))
            {
                pinfo.AccountNo = Guid.NewGuid().ToString("N");
            }
            else
            {
                pinfo.AccountNo = record.TradeNo;
            }
            pinfo.ReMark = "入驻缴费:" + record.OperateContent;
            pinfo.TradeType = CommonModel.PlatAccountType.SettledPayment;
            pinfo.CreateTime = DateTime.Now;
            pinfo.Amount = record.Amount;
            pinfo.AccoutID = PlatAccount.Id;
            PlatAccount.Balance += pinfo.Amount;//总余额加钱
            pinfo.Balance = PlatAccount.Balance;
            Context.PlatAccountItemInfo.Add(pinfo);
            Context.SaveChanges();
        }

        public void ShopReNew(long shopid, int year)
        {
            var shopinfo = Context.ShopInfo.Where(r => r.Id == shopid).FirstOrDefault();
            if (shopinfo != null)
            {
                DateTime enddate = shopinfo.EndDate.Value;
                if (enddate < DateTime.Now)
                    enddate = DateTime.Now;
                shopinfo.EndDate = enddate.AddYears(year);
                Context.SaveChanges();
            }
        }

        public void ShopUpGrade(long shopid, long gradeid)
        {
            var shopinfo = Context.ShopInfo.Where(r => r.Id == shopid).FirstOrDefault();
            if (shopinfo != null)
            {
                shopinfo.GradeId = gradeid;
                Context.SaveChanges();
            }
        }

        public ObsoletePageModel<ShopRenewRecord> GetShopRenewRecords(ShopQuery query)
        {
            IQueryable<ShopRenewRecord> complaints = Context.ShopRenewRecord.Where(r => r.ShopId == query.BrandId).AsQueryable();

            int total;
            complaints = complaints.GetPage(out total, query.PageNo, query.PageSize);

            ObsoletePageModel<ShopRenewRecord> pageModel = new ObsoletePageModel<ShopRenewRecord>() { Models = complaints, Total = total };
            return pageModel;
        }


        public void ApplyShopBusinessCate(long shopId, IEnumerable<long> categoryIds)
        {
            BusinessCategoriesApplyInfo info = new BusinessCategoriesApplyInfo();
            info.ApplyDate = DateTime.Now;
            info.AuditedStatus = BusinessCategoriesApplyInfo.BusinessCateApplyStatus.UnAudited;
            var shop = Context.ShopInfo.Find(shopId);
            info.ShopId = shop.Id;
            info.ShopName = shop.ShopName;
            var categories = Context.CategoryInfo.FindBy(item => categoryIds.Contains(item.Id)).ToArray();
            var excate = Context.BusinessCategoryInfo.Where(a => a.ShopId == shopId && categoryIds.Contains(a.CategoryId)).ToList();
            foreach (var cate in categories)
            {
                if (!excate.Any(a => a.CategoryId == cate.Id))
                {
                    BusinessCategoriesApplyDetailInfo detail = new BusinessCategoriesApplyDetailInfo();
                    detail.CategoryId = cate.Id;
                    detail.CommisRate = cate.CommisRate;
                    info.Himall_BusinessCategoriesApplyDetail.Add(detail);
                }
            }
            Context.BusinessCategoriesApplyInfo.Add(info);
            Context.SaveChanges();
        }

        public void AuditShopBusinessCate(long applyId, BusinessCategoriesApplyInfo.BusinessCateApplyStatus status)
        {
            var apply = Context.BusinessCategoriesApplyInfo.FirstOrDefault(a => a.Id == applyId);
            apply.AuditedStatus = status;
            if (status == BusinessCategoriesApplyInfo.BusinessCateApplyStatus.Audited) //審核通過
            {
                // List<long> categoryids = new List<long>();
                foreach (var t in apply.Himall_BusinessCategoriesApplyDetail)
                {
                    var cate = Context.BusinessCategoryInfo.Any(a => a.ShopId == apply.ShopId && a.CategoryId == t.CategoryId);
                    if (!cate)
                    {
                        var businessCategoies = new BusinessCategoryInfo()
                        {
                            CategoryId = t.CategoryId,
                            ShopId = apply.ShopId,
                            CommisRate = t.CommisRate
                        };
                        Context.BusinessCategoryInfo.Add(businessCategoies);
                    }
                }
            }
            apply.AuditedDate = DateTime.Now;
            Context.SaveChanges();
        }

        public ObsoletePageModel<BusinessCategoriesApplyInfo> GetBusinessCateApplyList(BussinessCateApplyQuery query)
        {
            int total = 0;
            var apply = Context.BusinessCategoriesApplyInfo.AsQueryable();

            if (!string.IsNullOrEmpty(query.ShopName))
            {
                apply = apply.Where(a => a.ShopName.Contains(query.ShopName));
            }
            if (query.shopId.HasValue && query.shopId.Value != 0)
            {
                apply = apply.Where(a => a.ShopId == query.shopId.Value);
            }
            if (query.Status.HasValue)
            {
                apply = apply.Where(a => a.AuditedStatus == query.Status.Value);
            }
            var result = apply.GetPage(out total, d => d.OrderByDescending(a => a.ApplyDate), query.PageNo, query.PageSize);
            // var favorite = context.FavoriteShopInfo.FindBy(a => a.UserId == userId, pageNo, pageSize, out total, a => a.Id, false);
            ObsoletePageModel<BusinessCategoriesApplyInfo> pageModel = new ObsoletePageModel<BusinessCategoriesApplyInfo>()
            {
                Models = result,
                Total = total
            };
            return pageModel;
        }

        public BusinessCategoriesApplyInfo GetBusinessCategoriesApplyInfo(long applyId)
        {
            var apply = Context.BusinessCategoriesApplyInfo.Find(applyId);
            foreach (var detail in apply.Himall_BusinessCategoriesApplyDetail.ToList())
            {
                detail.CatePath = GetCategoryNameByPath(detail.CategoryId);
            }
            return apply;
        }

        public List<CategoryRateModel> GetThirdBusinessCategory(long id, long shopId)
        {
            List<CategoryRateModel> list = new List<CategoryRateModel>();
            var allCate = Context.CategoryInfo.ToList();
            var c = Context.CategoryInfo.FindById(id);
            if (c.Depth == 1 && c.ParentCategoryId == 0)
            {
                var secondCategoryIds = allCate.Where(item => item.ParentCategoryId == c.Id).Select(item => item.Id);
                var thcate = allCate.Where(item => secondCategoryIds.Contains(item.ParentCategoryId));
                foreach (var t in thcate)
                {
                    list.Add(new CategoryRateModel { Id = t.Id, Path = GetCategoryNameByPath(t.Id), Rate = t.CommisRate });
                }
            }
            else if (c.Depth == 2)
            {
                var thridCate = allCate.Where(a => a.ParentCategoryId == c.Id);
                foreach (var th in thridCate)
                {
                    list.Add(new CategoryRateModel { Id = th.Id, Path = GetCategoryNameByPath(th.Id), Rate = th.CommisRate });
                }
            }
            else
            {
                list.Add(new CategoryRateModel { Id = c.Id, Path = GetCategoryNameByPath(c.Id), Rate = c.CommisRate });
            }
            var mycate = Context.BusinessCategoryInfo.Where(a => a.ShopId == shopId).Select(item => item.CategoryId).ToList();
            list = list.Where(a => !mycate.Contains(a.Id)).ToList();
            return list;
        }

        /// <summary>
        /// 冻结/解冻店铺
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state">true冻结 false解冻</param>
        public void FreezeShop(long id, bool state)
        {
            var shop = Context.ShopInfo.FirstOrDefault(d => d.Id == id);
            if (shop == null)
            {
                throw new HimallException("错误的店铺编号");
            }
            if (shop.IsSelf == false)
            {
                if (state)
                {
                    shop.ShopStatus = ShopInfo.ShopAuditStatus.Freeze;
                }
                else
                {
                    shop.ShopStatus = ShopInfo.ShopAuditStatus.Open;
                }
                SaleOffAllProduct(shop.Id);
                CloseAllMarketingAction(shop.Id);
                Context.SaveChanges();
            }
        }
        /// <summary>
        /// 关闭所有营销活动
        /// </summary>
        public void CloseAllMarketingAction(long id)
        {
            Context.Database.ExecuteSqlCommand("update Himall_FlashSale set Status=4,EndDate='" + DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss") + "' where ShopId=" + id.ToString() + "");
        }
        /// <summary>
        /// 将所有在售的商品下架
        /// </summary>
        /// <param name="id"></param>
        public void SaleOffAllProduct(long id)
        {
            Context.Database.ExecuteSqlCommand("update Himall_Products set SaleStatus=2 where ShopId=" + id.ToString() + " and SaleStatus=1");
        }

        public void AutoCloseMarketingActionByShopExpiredOrFreeze()
        {
            Context.Database.ExecuteSqlCommand("update Himall_Products set SaleStatus=2 where  ShopId in (select id from himall_shops where ShopStatus=6 or EndDate<'" + DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss") + "') and SaleStatus=1");
        }

        public void AutoSaleOffProductByShopExpiredOrFreeze()
        {
            Context.Database.ExecuteSqlCommand("update Himall_FlashSale set EndDate='" + DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss") + "' where  ShopId in (select id from himall_shops where ShopStatus=6 or EndDate<'" + DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss") + "')");
        }

        public int GetShopFavoritesCount(long shopId)
        {
            int result = 0;
            result = Context.FavoriteShopInfo.Where(item => item.ShopId == shopId).Count();
            return result;
        }

        public int GetShopProductCount(long shopId)
        {
            return Context.ProductInfo.Where(item => item.ShopId == shopId && item.SaleStatus == ProductInfo.ProductSaleStatus.OnSale).Count();
        }

        /// <summary>
        /// 获取单条入驻缴费记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ShopRenewRecord GetShopRenewRecord(long id)
        {
            return Context.ShopRenewRecord.Where(a => a.Id == id).FirstOrDefault();
        }
        #endregion

        /// <summary>
        /// 获取商铺管理员会员ID
        /// </summary>
        /// <param name="ShopId"></param>
        /// <returns></returns>
        public long GetShopManagers(long ShopId)
        {
            long id = (
                from p in Context.ManagerInfo
                join u in Context.UserMemberInfo on p.UserName equals u.UserName
                where p.ShopId == ShopId
                select u.Id
                    ).FirstOrDefault();
            return id;
        }

        /// <summary>
        /// 获取自营店铺信息
        /// </summary>
        /// <returns></returns>
        public ShopInfo GetSelfShop()
        {
            ShopInfo result = null;
            string sql = "select * from himall_shops where isself = 1";
            using(MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                result = conn.QueryFirstOrDefault<ShopInfo>(sql);
            }

            return result;
        }

        /// <summary>
        /// 获取商铺免邮活动 邮费
        /// </summary>
        /// <param name="ShopId"></param>
        /// <returns></returns>
        public decimal GetShopFreeFreight(long id)
        {
            return Context.ShopInfo.Where(a => a.Id == id).Select(a => a.FreeFreight).FirstOrDefault();
        }
    }
}
