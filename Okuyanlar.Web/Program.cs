using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Interfaces;
using Okuyanlar.Core.Settings;
using Okuyanlar.Data;
using Okuyanlar.Data.Repositories;
using Okuyanlar.Service.Services;
using Okuyanlar.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// --- DATABASE (SQLite) ---
string dbFileName = "Okuyanlar.db";
string projectRootPath = builder.Environment.ContentRootPath;
string dbPath = Path.Combine(projectRootPath, dbFileName);
string connectionString = $"Data Source={dbPath}";

builder.Services.AddDbContext<OkuyanlarDbContext>(options =>
    options.UseSqlite(connectionString));

// --- SETTINGS ---
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// --- DI (Repositories + Services) ---
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookRatingRepository, BookRatingRepository>();


builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<BookService>();

builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Forgot/Reset token (memory cache)
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IPasswordTokenService, PasswordTokenService>();

// --- AUTH ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

var app = builder.Build();

// --- MIDDLEWARE ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// --- SEED (varsa) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<OkuyanlarDbContext>();
        var hasher = services.GetRequiredService<IPasswordHasher<User>>();
        DbInitializer.Initialize(context, hasher);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database seed error.");
    }
}

app.Run();
