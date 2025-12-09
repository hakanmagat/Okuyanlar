using System.ComponentModel.DataAnnotations;
using Okuyanlar.Core.Enums;

namespace Okuyanlar.Web.Models
{
  /// <summary>
  /// View Model (DTO) used for transferring user creation data from the View to the Controller.
  /// Contains validation attributes for client and server-side checks.
  /// </summary>
  public class CreateUserViewModel
  {
    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [Display(Name = "Kullanıcı Adı")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-posta Adresi")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Rol seçimi zorunludur.")]
    [Display(Name = "Kullanıcı Rolü")]
    public UserRole Role { get; set; }
  }
}