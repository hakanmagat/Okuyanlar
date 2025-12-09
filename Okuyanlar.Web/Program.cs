using Microsoft.EntityFrameworkCore;
using Okuyanlar.Core.Interfaces;
using Okuyanlar.Core.Settings;
using Okuyanlar.Data;
using Okuyanlar.Data.Repositories;
using Okuyanlar.Service.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Okuyanlar.Core.Entities;

var builder = WebApplication.CreateBuilder(args);


string dbFileName = "Okuyanlar.db";

string projectRootPath = builder.Environment.ContentRootPath;

string dbPath = Path.Combine(projectRootPath, dbFileName);

string connectionString = $"Data Source={dbPath}";

builder.Services.AddDbContext<OkuyanlarDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));


builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<IEmailService, SmtpEmailService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();