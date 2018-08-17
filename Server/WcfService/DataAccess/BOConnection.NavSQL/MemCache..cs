using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Runtime.Caching;
namespace LSOmni.DataAccess.BOConnection.NavSQL
{
    /*
     * usage
        protected static MemCache OmniMemoryCache = null;
       ...
        if (OmniMemoryCache == null)
            OmniMemoryCache = new MemCache(60); //60 minutes
     */
    public class MemCache
    {
        //private ObjectCache cache;
        private MemoryCache cache;
        private double timeSpanInMinutes = 60 * 24 * 7;  //7 days

        private System.Collections.Specialized.NameValueCollection GetCacheSettings()
        {
            System.Collections.Specialized.NameValueCollection cacheSettings = new System.Collections.Specialized.NameValueCollection(2);
            cacheSettings.Add("cacheMemoryLimitMegabytes", Convert.ToString(50));
            cacheSettings.Add("pollingInterval", Convert.ToString("00:00:10")); //"HH:MM:SS"
            return cacheSettings;
        }
        public MemCache()
        {
            cache = new MemoryCache("MyOmniCache", GetCacheSettings()); //MemoryCache.Default;  
        }
        public MemCache(double timeSpanInMinutes)
        {
            cache = new MemoryCache("MyOmniCache", GetCacheSettings());//MemoryCache.Default;  

            this.timeSpanInMinutes = timeSpanInMinutes;
        }
        public void Add(string key, Object data)
        {
            CacheItem ci = new CacheItem(key, data);

            CacheItemPolicy policy = new CacheItemPolicy();
            policy.Priority = CacheItemPriority.Default;

            policy.SlidingExpiration.Add(TimeSpan.FromMinutes(this.timeSpanInMinutes));
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(this.timeSpanInMinutes);

            cache.Set(ci, policy);
        }
        public void Add(string key, Object data, double timeSpanInMinutes)
        {
            CacheItem ci = new CacheItem(key, data);

            CacheItemPolicy policy = new CacheItemPolicy();
            policy.Priority = CacheItemPriority.Default;

            policy.SlidingExpiration.Add(TimeSpan.FromMinutes(timeSpanInMinutes));
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(timeSpanInMinutes);

            cache.Set(ci, policy);
        }
        public Object Get(string key)
        {
            if (cache.Contains(key))
                return cache.Get(key);
            else
                return null;
        }
        public bool Contains(string key)
        {
            return cache.Contains(key);
        }

        public void Remove(string key)
        {
            // 
            if (cache.Contains(key))
            {
                cache.Remove(key);
            }
        }
    }

}

