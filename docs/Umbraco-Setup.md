# Umbraco Setup #

## Prerequisites ##

You will need to have a Visual Studio Web Application project with Umbraco v7.3.8 or above installed.

Ensure your content managed image type media items are using a Image Cropper for the `umbracoFile` property (this is default in Umbraco v7.4+)

![Image Type Cropper](images/image-type-cropper.png)

## 1. ImageProcessor.Web.Config ##

Install this package via NuGet, it will add three configuration files to your solution.

NuGet Package:

	Install-Package ImageProcessor.Web.Config  

## 2. UmbracoFileSystemProviders.Azure ##

Use v0.5.2-beta or above

Install the UmbracoFileSystemProviders.Azure package. Recommended that you first install using the Umbraco package and then install the NuGet package over the top, configuration will be maintained, this is to take advantage of the Umbraco installer configuring everything needed automatically.

[Download Umbraco package](https://our.umbraco.org/projects/collaboration/umbracofilesystemprovidersazure/)

NuGet Package:

	Install-Package UmbracoFileSystemProviders.Azure -Pre  

When running the configuration in the Umbraco installer be sure to use the key and account name from [Step 2 of the Azure Setup.](Azure-Setup.md)
    
![Installing Umbraco File System Providers](images/umbraco-azure-filesystem-provider-install.png)

**Note:** Be sure to leave the "UseDefaultRoute" checked

## 3. ImageProcessor.Web.Plugins.AzureBlobCache

Install this package via NuGet

NuGet Package:

    Install-Package ImageProcessor.Web.Plugins.AzureBlobCache

Once installed you will need to edit /Config/imageprocessor/cache.config

It should look like this:

	  <caching currentCache="DiskCache">
	    <caches>
	      <cache name="DiskCache" type="ImageProcessor.Web.Caching.DiskCache, ImageProcessor.Web" maxDays="365">
	        <settings>
	          <setting key="VirtualCachePath" value="~/app_data/cache" />
	        </settings>
	      </cache>
	      <cache name="AzureBlobCache" type="ImageProcessor.Web.Plugins.AzureBlobCache.AzureBlobCache, ImageProcessor.Web.Plugins.AzureBlobCache" maxDays="365">
	        <settings>
	          <setting key="CachedStorageAccount" value="DefaultEndpointsProtocol=https;AccountName=[CacheAccountName];AccountKey=[CacheAccountKey]"/>
	          <setting key="CachedBlobContainer" value="cache"/>
	          <setting key="UseCachedContainerInUrl" value="true"/>
	          <setting key="CachedCDNRoot" value="[CdnRootUrl]"/>
	          <setting key="SourceStorageAccount" value=""/>
	          <setting key="SourceBlobContainer" value=""/>
	          <setting key="StreamCachedImage" value="false"/>
	        </settings>
	      </cache>
	    </caches>
	  </caching>

In the AzureBlobCache section you need to update the following keys:

1. CachedStorageAccount set to the key and account name from [Step 2 of the Azure Setup](Azure-Setup.md).
2. CachedBlobContainer change this if you want to (e.g. cloudcache), it will be included in the path of cropped images.
3. CachedCDNRoot set to name of your CDN endpoint from [Step 5 of the Azure Setup](Azure-Setup.md).

Now change:

	<caching currentCache="DiskCache">

To

	<caching currentCache="AzureBlobCache">

This will enable ImageProcessor.Web to store it's cache files in your storage account container.

## 4. Our.Umbraco.AzureCDNToolkit ##

NuGet Package:

    Install-Package Our.Umbraco.AzureCDNToolkit

## 5. Upload assets to the "assets" container

Upload your static assets to the container created in [Step 7 of the Azure Setup](Azure-Setup.md).

![Upload assets](images/upload-assets.png)
