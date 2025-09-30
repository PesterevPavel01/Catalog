using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Authorization;
using Catalog.Contracts.Entities.Authorization;
using Catalog.Domain.Entities.Authorization;

namespace Catalog.ExchangeService.Application.Processors
{
    public class UserSetRoleProcessor
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserSetRoleProcessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<UserDto, string>> ProcessAsync(UserDto model, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.UserName == model.UserName,
                    trackingType: TrackingType.Tracking
                );

            if (user is null)
                return Operation.Error("User not found!");

            var currentRole = await _unitOfWork.GetRepository<Role>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == model.Roles.First(),
                    trackingType: TrackingType.Tracking
                );

            if (currentRole is null)
                return Operation.Error($"{model.Roles.First()} role not found!");

            var roleResult = user.AddRole(currentRole);

            if (!roleResult.Ok)
                return Operation.Error(roleResult.Error);

            var operationResult = user.SetExternalId(model.ExternalId);

            if (!operationResult.Ok)
                return Operation.Error(operationResult.Error);

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            return new UserDto() { ExternalId = user.ExternalId, UserName = user.UserName };
        }
    }
}
