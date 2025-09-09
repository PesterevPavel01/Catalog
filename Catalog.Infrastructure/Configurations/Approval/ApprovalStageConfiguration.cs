using Catalog.Contracts.Entities.Approval;
using Catalog.Infrastructure.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations.Approval
{
    public class ApprovalStageConfiguration : SimpleEntityConfiguration<ApprovalStage>
    {
        protected override void AddBuilder(EntityTypeBuilder<ApprovalStage> builder)
        {
            builder
                .HasMany(x => x.ApprovalWorkflowItems)
                .WithOne(x => x.ApprovalStage)
                .HasForeignKey(x => x.ApprovalStageId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);
        }

        protected override string TableName()
            => "approval_stages";
    }
}
