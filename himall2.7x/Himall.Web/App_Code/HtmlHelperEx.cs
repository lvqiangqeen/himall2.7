using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Himall.Web.App_Code;

namespace System.Web.Mvc.Html
{
	public static class HtmlHelperEx
	{
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TModel"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="html"></param>
		/// <param name="expression"></param>
		/// <returns>返回true表示验证通过</returns>
		public static bool IsValid<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
		{
			var controller = (html.ViewContext.Controller as Controller);
			if (controller == null)
				return true;

			var modelState = controller.ModelState;
			if (modelState == null || modelState.Count == 0)
				return true;

			var memeber = ToModelStateKey(expression).TrimStart('.');

			if (!modelState.ContainsKey(memeber))
				return true;

			if (modelState[memeber].Errors.Count > 0)
				return false;

			return true;
		}

		private static string ToModelStateKey(Expression expression)
		{
			if (expression.NodeType == ExpressionType.Lambda)
				return ToModelStateKey(((LambdaExpression)expression).Body);

			if (expression.NodeType == ExpressionType.MemberAccess)
			{
				var memberExpression = (MemberExpression)expression;

				return string.Format("{0}.{1}", ToModelStateKey(memberExpression.Expression), memberExpression.Member.Name);
			}

			if (expression.NodeType == ExpressionType.ArrayIndex)
			{
				var leftExpression = (Expression)((dynamic)expression).Left;
				var rightExpression = (Expression)((dynamic)expression).Right;

				object value;
				if (rightExpression.NodeType == ExpressionType.MemberAccess)
				{
					var memberExpression = (MemberExpression)rightExpression;
					if (memberExpression.Expression.NodeType == ExpressionType.Constant)
					{
						if (memberExpression.Member is System.Reflection.FieldInfo)
							value = ((System.Reflection.FieldInfo)memberExpression.Member).GetValue(((ConstantExpression)memberExpression.Expression).Value);
						else if (memberExpression.Member is System.Reflection.PropertyInfo)
							value = ((System.Reflection.PropertyInfo)memberExpression.Member).GetValue(((ConstantExpression)memberExpression.Expression).Value);
						else
							value = memberExpression.Member.DeclaringType.GetProperty(memberExpression.Member.Name).GetValue(((ConstantExpression)memberExpression.Expression).Value);
					}
					else
						value = ToModelStateKey(memberExpression);
				}
				else
					value = ToModelStateKey(rightExpression);

				return string.Format("{0}[{1}]", ToModelStateKey(leftExpression), value);
			}

			if (expression.NodeType == ExpressionType.Constant)
				return ((ConstantExpression)expression).Value.ToString();

			return "";
		}
	}
}