# CadColab - contexto atual para continuidade

Versao: `0.1.13`  
Data: 25/09/2026  
Projeto: `Automind.CadastroColaboradores`

## Estado operacional

- `Automind:Mode=PilotWrite`;
- target `net10.0-windows`;
- App Pool `CadastroColaboradores` executa como `AUTOMIND\gMSA_CadColab$`;
- criacao real limitada a `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- `GroupWritesEnabled=true`;
- `GroupWriteAllowedDns` contem somente `_CriaMovePastas` em `07.Outros`;
- `Microsoft365.Enabled=true`;
- `Microsoft365.LicenseInventoryEnabled=true`;
- `Microsoft365.LicenseWritesEnabled=true`;
- `Microsoft365.UsageLocation=BR`;
- `SyncPollSeconds=10`;
- `SyncMaxWaitSeconds=180`;
- Teams, `proxyAddresses` e `pwdLastSet` continuam sem escrita.

## Active Directory - piloto concluido

Teste completo `I2609-0305`:
- `lucas.costa` criado em `07.Outros`;
- atributos, senha, UPN/mail, manager e enable validados;
- membership direta `_CriaMovePastas` validada;
- auditoria completa ate `provisioning-complete`;
- teste negativo com `_Engenharia` fora da allowlist bloqueou a escrita.

Nao ampliar OU/grupos de producao sem nova decisao explicita.

## Microsoft 365 / Entra / Graph

App Registration:
- nome `Automind.CadColab`;
- tenant `9ab05ca8-1779-410b-ae61-82dbd20810f3`;
- client ID `3558d29a-1098-467c-b4e3-aeaf0afeaaf4`;
- certificado `CN=Automind.CadColab.Graph`;
- thumbprint `A38B594A4A2594A3D83B33B52FD7828935700C29`;
- validade ate 24/09/2028;
- chave privada nao exportavel no servidor e legivel pela gMSA.

Permissoes Application com Admin Consent:
- `LicenseAssignment.Read.All`;
- `User.Read.All`;
- `User.ReadUpdate.All`;
- `LicenseAssignment.ReadWrite.All`.

### Teste manual real concluido

Usuario piloto `lucas.costa@automind.com.br`:
- `UsageLocation` alterado de vazio para `BR`;
- Microsoft 365 Business Standard atribuida diretamente;
- readback confirmou `Active`, sem erro e sem grupo de origem;
- licenca removida em seguida;
- estado final: `UsageLocation=BR`, 0 licencas;
- tenant voltou a 149 Business Standard consumidas / 17 disponiveis naquele snapshot.

### v0.1.12 validada no servidor

O PDF de validacao de 25/09/2026 confirmou:
- `Graph conectado`;
- lista real com nome/SKU/quantidade;
- Business Standard exibida como 17 de 166 disponiveis naquele momento;
- estados Disponivel, Sem vagas e Suspensa;
- escrita de licencas ainda bloqueada, como previsto para a v0.1.12.

## Implementacao v0.1.13

Novo fluxo M365 integrado ao cadastro piloto:
1. operador seleciona somente SKUs visualmente disponiveis;
2. `Validar AD + M365` tambem revalida os SKUs no Graph;
3. criacao AD ocorre pelo fluxo ja validado;
4. apos sucesso AD, o navegador chama automaticamente o endpoint M365;
5. enquanto o usuario nao existir no Entra, o endpoint retorna `PendingSynchronization` sem escrever licencas;
6. a tela repete a consulta a cada 10s por ate 180s;
7. quando o usuario aparece, o backend valida UPN exato e que ele pertence a `WriteAllowedOuDns`;
8. se `UsageLocation` estiver vazio, define `BR`; se ja for outro pais, para e nao sobrescreve;
9. revalida inventario/vagas;
10. atribui somente SKUs ainda ausentes no usuario;
11. readback confirma `assignedLicenses`/estado e tolera propagacao do Graph antes de declarar falha;
12. auditoria registra `m365-license-start`, `m365-usage-location`, `m365-license-assign`, `m365-license-readback`, `m365-license-complete`;
13. em falha apos atribuicao, tenta remover somente os SKUs adicionados nessa operacao e registra `m365-license-rollback`.

A operacao e idempotente para licencas ja atribuidas: elas nao entram no conjunto de rollback.

### Limite conhecido do piloto

Nao existe fila/background job nesta versao. O polling ocorre enquanto a tela permanece aberta. Se o usuario nao sincronizar em 180s, nenhuma licenca e aplicada e o botao `Repetir atribuicao M365` fica disponivel na mesma tela.

O Entra Cloud Sync corporativo identificado no ambiente trabalha por ciclos; a documentacao Microsoft informa que Cloud Sync provisiona mudancas aproximadamente a cada 2 minutos. Por isso o limite do piloto foi configurado em 180s.

## Proximo teste

Na v0.1.13:
1. build local deve retornar 0 erros/0 warnings;
2. publicar via branch `release`;
3. usar NOVO usuario piloto em `07.Outros`;
4. selecionar somente `_CriaMovePastas` se membership for necessaria;
5. selecionar `Microsoft 365 Business Standard` (ou outro SKU com vaga);
6. validar AD/M365;
7. criar usuario;
8. observar sincronizacao + atribuicao;
9. conferir portal M365 e `ProvisioningAudit.jsonl`;
10. parar no primeiro resultado inesperado.

## Rollback rapido da v0.1.13

Definir:

`Automind:Microsoft365:LicenseWritesEnabled=false`

e publicar. O inventario continua visivel, mas selecao/escrita ficam bloqueadas.

Rollback de uma tentativa e automatico somente para licencas adicionadas pela propria tentativa. `UsageLocation=BR` nao e revertido para vazio.

Rollback completo do Entra/certificado/ACE: `Docs/07-MICROSOFT-365.md`.

## Regras permanentes

- `Docs/CHECKPOINT.md` e o unico checkpoint cumulativo, mais novo primeiro;
- documentos tematicos evoluem no proprio arquivo;
- cada alteracao real deve ser feita uma por vez, com efeito, rollback e validacao;
- nao usar `iisreset` nem reiniciar DC/AD/servidor;
- nao alterar comportamento de exibicao da senha em PDF/impressao sem novo pedido;
- `proxyAddresses` continua fora da escrita do aplicativo.
