using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Core;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class ShopBonusModel
    {
        public ShopBonusModel()
        {

        }

        public ShopBonusModel( ShopBonusInfo m )
        {
            Id = m.Id;
            Count = m.Count;
            RandomAmountStart = m.RandomAmountStart;
            RandomAmountEnd = m.RandomAmountEnd;
            UseState = m.UseState;
            UseStateStr = m.UseState.ToDescription();
            UsrStatePrice = m.UsrStatePrice;
            GrantPrice = m.GrantPrice;
            DateStart = m.DateStart;
            DateEnd = m.DateEnd;
            DateStartStr = m.DateStart.ToString( "yyyy-MM-dd" );
            DateEndStr = m.DateEnd.ToString( "yyyy-MM-dd" );
            BonusDateStart = m.BonusDateStart;
            BonusDateEnd = m.BonusDateEnd;
            BonusDateStartStr = m.BonusDateStart.ToString( "yyyy-MM-dd" );
            BonusDateEndStr = m.BonusDateEnd.ToString( "yyyy-MM-dd" );
            ShareTitle = m.ShareTitle;
            ShareDetail = m.ShareDetail;
            ShareImg = m.ShareImg;
            SynchronizeCard = m.SynchronizeCard;
            CardTitle = m.CardTitle;
            CardColor = m.CardColor;
            CardSubtitle = m.CardSubtitle;
            IsInvalid = m.IsInvalid;
            ReceiveCount = ( int )m.ReceiveCount;
            Name = m.Name;
            QRPath = m.QRPath;
            ShopId = m.ShopId;
        }

        public static implicit operator ShopBonusInfo( ShopBonusModel m )
        {
            return new ShopBonusInfo
            {
                Id = m.Id ,
                Count = m.Count ,
                RandomAmountStart = m.RandomAmountStart ,
                RandomAmountEnd = m.RandomAmountEnd ,
                UseState = m.UseState ,
                UsrStatePrice = m.UsrStatePrice ,
                GrantPrice = m.GrantPrice ,
                DateStart = m.DateStart ,
                DateEnd = m.DateEnd ,
                BonusDateStart = m.BonusDateStart ,
                BonusDateEnd = m.BonusDateEnd ,
                ShareTitle = m.ShareTitle ,
                ShareDetail = m.ShareDetail ,
                ShareImg = m.ShareImg ,
                SynchronizeCard = m.SynchronizeCard ,
                CardTitle = m.CardTitle ,
                CardColor = m.CardColor ,
                CardSubtitle = m.CardSubtitle ,
                IsInvalid = m.IsInvalid ,
                ReceiveCount = m.ReceiveCount ,
                Name = m.Name ,
                QRPath = m.QRPath
            };
        }

        #region Propteries
        public long Id { get; set; }

        public int Count { get; set; }

        public decimal RandomAmountStart { get; set; }

        public decimal RandomAmountEnd { get; set; }

        public Himall.Model.ShopBonusInfo.UseStateType UseState { get; set; }

        public string UseStateStr { get; set; }

        public decimal UsrStatePrice { get; set; }

        public decimal GrantPrice { get; set; }

        public DateTime DateStart { get; set; }

        public DateTime DateEnd { get; set; }

        public string DateStartStr { get; set; }

        public string DateEndStr { get; set; }

        public DateTime BonusDateStart { get; set; }

        public DateTime BonusDateEnd { get; set; }

        public string BonusDateStartStr { get; set; }

        public string BonusDateEndStr { get; set; }

        public string ShareTitle { get; set; }

        public string ShareDetail { get; set; }

        public string ShareImg { get; set; }

        public bool SynchronizeCard { get; set; }

        public string CardTitle { get; set; }

        public string CardColor { get; set; }

        public string CardSubtitle { get; set; }

        public bool IsInvalid { get; set; }

        public int ReceiveCount { get; set; }

        public string Name { get; set; }

        public string QRPath { get; set; }

        #endregion

        public long ShopId { get; set; }

        public bool IsShowSyncWeiXin { get; set; }

        public long ReceiveId { get; set; }
        #region 微信JSSDK参数
        public WXSyncJSInfoByCard WXJSInfo { get; set; }
        public WXJSCardModel WXJSCardInfo { get; set; }
        #endregion
    }
}