using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Himall.Core;
using Himall.CommonModel;
using Himall.Model.Models;

namespace Himall.Model
{
    public partial class FightGroupActiveInfo
    {
        /// <summary>
        /// 店铺名
        /// <para>手动补充</para>
        /// </summary>
        [NotMapped]
        public string ShopName { get; set; }
        /// <summary>
        /// 商品图片地址
        /// </summary>
        [NotMapped]
        public string ProductImgPath { get; set; }
        /// <summary>
        /// 拼团活动状态
        /// </summary>
        [NotMapped]
        public FightGroupActiveStatus ActiveStatus {
            get
            {
                FightGroupActiveStatus result = FightGroupActiveStatus.Ending;
                if(EndTime<DateTime.Now)
                {
                    result = FightGroupActiveStatus.Ending;
                }
                else
                {
                    if(StartTime>DateTime.Now)
                    {
                        result = FightGroupActiveStatus.WillStart;
                    }
                    else
                    {
                        result = FightGroupActiveStatus.Ongoing;
                    }
                }
                return result;
            }
        }
        /// <summary>
        /// 活动项
        /// <para>手动维护</para>
        /// </summary>
        [NotMapped]
        public List<FightGroupActiveItemInfo> ActiveItems { get; set; }
        /// <summary>
        /// 火拼价
        /// </summary>
        [NotMapped]
        public decimal MiniGroupPrice { get; set; }
        /// <summary>
        /// 最低售价
        /// </summary>
        [NotMapped]
        public decimal MiniSalePrice { get; set; }
        /// <summary>
        /// 运费模板
        /// </summary>
        [NotMapped]
        public long FreightTemplateId { get; set; }
        /// <summary>
        /// 商品广告语
        /// </summary>
        [NotMapped]
        public string ProductShortDescription { get; set; }
        /// <summary>
        /// 商品评价数
        /// </summary>
        [NotMapped]
        public int ProductCommentNumber { get; set; }
        /// <summary>
        /// 商品编码
        /// </summary>
        [NotMapped]
        public string ProductCode { get; set; }
        /// <summary>
        /// 商品单位
        /// </summary>
        [NotMapped]
        public string MeasureUnit { get; set; }
        /// <summary>
        /// 商品是否可购买
        /// </summary>
        [NotMapped]
        public bool CanBuy { get; set; }
        /// <summary>
        /// 商品是否还有库存
        /// </summary>
        [NotMapped]
        public bool HasStock { get; set; }

        public long SaleCounts { get; set; }
        /// <summary>
        /// 管理审核状态
        /// </summary>
        [NotMapped]
        public FightGroupManageAuditStatus FightGroupManageAuditStatus { get
            {
                FightGroupManageAuditStatus result = FightGroupManageAuditStatus.Normal;
                if(ManageAuditStatus==-1)
                {
                    result = FightGroupManageAuditStatus.SoldOut;
                }
                return result;
            }
        }
        [NotMapped]
        public List<ComboDetail> ComboList { get; set; }

        /// <summary>
        /// 商品默认图片
        /// </summary>
        [NotMapped]
        public string ProductDefaultImage { get; set; }
        /// <summary>
        /// 商品其他图片
        /// </summary>
        [NotMapped]
        public List<string> ProductImages { get; set; }

        /// <summary>
        /// 详情
        /// </summary>
        [NotMapped]
        public string ShowMobileDescription { get; set; }
    }
}
