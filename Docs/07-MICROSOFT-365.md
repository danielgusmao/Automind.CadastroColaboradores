# Microsoft 365 / Entra / Microsoft Graph

## Estado vigente - 25/09/2026 - v0.1.13

A integracao Microsoft 365 esta autenticada por App-only/certificado e teve leitura, `UsageLocation`, atribuicao e remocao de licenca validadas manualmente no tenant real.

A v0.1.12 foi publicada e validada no servidor com inventario somente leitura. A v0.1.13 habilita escrita piloto controlada no fluxo normal do cadastro.

Estado desta versao:
- leitura de inventario: **habilitada**;
- selecao de licenca: **habilitada somente para SKU disponivel**;
- atribuicao direta de licenca: **habilitada no piloto**;
- `LicenseWritesEnabled=true`;
- `UsageLocation=BR` como valor esperado para novo colaborador;
- polling Entra: 10 segundos, limite 180 segundos;
- rollback por operacao: remove somente licencas adicionadas pela tentativa.

## Identidade App-only

App Registration:
- nome: `Automind.CadColab`;
- tipo: single tenant;
- Application (client) ID: `3558d29a-1098-467c-b4e3-aeaf0afeaaf4`;
- Object ID: `1ed30afe-dfc7-45ea-bdd5-d6d17808091e`;
- Directory (tenant) ID: `9ab05ca8-1779-410b-ae61-82dbd20810f3`.

Certificado:
- Store: `LocalMachine\My` no servidor `10.1.2.21`;
- Subject: `CN=Automind.CadColab.Graph`;
- Thumbprint: `A38B594A4A2594A3D83B33B52FD7828935700C29`;
- validade ate `24/09/2028`;
- chave privada nao exportavel;
- certificado publico: `C:\Automind.CadastroColaboradores\Automind.CadColab.Graph.cer`.

A gMSA `AUTOMIND\gMSA_CadColab$` possui somente `Read, Synchronize` no arquivo CNG:

`C:\ProgramData\Microsoft\Crypto\Keys\3f4b61607e15b0dae42c2006d96b04b8_54f5abd6-87df-4327-bf2f-02580f1eda0a`

Nao exportar a chave privada e nao criar client secret.

## Permissoes Microsoft Graph atuais

Application + Admin Consent:
- `LicenseAssignment.Read.All`;
- `User.Read.All`;
- `User.ReadUpdate.All`;
- `LicenseAssignment.ReadWrite.All`.

Delegated existente:
- `User.Read` - nao utilizado no fluxo App-only.

Permissoes minimas confirmadas na documentacao Microsoft:
- `User.ReadUpdate.All` para update de propriedades comuns de usuario em Application;
- `LicenseAssignment.ReadWrite.All` para `POST /users/{id|UPN}/assignLicense` em Application.

## Validacoes reais anteriores

### subscribedSkus

`GET /v1.0/subscribedSkus` retornou 20 SKUs. Snapshot observado em 25/09/2026:
- Business Standard: 166 habilitadas / 149 consumidas / 17 disponiveis;
- Business Basic: 24 / 22 / 2;
- Office 365 E3: 6 / 5 / 1;
- Power BI Pro: 4 / 1 / 3;
- Project Plan 3: 22 / 16 / 6.

Esses numeros sao historicos. O sistema sempre deve usar o Graph atual.

### Padrao da coorte

`Automation Systems Analyst + ENGENHARIA`, excluindo `07.Outros`:
- 5 usuarios;
- `O365_BUSINESS_PREMIUM`: 5/5, direta;
- `FLOW_FREE`: 4/5;
- `POWER_BI_STANDARD`: 1/5;
- todos com `UsageLocation=BR`;
- Business Standard sem `disabledPlans`.

### Lucas Costa - teste controlado

`lucas.costa@automind.com.br`:
1. iniciou sincronizado, `UsageLocation` vazio, 0 licencas;
2. `UsageLocation=BR` aplicado via Graph e confirmado;
3. Business Standard (`f245ecc8-75af-4f8e-b61f-27d8114de5f3`) atribuida diretamente;
4. readback: `Active`, `None`, `assignedByGroup` vazio;
5. portal mostrou 16/166 disponiveis durante a atribuicao;
6. a mesma licenca foi removida;
7. estado final: 0 licencas, `UsageLocation=BR`, inventario voltou a 17/166 naquele momento.

## v0.1.12 - inventario visual validado

O formulario publicado exibiu corretamente:
- `Graph conectado`;
- nome amigavel;
- SKU;
- `<disponiveis> de <total> licencas disponiveis`;
- estados `Disponivel`, `Sem vagas`, `Suspensa`;
- checkboxes bloqueados, como previsto naquela versao.

## v0.1.13 - fluxo de escrita implementado

### Pre-validacao

`SelectedLicenseSkuIds` e enviado junto com a pre-validacao. O backend:
1. remove GUID vazio/duplicado;
2. consulta inventario fresco;
3. confirma que o SKU existe;
4. bloqueia `Suspended`;
5. bloqueia SKU sem vaga, salvo capacidade tratada como ilimitada;
6. adiciona o check `Licencas Microsoft 365 validas`.

### Criacao + sincronizacao

A criacao AD continua independente e segue o writer ja validado. Depois que `CreateUserAsync` retorna sucesso, o navegador chama `AplicarLicencasMicrosoft365`.

O endpoint M365 exige:
- confirmacao explicita;
- `LicenseWritesEnabled=true`;
- operador autorizado;
- UPN `@automind.com.br`;
- usuario localizado de forma unica no AD;
- DN do usuario dentro de `WriteAllowedOuDns`.

Se o usuario ainda nao existe no Entra, retorna `PendingSynchronization=true` e **nao altera UsageLocation nem licencas**.

A pagina repete a tentativa a cada `SyncPollSeconds=10` por ate `SyncMaxWaitSeconds=180`.

Referencia Microsoft: Entra Cloud Sync usa modelo agendado e provisiona mudancas aproximadamente a cada 2 minutos (`Cloud sync deep dive - how it works`).

### UsageLocation

Quando o usuario aparece no Entra:
- vazio -> PATCH para `BR` + readback;
- `BR` -> continua sem nova escrita;
- outro valor -> para e exige revisao; nao sobrescreve automaticamente.

`UsageLocation=BR` nao e revertido para vazio em rollback de licenca.

### assignLicense

Antes da escrita, o inventario e consultado novamente.

O servico calcula:
- licencas ja existentes;
- licencas realmente novas (`toAdd`).

Somente `toAdd` e enviado em `addLicenses`, com `disabledPlans=[]`.

Depois:
- cache do inventario e invalidado;
- readback confirma os SKUs;
- estado de licenca com erro bloqueia sucesso;
- UI marca licencas como atribuidas e ajusta a quantidade exibida localmente.

### Rollback automatico da operacao

Se houver falha depois que `assignLicense` foi tentado:
1. o sistema envia `removeLicenses` **somente para `toAdd`**;
2. rele o usuario;
3. confirma que os SKUs adicionados pela tentativa foram removidos;
4. se nao confirmar, `RequiresManualReview=true`.

Licencas que ja existiam antes da tentativa nunca sao removidas pelo rollback.

### Auditoria M365

Mesmo arquivo:

`C:\Automind.CadastroColaboradores\Logs\ProvisioningAudit.jsonl`

Acoes:
- `m365-license-start`;
- `m365-usage-location` quando aplicavel;
- `m365-license-assign`;
- `m365-license-readback`;
- `m365-license-rollback` quando necessario;
- `m365-license-complete`.

Campos novos suportados no JSONL:
- `UserPrincipalName`;
- `Licenses`.

Senha nunca e registrada.

## Configuracao v0.1.13

```json
"Microsoft365": {
  "Enabled": true,
  "LicenseInventoryEnabled": true,
  "LicenseWritesEnabled": true,
  "UsageLocation": "BR",
  "SyncPollSeconds": 10,
  "SyncMaxWaitSeconds": 180,
  "TenantId": "9ab05ca8-1779-410b-ae61-82dbd20810f3",
  "ClientId": "3558d29a-1098-467c-b4e3-aeaf0afeaaf4",
  "CertificateThumbprint": "A38B594A4A2594A3D83B33B52FD7828935700C29",
  "InventoryCacheMinutes": 5
}
```

## Teste apos deploy v0.1.13

Usar NOVO usuario piloto em `07.Outros`.

1. confirmar inventario Graph;
2. confirmar checkbox habilitado apenas em SKU disponivel;
3. selecionar Business Standard;
4. `Validar AD + M365` -> check M365 verde;
5. criar usuario;
6. observar status de espera do Entra;
7. confirmar atribuicao M365;
8. conferir portal: usuario licenciado e quantidade reduzida em 1;
9. conferir `UsageLocation=BR`;
10. conferir auditoria `m365-license-*`;
11. nao excluir usuario automaticamente depois do teste; seguir politica de contencao/desabilitacao antes de qualquer exclusao.

## Rollback

### Contencao funcional mais simples

Definir:

`Automind:Microsoft365:LicenseWritesEnabled=false`

Depois publicar. Resultado:
- inventario continua visivel;
- checkboxes ficam desabilitados;
- endpoint recusa escrita.

Para desligar tambem leitura:
- `Enabled=false`; ou
- `LicenseInventoryEnabled=false`.

### Rollback de uma tentativa de licenciamento

O servico ja tenta remover somente os SKUs que adicionou. Se `RequiresManualReview=true`, parar e conferir `assignedLicenses` antes de qualquer nova acao.

Nunca remover em massa todas as licencas do usuario para desfazer uma tentativa.

`UsageLocation=BR` permanece.

### Contencao da autenticacao Graph

No Entra, remover somente o certificado de thumbprint:

`A38B594A4A2594A3D83B33B52FD7828935700C29`

Novos tokens App-only deixam de funcionar.

### Rollback de permissoes Graph

Revogar/remover individualmente, uma por vez:
1. `LicenseAssignment.ReadWrite.All`;
2. `User.ReadUpdate.All`;
3. `User.Read.All`;
4. `LicenseAssignment.Read.All`.

Validar entre cada etapa. Nao alterar permissoes de outros apps.

### Rollback da ACE da chave privada

Servidor `10.1.2.21`:

```powershell
$key="C:\ProgramData\Microsoft\Crypto\Keys\3f4b61607e15b0dae42c2006d96b04b8_54f5abd6-87df-4327-bf2f-02580f1eda0a";icacls $key /remove:g 'AUTOMIND\gMSA_CadColab$'
```

Remover somente a ACE do CadColab.

### Rollback do certificado local

Somente apos conter o uso cloud:

```powershell
Remove-Item "Cert:\LocalMachine\My\A38B594A4A2594A3D83B33B52FD7828935700C29"
```

### Rollback integral da App Registration

Ultimo recurso: excluir somente `Automind.CadColab`, conferindo antes o Client ID `3558d29a-1098-467c-b4e3-aeaf0afeaaf4`.

## Regra

Toda reversao e toda ampliacao de escopo devem continuar uma alteracao por vez, com validacao entre etapas. Nao restaurar ACL inteira e nao alterar licencas nao criadas pela operacao corrente.
