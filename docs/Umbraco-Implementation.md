# Umbraco Implementation #

Now you have everything setup and installed you are ready to begin using the Umbraco Azure CDN Toolkit.

## 1. Cropped images ##

Where you would normally use the Umbraco `GetCropUrl` UrlHelper method instead you now use `GetCropCdnUrl`.

e.g.

	<img src="@Url.GetCropUrl(Umbraco.TypedMedia(1084), width: 150)"/>

change to:

	<img src="@Url.GetCropCdnUrl(Umbraco.TypedMedia(1084), width: 150)"/>

With the toolkit enabled, you should now see your cropped image paths change to absolute CDN references.

e.g. 

	<img src="/media/1052/jan.jpg?anchor=center&mode=crop&width=150&rnd=131070397620000000"/>

change to:

	<img src="https://azurecdntoolkit.azureedge.net/cloudcache/e/0/2/7/d/2/e027d2acab49ab523db3ece72c0651310dd3320c.jpg"/>

If you turn off the toolkit in web.config it should render as it would if you had used `GetCropUrl` directly, useful for development!

Internally cache busting has been added, you can view this in the [dashboard](Umbraco-Dashboard.md).

## 2. Static assets ##

Where you would normally reference css, js or static images you can now use a UrlHelper method called `ResolveCdn`.

e.g.

	<link rel="stylesheet" type="text/css" href="/css/bootstrap.min.css">

change to:

    <link rel="stylesheet" type="text/css" href="@Url.ResolveCdn("/css/bootstrap.min.css")">

With the toolkit enabled, you should now see your asset paths change to absolute CDN reference.

e.g.

	<link rel="stylesheet" type="text/css" href="https://azurecdntoolkit.azureedge.net/assets/css/bootstrap.min.css?v=0.0.1">

You can see that the toolkit has also added that all important cachebuster variable.

## 3. Downloads ##

Where you might normally render a link to download a pdf or docx stored in Umbraco media you can now use the `ResolveCdn` UrlHelper

e.g.

	<a href="@Umbraco.TypedMedia(1081).Url">Download Word Doc</a>

change to:

	<a href="@Url.ResolveCdn(Umbraco.TypedMedia(1081))">Download Word Doc</a>

With the toolkit enabled, you should now see your media paths change to absolute CDN reference.

e.g.

	<a href="https://azurecdntoolkit.azureedge.net/assets/media/1050/blank.docx?rnd=131070387800000000">Download Word Doc</a>

You can see that the toolkit has also added that all important cachebuster variable.

## 4. Static assets with ImageProcessor.Web commands ##

Sometimes there might be some images with ImageProcessor.Web commands.

e.g. 

	<img src="/images/FSUK.jpg?width=100"/>

change to:

	<img src="@Url.ResolveCdn("/images/FSUK.jpg?width=100", asset: false)"/>

With the toolkit enabled, you should now see your image paths change to absolute CDN reference, the result will of course be the output from ImageProcessor.Web

e.g.

	<img src="https://azurecdntoolkit.azureedge.net/cloudcache/a/9/f/6/7/0/a9f6707b344b1e1c22c054ca86c1fc06233149ad.jpg"/>

Internally cache busting has been added, you can view this in the [dashboard](Umbraco-Dashboard.md).
