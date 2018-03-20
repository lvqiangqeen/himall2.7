using Himall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    public class FreightTemplate
    {

        public FreightTemplate()
        {
            FreightArea = new List<DTO.FreightArea>();
        }

        public  long Id { get; set; }
        public string Name { get; set; }
        public Nullable<int> SourceAddress { get; set; }
        public string SendTime { get; set; }
        public FreightTemplateType IsFree { get; set; }
        public ValuationMethodType ValuationMethod { get; set; }
        public Nullable<int> ShippingMethod { get; set; }
        public long ShopID { get; set; }
        public List<FreightArea> FreightArea { get; set; }
    }

    public class FreightArea
    {
        public FreightArea()
        {
            FreightAreaDetail = new List<DTO.FreightAreaDetail>();
        }
        public  long Id { get; set; }
        public long FreightTemplateId { get; set; }
        public string AreaContent { get; set; }
        public Nullable<int> FirstUnit { get; set; }
        public Nullable<float> FirstUnitMonry { get; set; }
        public Nullable<int> AccumulationUnit { get; set; }
        public Nullable<float> AccumulationUnitMoney { get; set; }
        public Nullable<sbyte> IsDefault { get; set; }
        public List<FreightAreaDetail> FreightAreaDetail { get; set; }
    }

    public class FreightAreaDetail
    {
        public long Id { get; set; }
        /// <summary>
        /// 运费模板ID
        /// </summary>
        public long FreightTemplateId { get; set; }
       /// <summary>
       /// 运费区域ID
       /// </summary>
        public long FreightAreaId { get; set; }
        /// <summary>
        /// 省会ID
        /// </summary>
        public int ProvinceId { get; set; }

        /// <summary>
        /// 省会名称
        /// </summary>
        public string ProvinceName { set; get; }
        /// <summary>
        /// 市ID
        /// </summary>
        public Nullable<int> CityId { get; set; }

        /// <summary>
        /// 市区名称
        /// </summary>
        public string CityName { set; get; }
        /// <summary>
        /// 县/区ID
        /// </summary>
        public Nullable<int> CountyId { get; set; }

       /// <summary>
       /// 县区名称
       /// </summary>
        public string CountyName { set; get; }

        /// <summary>
        /// 乡镇ID
        /// </summary>
        public string TownIds { get; set; }

        /// <summary>
        /// 乡镇名称
        /// </summary>
        public string TownNames { set; get; }
    }
}
