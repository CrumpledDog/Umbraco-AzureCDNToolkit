namespace Our.Umbraco.AzureCDNToolkit.Events
{
    using System.Collections.Generic;
    using System.Linq;

    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using global::Umbraco.Core;
    using global::Umbraco.Web;
    using global::Umbraco.Web.UI.JavaScript;

    using Controllers;

    public class ServerVariableParser : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ServerVariablesParser.Parsing += ServerVariablesParser_Parsing;
        }

        void ServerVariablesParser_Parsing(object sender, Dictionary<string, object> e)
        {
            if (HttpContext.Current == null) return;
            var urlHelper = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()));

            var mainDictionary = new Dictionary<string, object>
            {
                {
                    "cacheApiBaseUrl",
                    urlHelper.GetUmbracoApiServiceBaseUrl<CacheApiController>(controller => controller.GetAllServers())
                }
            };

            if (!e.Keys.Contains("azureCdnToolkitUrls"))
            {
                e.Add("azureCdnToolkitUrls", mainDictionary);
            }
        }
    }
}
