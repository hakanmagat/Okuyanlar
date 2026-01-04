using System.ComponentModel.DataAnnotations;

namespace Okuyanlar.Web.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "E-posta zorunlu.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta gir.")]
        public string Email { get; set; } = "";
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "E-posta zorunlu.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta gir.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Kod zorunlu.")]
        public string Code { get; set; } = "";

        [Required(ErrorMessage = "Yeni şifre zorunlu.")]
        [MinLength(8, ErrorMessage = "Şifre en az 8 karakter olmalı.")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "Şifre tekrar zorunlu.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = "";
    }
}
