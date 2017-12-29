using System;
using System.Reflection;
using AJM.Models;
using Quartz;

namespace AJM.Main.Base
{
    /// <summary>
    /// 子任务基类
    /// </summary>
    public class BaseJob
    {
        /// <summary>
        /// 从作业数据地图中获取配置信息
        /// </summary>
        /// <param name="datamap">作业数据地图</param>
        /// <returns></returns>
        protected static JobConfigEntity GetConfigFromDataMap(IJobExecutionContext context)
        {
            JobConfigEntity config = new JobConfigEntity();

            //获取JobDataMap
            JobDataMap datamap = context.JobDetail.JobDataMap;
            //获取JobConfigEntity公共属性
            PropertyInfo[] properties = typeof(JobConfigEntity).GetProperties();
            foreach (PropertyInfo info in properties)
            {
                if (info.PropertyType == typeof(string))
                {
                    info.SetValue(config, datamap.GetString(info.Name), null);
                }
                else if (info.PropertyType == typeof(Int32))
                {
                    info.SetValue(config, datamap.GetInt(info.Name), null);
                }
            }
            return config;
        }

        /// <summary>
        /// 设置JobDataMap
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="context">上下文对象</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        protected void SetJobDataMap<T>(IJobExecutionContext context, string key, T value) where T : class
        {
            context.JobDetail.JobDataMap[key] = value;
        }

        /// <summary>
        /// 获取JobDataMap
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="context">上下文对象</param>
        /// <param name="key">键</param>
        /// <returns></returns>
        protected T GetJobDataMap<T>(IJobExecutionContext context, string key) where T : class
        {
            return context.JobDetail.JobDataMap[key] as T;
        }
    }
}
