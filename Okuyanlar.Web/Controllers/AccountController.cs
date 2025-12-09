using Microsoft.AspNetCore.Mvc;
using Okuyanlar.Service.Services;
using Okuyanlar.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Okuyanlar.Web.Controllers
{
  /// <summary>
  /// Manages user authentication flows: Login, Logout, and Password Creation.
  /// </summary>
  public class AccountController : Controller
  {
    private readonly UserService _userService;

    public AccountController(UserService userService)
    {
      _userService = userService;
    }

    /// <summary>
    /// Displays the password creation form for users coming from the email link.
    /// </summary>
    [HttpGet]
    public IActionResult CreatePassword(string token, string email)
    {
      if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
      {
        return BadRequest("Invalid link.");
      }

      var model = new CreatePasswordViewModel
      {
        Token = token,
        Email = email
      };

      return View(model);
    }

    /// <summary>
    /// Processes the password creation form.
    /// </summary>    
    [HttpPost]
    public IActionResult CreatePassword(CreatePasswordViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      try
      {
        // Token doğrulama işlemi burada yapılabilir (Service katmanında)
        // Şimdilik direkt şifreyi atıyoruz.
        _userService.SetPassword(model.Email, model.Password);

        return RedirectToAction("Login"); // Başarılıysa girişe yolla
      }
      catch (Exception ex)
      {
        ModelState.AddModelError("", ex.Message);
        return View(model);
      }
    }

    /// <summary>
    /// Displays the login page.
    /// </summary>
    [HttpGet]
    public IActionResult Login()
    {
      return View();
    }

    /// <summary>
    /// Authenticates the user and establishes a cookie session.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
      try
      {
        // 1. Servisten kullanıcıyı doğrula
        var user = _userService.ValidateUser(email, password);

        if (user == null)
        {
          ViewBag.Error = "E-posta veya şifre hatalı.";
          return View();
        }

        // 2. Kimlik Kartı (Claims) Oluştur
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()) // Rol bazlı yetki için
            };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        // 3. Çerezi Tarayıcıya Ver (Giriş Yap)
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        return RedirectToAction("Index", "Home");
      }
      catch (Exception ex)
      {
        ViewBag.Error = ex.Message;
        return View();
      }
    }

    /// <summary>
    /// Signs the user out by deleting the authentication cookie.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
      await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
      return RedirectToAction("Login");
    }
  }
}