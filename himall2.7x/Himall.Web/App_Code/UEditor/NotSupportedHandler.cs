
namespace Himall.Web.App_Code.UEditor
{
    /// <summary>
    /// NotSupportedHandler 的摘要说明
    /// </summary>
    public class NotSupportedHandler : IUEditorHandle
    {
        public NotSupportedHandler() { }

        public object Process()
        {
            return (new
            {
                state = "action 参数为空或者 action 不被支持。"
            });
        }
    }
}