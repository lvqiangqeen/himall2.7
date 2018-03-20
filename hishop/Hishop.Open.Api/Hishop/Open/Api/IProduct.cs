namespace Hishop.Open.Api
{
    using System;

    public interface IProduct
    {
        string GetProduct(int num_iid);
        string GetSoldProducts(DateTime? start_modified, DateTime? end_modified, string approve_status, string q, string order_by, int page_no, int page_size);
        string UpdateProductApproveStatus(int num_iid, string approve_status);
        string UpdateProductQuantity(int num_iid, string sku_id, int quantity, int type);
    }
}

