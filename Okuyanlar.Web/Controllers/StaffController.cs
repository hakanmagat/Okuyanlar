using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Enums;
using Okuyanlar.Core.Extensions;
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
        private readonly IWebHostEnvironment _env;

        public StaffController(UserService userService, IUserRepository userRepository, IEmailService emailService, IBookRepository bookRepository, IWebHostEnvironment env)
        {
            _userService = userService;
            _userRepository = userRepository;
            _emailService = emailService;
            _bookRepository = bookRepository;
            _env = env;
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
                        Text = r.GetDisplayName(),
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
                        Text = r.GetDisplayName(),
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
                    Text = r.GetDisplayName(),
                    Value = r.ToString()
                }).ToList();
                return View("~/Views/Staff/UserCreate.cshtml", model);
            }

            if (!ModelState.IsValid)
            {
                model.AllowedRoles = allowed.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = r.GetDisplayName(),
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
                    Text = r.GetDisplayName(),
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
        [HttpGet]
        public IActionResult BookManage()
        {
            var books = _bookRepository.GetAll()
                .OrderByDescending(b => b.CreatedAt)
                .ToList();
            return View("~/Views/Staff/BookManage.cshtml", books);
        }

        [Authorize(Roles = "SystemAdmin,Admin,Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BookCreate(Book model, IFormFile? coverImage)
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
                // Handle optional cover image upload
                if (coverImage != null && coverImage.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var ext = Path.GetExtension(coverImage.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError(string.Empty, "Only image files are allowed (.jpg, .jpeg, .png, .gif, .webp).");
                        return View("~/Views/Staff/BookCreate.cshtml", model);
                    }

                    // Max 5 MB
                    const long maxBytes = 5 * 1024 * 1024;
                    if (coverImage.Length > maxBytes)
                    {
                        ModelState.AddModelError(string.Empty, "Image file too large (max 5 MB).");
                        return View("~/Views/Staff/BookCreate.cshtml", model);
                    }

                    var webRoot = _env.WebRootPath;
                    var coversDir = Path.Combine(webRoot, "images", "covers");
                    if (!Directory.Exists(coversDir))
                    {
                        Directory.CreateDirectory(coversDir);
                    }

                    // Unique file name
                    var fileName = $"book_{Guid.NewGuid():N}{ext}";
                    var savePath = Path.Combine(coversDir, fileName);
                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        coverImage.CopyTo(stream);
                    }

                    // Persist relative path for web usage
                    model.CoverUrl = $"/images/covers/{fileName}";
                }

                model.CreatedAt = DateTime.UtcNow;
                _bookRepository.Add(model);
                TempData["SuccessMessage"] = $"Book '{model.Title}' has been successfully added to the inventory.";
                return RedirectToAction("BookManage");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating book: {ex.Message}");
                return View("~/Views/Staff/BookCreate.cshtml", model);
            }
        }

        // -------------------------
        // Book Edit
        // -------------------------
        [Authorize(Roles = "SystemAdmin,Admin,Librarian")]
        [HttpGet]
        public IActionResult BookEdit(int id)
        {
            var book = _bookRepository.GetById(id);
            if (book == null)
            {
                return NotFound();
            }

            return View("~/Views/Staff/BookEdit.cshtml", book);
        }

        [Authorize(Roles = "SystemAdmin,Admin,Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BookEdit(Book model, IFormFile? coverImage)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Staff/BookEdit.cshtml", model);
            }

            var book = _bookRepository.GetById(model.Id);
            if (book == null)
            {
                return NotFound();
            }

            // Check for duplicate ISBN on other books
            var isbnOwner = _bookRepository.GetByISBN(model.ISBN);
            if (isbnOwner != null && isbnOwner.Id != model.Id)
            {
                ModelState.AddModelError(string.Empty, $"Another book with ISBN '{model.ISBN}' already exists.");
                return View("~/Views/Staff/BookEdit.cshtml", model);
            }

            try
            {
                // Handle optional cover image upload
                if (coverImage != null && coverImage.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var ext = Path.GetExtension(coverImage.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError(string.Empty, "Only image files are allowed (.jpg, .jpeg, .png, .gif, .webp).");
                        return View("~/Views/Staff/BookEdit.cshtml", model);
                    }

                    const long maxBytes = 5 * 1024 * 1024;
                    if (coverImage.Length > maxBytes)
                    {
                        ModelState.AddModelError(string.Empty, "Image file too large (max 5 MB).");
                        return View("~/Views/Staff/BookEdit.cshtml", model);
                    }

                    var webRoot = _env.WebRootPath;
                    var coversDir = Path.Combine(webRoot, "images", "covers");
                    if (!Directory.Exists(coversDir))
                    {
                        Directory.CreateDirectory(coversDir);
                    }

                    var fileName = $"book_{Guid.NewGuid():N}{ext}";
                    var savePath = Path.Combine(coversDir, fileName);
                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        coverImage.CopyTo(stream);
                    }

                    // Optional cleanup: remove old cover if it was inside covers dir
                    if (!string.IsNullOrWhiteSpace(book.CoverUrl))
                    {
                        var oldPath = book.CoverUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
                        var fullOldPath = Path.Combine(_env.WebRootPath, oldPath);
                        if (System.IO.File.Exists(fullOldPath) && fullOldPath.Contains(Path.Combine(_env.WebRootPath, "images", "covers")))
                        {
                            System.IO.File.Delete(fullOldPath);
                        }
                    }

                    book.CoverUrl = $"/images/covers/{fileName}";
                }

                // Update fields
                book.Title = model.Title;
                book.Author = model.Author;
                book.ISBN = model.ISBN;
                book.Stock = model.Stock;
                book.IsActive = model.IsActive;
                book.Category = model.Category;

                _bookRepository.Update(book);
                TempData["SuccessMessage"] = $"Book '{book.Title}' has been updated.";
                return RedirectToAction("BookManage");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating book: {ex.Message}");
                return View("~/Views/Staff/BookEdit.cshtml", model);
            }
        }

        // -------------------------
        // Remove Book Cover
        // -------------------------
        [Authorize(Roles = "SystemAdmin,Admin,Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveBookCover(int id)
        {
            var book = _bookRepository.GetById(id);
            if (book == null)
            {
                return NotFound();
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(book.CoverUrl))
                {
                    var relativePath = book.CoverUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
                    var fullPath = Path.Combine(_env.WebRootPath, relativePath);

                    var coversRoot = Path.Combine(_env.WebRootPath, "images", "covers");
                    if (System.IO.File.Exists(fullPath) && fullPath.StartsWith(coversRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                book.CoverUrl = null;
                _bookRepository.Update(book);
                TempData["SuccessMessage"] = $"Cover removed for '{book.Title}'.";
                return RedirectToAction("BookEdit", new { id = book.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to remove cover: {ex.Message}";
                return RedirectToAction("BookEdit", new { id = id });
            }
        }
    }
}
