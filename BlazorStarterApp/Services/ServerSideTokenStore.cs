using System.Collections.Concurrent;
using System.Security.Claims;
using Duende.AccessTokenManagement.OpenIdConnect;

namespace BlazorStarterApp.Services;

public class ServerSideTokenStore : IUserTokenStore
{
    private static readonly ConcurrentDictionary<string, UserToken> Tokens = new();

    public Task<UserToken> GetTokenAsync(ClaimsPrincipal user, UserTokenRequestParameters? parameters = null)
    {
        var sub = user.FindFirst("sub")?.Value ?? throw new InvalidOperationException("no sub claim");

        if (Tokens.TryGetValue(sub, out var value))
        {
            return Task.FromResult(value);
        }

        return Task.FromResult(new UserToken { Error = "not found" });
    }

    public Task StoreTokenAsync(ClaimsPrincipal user, UserToken token, UserTokenRequestParameters? parameters = null)
    {
        var sub = user.FindFirst("sub")?.Value ?? throw new InvalidOperationException("no sub claim");
        Tokens[sub] = token;

        return Task.CompletedTask;
    }

    public Task ClearTokenAsync(ClaimsPrincipal user, UserTokenRequestParameters? parameters = null)
    {
        var sub = user.FindFirst("sub")?.Value ?? throw new InvalidOperationException("no sub claim");

        Tokens.TryRemove(sub, out _);
        return Task.CompletedTask;
    }
}