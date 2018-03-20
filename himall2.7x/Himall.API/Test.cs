using Himall.Web.Framework;
using System;
using System.Collections;
using System.Web.Http;
using Himall.Model;

namespace Himall.API
{
    [RoutePrefix("Api")]
    public class TestController : HiAPIController<UserMemberInfo>
    {
        [Route("Test.Get")]
        public string Get()
        {
            return "欢迎使用Himall API服务";
        }

        public IEnumerable GetArray()
        {
            return new[] { new { title = "this is title", time = DateTime.Now }, new { title = "this is title", time = DateTime.Now } };
        }


		protected override UserMemberInfo GetUser()
		{
			throw new NotImplementedException();
		}
	}
}
