using Catalog.Contracts.Entities.Approval;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations.Approval
{
    public sealed class ApprovalWorkflowItemConfiguration : IEntityTypeConfiguration<ApprovalWorkflowItem>
    {
        public void Configure(EntityTypeBuilder<ApprovalWorkflowItem> builder)
        {
            builder.ToTable("approval_workflow_items");

            builder.HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder
                .HasOne(x => x.Initiator)
                .WithMany(x => x.ApprovalWorkflowItems)
                .HasForeignKey(x => x.InitiatorId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(x => x.ApprovalStage)
                .WithMany(x => x.ApprovalWorkflowItems)
                .HasForeignKey(x => x.ApprovalStageId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(x => x.ApprovalWorkflow)
                .WithMany(x => x.ApprovalWorkflowItems)
                .HasForeignKey(x => x.ApprovalWorkflowId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
