using Microsoft.AspNetCore.Mvc;
using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Interfaces;
using Okuyanlar.Service.Services;
using Okuyanlar.Web.Models;

namespace Okuyanlar.Web.Controllers
{
  /// <summary>
  /// Handles administrative operations related to User Management.
  /// Allows authorized roles to create new users (Librarians, EndUsers).
  /// </summary>
  public class UserController : Controller
  {
    private readonly UserService _userService;
    private readonly IUserRepository _userRepository;

    public UserController(UserService userService, IUserRepository userRepository)
    {
      _userService = userService;
      _userRepository = userRepository;
    }

    /// <summary>
    /// Displays the user creation form.
    /// </summary>    
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }

    /// <summary>
    /// Processes the user creation form submission.
    /// Validates input and delegates the creation logic to the Service layer.
    /// </summary>
    /// <param name="model">The data transfer object containing new user details.</param>
    [HttpPost]
    public IActionResult Create(CreateUserViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      try
      {
        // TODO: Replace with currently logged-in user after implementing Authentication
        var currentAdmin = _userRepository.GetByUsername("Admin");

        if (currentAdmin == null)
        {
          currentAdmin = new User { Username = "System", Role = Core.Enums.UserRole.Admin };
        }

        var newUser = new User
        {
          Username = model.Username,
          Email = model.Email,
          Role = model.Role
        };

        _userService.CreateUser(currentAdmin, newUser);

        TempData["SuccessMessage"] = $"{newUser.Username} başarıyla oluşturuldu ve şifre linki gönderildi!";
        return RedirectToAction("Create");
      }
      catch (Exception ex)
      {
        ModelState.AddModelError("", ex.Message);
        return View(model);
      }
    }
  }
}