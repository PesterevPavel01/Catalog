using Calabonga.OperationResults;
using Catalog.Contracts.Dto.Approval;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Authorization;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Catalog.Contracts.Entities.Approval
{
    public sealed class ApprovalWorkflow : SimpleEntity
    {
        public static string CompletedStageCode = "COMPLETED";

        private readonly List<ApprovalWorkflowItem> _approvalWorkflowItems = [];

        protected ApprovalWorkflow(TitleValue title, string code, Guid id) : base(title, code, id)
        {
        }

        public static Operation<ApprovalWorkflow,string> Create(string title, string code, OrderItem orderItem, ApprovalStage startStage, ApplicationUser user) 
        {
            if (string.IsNullOrWhiteSpace(title))
                return Operation.Error("Title is empty or null!");

            if (string.IsNullOrWhiteSpace(code))
                return Operation.Error("Code is empty or null!");

            if (orderItem is null)
                return Operation.Error("OrderItem cannot be null!");

            if (startStage is null)
                return Operation.Error("ApprovalStage cannot be null!");

            var titleValue = TitleValue.Create(title);
            
            if (!titleValue.Ok)
                return Operation.Error(titleValue.Error);

            var workflow = new ApprovalWorkflow(titleValue.Result, code, Guid.NewGuid());

            var operationResult = ApprovalWorkflowItem.Create(user, startStage, 1);

            if (!operationResult.Ok)
                return Operation.Error(operationResult.Error);

            workflow._approvalWorkflowItems.Add(operationResult.Result);
            
            workflow.OrderItem = orderItem;

            return workflow;
        }

        public bool IsCompleted => CheckIsCompleted();

        public IReadOnlyCollection<ApprovalWorkflowItem> ApprovalWorkflowItems => 
            _approvalWorkflowItems
                .OrderBy(x => x.CreatedAt)
                .ToArray()
                .AsReadOnly();

        public ApprovalWorkflowItem ActiveStage => ApprovalWorkflowItems.Last();

        public OrderItem OrderItem { get; private set; } = null!;
        public Guid OrderItemId { get; private set; }

        public Operation<ApprovalWorkflowItem, string> Approve(ApplicationUser user, ApprovalStage stage, short number) 
        {
            if (ActiveStage.ApprovalStage.Code == stage.Code)
                return Operation.Error("Stage already activated!");

            var operationResult = ApprovalWorkflowItem.Create(user, stage, number);

            if (!operationResult.Ok)
                return Operation.Error(operationResult.Error);

            _approvalWorkflowItems.Add(operationResult.Result);

            return operationResult.Result;
        }

        public ApprovalWorkflowDto ConvertToDto()
            => new()
            {
                WorkflowCode = this.Code,
                ActiveStage = ActiveStage.ConvertToDto(),
                OrderItem = OrderItem.ConvertToDto(),
                IsCompleted = this.IsCompleted,
                ApprovalWorkflowItems = ApprovalWorkflowItems.Select(x => x.ConvertToDto())
            };

        public static Func<IQueryable<ApprovalWorkflow>, IIncludableQueryable<ApprovalWorkflow, object>> IncludeRequiredField()
            =>
            query => query
                .Include(x => x.OrderItem)
                    .ThenInclude(oi => oi.Module)
                        .ThenInclude(m => m.Components)
                            .ThenInclude(c => c.ComponentType)
                .Include(x => x.OrderItem)
                    .ThenInclude(oi => oi.Module)
                        .ThenInclude(m => m.Components)
                            .ThenInclude(c => c.ComponentNumericParameters)
                                .ThenInclude(tp => tp.ParameterType)
                .Include(x => x.OrderItem)
                    .ThenInclude(oi => oi.Module)
                        .ThenInclude(m => m.Components)
                            .ThenInclude(c => c.ComponentTextParameters)
                                .ThenInclude(tp => tp.ParameterType)
                .Include(x => x.OrderItem)
                    .ThenInclude(oi => oi.Module)
                        .ThenInclude(m => m.ModuleType)
                .Include(x => x.ApprovalWorkflowItems)
                    .ThenInclude(ai => ai.ApprovalStage)
                .Include(x => x.ApprovalWorkflowItems)
                    .ThenInclude(ai => ai.User);

        public Operation<ApprovalWorkflowItem, string> Complete(ApplicationUser user, ApprovalStage completedStage)
        {
            if (completedStage.Code != CompletedStageCode)
                return Operation.Error("Input Stage is not a Completed Stage");

            var number = (short)(ActiveStage.Number + 1);

            return Approve(user, completedStage, number);
        }

        private bool CheckIsCompleted()
         => ActiveStage.ApprovalStage.Code == CompletedStageCode;
    }
}
