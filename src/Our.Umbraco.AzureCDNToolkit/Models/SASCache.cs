namespace Our.Umbraco.AzureCDNToolkit.Models
{
    using System;

    /// <summary>
    /// used for SAS caching
    /// </summary>
    internal sealed class SASCache
    {
        /// <summary>
        /// defines how long this item is valid in minutes as defined in cache.config 'SASValidityInMinutes'
        /// </summary>
        public double ValidityMinutes { get; set; }
        /// <summary>
        /// stores creation time of SAS
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// stores SAS query string
        /// </summary>
        public string SASQueryString { get; set; }
    }
}
