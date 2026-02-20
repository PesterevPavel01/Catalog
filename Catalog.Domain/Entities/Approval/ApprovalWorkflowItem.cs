using Calabonga.OperationResults;
using Catalog.Contracts.Dto.Approval;
using Catalog.Domain.Entities.Authorization;
using Catalog.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Contracts.Entities.Approval
{
    public sealed class ApprovalWorkflowItem : Entity
    {
        private ApprovalWorkflowItem(Guid id) : base(id)
        {
        }
        public short Number {  get; private set; }

        public ApprovalStage ApprovalStage { get; private set; }
        public Guid ApprovalStageId { get; private set; }

        public ApprovalWorkflow ApprovalWorkflow { get; private set; }
        public Guid ApprovalWorkflowId { get; private set; }

        public ApplicationUser Initiator { get; private set; }
        public Guid InitiatorId { get; private set; }

        public static Operation<ApprovalWorkflowItem, string> Create(ApplicationUser user, ApprovalStage stage, short number) 
        {
            var approvalWorkflowItem = new ApprovalWorkflowItem(Guid.NewGuid());

            approvalWorkflowItem.Initiator = user;

            approvalWorkflowItem.ApprovalStage = stage;

            approvalWorkflowItem.Number = number;

            return approvalWorkflowItem;
        }

        public ApprovalWorkflowItemDto ConvertToDto()
            => new()
            {
                ApprovalStage = this.ApprovalStage.ConvertToDto(),
                Number = this.Number,
                InitiatorName = Initiator.UserName
            };

        public static Func<IQueryable<ApprovalWorkflowItem>, IIncludableQueryable<ApprovalWorkflowItem, object>> IncludeRequiredField()
            =>
            query => query
                .Include(x => x.ApprovalStage);
    }
}
