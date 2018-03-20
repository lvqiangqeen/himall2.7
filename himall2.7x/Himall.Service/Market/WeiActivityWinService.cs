using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Entity;
using Himall.Core;
using System.Data.Entity.Infrastructure;
using Himall.Service.Market.Business;
using System.Drawing;
using System.IO;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.User;

namespace Himall.Service
{
    public class WeiActivityWinService : ServiceBase,IWeiActivityWinService
    {
        public ObsoletePageModel<WeiActivityWinInfo> Get(string text,long id, int pageIndex, int pageSize)
        {
            
            IQueryable<WeiActivityWinInfo> query = Context.WeiActivityWinInfo;
        
            if (text == "1")
            {
                query = query.Where(p => p.IsWin == true);
            }
            else if (text == "0")
            {
                query = query.Where(p => p.IsWin == false);
            }
            query = query.Where(p => p.ActivityId == id);

            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }
            
            int total = 0;
            IQueryable<WeiActivityWinInfo> datas = query.GetPage(out total, p => p.OrderByDescending(o => o.AddDate), pageIndex, pageSize);
            ObsoletePageModel<WeiActivityWinInfo> pageModel = new ObsoletePageModel<WeiActivityWinInfo>()
            {
                Models = datas,
                Total = total
            };
            return pageModel;
        }

      

        public void AddWinner(WeiActivityWinInfo info)
        {
            var awardinfo = Context.WeiActivityAwardInfo.FirstOrDefault(t => t.Id == info.AwardId);
            if (awardinfo !=null && awardinfo.BonusId>0)
            {
                var bonu = Context.BonusInfo.FirstOrDefault(t => t.Id == awardinfo.BonusId);
                if (bonu != null)
                    info.AwardName = bonu.Name;
            }
            Context.WeiActivityWinInfo.Add(info);
            Context.SaveChanges();
        }

        public string GetWinNumber(long id, string text)
        {
            IQueryable<WeiActivityWinInfo> query = Context.WeiActivityWinInfo;
            query = query.Where(p => p.ActivityId==id );
            if (text=="True")
            {
                 query = query.Where(p => p.IsWin==true );
            }
            else if (text=="False")
            {
                query = query.Where(p => p.IsWin == false);
            }
            return  query.GroupBy(x => x.UserId).Count().ToString();
        }

        public List<WeiActivityWinInfo> GetWinInfo(long userId)
        {
            IQueryable<WeiActivityWinInfo> query = Context.WeiActivityWinInfo;
            query = query.Where(p => p.UserId == userId);
            return query.ToList();
        }

       
    }
}
