namespace Codeflix.Catalog.Domain.SeedWork;

public interface IGenericRepository<TAggregate> : IRepository
{
    Task InsertAsync(TAggregate aggregate, CancellationToken cancellationToken);
    Task<TAggregate> GetAsync(Guid id, CancellationToken cancellationToken);
}