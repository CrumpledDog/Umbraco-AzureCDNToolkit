# Umbraco Azure CDN Toolkit #

The AzureCDNToolkit package allows you to fully utilise and integrate the Azure CDN with your Umbraco powered website. There are three types files that should be served from CDN if you have one. 

- Assets - css, js & static images used by templates etc..
- Images managed by Umbraco - cropped or not
- Files - pdfs, docx etc

The toolkit depends on two packages being installed, these are the [UmbracoFileSystemProviders.Azure](https://github.com/JimBobSquarePants/UmbracoFileSystemProviders.Azure) and the [ImageProcessor.Web Azure Blob Cache plugin](http://imageprocessor.org/imageprocessor-web/plugins/azure-blob-cache/)

Once installed and setup the package provides UrlHelper methods to use for resolving url paths and for getting Image Cropper urls.

Some examples:
	
	@Url.ResolveCdn("/css/style.css")
	
	@Url.GetCropCdnUrl(Umbraco.TypedMedia(1084), width: 150)
    
	<div class="brand" style="background-image: url('@Url.ResolveCdn(home.GetPropertyValue<string>("siteLogo") + "?height=65&width=205&bgcolor=000", false, false)')"></div>

[![Build status](https://ci.appveyor.com/api/projects/status/7lj6r6uoofm9mb24?svg=true)](https://ci.appveyor.com/project/JeavonLeopold/umbraco-azurecdntoolkit)

## Documentation ##

1. [Azure Setup](docs/Azure-Setup.md)
2. [Umbraco Setup](docs/Umbraco-Setup.md)

## Installation ##

Both NuGet and Umbraco packages are available. 

|NuGet Packages    |Version           |
|:-----------------|:-----------------|
|**Release**|[![NuGet download](http://img.shields.io/nuget/v/Our.Umbraco.AzureCDNToolkit.svg)](https://www.nuget.org/packages/Our.Umbraco.AzureCDNToolkit/)
|**Pre-release**|[![MyGet download](https://img.shields.io/myget/umbraco-packages/vpre/Our.Umbraco.AzureCDNToolkit.svg)](https://www.myget.org/gallery/umbraco-packages)

|Umbraco Packages  |                  |
|:-----------------|:-----------------|
|**Release**|[![Our Umbraco project page](https://img.shields.io/badge/our-umbraco-orange.svg)](https://our.umbraco.org/projects/collaboration/azurecdntoolkit/) 
|**Pre-release**| [![AppVeyor Artifacts](https://img.shields.io/badge/appveyor-umbraco-orange.svg)](https://ci.appveyor.com/project/JeavonLeopold/umbraco-azurecdntoolkit/build/artifacts)