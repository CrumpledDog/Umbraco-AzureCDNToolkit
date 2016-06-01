namespace Our.Umbraco.AzureCDNToolkit.Models
{
    using System;

    using Newtonsoft.Json;
    public class CachedImagesRequest
    {
        [JsonProperty ("requestid")]
        public Guid RequestId { get; set; }

        [JsonProperty("serveridentity")]
        public string ServerIdentity { get; set; }
    }
}
