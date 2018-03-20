
namespace Himall.IServices.QueryModel
{
    public partial class VshopQuery : QueryBase
    {
        public string Name { get; set; }

        public Himall.Model.VShopExtendInfo.VShopExtendType? VshopType {get;set;}

        public Himall.Model.VShopExtendInfo.VShopExtendState VshopState { get; set; }

        /// <summary>
        /// 要排除的VshopId
        /// </summary>
        public long? ExcepetVshopId { set; get; }

    }
}
