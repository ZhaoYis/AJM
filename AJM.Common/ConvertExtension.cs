using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AJM.Common
{
    /// <summary>
    /// 数据类型转换扩展方法
    /// </summary>
    public static class ConvertExtension
    {
        #region 类型转换
        /// <summary>
        /// object 转换成string 包括为空的情况
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>返回值不含空格</returns>
        public static string ToStringEx(this object obj)
        {
            return obj == null ? string.Empty : obj.ToString().Trim();
        }

        /// <summary>
        /// 时间object 转换成格式化的string 包括为空的情况
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="format"></param>
        /// <returns>返回值不含空格</returns>
        public static string TryToDateTimeToString(this object obj, string format)
        {
            if (obj == null)
                return string.Empty;
            DateTime dt;
            if (DateTime.TryParse(obj.ToString(), out dt))
                return dt.ToString(format);
            else
                return string.Empty;
        }

        /// <summary>
        /// 字符转Int
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>成功:返回对应Int值;失败:返回0</returns>
        public static int TryToInt32(this object obj)
        {
            int rel = 0;

            if (!string.IsNullOrEmpty(obj.ToStringEx()))
            {
                int.TryParse(obj.ToStringEx(), out rel);
            }
            return rel;
        }

        /// <summary>
        /// 字符转Int64
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Int64 TryToInt64(this object obj)
        {
            Int64 rel = 0;
            if (!string.IsNullOrEmpty(obj.ToStringEx()))
            {
                Int64.TryParse(obj.ToStringEx(), out rel);
            }
            return rel;
        }

        /// <summary>
        /// 字符转DateTime
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>成功:返回对应Int值;失败:时间初始值</returns>
        public static DateTime TryToDateTime(this object obj)
        {
            DateTime rel = new DateTime();
            if (!string.IsNullOrEmpty(obj.ToStringEx()))
            {
                DateTime.TryParse(obj.ToStringEx(), out rel);
            }
            return rel;
        }

        /// <summary>
        /// 转换成bool类型
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Boolean TryToBoolean(this object obj)
        {
            Boolean rel = false;
            if (!string.IsNullOrEmpty(obj.ToStringEx()))
            {
                string s = obj.ToStringEx();

                if (s.Equals("true") || s.Equals("1") || s.Equals("是"))
                {
                    rel = true;
                }
                else
                {
                    Boolean.TryParse(obj.ToStringEx(), out rel);
                }
            }
            return rel;
        } 
        #endregion

        #region 将对象属性转换为Key-Value键值对
        /// <summary>
        /// 将对象属性转换为Key-Value键值对
        /// </summary>  
        /// <param name="o"></param>  
        /// <returns></returns>  
        public static Dictionary<string, object> Object2Dictionary(this object o)
        {
            Dictionary<string, object> map = new Dictionary<string, object>();

            Type t = o.GetType();

            PropertyInfo[] pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo p in pi)
            {
                MethodInfo mi = p.GetGetMethod();

                if (mi != null && mi.IsPublic)
                {
                    map.Add(p.Name, mi.Invoke(o, new object[] { }));
                }
            }
            return map;
        }
        #endregion

        #region 把对象类型转换成指定的类型，转化失败时返回类型默认值

        /// <summary>
        /// 把对象类型转换成指定的类型，转化失败时返回指定默认值
        /// </summary>
        /// <typeparam name="T">动态类型</typeparam>
        /// <param name="value">要转换的原对象</param>
        /// <param name="detaultValue">转换失败时返回的默认值</param>
        /// <returns>转化后指定类型对象，转化失败时返回指定默认值</returns>
        public static T CastTo<T>(this object value, T detaultValue)
        {
            object result;
            Type t = typeof(T);
            try
            {
                result = t.IsEnum ? System.Enum.Parse(t, value.ToString()) : Convert.ChangeType(value, t);

            }
            catch (Exception)
            {
                return detaultValue;
            }

            return (T)result;
        }

        /// <summary>
        /// 把对象类型转换成指定的类型，转化失败时返回类型默认值
        /// </summary>
        /// <typeparam name="T">动态类型</typeparam>
        /// <param name="value">要转换的原对象</param>
        /// <returns>转化后指定类型对象，转化失败时返回类型默认值</returns>
        public static T CastTo<T>(this object value)
        {
            object result;
            Type t = typeof(T);
            try
            {
                if (t.IsEnum)
                {
                    result = System.Enum.Parse(t, value.ToString());
                }
                else if (t == typeof(Guid))
                {
                    result = Guid.Parse(value.ToString());
                }
                else
                {
                    result = Convert.ChangeType(value, t);
                }

            }
            catch (Exception)
            {
                result = default(T);
            }

            return (T)result;
        }
        #endregion

        #region 合并序列、数组去重
        /// <summary>
        /// 合并两个序列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a">原数组</param>
        /// <param name="b">需要合并的数组</param>
        /// <returns></returns>
        public static T[] ConcatArray<T>(this T[] a, T[] b)
        {
            return a.Concat(b).ToArray();
        }

        /// <summary>
        /// 数组去掉重复的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">要处理的数组</param>
        /// <returns></returns>
        public static T[] DeleteRepeatData<T>(this T[] array)
        {
            return array.GroupBy(p => p).Select(p => p.Key).ToArray();
        }
        #endregion

        #region 补足位数
        /// <summary>
        /// 指定字符串的固定长度，如果字符串小于固定长度，
        /// 则在字符串的前面补足零，可设置的固定长度最大为9位
        /// </summary>
        /// <param name="text">原始字符串</param>
        /// <param name="limitedLength">字符串的固定长度</param>
        public static string RepairZero(this string text, int limitedLength)
        {
            //补足0的字符串
            string temp = "";

            //补足0
            for (int i = 0; i < limitedLength - text.Length; i++)
            {
                temp += "0";
            }

            //连接text
            temp += text;

            //返回补足0的字符串
            return temp;
        }

        /// <summary>
        /// 小时、分钟、秒小于10补足0
        /// </summary>
        /// <param name="text">原始字符串</param>
        /// <returns></returns>
        public static string RepairZero(this int text)
        {
            string res = string.Empty;
            if (text >= 0 && text < 10)
            {
                res += "0" + text;
            }
            else
            {
                res = text.ToString();
            }
            return res;
        }

        #endregion
    }
}