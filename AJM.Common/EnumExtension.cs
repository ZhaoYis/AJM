using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace AJM.Common
{
    /// <summary>
    /// 枚举操作扩展方法
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// 返回枚举项的描述信息。
        /// </summary>
        /// <param name="value">要获取描述信息的枚举项。</param>
        /// <returns>枚举想的描述信息。</returns>
        public static string GetEnumDescription(this Enum value)
        {
            Type enumType = value.GetType();
            // 获取枚举常数名称。
            string name = Enum.GetName(enumType, value);
            if (name != null)
            {
                // 获取枚举字段。
                FieldInfo fieldInfo = enumType.GetField(name);
                if (fieldInfo != null)
                {
                    // 获取描述的属性。
                    DescriptionAttribute attr = Attribute.GetCustomAttribute(fieldInfo,
                        typeof(DescriptionAttribute), false) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 遍历枚举对象的所有元素
        /// </summary>
        /// <typeparam name="T">枚举对象</typeparam>
        /// <returns>Dictionary：枚举值-描述</returns>
        public static Dictionary<int, string> GetEnumValues<T>()
        {
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            foreach (var code in System.Enum.GetValues(typeof(T)))
            {
                ////获取名称
                //string strName = System.Enum.GetName(typeof(T), code);

                object[] objAttrs = code.GetType().GetField(code.ToString()).GetCustomAttributes(typeof(TextAttribute), true);
                if (objAttrs.Length > 0)
                {
                    TextAttribute descAttr = objAttrs[0] as TextAttribute;
                    if (!dictionary.ContainsKey((int)code))
                    {
                        if (descAttr != null)
                            dictionary.Add((int)code, descAttr.Value);
                    }
                    //Console.WriteLine(string.Format("[{0}]", descAttr.Value));
                }
                //Console.WriteLine(string.Format("{0}={1}", code.ToString(), Convert.ToInt32(code)));
            }
            return dictionary;
        }
    }

    /// <summary>
    /// 自定义描述
    /// </summary>
    public class TextAttribute : Attribute
    {
        /// <summary>
        /// 描述信息
        /// </summary>
        public string Value { get; private set; }

        public TextAttribute(string value)
        {
            this.Value = value;
        }
    }
}