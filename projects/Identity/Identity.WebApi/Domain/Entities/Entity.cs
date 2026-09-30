namespace Identity.WebApi.Domain.Entities;

/// <summary>Base de toda entidade persistida: identidade única por <see cref="Id"/>.</summary>
public abstract class Entity
{
    public Guid Id { get; protected set; }
}
