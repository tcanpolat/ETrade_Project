using System.ComponentModel.DataAnnotations;

namespace ETICARET.API.Models
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Kullanıcı adı alanı zorunludur")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Email alanı zorunludur")]
        [EmailAddress(ErrorMessage = "Email türü geçerli değildir")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Şifre alanı zorunludur")]
        [DataType(DataType.Password)]
        [StringLength(100,MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakterli olmalıdır.")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Şifre tekrar alanı zorunludur")]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage = "Şifreler Eşleşmiyor")]
        public string ConfirmPassword { get; set; }

    }
}
