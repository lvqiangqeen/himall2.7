using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Himall.Model;

namespace Himall.ViewModel
{
    public class OrderRefundModel
    {
        public long RefundId { get; set; }
        public long OrderId { get;set; }

        public string AuditStatus { get; set; }

        public string ConfirmStatus { get; set; }

        public string ApplyDate { get; set; }
        
        public long ShopId { get;set; }

        public string ShopName { get;set; }

        public long UserId { get;set; }

        public string UserName { get;set; }

        public string ContactPerson { get; set; }

        public string ContactCellPhone { get;set; }

        public string RefundAccount { get; set; }

        public string Amount { get; set; }

        public long ReturnQuantity { get; set; }

        public long Quantity { get; set; }

        public string SalePrice { get; set; }

        public string ProductName { get; set; }

        public string Reason { get; set; }

        public string ExpressCompanyName { get; set; }

        public string ShipOrderNumber { get; set; }

        public string Payee { get; set; }

        public string PayeeAccount { get;set;}

        public long ProductId { get; set; }

        public string ThumbnailsUrl { get; set; }

        public int RefundMode { get; set; }

        public string ManagerRemark { get; set; }
        public string SellerRemark { get; set; }

        /// <summary>
        /// 退款状态
        /// </summary>
        public string RefundStatus{ get; set; }

        public string RefundPayType { get; set; }
        public int RefundPayStatus { get; set; }
        public int ApplyNumber { get; set; }
        /// <summary>
        /// 下一阶段剩余秒数
        /// <para>-999表示不自动执行 -1表示己过期</para>
        /// </summary>
        public double nextSecond { get; set; }
        public string CertPic1 { get; set; }
        public string CertPic2 { get; set; }
        public string CertPic3 { get; set; }
    }
}