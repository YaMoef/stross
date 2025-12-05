using Stross.Domain.Entities;

namespace Stross.Abstractions.Accessors;

public interface IUserAccessor
{
    public string? UserName { get; }

    public Task<User?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}