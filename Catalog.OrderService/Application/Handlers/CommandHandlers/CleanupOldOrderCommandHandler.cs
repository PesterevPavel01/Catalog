using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Configurations;
using Catalog.Contracts.Entities.Approval;
using Catalog.Contracts.Events;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Domain.Entities;
using Microsoft.Extensions.Options;
using Rebus.Bus;

namespace Catalog.OrderService.Application.Handlers.CommandHandlers
{
    public class CleanupOldOrderCommandHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBus _bus;
        private readonly OrderConfiguration _orderConfiguration;

        public CleanupOldOrderCommandHandler(IUnitOfWork unitOfWork, IOptions<OrderConfiguration> options, IBus bus)
        {
            _unitOfWork = unitOfWork;
            _bus = bus;
            _orderConfiguration = options.Value;
        }

        public async Task<Operation<bool, string>> HandleAsync(CancellationToken cancellationToken = default)
        {
            var orders = (await _unitOfWork.GetRepository<Order>()
                .GetAllAsync(
                    predicate: x =>
                    x.Enabled &&
                    ((x.OrderItems.Any()
                        && !x.OrderItems.Any(item => item.ApprovalWorkflow == null) 
                        && !x.OrderItems.Any(item => !item.ApprovalWorkflow.ApprovalWorkflowItems.Any())
                        && x.OrderItems.FirstOrDefault(item => item.ApprovalWorkflow.ApprovalWorkflowItems.OrderByDescending(oi => oi.Number).First().ApprovalStage.Code != ApprovalWorkflow.CompletedStageCode) == null
                        && x.OrderItems.Select(item => item.ApprovalWorkflow)
                            .Select(aw => aw.ApprovalWorkflowItems
                            .OrderByDescending(aw => aw.Number).First())
                            .OrderByDescending(oaw => oaw.CreatedAt).First().CreatedAt < DateTime.Now.AddDays(_orderConfiguration.ArchiveStorageDays * -1))
                        || ((!x.OrderItems.Any() 
                                || x.OrderItems.Any(item => item.ApprovalWorkflow == null) 
                                || x.OrderItems.Any(item => !item.ApprovalWorkflow.ApprovalWorkflowItems.Any())) 
                            && x.CreatedAt < DateTime.Now.AddDays(_orderConfiguration.ArchiveStorageDays * -1))),
                    include: Order.IncludeRequiredField(),
                    trackingType: TrackingType.Tracking
                )).ToList();

            var disabledOrders = await _unitOfWork.GetRepository<Order>()
            .GetAllAsync(
                predicate: x => !x.Enabled && x.CreatedAt < DateTime.Now.AddDays(_orderConfiguration.ArchiveStorageDays * -1),
                include: Order.IncludeRequiredField(),
                trackingType: TrackingType.Tracking);

            if (disabledOrders.Any())
                orders.AddRange(disabledOrders);

            if (!orders.Any())
                return true;

            _unitOfWork.GetRepository<Order>().Delete(orders);

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            await _bus.Publish(new CleanupOldOrderEvent(_orderConfiguration.ArchiveStorageDays));

            return true;
        }
    }
}