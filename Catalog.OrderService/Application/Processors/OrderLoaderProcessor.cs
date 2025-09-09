using System.Linq.Expressions;
using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Configurations;
using Microsoft.Extensions.Options;

namespace Catalog.OrderService.Application.Processors
{
    public sealed class OrderLoaderProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<ApplicationConfiguration> _applicationConfiguration;

        public OrderLoaderProcessor(IUnitOfWork unitOfWork, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _unitOfWork = unitOfWork;
            _applicationConfiguration = applicationConfiguration;
        }

        public async Task<Operation<List<OrderDto>, string>> ProcessAsync(Expression<Func<Order, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            var modules = await _unitOfWork
                .GetRepository<Order>()
                .GetAllAsync(
                    predicate: predicate,
                    include: Order.IncludeRequiredField(),
                    trackingType: TrackingType.NoTracking);

            return modules.Select(x => x.ConvertToDto()).ToList();
        }
    }
}
