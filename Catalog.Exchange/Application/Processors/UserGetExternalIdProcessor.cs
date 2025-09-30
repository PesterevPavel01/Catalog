using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Authorization;
using Catalog.Domain.Entities.Authorization;

namespace Catalog.ExchangeService.Application.Processors
{
    public class UserGetExternalIdProcessor
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserGetExternalIdProcessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<UserDto, string>> ProcessAsync(string userName, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.UserName == userName,
                    trackingType: TrackingType.NoTracking
                );

            if (user is null)
                return Operation.Error("User not found!");

            if (user.ExternalId is null)
                return Operation.Error("ExternalId not set!");

            return new UserDto() { ExternalId = user.ExternalId, UserName = user.UserName };
        }
    }
}
