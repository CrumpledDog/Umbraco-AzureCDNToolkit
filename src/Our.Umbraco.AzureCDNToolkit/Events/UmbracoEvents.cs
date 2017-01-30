namespace Our.Umbraco.AzureCDNToolkit.Events
{
    using System.Linq;
    using System.Web;
    using System.Web.Configuration;

    using global::Umbraco.Core;
    using global::Umbraco.Core.Security;
    using global::Umbraco.Core.PropertyEditors;
    using global::Umbraco.Web.PropertyEditors.ValueConverters;

    using ImageProcessor.Web.HttpModules;
    public class UmbracoEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            PropertyValueConvertersResolver.Current.RemoveType<RteMacroRenderingValueConverter>();
        }

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ImageProcessingModule.ValidatingRequest += ImageProcessingModule_ValidatingRequest;
        }

        private void ImageProcessingModule_ValidatingRequest(object sender, ImageProcessor.Web.Helpers.ValidatingRequestEventArgs args)
        {
            var securityToken = WebConfigurationManager.AppSettings["AzureCDNToolkit:SecurityToken"];
            var useAzureCdnToolkit = bool.Parse(WebConfigurationManager.AppSettings["AzureCDNToolkit:UseAzureCdnToolkit"]);

            if (useAzureCdnToolkit && !string.IsNullOrWhiteSpace(args.QueryString) && !string.IsNullOrEmpty(securityToken))
            {
                var queryCollection = HttpUtility.ParseQueryString(args.QueryString);

                // if token is not present or value doesn't match then we can cancel the request
                if (!queryCollection.AllKeys.Contains("securitytoken") || queryCollection["securitytoken"] != securityToken)
                {
                    // we can allow on-demand image processor requests if the user has a umbraco auth ticket which means they are logged into Umbraco for things like grid editor previews
                    var ticket = new HttpContextWrapper(HttpContext.Current).GetUmbracoAuthTicket();
                    if (ticket == null)
                    {
                        args.Cancel = true;
                    }
                }
            }
        }
    }
}
