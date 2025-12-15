using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

using Mrtc.Test.Api.Models;

namespace Mrtc.Test.Api.Handlers;

public class BasicAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    // You'll need a user service to validate credentials (e.g., IUserService)
    // private readonly IUserService _userService;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.Fail("Missing Authorization Header");
        }

        if (!AuthenticationHeaderValue.TryParse(Request.Headers["Authorization"], out AuthenticationHeaderValue headerValue))
        {
            return AuthenticateResult.Fail("Invalid Authorization Header format");
        }

        if (!BasicAuthenticationDefaults.AuthenticationScheme.Equals(headerValue.Scheme, StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Unknown authentication scheme");
        }

        if (string.IsNullOrWhiteSpace(headerValue.Parameter))
        {
            return AuthenticateResult.Fail("Missing credentials");
        }

        var credentialBytes = Convert.FromBase64String(headerValue.Parameter);
        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
        var username = credentials.FirstOrDefault();
        var password = credentials.LastOrDefault();


        bool isValidUser = (username == "test_user" && password == "test_password");

        if (!isValidUser)
        {
            return AuthenticateResult.Fail("Invalid Username or Password");
        }

        var claims = new[] {
        new Claim(ClaimTypes.NameIdentifier, username!),
        new Claim(ClaimTypes.Name, username!)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
    
    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{Request.Host}\"";
        await base.HandleChallengeAsync(properties);
    }
}
