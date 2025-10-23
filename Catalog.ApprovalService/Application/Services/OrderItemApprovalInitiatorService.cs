using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.ApprovalService.Application.Processors.OrderItems;
using Catalog.Contracts.Dto.Order;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.ApprovalService.Application.Services
{
    public class OrderItemApprovalInitiatorService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly OrderItemApprovalWorkflowCreatorProcessor _processor;

        public OrderItemApprovalInitiatorService(IUnitOfWork unitOfWork, OrderItemApprovalWorkflowCreatorProcessor processor)
        {
            _unitOfWork = unitOfWork;
            _processor = processor;
        }

        //У OrderItem есть только два события, которые влияют на ApprovalWorkflow это добавление в заказ и удаление!
        /// <summary>
        /// It will initiate a new approval process only if there are already other approval processes running for the order
        /// </summary>
        /// <param name="models"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>

        public async Task<Operation<bool, string>> InitializeAsync(IEnumerable<CreateOrderItemDto> models, CancellationToken cancellationToken)
        {
            var orderCode = models.First(x => x.OrderCode != null).OrderCode;

            var order = await _unitOfWork
                .GetRepository<Order>()
                .GetFirstOrDefaultAsync(
                    trackingType: TrackingType.Tracking,
                    include: query => query
                        .Include(x => x.OrderItems)
                            .ThenInclude(x => x.ApprovalWorkflow)
                                .ThenInclude(x => x.ApprovalWorkflowItems),
                    predicate: x => x.Code == orderCode && x.Enabled);

            if (order is null)
                return Operation.Error("Order not found!");

            if (order.OrderItems is null || !order.OrderItems.Where(x => x.Enabled).Any())
                return Operation.Error("Order Items not found!");

            //Checking for the presence of ApprovalWorkflow; if it not exists, then return.

            if (order.OrderItems.FirstOrDefault(x => x.Enabled && x.ApprovalWorkflow is not null && x.ApprovalWorkflow.Enabled) is null)
                return true;

            var result = await _processor.ProcessAsync(models, cancellationToken);

            return true;
        }
    }
}
