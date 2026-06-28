using System.Reflection;
using Common.Domain;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Persistence;

public abstract class DbInitializer : IDbInitializer
{
    private readonly DbContext _db;
    private readonly IEnumerable<IInitialData> _initialDataProviders;

    protected internal DbInitializer(DbContext db)
    {
        _db = db;
        _initialDataProviders = [];
    }

    protected internal DbInitializer(
        DbContext db,
        IEnumerable<IInitialData> initialDataProviders)
        : this(db)
        => _initialDataProviders = initialDataProviders;

    public virtual void Initialize()
    {
        _db.Database.Migrate();

        foreach (var initialDataProvider in _initialDataProviders)
        {
            if (DataSetIsEmpty(initialDataProvider.EntityType))
            {
                var data = initialDataProvider.GetData();
            
                foreach (var entity in data)
                {
                    _db.Add(entity);
                }
            }
        }

        _db.SaveChanges();
    }

    private bool DataSetIsEmpty(Type type)
    {
        var setMethod = typeof(DbInitializer)
            .GetMethod(nameof(GetSet), BindingFlags.Instance | BindingFlags.NonPublic)!
            .MakeGenericMethod(type);

        var set = setMethod.Invoke(this, []);

        var countMethod = typeof(Queryable)
            .GetMethods()
            .First(m => m.Name == nameof(Queryable.Count) && m.GetParameters().Length == 1)
            .MakeGenericMethod(type);

        var result = (int)countMethod.Invoke(null, [set])!;

        return result == 0;
    }

    private DbSet<TEntity> GetSet<TEntity>()
        where TEntity : class
        => _db.Set<TEntity>();
}