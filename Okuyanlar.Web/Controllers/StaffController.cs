using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Okuyanlar.Core.Entities;
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
        private readonly IBookRepository _bookRepository;

        public StaffController(UserService userService, IUserRepository userRepository, IEmailService emailService, IBookRepository bookRepository)
        {
            _userService = userService;
            _userRepository = userRepository;
            _emailService = emailService;
            _bookRepository = bookRepository;
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

        private User? GetCurrentUser()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return null;
            return _userRepository.GetByUsername(username);
        }

        private bool CanCreateUser()
        {
            var role = GetCurrentUserRole();
            if (role == null) return false;
            return _userService.GetCreatableRoles(role.Value).Count > 0;
        }

        // -------------------------
        // (A) Modal: Create User
        // -------------------------
        [Authorize(Roles = "SystemAdmin,Admin,Librarian")]
        [HttpGet]
        public IActionResult UserCreateModal()
        {
            var role = GetCurrentUserRole() ?? UserRole.EndUser;
            var allowed = _userService.GetCreatableRoles(role);

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

            return PartialView("~/Views/Staff/_UserCreateModal.cshtml", model);
        }

        [Authorize(Roles = "SystemAdmin,Admin,Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UserCreateModal(CreateUserViewModel model)
        {
            if (!CanCreateUser())
                return BadRequest("You do not have permission for this operation.");

            var currentRole = GetCurrentUserRole() ?? UserRole.EndUser;
            var allowed = _userService.GetCreatableRoles(currentRole);

            if (string.IsNullOrWhiteSpace(model.Role) || !Enum.TryParse<UserRole>(model.Role, out var selectedRole))
                return BadRequest("Role selection is invalid.");

            if (!allowed.Contains(selectedRole))
                return BadRequest("You cannot create this role.");

            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Username))
                return BadRequest("Username and email are required.");

            try
            {
                var creator = GetCurrentUser();
                if (creator == null) return BadRequest("User not found.");

                var newUser = new User { Username = model.Username, Email = model.Email, Role = selectedRole };
                _userService.CreateUser(creator, newUser);
                return Ok("User created. A password creation link has been sent to the email.");
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
            var users = _userRepository.GetAll().ToList();
            return PartialView("~/Views/Staff/_UserListModal.cshtml", users);
        }

        // -------------------------
        // Full-page Create User (legacy flow)
        // -------------------------
        [Authorize(Roles = "SystemAdmin,Admin,Librarian")]
        [HttpGet]
        public IActionResult UserCreate()
        {
            var role = GetCurrentUserRole() ?? UserRole.EndUser;
            var allowed = _userService.GetCreatableRoles(role);

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

            return View("~/Views/Staff/UserCreate.cshtml", model);
        }

        [Authorize(Roles = "SystemAdmin,Admin,Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(CreateUserViewModel model)
        {
            if (!CanCreateUser())
            {
                ModelState.AddModelError(string.Empty, "You do not have permission for this operation.");
                model.AllowedRoles = null;
                return View("~/Views/Staff/UserCreate.cshtml", model);
            }

            var currentRole = GetCurrentUserRole() ?? UserRole.EndUser;
            var allowed = _userService.GetCreatableRoles(currentRole);

            if (!Enum.TryParse<UserRole>(model.Role, out var selectedRole) || !allowed.Contains(selectedRole))
            {
                ModelState.AddModelError(string.Empty, "You cannot create this role.");
                model.AllowedRoles = allowed.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = r.ToString(),
                    Value = r.ToString()
                }).ToList();
                return View("~/Views/Staff/UserCreate.cshtml", model);
            }

            if (!ModelState.IsValid)
            {
                model.AllowedRoles = allowed.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = r.ToString(),
                    Value = r.ToString()
                }).ToList();
                return View("~/Views/Staff/UserCreate.cshtml", model);
            }

            var creator = GetCurrentUser();
            if (creator == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                model.AllowedRoles = allowed.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = r.ToString(),
                    Value = r.ToString()
                }).ToList();
                return View("~/Views/Staff/UserCreate.cshtml", model);
            }

            var newUser = new User { Username = model.Username, Email = model.Email, Role = selectedRole };
            _userService.CreateUser(creator, newUser);
            return RedirectToAction("UserCreate");
        }

        // -------------------------
        // (C) Book Create
        // -------------------------
        [Authorize(Roles = "SystemAdmin,Admin,Librarian")]
        [HttpGet]
        public IActionResult BookCreate()
        {
            return View("~/Views/Staff/BookCreate.cshtml", new Book());
        }

        [Authorize(Roles = "SystemAdmin,Admin,Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BookCreate(Book model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Staff/BookCreate.cshtml", model);
            }

            // Check for duplicate ISBN
            var existingBook = _bookRepository.GetByISBN(model.ISBN);
            if (existingBook != null)
            {
                ModelState.AddModelError(string.Empty, $"A book with ISBN '{model.ISBN}' already exists.");
                return View("~/Views/Staff/BookCreate.cshtml", model);
            }

            try
            {
                model.CreatedAt = DateTime.UtcNow;
                _bookRepository.Add(model);
                TempData["SuccessMessage"] = $"Book '{model.Title}' has been successfully added to the inventory.";
                return RedirectToAction("BookCreate");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating book: {ex.Message}");
                return View("~/Views/Staff/BookCreate.cshtml", model);
            }
        }
    }
}
