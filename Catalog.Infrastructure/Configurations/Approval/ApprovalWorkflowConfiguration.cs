using Catalog.Contracts.Entities.Approval;
using Catalog.Infrastructure.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations.Approval
{
    public class ApprovalWorkflowConfiguration : SimpleEntityConfiguration<ApprovalWorkflow>
    {
        protected override void AddBuilder(EntityTypeBuilder<ApprovalWorkflow> builder)
        {
            builder
                .HasMany(x => x.ApprovalWorkflowItems)
                .WithOne(x => x.ApprovalWorkflow)
                .HasForeignKey(x => x.ApprovalStageId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(x => x.OrderItem)
                .WithOne(x => x.ApprovalWorkflow)
                .OnDelete(DeleteBehavior.Cascade);
        }

        protected override string TableName()
        => "approval_workflows";
    }
}
