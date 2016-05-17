namespace Our.Umbraco.AzureCDNToolkit.CacheRefreshers
{
    using System;
    using global::Umbraco.Core.Cache;

    public class CacheWiper : JsonCacheRefresherBase<CacheWiper>
    {
        public static Guid Guid
        {
            get { return new Guid("8882A4B1-69C5-4B41-B578-C65E6F630A97"); }
        }

        protected override CacheWiper Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return Guid; }
        }

        public override string Name
        {
            get { return "AzureCDNToolKitCacheWiper"; }
        }
    }
}
