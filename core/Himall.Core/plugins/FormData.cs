using System.Collections.Generic;

namespace Himall.Core.Plugins
{
    public class FormData
    {
        public IEnumerable<FormItem> Items { get; set; }


        public class FormItem
        {

            /// <summary>
            /// 表单项名称(对应name属性)
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 表单项显示名称
            /// </summary>
            public string DisplayName { get; set; }

            /// <summary>
            /// 是否为必填项
            /// </summary>
            public bool IsRequired { get; set; }

            /// <summary>
            /// 表单项类型
            /// </summary>
            public FormItemType Type { get; set; }

            /// <summary>
            /// 表单项的值
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// 表单项类型
        /// </summary>
        public enum FormItemType
        {
            /// <summary>
            /// 对应input中type="text"
            /// </summary>
            text = 1,

            checkbox,

            password,
        }
    }
}
