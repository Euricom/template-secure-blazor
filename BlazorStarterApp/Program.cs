using BlazorStarterApp.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using BlazorStarterApp.Services;
using Duende.AccessTokenManagement.OpenIdConnect;
using ServerSideTokenStore = BlazorStarterApp.Services.ServerSideTokenStore;

var builder = WebApplication.CreateBuilder(args);

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();


// Configure user token management options
builder.Services.Configure<UserTokenManagementOptions>(options =>
{
    options.RefreshBeforeExpiration = TimeSpan.FromMinutes(5); // Refresh tokens 5 minutes before they expire
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie("Cookies", options => 
    {
        options.Cookie.Name = "__Host-blazor"; // --Host- prefix prevents cross-site request forgery (CSRF) attacks
        options.Cookie.SameSite = SameSiteMode.Lax; // Set SameSite to Lax for cross-site requests
        options.EventsType = typeof(CookieEvents); // Use custom events for cookie authentication
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = builder.Configuration["Authentication:OpenIdConnect:Authority"];
        options.ClientId = builder.Configuration["Authentication:OpenIdConnect:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:OpenIdConnect:ClientSecret"];
        options.ResponseType = "code";
        options.ResponseMode = "query";

        options.MapInboundClaims = false; // Disable automatic claim mapping
        options.GetClaimsFromUserInfoEndpoint = true; // Get claims from the UserInfo endpoint
        
        // Don't store tokens in cookies - Our tokenstore will handle token storage
        options.SaveTokens = false;
        
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("offline_access");

        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "role";

        // Set the post logout redirect URI to ensure proper redirection after logout
        options.SignedOutCallbackPath = "/signout-callback-oidc";
        options.SignedOutRedirectUri = "/";
    
        options.EventsType = typeof(OidcEvents); // Use custom events for OpenIdConnect
    });

// adds access token management
builder.Services.AddOpenIdConnectAccessTokenManagement() // 
    .AddBlazorServerAccessTokenManagement<ServerSideTokenStore>(); // Register the custom token store

// register events to customize authentication handlers
builder.Services.AddTransient<CookieEvents>();
builder.Services.AddTransient<OidcEvents>();
builder.Services.AddAuthorization();

// Add distributed cache for Duende AccessTokenManagement
builder.Services.AddDistributedMemoryCache();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
// Add this line to provide authentication state for Blazor components
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add minimal API for login
app.MapGet("/Account/LogIn", (string? returnUrl, HttpContext httpContext) => {
    var redirectUri = "/";

    if (!string.IsNullOrWhiteSpace(returnUrl))
    {
        // Create a temporary IUrlHelper to check if URL is local
        var urlHelper = httpContext.RequestServices
            .GetRequiredService<IUrlHelperFactory>()
            .GetUrlHelper(new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor()));
            
        if (urlHelper.IsLocalUrl(returnUrl))
        {
            redirectUri = returnUrl;
        }
    }

    var props = new AuthenticationProperties
    {
        RedirectUri = redirectUri
    };

    return Results.Challenge(props);
});

// Add minimal API for logout
app.MapGet("/Account/LogOut", async (HttpContext context) => {
    // First, clear the local cookie
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    
    // Then, redirect to the identity provider's logout endpoint
    return Results.SignOut(
        new AuthenticationProperties { 
            RedirectUri = "/" 
        },
        [OpenIdConnectDefaults.AuthenticationScheme]
    );
});

app.Run();