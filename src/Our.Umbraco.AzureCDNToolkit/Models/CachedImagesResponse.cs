namespace Our.Umbraco.AzureCDNToolkit.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    public class CachedImagesResponse
    {
        [JsonProperty ("requestid")]
        public Guid RequestId { get; set; }

        [JsonProperty("cachedimages")]
        public IEnumerable<CachedImage> CachedImages { get; set; }
    }
}
