using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Authorization;
using Catalog.Domain.Entities.Authorization;
using Catalog.ExchangeService.Application.Commands.Cache;
using Catalog.Redis;
using Rebus.Bus;
using Microsoft.EntityFrameworkCore;

namespace Catalog.ExchangeService.Application.Handlers.Users
{
    public class RefrashCacheUsersCommandHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RedisService<UserDto> _redisService;
        private readonly IBus _bus;


        public RefrashCacheUsersCommandHandler(IUnitOfWork unitOfWork, RedisServiceFactory redisServiceFactory, IBus bus)
        {
            _unitOfWork = unitOfWork;
            _redisService = redisServiceFactory.GetService<UserDto>();
            _bus = bus;
        }

        public async Task<Operation<bool, string>> HandleAsync(CancellationToken cancellationToken)
        {
            var cacheKey = _redisService.GenerateCacheKey();

            var invalidateResult = await _redisService.InvalidateCacheAsync(cacheKey, cancellationToken);

            if (!invalidateResult.Ok)
                return Operation.Error(invalidateResult.Error);

            var usersResult = await _unitOfWork.GetRepository<ApplicationUser>()
                .GetAllAsync(
                    predicate: x => x.Enabled,
                    include: query => query.Include(x => x.Roles),
                    trackingType: TrackingType.NoTracking
                );

            var users = usersResult
                .Where(x => x.Enabled)
                .Select(x => new UserDto()
                {
                    ExternalId = x.ExternalId ?? "none",
                    UserName = x.UserName,
                    Roles = x.Roles.Select(x => x.Code)
                });

            await _bus.Send(new CacheUsersCommand(cacheKey, users));

            return true;
        }
    }
}
