using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    options.Authority = builder.Configuration["Authentication:OpenIdConnect:Authority"];
    options.ClientId = builder.Configuration["Authentication:OpenIdConnect:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:OpenIdConnect:ClientSecret"];
    options.ResponseType = "code";
    options.ResponseMode = "query";

    options.MapInboundClaims = false;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.SaveTokens = true;

    // Configure revocation endpoint
    options.RevocationEndpoint = $"{builder.Configuration["Authentication:OpenIdConnect:Authority"]}oauth/revoke";

    // request scopes + refresh tokens
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("offline_access");

    options.TokenValidationParameters.NameClaimType = "name";
    options.TokenValidationParameters.RoleClaimType = "role";

    options.EventsType = typeof(OidcEvents);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run(); 