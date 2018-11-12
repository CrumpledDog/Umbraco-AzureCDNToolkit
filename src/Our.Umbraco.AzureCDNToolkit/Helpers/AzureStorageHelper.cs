using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Our.Umbraco.AzureCDNToolkit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Our.Umbraco.AzureCDNToolkit.Helpers
{
    /// <summary>
    /// Singleton base helper class for Azure blob storage
    /// </summary>
    public sealed class AzureStorageHelper
    {
        const string MEDIA = "Media";
        const string ASSETS = "Assets";

        private readonly int blobContainerCacheDurationInHours = 5;
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
        private readonly List<AzureStorageCache> cloudCachedBlobContainers;
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
            cloudCachedBlobContainers = new List<AzureStorageCache>();
            sasValidityMinutesSetting = WebConfigurationManager.AppSettings["AzureCDNToolkit:SASValidityInMinutes"];
            string connectionStringMedia = WebConfigurationManager.AppSettings["AzureCDNToolkit:MediaConnectionString"];
            string connectionStringAssets = WebConfigurationManager.AppSettings["AzureCDNToolkit:AssetsConnectionString"];
            string blobContainerCacheDurationInHoursSetting = WebConfigurationManager.AppSettings["AzureCDNToolkit:ContainerCacheDurationInHours"];
            int hours;
            if (int.TryParse(blobContainerCacheDurationInHoursSetting, out hours))
            {
                blobContainerCacheDurationInHours = hours;
            }
            if (string.IsNullOrEmpty(connectionStringAssets))
            {
                connectionStringAssets = connectionStringMedia;
            }
            SetupAzureStorageCache(connectionStringMedia, MEDIA);
            SetupAzureStorageCache(connectionStringAssets, ASSETS);
        }

        private AzureStorageCache SetupAzureStorageCache(string connectionString, string containerNameSetting, string containerName = null, string path = null)
        {            
            var settingsName = $"AzureCDNToolkit:{containerNameSetting}ConnectionString";
            if (string.IsNullOrEmpty(connectionString))
            {
                // try getting connection from Web.config (FileSystemProviders section)
                settingsName = $"AzureBlobFileSystem.ConnectionString:{AzureCdnToolkit.Instance.MediaContainer}";
                connectionString = WebConfigurationManager.AppSettings[settingsName];
            }
            CloudStorageAccount acc;
            if (string.IsNullOrEmpty(connectionString) || !CloudStorageAccount.TryParse(connectionString, out acc))
            {
                throw new ArgumentException($"Invalid Azure connectionString in appSetting '{settingsName}'");
            }
            if (!acc.Credentials.IsSharedKey)
            {
                throw new ArgumentException($"Azure connectionString in appSetting '{settingsName}' must use Shared Key authentication.");
            }
            var storageUri = new StorageUri(acc.BlobStorageUri.PrimaryUri, acc.BlobStorageUri.SecondaryUri);
            var cloudCachedBlobClient = new CloudBlobClient(storageUri, acc.Credentials);

            if (containerName == null && containerNameSetting != null)
            {
                containerName = WebConfigurationManager.AppSettings[$"AzureCDNToolkit:{containerNameSetting}Container"];
            }
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("Invalid container name in appSetting 'AzureCDNToolkit:AssetsContainer'");
            }
            containerName = containerName.ToLowerInvariant().Trim('/');
            var container = cloudCachedBlobClient.GetContainerReference(containerName);
            if (container == null)
            {
                LogHelper.Warn<AzureStorageHelper>($"GetPathWithSasTokenQuery() could not find or connect Azure blob container for path: {path}");
                return null;
            }
            var rVal = cloudCachedBlobContainers.FirstOrDefault(c => c.ContainerName.InvariantEquals(containerName));
            if (rVal != null)
            {
                rVal.Time = DateTime.Now;
                rVal.Container = container;
            }
            else
            {
                rVal = new AzureStorageCache()
                {
                    Time = DateTime.Now,
                    ConnectionString = connectionString,
                    ContainerName = containerName,
                    Container = container,
                    SasCache = new SASCache(),
                    Permissions = container.GetPermissions()
                };
                cloudCachedBlobContainers.Add(rVal);
            }
            return rVal;
        }

        /// <summary>
        /// Gets path with addtional SAS token query string if one is necessary
        /// </summary>
        /// <param name="path">original path to be extended</param>
        /// <param name="containerName">[optional] Azure blob container name (default: configured AzureCDNToolkit:MediaCacheContainer)</param>
        /// <returns>orig. pat with SAS querystring</returns>
        public string GetPathWithSasTokenQuery(string path, string containerName = null)
        {
            Uri uri;
            if (path.Contains("?"))
            {
                uri = new Uri(path);
                if (uri.Query.Contains("&sig="))
                {
                    // sas token is already attached
                    return path;
                }
            }
            AzureStorageCache containerItem = null;
            if (string.IsNullOrEmpty(containerName))
            {
                uri = new Uri(path);
                if (uri.Segments.Length < 2)
                {
                    LogHelper.Warn<AzureStorageHelper>($"GetPathWithSasTokenQuery() could not find Azure blob container for path: {path}");
                    return path;
                }
                containerName = uri.Segments[1].Trim(uri.Segments[0]);
                if (string.IsNullOrEmpty(containerName))
                {
                    LogHelper.Warn<AzureStorageHelper>($"GetPathWithSasTokenQuery() could not find Azure blob container for path: {path}");
                    return path;
                }
            }
            containerItem = cloudCachedBlobContainers.FirstOrDefault(c => c.ContainerName.Equals(containerName.ToLower()));
            if (containerItem == null)
            {
                containerItem = SetupAzureStorageCache(null, null, containerName, path);
                if (containerItem == null)
                {
                    return path;
                }
            }
            if ((DateTime.Now - containerItem.Time).TotalHours > blobContainerCacheDurationInHours)
            {
                containerItem = SetupAzureStorageCache(containerItem.ConnectionString, null, containerItem.ContainerName, path);
            }
            var cloudCachedBlobContainer = containerItem.Container;
            var sasCache = containerItem.SasCache;

            if (containerItem.Permissions.PublicAccess != BlobContainerPublicAccessType.Off)
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
                double sasValidityMinutes;
                double.TryParse(sasValidityMinutesSetting, out sasValidityMinutes);
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
