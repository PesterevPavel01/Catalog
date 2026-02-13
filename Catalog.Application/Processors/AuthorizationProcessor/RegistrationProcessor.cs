using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Authorization;
using Catalog.Domain.Entities.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Application.Processors.AuthorizationProcessor
{
    public sealed class RegistrationProcessor
    {
        private readonly IUnitOfWork _unitOfWork = null!;

        public RegistrationProcessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<string,string>> ProcessAsync(RegistrationUserDto model, CancellationToken token) 
        {
            var user = (await _unitOfWork.GetRepository<ApplicationUser>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.UserName == model.UserName,
                    trackingType: TrackingType.Tracking
                ));

                if (user is not null)
                    return Operation.Error("User already exists!");

            var userResult = ApplicationUser
                .Create(
                    id: Guid.NewGuid(),
                    userName: model.UserName,
                    password: model.Password
                );

            if (!userResult.Ok)
                throw new DbUpdateException(userResult.Error);

            var newUser = userResult.Result;

            await _unitOfWork.GetRepository<ApplicationUser>().InsertAsync(newUser, token);

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }
            
            return newUser.UserName;
        }
    }
}
