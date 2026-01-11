using CashRegister.Application.Interfaces;
using CashRegister.Infrastructure.Data;
using CashRegister.Infrastructure.Services;
using CashRegister.Web.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

// Load environment variables from .env file (located in project root)
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);

// Configure Web app to run on port from .env
var webappPort = Environment.GetEnvironmentVariable("WEBAPP_PORT") ?? "5200";
builder.WebHost.UseUrls($"http://0.0.0.0:{webappPort}");

// Build connection string from environment variables
var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "CashRegisterDb";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "sa";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "YourStrong@Password123";
var connectionString = $"Server={dbServer};Database={dbName};User Id={dbUser};Password={dbPassword};TrustServerCertificate=True;";

// JWT settings (needed by AuthService but token not used by Web app)
builder.Configuration["Jwt:SecretKey"] = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "DefaultSecretKeyForWebAppAtLeast32CharsLong!";
builder.Configuration["Jwt:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "CashRegisterApp";
builder.Configuration["Jwt:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "CashRegisterAppUsers";
builder.Configuration["Jwt:ExpiryMinutes"] = Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? "480";

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Data Protection - persist keys to database to survive container restarts
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>()
    .SetApplicationName("CashRegisterApp");

// Application Services
builder.Services.AddScoped<ITokenService, TokenService>(); // Required by AuthService
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICashEntryService, CashEntryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBranchService, BranchService>();

// Cookie-based Authentication (8-hour sessions with sliding expiration)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".CashRegister.Auth";
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Allow HTTP in development
        options.Cookie.SameSite = SameSiteMode.Lax; // Less strict for development
    });

// Role-based Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("InputerOnly", policy => policy.RequireRole("Inputer"));
    options.AddPolicy("AuthorizerOnly", policy => policy.RequireRole("Authorizer"));
    options.AddPolicy("ViewerOnly", policy => policy.RequireRole("Viewer"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("InputerOrAuthorizer", policy =>
        policy.RequireRole("Inputer", "Authorizer"));
});

// Custom authorization handler for branch-level access control
builder.Services.AddSingleton<IAuthorizationHandler, BranchAccessHandler>();

// Configure Antiforgery with unique cookie name to avoid conflicts with old tokens
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = ".CashRegister.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection(); // Only redirect to HTTPS in production
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// Seed database with branches and default users on first run
await DbInitializer.InitializeAsync(app.Services);

app.Run();
