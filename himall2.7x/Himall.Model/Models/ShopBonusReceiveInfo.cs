using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public partial class ShopBonusReceiveInfo : IBaseCoupon
    {
        public enum ReceiveState
        {
            [Description( "未使用" )]
            NotUse = 1 ,

            [Description( "已使用" )]
            Use = 2 ,

            [Description( "已过期" )]
            Expired = 3
        }


        [NotMapped]
        public long BaseId
        {
            get { return this.Id; }
        }

        [NotMapped]
        public decimal BasePrice
        {
            get { return ( decimal )this.Price; }
        }

        [NotMapped]
        public string BaseName
        {
            get { return this.Himall_ShopBonusGrant.Himall_ShopBonus.Name; }
        }

        [NotMapped]
        public CouponType BaseType
        {
            get { return CouponType.ShopBonus; }
        }

        [NotMapped]
        public string BaseShopName
        {
            get { return this.Himall_ShopBonusGrant.Himall_ShopBonus.Himall_Shops.ShopName; }
        }

        [NotMapped]
        public DateTime BaseEndTime
        {
            get { return this.Himall_ShopBonusGrant.Himall_ShopBonus.BonusDateEnd; }


        }

        [NotMapped]
        public decimal BaseUseStatePrice
        {
            get { return this.Himall_ShopBonusGrant.Himall_ShopBonus.UsrStatePrice; }
        }

        public long BaseShopId
        {
            get { return this.Himall_ShopBonusGrant.Himall_ShopBonus.ShopId; }
        }
    }
}
