using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Configurations;
using Catalog.Contracts.Entities.Approval;
using Catalog.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;

namespace Catalog.OrderService.Application.Messages.OrderMessages
{
    public sealed class CleanupOldOrder
    {
        public record Request(int ArchiveStorageDays) : IRequest<Operation<bool, string>>;

        public class Handler(IUnitOfWork unitOfWork)
            : IRequestHandler<Request, Operation<bool, string>>
        {

            public async Task<Operation<bool, string>> Handle(Request request, CancellationToken cancellationToken)
            {
                var orders = (await unitOfWork.GetRepository<Order>()
                    .GetAllAsync(
                        predicate: x =>
                        x.Enabled &&
                        (x.OrderItems.Any()
                            && !x.OrderItems.Any(item => item.ApprovalWorkflow == null)
                            && !x.OrderItems.Any(item => !item.ApprovalWorkflow.ApprovalWorkflowItems.Any())
                            && x.OrderItems.FirstOrDefault(item => item.ApprovalWorkflow.ApprovalWorkflowItems.OrderByDescending(oi => oi.Number).First().ApprovalStage.Code != ApprovalWorkflow.CompletedStageCode) == null
                            && x.OrderItems.Select(item => item.ApprovalWorkflow)
                                .Select(aw => aw.ApprovalWorkflowItems
                                .OrderByDescending(aw => aw.Number).First())
                                .OrderByDescending(oaw => oaw.CreatedAt).First().CreatedAt < DateTime.Now.AddDays(request.ArchiveStorageDays * -1)
                            || (!x.OrderItems.Any()
                                    || x.OrderItems.Any(item => item.ApprovalWorkflow == null)
                                    || x.OrderItems.Any(item => !item.ApprovalWorkflow.ApprovalWorkflowItems.Any()))
                                && x.CreatedAt < DateTime.Now.AddDays(request.ArchiveStorageDays * -1)),
                        include: Order.IncludeRequiredField(),
                        trackingType: TrackingType.Tracking
                    )).ToList();

                var disabledOrders = await unitOfWork.GetRepository<Order>()
                .GetAllAsync(
                    predicate: x => !x.Enabled && x.CreatedAt < DateTime.Now.AddDays(request.ArchiveStorageDays * -1),
                    include: Order.IncludeRequiredField(),
                    trackingType: TrackingType.Tracking);

                if (disabledOrders.Any())
                    orders.AddRange(disabledOrders);

                if (!orders.Any())
                    return true;

                unitOfWork.GetRepository<Order>().Delete(orders);

                var result = await unitOfWork.SaveChangesAsync();

                if (unitOfWork.Result.Exception is not null)
                {
                    return Operation.Error(unitOfWork.Result.Exception.Message);
                }

                return true;
            }
        }
    }
}