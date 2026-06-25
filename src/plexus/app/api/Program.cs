using System.Security.Claims;
using System.Text.Json;
using Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string is not configured. " +
        "Set DB_CONNECTION_STRING env var or ConnectionStrings:DefaultConnection in appsettings.");
}

var keycloakAuthority    = builder.Configuration["Keycloak:Authority"]!;
var keycloakMetadata     = builder.Configuration["Keycloak:MetadataAddress"]!;
var keycloakClientId     = builder.Configuration["Keycloak:ClientId"]!;
var keycloakClientSecret = builder.Configuration["Keycloak:ClientSecret"]!;
var keycloakAudience     = builder.Configuration["Keycloak:Audience"]!;
var keycloakIssuer       = builder.Configuration["Keycloak:Issuer"]!;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MetadataAddress = keycloakMetadata;

        options.Authority = keycloakAuthority;

        options.Audience = keycloakAudience;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer              = keycloakIssuer,
            ValidAudience            = keycloakAudience,
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
        };

        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is not ClaimsIdentity claimsIdentity) return Task.CompletedTask;

                var realmAccess = context.Principal
                    ?.FindFirst("realm_access")?.Value;

                if (realmAccess is not null)
                {
                    var parsed = JsonDocument.Parse(realmAccess);
                    if (parsed.RootElement.TryGetProperty("roles", out var roles))
                    {
                        foreach (var role in roles.EnumerateArray())
                        {
                            claimsIdentity.AddClaim(
                                new Claim(ClaimTypes.Role, role.GetString()!));
                        }
                    }
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Dev", policy => policy
        .WithOrigins("http://localhost:5044")
        .AllowAnyHeader()
        .AllowAnyMethod());
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = "Plexus API",
        Version = "v1"
    });

    options.AddSecurityDefinition(nameof(SecuritySchemeType.OAuth2), new OpenApiSecurityScheme
    {
        Type  = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{keycloakAuthority}/protocol/openid-connect/auth"),
                TokenUrl         = new Uri($"{keycloakAuthority}/protocol/openid-connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid",  "OpenID Connect" },
                    { "profile", "User profile"   },
                    { "email",   "Email"           }
                }
            }
        }
    });

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference(nameof(SecuritySchemeType.OAuth2), doc),
            ["openid", "profile"]
        }
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Plexus API v1");
        options.OAuthClientId(keycloakClientId);
        options.OAuthClientSecret(keycloakClientSecret);
        options.OAuthUsePkce();
        options.OAuthScopes("openid", "profile", "email");
    });
    app.MapOpenApi();
}
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors("Dev");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("users/me", (ClaimsPrincipal user) => Results.Ok(new
{
    UserId   = user.FindFirstValue(ClaimTypes.NameIdentifier),
    Email    = user.FindFirstValue(ClaimTypes.Email),
    Username = user.FindFirstValue("preferred_username"),
    Claims   = user.Claims.Select(c => new { c.Type, c.Value })
}))
.RequireAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();