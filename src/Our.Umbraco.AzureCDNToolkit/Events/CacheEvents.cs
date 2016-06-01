namespace Our.Umbraco.AzureCDNToolkit.Events
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    using global::Umbraco.Core;
    using global::Umbraco.Core.Cache;
    using global::Umbraco.Web.Cache;
    using global::Umbraco.Core.Logging;

    using CacheRefreshers;
    using Models;

    public class CacheEvents: ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication,
            ApplicationContext applicationContext)
        {
            CacheRefresherBase<CacheRequester>.CacheUpdated += CacheRequester_Request;
            CacheRefresherBase<CacheResponder>.CacheUpdated += CacheResponder_Response;
            CacheRefresherBase<CacheWiper>.CacheUpdated += CacheWiper_Request;
        }

        /// <summary>
        /// Handles all cache 'requests', and checks to see if the current machine should respond (with another 'cache refresher')
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CacheRequester_Request(CacheRequester sender, CacheRefresherEventArgs e)
        {
            var rawPayLoad = (string)e.MessageObject;

            var payload = JsonConvert.DeserializeObject<CachedImagesRequest>(rawPayLoad);

            if (
                ApplicationContext.Current.Services.ServerRegistrationService.CurrentServerIdentity.InvariantEquals(
                    payload.ServerIdentity))
            {
                // THIS SERVER SHOULD RETURN DATA VIA CacheImagesResponder

                var runtimeCache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
                var cachedItems = runtimeCache.GetCacheItemsByKeySearch<CachedImage>(AzureCDNToolkit.Constants.Keys.CachePrefix);

                var response = new CachedImagesResponse()
                {
                    RequestId = payload.RequestId,
                    CachedImages = cachedItems
                };

                var json = JsonConvert.SerializeObject(response);
                DistributedCache.Instance.RefreshByJson(CacheResponder.Guid, json);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CacheResponder_Response(CacheResponder sender, CacheRefresherEventArgs e)
        {
            var rawPayLoad = (string)e.MessageObject;
            var payload = JsonConvert.DeserializeObject<CachedImagesResponse>(rawPayLoad);

            var runtimeCache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
            var cacheKey = string.Format("{0}{1}", AzureCDNToolkit.Constants.Keys.CachePrefixResponse, payload.RequestId);
            runtimeCache.InsertCacheItem<IEnumerable<CachedImage>>(cacheKey, () => payload.CachedImages);
        }


        /// <summary>
        /// Handles all cache 'requests', and checks to see if the current machine should respond (with another 'cache refresher')
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CacheWiper_Request(CacheWiper sender, CacheRefresherEventArgs e)
        {
            var rawPayLoad = (string)e.MessageObject;

            var payload = JsonConvert.DeserializeObject<CachedImagesWipe>(rawPayLoad);

            if (
                ApplicationContext.Current.Services.ServerRegistrationService.CurrentServerIdentity.InvariantEquals(
                    payload.ServerIdentity))
            {
                // This server should wipe it's application cache
                var runtimeCache = ApplicationContext.Current.ApplicationCache.RuntimeCache;

                if (payload.WebUrl != null)
                {
                    // wipe specific url
                    var cachePrefix = AzureCDNToolkit.Constants.Keys.CachePrefix;
                    var cacheKey = string.Format("{0}{1}", cachePrefix, payload.WebUrl);
                    runtimeCache.ClearCacheItem(cacheKey);

                    LogHelper.Info<CacheEvents>(string.Format("Azure CDN Toolkit: CDN image path runtime cache for key {0} cleared by dashboard control request", payload.WebUrl));
                }
                else
                {
                    // clear all keys
                    runtimeCache.ClearCacheByKeySearch(AzureCDNToolkit.Constants.Keys.CachePrefix);

                    LogHelper.Info<CacheEvents>("Azure CDN Toolkit: CDN image path runtime cache cleared by dashboard control request");
                }

            }
        }
    }
}
