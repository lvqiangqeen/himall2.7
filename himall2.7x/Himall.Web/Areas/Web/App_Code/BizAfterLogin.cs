
using Himall.Web;
using Autofac;
namespace Himall.Web.Areas.Web
{
    public static class BizAfterLogin
    {
        public static void Run(long memberId)
        {
            CartHelper cart = new CartHelper();
            //同步客户端购物车信息至服务器
            cart.UpdateCartInfoFromCookieToServer(memberId);
        }

    }
}