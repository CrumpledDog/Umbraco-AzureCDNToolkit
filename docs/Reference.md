# Reference #

## GetCropCdnUrl ##

### IPublishedContent ###

This method is a direct replacement of the Umbraco UrlHelper `GetCropUrl` method

	IHtmlString GetCropCdnUrl(this UrlHelper urlHelper,
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

### ImageCropDataSet ###

This method is useful if you have strongly typed models (e.g. Ditto) with ImageCropDataSet image cropper properties.

	 IHtmlString GetCropCdnUrl(this UrlHelper urlHelper,
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

If possible you should try to supply a cacheBusterValue ideally derived from the Umbraco UpdateDate. 

e.g. 

	var cacheBusterValue = MyModel.UpdateDate.ToFileTimeUtc().ToString(CultureInfo.InvariantCulture);

## ResolveCdn ##

### IPublishedContent ###

	IHtmlString ResolveCdn(this UrlHelper urlHelper, IPublishedContent mediaItem, bool asset = true, string querystring = null, bool htmlEncode = true)

### String ###

	IHtmlString ResolveCdn(this UrlHelper urlHelper, string path, bool asset = true, bool htmlEncode = true)

### String Advanced ###

	IHtmlString ResolveCdn(this UrlHelper urlHelper, string path, string cacheBuster, bool asset = true, string cacheBusterName = "v", bool htmlEncode = true)