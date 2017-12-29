using System.Collections.Generic;
using AJM.Models;

namespace AJM.Interface
{
    /// <summary>
    /// 公共基类接口
    /// </summary>
    public interface IBaseJobManager
    {
        /// <summary>
        /// 初始化任务配置文件
        /// </summary>
        /// <returns></returns>
        List<JobConfigEntity> InitJobConfig();
        
    }
}