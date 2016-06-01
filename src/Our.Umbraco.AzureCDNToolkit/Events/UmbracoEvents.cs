namespace Our.Umbraco.AzureCDNToolkit.Events
{
    using global::Umbraco.Core;
    using global::Umbraco.Core.PropertyEditors;
    using global::Umbraco.Web.PropertyEditors.ValueConverters;
    public class UmbracoEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            PropertyValueConvertersResolver.Current.RemoveType<RteMacroRenderingValueConverter>();
        }
    }
}
