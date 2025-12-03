using System.Linq.Expressions;
using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Components;
using Catalog.Contracts.Dto.Order;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Catalog.ExchangeService.Application.Processors
{
    public class ComponentLoaderProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<ComponentConfiguration> _applicationConfiguration;
        public ComponentLoaderProcessor(IUnitOfWork unitOfWork, IOptions<ComponentConfiguration> options)
        {
            _unitOfWork = unitOfWork;
            _applicationConfiguration = options;
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
            var components = await _unitOfWork
                .GetRepository<Component>()
                .GetAllAsync(
                    predicate: predicate,
                    include: Component.IncludeRequiredField(),
                    trackingType: TrackingType.NoTracking);

            if (!components.Any())
            {
                return new List<ComponentDto>();
            }

            if (ascending)
                components = [.. components.OrderBy(x => x.CreatedAt)];
            else
                components = [.. components.OrderByDescending(x => x.CreatedAt)];

            return components.Select(x => x.ConvertToDto()).ToList();
        }
    }
}
