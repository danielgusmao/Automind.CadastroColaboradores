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

    public async Task<IReadOnlyList<ProvisioningAuditEntry>> ReadRecentAsync(int maxEntries = 1000, CancellationToken cancellationToken = default)
    {
        var path = directory.ProvisioningAuditFile;
        if (!File.Exists(path)) return [];

        var take = Math.Clamp(maxEntries, 1, 10000);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var lines = await File.ReadAllLinesAsync(path, cancellationToken);
            var result = new List<ProvisioningAuditEntry>();
            foreach (var line in lines.TakeLast(take))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var entry = JsonSerializer.Deserialize<ProvisioningAuditEntry>(line, JsonOptions);
                    if (entry is not null) result.Add(entry);
                }
                catch (JsonException)
                {
                    // Linha incompleta/corrompida nao invalida o restante do historico.
                }
            }
            return result;
        }
        finally
        {
            _gate.Release();
        }
    }
}
