namespace Our.Umbraco.AzureCDNToolkit.Models
{
    using Newtonsoft.Json;
    public class CachedImage
    {
        [JsonProperty ("weburl")]
        public string WebUrl { get; set; }
        [JsonProperty("cacheurl")]
        public string CacheUrl { get; set; }
    }
}
