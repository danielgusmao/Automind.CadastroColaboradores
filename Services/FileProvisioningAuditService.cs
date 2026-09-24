using System.Text.Json;
using Automind.CadastroColaboradores.Models;

namespace Automind.CadastroColaboradores.Services;

public sealed class FileProvisioningAuditService(AdConnectionFactory directory) : IProvisioningAuditService
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public async Task AppendAsync(ProvisioningAuditEntry entry, CancellationToken cancellationToken = default)
    {
        var path = directory.ProvisioningAuditFile;
        var folder = Path.GetDirectoryName(path);
        if (string.IsNullOrWhiteSpace(folder))
            throw new InvalidOperationException("Caminho de auditoria de provisionamento inválido.");

        await _gate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(folder);
            var json = JsonSerializer.Serialize(entry, JsonOptions);
            await File.AppendAllTextAsync(path, json + Environment.NewLine, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }
}
