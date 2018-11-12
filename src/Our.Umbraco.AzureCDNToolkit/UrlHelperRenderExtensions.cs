namespace Our.Umbraco.AzureCDNToolkit
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Configuration;

    using global::Umbraco.Core.Models;
    using global::Umbraco.Web.Models;
    using global::Umbraco.Core;
    using global::Umbraco.Web;
    using global::Umbraco.Core.Logging;

    using Newtonsoft.Json;

    using Models;
    using Our.Umbraco.AzureCDNToolkit.Helpers;

    public static class UrlHelperRenderExtensions
    {
        public static IHtmlString GetCropCdnUrl(this UrlHelper urlHelper,
            ImageCropDataSet imageCropper,
            int? width = null,
            int? height = null,
            string propertyAlias = global::Umbraco.Core.Constants.Conventions.Media.File,
            string cropAlias = null,
            int? quality = null,
            ImageCropMode? imageCropMode = null,
            ImageCropAnchor? imageCropAnchor = null,
            bool preferFocalPoint = false,
            bool useCropDimensions = false,
            string cacheBusterValue = null,
            string furtherOptions = null,
            ImageCropRatioMode? ratioMode = null,
            bool upScale = true,
            bool htmlEncode = true
            )
        {

            // if no cacheBusterValue provided we need to make one
            if (cacheBusterValue == null)
            {
                cacheBusterValue = DateTime.UtcNow.ToFileTimeUtc().ToString(CultureInfo.InvariantCulture);
            }

            var cropUrl = GetCropUrl(imageCropper.Src, imageCropper, width, height, cropAlias, quality, imageCropMode,
                imageCropAnchor, preferFocalPoint, useCropDimensions, cacheBusterValue, furtherOptions, ratioMode,
                upScale);

            return UrlToCdnUrl(cropUrl, htmlEncode);
        }

        public static IHtmlString GetCropCdnUrl(this UrlHelper urlHelper,
            IPublishedContent mediaItem,
            int? width = null,
            int? height = null,
            string propertyAlias = global::Umbraco.Core.Constants.Conventions.Media.File,
            string cropAlias = null,
            int? quality = null,
            ImageCropMode? imageCropMode = null,
            ImageCropAnchor? imageCropAnchor = null,
            bool preferFocalPoint = false,
            bool useCropDimensions = false,
            bool cacheBuster = true,
            string furtherOptions = null,
            ImageCropRatioMode? ratioMode = null,
            bool upScale = true,
            bool htmlEncode = true
            )
        {
            var cropUrl =
                urlHelper.GetCropUrl(mediaItem, width, height, propertyAlias, cropAlias, quality, imageCropMode,
                    imageCropAnchor, preferFocalPoint, useCropDimensions, cacheBuster, furtherOptions, ratioMode, upScale, false)
                    .ToString();

            return UrlToCdnUrl(cropUrl, htmlEncode);
        }

        public static IHtmlString ResolveCdn(this UrlHelper urlHelper, string path, bool asset = true, bool htmlEncode = true)
        {
            return ResolveCdn(urlHelper, path, AzureCdnToolkit.Instance.CdnPackageVersion, asset, htmlEncode: htmlEncode);
        }

        // Special version of the method with fallback image for TinyMce converter
        internal static IHtmlString ResolveCdnFallback(this UrlHelper urlHelper, IPublishedContent mediaItem, bool asset = true, string querystring = null, bool htmlEncode = true, string fallbackImage = null)
        {

            LogHelper.Debug(typeof(UrlHelperRenderExtensions), string.Format("Parsed out media item from TinyMce: {0}", mediaItem.Name));

            var std = ResolveCdn(urlHelper, mediaItem, asset, querystring, htmlEncode);
            if (std == null)
            {
                return ResolveCdn(urlHelper, fallbackImage, asset, htmlEncode);
            }
            return std;
        }
        public static IHtmlString ResolveCdn(this UrlHelper urlHelper, IPublishedContent mediaItem, bool asset = true, string querystring = null, bool htmlEncode = true)
        {
            var cacheBusterValue = mediaItem.UpdateDate.ToFileTimeUtc().ToString(CultureInfo.InvariantCulture);

            var path = mediaItem.Url;

            // If mediaItem.Url is null attempt to get from Cropper
            if (path == null)
            {
                // attempt to get value from Cropper if there is one
                var umbracoFile = mediaItem.GetProperty(global::Umbraco.Core.Constants.Conventions.Media.File).DataValue.ToString();
                if (!string.IsNullOrEmpty(umbracoFile))
                {
                    var cropper = JsonConvert.DeserializeObject<ImageCropDataSet>(umbracoFile);
                    if (cropper != null)
                    {
                        path = cropper.Src;
                    }
                }
                else
                {
                    // unable to get a Url for the media item
                    return null;
                }
            }

            if (querystring != null)
            {
                path = string.Format("{0}?{1}", path, querystring);
            }

            return ResolveCdn(urlHelper, path, cacheBusterValue, asset, "rnd", htmlEncode);
        }

        public static IHtmlString ResolveCdn(this UrlHelper urlHelper, string path, string cacheBuster, bool asset = true, string cacheBusterName = "v", bool htmlEncode = true)
        {
            if (AzureCdnToolkit.Instance.UseAzureCdnToolkit)
            {
                var hasQuerystring = path.InvariantContains("?");
                var absoluteDomain = AzureCdnToolkit.Instance.Domain ?? HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                var wasAbsolute = false;

                Uri srcUri;
                if (Uri.TryCreate(path, UriKind.Absolute, out srcUri))
                {
                    wasAbsolute = true;
                }

                if (srcUri == null)
                {
                    // relative url that we need to make absolute so that Uri works properly
                    Uri.TryCreate(string.Format("{0}{1}", absoluteDomain, path), UriKind.Absolute, out srcUri);
                }

                var qs = srcUri.ParseQueryString();

                if (wasAbsolute &&
                    string.Format("{0}://{1}", srcUri.Scheme, srcUri.DnsSafeHost) != absoluteDomain)
                {
                    if (AzureCdnToolkit.Instance.usePrivateMedia)
                    {
                        path = AzureStorageHelper.Instance.GetPathWithSasTokenQuery(path);
                    }
                    // absolute url already and not this site - abort!
                    return new HtmlString(path);
                }

                var separator = path.InvariantContains("?") ? "&" : "?";
                string cdnPath;

                if (asset && !path.InvariantContains("/media/"))
                {
                    cdnPath = string.Format("{0}/{1}", AzureCdnToolkit.Instance.CdnUrl, AzureCdnToolkit.Instance.AssetsContainer);
                }
                else
                {
                    cdnPath = AzureCdnToolkit.Instance.CdnUrl;
                }

                // Check if we should add version cachebuster
                if (qs["v"] == null && qs["rnd"] == null)
                {
                    qs.Add(cacheBusterName, cacheBuster);
                    path = string.Format("{0}?{1}", srcUri.LocalPath, qs);
                }


                // TRY for ImageProcessor Azure Cache and check for ApplicationContext.Current (otherwise a test)
                if (!asset && ApplicationContext.Current != null)
                {
                    var azureCachePath = UrlToCdnUrl(path, false, currentDomain: absoluteDomain).ToString();

                    if (!azureCachePath.InvariantEquals(string.Format("{0}{1}", absoluteDomain, path)))
                    {
                        path = azureCachePath;
                    }
                }
                else if (!asset && qs.AllKeys.Any(x => !x.InvariantEquals("v") && !x.InvariantEquals("rnd")))
                {
                    // check if has querystring excluding cachebusters, if it does return as is as ImageProcessor needs to process
                }
                else
                {
                    // direct request to CDN, should remove all querystrings except cachebuster

                    // Adjust for custom media container names
                    if (!AzureCdnToolkit.Instance.MediaContainer.InvariantEquals("media"))
                    {
                        srcUri = new Uri(srcUri.AbsoluteUri.Replace("/media/",
                            string.Format("/{0}/", AzureCdnToolkit.Instance.MediaContainer)));
                    }

                    if (qs["rnd"] != null)
                    {
                        cacheBusterName = "rnd";
                    }
                    path = string.Format("{0}{1}?{2}={3}", cdnPath, srcUri.LocalPath, cacheBusterName, cacheBuster);
                }
            }
            if (AzureCdnToolkit.Instance.usePrivateMedia)
            {
                path = AzureStorageHelper.Instance.GetPathWithSasTokenQuery(path);
            }
            
            return htmlEncode ? new HtmlString(HttpUtility.HtmlEncode(path)) : new HtmlString(path);

        }

        internal static IHtmlString UrlToCdnUrl(string cropUrl, bool htmlEncode, string currentDomain = null)
        {
            if (string.IsNullOrEmpty(cropUrl))
            {
                return new HtmlString(string.Empty);
            }

            // If toolkit disabled return orginal string
            if (!AzureCdnToolkit.Instance.UseAzureCdnToolkit)
            {
                return new HtmlString(cropUrl);
            }

            if (string.IsNullOrEmpty(currentDomain))
            {
                currentDomain = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            }

            var cachePrefix = Constants.Keys.CachePrefix;
            var cacheKey = string.Format("{0}{1}", cachePrefix, cropUrl);

            var absoluteCropPath = string.Format("{0}{1}", currentDomain, cropUrl);
            var cachedItem = Cache.GetCacheItem<CachedImage>(cacheKey);

            var fullUrlPath = string.Empty;

            try
            {
                if (cachedItem == null)
                {
                    var newCachedImage = new CachedImage { WebUrl = cropUrl };

                    LogHelper.Debug(typeof(UrlHelperRenderExtensions), string.Format("Attempting to resolve: {0}", absoluteCropPath));

                    // if security token has been setup we need to add it here
                    var securityToken = WebConfigurationManager.AppSettings["AzureCDNToolkit:SecurityToken"];
                    if (!string.IsNullOrEmpty(securityToken))
                    {
                        absoluteCropPath = string.Format("{0}&securitytoken={1}", absoluteCropPath, securityToken);
                    }

                    // Retry five times before giving up to account for networking issues
                    TryFiveTimes(() =>
                    {
                        string responseUrl = null;
                        var request = (HttpWebRequest)WebRequest.Create(absoluteCropPath);
                        request.Method = "HEAD";
                        if (AzureCdnToolkit.Instance.usePrivateMedia)
                        {
                            try
                            {
                                using (var response = (HttpWebResponse)request.GetResponse()) { }
                            }
                            catch (System.Net.WebException ex)
                            {
                                responseUrl = ex.Response.ResponseUri.AbsoluteUri;
                                if (responseUrl != absoluteCropPath)
                                {
                                    var sasUrl = AzureStorageHelper.Instance.GetPathWithSasTokenQuery(responseUrl);
                                    request = (HttpWebRequest)WebRequest.Create(sasUrl);
                                }
                            }
                        }
                        using (var response = (HttpWebResponse)request.GetResponse())
                        {
                            var responseCode = response.StatusCode;
                            if (responseCode.Equals(HttpStatusCode.OK))
                            {
                                var absoluteUri = response.ResponseUri.AbsoluteUri;
                                newCachedImage.CacheUrl = responseUrl != null ? responseUrl : absoluteUri;

                                // this is to mark URLs returned direct to Blob by ImageProcessor as not fully resolved
                                newCachedImage.Resolved = absoluteUri.InvariantContains(AzureCdnToolkit.Instance.CdnUrl);

                                Cache.InsertCacheItem<CachedImage>(cacheKey, () => newCachedImage);
                                fullUrlPath = absoluteUri;
                            }
                        }
                    });

                }
                else
                { 
                    if (AzureCdnToolkit.Instance.usePrivateMedia)
                    {
                        fullUrlPath = AzureStorageHelper.Instance.GetPathWithSasTokenQuery(cachedItem.CacheUrl);
                    }
                    else
                    {
                        fullUrlPath = cachedItem.CacheUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(UrlHelperRenderExtensions), "Error resolving media url from the CDN", ex);

                // we have tried 5 times and failed so let's cache the normal address
                var newCachedImage = new CachedImage { WebUrl = cropUrl };
                newCachedImage.Resolved = false;
                newCachedImage.CacheUrl = cropUrl;
                Cache.InsertCacheItem<CachedImage>(cacheKey, () => newCachedImage);

                fullUrlPath = cropUrl;
            }

            return htmlEncode ? new HtmlString(HttpUtility.HtmlEncode(fullUrlPath)) : new HtmlString(fullUrlPath);
        }

        /// <summary>
        /// Tries to execute a delegate action five times.
        /// </summary>
        /// <param name="delegateAction">The delegate to be executed</param>
        private static void TryFiveTimes(Action delegateAction)
        {
            for (int retry = 0; ; retry++)
            {
                try
                {
                    delegateAction();
                    return;
                }
                catch (Exception)
                {
                    if (retry >= 5)
                    {
                        throw;
                    }
                }
            }
        }

        // This method is copied from Umbraco v7.4 as we need to support Umbraco v7.3 for the time being
        private static string GetCropUrl(
            this string imageUrl,
            ImageCropDataSet cropDataSet,
            int? width = null,
            int? height = null,
            string cropAlias = null,
            int? quality = null,
            ImageCropMode? imageCropMode = null,
            ImageCropAnchor? imageCropAnchor = null,
            bool preferFocalPoint = false,
            bool useCropDimensions = false,
            string cacheBusterValue = null,
            string furtherOptions = null,
            ImageCropRatioMode? ratioMode = null,
            bool upScale = true)
        {
            if (string.IsNullOrEmpty(imageUrl) == false)
            {
                var imageProcessorUrl = new StringBuilder();

                if (cropDataSet != null && (imageCropMode == ImageCropMode.Crop || imageCropMode == null))
                {
                    var crop = cropDataSet.GetCrop(cropAlias);

                    imageProcessorUrl.Append(cropDataSet.Src);

                    var cropBaseUrl = cropDataSet.GetCropBaseUrl(cropAlias, preferFocalPoint);
                    if (cropBaseUrl != null)
                    {
                        imageProcessorUrl.Append(cropBaseUrl);
                    }
                    else
                    {
                        return null;
                    }

                    if (crop != null & useCropDimensions)
                    {
                        width = crop.Width;
                        height = crop.Height;
                    }

                    // If a predefined crop has been specified & there are no coordinates & no ratio mode, but a width parameter has been passed we can get the crop ratio for the height
                    if (crop != null && string.IsNullOrEmpty(cropAlias) == false && crop.Coordinates == null && ratioMode == null && width != null && height == null)
                    {
                        var heightRatio = (decimal)crop.Height / (decimal)crop.Width;
                        imageProcessorUrl.Append("&heightratio=" + heightRatio.ToString(CultureInfo.InvariantCulture));
                    }

                    // If a predefined crop has been specified & there are no coordinates & no ratio mode, but a height parameter has been passed we can get the crop ratio for the width
                    if (crop != null && string.IsNullOrEmpty(cropAlias) == false && crop.Coordinates == null && ratioMode == null && width == null && height != null)
                    {
                        var widthRatio = (decimal)crop.Width / (decimal)crop.Height;
                        imageProcessorUrl.Append("&widthratio=" + widthRatio.ToString(CultureInfo.InvariantCulture));
                    }
                }
                else
                {
                    imageProcessorUrl.Append(imageUrl);

                    if (imageCropMode == null)
                    {
                        imageCropMode = ImageCropMode.Pad;
                    }

                    imageProcessorUrl.Append("?mode=" + imageCropMode.ToString().ToLower());

                    if (imageCropAnchor != null)
                    {
                        imageProcessorUrl.Append("&anchor=" + imageCropAnchor.ToString().ToLower());
                    }
                }

                if (quality != null)
                {
                    imageProcessorUrl.Append("&quality=" + quality);
                }

                if (width != null && ratioMode != ImageCropRatioMode.Width)
                {
                    imageProcessorUrl.Append("&width=" + width);
                }

                if (height != null && ratioMode != ImageCropRatioMode.Height)
                {
                    imageProcessorUrl.Append("&height=" + height);
                }

                if (ratioMode == ImageCropRatioMode.Width && height != null)
                {
                    // if only height specified then assume a sqaure
                    if (width == null)
                    {
                        width = height;
                    }

                    var widthRatio = (decimal)width / (decimal)height;
                    imageProcessorUrl.Append("&widthratio=" + widthRatio.ToString(CultureInfo.InvariantCulture));
                }

                if (ratioMode == ImageCropRatioMode.Height && width != null)
                {
                    // if only width specified then assume a sqaure
                    if (height == null)
                    {
                        height = width;
                    }

                    var heightRatio = (decimal)height / (decimal)width;
                    imageProcessorUrl.Append("&heightratio=" + heightRatio.ToString(CultureInfo.InvariantCulture));
                }

                if (upScale == false)
                {
                    imageProcessorUrl.Append("&upscale=false");
                }

                if (furtherOptions != null)
                {
                    imageProcessorUrl.Append(furtherOptions);
                }

                if (cacheBusterValue != null)
                {
                    imageProcessorUrl.Append("&rnd=").Append(cacheBusterValue);
                }

                return imageProcessorUrl.ToString();
            }

            return string.Empty;
        }


    }
}
