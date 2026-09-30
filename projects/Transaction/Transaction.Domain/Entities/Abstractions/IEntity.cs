namespace Transaction.Domain.Entities.Abstractions;

public interface IEntity
{
    Guid Id { get; set; }
    string CreatedBy { get; set; }
    DateTimeOffset CreatedAt { get; set; }
}
