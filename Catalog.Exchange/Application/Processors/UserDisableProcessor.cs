using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Domain.Entities.Authorization;

namespace Catalog.ExchangeService.Application.Processors
{
    public class UserDisableProcessor
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserDisableProcessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<bool, string>> ProcessAsync(String userName, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.UserName == userName,
                    trackingType: TrackingType.Tracking
                );

            if (user is null)
                return Operation.Error("Users not found!");

            user.Disable();

            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0;
        }
    }
}
