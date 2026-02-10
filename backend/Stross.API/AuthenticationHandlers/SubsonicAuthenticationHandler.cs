using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Stross.API.Helpers;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Application.Slices.Subsonic.Services;
using Stross.Exception.Exceptions;
using Stross.SubsonicModels;
using IAuthenticationService = Stross.Infrastructure.Services.AuthenticationService.IAuthenticationService;

namespace Stross.API.AuthenticationHandlers;

public class SubsonicAuthenticationHandler : AuthenticationHandler<SubsonicAuthenticationOptions>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ISubsonicResponseFormatService _subsonicResponseFormatService;

    private bool _isLegacyAuthentication = false;
    private string _failureMessage = "";

    public SubsonicAuthenticationHandler(
        IOptionsMonitor<SubsonicAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IAuthenticationService authenticationService, ISubsonicResponseFormatService subsonicResponseFormatService) : base(options, logger, encoder)
    {
        _authenticationService = authenticationService;
        _subsonicResponseFormatService = subsonicResponseFormatService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? legacyPassword = Request.Query["p"].FirstOrDefault();

        if (!string.IsNullOrEmpty(legacyPassword))
        {
            _isLegacyAuthentication = true;

            AuthenticateResult.Fail("Legacy authentication is not supported by server.");
        }

        string? userName = Request.Query["u"].FirstOrDefault();
        string? token = Request.Query["t"].FirstOrDefault();
        string? salt = Request.Query["s"].FirstOrDefault();

        try
        {
            Stross.Domain.Entities.User user = await _authenticationService.AuthenticateWithApiKeyAsync(userName, token, salt);

            Claim[] claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName), new Claim(ClaimTypes.GivenName, user.DisplayName)
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, SubsonicAuthenticationOptions.Scheme);
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            return AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, SubsonicAuthenticationOptions.Scheme));
        }
        catch (AuthenticationException ex)
        {
            _failureMessage = ex.Message;

            return AuthenticateResult.Fail(ex.Message);
        }
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        SubsonicBaseResponse response;

        if (_isLegacyAuthentication)
            response = new SubsonicBaseResponse(new Response
            {
                Error = new Error
                {
                    Code = 0,
                    Message = "Legacy authentication is not supported by server."
                }
            })
            {
                Format = _subsonicResponseFormatService.ResponseFormat
            };
        else
            response = new SubsonicBaseResponse(new Response
            {
                Error = new Error
                {
                    Code = 40,
                    Message = _failureMessage
                }
            })
            {
                Format = _subsonicResponseFormatService.ResponseFormat
            };

        return SubsonicResponseHelper.CreateSubsonicResult(response).ExecuteAsync(Context);
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        SubsonicBaseResponse response = new SubsonicBaseResponse(new Response
        {
            Error = new Error
            {
                Code = 50,
                Message = "User is not authorized for the given operation."
            }
        })
        {
            Format = _subsonicResponseFormatService.ResponseFormat
        };

        return SubsonicResponseHelper.CreateSubsonicResult(response).ExecuteAsync(Context);
    }
}
