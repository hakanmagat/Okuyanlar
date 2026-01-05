using System.ComponentModel.DataAnnotations;
using Okuyanlar.Core.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Okuyanlar.Web.Models
{
  /// <summary>
  /// View Model (DTO) used for transferring user creation data from the View to the Controller.
  /// Contains validation attributes for client and server-side checks.
  /// </summary>
  public class CreateUserViewModel
  {
    [Required(ErrorMessage = "Username is required.")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role selection is required.")]
    [Display(Name = "User Role")]
    public string Role { get; set; } = string.Empty;

    public IEnumerable<SelectListItem>? AllowedRoles { get; set; }
  }
}