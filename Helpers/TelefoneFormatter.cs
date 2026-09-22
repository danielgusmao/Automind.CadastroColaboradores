namespace Automind.CadastroColaboradores.Helpers;

public static class TelefoneFormatter
{
    /// <summary>
    /// Formata telefone celular brasileiro no padrão: (71) 9 8169-6721.
    /// Aceita 10 dígitos (DDD + 8 dígitos), 11 dígitos (DDD + 9 dígitos)
    /// e 13 dígitos iniciados por 55. Para celular com 10 dígitos, inclui o 9
    /// após o DDD.
    /// </summary>
    public static string? FormatarCelular(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return telefone;

        var digitos = new string(telefone.Where(char.IsDigit).ToArray());

        if (digitos.Length == 13 && digitos.StartsWith("55", StringComparison.Ordinal))
            digitos = digitos[2..];

        if (digitos.Length == 10)
            digitos = digitos.Insert(2, "9");

        if (digitos.Length != 11)
            return telefone.Trim();

        return $"({digitos[..2]}) {digitos[2]} {digitos.Substring(3, 4)}-{digitos.Substring(7, 4)}";
    }
}
