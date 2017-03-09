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

            // If toolkit is disabled then return base conversion
            if (!AzureCdnToolkit.Instance.UseAzureCdnToolkit)
            {
                return coreConversion;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(coreConversion.ToString());

            if (doc.ParseErrors.Any() || doc.DocumentNode == null)
            {
                return coreConversion;
            }

            var modified = false;

            ResolveUrlsForElement(doc, "img", "src", "data-id", false, false, ref modified);
            ResolveUrlsForElement(doc, "a", "href", "data-id", true, true, ref modified);

            return modified ? doc.DocumentNode.OuterHtml : coreConversion;
        }

        private static void ResolveUrlsForElement(HtmlDocument doc, string elementName, string attributeName, string idAttributeName, bool idAttributeMandatory, bool asset, ref bool modified)
        {
            var htmlNodes = doc.DocumentNode.SelectNodes(string.Concat("//", elementName));

            if (htmlNodes == null)
            {
                return;
            }

            foreach (var htmlNode in htmlNodes)
            {
                var urlAttr = htmlNode.Attributes.FirstOrDefault(x => x.Name == attributeName);
                var idAttr = htmlNode.Attributes.FirstOrDefault(x => x.Name == idAttributeName);

                if (urlAttr == null || (idAttributeMandatory && idAttr == null))
                {
                    continue;
                }

                // html decode the url as variables encoded in tinymce
                var src = HttpUtility.HtmlDecode(urlAttr.Value);
                var resolvedSrc = string.Empty;

                var hasQueryString = src.InvariantContains("?");
                var querystring = new NameValueCollection();

                if (hasQueryString && src != null)
                {
                    querystring = HttpUtility.ParseQueryString(src.Substring(src.IndexOf('?')));
                }


                // can only resolve ImageProcessor Azure Cache Urls if resolvable domain is set
                if (AzureCdnToolkit.Instance.Domain == null)
                {
                    continue;
                }
                if (idAttr != null)
                {
                    // Umbraco media
                    int nodeId;
                    if (int.TryParse(idAttr.Value, out nodeId))
                    {
                        var node = UmbracoContext.Current.MediaCache.GetById(nodeId) ??
                                   UmbracoContext.Current.ContentCache.GetById(nodeId);

                        if (node != null)
                        {
                            if (hasQueryString)
                            {
                                resolvedSrc =
                                    new UrlHelper().ResolveCdnFallback(node, asset: asset,
                                        querystring: querystring.ToString(), fallbackImage: src).ToString();
                            }
                            else
                            {
                                resolvedSrc =
                                    new UrlHelper().ResolveCdnFallback(node, asset: asset, fallbackImage: src)
                                        .ToString();
                            }
                        }
                    }
                }
                else
                {
                    // Image in TinyMce doesn't have a data-id attribute so lets add package cache buster
                    resolvedSrc = new UrlHelper().ResolveCdn(src, asset: asset).ToString();
                }

                // If the resolved url is different to the orginal change the src attribute
                if (resolvedSrc == string.Concat(AzureCdnToolkit.Instance.Domain, src))
                {
                    continue;
                }

                urlAttr.Value = resolvedSrc;
                modified = true;
            }
        }
    }
}