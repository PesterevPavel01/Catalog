using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.ApprovalService.Application.Processors.OrderItems;
using Catalog.Contracts.Dto.Order;
using Catalog.Domain.Entities;

namespace Catalog.ApprovalService.Application.Services
{
    public class OrderApprovalInitiatorService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly OrderItemApprovalWorkflowCreatorProcessor _processor;

        public OrderApprovalInitiatorService(IUnitOfWork unitOfWork, OrderItemApprovalWorkflowCreatorProcessor processor)
        {
            _unitOfWork = unitOfWork;
            _processor = processor;
        }

        /// <summary>
        /// Starts new approval processes if the order does not have any approval processes
        /// </summary>
        /// <param name="orderCode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>

        public async Task<Operation<OrderDto, string>> InitializeAsync(string orderCode, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork
                .GetRepository<Order>()
                .GetFirstOrDefaultAsync(
                    trackingType: TrackingType.Tracking,
                    include: Order.IncludeRequiredField(),
                    predicate: x => x.Code == orderCode && x.Enabled);

            if (order is null)
                return Operation.Error("Order not found!");

            if (order.OrderItems is null || !order.OrderItems.Where(x => x.Enabled).Any())
                return Operation.Error("Order Items not found!");

            //Checking for the presence of ApprovalWorkflow, if it exists, then there is an error

            if (order.OrderItems.FirstOrDefault(x => x.Enabled && x.ApprovalWorkflow is not null && x.ApprovalWorkflow.Enabled) is not null)
                return Operation.Error("The approval workflow for the order has already been initiated.");

            var result = await _processor.ProcessAsync(order.ConvertToDto(), cancellationToken);

            return order.ConvertToDto();
        }
    }
}