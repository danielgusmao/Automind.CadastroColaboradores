using System.DirectoryServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Automind.CadastroColaboradores.Models;

namespace Automind.CadastroColaboradores.Services;

[SupportedOSPlatform("windows")]
public sealed class WindowsAdProvisioningWriteService(
    AdConnectionFactory directory,
    IProvisioningAuditService audit,
    ILogger<WindowsAdProvisioningWriteService> logger) : IAdProvisioningWriteService
{
    private const int AccountDisable = 0x0002;
    private const int NormalAccount = 0x0200;

    private static readonly string[] BaseAttributeNames =
    [
        "givenName", "sn", "displayName", "description", "physicalDeliveryOfficeName",
        "telephoneNumber", "mail", "title", "department", "company", "userPrincipalName",
        "sAMAccountName", "userAccountControl"
    ];

    public bool IsWriteModeEnabled => directory.IsPilotWriteEnabled;
    public bool GroupWritesEnabled => directory.GroupWritesEnabled;
    public IReadOnlySet<string> WriteAllowedOuDns => directory.WriteAllowedOuDns;
    public string Mode => directory.Mode;

    public async Task<AdProvisioningCreateResponse> CreateUserAsync(
        AdProvisioningWriteCommand command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var response = new AdProvisioningCreateResponse();
        if (!IsWriteModeEnabled)
            return Fail(response, "A aplicação está em modo somente leitura. A criação no AD permanece bloqueada.");

        var commandError = ValidateCommand(command);
        if (commandError is not null)
            return Fail(response, commandError);

        if (!WriteAllowedOuDns.Contains(command.OuDistinguishedName))
            return Fail(response, "A OU informada não pertence à allowlist de escrita do piloto.");

        if (command.GroupDns.Count > 0)
            return Fail(response, "A escrita de grupos ainda não está habilitada no piloto. Remova os grupos selecionados antes da criação.");

        var technicalIdentity = WindowsIdentity.GetCurrent()?.Name ?? string.Empty;
        if (!string.Equals(technicalIdentity, directory.ExpectedTechnicalIdentity, StringComparison.OrdinalIgnoreCase))
            return Fail(response, "A identidade técnica atual do processo não corresponde à gMSA autorizada para o piloto.");

        var operationId = Guid.NewGuid().ToString("N");
        var changedAttributes = BaseAttributeNames
            .Where(x => !string.Equals(x, "telephoneNumber", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrWhiteSpace(command.TelephoneNumber))
            .Concat(["manager"])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        string? userDn = null;
        var userCreated = false;
        string? temporaryPassword = null;

        try
        {
            // A auditoria precisa estar gravável antes da primeira escrita no AD.
            await AuditAsync(operationId, command, technicalIdentity, "provisioning-start", "started", null, changedAttributes, cancellationToken);
            AddStep(response, "audit", true, "Auditoria iniciada antes da escrita no AD.");

            cancellationToken.ThrowIfCancellationRequested();
            temporaryPassword = GeneratePassword(directory.InitialPasswordLength);

            using var ou = directory.Open(command.OuDistinguishedName);
            using var user = ou.Children.Add($"CN={EscapeRdn(command.Cn)}", "user");
            user.Properties["sAMAccountName"].Value = command.SamAccountName;
            user.Properties["userAccountControl"].Value = NormalAccount | AccountDisable;
            user.CommitChanges();

            userCreated = true;
            userDn = Convert.ToString(user.Properties["distinguishedName"].Value)
                ?? $"CN={EscapeRdn(command.Cn)},{command.OuDistinguishedName}";
            response.UserCreated = true;
            response.DistinguishedName = userDn;
            AddStep(response, "create-disabled", true, "Usuário criado inicialmente desabilitado.");
            await AuditAsync(operationId, command, technicalIdentity, "create-disabled", "success", userDn, ["sAMAccountName", "userAccountControl"], cancellationToken);

            ApplyBaseAttributes(user, command);
            user.CommitChanges();
            AddStep(response, "attributes", true, "Atributos aprovados aplicados.");
            await AuditAsync(operationId, command, technicalIdentity, "attributes", "success", userDn, changedAttributes.Where(x => x != "manager").ToList(), cancellationToken);

            user.Invoke("SetPassword", new object[] { temporaryPassword });
            user.CommitChanges();
            AddStep(response, "password", true, "Senha inicial definida; o valor não é registrado pela aplicação.");
            await AuditAsync(operationId, command, technicalIdentity, "password", "success", userDn, [], cancellationToken);

            user.Properties["manager"].Value = command.ManagerDistinguishedName;
            user.CommitChanges();
            AddStep(response, "manager", true, "Superior imediato definido pelo DistinguishedName validado.");
            await AuditAsync(operationId, command, technicalIdentity, "manager", "success", userDn, ["manager"], cancellationToken);

            AddStep(response, "groups", true, "Piloto sem escrita de grupos; nenhuma membership foi alterada.");
            await AuditAsync(operationId, command, technicalIdentity, "groups", "skipped", userDn, [], cancellationToken);

            ValidateReadBack(userDn, command, expectDisabled: true);
            AddStep(response, "readback-disabled", true, "Releitura confirmou os atributos e o estado desabilitado.");
            await AuditAsync(operationId, command, technicalIdentity, "readback-disabled", "success", userDn, changedAttributes, cancellationToken);

            using (var toEnable = directory.Open(userDn))
            {
                toEnable.RefreshCache(["userAccountControl"]);
                var currentUac = Convert.ToInt32(toEnable.Properties["userAccountControl"].Value ?? (NormalAccount | AccountDisable));
                toEnable.Properties["userAccountControl"].Value = (currentUac | NormalAccount) & ~AccountDisable;
                toEnable.CommitChanges();
            }

            ValidateReadBack(userDn, command, expectDisabled: false);
            response.Enabled = true;
            AddStep(response, "enable", true, "Conta habilitada somente após a releitura das etapas anteriores.");
            await AuditAsync(operationId, command, technicalIdentity, "enable", "success", userDn, ["userAccountControl"], cancellationToken);

            await AuditAsync(operationId, command, technicalIdentity, "provisioning-complete", "success", userDn, changedAttributes, cancellationToken);

            response.Success = true;
            response.RequiresManualReview = false;
            response.Message = "Usuário piloto criado e validado no Active Directory.";
            response.TemporaryPassword = temporaryPassword;
            return response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            if (userCreated && !string.IsNullOrWhiteSpace(userDn))
                TryKeepDisabled(userDn, response);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "Falha no provisionamento AD. Operacao: {OperationId}; tipo: {ErrorType}; codigo: {Code}",
                operationId,
                exception.GetType().Name,
                exception.HResult);

            if (userCreated && !string.IsNullOrWhiteSpace(userDn))
                TryKeepDisabled(userDn, response);

            try
            {
                await audit.AppendAsync(new ProvisioningAuditEntry
                {
                    OperationId = operationId,
                    Chamado = command.Chamado,
                    Operator = command.Operator,
                    TechnicalIdentity = technicalIdentity,
                    Action = "provisioning-failed",
                    Status = "failed",
                    DistinguishedName = userDn,
                    OuDistinguishedName = command.OuDistinguishedName,
                    Attributes = changedAttributes,
                    Groups = [],
                    ErrorType = exception.GetType().Name,
                    ErrorCode = exception.HResult
                }, CancellationToken.None);
            }
            catch (Exception auditException)
            {
                logger.LogError(
                    "Falha adicional ao registrar auditoria do provisionamento. Operacao: {OperationId}; tipo: {ErrorType}; codigo: {Code}",
                    operationId,
                    auditException.GetType().Name,
                    auditException.HResult);
            }

            response.Success = false;
            response.UserCreated = userCreated;
            response.Enabled = false;
            response.RequiresManualReview = userCreated;
            response.DistinguishedName = userDn;
            response.TemporaryPassword = null;
            response.Message = userCreated
                ? "O provisionamento foi interrompido após a criação do objeto. A conta foi mantida/desabilitada e exige revisão antes de qualquer nova ação."
                : "O provisionamento foi interrompido antes da criação do usuário. Nenhum usuário foi criado.";
            AddStep(response, "failure", false, "Fluxo interrompido no primeiro erro; nenhuma etapa posterior foi executada.");
            return response;
        }
        finally
        {
            // Reduz o tempo de vida da referência da senha; strings em .NET não podem ser apagadas de forma garantida.
            temporaryPassword = null;
        }
    }


    private string? ValidateCommand(AdProvisioningWriteCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Chamado)) return "Chamado TOPdesk obrigatório.";
        if (string.IsNullOrWhiteSpace(command.Operator)) return "Operador humano não identificado.";
        if (string.IsNullOrWhiteSpace(command.Cn) || string.IsNullOrWhiteSpace(command.GivenName) || string.IsNullOrWhiteSpace(command.Surname)) return "Nome completo inválido para criação.";
        if (string.IsNullOrWhiteSpace(command.SamAccountName) || command.SamAccountName.Length > 20) return "sAMAccountName inválido para criação.";
        if (string.IsNullOrWhiteSpace(command.UserPrincipalName) || string.IsNullOrWhiteSpace(command.Mail)) return "UPN/mail obrigatórios.";
        if (!string.Equals(command.UserPrincipalName, command.Mail, StringComparison.OrdinalIgnoreCase)) return "UPN e mail devem ser iguais no piloto.";
        if (!command.Mail.EndsWith($"@{directory.EmailDomain}", StringComparison.OrdinalIgnoreCase)) return "E-mail fora do domínio corporativo configurado.";
        if (string.IsNullOrWhiteSpace(command.Description) || string.IsNullOrWhiteSpace(command.Title) || string.IsNullOrWhiteSpace(command.Department)) return "Cargo, descrição e departamento são obrigatórios.";
        if (!string.Equals(command.Company, "Automind", StringComparison.Ordinal)) return "Company deve permanecer Automind.";
        if (string.IsNullOrWhiteSpace(command.ManagerDistinguishedName)) return "Manager DN obrigatório.";
        if (string.IsNullOrWhiteSpace(command.OuDistinguishedName)) return "OU de destino obrigatória.";
        return null;
    }

    private static void ApplyBaseAttributes(DirectoryEntry user, AdProvisioningWriteCommand command)
    {
        Set(user, "givenName", command.GivenName);
        Set(user, "sn", command.Surname);
        Set(user, "displayName", command.DisplayName);
        Set(user, "description", command.Description);
        Set(user, "physicalDeliveryOfficeName", command.Office);
        Set(user, "telephoneNumber", command.TelephoneNumber);
        Set(user, "mail", command.Mail);
        Set(user, "title", command.Title);
        Set(user, "department", command.Department);
        Set(user, "company", command.Company);
        Set(user, "userPrincipalName", command.UserPrincipalName);
    }

    private void ValidateReadBack(string userDn, AdProvisioningWriteCommand command, bool expectDisabled)
    {
        using var user = directory.Open(userDn);
        user.RefreshCache([
            "cn", "givenName", "sn", "displayName", "description", "physicalDeliveryOfficeName",
            "telephoneNumber", "mail", "title", "department", "company", "manager",
            "userPrincipalName", "sAMAccountName", "userAccountControl"
        ]);

        Expect(user, "cn", command.Cn);
        Expect(user, "givenName", command.GivenName);
        Expect(user, "sn", command.Surname);
        Expect(user, "displayName", command.DisplayName);
        Expect(user, "description", command.Description);
        Expect(user, "physicalDeliveryOfficeName", command.Office);
        Expect(user, "telephoneNumber", command.TelephoneNumber);
        Expect(user, "mail", command.Mail);
        Expect(user, "title", command.Title);
        Expect(user, "department", command.Department);
        Expect(user, "company", command.Company);
        Expect(user, "manager", command.ManagerDistinguishedName);
        Expect(user, "userPrincipalName", command.UserPrincipalName);
        Expect(user, "sAMAccountName", command.SamAccountName);

        var uac = Convert.ToInt32(user.Properties["userAccountControl"].Value ?? 0);
        var disabled = (uac & AccountDisable) == AccountDisable;
        if (disabled != expectDisabled)
            throw new InvalidOperationException("A releitura do userAccountControl não corresponde ao estado esperado.");
    }

    private void TryKeepDisabled(string userDn, AdProvisioningCreateResponse response)
    {
        try
        {
            using var user = directory.Open(userDn);
            user.RefreshCache(["userAccountControl"]);
            var currentUac = Convert.ToInt32(user.Properties["userAccountControl"].Value ?? NormalAccount);
            user.Properties["userAccountControl"].Value = currentUac | NormalAccount | AccountDisable;
            user.CommitChanges();
            response.Enabled = false;
            AddStep(response, "safety-disable", true, "Após a falha, a conta foi confirmada como desabilitada. O objeto não foi excluído automaticamente.");
        }
        catch (Exception disableException)
        {
            logger.LogError(
                "Nao foi possivel confirmar conta desabilitada apos falha. Tipo: {ErrorType}; codigo: {Code}",
                disableException.GetType().Name,
                disableException.HResult);
            response.RequiresManualReview = true;
            AddStep(response, "safety-disable", false, "Não foi possível confirmar automaticamente o estado desabilitado. Interrompa o fluxo e revise o objeto no AD.");
        }
    }

    private Task AuditAsync(
        string operationId,
        AdProvisioningWriteCommand command,
        string technicalIdentity,
        string action,
        string status,
        string? userDn,
        List<string> attributes,
        CancellationToken cancellationToken)
        => audit.AppendAsync(new ProvisioningAuditEntry
        {
            OperationId = operationId,
            Chamado = command.Chamado,
            Operator = command.Operator,
            TechnicalIdentity = technicalIdentity,
            Action = action,
            Status = status,
            DistinguishedName = userDn,
            OuDistinguishedName = command.OuDistinguishedName,
            Attributes = attributes,
            Groups = []
        }, cancellationToken);

    private static void Set(DirectoryEntry entry, string property, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            entry.Properties[property].Value = value.Trim();
    }

    private static void Expect(DirectoryEntry entry, string property, string? expected)
    {
        var actual = entry.Properties[property].Value?.ToString();
        if (string.IsNullOrWhiteSpace(expected))
        {
            if (!string.IsNullOrWhiteSpace(actual))
                throw new InvalidOperationException($"O atributo {property} foi gravado quando deveria permanecer vazio.");
            return;
        }

        if (!string.Equals(actual?.Trim(), expected.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"A releitura do atributo {property} não corresponde ao valor solicitado.");
    }

    private static AdProvisioningCreateResponse Fail(AdProvisioningCreateResponse response, string message)
    {
        response.Success = false;
        response.Message = message;
        response.RequiresManualReview = false;
        return response;
    }

    private static void AddStep(AdProvisioningCreateResponse response, string key, bool success, string message)
        => response.Steps.Add(new AdProvisioningStepResult { Key = key, Success = success, Message = message });

    private static string GeneratePassword(int length)
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@$%*-_+";
        var all = upper + lower + digits + symbols;

        var chars = new List<char>(length)
        {
            Pick(upper), Pick(lower), Pick(digits), Pick(symbols)
        };

        while (chars.Count < length)
            chars.Add(Pick(all));

        for (var i = chars.Count - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars.ToArray());
    }

    private static char Pick(string source) => source[RandomNumberGenerator.GetInt32(source.Length)];

    private static string EscapeRdn(string value)
    {
        var trimmed = value.Trim();
        var builder = new StringBuilder(trimmed.Length + 8);

        for (var i = 0; i < trimmed.Length; i++)
        {
            var ch = trimmed[i];
            var special = ch is ',' or '+' or '"' or '\\' or '<' or '>' or ';' or '=';
            var leadingSpecial = i == 0 && (ch == '#' || ch == ' ');
            var trailingSpace = i == trimmed.Length - 1 && ch == ' ';
            if (special || leadingSpecial || trailingSpace) builder.Append('\\');
            builder.Append(ch);
        }

        return builder.ToString();
    }
}
