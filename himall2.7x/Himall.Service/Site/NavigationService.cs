using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System.Linq;
using Newtonsoft.Json;
using Dapper;
using MySql.Data.MySqlClient;

namespace Himall.Service
{
    public class NavigationService : ServiceBase, INavigationService
    {
        public void OpenOrClose(long id, bool status)
        {
            var result = Context.BannerInfo.Where(r => r.Id == id).FirstOrDefault();
            if (result != null)
            {
                result.STATUS = status ? 1 : 0;
                Context.SaveChanges();
            }
        }

        public void AddPlatformNavigation(BannerInfo model)
        {
            model.ShopId = 0;
            AddNavigation(model);
        }

        public void UpdatePlatformNavigation(BannerInfo model)
        {
            model.ShopId = 0;
            UpdateNavigation(model);
        }

        public void DeletePlatformNavigation(long id)
        {
            DeleteNavigation(0, id);
        }


        public IQueryable<BannerInfo> GetPlatNavigations()
        {
            using(MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {

            }
            var result = Context.BannerInfo.FindBy(item => item.ShopId == 0).OrderBy(item => item.DisplaySequence);
            return result;
        }

        public IQueryable<BannerInfo> GetSellerNavigations(long shopId, PlatformType plat = PlatformType.PC)
        {
            var result = Context.BannerInfo.FindBy(item => item.ShopId != 0 && item.ShopId == shopId && item.Platform == plat).OrderBy(item => item.DisplaySequence);
            return result;
        }

        public BannerInfo GetSellerNavigation(long id)
        {
            var banner = Context.BannerInfo.FindById<BannerInfo>(id);
            return banner;
        }

        public void SwapPlatformDisplaySequence(long id, long id2)
        {
            SwapDisplaySequence(0, id, id2);
        }


        public void DeleteSellerformNavigation(long shopId, long id)
        {
            DeleteNavigation(shopId, id);
        }

        public void SwapSellerDisplaySequence(long shopId, long id, long id2)
        {
            SwapDisplaySequence(shopId, id, id2);
        }


        public void AddSellerNavigation(BannerInfo model)
        {
            if (model.ShopId == 0)
                throw new HimallException("店铺id必须大于0");
            if (model.Platform == Himall.Core.PlatformType.WeiXin && Context.BannerInfo.Where(item => item.ShopId == model.ShopId && item.Platform == PlatformType.WeiXin).Count() >= 5)
                throw new Himall.Core.HimallException("导航最多只能添加5个");
            AddNavigation(model);
        }

        public void UpdateSellerNavigation(BannerInfo model)
        {
            if (model.ShopId == 0)
                throw new HimallException("店铺id必须大于0");
            UpdateNavigation(model);
        }


        void DeleteNavigation(long shopId, long id)
        {
            var m = Context.BannerInfo.FindBy(item => item.Id == id && item.ShopId == shopId).FirstOrDefault();
            if (m == null)
            {
                throw new HimallException("该导航不存在，或者已被删除!");
            }
            Context.BannerInfo.Remove(m);
            Context.SaveChanges();
        }

        void SwapDisplaySequence(long shopId, long id, long id2)
        {
            var m1 = Context.BannerInfo.FirstOrDefault(item => item.ShopId == shopId && item.Id == id);
            if (m1 == null)
                throw new HimallException("id为" + id + "的导航不存在，或者已被删除!");

            var m2 = Context.BannerInfo.FirstOrDefault(item => item.ShopId == shopId && item.Id == id2);
            if (m2 == null)
                throw new HimallException("id为" + id2 + "的导航不存在，或者已被删除!");

            if (m1 != null && m2 != null)
            {
                var temp = m1.DisplaySequence;
                m1.DisplaySequence = m2.DisplaySequence;
                m2.DisplaySequence = temp;
                Context.SaveChanges();
            }
        }


        void AddNavigation(BannerInfo model)
        {
            model.Position = 0;
            long max = Context.BannerInfo.Where(item => item.ShopId == model.ShopId).Count();
            if (max > 0)
                max = Context.BannerInfo.Where(item => item.ShopId == model.ShopId).Max(item => item.DisplaySequence);

            model.DisplaySequence = max + 1;
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new HimallException("请输入导航的名称");
            }
            if (System.Text.Encoding.Default.GetByteCount(model.Name) > 20)
            {
                throw new HimallException("导航名称只能为20字符(中文10字符)");
            }
            Context.BannerInfo.Add(model);
            Context.SaveChanges();
        }

        void UpdateNavigation(BannerInfo model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new HimallException("请输入导航的名称");
            }
            if (System.Text.Encoding.Default.GetByteCount(model.Name) > 20)
            {
                throw new HimallException("导航名称只能为20字符(中文10字符)");
            }
            var m = Context.BannerInfo.FindBy(item => item.Id == model.Id && item.ShopId == model.ShopId).FirstOrDefault();
            if (m == null)
            {
                throw new HimallException("该导航不存在，或者已被删除!");
            }
            m.Name = model.Name;
            m.Url = model.Url;
            m.UrlType = model.UrlType;
            Context.SaveChanges();
        }

    }
}

