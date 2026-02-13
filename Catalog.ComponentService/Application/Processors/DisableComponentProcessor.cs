using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.ComponentService.Application.Processors
{
    public class DisableComponentProcessor
    {
        private readonly IUnitOfWork _unitOfWork;

        public DisableComponentProcessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<bool, string>> ProcessAsync(string componentCode, CancellationToken cancellationToken)
        {
            var componentRepository = _unitOfWork.GetRepository<Component>();

            var component = await componentRepository
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == componentCode,
                    trackingType: TrackingType.Tracking,
                    include: query => query.Include(x => x.Modules));

            if (component is null)
                return Operation.Error("Component not found!");

            component.Disable();

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            return true;
        }
    }
}
