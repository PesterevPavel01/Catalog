using Catalog.Contracts.Entities.Exchange;
using Catalog.Infrastructure.Configurations.Base;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations.Exchange
{
    public sealed class ExchangeEventConfiguration : SimpleEntityConfiguration<ExchangeEvent>
    {
        protected override void AddBuilder(EntityTypeBuilder<ExchangeEvent> builder)
        {
            builder
                .Property(x => x.Message)
                .HasMaxLength(500);

            builder
                .Property(x => x.Type)
                .HasMaxLength(255);
        }

        protected override string TableName() => "exchange_events";
    }
}