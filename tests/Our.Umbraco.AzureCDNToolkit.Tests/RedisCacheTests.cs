using System;
using System.Linq;
using NUnit.Framework;

namespace Our.Umbraco.AzureCDNToolkit.Tests
{
    [TestFixture]
    public class RedisCacheTests
    {
        [Test]
        public void TestGetCacheItem()
        {
            AzureCdnToolkit.Instance.Refresh();
            
            var stringToSet = "Test value";
            var cacheKey = Guid.NewGuid().ToString();

            // add one item to ensure something is there to flush
            RedisCache.InsertCacheItem(cacheKey, stringToSet);

            var cacheItem = RedisCache.GetCacheItem<string>(cacheKey);

            Assert.IsNotNull(cacheItem);
        }

        [Test]
        public void TestInsertCacheItem()
        {
            AzureCdnToolkit.Instance.Refresh();
            var endpoints = RedisCache.Connection.GetEndPoints();
            var server = RedisCache.Connection.GetServer(endpoints.First());
            var itemsBeforeInsert = server.DatabaseSize();
            var cacheKey = Guid.NewGuid().ToString();

            RedisCache.InsertCacheItem(cacheKey, "Test value");

            var itemsAfterInsert = server.DatabaseSize();

            Assert.AreNotEqual(itemsBeforeInsert, itemsAfterInsert);
        }

        [Test]
        public void TestGetCacheItemsByKeySearch()
        {
            AzureCdnToolkit.Instance.Refresh();
            var cacheKey = Guid.NewGuid().ToString();
            var cacheKeySub = cacheKey.Substring(0, 8);

            RedisCache.InsertCacheItem(cacheKey, "Test value");

            var item = RedisCache.GetCacheItemsByKeySearch<string>(cacheKeySub).FirstOrDefault();

            Assert.IsNotNull(item);
        }

        [Test]
        public void TestClearCache()
        {
            AzureCdnToolkit.Instance.Refresh();

            var endpoints = RedisCache.Connection.GetEndPoints();
            var server = RedisCache.Connection.GetServer(endpoints.First());

            // add one item to ensure something is there to flush
            RedisCache.Db.StringSet(Guid.NewGuid().ToString(), "Test value");

            var keysBeforeFlush = server.DatabaseSize();
            server.FlushAllDatabases();
            var keysAfterFlush = server.DatabaseSize();

            Assert.AreNotEqual(keysBeforeFlush, keysAfterFlush);
        }

        [Test]
        public void TestClearCacheItem()
        {
            AzureCdnToolkit.Instance.Refresh();
            var cacheKey = Guid.NewGuid().ToString();
            var stringToSet = "Test value";

            RedisCache.InsertCacheItem(cacheKey, stringToSet);

            var insertedItem = RedisCache.GetCacheItem<string>(cacheKey);

            Assert.AreEqual(insertedItem, stringToSet);
            
            RedisCache.ClearCacheItem(cacheKey);

            insertedItem = RedisCache.GetCacheItem<string>(cacheKey);

            Assert.IsNull(insertedItem);
        }
        
        [Test]
        public void TestClearCacheByKeySearch()
        {
            AzureCdnToolkit.Instance.Refresh();

            // insert some items that all start with the same string
            var cacheKey = Guid.NewGuid().ToString().Substring(0, 8);

            for (var i = 0; i < 10; i++)
            {
                var uniqueCacheKey = cacheKey + Guid.NewGuid();
                RedisCache.InsertCacheItem(uniqueCacheKey, "Test value");
            }

            var itemsBeforeDelete = RedisCache.GetCacheItemsByKeySearch<string>(cacheKey);

            Assert.AreEqual(itemsBeforeDelete.Count(), 10);

            RedisCache.ClearCacheByKeySearch(cacheKey);

            var itemsAfterDelete = RedisCache.GetCacheItemsByKeySearch<string>(cacheKey);

            Assert.AreNotEqual(itemsBeforeDelete.Count(), itemsAfterDelete.Count());
        }
    }
}