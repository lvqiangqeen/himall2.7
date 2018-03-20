using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Hishop.Open.Api;
using Himall.OpenApi.Model;
using Himall.OpenApi.Model.Parameter;

namespace Himall.OpenApi
{
    /// <summary>
    /// 商品控制器
    /// <note>不能命名重复</note>
    /// </summary>
    [RoutePrefix("OpenApi")]
    public class ProductController : HiOpenAPIController
    {
        private ProductHelper _productHeelper { get; set; }
        public ProductController()
        {
            _productHeelper = new ProductHelper();
        }

        /// <summary>
        /// 获取指定商品的详情信息
        /// </summary>
        public object GetProduct([FromUri]GetProductParameterModel para)
        {
            #region 参数初始
            if (para == null)
            {
                para = new GetProductParameterModel();
            }
            para.ValueInit();
            para.CheckParameter();
            #endregion

            var model = _productHeelper.GetProduct(para.num_iid, para.app_key);

            var result = new { product_get_response = new { item = model } };

            return result;
        }

        /// <summary>
        /// 获取当前商家的商品列表
        /// </summary>
        /// <param name="start_modified"></param>
        /// <param name="end_modified"></param>
        /// <param name="approve_status"></param>
        /// <param name="q"></param>
        /// <param name="order_by"></param>
        /// <param name="page_no"></param>
        /// <param name="page_size"></param>
        /// <param name="app_key"></param>
        /// <param name="timestamp"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        public object GetSoldProducts([FromUri]GetSoldProductsParameterModel para)
        {
            #region 参数初始
            if (para==null)
            {
                para = new GetSoldProductsParameterModel();
            }
            para.ValueInit();
            para.CheckParameter();
            #endregion

            var data = _productHeelper.GetSoldProducts(para.start_modified, para.end_modified, para.approve_status, para.q, para.order_by, para.page_no.Value, para.page_size.Value, para.app_key);
            List<product_list_model> datalist = new List<product_list_model>();
            if (data.Total > 0)
            {
                datalist = data.Models.ToList();
            }
            var result = new { products_get_response = new { total_results = data.Total, items = datalist } };

            return result;
        }

        /// <summary>
        /// 商品/SKU库存修改(提供按照全量或增量形式修改宝贝/SKU库存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public object UpdateProductQuantity(UpdateProductQuantityParameterModel para)
        {
            #region 参数初始
            if(para==null)
            {
                para = new UpdateProductQuantityParameterModel();
            }
            para.ValueInit();
            para.CheckParameter();
            #endregion;

            var model = _productHeelper.UpdateProductQuantity(para.num_iid, para.sku_id, para.quantity, para.type.Value, para.app_key);

            var result = new { product_quantity_update_response = new { item = model } };

            return result;
        }

        /// <summary>
        /// 修改商品销售状态 (上架， 下架， 入库)
        /// </summary>
        [HttpPost]
        public object UpdateProductApproveStatus(UpdateProductApproveStatusParameterModel para)
        {
            #region 参数初始
            if (para == null)
            {
                para = new UpdateProductApproveStatusParameterModel();
            }
            para.ValueInit();
            para.CheckParameter();
            #endregion;

            var model = _productHeelper.UpdateProductApproveStatus(para.num_iid, para.approve_status, para.app_key);

            var result = new { product_quantity_update_response = new { item = model } };

            return result;
        }

    }
}
