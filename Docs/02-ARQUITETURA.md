# Arquitetura acordada

- ASP.NET Core MVC
- .NET 10
- IIS Windows
- Servidor: `10.1.2.21`
- HTTP interno inicialmente (sem HTTPS/certificado nesta fase)
- SQL Server: **não instalar agora**; primeiro verificar infraestrutura corporativa existente.
- Integracoes: TOPdesk Bridge/REST, Active Directory e Microsoft Graph; Teams permanece futura.

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

## 25/09/2026 - arquitetura Microsoft Graph da v0.1.12

A camada M365 passa a usar:
- `IMicrosoft365LicenseService`: contrato de leitura do inventario;
- `Microsoft365LicenseService`: OAuth App-only + certificado e chamada HTTP ao Microsoft Graph;
- certificado localizado por thumbprint em `LocalMachine\My`;
- nenhuma chave/segredo em `appsettings.json`;
- `IHttpClientFactory` com cliente `MicrosoftGraph` e timeout de 15 segundos;
- cache em memoria de 5 minutos para `subscribedSkus`;
- falha do Graph isolada da carga do formulario/AD.

Fluxo da v0.1.12:

`IIS/gMSA -> certificado local -> Entra token endpoint -> Graph /subscribedSkus -> ViewBag -> Novo.cshtml`

O Graph usa validacao TLS padrao do Windows/.NET. Nao existe `ServerCertificateValidationCallback` permissivo no codigo.

`LicenseWritesEnabled=false` e nao ha endpoint de escrita M365 nesta versao, mesmo que as permissoes de escrita ja tenham sido validadas no tenant.


## 25/09/2026 - arquitetura Microsoft Graph da v0.1.13

A camada M365 passa de inventario somente leitura para escrita piloto controlada.

Fluxo:

`Novo.cshtml -> ValidarAd (AD + inventario Graph) -> CreateUserAsync AD -> JavaScript poll -> AplicarLicencasMicrosoft365 -> Graph user -> UsageLocation -> assignLicense -> readback/auditoria`

Caracteristicas:
- `LicenseWritesEnabled=true`;
- selecao somente em SKU com `CanAssign=true`;
- backend sempre consulta inventario fresco antes da escrita;
- endpoint M365 exige operador autorizado e usuario AD dentro de `WriteAllowedOuDns`;
- se o usuario ainda nao existir no Entra, retorna `PendingSynchronization` sem escrever;
- navegador faz polling configuravel (`10s`, maximo `180s` nesta versao);
- `UsageLocation` vazio recebe `BR`; valor nao vazio e diferente de `BR` bloqueia a automacao;
- `assignLicense` adiciona somente SKUs ausentes;
- rollback nunca remove licenca que ja existia antes da operacao;
- cache do inventario e invalidado apos assign/rollback;
- auditoria M365 usa `IProvisioningAuditService`.

Nao ha daemon/fila/background worker na v0.1.13; o retry automatico existe enquanto a pagina permanece aberta.
