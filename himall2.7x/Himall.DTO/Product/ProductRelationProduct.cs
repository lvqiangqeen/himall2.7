using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
	public class ProductRelationProduct
	{
		public int Id { get; set; }
		
		public long ProductId { get; set; }

		/// <summary>
		/// 推荐商品id，以逗号分隔
		/// </summary>
		public string Relation { get; set; }

		/// <summary>
		/// 推荐商品id列表
		/// </summary>
		public long[] RelationProductIds
		{
			get
			{
				if (string.IsNullOrWhiteSpace(Relation))
					return new long[0];

				long temp;
				return Relation.Split(',').Where(p => long.TryParse(p, out temp)).Select(p => long.Parse(p)).ToArray();
			}
		}
	}
}
