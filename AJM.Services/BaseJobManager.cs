using System;
using System.Collections.Generic;
using AJM.Cache;
using AJM.Common;
using AJM.Interface;
using AJM.Models;

namespace AJM.Services
{
    /// <summary>
    /// 公共基类
    /// </summary>
    public class BaseJobManager : IBaseJobManager
    {
        /// <summary>
        /// 初始化任务配置文件
        /// </summary>
        /// <returns></returns>
        public List<JobConfigEntity> InitJobConfig()
        {
            List<JobConfigEntity> res = CacheFactory.GetCache().GetCache<List<JobConfigEntity>>("JobConfig_Cache_Key");
            if (res == null)
            {
                string filePath = CommonHelper.GetBaseDirectory("XmlConfig\\JobConfig.xml");
                res = CommonHelper.ConvertXmlToObject<JobConfigEntity>(filePath, "AJMSettings");
                CacheFactory.GetCache().WriteCache(res, "JobConfig_Cache_Key", DateTime.Now.AddDays(1));
            }
            return res;
        }
    }
}