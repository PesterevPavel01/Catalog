using System.Linq.Expressions;
using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Module;
using Catalog.Domain.Entities;

namespace Catalog.ModuleConfigurationService.Application.Processors
{
    public class ModuleLoaderProcessor
    {
        private readonly IUnitOfWork _unitOfWork;

        public ModuleLoaderProcessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<List<ModuleDto>, string>> ProcessAsync(Expression<Func<Module, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            var modules = await _unitOfWork
                .GetRepository<Module>()
                .GetAllAsync(
                    predicate: predicate,
                    include: Module.IncludeRequiredField(),
                    trackingType: TrackingType.NoTracking);

            if (!modules.Any())
                return new List<ModuleDto>();

            return modules.Select(x => x.ConvertToDto()).ToList();
        }
    }
}
