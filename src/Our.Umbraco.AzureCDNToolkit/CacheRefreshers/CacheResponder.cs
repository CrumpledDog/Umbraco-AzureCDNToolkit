namespace Our.Umbraco.AzureCDNToolkit.CacheRefreshers
{
    using System;
    using global::Umbraco.Core.Cache;

    public class CacheResponder : JsonCacheRefresherBase<CacheResponder>
    {
        public static Guid Guid
        {
            get { return new Guid("A4EDE1C6-C73B-4DB2-ADC9-23C22B2152F9"); }
        }

        protected override CacheResponder Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return Guid; }
        }

        public override string Name
        {
            get { return "AzureCDNToolKitCacheResponder"; }
        }
    }
}
