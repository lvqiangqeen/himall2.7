using Himall.IServices;
using Himall.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices.QueryModel;
using Himall.Entity;
using Himall.Core;
using Himall.Core.Helper;
using System.Drawing;


namespace Himall.Service
{
    public class VShopService : ServiceBase, IVShopService
    {
        public ObsoletePageModel<VShopInfo> GetVShopByParamete(VshopQuery vshopQuery)
        {
            int total = 0;
            var vshops = Context.VShopInfo.AsQueryable();
            if (vshopQuery.VshopType.HasValue)
            {
                if (vshopQuery.VshopType != 0)
                {
                    vshops = from a in Context.VShopInfo
                             where
                             (from b in a.VShopExtendInfo
                              where b.Type == vshopQuery.VshopType
                              select b.VShopId).Contains(a.Id)
                             select a;
                }
                else
                {
                    vshops = from a in Context.VShopInfo
                             where
                             a.VShopExtendInfo.Count() == 0
                             select a;
                }
            }
            if (!string.IsNullOrEmpty(vshopQuery.Name))
            {
                vshops = vshops.Where(a => a.Name.Contains(vshopQuery.Name));
            }
            if (vshopQuery.ExcepetVshopId.HasValue && vshopQuery.ExcepetVshopId.Value != 0)
            {
                vshops = vshops.Where(a => a.Id != vshopQuery.ExcepetVshopId.Value);
            }
            vshops = vshops.Where(e => e.State == VShopInfo.VshopStates.Normal || e.State == VShopInfo.VshopStates.Close);
            total = vshops.Count();
            ObsoletePageModel<VShopInfo> result = new ObsoletePageModel<VShopInfo>();
            result.Models = vshops.OrderBy(a => a.CreateTime).Skip((vshopQuery.PageNo - 1) * vshopQuery.PageSize).Take(vshopQuery.PageSize).AsQueryable();
            result.Total = total;
            return result;
        }

        IEnumerable<VShopInfo> GetAllVshop()
        {
            return Context.VShopInfo.Where(a => a.State != Himall.Model.VShopInfo.VshopStates.Close);
        }

        VShopInfo GetVshopById(long vshopId)
        {
            return Context.VShopInfo.Where(a => a.Id == vshopId).FirstOrDefault();
        }

        public IEnumerable<VShopInfo> GetHotShop(VshopQuery vshopQuery, DateTime? startTime, DateTime? endTime, out int total)
        {
            var vshops = (from s in Context.VShopInfo
                          join vs in Context.VShopExtendInfo on s.Id equals vs.VShopId
                          where vs.Type == VShopExtendInfo.VShopExtendType.HotVShop
                          orderby vs.Sequence, s.CreateTime
                          select s);
            if (!string.IsNullOrEmpty(vshopQuery.Name))
                vshops = vshops.Where(a => a.Name.Contains(vshopQuery.Name));
            if (startTime.HasValue)
            {
                vshops = vshops.Where(a => a.VShopExtendInfo.Any(item => item.AddTime >= startTime));
            }
            if(endTime.HasValue)
            {
                var end=endTime.Value.Date.AddDays(1);
                vshops = vshops.Where(a => a.VShopExtendInfo.Any(item =>item.AddTime < end));
            }
            total = vshops.Count();
            return vshops.Skip((vshopQuery.PageNo - 1) * vshopQuery.PageSize).Take(vshopQuery.PageSize);
        }

        public VShopInfo GetTopShop()
        {
            return Context.VShopInfo.Where(a => a.State != VShopInfo.VshopStates.Close && a.VShopExtendInfo.Any(item => item.Type == Himall.Model.VShopExtendInfo.VShopExtendType.TopShow)).FirstOrDefault();
        }

        public void SetTopShop(long vshopId)
        {
            var newTopVshop = GetVshopById(vshopId);
            if (Context.VShopExtendInfo.Where(a => a.Type == VShopExtendInfo.VShopExtendType.TopShow).Count() == 1)
            {
                var oldTopVshop = Context.VShopExtendInfo.Where(a => a.Type == VShopExtendInfo.VShopExtendType.TopShow).FirstOrDefault();
                Context.VShopExtendInfo.Remove(oldTopVshop);
            }
            if (Context.VShopExtendInfo.Where(a => a.VShopId == vshopId).Count() >= 1)
            {
                Context.VShopExtendInfo.Remove(item => item.VShopId == vshopId);
            }
            Context.VShopExtendInfo.Add(new VShopExtendInfo
            {
                VShopId = vshopId,
                Type = VShopExtendInfo.VShopExtendType.TopShow,
                AddTime = DateTime.Now
            });
            Context.SaveChanges();
        }

        public void SetHotShop(long vshopId)
        {
            var shops = GetAllVshop().ToList();
            if (shops.Where(a => a.VShopExtendInfo != null && a.VShopExtendInfo.Any(item => item.Type == VShopExtendInfo.VShopExtendType.HotVShop)).Count() >= 60)
                throw new Himall.Core.HimallException("热门微店最多为60个");

            if (GetVshopById(vshopId).VShopExtendInfo.Where(a => a.Type == VShopExtendInfo.VShopExtendType.HotVShop).Count() >= 1)
                throw new Himall.Core.HimallException("该微店已经是热门微店");
            if (Context.VShopExtendInfo.Where(a => a.VShopId == vshopId).Count() >= 1)
            {
                Context.VShopExtendInfo.Remove(item => item.VShopId == vshopId);
            }
            Context.VShopExtendInfo.Add(new VShopExtendInfo
            {
                VShopId = vshopId,
                AddTime = DateTime.Now,
                Type = VShopExtendInfo.VShopExtendType.HotVShop,
                Sequence = 1
            });
            Context.SaveChanges();
        }

        public void CloseShop(long vshopId)
        {
            var vshopInfo = GetVshopById(vshopId);
            vshopInfo.State = VShopInfo.VshopStates.Close;
            Context.SaveChanges();
        }

        public void SetShopNormal(long vshopId)
        {
            var vshopInfo = GetVshopById(vshopId);
            vshopInfo.State = VShopInfo.VshopStates.Normal;
            Context.SaveChanges();
        }

        public void DeleteHotShop(long vshopId)
        {
            Context.VShopExtendInfo.Remove(item => item.VShopId == vshopId && item.Type == VShopExtendInfo.VShopExtendType.HotVShop);
            Context.SaveChanges();
        }

        public void ReplaceHotShop(long oldVShopId, long newHotVShopId)
        {
            var vshopExtendInfo = Context.VShopExtendInfo.FindBy(item => item.VShopId == oldVShopId && item.Type == VShopExtendInfo.VShopExtendType.HotVShop).FirstOrDefault();
            vshopExtendInfo.VShopId = newHotVShopId;
            vshopExtendInfo.AddTime = DateTime.Now;
            Context.SaveChanges();
        }

        public void UpdateSequence(long vshopId, int? sequence)
        {
            var vshopExtendInfo = Context.VShopExtendInfo.FindBy(item => item.VShopId == vshopId && item.Type == VShopExtendInfo.VShopExtendType.HotVShop).FirstOrDefault();
            vshopExtendInfo.Sequence = sequence;
            Context.SaveChanges();
        }
        public void AuditThrough(long vshopId)
        {
            Context.VShopExtendInfo.Add(new VShopExtendInfo
            {
                Sequence = 1,
                VShopId = vshopId,
                AddTime = DateTime.Now,
                Type = VShopExtendInfo.VShopExtendType.HotVShop
            });
            Context.SaveChanges();
        }

        public void AuditRefused(long vshopId)
        {
            var vshopInfo = GetVshopById(vshopId);
            vshopInfo.State = Himall.Model.VShopInfo.VshopStates.Refused;
            Context.SaveChanges();
        }

        public void ReplaceTopShop(long oldVShopId, long newTopVShopId)
        {
            var vshopExtendInfo = Context.VShopExtendInfo.FindBy(item => item.VShopId == oldVShopId && item.Type == VShopExtendInfo.VShopExtendType.TopShow).FirstOrDefault();
            vshopExtendInfo.VShopId = newTopVShopId;
            vshopExtendInfo.AddTime = DateTime.Now;
            Context.SaveChanges();
        }

        public void DeleteTopShop(long vshopId)
        {
            Context.VShopExtendInfo.Remove(item => item.VShopId == vshopId && item.Type == VShopExtendInfo.VShopExtendType.TopShow);
            Context.SaveChanges();
        }


        public VShopInfo GetVShopByShopId(long shopId)
        {
            return Context.VShopInfo.FirstOrDefault(item => item.ShopId == shopId);
        }


        public void CreateVshop(VShopInfo vshopInfo)
        {
            if (vshopInfo.ShopId <= 0)
                throw new Himall.Core.InvalidPropertyException("请传入合法的店铺Id，店铺Id必须大于0");
            if (string.IsNullOrWhiteSpace(vshopInfo.Logo))
                throw new Himall.Core.InvalidPropertyException("微店Logo不能为空");
            if (string.IsNullOrWhiteSpace(vshopInfo.WXLogo))
                throw new Himall.Core.InvalidPropertyException("微信Logo不能为空");
            if (string.IsNullOrWhiteSpace(vshopInfo.BackgroundImage))
                throw new Himall.Core.InvalidPropertyException("微店背景图片不能为空");
            //if (string.IsNullOrWhiteSpace(vshopInfo.Description))
            //{
            //    throw new Himall.Core.InvalidPropertyException("微店描述不能为空");
            //}
            if (!string.IsNullOrWhiteSpace(vshopInfo.Tags) && vshopInfo.Tags.Split(',').Length > 4)
                throw new Himall.Core.InvalidPropertyException("最多只能有4个标签");

            bool exist = Context.VShopInfo.Any(item => item.ShopId == vshopInfo.ShopId);
            if (exist)
                throw new Himall.Core.InvalidPropertyException(string.Format("店铺{0}已经创建过微店", vshopInfo.ShopId));

            var shopInfo = ServiceProvider.Instance<IShopService>.Create.GetShop(vshopInfo.ShopId);
            vshopInfo.Name = shopInfo.ShopName;//使用店铺名称作为微店名称
            vshopInfo.CreateTime = DateTime.Now;
            vshopInfo.State = VShopInfo.VshopStates.Normal;

            string logo = vshopInfo.Logo, backgroundImage = vshopInfo.BackgroundImage;
            string wxlogo = vshopInfo.WXLogo;
            CopyImages(vshopInfo.ShopId, ref logo, ref backgroundImage, ref wxlogo);
            vshopInfo.Logo = logo;
            vshopInfo.BackgroundImage = backgroundImage;
            vshopInfo.WXLogo = wxlogo;

            Context.VShopInfo.Add(vshopInfo);
            Context.SaveChanges();
        }

        public void UpdateVShop(VShopInfo vshopInfo)
        {
            if (vshopInfo.Id <= 0)
                throw new Himall.Core.InvalidPropertyException("请传入合法的微店Id，微店Id必须大于0");
            if (string.IsNullOrWhiteSpace(vshopInfo.Logo))
                throw new Himall.Core.InvalidPropertyException("微店Logo不能为空");
            if (string.IsNullOrWhiteSpace(vshopInfo.WXLogo))
                throw new Himall.Core.InvalidPropertyException("微信Logo不能为空");
            if (string.IsNullOrWhiteSpace(vshopInfo.BackgroundImage))
                throw new Himall.Core.InvalidPropertyException("微店背景图片不能为空");
            //if (string.IsNullOrWhiteSpace(vshopInfo.Description))
            //{
            //    throw new Himall.Core.InvalidPropertyException("微店描述不能为空");
            //}
            if (!string.IsNullOrWhiteSpace(vshopInfo.Tags) && vshopInfo.Tags.Split(',').Length > 4)
                throw new Himall.Core.InvalidPropertyException("最多只能有4个标签");

            var oriVShop = GetVshopById(vshopInfo.Id);
            if (oriVShop.ShopId != vshopInfo.ShopId)
                throw new Himall.Core.InvalidPropertyException("修改微店信息时，不能变更所属店铺");

            oriVShop.HomePageTitle = vshopInfo.HomePageTitle;
            oriVShop.Tags = vshopInfo.Tags;
            oriVShop.Description = vshopInfo.Description;

            string logo = vshopInfo.Logo, backgroundImage = vshopInfo.BackgroundImage;
            string wxlogo = vshopInfo.WXLogo;
            CopyImages(vshopInfo.ShopId, ref logo, ref backgroundImage, ref wxlogo);
            oriVShop.Logo = logo;
            oriVShop.BackgroundImage = backgroundImage;
            oriVShop.WXLogo = wxlogo;

            Context.SaveChanges();
        }

        void CopyImages(long shopId, ref string logo, ref string backgroundImage, ref string wxlogo)
        {
            string newDir = string.Format("/Storage/Shop/{0}/VShop/", shopId);

            if (!string.IsNullOrWhiteSpace(logo))
            {
                if (logo.Contains("/temp/"))
                {
                    string logoname = logo.Substring(logo.LastIndexOf('/') + 1);
                    string oldlogo = logo.Substring(logo.LastIndexOf("/temp"));
                    string newLogo = newDir + logoname;
                    Core.HimallIO.CopyFile(oldlogo, newLogo, true);
                    logo = newLogo;
                }
                else if (logo.Contains("/Storage/"))
                {
                    logo = logo.Substring(logo.LastIndexOf("/Storage"));
                }
            }

            if (!string.IsNullOrWhiteSpace(backgroundImage))
            {
                if (backgroundImage.Contains("/temp/"))
                {
                    string logoname = backgroundImage.Substring(backgroundImage.LastIndexOf('/') + 1);
                    string oldpic = backgroundImage.Substring(backgroundImage.LastIndexOf("/temp"));
                    string newfile = newDir + logoname;
                    Core.HimallIO.CopyFile(oldpic, newfile, true);
                    backgroundImage = newfile;
                }
                else if (backgroundImage.Contains("/Storage/"))
                {
                    backgroundImage = backgroundImage.Substring(logo.LastIndexOf("/Storage"));
                }
            }

            if (!string.IsNullOrWhiteSpace(wxlogo))
            {
                if (wxlogo.Contains("/temp/"))
                {
                    string logoname = wxlogo.Substring(wxlogo.LastIndexOf('/') + 1);
                    string logofilepername = logoname.Substring(0, logoname.LastIndexOf('.'));
                    string oldpic = wxlogo.Substring(wxlogo.LastIndexOf("/temp"));
                    string newfile = newDir + logofilepername + ".png";
                    //Core.HimallIO.CreateThumbnail(oldpic, newfile, 100, 100);
                    Core.HimallIO.CopyFile(oldpic, newfile, true);
                    wxlogo = newfile;
                }
                else if (wxlogo.Contains("/Storage/"))
                {
                    wxlogo = wxlogo.Substring(wxlogo.LastIndexOf("/Storage"));
                }
            }

        }
        /// <summary>
        /// 增加访问统计
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int LogVisit(long id)
        {
            int result = 0;
            var vshopInfo = GetVshopById(id);
            vshopInfo.VisitNum += 1;
            Context.SaveChanges();
            result = vshopInfo.VisitNum;
            return result;
        }

        public IQueryable<VShopInfo> GetVShops()
        {
            return Context.VShopInfo.FindAll();
        }

		/// <summary>
		/// 根据商家id获取微店信息
		/// </summary>
		/// <param name="shopIds"></param>
		/// <returns></returns>
		public List<VShopInfo> GetVShopsByShopIds(IEnumerable<long> shopIds)
		{
			return this.Context.VShopInfo.Where(p => shopIds.Contains(p.ShopId)).ToList();
		}

        public IQueryable<VShopInfo> GetVShops(int page, int pageSize, out int total)
        {
            total = Context.VShopInfo.Count();
            return Context.VShopInfo.OrderByDescending(item => item.Id).Skip((page - 1) * pageSize).Take(pageSize);
        }

        public IQueryable<VShopInfo> GetVShops(int page, int pageSize, out int total, VShopInfo.VshopStates state)
        {
            var vshop = Context.VShopInfo.Where(item => item.State == state);
            total = vshop.Count();
            return vshop.OrderByDescending(item => item.Id).Skip((page - 1) * pageSize).Take(pageSize);
        }

        public VShopInfo GetVShop(long id)
        {
            return Context.VShopInfo.FirstOrDefault(item => item.Id == id);
        }


        public IQueryable<VShopInfo> GetHotShops(int page, int pageSize, out int total)
        {
            var vshops = from a in Context.VShopInfo
                         where
                         (from b in Context.VShopExtendInfo
                          where b.Type == VShopExtendInfo.VShopExtendType.HotVShop
                          select b.VShopId).Contains(a.Id) && a.State == VShopInfo.VshopStates.Normal
                         select a;
            total = vshops.Count();
            return vshops.OrderBy(d => d.VShopExtendInfo.FirstOrDefault().Sequence).ThenByDescending(item => item.CreateTime).Skip((page - 1) * pageSize).Take(pageSize);
        }


        public void AddVisitNumber(long vshopId)
        {
            var vshop = Context.VShopInfo.FirstOrDefault(item => item.Id == vshopId);
            if (vshop != null)
            {
                vshop.VisitNum++;
                Context.SaveChanges();
            }
        }

        public void AddBuyNumber(long vshopId)
        {
            var vshop = Context.VShopInfo.FirstOrDefault(item => item.Id == vshopId);
            if (vshop != null)
            {
                vshop.buyNum++;
                Context.SaveChanges();
            }
        }

        public IQueryable<VShopInfo> GetUserConcernVShops(long userId, int pageNo, int pageSize)
        {
            var favorite = Context.FavoriteShopInfo.Where(item => item.UserId == userId).Select(item => item.ShopId).ToArray();
            var vshops = Context.VShopInfo.Where(item => item.State != VShopInfo.VshopStates.Close);
            var vshop = vshops.Where(item => favorite.Contains(item.ShopId)).OrderByDescending(item => item.Id).Skip((pageNo - 1) * pageSize).Take(pageSize);
            return vshop;
        }

        public WXShopInfo GetVShopSetting(long shopId)
        {
            return Context.WXShopInfo.FirstOrDefault(item => item.ShopId == shopId);
        }

        public void SaveVShopSetting(WXShopInfo wxShop)
        {
            if (GetVShopSetting(wxShop.ShopId) == null)
                AddVShopSetting(wxShop);
            else
                UpdateVShopSetting(wxShop);

        }

        void AddVShopSetting(WXShopInfo vshopSetting)
        {
            if (string.IsNullOrEmpty(vshopSetting.AppId))
                throw new Himall.Core.HimallException("微信AppId不能为空！");
            if (string.IsNullOrEmpty(vshopSetting.AppSecret))
                throw new HimallException("微信AppSecret不能为空！");
            Context.WXShopInfo.Add(vshopSetting);
            Context.SaveChanges();

        }

        void UpdateVShopSetting(WXShopInfo vshopSetting)
        {
            var wxShop = GetVShopSetting(vshopSetting.ShopId);
            if (string.IsNullOrEmpty(vshopSetting.AppId))
                throw new Himall.Core.HimallException("微信AppId不能为空！");
            if (string.IsNullOrEmpty(vshopSetting.AppSecret))
                throw new HimallException("微信AppSecret不能为空！");
            wxShop.ShopId = vshopSetting.ShopId;
            wxShop.AppId = vshopSetting.AppId;
            wxShop.AppSecret = vshopSetting.AppSecret;
            wxShop.FollowUrl = vshopSetting.FollowUrl;
            Context.SaveChanges();

        }
        /// <summary>
        /// 店铺要显示的优惠卷
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public IQueryable<CouponSettingInfo> GetVShopCouponSetting(long shopid)
        {
            IQueryable<long> couponIdList = Context.CouponInfo.Where(item => item.ShopId == shopid).Select(item => item.Id);
            IQueryable<CouponSettingInfo> couponSetList = Context.CouponSettingInfo.Where(item =>
                couponIdList.Contains(item.CouponID)//过滤店铺
                && item.Himall_Coupon.EndTime > DateTime.Now
                && (item.Display.HasValue ? item.Display == 1 : true)//过滤不显示的
                );
            return couponSetList;
        }

        public void SaveVShopCouponSetting(IEnumerable<CouponSettingInfo> infolist)
        {
            if (infolist == null)
            {
                throw new HimallException("没有可更新的数据！");
            }
            foreach (var el in infolist)
            {
                var setinfo = Context.CouponSettingInfo.FirstOrDefault(item => item.CouponID == el.CouponID);
                if (setinfo != null)
                {
                    setinfo.Display = el.Display;
                }
                else
                {
                    if (el.Display.HasValue && el.Display == 1)
                    {//Disply=1为显示，需增加配置信息
                        Context.CouponSettingInfo.Add(new CouponSettingInfo()
                        {
                            CouponID = el.CouponID,
                            Display = el.Display,
                            PlatForm = PlatformType.Mobile
                        });
                    }
                }
            }
            Context.SaveChanges();
        }

        public string GetVShopLog(long vshopid)
        {
            //TODO LRL 判断没有开微店的情况
            var vshop = Context.VShopInfo.Where(p => p.Id == vshopid);
            if (null == vshop || vshop.Count() == 0)
            {
                return "";
            }
            else
            {
                return Context.VShopInfo.Where(p => p.Id == vshopid).Select(p => p.WXLogo).Single<string>();
            }
        }
    }
}