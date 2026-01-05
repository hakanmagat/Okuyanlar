using System.ComponentModel.DataAnnotations;

namespace Okuyanlar.Web.Models
{
  /// <summary>
  /// View Model (DTO) for the Password Creation flow.
  /// Carries the secure token, user email, and the new password with validation rules.
  /// </summary>
  public class CreatePasswordViewModel
  {
    /// <summary>
    /// The security token received via email link. Used to verify the request.
    /// </summary>
    [Required]
    public string? Token { get; set; }

    /// <summary>
    /// The email address of the user setting the password.
    /// </summary>
    [Required]
    public string? Email { get; set; }

    /// <summary>
    /// The new password chosen by the user.
    /// Must be at least 6 characters long.
    /// </summary>
    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [Display(Name = "New Password")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    /// <summary>
    /// Confirmation field to ensure the user typed the password correctly.
    /// </summary>
    [Required(ErrorMessage = "Password confirmation is required.")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    [DataType(DataType.Password)]
    public string? ConfirmPassword { get; set; }
    [Range(typeof(bool), "true", "true", ErrorMessage = "KVKK consent is required to continue.")]
    public bool KvkkAccepted { get; set; }
  }
}