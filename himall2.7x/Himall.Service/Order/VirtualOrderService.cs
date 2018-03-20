using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;
using Himall.IServices;

namespace Himall.Service
{
    public class VirtualOrderService : ServiceBase, IVirtualOrderService
    {
        public bool CreateVirtualOrder(VirtualOrderInfo model)
        {
            Context.VirtualOrderInfo.Add(model);
            return Context.SaveChanges() > 0;
        }
        public bool UpdateMoneyFlagByPayNum(string payNum)
        {
            VirtualOrderInfo model = Context.VirtualOrderInfo.Where(v => v.PayNum == payNum).FirstOrDefault();
            model.MoneyFlag = 2;
            return Context.SaveChanges() > 0;
        }
        public bool UpdateShopAccountByPayNum(string payNum)
        {
            VirtualOrderInfo model = Context.VirtualOrderInfo.Where(v => v.PayNum == payNum).FirstOrDefault();
            var shopAccount = Context.ShopAccountInfo.Where(s => s.ShopId == model.ShopId).FirstOrDefault();
            shopAccount.Balance += model.PayAmount * 0.94m;//更新商铺余额
            shopAccount.PendingSettlement += model.PayAmount * 0.94m;//更新商铺待结算金额
            return Context.SaveChanges() > 0;
        }
        public bool UpdatePlatAccountByPayNum(string payNum)
        {
            VirtualOrderInfo model = Context.VirtualOrderInfo.Where(v => v.PayNum == payNum).FirstOrDefault();
            var platAccount = Context.PlatAccountInfo.Where(p => true).FirstOrDefault();
            platAccount.Balance += model.PayAmount * 0.06m;
            platAccount.PendingSettlement += model.PayAmount * 0.94m;
            return Context.SaveChanges() > 0;
        }
    }
}
