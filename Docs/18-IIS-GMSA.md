# IIS - gMSA do CadastroColaboradores

## Objetivo

Executar o App Pool `CadastroColaboradores` com a identidade tecnica `AUTOMIND\gMSA_CadColab$`, mantendo autorizacao humana separada e sem usar credenciais administrativas pessoais.

## Estado validado antes da troca

Servidor: `SV052022-6121` / `10.1.2.21`.

App Pool `CadastroColaboradores`:

- Estado: `Started`;
- identidade efetiva: `ApplicationPoolIdentity`;
- `LogonType=LogonBatch`;
- `manualGroupMembership=False`;
- `LoadUserProfile=True`;
- `SetProfileEnvironment=True`;
- `StartMode=OnDemand`;
- pipeline `Integrated`;
- autenticacao anonima: habilitada;
- usuario anonimo: `IUSR`;
- autenticacao Windows: desabilitada;
- binding: `http 10.1.2.21:80:cadastro.automind.com.br`.

O campo residual `UserName=AUTOMIND\CriaMovePastas` nao e a identidade efetiva enquanto `IdentityType=ApplicationPoolIdentity`.

## gMSA

- conta: `AUTOMIND\gMSA_CadColab$`;
- `Test-ADServiceAccount=True` no servidor;
- membro do grupo AD `SG_CadastroColaboradores_AD_Writer`;
- delegacao piloto do grupo tecnico aplicada somente em `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- nenhuma ACL NTFS especifica adicionada preventivamente para a gMSA.

## IIS_IUSRS e LogonBatch

A politica local exportada apresentou:

- `SeBatchLogonRight` incluindo `S-1-5-32-568` (`IIS_IUSRS`);
- `SeDenyBatchLogonRight` nao configurado.

O App Pool esta com `manualGroupMembership=False`. Portanto, nao sera adicionada associacao local explicita da gMSA ao `IIS_IUSRS` nem alterado direito local antes de necessidade comprovada. O IIS usara seu comportamento padrao de adicionar o SID `IIS_IUSRS` ao token do worker; o inicio real do worker sera a validacao definitiva.

## Politica de alteracao

Antes de trocar a identidade:

1. criar backup local do IIS;
2. registrar baseline nao secreto do App Pool;
3. preparar comando de mudanca;
4. preparar rollback primario restrito ao App Pool;
5. definir validacao de worker, owner do processo e HTTP local;
6. obter autorizacao explicita;
7. alterar somente o pool `CadastroColaboradores`;
8. nao usar `iisreset`.

O backup integral do IIS e apenas contingencia. Nao deve ser restaurado automaticamente se houver possibilidade de alteracoes legitimas simultaneas em outros sites/pools.

## Backup pre-gMSA confirmado em 24/09/2026

Antes de qualquer mudanca real na identidade do App Pool, foi criado o backup local:

- `CadColab-Pre-gMSA-20260924-082229`;
- `C:\Windows\System32\inetsrv\backup\CadColab-Pre-gMSA-20260924-082229\applicationHost.config`;
- SHA-256: `B46EE9FEA44440598CB5B3014A631E3FD89E78D4358FCF22DA90385853511840`.

Baseline nao secreto do pool:

- `C:\Temp\CadastroColaboradores-Rollback\IIS-CadastroColaboradores-before-20260924-082229.txt`;
- SHA-256: `CD51C31F6CE9B87B1E09B00C5B5D48BAE2756CFB07946777FC3DE0755F8EE45B`.

A criacao desses arquivos nao alterou a identidade do App Pool e nao executou recycle, restart ou `iisreset`.

## 24/09/2026 - identidade do App Pool alterada para gMSA

A identidade do App Pool `CadastroColaboradores` foi alterada de `ApplicationPoolIdentity` para `SpecificUser` usando `AUTOMIND\gMSA_CadColab$`.

Validacao imediatamente apos a alteracao:

- `State=Started`;
- `IdentityType=SpecificUser`;
- `UserName=AUTOMIND\gMSA_CadColab$`;
- `LogonType=LogonBatch`;
- `Test-ADServiceAccount gMSA_CadColab=True`;
- a requisicao de teste para `http://127.0.0.1/` com `Host=cadastro.automind.com.br` retornou HTTP 404;
- nenhum worker foi encontrado nessa tentativa;
- nenhum evento WAS/W3SVC recente foi retornado.

O HTTP 404 e a ausencia de worker **nao foram tratados como falha da gMSA**, porque o binding real do site esta restrito a `10.1.2.21:80:cadastro.automind.com.br`; a requisicao foi enviada ao endereco de loopback `127.0.0.1`, que nao corresponde ao IP configurado no binding. A validacao funcional deve ser repetida contra `10.1.2.21` (ou pelo FQDN), preservando o Host correto, antes de decidir por rollback.

Enquanto essa validacao nao for concluida:

- nao alterar ACL NTFS;
- nao alterar direitos locais;
- nao executar `iisreset`;
- nao restaurar o backup integral do IIS;
- manter preparado o rollback restrito ao App Pool para `ApplicationPoolIdentity` se a ativacao real do worker falhar.

## 24/09/2026 - validacao funcional concluida com gMSA

A validacao consolidada posterior confirmou que a troca do App Pool foi bem-sucedida e funcional:

- App Pool `CadastroColaboradores`: `Started`;
- `IdentityType=SpecificUser`;
- `UserName=AUTOMIND\gMSA_CadColab$`;
- `Test-ADServiceAccount gMSA_CadColab=True`;
- site `CadastroColaboradores`: `Started`;
- binding: `http/10.1.2.21:80:cadastro.automind.com.br`;
- aplicacao vinculada ao pool: `CadastroColaboradores/`;
- requisicao via `10.1.2.21` com Host correto: HTTP `200`;
- requisicao via FQDN `cadastro.automind.com.br`: HTTP `200`;
- worker criado: PID `5820` no momento do teste;
- owner real do worker: `AUTOMIND\gMSA_CadColab$`;
- evento `IIS AspNetCore Module V2`, ID `1032`, informou que `C:\Automind.CadastroColaboradores\` iniciou com sucesso;
- nenhum erro WAS/W3SVC ou IIS/.NET foi retornado na janela consultada.

Conclusao operacional: a identidade tecnica gMSA esta ativa no App Pool e o site inicia e responde corretamente sob essa identidade. Nao adicionar ACL NTFS, direitos locais ou membros em `IIS_IUSRS` sem necessidade comprovada.

O rollback para `ApplicationPoolIdentity` e o backup `CadColab-Pre-gMSA-20260924-082229` permanecem documentados como contingencia, mas nao devem ser executados no estado atual.

## 24/09/2026 - estado da aplicacao apos validacao da gMSA

Apos a validacao funcional do App Pool com `AUTOMIND\\gMSA_CadColab$`, o `appsettings.json` implantado foi lido sem alteracao e apresentou:

- `Automind:Mode=ReadOnly`;
- `Automind:Ad:Server=10.1.2.1`;
- `Automind:Ad:PeopleSearchBase=OU=Automind,DC=automind,DC=com,DC=br`;
- `Automind:Ad:AuthorizedGroup=_informatica`;
- `22` OUs na allowlist;
- `07.Outros` ausente da allowlist;
- `Automind:Microsoft365:Enabled=false`;
- SHA-256 do arquivo implantado: `9C6DB1F3CE04EB96C99157DA8E2A99CAC0808B83448F32D933507E315DAB441A`.

O pool permaneceu `Started`, `SpecificUser`, com a gMSA valida e HTTP `200`.

Conclusao: a mudanca de identidade do IIS nao alterou o modo funcional da aplicacao. A aplicacao continua bloqueada para escrita e nenhuma ACL NTFS adicional foi necessaria.

## 24/09/2026 - relacao com a implementacao de escrita

O novo codigo de provisionamento exige, antes de escrever, que a identidade real do processo seja exatamente `AUTOMIND\gMSA_CadColab$`. Assim, uma publicacao executada sob outra identidade permanece bloqueada pelo proprio servico de escrita.

A configuracao entregue nesta etapa continua `Automind:Mode=ReadOnly`; portanto, a existencia da gMSA funcional no App Pool nao ativa por si so nenhuma escrita AD.
