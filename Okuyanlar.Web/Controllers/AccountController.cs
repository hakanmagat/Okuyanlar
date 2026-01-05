using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Okuyanlar.Core.Interfaces;
using Okuyanlar.Core.Enums;
using Okuyanlar.Service.Services;
using Okuyanlar.Web.Models;
using Okuyanlar.Web.Services;
using System.Security.Claims;
using System.Linq;

namespace Okuyanlar.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IPasswordTokenService _passwordTokenService;

        public AccountController(
            UserService userService,
            IUserRepository userRepository,
            IEmailService emailService,
            IPasswordTokenService passwordTokenService)
        {
            _userService = userService;
            _userRepository = userRepository;
            _emailService = emailService;
            _passwordTokenService = passwordTokenService;
        }

        // -------------------- LOGIN --------------------
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                var user = _userService.ValidateUser(email, password);

                if (user == null)
                {
                    ViewBag.Error = "Email or password is incorrect.";
                    return View();
                }

                // If you want to enforce KVKK consent here as well, you can lock it:
                // if (!user.KvkkAccepted) { ViewBag.Error = "KVKK consent is required to continue."; return View(); }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // -------------------- CREATE PASSWORD (ACCOUNT ACTIVATION) --------------------
        [HttpGet]
        public IActionResult CreatePassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                return BadRequest("Invalid link.");

            var model = new CreatePasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePassword(CreatePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Set the password
                _userService.SetPassword(model.Email!, model.Password!);

                // Mark KVKK consent on the user
                var user = _userRepository.GetByEmail(model.Email!);
                if (user != null)
                {
                    user.KvkkAccepted = true;
                    user.KvkkAcceptedAt = DateTime.UtcNow;
                    _userRepository.Update(user);
                }

                TempData["SuccessMessage"] = "Your account is activated. You can log in.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // -------------------- FORGOT / RESET (CODE-BASED) --------------------
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Email cannot be empty.";
                return View();
            }

            var user = _userRepository.GetByEmail(email);

            // To avoid information leakage, keep the same response regardless:
            if (user == null)
            {
                ViewBag.Success = "If this email is registered, a password reset code has been sent.";
                return View();
            }

            var code = _passwordTokenService.CreateResetToken(email);
            _emailService.SendPasswordResetLink(email, user.Username, code);

            ViewBag.Success = "A password reset code has been sent to the email address.";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(string email, string code, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(code) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Error = "Fill in all fields.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            if (!_passwordTokenService.ValidateResetToken(email, code))
            {
                ViewBag.Error = "Code is invalid or expired.";
                return View();
            }

            try
            {
                _userService.SetPassword(email!, newPassword!);
                _passwordTokenService.ConsumeResetToken(email, code);

                TempData["SuccessMessage"] = "Your password has been updated. You can log in.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        // -------------------- PROFILE (MEMBER AREA) --------------------
        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
                return RedirectToAction("Login");

            var user = _userRepository.GetByEmail(email);
            if (user == null)
                return RedirectToAction("Login");

            var allowedRoles = _userService
                .GetCreatableRoles(user.Role)
                .Select(r => r.ToString())
                .ToList();

            ViewBag.AllowedRoles = allowedRoles;
            ViewBag.CanCreateUser = allowedRoles.Any();

            return View(user);
        }

        // A (Modal) -> Ajax post
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid form.");

            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            // Verify the current password
            var user = _userService.ValidateUser(email, model.CurrentPassword);
            if (user == null)
                return BadRequest("Current password is incorrect.");

            try
            {
                _userService.SetPassword(email, model.NewPassword);
                return Ok("Password updated.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
