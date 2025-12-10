using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Interfaces;
using Okuyanlar.Service.Services;
using Okuyanlar.Web.Models;
using Okuyanlar.Core.Enums;

namespace Okuyanlar.Web.Controllers
{
  /// <summary>
  /// Handles administrative operations related to User Management.
  /// Enforces role-based access control for creating users.
  /// </summary>
  [Authorize(Roles = "SystemAdmin, Admin, Librarian")]
  public class UserController : Controller
  {
    private readonly UserService _userService;
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    /// <param name="userService">Service for business logic operations.</param>
    /// <param name="userRepository">Repository for data access.</param>
    public UserController(UserService userService, IUserRepository userRepository)
    {
      _userService = userService;
      _userRepository = userRepository;
    }

    /// <summary>
    /// Displays the user creation form.
    /// Populates the role dropdown based on the current user's permission level.
    /// </summary>
    /// <returns>The creation view with the populated model.</returns>
    [HttpGet]
    public IActionResult Create()
    {
      var model = new CreateUserViewModel();

      var currentUser = GetCurrentUser();
      if (currentUser != null)
      {
        model.AllowedRoles = GetAllowedRolesForUser(currentUser.Role);
      }

      return View(model);
    }

    /// <summary>
    /// Processes the user creation form submission.
    /// </summary>
    /// <param name="model">The data transfer object containing new user details.</param>
    /// <returns>Redirects to Create on success, or redisplays the form on failure.</returns>
    [HttpPost]
    public IActionResult Create(CreateUserViewModel model)
    {
      // We must fetch the current user to re-populate the dropdown if validation fails.
      var currentUser = GetCurrentUser();

      if (!ModelState.IsValid)
      {
        if (currentUser != null)
          model.AllowedRoles = GetAllowedRolesForUser(currentUser.Role);

        return View(model);
      }

      try
      {
        if (currentUser == null) return RedirectToAction("Login", "Account");

        var newUser = new User
        {
          Username = model.Username,
          Email = model.Email,
          Role = model.Role
        };

        _userService.CreateUser(currentUser, newUser);

        TempData["SuccessMessage"] = $"{newUser.Username} created successfully.";
        return RedirectToAction("Create");
      }
      catch (Exception ex)
      {
        ModelState.AddModelError("", ex.Message);

        // Re-populate the dropdown so the user can try again
        if (currentUser != null)
          model.AllowedRoles = GetAllowedRolesForUser(currentUser.Role);

        return View(model);
      }
    }

    // --- HELPER METHODS ---

    /// <summary>
    /// Retrieves the currently logged-in user entity from the database using the identity cookie.
    /// </summary>
    /// <returns>The User entity if found; otherwise, null.</returns>
    private User? GetCurrentUser()
    {
      var username = User.Identity?.Name;
      if (string.IsNullOrEmpty(username)) return null;
      return _userRepository.GetByUsername(username);
    }

    /// <summary>
    /// Determines which roles the current user is allowed to create based on the hierarchy rules.
    /// </summary>
    /// <param name="currentRole">The role of the logged-in user.</param>
    /// <returns>A list of SelectListItems for the dropdown menu.</returns>
    private IEnumerable<SelectListItem> GetAllowedRolesForUser(UserRole currentRole)
    {
      var allowedRoles = new List<UserRole>();

      switch (currentRole)
      {
        case UserRole.SystemAdmin:
          // Rule: SystemAdmin can only create Admin
          allowedRoles.Add(UserRole.Admin);
          break;

        case UserRole.Admin:
          // Rule: Admin can create Admin and Librarian
          allowedRoles.Add(UserRole.Admin);
          allowedRoles.Add(UserRole.Librarian);
          break;

        case UserRole.Librarian:
          // Rule: Librarian can only create EndUser
          allowedRoles.Add(UserRole.EndUser);
          break;
      }

      // Convert to SelectListItem for the View
      return allowedRoles.Select(r => new SelectListItem
      {
        Text = r.ToString(),
        Value = r.ToString()
      });
    }
  }
}