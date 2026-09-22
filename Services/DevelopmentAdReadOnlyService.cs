namespace Automind.CadastroColaboradores.Services;

// MOCK DE DESENVOLVIMENTO. Nao consulta nem modifica o AD real.
public sealed class DevelopmentAdReadOnlyService : IAdReadOnlyService
{
    public Task<bool> ValidateCredentialsAsync(string usuario, string senha, CancellationToken cancellationToken = default)
        => Task.FromResult(!string.IsNullOrWhiteSpace(usuario) && !string.IsNullOrWhiteSpace(senha));

    public Task<bool> IsAuthorizedAsync(string usuario, CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public Task<IReadOnlyList<string>> GetOrganizationalUnitsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<string>>([
            "03.UDN/Engenharia",
            "03.UDN/GEAUT",
            "03.UDN/GELOG",
            "03.UDN/GEMED",
            "03.UDN/GETEC",
            "03.UDN/Inovacao",
            "03.UDN/Suporte",
            "03.UDN/T&S",
            "03.UDN/Tecnologia",
            "04.UDA/ADM",
            "04.UDA/Backoffice",
            "04.UDA/Financeiro",
            "04.UDA/GSTI",
            "04.UDA/Pessoas",
            "04.UDA/SGI",
            "04.UDA/SGSI",
            "05.Terceiros-Ext"
        ]);
}
