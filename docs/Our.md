The AzureCDNToolkit package allows you to fully utilise and integrate the Azure CDN with your Umbraco powered website. There are three file types that should be served from CDN if you have one. 

- Assets - css, js & static images used by templates etc..
- Images managed by Umbraco - cropped or not
- Files - pdfs, docx etc

The toolkit depends on two packages being installed, these are the [UmbracoFileSystemProviders.Azure](https://github.com/JimBobSquarePants/UmbracoFileSystemProviders.Azure) and the [ImageProcessor.Web Azure Blob Cache plugin](http://imageprocessor.org/imageprocessor-web/plugins/azure-blob-cache/)

Once installed and setup the package provides UrlHelper methods to use for resolving url paths for assets and Image Cropper urls and also a value converter for the TinyMce editor so that images within the content are also resolved.

Some examples:
	
	@Url.ResolveCdn("/css/style.css")
	
	@Url.GetCropCdnUrl(Umbraco.TypedMedia(1084), width: 150)
    
	<div class="brand" style="background-image: url('@Url.ResolveCdn(home.GetPropertyValue<string>("siteLogo") + "?height=65&width=205&bgcolor=000", false, false)')"></div>

When using these methods, the toolkit will attempt to resolve the urls to their **absolute** paths and **crucially** ensures that a cache busting querystring variable is added. Without the cache busting using the Azure CDN can become tricky when you want to update with new content. This has the added benefit of avoiding your site needing to handle any 301 redirects and also gets some optimisation benefit (PageSpeed etc).

Some examples:

1. `<link rel="stylesheet" type="text/css" href="/css/bootstrap.min.css">`
2. `<img src="/media/1052/jan.jpg?anchor=center&mode=crop&width=150&rnd=131070397620000000"/>`
3. `<a href="/media/1050/blank.docx">Download Word Doc</a>`

Becomes:

1. `<link rel="stylesheet" type="text/css" href="https://azurecdntoolkitdemo.azureedge.net/assets/css/bootstrap.min.css?v=0.0.1">`
2. `<img src="https://azurecdntoolkitdemo.azureedge.net/cloudcache/e/0/2/7/d/2/e027d2acab49ab523db3ece72c0651310dd3320c.jpg"/>`
3. `<a href="https://azurecdntoolkitdemo.azureedge.net/assets/media/1050/blank.docx?rnd=131070387800000000">Download Word Doc</a>`

**Note**: this package is not compatible with [Slimsy](https://our.umbraco.org/projects/website-utilities/slimsy/) v1 (it should work with the upcoming Slimsy v2 using img srcset)

## NuGet ##

It is highly recommended that you install this package via NuGet:

    Install-Package Our.Umbraco.AzureCDNToolkit -Pre

## Documentation ##

1. [Azure Setup](https://github.com/CrumpledDog/Umbraco-AzureCDNToolkit/blob/develop/docs/Azure-Setup.md)
2. [Umbraco Setup](https://github.com/CrumpledDog/Umbraco-AzureCDNToolkit/blob/develop/docs/Umbraco-Setup.md)
3. [Umbraco Implementation](https://github.com/CrumpledDog/Umbraco-AzureCDNToolkit/blob/develop/docs/Umbraco-Implementation.md)
4. [Umbraco Dashboard](https://github.com/CrumpledDog/Umbraco-AzureCDNToolkit/blob/develop/docs/Umbraco-Dashboard.md)
5. [Reference](https://github.com/CrumpledDog/Umbraco-AzureCDNToolkit/blob/develop/docs/Reference.md)