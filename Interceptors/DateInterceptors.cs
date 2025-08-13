using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Interceptors
{
    public class DateInterceptors : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var dbContext = eventData.Context;

            if (dbContext == null) return base.SavingChanges(eventData, result);

            var entries = dbContext.ChangeTracker.Entries<IAuditable>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                    entry.Property(x => x.CreatedAt).CurrentValue = DateTime.UtcNow;

                if (entry.State == EntityState.Modified)
                    entry.Property(x => x.UpdatedAt).CurrentValue = DateTime.UtcNow;
            }

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var dbContext = eventData.Context;

            if (dbContext == null) return base.SavingChangesAsync(eventData, result);

            var entries = dbContext.ChangeTracker.Entries<IAuditable>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                    entry.Property(x => x.CreatedAt).CurrentValue = DateTime.UtcNow;

                if (entry.State == EntityState.Modified)
                    entry.Property(x => x.UpdatedAt).CurrentValue = DateTime.UtcNow;
            }
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
