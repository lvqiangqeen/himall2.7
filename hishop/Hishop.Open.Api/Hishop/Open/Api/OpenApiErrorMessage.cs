namespace Hishop.Open.Api
{
    using System;
    using System.ComponentModel;

    public static class OpenApiErrorMessage
    {
        public static string GetEnumDescription(Enum enumSubitem)
        {
            string name = enumSubitem.ToString();
            object[] customAttributes = enumSubitem.GetType().GetField(name).GetCustomAttributes(typeof(DescriptionAttribute), false);
            if ((customAttributes == null) || (customAttributes.Length == 0))
            {
                return name;
            }
            DescriptionAttribute attribute = (DescriptionAttribute) customAttributes[0];
            return attribute.Description;
        }

        public static string ShowErrorMsg(Enum enumSubitem, string fields)
        {
            string str = GetEnumDescription(enumSubitem).Replace("_", " ");
            string format = "{{\"error_response\":{{\"code\":\"{0}\",\"msg\":\"{1}:{2}\",\"sub_msg\":\"{3}\"}}}}";
            return string.Format(format, new object[] { Convert.ToInt16(enumSubitem).ToString(), enumSubitem.ToString().Replace("_", " "), fields, str });
        }
    }
}

