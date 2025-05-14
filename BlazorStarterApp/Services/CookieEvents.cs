using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BlazorStarterApp.Services;

public class CookieEvents : CookieAuthenticationEvents
{
    private readonly IUserTokenStore _store;
    private readonly IUserTokenManagementService _tokenManagementService;

    public CookieEvents(
        IUserTokenStore store,
        IUserTokenManagementService tokenManagementService)
    {
        _store = store;
        _tokenManagementService = tokenManagementService;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var token = await _store.GetTokenAsync(context.Principal!);
        if (token.IsError)
        {
            context.RejectPrincipal();
            return;
        }

        await base.ValidatePrincipal(context);
    }
}