using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Stross.Domain.Entities;
using Stross.Exception.Exceptions;

namespace Stross.Infrastructure.Services.AuthenticationService;

public class AuthenticationService : IAuthenticationService
{
    private readonly StrossContext _context;

    public AuthenticationService(StrossContext context)
    {
        _context = context;
    }

    public async Task<User> AuthenticateWithApiKeyAsync(string? username, string? token, string? salt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(token))
            throw new AuthenticationException();

        if (string.IsNullOrEmpty(salt) || salt.Length < Domain.Constants.Constants.SaltMinSize)
            throw new AuthenticationException();

        User? user = await _context.Users
            .Include(u => u.UserApiKeys)
            .FirstOrDefaultAsync(u => u.UserName.ToLower() == username.ToLower(), cancellationToken);

        byte[] tokenByte = GetByteArrayFromString(token);

        if (user is null)
            throw new AuthenticationException();

        if (user.UserApiKeys
            .Select(k => GetByteArrayFromString(k.GetMd5Hash(salt)))
            .Any(t => AreEqual(tokenByte, t)))
        {
            return user;
        }

        throw new AuthenticationException();
    }

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
    private static bool AreEqual(byte[] a, byte[] b)
    {
        if (a == null || b == null || a.Length != b.Length)
            return false;

        int diff = 0;

        for (int i = 0; i < a.Length; i++)
            diff |= a[i] ^ b[i];

        return diff == 0;
    }

    private static byte[] GetByteArrayFromString(string input)
    {
        return Encoding.UTF8.GetBytes(input);
    }
}