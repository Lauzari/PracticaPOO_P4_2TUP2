using Core.Interfaces;
using Core.Services;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Web.Middleware;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IBankAccountRepository, BankAccountRepository>();
builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();
builder.Services.AddScoped<BankAccountService>();
builder.Services.AddScoped<UserServices>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

/* Configuración de conexión con SQLite */
var connection = new SqliteConnection("Data Source=WebApiBankAccount.db");
connection.Open();

using (var command = connection.CreateCommand())
{
    command.CommandText = "PRAGMA journal_mode = DELETE;";
    command.ExecuteNonQuery();
}

builder.Services.AddDbContext<ApplicationDbContext>(DbContextOptions => DbContextOptions.UseSqlite(connection));


#region Swagger config
//builder.Services.AddSwaggerGen();
string instance = builder.Configuration["AzureAd:Instance"]!;
string tenantId = builder.Configuration["AzureAd:TenantId"]!;
string clientid = builder.Configuration["AzureAd:ClientId"]!;
string ApplicationIdURI = builder.Configuration["AzureAd:ApplicationIdURI"]!;
string scope = "default";
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Configure OAuth2
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{instance}{tenantId}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri($"{instance}{tenantId}/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    { $"{ApplicationIdURI}/{scope}", "Access API" }
                }
            }
        }
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { $"{ApplicationIdURI}/{scope}" }
        }
    });
});
#endregion

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

var app = builder.Build();


#region Swagger pipeline config
string spaClientId = builder.Configuration["AzureAd:SpaClientId"]!;
app.UseSwagger();
var oAuthRedirectUrl = builder.Configuration["AzureAd:RedirectUri"];
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.OAuthClientId($"{spaClientId}");
    c.OAuthUsePkce();  // Recommended for B2C
    c.OAuth2RedirectUrl(oAuthRedirectUrl); // Same as the one registered in Azure B2C
    c.OAuthScopes($"{ApplicationIdURI}/{scope}"); //Selects this scope by default.
});
#endregion

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
