using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthVerse.IntegrationTests;

/// <summary>
/// Test authentication handler that bypasses Firebase authentication.
/// Uses X-User-Id header to set the user identity.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestAuth";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Get user ID from X-User-Id header
        if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
        {
            return Task.FromResult(AuthenticateResult.Fail("X-User-Id header not found"));
        }

        var userIdString = userIdHeader.ToString();
        if (string.IsNullOrEmpty(userIdString))
        {
            return Task.FromResult(AuthenticateResult.Fail("X-User-Id header is empty"));
        }

        // IMPORTANT: X-User-Id must be a valid Guid per HEXAGONAL_ROADMAP invariants
        // "Dev/Test bypass varsa: X-User-Id Guid olmalı, parse edilemiyorsa 401"
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Task.FromResult(AuthenticateResult.Fail($"X-User-Id header must be a valid Guid, got: '{userIdString}'"));
        }

        var claims = new[]
        {
            new Claim("user_id", userId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("firebase_uid", $"test-firebase-{userId}")
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
