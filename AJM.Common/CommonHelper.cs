using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace AJM.Common
{
    /// <summary>
    /// 公共方法帮助类
    /// </summary>
    public class CommonHelper
    {
        #region 将XML内容转换成目标对象实体集合
        /// <summary>
        /// 将XML内容转换成目标对象实体集合
        /// </summary>
        /// <typeparam name="T">目标对象实体</typeparam>
        /// <param name="fileName">完整文件名(根目录下只需文件名称)</param>
        /// <param name="wrapperNodeName">主节点名称</param>
        /// <returns></returns>
        public static List<T> ConvertXmlToObject<T>(string fileName, string wrapperNodeName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            List<T> result = new List<T>();
            Type type = typeof(T);
            XmlNodeList nodeList = doc.ChildNodes;
            if (!string.IsNullOrEmpty(wrapperNodeName))
            {
                foreach (XmlNode node in doc.ChildNodes)
                {
                    if (node.Name == wrapperNodeName)
                    {
                        nodeList = node.ChildNodes;
                        break;
                    }
                }
            }
            object oneT = null;
            foreach (XmlNode node in nodeList)
            {
                if (node.NodeType == XmlNodeType.Comment || node.NodeType == XmlNodeType.XmlDeclaration) continue;
                oneT = type.Assembly.CreateInstance(type.FullName ?? throw new InvalidOperationException());
                foreach (XmlNode item in node.ChildNodes)
                {
                    if (item.NodeType == XmlNodeType.Comment) continue;
                    var property = type.GetProperty(item.Name);
                    if (property != null)
                        property.SetValue(oneT, Convert.ChangeType(item.InnerText, property.PropertyType), null);
                }
                result.Add((T)oneT);
            }
            return result;
        }
        #endregion

        #region 获取基目录

        /// <summary>
        /// 获取基目录
        /// </summary>
        /// <returns></returns>
        public static string GetBaseDirectory(string path)
        {
            try
            {
                return AppDomain.CurrentDomain.BaseDirectory + path;
            }
            catch (Exception)
            {
                return Path.GetFullPath(path);
            }
        }

        #endregion

    }
}