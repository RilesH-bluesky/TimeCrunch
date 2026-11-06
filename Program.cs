using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using TimeCrunch.Components;
using TimeCrunch.Data;
using TimeCrunch.Models;
using TimeCrunch.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// --- Database (SQLite via EF Core) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=timecrunch.db";

// A factory lets later services create short-lived contexts (safe in Blazor Server),
// while the scoped registration below is what ASP.NET Core Identity uses.
builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

// --- Authentication / Identity ---
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

builder.Services.AddTimeCrunchIdentity();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// --- App services ---
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<GoalService>();

var app = builder.Build();

await DbInitializer.InitializeAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();

// Signing out needs HttpContext (to clear the cookie), so it is a plain endpoint
// that the nav menu posts a form to. [FromForm] makes the antiforgery check apply.
app.MapPost("/account/logout", async ([FromForm] string? returnUrl, SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.LocalRedirect("/login");
});

// After the user changes their username, email or password the auth cookie must be reissued
// (interactive Blazor components cannot set cookies). The edit page redirects here.
app.MapGet("/account/refresh", async (HttpContext http, string? returnUrl,
    UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) =>
{
    var user = await userManager.GetUserAsync(http.User);
    if (user is not null)
    {
        await signInManager.RefreshSignInAsync(user);
    }

    var isLocal = !string.IsNullOrEmpty(returnUrl)
        && returnUrl.StartsWith('/')
        && !returnUrl.StartsWith("//")
        && !returnUrl.StartsWith("/\\");
    return Results.LocalRedirect(isLocal ? returnUrl! : "/profile");
}).RequireAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
