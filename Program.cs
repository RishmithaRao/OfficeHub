using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using OfficeHub.Auth;
using OfficeHub.Components;
using OfficeHub.Data;
using OfficeHub.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

var appData = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(appData);
var sqlitePath = Path.Combine(appData, "officehub.db");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={sqlitePath}"));

builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<ReportExportService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.Cookie.Name = "officehub.auth";
        options.Cookie.HttpOnly = true;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.EnsureSeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapPost("/auth/login", async (HttpContext http) =>
{
    var form = await http.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var returnUrl = form["returnUrl"].ToString();

    var trimmedEmail = email.Trim();
    if (!TryValidateDemoUser(trimmedEmail, password, out var roles))
        return Results.Redirect($"/login?error=1&returnUrl={Uri.EscapeDataString(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl)}");

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, trimmedEmail),
        new(ClaimTypes.Email, trimmedEmail)
    };
    foreach (var role in roles)
        claims.Add(new Claim(ClaimTypes.Role, role));

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await http.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(identity));

    var safeReturn = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
    if (!safeReturn.StartsWith('/'))
        safeReturn = "/";
    return Results.Redirect(safeReturn);
}).DisableAntiforgery();

app.MapPost("/auth/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
}).DisableAntiforgery();

app.MapGet("/api/export/tasks.csv", async (
    HttpContext http,
    ReportExportService reports,
    CancellationToken ct) =>
{
    if (http.User.Identity?.IsAuthenticated != true)
        return Results.Unauthorized();
    var bytes = await reports.BuildTasksCsvAsync(ct);
    return Results.File(bytes, "text/csv; charset=utf-8", "work-items.csv");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static bool TryValidateDemoUser(string email, string password, out string[] roles)
{
    roles = [];
    var key = email.ToLowerInvariant();
    var map = new Dictionary<string, (string Password, string[] Roles)>(StringComparer.OrdinalIgnoreCase)
    {
        ["admin@officehub.local"] = ("Admin123!", ["Admin", "Manager", "User"]),
        ["manager@officehub.local"] = ("Manager123!", ["Manager", "User"]),
        ["user@officehub.local"] = ("User123!", ["User"])
    };

    if (!map.TryGetValue(key, out var entry) || entry.Password != password)
        return false;

    roles = entry.Roles;
    return true;
}
