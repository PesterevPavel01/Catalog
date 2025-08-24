using Catalog.Contracts.Entities.Parameters.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations.Parameters
{
    public abstract class NumericParameterConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
        where TEntity : NumericParameter
    {
        public void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.ToTable(TableName());

            builder.HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();
        }

        protected abstract void AddBuilder(EntityTypeBuilder<TEntity> builder);

        protected abstract string TableName();
    }
}