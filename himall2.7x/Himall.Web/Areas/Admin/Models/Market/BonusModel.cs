using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Admin.Models.Market
{
    public class BonusModel
    {
        public BonusModel()
        {
        }

        public BonusModel( BonusInfo m )
        {
            Id = m.Id;
            Type = m.Type;
            Style = m.Style;
            PriceType = ( BonusInfo.BonusPriceType )m.PriceType;
            Name = m.Name;
            MerchantsName = m.MerchantsName;
            Remark = m.Remark;
            Blessing = m.Blessing;
            TotalPrice = m.TotalPrice;
            StartTime = m.StartTime;
            EndTime = m.EndTime;
            FixedAmount = ( decimal )m.FixedAmount;
            RandomAmountStart = ( decimal )m.RandomAmountStart;
            RandomAmountEnd = ( decimal )m.RandomAmountEnd;
            ImagePath = m.ImagePath;
            Description = m.Description;
            ReceiveCount = m.ReceiveCount;
            ReceivePrice = m.ReceivePrice;
            ReceiveHref = m.ReceiveHref;
            QRPath = m.QRPath;
            IsInvalid = m.IsInvalid;
            IsAttention = m.IsAttention;
            IsGuideShare = m.IsGuideShare;
        }

        public static implicit operator BonusInfo( BonusModel m )
        {
            return new BonusInfo
            {
                Id = m.Id ,
                Type = m.Type ,
                Style = m.Style ,
                PriceType = m.PriceType ,
                Name = m.Name ,
                MerchantsName = m.MerchantsName ,
                Remark = m.Remark ,
                Blessing = m.Blessing ,
                TotalPrice = m.TotalPrice ,
                StartTime = m.StartTime ,
                EndTime = m.EndTime ,
                FixedAmount = m.FixedAmount ,
                RandomAmountStart = m.RandomAmountStart ,
                RandomAmountEnd = m.RandomAmountEnd ,
                ImagePath = m.ImagePath ,
                Description = m.Description,
                IsAttention = m.IsAttention,
                IsGuideShare = m.IsGuideShare
            };
        }

        #region Properties
        public long Id
        {
            get;
            set;
        }

        public BonusInfo.BonusType Type
        {
            get;
            set;
        }

        public string TypeStr
        {
            get;
            set;
        }

        public BonusInfo.BonusStyle Style
        {
            get;
            set;
        }

        public BonusInfo.BonusPriceType PriceType
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string MerchantsName
        {
            get;
            set;
        }

        public string Remark
        {
            get;
            set;
        }

        public string Blessing
        {
            get;
            set;
        }

        public decimal TotalPrice
        {
            get;
            set;
        }

        public DateTime StartTime
        {
            get;
            set;
        }

        public DateTime EndTime
        {
            get;
            set;
        }

        public string StartTimeStr
        {
            get;
            set;
        }
         
        public string EndTimeStr
        {
            get;
            set;
        }


        public decimal FixedAmount
        {
            get;
            set;
        }

        public decimal RandomAmountStart
        {
            get;
            set;
        }

        public decimal RandomAmountEnd
        {
            get;
            set;
        }

        public int ReceiveCount
        {
            get;
            set;
        }

        public string StateStr { get; set; }

        public string ReceiveHref { get; set; } 

        public decimal ReceivePrice { get; set; }

        public string ImagePath
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }
        #endregion



        public bool IsInvalid
        {
            get;
            set;
        }

        public bool IsAttention { get; set; }

        public bool IsGuideShare { get; set; }

        public string QRPath { get; set; }
    }
}