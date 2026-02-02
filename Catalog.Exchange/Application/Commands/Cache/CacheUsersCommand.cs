using Catalog.Contracts.Dto.Authorization;
using Catalog.Contracts.Interfaces;

namespace Catalog.ExchangeService.Application.Commands.Cache
{
    public sealed record CacheUsersCommand(string CacheKey, IEnumerable<UserDto> Users) : IOrderQueueEvent;
}
