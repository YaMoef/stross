using Microsoft.EntityFrameworkCore;
using Stross.Abstractions.Accessors;
using Stross.Domain.Entities;
using Stross.Infrastructure;

namespace Stross.API.Accessors;

public class UserAccessor : IUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly StrossContext _context;

    public UserAccessor(IHttpContextAccessor httpContextAccessor, StrossContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public string? UserName => _httpContextAccessor.HttpContext?.User.Identity?.Name;

    public Task<User?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.UserName == UserName, cancellationToken);
    }
}