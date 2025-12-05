using System.Linq.Expressions;
using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.ComponentService.Application.Processors;
using Catalog.Contracts.Dto.Components;
using Catalog.Domain.Entities;

namespace Catalog.ExchangeService.Application.Processors
{
    public class ComponentLoaderProcessor
    {
        private readonly IUnitOfWork _unitOfWork;

        public ComponentLoaderProcessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Метод для получения списка компонентов по заданным параметрам
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="ascending"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>

        public async Task<Operation<List<ComponentDto>, string>> ProcessAsync(Expression<Func<Component, bool>>? predicate = null, bool ascending = false, CancellationToken cancellationToken = default)
        {
                var components = (await _unitOfWork
                    .GetRepository<Component>()
                    .GetAllAsync(
                        predicate: predicate,
                        include: Component.IncludeRequiredField(),
                        trackingType: TrackingType.NoTracking));

                if (!components.Any())
                {
                    return new List<ComponentDto>();
                }

                if (ascending)
                    components = [.. components.OrderBy(x => x.CreatedAt)];
                else
                    components = [.. components.OrderByDescending(x => x.CreatedAt)];

                var componentDto = components.Select(x => x.ConvertToDto()).ToList();

                return componentDto;
        }
    }
}
