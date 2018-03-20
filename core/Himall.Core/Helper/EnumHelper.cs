using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Himall.Core
{
    public  static class EnumHelper
    {

        /// <summary>
        /// 根据枚举类型和枚举值获取枚举描述
        /// </summary>
        /// <returns></returns>
        public static string ToDescription(this System.Enum value)
        {
            if (value == null) return "";

            Type type = value.GetType();
            string enumText = System.Enum.GetName(type, value);
            return GetDescription(type, enumText);
        }



        #region 内部成员

        /// <summary>
        /// 转化枚举及其描述为字典类型
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="enumObj"></param>
        /// <returns></returns>
        public static Dictionary<int,string> ToDescriptionDictionary<TEnum>()
        {   
            Type type=typeof(TEnum);
            var values =Enum.GetValues(type);
             var  enums = new Dictionary<int, string>();
            foreach(Enum item in values )
            {
             enums.Add(Convert.ToInt32(item) ,item.ToDescription());
            }
            
            return enums;
        }

        /// <summary>
        /// 转化枚举及其Text值转为字典类型
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="enumObj"></param>
        /// <returns></returns>
        public static Dictionary<int, string> ToDictionary<TEnum>()
        {
            Type type = typeof(TEnum);
            var values = Enum.GetValues(type);
            var enums = new Dictionary<int, string>();
            foreach (Enum item in values)
            {
                enums.Add(Convert.ToInt32(item), item.ToString());
            }

            return enums;
        }

        private static bool IsIntType(double d)
        {
            return (int)d != d;
        }

        //public static SelectList ToSelectList<TEnum>(this TEnum enumObj,bool perfix=true,bool onlyFlag=false)
        //{
        //    var values = from TEnum e in Enum.GetValues(typeof(TEnum))
        //                 select new { Id = Convert.ToInt32(e), Name = GetDescription(typeof(TEnum),e.ToString()) };
        //    //if (onlyFlag)
        //    //    values = values.Where(v => IsIntType(Math.Log(v.Id, 2)));
        //   var t= values.ToList();
        //   var item = new { Id = 0, Name = "请选择..." };
        //   if (perfix)
        //   {
        //       t.Insert(0, item);
        //   }
        // return  new SelectList(t, "Id", "Name", enumObj);
        //} 

        static Hashtable enumDesciption = GetDescriptionContainer();

        static Hashtable GetDescriptionContainer()
        {
            enumDesciption = new Hashtable();
            return enumDesciption;
        }

        static void AddToEnumDescription(Type enumType)
        {
            enumDesciption.Add(enumType, GetEnumDic(enumType));
        }


        ///<summary>
        /// 返回 Dic&lt;枚举项，描述&gt;
        ///</summary>
        ///<param name="enumType">枚举的类型</param>
        ///<returns>Dic&lt;枚举项，描述&gt;</returns>
        static Dictionary<string, string> GetEnumDic(Type enumType)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            FieldInfo[] fieldinfos = enumType.GetFields();
            foreach (FieldInfo field in fieldinfos)
            {
                if (field.FieldType.IsEnum)
                {
                    Object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    dic.Add(field.Name, ((DescriptionAttribute)objs[0]).Description);
                }

            }

            return dic;
        }

        /// <summary>
        /// 根据枚举类型和枚举值获取枚举描述
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <param name="enumText">枚举值</param>
        /// <returns></returns>
        static string GetDescription(Type enumType, string enumText)
        {
            if (String.IsNullOrEmpty(enumText))
                return null;
            if (!enumDesciption.ContainsKey(enumType))
                AddToEnumDescription(enumType);

            object value = enumDesciption[enumType];

            if (value != null && !String.IsNullOrEmpty(enumText))
            {
                Dictionary<string, string> description = (Dictionary<string, string>)value;
                return description[enumText];
            }
            else
                throw new ApplicationException("不存在枚举的描述");

        }

        #endregion
    }
}
