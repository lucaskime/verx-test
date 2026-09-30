using Identity.WebApi.Domain.Entities;

namespace Identity.WebApi.Domain.Repositories;

/// <summary>
/// Contrato genérico de persistência para qualquer <see cref="Entity"/>.
/// Consultas específicas de cada agregado ficam na interface derivada (ex.: <see cref="IUserRepository"/>).
/// </summary>
public interface IRepository<TEntity> where TEntity : Entity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken);

    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken);

    Task RemoveAsync(TEntity entity, CancellationToken cancellationToken);
}
