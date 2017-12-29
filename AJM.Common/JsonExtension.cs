using System.Collections.Generic;
using Newtonsoft.Json;

namespace AJM.Common
{
    /// <summary>
    /// Json操作扩展方法
    /// </summary>
    public static class JsonExtension
    {
        /// <summary>
        /// Json字符串反序列化成对象
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static object JsonToObject(this string json)
        {
            return JsonConvert.DeserializeObject(json);
        }

        /// <summary>
        /// Json字符串反序列化成实体对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T JsonToObject<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Json字符串反序列化成对象集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static List<T> JsonToList<T>(this string json)
        {
            return JsonConvert.DeserializeObject<List<T>>(json);
        }
        
        /// <summary>
        /// 对象序列化成Json字符串
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        /// <param name="isIgnoreNullValue">是否忽略值为NULL的属性，默认false</param>
        /// <returns></returns>
        public static string TryToJson(this object obj, bool isIgnoreNullValue = false)
        {
            string res;
            if (isIgnoreNullValue)
            {
                JsonSerializerSettings jsetting = new JsonSerializerSettings();

                JsonConvert.DefaultSettings = () =>
                {
                    //日期类型默认格式化处理
                    //jsetting.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
                    //jsetting.DateFormatString = "yyyy-MM-dd HH:mm:ss";

                    //空值处理,忽略值为NULL的属性
                    jsetting.NullValueHandling = NullValueHandling.Ignore;

                    return jsetting;
                };
                res = JsonConvert.SerializeObject(obj, Formatting.Indented, jsetting);
            }
            else
            {
                res = JsonConvert.SerializeObject(obj);
            }
            return res;
        }
        
    }
}