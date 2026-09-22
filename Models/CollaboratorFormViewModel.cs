using Automind.CadastroColaboradores.Helpers;

namespace Automind.CadastroColaboradores.Models;

public sealed class CollaboratorFormViewModel
{
    private string? _telefoneCelular;

    public string? Chamado { get; set; }
    public string Fonte { get; set; } = "Manual";
    public string NomeCompleto { get; set; } = string.Empty;
    public string NomeGuerra { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string? TelefoneCelular
    {
        get => _telefoneCelular;
        set => _telefoneCelular = TelefoneFormatter.FormatarCelular(value);
    }

    public bool DivulgarContato { get; set; }
    public string? LocalTrabalho { get; set; }
    public string? SuperiorImediato { get; set; }
    public string? CargoPortugues { get; set; }
    public string? CargoIngles { get; set; }
    public string? Departamento { get; set; }
    public string Empresa { get; set; } = "Automind";
    public string? PerfilUsuario { get; set; }
    public string? GrupoTrabalho { get; set; }
    public string? OuSelecionada { get; set; }
    public List<GroupSuggestion> GruposSugeridos { get; set; } = [];
}

public sealed class GroupSuggestion
{
    public string Nome { get; set; } = string.Empty;
    public int EncontradoEm { get; set; }
    public int TotalComparados { get; set; }
    public bool Selecionado { get; set; }
    public bool Protegido { get; set; }
}
