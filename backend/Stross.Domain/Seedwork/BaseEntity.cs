namespace Stross.Domain.Seedwork;

public class BaseEntity : IBaseEntity
{
    public long Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public long CreatedBy { get; private set; }
    public long UpdatedBy { get; private set; }
}