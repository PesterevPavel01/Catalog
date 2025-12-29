using Catalog.Contracts.Entities;
using Catalog.Infrastructure.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations
{
    public class OrderEventConfiguration : SimpleEntityConfiguration<OrderEvent>
    {
        protected override void AddBuilder(EntityTypeBuilder<OrderEvent> builder)
        {
            builder
                .HasOne(x => x.Order)
                .WithMany(x => x.OrderHistory)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        protected override string TableName()
            => "order_events";
    }
}
