using System.ComponentModel.DataAnnotations;

namespace BlogApi.ViewModels.Accounts
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "É necessário informar o e-mail.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "É necessário informar a senha.")]
        public string Password { get; set; }


    }
}