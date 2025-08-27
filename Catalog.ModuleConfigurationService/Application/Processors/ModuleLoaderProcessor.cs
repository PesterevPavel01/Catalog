using System.Linq.Expressions;
using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Module;
using Catalog.Domain.Entities;
using Catalog.ModuleConfigurationService.Application.Configurations;
using Microsoft.Extensions.Options;

namespace Catalog.ModuleConfigurationService.Application.Processors
{
    public class ModuleLoaderProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<ApplicationConfiguration> _applicationConfiguration;

        public ModuleLoaderProcessor(IUnitOfWork unitOfWork, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _unitOfWork = unitOfWork;
            _applicationConfiguration = applicationConfiguration;
        }

        public async Task<Operation<List<ModuleDto>, string>> ProcessAsync(Expression<Func<Module, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            var modules = await _unitOfWork
                .GetRepository<Module>()
                .GetAllAsync(
                    predicate: predicate,
                    include: Module.IncludeRequaredField(),
                    trackingType: TrackingType.NoTracking);

            return modules.Select(x => x.ConvertToDto()).ToList();
        }
    }
}
