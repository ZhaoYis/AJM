using System;
using System.Configuration;

namespace AJM.Common
{
    /// <summary>
    /// 操作配置文件帮助类
    /// </summary>
    public class ConfigHelper
    {
        /// <summary>
        /// 根据Key取Value值
        /// </summary>
        /// <param name="key"></param>
        public static string GetValue(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key].ToString().Trim();
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// 根据Key修改Value
        /// </summary>
        /// <param name="key">要修改的Key</param>
        /// <param name="value">要修改为的值</param>
        public static void SetValue(string key, string value)
        {
            System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
            //获取自定义配置文件路径
            string filePath = CommonHelper.GetBaseDirectory("/XmlConfig/system.config");
            xDoc.Load(filePath);

            System.Xml.XmlNode xNode = xDoc.SelectSingleNode("//appSettings");

            if (xNode != null)
            {
                System.Xml.XmlElement xElem1;
                System.Xml.XmlElement xElem2;

                xElem1 = (System.Xml.XmlElement)xNode.SelectSingleNode("//add[@key='" + key + "']");
                if (xElem1 != null)
                {
                    xElem1.SetAttribute("value", value);
                }
                else
                {
                    xElem2 = xDoc.CreateElement("add");
                    xElem2.SetAttribute("key", key);
                    xElem2.SetAttribute("value", value);
                    xNode.AppendChild(xElem2);
                }
            }
            xDoc.Save(filePath);
        }
    }
}