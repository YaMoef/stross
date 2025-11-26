using Microsoft.AspNetCore.Authentication;

namespace Stross.API.AuthenticationHandlers;

public class SubsonicAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "SubsonicScheme";
    public string Scheme => DefaultScheme;
}