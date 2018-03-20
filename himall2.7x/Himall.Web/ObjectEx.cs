using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Himall.Core;

namespace Himall.Web
{
	public static class ObjectEx
	{
		/// <summary>
		/// 转换为下拉框列表项
		/// </summary>
		/// <param name="enumItem"></param>
		/// <param name="useName">值是否使用枚举项的名称</param>
		/// <returns></returns>
		public static List<SelectListItem> ToSelectList(this Enum enumItem, bool useName = false)
		{
			var names = Enum.GetNames(enumItem.GetType());

			return names.Select(p =>
			{
				var item = Enum.Parse(enumItem.GetType(), p);
				return new SelectListItem
				{
					Text = ((Enum)item).ToDescription(),
					Value = useName ? item.ToString() : ((int)item).ToString()
				};
			}).ToList();
		}
	}
}