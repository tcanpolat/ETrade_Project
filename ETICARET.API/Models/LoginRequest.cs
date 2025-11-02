using System.ComponentModel.DataAnnotations;

namespace ETICARET.API.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage ="Email Alanı Zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi girin")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Şifre Alanı Zorunludur")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
