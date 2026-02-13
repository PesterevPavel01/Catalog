using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Catalog.Contracts.Dto.Authorization;
using Catalog.Domain.Entities.Authorization;
using Catalog.ExchangeService.Application.Commands.Cache;
using Catalog.Redis;
using Rebus.Bus;

namespace Catalog.ExchangeService.Application.Handlers.Users
{
    public sealed class CachedUsersQueryHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RedisService<UserDto> _redisService;
        private readonly IBus _bus;

        public CachedUsersQueryHandler(IUnitOfWork unitOfWork, RedisServiceFactory redisServiceFactory, IBus bus)
        {
            _unitOfWork = unitOfWork;
            _redisService = redisServiceFactory.GetService<UserDto>(); ;
            _bus = bus;
        }

        public async Task<Operation<IEnumerable<UserDto>, string>> HandleAsync(CancellationToken cancellationToken)
        {
            IEnumerable<UserDto> users = [];

            var cacheKey = _redisService.GenerateCacheKey();

            var cachedUsers = await _redisService.GetFromCacheAsync(cacheKey, cancellationToken);

            if (cachedUsers.Ok)
                users = cachedUsers.Result.ToList();
            else
            {
                var usersResult = await _unitOfWork.GetRepository<ApplicationUser>()
                .GetAllAsync(
                    predicate: x => x.Enabled,
                    include: query => query.Include(x => x.Roles),
                    trackingType: TrackingType.NoTracking
                );

                users = usersResult
                    .Where(x => x.Enabled)
                    .Select(x => new UserDto()
                    {
                        ExternalId = x.ExternalId ?? "none",
                        UserName = x.UserName,
                        Roles = x.Roles.Select(x => x.Code)
                    });

                await _bus.Send(new CacheUsersCommand(cacheKey, users));
            }

            return users.ToArray();
        }
    }
}