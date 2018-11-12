using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace Our.Umbraco.AzureCDNToolkit.Models
{
    internal sealed class AzureStorageCache
    {
        public DateTime Time { get; set; }
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
        public CloudBlobContainer Container { get; set; }
        public SASCache SasCache { get; set; }
        public BlobContainerPermissions Permissions { get; set; }
    }
}
