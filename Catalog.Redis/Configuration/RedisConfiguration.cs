namespace Catalog.Redis.Configuration
{
    public sealed class RedisConfiguration
    {
        public bool CacheEnabled { get; set; } = true;

        public int CacheDurationMinutes { get; set; } = 2;
    }
}