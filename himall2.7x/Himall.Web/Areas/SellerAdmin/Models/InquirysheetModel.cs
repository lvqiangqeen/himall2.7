using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class InquirysheetModel
    {
        public IList<Product> products;
        public InquirysheetModel()
        {
            products = new List<Product>();
        }
        #region 报价单
        public long Id { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// 提交时间
        /// </summary>
        public string PublishDate { get; set; }

        /// <summary>
        /// 收获时间
        /// </summary>
        public string ShippingEndDate { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public string QuotationEndDate { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 状态名称
        /// </summary>
        public string StateName { get; set; }
        /// <summary>
        /// 报价要求
        /// </summary>
        public string ContainTax { get; set; }

        /// <summary>
        /// 发票要求
        /// </summary>
        public string InvoiceName { get; set; }

        /// <summary>
        /// 收货方式
        /// </summary>
        public string ReceivingName { get; set; }

        /// <summary>
        /// 收获地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 补充说明
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 报价总计
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// 报价说明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 联系人
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 资质要求
        /// </summary>
        public string QualificationRequirement { get; set; }


        public class Product
        {
            /// <summary>
            /// 编号
            /// </summary>
            public long Id { get; set; }
            /// <summary>
            /// 产品名称
            /// </summary>
            public string ProductName { get; set; }

            /// <summary>
            /// 所属行业
            /// </summary>
            public string Industry { get; set; }

            /// <summary>
            /// 采购量加单位
            /// </summary>
            public string Quantity { get; set; }

            /// <summary>
            /// 附件
            /// </summary>
            public string Annex { get; set; }

            /// <summary>
            /// 报价
            /// </summary>
            public decimal Price { get; set; }

            /// <summary>
            /// 产品描述
            /// </summary>
            public string ShortDescription { get; set; }
        }
                
        #endregion
       

    }
}