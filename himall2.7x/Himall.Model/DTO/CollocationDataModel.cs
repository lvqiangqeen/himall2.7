using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class CollocationDataModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ShortDesc { get; set; }
        public long ShopId { get; set; }
        public DateTime CreateTime { get; set; }
        public List<CollocationPoruductModel> CollocationPoruducts { get; set; }
    }


    public class CollocationPoruductModel
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string ImagePath { set; get; }
        public string ProductName { set; get; }
        public long ColloId { get; set; }
        public bool IsMain { get; set; }
        public int DisplaySequence { get; set; }
        public List<CollocationSkus> CollocationSkus { set; get; }
    }


    /// <summary>
    /// 
    /// </summary>
    public class CollocationSkus
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string SkuID { get; set; }
        public string SKUName { get; set; }
        public long ColloProductId { get; set; }
        public decimal Price { get; set; }
        public decimal SkuPirce { get; set; }
    }
}
