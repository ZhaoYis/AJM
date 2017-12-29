using System;
using System.Diagnostics;
using AJM.Common;
using AJM.Main.Base;
using AJM.Models;
using Quartz;

namespace AJM.Main.Job
{
    /// <summary>
    /// 测试任务
    /// </summary>
    public class TestJob : BaseJob, IJob
    {
        #region Attribute
        /// <summary>
        /// 当前作业配置信息
        /// </summary>
        private JobConfigEntity _config = null;
        #endregion

        /// <summary>
        /// 作业执行入口
        /// </summary>
        /// <param name="context">作业执行上下文</param>
        public void Execute(IJobExecutionContext context)
        {
            _config = GetConfigFromDataMap(context);
            //Do Something

            this.SetJobDataMap(context, "", "");

            Trace.WriteLine(_config.Name + "=============执行时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}