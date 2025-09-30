using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Authorization;
using Catalog.Domain.Entities.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Catalog.ExchangeService.Application.Processors
{
    public class UserRolesLoaderProcessor
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserRolesLoaderProcessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<UserDto, string>> ProcessAsync(string userName, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.UserName == userName,
                    trackingType: TrackingType.NoTracking,
                    include: query => query.Include(x => x.Roles)
                );

            if (user is null)
                return Operation.Error("User not found!");

            return new UserDto() { ExternalId = user.ExternalId ?? "none", UserName = user.UserName, Roles = user.Roles.Select(x => x.Code)};
        }
    }
}