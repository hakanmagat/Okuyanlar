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
    [Required(ErrorMessage = "Şifre zorunludur.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalı.")]
    [Display(Name = "Yeni Şifre")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    /// <summary>
    /// Confirmation field to ensure the user typed the password correctly.
    /// </summary>
    [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
    [Display(Name = "Şifre Tekrar")]
    [DataType(DataType.Password)]
    public string? ConfirmPassword { get; set; }
        [Range(typeof(bool), "true", "true", ErrorMessage = "Devam etmek için KVKK onayı gerekir.")]
        public bool KvkkAccepted { get; set; }
    }
}