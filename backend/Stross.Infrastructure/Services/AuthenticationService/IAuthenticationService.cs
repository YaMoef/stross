using Stross.Domain.Entities;

namespace Stross.Infrastructure.Services.AuthenticationService;

public interface IAuthenticationService
{
    public Task<User> AuthenticateWithApiKeyAsync(string? username, string? token, string? salt, CancellationToken cancellationToken = default);
}
