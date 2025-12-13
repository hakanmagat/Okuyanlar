using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Interfaces;
using Okuyanlar.Core.Settings;
using Okuyanlar.Data; // Required for DbInitializer
using Okuyanlar.Data.Repositories;
using Okuyanlar.Service.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 1. DATABASE CONFIGURATION (Cross-Platform) ---
string dbFileName = "Okuyanlar.db";
string projectRootPath = builder.Environment.ContentRootPath;
string dbPath = Path.Combine(projectRootPath, dbFileName);
string connectionString = $"Data Source={dbPath}";

builder.Services.AddDbContext<OkuyanlarDbContext>(options =>
    options.UseSqlite(connectionString));

// --- 2. SETTINGS CONFIGURATION ---
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// --- 3. DEPENDENCY INJECTION ---
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<BookService>();

// --- 4. AUTHENTICATION CONFIGURATION ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- 5. MIDDLEWARE PIPELINE ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // Who are you?
app.UseAuthorization();  // Are you allowed here?

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// --- 6. SEED DATA EXECUTION (This was missing) ---
// This block runs every time the application starts.
// It creates a temporary scope to resolve services and run the DbInitializer.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<OkuyanlarDbContext>();
        var hasher = services.GetRequiredService<IPasswordHasher<User>>();

        // Check database and create SystemAdmin if missing
        DbInitializer.Initialize(context, hasher);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
app.Run();