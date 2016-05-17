namespace Our.Umbraco.AzureCDNToolkit.Models
{
    using Newtonsoft.Json;
    public class CachedImagesWipe
    {
        [JsonProperty("weburl")]
        public string WebUrl { get; set; }

        [JsonProperty("serveridentity")]
        public string ServerIdentity { get; set; }
    }
}
