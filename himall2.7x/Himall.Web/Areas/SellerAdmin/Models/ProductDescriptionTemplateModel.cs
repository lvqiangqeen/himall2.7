
using System.ComponentModel.DataAnnotations;
namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class ProductDescriptionTemplateModel
    {
        public long Id { get; set; }

        public string Name { get; set; }
        /// <summary>
        /// PC端版式
        /// </summary>
        public string HtmlContent { get; set; }
        /// <summary>
        /// 移动端版式
        /// </summary>
        public string MobileHtmlContent { get; set; }

        public int Position { get; set; }

        public string PositionText { get; set; }

    }
}