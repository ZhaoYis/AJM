namespace AJM.Models
{
    /// <summary>
    /// 配置文件实体
    /// </summary>
    public class JobConfigEntity
    {
        /// <summary>
        /// 设置项名称(唯一标识)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 枚举码(唯一标识)
        /// </summary>
        public string EnumCode { get; set; }
        /// <summary>
        /// 作业名称
        /// </summary>
        public string JobName { get; set; }
        /// <summary>
        /// 作业分组名称
        /// </summary>
        public string JobGroup { get; set; }
        /// <summary>
        /// 作业身份名称
        /// </summary>
        public string JobIdentityName { get; set; }
        /// <summary>
        /// 触发器身份名称
        /// </summary>
        public string TriggerIdentityName { get; set; }
        /// <summary>
        /// 复杂任务Cron表达式
        /// </summary>
        public string CronExpression { get; set; }
        /// <summary>
        /// 跳过日期，格式：yyyyMMdd
        /// </summary>
        public string SkipDate { get; set; }
    }
}