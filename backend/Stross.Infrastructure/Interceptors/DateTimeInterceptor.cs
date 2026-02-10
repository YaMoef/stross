using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Stross.Domain.Seedwork;

namespace Stross.Infrastructure.Interceptors;

public class DateTimeInterceptor : ISaveChangesInterceptor
{
    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        throw new NotSupportedException();
    }

    public ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        List<EntityEntry<IBaseEntity>> entitiesUpdated = eventData.Context?.ChangeTracker.Entries<IBaseEntity>()
            .Where(e => e.State == EntityState.Modified).ToList() ?? [];
        List<EntityEntry<IBaseEntity>> entitiesAdded = eventData.Context?.ChangeTracker.Entries<IBaseEntity>()
            .Where(e => e.State == EntityState.Added).ToList() ?? [];

        foreach (EntityEntry<IBaseEntity> addedEntity in entitiesAdded)
        {
            addedEntity.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
        }

        foreach (EntityEntry<IBaseEntity> updatedEntity in entitiesUpdated)
        {
            updatedEntity.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
        }

        return ValueTask.FromResult(result);
    }
}
