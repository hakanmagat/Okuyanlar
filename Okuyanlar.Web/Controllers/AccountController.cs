using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Enums;
using Okuyanlar.Core.Interfaces;
using Okuyanlar.Service.Services;
using Okuyanlar.Web.Models;
using System.Security.Claims;

namespace Okuyanlar.Web.Controllers
{
  /// <summary>
  /// Central controller for all account-related actions:
  /// Login, Logout, Password Setup, and User Creation.
  /// </summary>
  public class AccountController : Controller
  {
    private readonly UserService _userService;
    private readonly IUserRepository _userRepository;

    public AccountController(UserService userService, IUserRepository userRepository)
    {
      _userService = userService;
      _userRepository = userRepository;
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
        // Token authentication is in here
        if (string.IsNullOrEmpty(model.Email))
        {
          ModelState.AddModelError("", "Email shouldn't be empty.");
          return View(model);
        }
        if (string.IsNullOrEmpty(model.Password))
        {
          ModelState.AddModelError("", "Password shouldn't be empty.");
          return View(model);
        }
        _userService.SetPassword(model.Email, model.Password);

        return RedirectToAction("Login");
      }
      catch (Exception ex)
      {
        ModelState.AddModelError("", ex.Message);
        return View(model);
      }
    }

  }
}