using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Okuyanlar.Core.Enums;
using Okuyanlar.Core.Interfaces;
using Okuyanlar.Service.Services;
using Okuyanlar.Web.Models;
using System.Security.Claims;

namespace Okuyanlar.Web.Controllers
{
    [Authorize]
    public class StaffController : Controller
    {
        private readonly UserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;

        public StaffController(UserService userService, IUserRepository userRepository, IEmailService emailService)
        {
            _userService = userService;
            _userRepository = userRepository;
            _emailService = emailService;
        }

        // -------------------------
        // Helpers
        // -------------------------
        private UserRole? GetCurrentUserRole()
        {
            var roleStr = User.FindFirstValue(ClaimTypes.Role);
            if (Enum.TryParse<UserRole>(roleStr, out var r)) return r;
            return null;
        }

        private static List<UserRole> GetCreatableRoles(UserRole currentRole)
        {
            // ✅ İstediğin hiyerarşi
            return currentRole switch
            {
                UserRole.SystemAdmin => new List<UserRole> { UserRole.Admin },
                UserRole.Admin => new List<UserRole> { UserRole.Admin, UserRole.Librarian },
                UserRole.Librarian => new List<UserRole> { UserRole.EndUser },
                _ => new List<UserRole>()
            };
        }

        private bool CanCreateUser()
        {
            var role = GetCurrentUserRole();
            if (role == null) return false;
            return GetCreatableRoles(role.Value).Count > 0;
        }

        // -------------------------
        // (A) Modal: Create User
        // -------------------------
        [Authorize(Roles = "SystemAdmin,Admin,Librarian")]
        [HttpGet]
        public IActionResult UserCreateModal()
        {
            var role = GetCurrentUserRole() ?? UserRole.EndUser;
            var allowed = GetCreatableRoles(role);

            var model = new CreateUserViewModel
            {
                AllowedRoles = allowed
                    .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = r.ToString(),
                        Value = r.ToString()
                    })
                    .ToList()
            };

            return PartialView("~/Views/Shared/Staff/_UserCreateModal.cshtml", model);
        }

        [Authorize(Roles = "SystemAdmin,Admin,Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UserCreateModal(CreateUserViewModel model)
        {
            if (!CanCreateUser())
                return BadRequest("Bu işlem için yetkin yok.");

            // Rol validate: seçilen rol allowed mı?
            var currentRole = GetCurrentUserRole() ?? UserRole.EndUser;
            var allowed = GetCreatableRoles(currentRole);

            if (string.IsNullOrWhiteSpace(model.Role) || !Enum.TryParse<UserRole>(model.Role, out var selectedRole))
                return BadRequest("Rol seçimi geçersiz.");

            if (!allowed.Contains(selectedRole))
                return BadRequest("Bu rolü oluşturamazsın.");

            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Username))
                return BadRequest("Kullanıcı adı ve e-posta zorunlu.");

            try
            {
                // UserService içinde CreateUser zaten token üretip email atıyorsa onu kullan.
                _userService.CreateUser(model.Username, model.Email, selectedRole);
                return Ok("Kullanıcı oluşturuldu. Şifre oluşturma bağlantısı e-postaya gönderildi.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // -------------------------
        // (B) Modal: List Users
        // -------------------------
        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpGet]
        public IActionResult UserListModal()
        {
            // Zip’te IUserRepository’de GetAll yoktu; onu ekledik.
            var users = _userRepository.GetAll().ToList();
            return PartialView("~/Views/Shared/Staff/_UserListModal.cshtml", users);
        }

        // ---------------------------------------------------
        // Mevcut full-page CreateUser action’ların kalsın isterse
        // (senin projende kullanılıyorsa)
        // ---------------------------------------------------
        [Authorize(Roles = "SystemAdmin,Admin,Librarian")]
        [HttpGet]
        public IActionResult UserCreate()
        {
            // Eski sayfa akışı lazımsa, rol filtreli şekilde dolduralım:
            var role = GetCurrentUserRole() ?? UserRole.EndUser;
            var allowed = GetCreatableRoles(role);

            var model = new CreateUserViewModel
            {
                AllowedRoles = allowed
                    .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = r.ToString(),
                        Value = r.ToString()
                    })
                    .ToList()
            };

            return View("~/Views/Shared/Staff/UserCreate.cshtml", model);
        }

        [Authorize(Roles = "SystemAdmin,Admin,Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(CreateUserViewModel model)
        {
            // Bu aksiyon senin eski akışınsa dokunmadım; sadece rol filtre koydum
            if (!CanCreateUser())
            {
                ModelState.AddModelError("", "Bu işlem için yetkin yok.");
                model.AllowedRoles = new();
                return View("~/Views/Shared/Staff/UserCreate.cshtml", model);
            }

            var currentRole = GetCurrentUserRole() ?? UserRole.EndUser;
            var allowed = GetCreatableRoles(currentRole);

            if (!Enum.TryParse<UserRole>(model.Role, out var selectedRole) || !allowed.Contains(selectedRole))
            {
                ModelState.AddModelError("", "Bu rolü oluşturamazsın.");
                model.AllowedRoles = allowed.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = r.ToString(),
                    Value = r.ToString()
                }).ToList();
                return View("~/Views/Shared/Staff/UserCreate.cshtml", model);
            }

            if (!ModelState.IsValid)
            {
                model.AllowedRoles = allowed.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = r.ToString(),
                    Value = r.ToString()
                }).ToList();
                return View("~/Views/Shared/Staff/UserCreate.cshtml", model);
            }

            _userService.CreateUser(model.Username, model.Email, selectedRole);
            return RedirectToAction("UserCreate");
        }
    }
}
