using Calabonga.OperationResults;
using Catalog.Contracts.Dto;

namespace Catalog.Contracts.Interfaces
{
    public interface ICacheService<TDto> 
        where TDto : class
    {
        Task<Operation<IEnumerable<TDto>, string>> GetFromCacheAsync(string cacheKey, CancellationToken cancellationToken);
        Task<Operation<PagedResponseDto<TDto>, string>> GetPagedResponseDtoFromCacheAsync(string cacheKey, CancellationToken cancellationToken);
        Task<Operation<bool, string>> SendToCacheAsync(string cacheKey, IEnumerable<TDto> data, CancellationToken cancellationToken);
        Task<Operation<bool, string>> SendToCacheAsync(string cacheKey, PagedResponseDto<TDto> data, CancellationToken cancellationToken);
        Task<Operation<bool, string>> InvalidateCacheAsync(string cacheKey, CancellationToken cancellationToken);
        string GenerateCacheKey(params (string Key, object Value)[] parameters);
    }
}
