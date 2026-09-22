using System.ComponentModel.DataAnnotations;

namespace Automind.CadastroColaboradores.Models;

public sealed class LoginViewModel
{
    [Required]
    [Display(Name = "Usuário")]
    public string Usuario { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;
}
