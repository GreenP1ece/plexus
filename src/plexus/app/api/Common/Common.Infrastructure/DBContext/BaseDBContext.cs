using Common.Domain.Models;
using Common.Infrastructure.Events;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.DBContext;

public abstract class BaseDbContext<TContext>(DbContextOptions<TContext> options,
    IEventDispatcher eventDispatcher)
        : DbContext(options) where TContext : DbContext
{
    private readonly IEventDispatcher _eventDispatcher = eventDispatcher;
    private bool _isDispatching;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Reentrant save from within an event handler: persist without re-dispatching to avoid loops.
        if (_isDispatching)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        var entitiesWithEvents = ChangeTracker
            .Entries<IEntity>()
            .Where(e => e.Entity.Events.Count != 0)
            .Select(e => e.Entity)
            .ToArray();

        var result = await base.SaveChangesAsync(cancellationToken);

        _isDispatching = true;
        try
        {
            foreach (var entity in entitiesWithEvents)
            {
                var events = entity.Events.ToArray();
                entity.ClearEvents();

                foreach (var domainEvent in events)
                {
                    await _eventDispatcher.Dispatch(domainEvent);
                }
            }
        }
        finally
        {
            _isDispatching = false;
        }

        return result;
    }
}