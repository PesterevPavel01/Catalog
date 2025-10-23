using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Catalog.OrderService.Application.Processors
{
    public class OrdersByCustomerLoginProcessor
    {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IOptions<ApplicationConfiguration> _applicationConfiguration;

            public OrdersByCustomerLoginProcessor(IUnitOfWork unitOfWork, IOptions<ApplicationConfiguration> applicationConfiguration)
            {
                _unitOfWork = unitOfWork;
                _applicationConfiguration = applicationConfiguration;
            }

            public async Task<Operation<IEnumerable<CommonOrderDto>, string>> ProcessAsync(string userLogin, int days,
                bool ascending = false, bool incompleteOnly = false, bool customOnly = false, CancellationToken cancellationToken = default)
            {
                var orders = await _unitOfWork
                    .GetRepository<Order>()
                    .GetAllAsync(
                        predicate: x => x.ApplicationUser.UserName == userLogin
                        && x.Enabled
                        && x.CreatedAt > DateTime.Now.AddDays(-1 * days),
                        include: Order.IncludeRequiredField(),
                        trackingType: TrackingType.NoTracking);

                if (!orders.Any())
                {
                    return new();
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
                    orders = [.. orders.Where(x => x.OrderItems.FirstOrDefault(oi => !oi.ApprovalWorkflow.IsCompleted) != null)];
                }

                var result = orders.Select(x => new CommonOrderDto()
                {
                    Code = x.Code,
                    Title = x.Title.Value,
                    UserName = x.ApplicationUser.UserName,
                    IsCompleted = x.IsCompleted(),
                    IsCustom = x.OrderItems.FirstOrDefault(item => item.Module.IsCustom == true) is not null,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Messages = x.OrderItems.SelectMany(x => x.Messages.Select(message => message.ConvertToDto()))
                }).ToArray();

                return result;
            }
    }
}
