# CadColab - contexto atual para continuidade

Versao: `0.1.12`  
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
- `Microsoft365.LicenseWritesEnabled=false`;
- Teams, `proxyAddresses` e `pwdLastSet` continuam sem escrita.

## Active Directory - piloto concluido

Teste completo `I2609-0305`:
- `lucas.costa` criado em `07.Outros`;
- atributos, senha, UPN/mail, manager e enable validados;
- membership direta `_CriaMovePastas` validada;
- auditoria completa ate `provisioning-complete`;
- teste negativo com `_Engenharia` fora da allowlist bloqueou a escrita.

Nao ampliar OU/grupos de producao sem nova decisao explicita.

## Microsoft 365 / Entra / Graph - validado

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

### Teste real de licenca concluido

Usuario piloto `lucas.costa@automind.com.br`:
- `UsageLocation` alterado de vazio para `BR`;
- Microsoft 365 Business Standard atribuida diretamente;
- readback confirmou `Active`, sem erro e sem grupo de origem;
- licenca removida em seguida;
- estado final: `UsageLocation=BR`, 0 licencas;
- tenant voltou a 149 Business Standard consumidas / 17 disponiveis.

## Implementacao v0.1.12

A tela `Novo colaborador` passa a mostrar uma secao Microsoft 365 somente leitura com:
- nome amigavel da licenca;
- SKU tecnico;
- quantidade disponivel e total no formato `17 de 166 licencas disponiveis`;
- status `Disponivel`, `Sem vagas` ou `Suspensa`;
- capacidade equivalente a ilimitada apresentada como ilimitada.

A fonte e `GET /v1.0/subscribedSkus` via App-only/certificado. Cache: 5 minutos.

**Nao existe atribuicao/remocao de licenca no codigo da v0.1.12.** Os checkboxes aparecem desabilitados e `LicenseWritesEnabled=false`.

## Proximo teste

Depois do build/deploy da v0.1.12 no servidor `10.1.2.21`:
1. abrir `Novo colaborador`;
2. confirmar secao `04 - MICROSOFT 365`;
3. confirmar `Graph conectado`;
4. conferir os numeros com o Microsoft 365 Admin Center;
5. confirmar que os checkboxes estao desabilitados;
6. confirmar que importacao TOPdesk e fluxo AD continuam normais.

## Rollback rapido da v0.1.12

Definir um destes valores e publicar:
- `Automind:Microsoft365:Enabled=false`; ou
- `Automind:Microsoft365:LicenseInventoryEnabled=false`.

Rollback completo do Entra/certificado/ACE: `Docs/07-MICROSOFT-365.md`.

## Regras permanentes

- `Docs/CHECKPOINT.md` e o unico checkpoint cumulativo, mais novo primeiro;
- documentos tematicos evoluem no proprio arquivo;
- cada alteracao real deve ser feita uma por vez, com efeito, rollback e validacao;
- nao usar `iisreset` nem reiniciar DC/AD/servidor;
- nao alterar comportamento de exibicao da senha em PDF/impressao sem novo pedido;
- `proxyAddresses` continua fora da escrita do aplicativo.
