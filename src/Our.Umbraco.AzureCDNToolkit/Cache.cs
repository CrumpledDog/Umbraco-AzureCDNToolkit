using Umbraco.Core.Logging;

namespace Our.Umbraco.AzureCDNToolkit
{
    using System;
    using System.Collections.Generic;

    public static class Cache
    {
        public static T GetCacheItem<T>(string cacheKey)
        {
            if (AzureCdnToolkit.Instance.UseRedisCache)
            {
                try
                {
                    return RedisCache.GetCacheItem<T>(cacheKey);
                }
                catch (Exception e)
                {
                    // log and fall back to local cache
                    LogHelper.Error<string>(e.Message, e);
                }
            }

            return LocalCache.GetLocalCacheItem<T>(cacheKey);
        }

        public static void InsertCacheItem<T>(string cacheKey, Func<T> getCacheItem)
        {
            if (AzureCdnToolkit.Instance.UseRedisCache)
            {
                try
                {
                    var item = getCacheItem();
                    RedisCache.InsertCacheItem(cacheKey, item);
                    return;
                }
                catch (Exception e)
                {
                    // log and fall back to local cache
                    LogHelper.Error<string>(e.Message, e);
                }
            }

            LocalCache.InsertLocalCacheItem(cacheKey, getCacheItem);
        }

        public static IEnumerable<T> GetCacheItemsByKeySearch<T>(string keyStartsWith)
        {
            if (AzureCdnToolkit.Instance.UseRedisCache)
            {
                try
                {
                    return RedisCache.GetCacheItemsByKeySearch<T>(keyStartsWith);
                }
                catch (Exception e)
                {
                    // log and fall back to local cache
                    LogHelper.Error<string>(e.Message, e);
                }
            }

            return LocalCache.GetLocalCacheItemsByKeySearch<T>(keyStartsWith);
        }

        public static void ClearCacheItem(string key)
        {
            if (AzureCdnToolkit.Instance.UseRedisCache)
            {
                try
                {
                    RedisCache.ClearCacheItem(key);
                    return;
                }
                catch (Exception e)
                {
                    // log and fall back to local cache
                    LogHelper.Error<string>(e.Message, e);
                }
            }

            LocalCache.ClearLocalCacheItem(key);
        }

        public static void ClearCacheByKeySearch(string keyStartsWith)
        {
            if (AzureCdnToolkit.Instance.UseRedisCache)
            {
                try
                {
                    RedisCache.ClearCacheByKeySearch(keyStartsWith);
                    return;
                }
                catch (Exception e)
                {
                    // log and fall back to local cache
                    LogHelper.Error<string>(e.Message, e);
                }
            }

            LocalCache.ClearLocalCacheByKeySearch(keyStartsWith);
        }
    }
}