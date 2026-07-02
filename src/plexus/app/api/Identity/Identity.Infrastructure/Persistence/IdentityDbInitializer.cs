using Common.Infrastructure.Persistence;


namespace Identity.Infrastructure.Persistence;

internal sealed class IdentityDbInitializer(IdentityDbContext db) : DbInitializer(db)
{
    private readonly IdentityDbContext _db = db;
}