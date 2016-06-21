namespace Our.Umbraco.AzureCDNToolkit
{
    using System;
    using System.Collections.Generic;

    using global::Umbraco.Core;
    using global::Umbraco.Core.Cache;

    internal static class LocalCache
    {
        internal static T GetLocalCacheItem<T>(string cacheKey)
        {
            var runtimeCache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
            var cachedItem = runtimeCache.GetCacheItem<T>(cacheKey);
            return cachedItem;
        }

        internal static void InsertLocalCacheItem<T>(string cacheKey, Func<T> getCacheItem)
        {
            var runtimeCache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
            runtimeCache.InsertCacheItem<T>(cacheKey, getCacheItem);
        }

        internal static IEnumerable<T> GetLocalCacheItemsByKeySearch<T>(string keyStartsWith)
        {
            var runtimeCache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
            return runtimeCache.GetCacheItemsByKeySearch<T>(keyStartsWith);
        }

        internal static void ClearLocalCacheItem(string key)
        {
            var runtimeCache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
            runtimeCache.ClearCacheItem(key);
        }

        internal static void ClearLocalCacheByKeySearch(string keyStartsWith)
        {
            var runtimeCache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
            runtimeCache.ClearCacheByKeySearch(keyStartsWith);
        }
    }
}
