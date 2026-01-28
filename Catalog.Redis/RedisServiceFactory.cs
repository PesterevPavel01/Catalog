using Catalog.Redis.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Catalog.Redis
{
    public sealed class RedisServiceFactory
    {
        #region fields

        private readonly IDistributedCache _cache;
        private readonly IOptions<RedisConfiguration> _configuration;
        private Dictionary<Type, object>? _services;
        private bool _disposed;

        #endregion

        #region Methods

        public RedisServiceFactory(
            IDistributedCache cache,
            IOptions<RedisConfiguration> configuration)
        {
            _configuration = configuration;
            _cache = cache;
        }

        public RedisService<TDto> GetService<TDto>()
            where TDto : class
        {
            _services ??= [];

            var type = typeof(TDto);
            if (!_services.ContainsKey(type))
            {
                _services[type] = new RedisService<TDto>(_cache, _configuration);
            }

            return (RedisService<TDto>)_services[type];
        }

        #endregion

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            //ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">The disposing.</param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _services?.Clear();
                }
            }
            _disposed = true;
        }
    }
}
