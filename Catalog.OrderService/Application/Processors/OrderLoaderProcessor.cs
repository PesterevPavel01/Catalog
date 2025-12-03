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
        public async Task<Operation<List<Order>, string>> ProcessAsync(Expression<Func<Order, bool>>? predicate = null,
            bool ascending = false, bool incompleteOnly = false, bool customOnly = false, CancellationToken cancellationToken = default)
        {
            var orders = await _unitOfWork
                .GetRepository<Order>()
                .GetAllAsync(
                    predicate: predicate,
                    include: Order.IncludeRequiredField(),
                    trackingType: TrackingType.NoTracking);


            if (!orders.Any())
            {
                return orders.ToList();
            }

            if (ascending)
                orders = [.. orders.OrderBy(x => x.CreatedAt)];
            else
                orders = [.. orders.OrderByDescending(x => x.CreatedAt)];

            if (customOnly)
            {
                orders = [.. orders.Where(x => x.OrderItems.FirstOrDefault(oi => oi.Module.IsCustom) != null)];
            }

            if (incompleteOnly)
            {
                orders = [.. orders.Where(x => !x.IsCompleted())];
            }

            return orders.ToList();
        }
    }
}
