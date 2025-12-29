using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.ApprovalService.Application.Configurations;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Entities.Approval;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Catalog.ApprovalService.Application.Processors
{
    public sealed class RemoveOrderWorkflowsProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<ApplicationConfiguration> _applicationConfiguration;

        public RemoveOrderWorkflowsProcessor(IOptions<ApplicationConfiguration> applicationConfiguration, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _applicationConfiguration = applicationConfiguration;
        }

        public async Task<Operation<OrderDto, string>> ProcessAsync(string orderCode, string userName, CancellationToken cancellationToken)
        {
            var orderRepository = _unitOfWork.GetRepository<Order>();

            var order = await orderRepository
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == orderCode && x.Enabled,
                    include: Order.IncludeRequiredField(),
                    trackingType: TrackingType.Tracking);

            if (order is null)
                return Operation.Error("Order not found!");

            var workflows = order.OrderItems.Where(item => item.ApprovalWorkflow is not null).Select(x => x.ApprovalWorkflow);

            if (!workflows.Any())
                return Operation.Error("Workflows not found!");

            var user = await _unitOfWork
                .GetRepository<ApplicationUser>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.UserName == userName,
                    trackingType: TrackingType.Tracking,
                    include: query => query.Include(x => x.Roles)
                );

            if (user is null)
                return Operation.Error("User not found!");

            var allowedApproverRoleCodes = _applicationConfiguration.Value.ApprovalWorkflowMap
                .Where(x => x.ApprovalStageCode == ApprovalWorkflow.CompletedStageCode).OrderByDescending(x => x.Position).FirstOrDefault();

            if (allowedApproverRoleCodes is null)
                return Operation.Error("Approval workflow map not found!");

            if (user.Roles.FirstOrDefault(x => allowedApproverRoleCodes.AllowedApproverRoleCodes.Contains(x.Code)) is null)
                return Operation.Error("Forbidden!");

            _unitOfWork.GetRepository<ApprovalWorkflow>().Delete(workflows);

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            return order.ConvertToDto();
        }
    }
}