using Catalog.Contracts.Entities.Parameters.Base;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations.Parameters
{
    public abstract class TextParameterConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
     where TEntity : TextParameter
    {
        public void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.ToTable(TableName());

            builder.HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder
                .Property(x => x.Value)
                .IsRequired()
                .HasConversion(x => x.Value, x => TextParameterValue.Create(x).Result);
        }
        protected abstract void AddBuilder(EntityTypeBuilder<TEntity> builder);

        protected abstract string TableName();
    }
}