# Microsoft 365 / Entra / Microsoft Graph

## Estado vigente - 25/09/2026 - v0.1.12

A integracao Microsoft 365 foi validada no tenant real e a v0.1.12 passa a exibir no formulario de cadastro o inventario de licencas retornado por `GET /v1.0/subscribedSkus`.

Nesta versao:
- leitura do inventario M365: **habilitada**;
- exibicao de licencas/quantidades na tela: **habilitada**;
- selecao de licenca na tela: **desabilitada**;
- atribuicao/remocao de licenca pela aplicacao: **nao implementada**;
- `Automind:Microsoft365:LicenseWritesEnabled=false`.

A permissao de escrita no Graph foi concedida e testada administrativamente, mas o codigo desta versao nao possui endpoint/acao de atribuicao de licencas.

## Identidade App-only

App Registration:
- nome: `Automind.CadColab`;
- tipo: single tenant;
- Application (client) ID: `3558d29a-1098-467c-b4e3-aeaf0afeaaf4`;
- Object ID: `1ed30afe-dfc7-45ea-bdd5-d6d17808091e`;
- Directory (tenant) ID: `9ab05ca8-1779-410b-ae61-82dbd20810f3`.

Autenticacao:
- OAuth 2.0 `client_credentials`;
- certificado em `LocalMachine\\My` no servidor `10.1.2.21`;
- Subject: `CN=Automind.CadColab.Graph`;
- Thumbprint: `A38B594A4A2594A3D83B33B52FD7828935700C29`;
- validade: ate `24/09/2028`;
- chave privada nao exportavel;
- certificado publico: `C:\\Automind.CadastroColaboradores\\Automind.CadColab.Graph.cer`.

A gMSA `AUTOMIND\\gMSA_CadColab$` recebeu somente `Read, Synchronize` no arquivo da chave privada CNG:

`C:\\ProgramData\\Microsoft\\Crypto\\Keys\\3f4b61607e15b0dae42c2006d96b04b8_54f5abd6-87df-4327-bf2f-02580f1eda0a`

Nao exportar a chave privada e nao criar client secret para esta integracao.

## Permissoes Microsoft Graph atuais

Application + Admin Consent:
- `LicenseAssignment.Read.All`;
- `User.Read.All`;
- `User.ReadUpdate.All`;
- `LicenseAssignment.ReadWrite.All`.

Delegated existente:
- `User.Read` - nao utilizado pelo fluxo App-only.

A v0.1.12 usa somente leitura de `subscribedSkus`. As permissoes de escrita foram concedidas para o teste controlado realizado em 25/09/2026, mas `LicenseWritesEnabled=false` e nenhum codigo de atribuicao de licenca existe nesta entrega.

## Validacoes realizadas

### Consulta de SKUs

`GET https://graph.microsoft.com/v1.0/subscribedSkus` autenticado por certificado retornou **20 SKUs**.

Snapshot relevante observado:
- `O365_BUSINESS_PREMIUM` / Microsoft 365 Business Standard: 166 habilitadas, 149 consumidas, 17 disponiveis;
- `O365_BUSINESS_ESSENTIALS` / Microsoft 365 Business Basic: 24 habilitadas, 22 consumidas, 2 disponiveis;
- `ENTERPRISEPACK`: 6 habilitadas, 5 consumidas, 1 disponivel;
- `POWER_BI_PRO`: 4 habilitadas, 1 consumida, 3 disponiveis;
- `PROJECTPROFESSIONAL`: 22 habilitadas, 16 consumidas, 6 disponiveis.

Os valores sao dinamicos; a tela deve sempre usar o Graph como fonte atual, nao estes numeros historicos.

### Coorte de referencia

Para `Automation Systems Analyst + ENGENHARIA`, excluindo contas piloto em `07.Outros`, foram considerados 5 usuarios de referencia:
- `O365_BUSINESS_PREMIUM`: 5/5, atribuicao direta;
- `FLOW_FREE`: 4/5, atribuicao direta;
- `POWER_BI_STANDARD`: 1/5, atribuicao direta.

Todos os 5 usuarios de referencia possuem `UsageLocation=BR` e nenhum `disabledPlan` no Business Standard.

### Usuario piloto Lucas Costa

Usuario sincronizado:
- `lucas.costa@automind.com.br`;
- `onPremisesSyncEnabled=True`;
- inicialmente `UsageLocation` vazio e 0 licencas.

Teste controlado:
1. `usageLocation` atualizado para `BR` via Graph e confirmado por releitura;
2. Microsoft 365 Business Standard (`f245ecc8-75af-4f8e-b61f-27d8114de5f3`) atribuida diretamente;
3. readback confirmou `state=Active`, `error=None`, `assignedByGroup` vazio;
4. portal Microsoft 365 mostrou 16 de 166 disponiveis durante o teste;
5. a mesma licenca foi removida via `assignLicense`;
6. readback final confirmou `Licencas=0`, Business Standard ausente;
7. inventario voltou para 149 consumidas e 17 disponiveis.

Estado final do usuario piloto apos o rollback da licenca:
- `UsageLocation=BR`;
- 0 licencas;
- nenhuma Business Standard atribuida.

## Problema de PowerShell identificado

Uma sessao do Windows PowerShell possuia `ServerCertificateValidationCallback` customizado, causando:

`There is no Runspace available to run scripts in this thread.`

Para os testes manuais, o callback foi removido apenas na sessao e TLS 1.2 foi usado. Isso foi temporario.

**A aplicacao nao implementa bypass de validacao TLS.** O servico .NET usa validacao TLS/certificado padrao do sistema operacional.

## Implementacao v0.1.12 - inventario visual

Novos componentes:
- `Models/Microsoft365LicenseModels.cs`;
- `Services/IMicrosoft365LicenseService.cs`;
- `Services/Microsoft365LicenseService.cs`.

Fluxo:
1. o App Pool executa como `AUTOMIND\\gMSA_CadColab$`;
2. a aplicacao localiza o certificado pelo thumbprint em `LocalMachine\\My`;
3. gera client assertion RSA SHA-256;
4. solicita token App-only ao Entra;
5. consulta `GET /v1.0/subscribedSkus`;
6. calcula `disponiveis = enabled - consumed`;
7. exibe nome, SKU, quantidade e status no formulario;
8. cache local de 5 minutos reduz chamadas ao Graph.

A tela apresenta quantidades no formato do Microsoft 365 Admin Center, por exemplo:

`Microsoft 365 Business Standard`  
`17 de 166 licencas disponiveis`

SKUs com capacidade equivalente a ilimitada sao apresentados como `Licencas ilimitadas disponiveis`. Assinaturas suspensas ficam identificadas como suspensas.

## Configuracao v0.1.12

```json
"Microsoft365": {
  "Enabled": true,
  "LicenseInventoryEnabled": true,
  "LicenseWritesEnabled": false,
  "TenantId": "9ab05ca8-1779-410b-ae61-82dbd20810f3",
  "ClientId": "3558d29a-1098-467c-b4e3-aeaf0afeaaf4",
  "CertificateThumbprint": "A38B594A4A2594A3D83B33B52FD7828935700C29",
  "InventoryCacheMinutes": 5
}
```

Nenhum segredo e armazenado no `appsettings.json`.

## Teste esperado apos deploy da v0.1.12

No servidor `10.1.2.21`:
1. publicar normalmente pelo Azure DevOps;
2. abrir `http://cadastro.automind.com.br/`;
3. acessar `Novo colaborador`;
4. localizar a secao `04 - MICROSOFT 365`;
5. confirmar `Graph conectado`;
6. confirmar que `Microsoft 365 Business Standard` mostra **17 de 166 licencas disponiveis** enquanto o tenant permanecer no mesmo estado;
7. confirmar que nenhum checkbox de licenca pode ser marcado;
8. confirmar que a pre-validacao/criacao AD continua independente da consulta M365.

Se a lista falhar, o formulario continua carregando e mostra erro somente na secao Microsoft 365.

# Rollback

## Rollback da funcionalidade v0.1.12

Forma mais simples e sem alterar Entra:

`Automind:Microsoft365:Enabled=false`

ou:

`Automind:Microsoft365:LicenseInventoryEnabled=false`

Depois publicar a configuracao. Efeito: o formulario deixa de consultar/exibir inventario M365; AD permanece inalterado.

## Contencao imediata da autenticacao Graph

No Entra, remover somente o certificado com thumbprint:

`A38B594A4A2594A3D83B33B52FD7828935700C29`

Isso impede novos tokens App-only sem alterar usuarios/licencas.

## Rollback de permissoes Graph

Remover/revogar individualmente, validando entre etapas:
- `LicenseAssignment.ReadWrite.All`;
- `User.ReadUpdate.All`;
- `User.Read.All`;
- `LicenseAssignment.Read.All`.

Nao remover permissoes de outros aplicativos.

## Rollback da ACE da chave privada

No servidor `10.1.2.21`, remover somente a ACE criada para a gMSA:

```powershell
$key="C:\ProgramData\Microsoft\Crypto\Keys\3f4b61607e15b0dae42c2006d96b04b8_54f5abd6-87df-4327-bf2f-02580f1eda0a";icacls $key /remove:g 'AUTOMIND\gMSA_CadColab$'
```

Validar que `SYSTEM` e `BUILTIN\\Administrators` permanecem intactos.

## Rollback do certificado local

Somente apos remover/revogar o uso do certificado no Entra:

```powershell
Remove-Item "Cert:\LocalMachine\My\A38B594A4A2594A3D83B33B52FD7828935700C29"
```

Remover o `.cer` publico local e opcional depois da validacao.

## Rollback integral da App Registration

Ultimo recurso: excluir somente `Automind.CadColab` no tenant. Antes disso registrar IDs, permissoes e certificado. Essa acao remove a identidade criada para esta integracao e deve ser tratada como alteracao real separada.

## Regra de rollback

Toda reversao deve ser executada uma alteracao por vez, com validacao entre etapas. Nao restaurar configuracoes/ACLs inteiras de forma cega e nao alterar usuarios/licencas de producao para desfazer a integracao.
