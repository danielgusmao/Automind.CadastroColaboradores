# Arquitetura acordada

- ASP.NET Core MVC
- .NET 10
- IIS Windows
- Servidor futuro: `10.1.2.21`
- HTTP interno inicialmente (sem HTTPS/certificado nesta fase)
- SQL Server: **não instalar agora**; primeiro verificar infraestrutura corporativa existente.
- Integrações futuras: TOPdesk REST API, Active Directory, Microsoft Graph e Teams.

## Arquitetura de identidade técnica para escrita no AD - aprovada em 23/09/2026

A autorização humana e a identidade técnica de execução devem permanecer separadas.

### Autorização humana

- o operador autentica no CadastroColaboradores com sua própria conta do AD;
- somente membros do grupo `_informatica` podem acessar o fluxo administrativo de criação;
- o nível administrativo individual do operador no AD não deve ser requisito para criar o colaborador pelo sistema;
- `_informatica` é controle de acesso à aplicação, não identidade de escrita LDAP.

### Identidade técnica

A aplicação deverá executar leituras/escritas de provisionamento no AD usando uma gMSA exclusiva do CadastroColaboradores, com senha gerenciada pelo Windows/AD e sem segredo estático em código ou `appsettings.json`.

Nome operacional sugerido para a gMSA: `gMSA_CadColab$`.

Observação: o nome curto foi escolhido para manter compatibilidade com a recomendação Microsoft de `sAMAccountName` de conta de serviço com 15 caracteres ou menos (o `$` é acrescentado à conta de serviço quando necessário).

A gMSA deve ser utilizável somente pelo servidor IIS `SV052022-6121` / `10.1.2.21` (diretamente ou por um grupo específico de hosts autorizado a recuperar a senha gerenciada).

### Grupo técnico de delegação

Criar um grupo de segurança dedicado, por exemplo:

`SG_CadastroColaboradores_AD_Writer`

Apenas a identidade técnica da aplicação deve receber as permissões delegadas por meio desse grupo. `_informatica` não deve receber ACLs de criação/alteração no AD por causa do CadastroColaboradores.

### Fluxo alvo

`operador membro de _informatica`

-> login no CadastroColaboradores

-> aplicação valida autorização humana

-> operador confirma a criação

-> backend executa como `gMSA_CadColab$`

-> AD avalia somente as permissões delegadas à identidade técnica

-> aplicação registra auditoria ligando o operador humano ao resultado técnico.

### Auditoria obrigatória

Registrar pelo menos:

- usuário humano solicitante;
- chamado TOPdesk;
- identidade técnica utilizada;
- objeto criado/alterado;
- OU de destino;
- grupos adicionados/removidos;
- data/hora;
- resultado e mensagem de erro quando houver.

Nunca registrar senha inicial em banco, log ou histórico.

### Fontes Microsoft

- Application Pool Identities: https://learn.microsoft.com/en-us/iis/manage/configuring-security/application-pool-identities
- Service Accounts in Windows Server: https://learn.microsoft.com/windows/security/identity-protection/access-control/service-accounts
- Manage Group Managed Service Accounts: https://learn.microsoft.com/en-us/windows-server/identity/ad-ds/manage/group-managed-service-accounts/group-managed-service-accounts/manage-group-managed-service-accounts
- Delegation of Control in AD DS: https://learn.microsoft.com/en-us/windows-server/identity/ad-ds/manage/delegation-control-wizard

## 24/09/2026 - camada de provisionamento AD preparada

A arquitetura passa a separar explicitamente:

- `IAdReadOnlyService`: consultas e pre-validacoes;
- `IAdProvisioningWriteService`: escrita AD controlada;
- `IProvisioningAuditService`: trilha JSONL sem senha;
- `IAdAuthenticationService`: login e revalidacao de autorizacao humana;
- `AdConnectionFactory`: configuracao, allowlists de leitura/escrita e conexoes LDAP.

A camada de escrita possui dupla trava de escopo (`Mode=PilotWrite` + `WriteAllowedOuDns`) e confirma a gMSA real do processo. No pacote desta etapa, `Mode=ReadOnly`, portanto a camada existe mas nao esta ativada.
