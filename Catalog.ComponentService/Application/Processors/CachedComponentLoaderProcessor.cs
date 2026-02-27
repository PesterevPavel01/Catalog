namespace Catalog.ComponentService.Application.Processors
{
    public class CachedComponentLoaderProcessor
    {/*
        private readonly IDistributedCache _cache;
        private readonly ILogger<CachedComponentLoaderProcessor> _logger;
        private readonly IOptions<ComponentConfiguration> _configuration;

        public CachedComponentLoaderProcessor(
            IDistributedCache cache,
            ILogger<CachedComponentLoaderProcessor> logger,
            IOptions<ComponentConfiguration> configuration)
        {
            _cache = cache;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<Operation<List<ComponentDto>,string>> GetComponentsAsync(
            string cacheKey,
            CancellationToken cancellationToken = default)
        {
            // Проверяем, включено ли кеширование
            if (!(_configuration.Value?.CacheEnabled ?? false))
            {
                return Operation.Error("Cache disabled!");
            }

            var cachedResult = await GetFromCacheAsync(cacheKey);

            if (!cachedResult.Ok)
                return Operation.Error(cachedResult.Error);

            return cachedResult;

        }

        private async Task<Operation<List<ComponentDto>, string>> GetFromCacheAsync(string cacheKey)
        {
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(cachedData))
                return Operation.Error("Data not found!");

            var models = JsonSerializer.Deserialize<List<ComponentDto>>(cachedData);

            if(models is null)
                return Operation.Error("Data not found!");

            return models;
        }

        public async Task<Operation<bool, string>> SendToCacheAsync(string cacheKey, List<ComponentDto> data, CancellationToken cancellationToken)
        {
            var invalidateCacheResult = await InvalidateCacheAsync(cacheKey);

            if (!invalidateCacheResult.Ok) { 
                return Operation.Error(invalidateCacheResult.Error);
            }

            if (data is null)
            {
                return Operation.Error("Components not found!");
            }

            var cacheDuration = TimeSpan.FromMinutes(
                _configuration.Value?.CacheDurationMinutes ?? 2);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheDuration,
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            try
            {
                var serializedData = JsonSerializer.Serialize(data);

                await _cache.SetStringAsync(cacheKey, serializedData, options);
            }
            catch (Exception ex)
            {
                return Operation.Error(ex.Message);
            }

            return true;
        }

        private async Task<Operation<bool,string>> InvalidateCacheAsync(string cacheKey)
        {
            try
            {
                await _cache.RemoveAsync(cacheKey);
            }
            catch (Exception ex)
            {
                return Operation.Error(ex.Message);
            }

            return true;
        }

        public string GenerateCacheKey(params (string Key, object Value)[] parameters)
        {
            var parts = new List<string> { "components" };

            foreach (var parameter in parameters)
            {
                if (parameter.Value != null)
                {
                    var valueString = parameter.Value.ToString();

                    if (!string.IsNullOrEmpty(valueString))
                    {
                            // Экранируем специальные символы
                            var safeValue = valueString.Replace(":", "_").Replace(" ", "_");
                            parts.Add($"{parameter.Key}:{safeValue}");
                    }
                }
            }

            parts.Add($"v:1.0");
            return string.Join(":", parts);
        }*/
    }
}
