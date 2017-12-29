using AJM.Interface;

namespace AJM.Cache
{
    /// <summary>
    /// 缓存工厂类
    /// </summary>
    public class CacheFactory
    {
        //TODO 加入配置文件

        /// <summary>
        /// 定义通用的Repository
        /// </summary>
        /// <returns></returns>
        public static ICache GetCache()
        {
            return new WebCache();
        }
    }
}