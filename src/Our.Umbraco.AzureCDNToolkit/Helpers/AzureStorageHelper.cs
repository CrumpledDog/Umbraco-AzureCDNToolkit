using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Our.Umbraco.AzureCDNToolkit.Models;
using System;
using System.Web.Configuration;

namespace Our.Umbraco.AzureCDNToolkit.Helpers
{
    /// <summary>
    /// Singleton base helper class for Azure blob storage
    /// </summary>
    public sealed class AzureStorageHelper
    {
        /// <summary>
        /// singleton instance
        /// </summary>
        private static volatile AzureStorageHelper _instance;
        /// <summary>
        /// instantiation helper object
        /// </summary>
        private static readonly object syncRoot = new Object();

        /// <summary>
        /// Life time of generated SAS tokens for private blobs
        /// </summary>
        private readonly string sasValidityMinutesSetting;
        /// <summary>
        /// Cloud blob container for media items cache
        /// </summary>
        private readonly CloudBlobContainer cloudCachedBlobContainerMedia;
        /// <summary>
        /// Cloud blob container name
        /// </summary>
        private readonly string containerNameMedia;
        /// <summary>
        /// Internal SAS token cache for media blob
        /// </summary>
        private readonly SASCache sasCacheMedia = new SASCache();

        #region prepared for future releases
        private readonly bool handleAssets = false;
        private readonly CloudBlobContainer cloudCachedBlobContainerAssets;
        private readonly string containerNameAssets;
        private readonly SASCache sasCacheAssets = new SASCache();
        #endregion

        /// <summary>
        /// Gets the singleton instance of the <see cref="AzureStorageHelper"/> class
        /// </summary>
        public static AzureStorageHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new AzureStorageHelper();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="AzureStorageHelper"/> class from being created
        /// (singleton constructor)
        /// </summary>
        private AzureStorageHelper()
        {
            sasValidityMinutesSetting = WebConfigurationManager.AppSettings["AzureCDNToolkit:SASValidityInMinutes"];
            string connectionStringMedia = WebConfigurationManager.AppSettings["AzureCDNToolkit:MediaConnectionString"];
            string connectionStringAssets = WebConfigurationManager.AppSettings["AzureCDNToolkit:AssetsConnectionString"];
            if (string.IsNullOrEmpty(connectionStringMedia))
            {
                // try getting connection from Web.config (FileSystemProviders section)
                connectionStringMedia = WebConfigurationManager.AppSettings[$"AzureBlobFileSystem.ConnectionString:{AzureCdnToolkit.Instance.MediaContainer}"];
            }
            if (string.IsNullOrEmpty(connectionStringAssets))
            {
                connectionStringAssets = connectionStringMedia;
            }
            if (string.IsNullOrEmpty(connectionStringMedia) || !CloudStorageAccount.TryParse(connectionStringMedia, out CloudStorageAccount acc))
            {
                throw new ArgumentException("invalid Azure connectionString in appSetting 'AzureCDNToolkit:MediaConnectionString'");
            }
            if (!acc.Credentials.IsSharedKey)
            {
                throw new ArgumentException("Azure connectionString in appSetting 'AzureCDNToolkit:MediaConnectionString' must use Shared Key");
            }
            var storageUri = new StorageUri(acc.BlobStorageUri.PrimaryUri, acc.BlobStorageUri.SecondaryUri);
            var cloudCachedBlobClient = new CloudBlobClient(storageUri, acc.Credentials);

            containerNameMedia = WebConfigurationManager.AppSettings["AzureCDNToolkit:MediaCacheContainer"];
            if (string.IsNullOrEmpty(containerNameMedia))
            {
                throw new ArgumentException("invalid container name in appSetting 'AzureCDNToolkit:MediaCacheContainer'");
            }
            containerNameMedia = containerNameMedia.ToLowerInvariant().Trim('/');
            cloudCachedBlobContainerMedia = cloudCachedBlobClient.GetContainerReference(containerNameMedia);

            // not very likely getting assets from private blobs(?)
            #region assets
            if (!handleAssets)
            {
                return;
            }
            if (!CloudStorageAccount.TryParse(connectionStringAssets, out acc))
            {
                throw new ArgumentException("invalid Azure connectionString in appSetting 'AzureCDNToolkit:AssetsConnectionString'");
            }
            if (!acc.Credentials.IsSharedKey)
            {
                throw new ArgumentException("Azure connectionString in appSetting 'AzureCDNToolkit:AssetsConnectionString' must use Shared Key");
            }
            storageUri = new StorageUri(acc.BlobStorageUri.PrimaryUri, acc.BlobStorageUri.SecondaryUri);
            cloudCachedBlobClient = new CloudBlobClient(storageUri, acc.Credentials);

            containerNameAssets = WebConfigurationManager.AppSettings["AzureCDNToolkit:AssetsContainer"];
            if (string.IsNullOrEmpty(containerNameAssets))
            {
                throw new ArgumentException("invalid container name in appSetting 'AzureCDNToolkit:AssetsContainer'");
            }
            containerNameAssets = containerNameAssets.ToLowerInvariant().Trim('/');
            cloudCachedBlobContainerAssets = cloudCachedBlobClient.GetContainerReference(containerNameAssets);
            #endregion
        }

        /// <summary>
        /// Gets path with addtional SAS token query string if one is necessary
        /// </summary>
        /// <param name="path">original path to be extended</param>
        /// <param name="containerName">[optional] Azure blob container name (default: configured AzureCDNToolkit:MediaCacheContainer)</param>
        /// <returns>orig. pat with SAS querystring</returns>
        public string GetPathWithSasTokenQuery(string path, string containerName = null)
        {
            bool forAssets = false;
            var cloudCachedBlobContainer = cloudCachedBlobContainerMedia;
            var sasCache = sasCacheMedia;
            if (handleAssets)
            {
                if (string.IsNullOrEmpty(containerName))
                {
                    containerName = containerNameMedia;
                }
                forAssets = containerName.ToLowerInvariant().Equals(containerNameAssets);
                cloudCachedBlobContainer = forAssets ? cloudCachedBlobContainerAssets : cloudCachedBlobContainerMedia;
                sasCache = forAssets ? sasCacheAssets : sasCacheMedia;
            }

            BlobContainerPermissions permissions = cloudCachedBlobContainer.GetPermissions();
            if (permissions.PublicAccess != BlobContainerPublicAccessType.Off)
            {
                // nothing is required for public blobs
                return path;
            }

            if (sasCache != null)
            {
                if (sasCache.CreationTime.AddMinutes(sasCache.ValidityMinutes / 2) < DateTime.UtcNow)
                {
                    sasCache.SASQueryString = null;
                }
            }
            if (string.IsNullOrEmpty(sasCache.SASQueryString))
            {
                // Shared Access Signatures can only be generated under shared key credentials!
                double.TryParse(sasValidityMinutesSetting, out double sasValidityMinutes);
                if (sasValidityMinutes <= 10)
                {
                    sasValidityMinutes = 10;
                }
                SharedAccessBlobPolicy accessPolicy = new SharedAccessBlobPolicy()
                {
                    Permissions = SharedAccessBlobPermissions.Read,
                    SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                    SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(sasValidityMinutes)
                };
                string sasQueryString = cloudCachedBlobContainer.GetSharedAccessSignature(accessPolicy);
                sasCache.SASQueryString = sasQueryString;
                sasCache.CreationTime = DateTime.UtcNow;
                sasCache.ValidityMinutes = sasValidityMinutes;
            }
            if (path.Contains("?"))
            {
                return path + sasCache.SASQueryString.Replace('?', '&');
            }
            else
            {
                return path + sasCache.SASQueryString;
            }
        }
    }
}
