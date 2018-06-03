using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;

namespace Niqiu.Core.Helpers
{
    public class WebCache
    {
        #region 构造器
        private WebCache() { }

        static WebCache() {
            HttpContext context = HttpContext.Current;
            if (context != null) { _cache = (Cache)context.Cache; } else { _cache = HttpRuntime.Cache; }
        }
        #endregion

        #region 公有靜态字段
        /// <summary>
        /// 日缓存因子
        /// </summary>
        public static readonly int DayFactor = 17280;
        /// <summary>
        /// 小时缓存因子
        /// </summary>
        public static readonly int HourFactor = 720;
        /// <summary>
        /// 分缓存因子
        /// </summary>
        public static readonly int MinuteFactor = 12;
        /// <summary>
        /// 秒缓存因子
        /// </summary>
        public static readonly double SecondFactor = 0.2;
        #endregion

        #region 私有静态字段
        private static readonly Cache _cache;
        /// <summary>
        /// 缓存因子
        /// </summary>
        private static int Factor = 5;
        #endregion

        #region 静态方法
       
        /// <summary>
        /// 重新设置缓存因子 
        /// </summary>
        /// <param name="cacheFactor"></param>
        public static void ReSetFactor(int cacheFactor) { Factor = cacheFactor; }
        /// <summary>
        /// 清空所有缓存项目
        /// </summary>
        public static void Clear() {
            IDictionaryEnumerator CacheEnum = _cache.GetEnumerator();
            ArrayList al = new ArrayList();
            while (CacheEnum.MoveNext()) { al.Add(CacheEnum.Key); }

            foreach (string key in al){ _cache.Remove(key); }
        }
        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="pattern">缓存键正则匹配模式</param>
        public static void RemoveByPattern(string pattern) {
            IDictionaryEnumerator CacheEnum = _cache.GetEnumerator();
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            while (CacheEnum.MoveNext()) {
                if (regex.IsMatch(CacheEnum.Key.ToString()))
                    _cache.Remove(CacheEnum.Key.ToString());
            }
        }
        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key">缓存键名</param>
        public static void Remove(string key) { _cache.Remove(key); }
        /// <summary>
        /// 增加缓存项目 
        /// </summary>
        /// <param name="key">缓存键名</param>
        /// <param name="obj">缓存对象</param>
        public static void Insert(string key, object obj) { Insert(key, obj, null, 1); }
        /// <summary>
        /// 增加缓存项目(缓存时间:小时缓存因子*12)
        /// </summary>
        /// <param name="key">缓存键名</param>
        /// <param name="obj">缓存对象</param>
        /// <param name="dep">缓存依赖荐</param>
        public static void Insert(string key, object obj, CacheDependency dep) { Insert(key, obj, dep, HourFactor * 12); }
        /// <summary>
        /// 增加缓存项目
        /// </summary>
        /// <param name="key">缓存键名</param>
        /// <param name="obj">缓存对象</param>
        /// <param name="seconds">缓存秒数</param>
        public static void Insert(string key, object obj, int seconds) { Insert(key, obj, null, seconds); }
        /// <summary>
        /// 增加缓存项目
        /// </summary>
        /// <param name="key">缓存键名</param>
        /// <param name="obj">缓存对象</param>
        /// <param name="seconds">缓存秒数</param>
        /// <param name="priority">缓存优先级</param>
        public static void Insert(string key, object obj, int seconds, CacheItemPriority priority) { Insert(key, obj, null, seconds, priority); }
        /// <summary>
        /// 增加缓存项目
        /// </summary>
        /// <param name="key">缓存键名</param>
        /// <param name="obj">缓存对象</param>
        /// <param name="dep">缓存依赖项</param>
        /// <param name="seconds">缓存秒数</param>
        public static void Insert(string key, object obj, CacheDependency dep, int seconds) { Insert(key, obj, dep, seconds, CacheItemPriority.Normal); }
        /// <summary>
        /// 增加缓存
        /// </summary>
        /// <param name="key">缓存键名</param>
        /// <param name="obj">缓存对象</param>
        /// <param name="dep">缓存依赖项</param>
        /// <param name="seconds">缓存秒数</param>
        /// <param name="priority">缓存优先级</param>
        public static void Insert(string key, object obj, CacheDependency dep, int seconds, CacheItemPriority priority) {
            if (obj != null) {
                _cache.Insert(key, obj, dep, DateTime.Now.AddSeconds(Factor * seconds), Cache.NoSlidingExpiration, priority, null);
            }
        }
        /// <summary>
        /// 微小缓存
        /// </summary>
        /// <param name="key">缓存键名</param>
        /// <param name="obj">缓存对象</param>
        /// <param name="secondFactor">缓存秒因子</param>
        public static void MicroInsert(string key, object obj, int secondFactor) {
            if (obj != null) {
                _cache.Insert(key, obj, null, DateTime.Now.AddSeconds(Factor * secondFactor), Cache.NoSlidingExpiration);
            }
        }

        /// <summary>
        /// 增加缓存项目
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public static void Max(string key, object obj) { Max(key, obj, null); }
        /// <summary>
        /// 最大缓存对象
        /// </summary>
        /// <param name="key">缓存键名</param>
        /// <param name="obj">缓存对象</param>
        /// <param name="dep">缓存依赖项</param>
        public static void Max(string key, object obj, CacheDependency dep) {
            if (obj != null) {
                _cache.Insert(key, obj, dep, DateTime.MaxValue, Cache.NoSlidingExpiration, CacheItemPriority.AboveNormal, null);
            }
        }
        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <param name="key">缓存键名</param>
        /// <returns>返回缓存对象</returns>
        public static object Get(string key) { return _cache[key]; }
        /// <summary>
        /// 根据秒缓存因子计算秒数
        /// </summary>
        public static int SecondFactorCalculate(int seconds) { return Convert.ToInt32(Math.Round((double)seconds * SecondFactor)); }
        #endregion
}
}