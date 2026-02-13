using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.ComponentService.Application.Processors;
using Catalog.Contracts.Dto.Components;
using Catalog.Contracts.Dto.Exchange;
using Catalog.Domain.Entities;
using Catalog.ExchangeService.Application.Processors;
using Microsoft.EntityFrameworkCore;

namespace Catalog.ComponentService.Application.Managers
{
    public sealed class ComponentSyncManager
    {
        private readonly ComponentCreateProcessor _componentCreatorProcessor;
        private readonly ComponentUpdateProcessor _componentUpdateProcessor;
        private readonly ComponentRemovalProcessor _componentRemovalProcessor;
        private readonly DisableComponentProcessor _disableComponentProcessor;
        private readonly IUnitOfWork _unitOfWork;

        public ComponentSyncManager(ComponentUpdateProcessor componentUpdateProcessor,
            ComponentCreateProcessor componentCreatorProcessor, ComponentRemovalProcessor componentRemovalProcessor,
            IUnitOfWork unitOfWork, DisableComponentProcessor disableComponentProcessor)
        {
            _componentUpdateProcessor = componentUpdateProcessor;
            _componentCreatorProcessor = componentCreatorProcessor;
            _componentRemovalProcessor = componentRemovalProcessor;
            _disableComponentProcessor = disableComponentProcessor;
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<IEnumerable<RejectedEntity>, string>> SyncAsync(IEnumerable<ComponentDto> models, CancellationToken cancellationToken) 
        {
            List<RejectedEntity> rejectedEntities = [];

            var existingComponents = await _unitOfWork
                .GetRepository<Component>()
                .GetAllAsync(
                    predicate: x => models.Select(m => m.ComponentCode).Contains(x.Code),
                    include: query => query.Include(x=> x.ComponentType)
                );

            var newComponentModels = models.Where(x => x.ComponentTitle != "removed" && !existingComponents.Any(c => x.ComponentCode == c.Code && x.ComponentTypeCode == c.ComponentType.Code));

            foreach (var newComponentModel in newComponentModels) 
            {
                var creationResult = await _componentCreatorProcessor.ProcessAsync(newComponentModel, cancellationToken);

                if (!creationResult.Ok)
                    rejectedEntities.Add(
                        new RejectedEntity(
                            newComponentModel.ComponentCode, 
                            creationResult.Error));

            }

            var existingComponentModels = models.Where(x => x.ComponentTitle == "removed" || existingComponents.ToArray().FirstOrDefault(c => x.ComponentCode == c.Code && x.ComponentTypeCode == c.ComponentType.Code) is not null);

            foreach (var existingComponentModel in existingComponentModels.Where(x => x.ComponentTitle != "removed"))
            {
                var creationResult = await _componentUpdateProcessor.ProcessAsync(existingComponentModel, cancellationToken);

                if (!creationResult.Ok)
                    rejectedEntities.Add(
                        new RejectedEntity(
                            existingComponentModel.ComponentCode,
                            creationResult.Error));

            }

            foreach (var removedComponentModel in existingComponentModels.Where(x => x.ComponentTitle == "removed"))
            {
                var removalResult = await _componentRemovalProcessor.ProcessAsync(removedComponentModel.ComponentCode, cancellationToken);

                if (!removalResult.Ok) { 

                    rejectedEntities.Add(
                        new RejectedEntity(
                            removedComponentModel.ComponentCode,
                            removalResult.Error));

                    var disableResult = await _disableComponentProcessor.ProcessAsync(removedComponentModel.ComponentCode, cancellationToken);

                    if(!disableResult.Ok)
                        rejectedEntities.Add(
                            new RejectedEntity(
                                removedComponentModel.ComponentCode,
                                disableResult.Error));
                    else
                        rejectedEntities.Add(
                            new RejectedEntity(
                                removedComponentModel.ComponentCode,
                                "Component has been deactivated!"));
                }
            }

            return rejectedEntities;
        }
    }
}
