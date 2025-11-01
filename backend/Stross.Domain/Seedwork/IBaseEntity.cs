namespace Stross.Domain.Seedwork;

public interface IBaseEntity
{
    public long Id { get; }

    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; }
    public long CreatedBy { get; }
    public long UpdatedBy { get; }
}