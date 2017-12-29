using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using AJM.Models;
using AJM.Services;
using Quartz;
using Quartz.Impl;

namespace AJM.Main
{
    /// <summary>
    /// 作业管理类
    /// </summary>
    public class JobManage : BaseJobManager
    {
        private IScheduler _sched = null;

        /// <summary>
        /// 开始所有作业调度
        /// </summary>
        public void JobStart()
        {
            //获取配置文件
            List<JobConfigEntity> configs = InitJobConfig();

            NameValueCollection properties = new NameValueCollection
            {
                ["author"] = "大师兄",
                ["version"] = "V1.0.0",
                ["createtime"] = "2017年12月29日"
            };

            ISchedulerFactory sf = new StdSchedulerFactory(properties);
            _sched = sf.GetScheduler();

            List<Type> allInheirtFromIJob = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IJob))).ToList();
            foreach (JobConfigEntity config in configs)
            {
                Type jobType = allInheirtFromIJob.FirstOrDefault(s => s.Name == config.JobName);
                if (jobType == null) continue;

                IJobDetail job = JobBuilder.Create(jobType)
                    .WithIdentity(config.JobIdentityName, config.JobGroup)
                    .Build();
                ICronTrigger trigger = (ICronTrigger)TriggerBuilder.Create()
                    .WithIdentity(config.TriggerIdentityName, config.JobGroup)
                    .WithCronSchedule(config.CronExpression)
                    .Build();

                foreach (PropertyInfo property in typeof(JobConfigEntity).GetProperties())
                {
                    job.JobDataMap.Put(property.Name, property.GetValue(config, null));
                }
                DateTimeOffset ft = _sched.ScheduleJob(job, trigger);
            }
            _sched.Start();
        }

        /// <summary>
        /// 停止所有作业调度
        /// </summary>
        public void JobStop()
        {
            if (_sched != null && _sched.IsStarted)
                _sched.Shutdown(true);
        }
    }
}