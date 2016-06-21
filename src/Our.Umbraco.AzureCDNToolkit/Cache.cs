namespace Our.Umbraco.AzureCDNToolkit
{
    using System;
    using System.Collections.Generic;

    public static class Cache
    {
        public static T GetCacheItem<T>(string cacheKey)
        {
            return LocalCache.GetLocalCacheItem<T>(cacheKey);
        }

        public static void InsertCacheItem<T>(string cacheKey, Func<T> getCacheItem)
        {
            LocalCache.InsertLocalCacheItem<T>(cacheKey, getCacheItem);
        }

        public static IEnumerable<T> GetCacheItemsByKeySearch<T>(string keyStartsWith)
        {
            return LocalCache.GetLocalCacheItemsByKeySearch<T>(keyStartsWith);
        }

        public static void ClearCacheItem(string key)
        {
            LocalCache.ClearLocalCacheItem(key);
        }

        public static void ClearCacheByKeySearch(string keyStartsWith)
        {
            LocalCache.ClearLocalCacheByKeySearch(keyStartsWith);
        }


    }
}
