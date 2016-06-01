namespace Our.Umbraco.AzureCDNToolkit.CacheRefreshers
{
    using System;
    using global::Umbraco.Core.Cache;
    public class CacheRequester : JsonCacheRefresherBase<CacheRequester>
    {
        public static Guid Guid
        {
            get { return new Guid("2A310ECC-D050-464D-9BED-2C9448255E01"); }
        }

        protected override CacheRequester Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return Guid; }
        }

        public override string Name
        {
            get { return "AzureCDNToolKitCacheReporter"; }
        }
    }
}
