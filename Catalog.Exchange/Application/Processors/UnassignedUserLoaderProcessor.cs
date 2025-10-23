using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Authorization;
using Catalog.Domain.Entities.Authorization;

namespace Catalog.ExchangeService.Application.Processors
{
    public class UnassignedUserLoaderProcessor
    {

        private readonly IUnitOfWork _unitOfWork;

        public UnassignedUserLoaderProcessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<IEnumerable<UserDto>, string>> ProcessAsync(CancellationToken cancellationToken)
        {
            var users = await _unitOfWork.GetRepository<ApplicationUser>()
                .GetAllAsync(
                    predicate: x => x.Roles.Count == 0 && x.Enabled,
                    trackingType: TrackingType.NoTracking
                );

            if (users is null || !users.Any())
                return Operation.Error("Users not found!");

            return users.Select(x => new UserDto() { ExternalId = x.ExternalId ?? "none", UserName = x.UserName, Roles = x.Roles.Select(x => x.Code) }).ToArray();
        }
    }
}