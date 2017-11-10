namespace Our.Umbraco.AzureCDNToolkit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using StackExchange.Redis;

    public class RedisCache
    {
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect(AzureCdnToolkit.Instance.RedisCacheConnectionString);
        });

        public static ConnectionMultiplexer Connection => LazyConnection.Value;

        public static IDatabase Db => Connection.GetDatabase();

        public static T GetCacheItem<T>(string cacheKey)
        {
            var cacheItem = Db.StringGet(cacheKey);

            if (cacheItem.HasValue)
            {
                return JsonConvert.DeserializeObject<T>(cacheItem.ToString());
            }

            return default(T);
        }

        public static void InsertCacheItem<T>(string cacheKey, T item)
        {
            string cacheValue = JsonConvert.SerializeObject(item);

            Db.StringSet(cacheKey, cacheValue);
        }

        public static IEnumerable<T> GetCacheItemsByKeySearch<T>(string keyStartsWith)
        {
            var endpoints = Connection.GetEndPoints();
            var server = Connection.GetServer(endpoints.First());
            var keys = server.Keys(pattern: keyStartsWith + "*");

            return keys.Select(key => JsonConvert.DeserializeObject<T>(Db.StringGet(key).ToString())).ToList();
        }

        public static void ClearCacheItem(string cacheKey)
        {
            Db.KeyDelete(cacheKey);
        }

        public static void ClearCache()
        {
            var endpoints = Connection.GetEndPoints();
            var server = Connection.GetServer(endpoints.First());
            server.FlushAllDatabases();
        }

        public static void ClearCacheByKeySearch(string keyStartsWith)
        {
            var endpoints = Connection.GetEndPoints();
            var server = Connection.GetServer(endpoints.First());
            var keys = server.Keys(pattern: keyStartsWith + "*");

            foreach (var key in keys)
            {
                Db.KeyDelete(key);
            }
        }
    }
}