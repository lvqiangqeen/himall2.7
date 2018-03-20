using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.DTO
{
	public class MemberOpenId
	{
		public new long Id { get; set; }
		public long UserId { get; set; }
		public string OpenId { get; set; }
		public string ServiceProvider { get; set; }
		public MemberOpenIdInfo.AppIdTypeEnum AppIdType { get; set; }
		public string UnionId { get; set; }
		public string UnionOpenId { get; set; }
	}
}
