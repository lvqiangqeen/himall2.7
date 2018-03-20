using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Admin.Models
{
    public class GiftViewModel
    {
        [Required]
        public long Id { get; set; }
        [Required(ErrorMessage="请填写礼品名称")]
        [MaxLength(100,ErrorMessage="超过100字符")]
        public string GiftName { get; set; }
        [Required(ErrorMessage = "请填写所需积分")]
        [Range(1, int.MaxValue, ErrorMessage = "积分必须大于0")]
        public int NeedIntegral { get; set; }
        [Required(ErrorMessage = "请填写限兑数量,0表不限")]
        [Range(0,int.MaxValue,ErrorMessage="限兑数量有误,0表示不限")]
        public int LimtQuantity { get; set; }
        [Required(ErrorMessage = "请填写库存数量")]
        [Range(0, int.MaxValue, ErrorMessage = "库存必须大于0")]
        public int StockQuantity { get; set; }
        [Required(ErrorMessage = "请填写兑换截止时间")]
        public System.DateTime EndDate { get; set; }
        [Required(ErrorMessage = "请选择会员等级要求")]
        public int NeedGrade { get; set; }
        [Required(ErrorMessage = "请填写虚拟销量")]
        public int VirtualSales { get; set; }
        public int RealSales { get; set; }
        public Model.GiftInfo.GiftSalesStatus SalesStatus { get; set; }
        public string ImagePath { get; set; }
        public int Sequence { get; set; }
        [Required(ErrorMessage = "请填写礼品价值")]
        [Range(0.01,99999.99,ErrorMessage="礼品价值应在0.01-99999之间")]
        public Nullable<decimal> GiftValue { get; set; }
        [Required(ErrorMessage = "请填写礼品描述")]
        public string Description { get; set; }
        public Nullable<System.DateTime> AddDate { get; set; }
        public string PicUrl1 { get; set; }
        public string PicUrl2 { get; set; }
        public string PicUrl3 { get; set; }
        public string PicUrl4 { get; set; }
        public string PicUrl5 { get; set; }
    }
}