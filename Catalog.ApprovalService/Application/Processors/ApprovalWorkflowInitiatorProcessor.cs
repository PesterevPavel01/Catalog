using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.ApprovalService.Application.Configurations;
using Catalog.Contracts.Dto.Approval;
using Catalog.Contracts.Entities.Approval;
using Catalog.Contracts.Entities.Configurations;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Authorization;
using Microsoft.Extensions.Options;

namespace Catalog.ApprovalService.Application.Processors
{
    public class ApprovalWorkflowInitiatorProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnumerable<ApprovalWorkflowMapItem> _workflowMap;

        public ApprovalWorkflowInitiatorProcessor(IUnitOfWork unitOfWork, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _unitOfWork = unitOfWork;
            _workflowMap = applicationConfiguration.Value.ApprovalWorkflowMap;
        }

        public async Task<Operation<IEnumerable<ApprovalWorkflowDto>, string>> ProcessAsync(string orderCode, CancellationToken cancellationToken)
        {
            if (_workflowMap is null)
                return Operation.Error("WorkflowMap cannot be null!");

            var startStageCode = _workflowMap.FirstOrDefault(x => x.Position == 1)?.ApprovalStageCode;

            if (startStageCode is null)
                return Operation.Error("WorkflowMap. Start stage not found!");

            var orderRepository = _unitOfWork.GetRepository<OrderItem>();

            var orderItems = await orderRepository
                .GetAllAsync(
                    predicate: x => x.Order.Code == orderCode,
                    include: OrderItem.IncludeRequiredField(),
                    trackingType: TrackingType.Tracking);

            if (orderItems is null || !orderItems.Any())
                return Operation.Error("Order Items not found!");

            var customOrderItems = orderItems
                .Where(x => x.Module.IsCustom).ToArray();

            if (customOrderItems.Length == 0)
                return new List<ApprovalWorkflowDto>();

            var startStage = await _unitOfWork.GetRepository<ApprovalStage>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == startStageCode,
                    trackingType: TrackingType.Tracking
                );

            if (startStage is null)
            {
                var stageCreationResult = ApprovalStage.Create(title: startStageCode, code: startStageCode);
                
                if(!stageCreationResult)
                    return Operation.Error(stageCreationResult.Error);

                startStage = stageCreationResult.Result;
            }

            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.UserName == "TECHNICAL_USER",
                    trackingType: TrackingType.Tracking
                );

            if (user == null)
                return Operation.Error("TECHNICAL_USER not found!");

            List<ApprovalWorkflow> workflows = [];

            foreach (var item in customOrderItems) 
            {
                var workflowCreateResult = ApprovalWorkflow.Create("DEFAULT", Guid.NewGuid().ToString(), item, startStage, user);

                if (!workflowCreateResult.Ok)
                    return Operation.Error($"Failed to create order workflow: OrderCode = {orderCode}, OrderItemId = {item.Id}, ModuleCode = {item.Module.Code}");

                workflows.Add(workflowCreateResult.Result);
            }

            await _unitOfWork.GetRepository<ApprovalWorkflow>().InsertAsync(workflows, cancellationToken);

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            return workflows.Select(x => x.ConvertToDto()).ToArray();
        }
    }
}