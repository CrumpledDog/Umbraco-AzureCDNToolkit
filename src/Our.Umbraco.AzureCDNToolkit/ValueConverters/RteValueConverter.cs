namespace Our.Umbraco.AzureCDNToolkit.ValueConverters
{
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Collections.Specialized;

    using global::Umbraco.Core;
    using global::Umbraco.Web;
    using global::Umbraco.Core.Models.PublishedContent;
    using global::Umbraco.Core.PropertyEditors;
    using global::Umbraco.Web.PropertyEditors.ValueConverters;

    using HtmlAgilityPack;

    [PropertyValueType(typeof(IHtmlString))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    public class RteValueConverter : RteMacroRenderingValueConverter
    {
        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null)
            {
                return null;
            }

            var coreConversion = base.ConvertDataToSource(
            propertyType,
            source,
            preview);

            var doc = new HtmlDocument();
            doc.LoadHtml(coreConversion.ToString());

            if (!doc.ParseErrors.Any() && doc.DocumentNode != null)
            {
                // TODO parse links for downloads from CDN directly?

                // Find all images with rel attribute
                var imgNodes = doc.DocumentNode.SelectNodes("//img");

                if (imgNodes != null)
                {
                    var modified = false;

                    foreach (var img in imgNodes)
                    {
                        var srcAttr = img.Attributes.FirstOrDefault(x => x.Name == "src");
                        var idAttr = img.Attributes.FirstOrDefault(x => x.Name == "data-id");

                        if (srcAttr != null)
                        {
                            // html decode the url as variables encoded in tinymce
                            var src = HttpUtility.HtmlDecode(srcAttr.Value);
                            var resolvedSrc = string.Empty;

                            var hasQueryString = src.InvariantContains("?");
                            var querystring = new NameValueCollection();

                            if (hasQueryString)
                            {
                                querystring = HttpUtility.ParseQueryString(src.Substring(src.IndexOf('?')));
                            }


                            // can only resolve ImageProcessor Azure Cache Urls if resolvable domain is set
                            if (AzureCdnToolkit.Instance.Domain != null)
                            {

                                if (idAttr != null)
                                {
                                    // Umbraco media
                                    int nodeId;
                                    if (int.TryParse(idAttr.Value, out nodeId))
                                    {
                                        var node = UmbracoContext.Current.MediaCache.GetById(nodeId);

                                        if (hasQueryString)
                                        {
                                            resolvedSrc = new UrlHelper().ResolveCdnFallback(node, asset:false, querystring: querystring.ToString(), fallbackImage:src).ToString();
                                        }
                                        else
                                        {
                                            resolvedSrc = new UrlHelper().ResolveCdnFallback(node, asset:false, fallbackImage:src).ToString();
                                        }
                                    }
                                }
                                else
                                {
                                    // Image in TinyMce doesn't have a data-id attribute so lets add package cache buster
                                    resolvedSrc = new UrlHelper().ResolveCdn(src, asset:false).ToString();
                                }

                                // If the resolved url is different to the orginal change the src attribute
                                if (resolvedSrc != string.Format("{0}{1}", AzureCdnToolkit.Instance.Domain, src))
                                {
                                    srcAttr.Value = resolvedSrc;
                                    modified = true;
                                }
                            }

                        }
                    }

                    if (modified)
                    {
                        return doc.DocumentNode.OuterHtml;
                    }
                }
            }

            return coreConversion;
        }

    }
}