using Calabonga.OperationResults;
using Catalog.Contracts.Dto;
using Catalog.Redis.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Catalog.Redis
{
    public class RedisService<TDto>
        where TDto : class
    {
        private readonly IDistributedCache _cache;
        private readonly IOptions<RedisConfiguration> _configuration;

        public RedisService(
            IDistributedCache cache,
            IOptions<RedisConfiguration> configuration)
        {
            _configuration = configuration;
            _cache = cache;
        }

        public async Task<Operation<IEnumerable<TDto>, string>> GetFromCacheAsync(string cacheKey, CancellationToken cancellationToken)
        {
            var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (string.IsNullOrEmpty(cachedData))
                return Operation.Error("Data not found!");

            var models = JsonSerializer.Deserialize<List<TDto>>(cachedData);

            if (models is null)
                return Operation.Error("Data not found!");

            return models;
        }

        public async Task<Operation<PagedResponseDto<TDto>, string>> GetPagedResponseDtoFromCacheAsync(string cacheKey, CancellationToken cancellationToken)
        {
            var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (string.IsNullOrEmpty(cachedData))
                return Operation.Error("Data not found!");

            var models = JsonSerializer.Deserialize<PagedResponseDto<TDto>>(cachedData);

            if (models is null)
                return Operation.Error("Data not found!");

            return models;
        }

        public async Task<Operation<bool, string>> SendToCacheAsync(string cacheKey, IEnumerable<TDto> data, CancellationToken cancellationToken)
        {
            var invalidateCacheResult = await InvalidateCacheAsync(cacheKey, cancellationToken);

            if (!invalidateCacheResult.Ok)
            {
                return Operation.Error(invalidateCacheResult.Error);
            }

            if (data is null)
            {
                return Operation.Error("Data not found!");
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

        public async Task<Operation<bool, string>> SendToCacheAsync(string cacheKey, PagedResponseDto<TDto> data, CancellationToken cancellationToken)
        {
            var invalidateCacheResult = await InvalidateCacheAsync(cacheKey, cancellationToken);

            if (!invalidateCacheResult.Ok)
            {
                return Operation.Error(invalidateCacheResult.Error);
            }

            if (data is null)
            {
                return Operation.Error("Data not found!");
            }

            var cacheDuration = TimeSpan.FromMinutes(
                _configuration.Value?.CacheDurationMinutes ?? 15);

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

        public async Task<Operation<bool, string>> InvalidateCacheAsync(string cacheKey, CancellationToken cancellationToken)
        {
            try
            {
                await _cache.RemoveAsync(cacheKey, cancellationToken);
            }
            catch (Exception ex)
            {
                return Operation.Error(ex.Message);
            }

            return true;
        }

        public string GenerateCacheKey(params (string Key, object Value)[] parameters)
        {
            List<string> parts = new();

            parts.Add($"{"entity"}:{typeof(TDto).Name}");

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

            return string.Join(":", parts);
        }
    }
}