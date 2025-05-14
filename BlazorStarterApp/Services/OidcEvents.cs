using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;

namespace BlazorStarterApp.Services;

public class OidcEvents : OpenIdConnectEvents
{
    private readonly IUserTokenStore _store;
    private readonly ILogger<OidcEvents> _logger;

    public OidcEvents(IUserTokenStore store, ILogger<OidcEvents> logger)
    {
        _store = store;
        _logger = logger;
    }

    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var exp = DateTimeOffset.UtcNow.AddSeconds(double.Parse(context.TokenEndpointResponse!.ExpiresIn));

        await _store.StoreTokenAsync(context.Principal!, new UserToken
        {
            AccessToken = context.TokenEndpointResponse.AccessToken,
            AccessTokenType = context.TokenEndpointResponse.TokenType,
            Expiration = exp,
            RefreshToken = context.TokenEndpointResponse.RefreshToken,
            Scope = context.TokenEndpointResponse.Scope
        });

        await base.TokenValidated(context);
    }

    // Handle redirection to end session endpoint
    public override Task RedirectToIdentityProviderForSignOut(RedirectContext context)
    {
        _logger.LogInformation("Redirecting to identity provider for sign-out");
        
        // Ensure we're clearing the tokens before redirecting
        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            try
            {
                // Clear any tokens stored for this user
                _store.ClearTokenAsync(context.HttpContext.User).GetAwaiter().GetResult();
                _logger.LogInformation("Tokens cleared during sign-out");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing tokens during sign-out");
            }
        }

        return base.RedirectToIdentityProviderForSignOut(context);
    }
}