using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{

    /// <summary>
    /// 发票类型
    /// </summary>
    public enum InvoiceType
    {
        /// <summary>
        /// 不需要发票
        /// </summary>
        [Description("不需要发票")]
        None = 0,
        /// <summary>
        /// 增值税发票
        /// </summary>
        [Description("增值税发票")]
        VATInvoice = 1,

        /// <summary>
        /// 普通发票
        /// </summary>
        [Description("普通发票")]
        OrdinaryInvoices = 2
    }
}
