# Rollback do Active Directory - Automind.CadastroColaboradores

## Objetivo

Definir como desfazer, de forma controlada, cada alteracao feita pelo projeto no Active Directory e nos componentes diretamente ligados a identidade tecnica.

Este documento e obrigatorio antes de novas escritas no AD.

## 1. Regras de rollback

1. Executar rollback somente com autorizacao explicita.
2. Reverter em ordem inversa das dependencias.
3. Remover somente o que foi criado ou alterado pelo projeto.
4. Nunca restaurar uma ACL completa sobre uma OU ou grupo sem comparar o estado atual.
5. Nunca reiniciar DC, AD DS, KDC, Netlogon ou o servidor como procedimento de rollback.
6. Nunca usar `iisreset` como rollback do projeto.
7. Parar ao primeiro resultado diferente do esperado.
8. Registrar comando, horario, operador, resultado e validacao.

## 2. Estado atual documentado - 23/09/2026

### Criado no AD

- `gMSA_CadColab$`
  - DN: `CN=gMSA_CadColab,CN=Managed Service Accounts,DC=automind,DC=com,DC=br`
  - host autorizado: `SV052022-6121$`
- `SG_CadastroColaboradores_AD_Writer`
  - DN: `CN=SG_CadastroColaboradores_AD_Writer,OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br`
  - tipo: `Global / Security`
  - membro atual: `gMSA_CadColab$`

### Preparado no servidor `10.1.2.21`

- `gMSA_CadColab` instalada localmente;
- `Test-ADServiceAccount gMSA_CadColab = True`.

### Ainda nao executado

- nenhuma delegacao de OU;
- nenhuma delegacao `Write Members` em grupos;
- App Pool ainda nao usa a gMSA;
- nenhuma escrita de usuario habilitada na aplicacao.

## 3. Rollback da associacao gMSA -> grupo tecnico

Mudanca futura:

```powershell
Add-ADGroupMember -Identity "SG_CadastroColaboradores_AD_Writer" -Members "gMSA_CadColab$"
```

Rollback exato:

```powershell
Remove-ADGroupMember -Identity "SG_CadastroColaboradores_AD_Writer" -Members "gMSA_CadColab$" -Confirm:$false
```

Validacao:

```powershell
Get-ADGroupMember "SG_CadastroColaboradores_AD_Writer"
```

A gMSA nao deve aparecer como membro.

## 4. Rollback do grupo tecnico

Objeto:

`SG_CadastroColaboradores_AD_Writer`

Pre-condicoes para remover:

- grupo sem membros;
- nenhuma ACE do AD referenciando o grupo criada pelo projeto;
- IIS/aplicacao nao dependem diretamente do grupo.

Rollback:

```powershell
Remove-ADGroup -Identity "SG_CadastroColaboradores_AD_Writer"
```

Validacao:

```powershell
Get-ADGroup -Identity "SG_CadastroColaboradores_AD_Writer" -ErrorAction SilentlyContinue
```

Resultado esperado: nenhum objeto retornado.

Nao remover o grupo enquanto houver delegacoes vinculadas a ele.

## 5. Rollback da instalacao local da gMSA

Local: servidor `10.1.2.21`.

Somente executar depois que nenhum App Pool, servico ou tarefa estiver usando a gMSA.

Rollback:

```powershell
Uninstall-ADServiceAccount -Identity "gMSA_CadColab"
```

Essa operacao remove a preparacao local da conta no servidor. O objeto da gMSA continua existindo no AD.


## 5.1. Estado atual da associacao gMSA -> grupo tecnico

A associacao abaixo ja existe:

`gMSA_CadColab$` -> `SG_CadastroColaboradores_AD_Writer`

Comando de rollback desta associacao:

```powershell
$svc=Get-ADServiceAccount "gMSA_CadColab";Remove-ADGroupMember -Identity "SG_CadastroColaboradores_AD_Writer" -Members $svc -Confirm:$false
```

Validacao apos rollback:

```powershell
Get-ADGroupMember "SG_CadastroColaboradores_AD_Writer";Get-ADServiceAccount "gMSA_CadColab" -Properties MemberOf | Select-Object Name,SamAccountName,@{N='MemberOf';E={$_.MemberOf -join '; '}} | Format-List
```

Resultado esperado: grupo tecnico sem a gMSA e `MemberOf` sem `SG_CadastroColaboradores_AD_Writer`.

Nao remover a associacao como rollback de outra etapa se ja existirem delegacoes que dependam desse grupo sem antes remover essas delegacoes.

## 6. Rollback do objeto gMSA no AD

Objeto:

`gMSA_CadColab$`

Somente remover depois de confirmar:

1. App Pool nao usa a gMSA;
2. nenhuma tarefa/servico usa a gMSA;
3. gMSA removida do grupo tecnico;
4. todas as delegacoes relacionadas foram removidas;
5. instalacao local removida do `10.1.2.21`.

Rollback:

```powershell
Remove-ADServiceAccount -Identity "gMSA_CadColab"
```

Validacao:

```powershell
Get-ADServiceAccount -Identity "gMSA_CadColab" -ErrorAction SilentlyContinue
```

Resultado esperado: nenhum objeto retornado.

## 7. Rollback de delegacoes em OUs

Para cada OU, a futura delegacao deve registrar antes da escrita:

- DN da OU;
- principal que recebera a permissao;
- `ActiveDirectoryRights`;
- `ObjectType`;
- `InheritedObjectType`;
- `InheritanceType`;
- ACE criada pelo projeto.

Regra de rollback:

**remover somente a ACE adicionada pelo projeto**.

Nao usar restauracao cega da ACL inteira com `Set-Acl`, pois isso pode apagar alteracoes feitas por outros administradores depois do baseline.

A etapa de delegacao fica bloqueada enquanto o comando de inclusao da ACE e seu comando de remocao especifica nao estiverem documentados lado a lado.



## 7.1. Delegacao piloto da OU Engenharia - rollback planejado

A delegacao ainda nao foi aplicada. A simulacao em memoria construiu `16` ACEs e confirmou que a ACL real permaneceu inalterada.

Conjunto previsto:

- 1 `CreateChild` para classe `user`;
- 14 `WriteProperty` dos atributos aprovados;
- 1 `Reset Password`;
- principal: `AUTOMIND\SG_CadastroColaboradores_AD_Writer`;
- alvo: `OU=Engenharia,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`.

Rollback obrigatorio da futura aplicacao:

1. reler a ACL atual da OU;
2. localizar somente as ACEs explicitas cujo principal, direito, `ObjectType`, `InheritedObjectType` e `InheritanceType` correspondam ao conjunto criado pelo projeto;
3. remover somente essas ACEs com `RemoveAccessRuleSpecific`;
4. executar um unico `Set-Acl`;
5. reler a ACL e confirmar ausencia das ACEs do projeto;
6. nao restaurar cegamente uma ACL completa salva anteriormente.

Antes do primeiro `Set-Acl`, o script exato de inclusao e o script exato de remocao devem ser simulados em memoria e produzir contagens reversiveis.

## 8. Rollback de permissao de grupos (`Write Members`)

A permissao para alterar membros de grupos sera delegada separadamente.

Para cada grupo aprovado, registrar:

- DN do grupo;
- ACL antes;
- ACE exata adicionada;
- ACL depois.

Rollback:

- remover somente a ACE de escrita em `member` criada pelo projeto;
- nao remover memberships existentes de usuarios;
- nao alterar outras ACEs do grupo.

A etapa fica bloqueada ate existir rollback especifico para cada ACE aplicada.

## 9. Rollback da identidade do App Pool

Ainda nao houve alteracao de identidade do App Pool.

Antes dessa mudanca, registrar:

- configuracao atual completa do pool;
- identidade atual: `ApplicationPoolIdentity`;
- ACL local necessaria;
- comando exato de mudanca;
- comando exato para voltar a `ApplicationPoolIdentity`;
- necessidade de recycle somente do pool.

Nao usar `iisreset`.

Nao remover/desinstalar a gMSA enquanto o App Pool estiver configurado para usa-la.

## 10. Rollback de usuario ficticio criado pelo projeto

Para o piloto, se um usuario ficticio for criado:

1. manter/desabilitar a conta;
2. registrar DN e grupos adicionados;
3. remover somente os grupos adicionados pelo projeto;
4. validar que nenhuma dependencia foi criada;
5. excluir o usuario somente com autorizacao explicita separada.

Desabilitar:

```powershell
Disable-ADAccount -Identity "<sAMAccountName>"
```

Exclusao de usuario ficticio, somente apos autorizacao:

```powershell
Remove-ADUser -Identity "<sAMAccountName>" -Confirm:$true
```

Para usuario real, nao existe exclusao automatica como rollback. A regra e desabilitar, parar e revisar.

## 11. Rollback de alteracoes em usuario existente

Se no futuro o sistema alterar um usuario existente, antes de qualquer `Set-ADUser` devem ser salvos os valores anteriores dos atributos que serao alterados.

Rollback = restaurar somente esses valores anteriores.

Nunca usar `-Clear` de forma generica para tentar voltar ao estado anterior.

## 12. Ordem de rollback completo do projeto

Se for necessario voltar integralmente ao estado anterior ao provisionamento, usar esta ordem:

1. bloquear escrita na aplicacao;
2. restaurar identidade anterior do App Pool, caso ja tenha sido alterada;
3. remover memberships de teste criadas pelo sistema;
4. desabilitar usuario ficticio e, se autorizado, remove-lo;
5. remover ACEs de `Write Members` criadas pelo projeto;
6. remover ACEs de delegacao das OUs criadas pelo projeto;
7. remover `gMSA_CadColab$` do `SG_CadastroColaboradores_AD_Writer`;
8. confirmar grupo tecnico sem dependencias e remove-lo;
9. desinstalar `gMSA_CadColab` do `SV052022-6121`;
10. confirmar gMSA sem dependencias e remover o objeto do AD;
11. validar replicacao e funcionamento normal por leitura;
12. registrar o encerramento do rollback.

## 13. Objetos que este rollback NAO pode alterar

- KDS Root Key existente;
- `pGMSA_c5fa29e8$`;
- `ADSyncMSA82bf3$`;
- `_informatica`;
- `CriaMovePastas` e seus grupos;
- ACEs preexistentes no dominio/OUs;
- grupos administrativos;
- GPO;
- DNS;
- Schema;
- configuracao dos DCs.

## 14. Regra para novas mudancas

Toda nova escrita no AD deve ser adicionada a este documento antes da execucao com quatro itens:

1. comando de mudanca;
2. efeito esperado;
3. comando de rollback;
4. validacao do rollback.

Sem esses quatro itens, a mudanca nao deve ser executada.

## 15. Validacao do rollback da delegacao da OU Engenharia

Em 23/09/2026 foi testado o ciclo completo somente em memoria para a OU:

`OU=Engenharia,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`

Resultado:

- ACL inicial: `40` ACEs;
- apos adicionar as 16 regras ao objeto ACL em memoria: `56` ACEs;
- apos `RemoveAccessRuleSpecific` das mesmas 16 regras: `40` ACEs;
- ACL real no AD: `40` ACEs;
- nenhum `Set-Acl` foi executado.

Conclusao: o rollback logico das 16 ACEs foi validado antes de qualquer delegacao real.

Antes da futura escrita real, salvar tambem um baseline da ACL/SDDL da OU em arquivo local no DC. Esse arquivo e evidencia de recuperacao; sua criacao nao altera o AD.

## 16. Baseline da OU piloto 07.Outros

Antes da primeira delegacao real foi salvo o baseline de:

`OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

Estado registrado:

- `40` ACEs;
- owner `AUTOMIND\Domain Admins`;
- heranca ativa.

Arquivos locais:

- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-ACL-before.xml`
- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-SDDL-before.txt`

O rollback preferencial permanece granular: remover somente as ACEs adicionadas pelo CadastroColaboradores. Os arquivos de baseline servem para comparacao, validacao e recuperacao controlada; nao devem ser aplicados cegamente sobre a ACL atual.

## 17. Validacao da delegacao piloto em 07.Outros

Antes de qualquer escrita real, o mesmo conjunto de 16 ACEs foi testado somente em memoria na OU:

`OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

Resultado:

- ACL inicial: `40`;
- ACL simulada: `56`;
- rollback em memoria: `40`;
- ACL real: `40`.

Baseline local disponivel:

- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-ACL-before.xml`
- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-SDDL-before.txt`

Hashes SHA-256:

- XML: `2F8C1F0AB538CAEFAD17F3CC59631D87F741860270DD1F11B113AD66EB25106D`
- SDDL: `53FB30862FDB19A069F25BC7C182949315AA00D85CAD229D0EEC61931CFC64B3`

Se a delegacao piloto real for executada, o rollback preferencial deve remover somente as 16 ACEs adicionadas pelo projeto, usando `RemoveAccessRuleSpecific` e `Set-Acl`. O baseline deve ser usado para comparacao e recuperacao controlada, nao para sobrescrever cegamente a ACL.

## 18. Rollback exato da ACL piloto em 07.Outros

A primeira delegacao real autorizada esta limitada a:

`OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

Conjunto do projeto:

- 1 ACE `CreateChild` para a classe `user`;
- 14 ACEs `WriteProperty` para os atributos aprovados;
- 1 ACE `ExtendedRight` para `Reset Password` em usuarios descendentes.

Rollback preferencial:

1. reler a ACL atual;
2. reconstruir exatamente as mesmas 16 regras;
3. executar `RemoveAccessRuleSpecific` para cada regra;
4. executar um unico `Set-Acl`;
5. confirmar zero ACEs do grupo tecnico na OU;
6. confirmar owner e heranca preservados;
7. comparar com o baseline salvo sem sobrescrever outras mudancas.

Baseline local:

- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-ACL-before.xml`
- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-SDDL-before.txt`

Hashes do baseline:

- XML: `2F8C1F0AB538CAEFAD17F3CC59631D87F741860270DD1F11B113AD66EB25106D`
- SDDL: `53FB30862FDB19A069F25BC7C182949315AA00D85CAD229D0EEC61931CFC64B3`

A restauracao integral do SDDL salvo e medida de recuperacao excepcional. Nao usar automaticamente, pois pode remover alteracoes legitimas feitas por terceiros depois do baseline.

## 19. Estado pos-aplicacao da ACL piloto e regra de parada

A ACL piloto de `07.Outros` foi aplicada e a validacao estrutural retornou:

- `56` ACEs totais;
- `16` ACEs do grupo tecnico;
- owner `AUTOMIND\Domain Admins` preservado;
- heranca ativa;
- snapshot pos-escrita salvo localmente.

Hashes pos-escrita:

- XML: `5B2832851DA742D9AA4B3B768214282F694B41626BFC965ABE8F4B16E02C4584`
- SDDL: `31E8B76A144EFCB7CA83C8897651FB55FC3B1FDF362CCAA09CC3CC900BF95AC3`

A exibicao detalhada apresentou possivel divergencia entre as ACEs esperadas e as retornadas: `company` apareceu duas vezes e `givenName` nao apareceu. Antes de qualquer rollback ou correcao, confirmar essa condicao por consulta somente leitura que conte cada GUID esperado.

Nao executar restauracao integral do SDDL como primeira acao. Caso uma ACE divergente seja confirmada, preferir correcao granular e validar novamente o conjunto completo.


## 20. Fechamento da validacao detalhada da ACL piloto 07.Outros

Em 23/09/2026, uma consulta somente leitura contou cada ACE esperada do principal `AUTOMIND\SG_CadastroColaboradores_AD_Writer` na OU:

`OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

Resultado:

- `givenName`: 1;
- `sn`: 1;
- `displayName`: 1;
- `description`: 1;
- `physicalDeliveryOfficeName`: 1;
- `telephoneNumber`: 1;
- `mail`: 1;
- `title`: 1;
- `department`: 1;
- `company`: 1;
- `manager`: 1;
- `userPrincipalName`: 1;
- `sAMAccountName`: 1;
- `userAccountControl`: 1;
- `CreateChild User`: 1;
- `Reset Password`: 1;
- total de ACEs do grupo tecnico: 16.

Conclusao: a aparente duplicidade mostrada na primeira tabela nao existe na ACL real. Nenhuma ACE precisa ser corrigida e o rollback nao deve ser executado. O rollback granular da secao 18 permanece preparado para uso somente se uma reversao futura for explicitamente autorizada ou tecnicamente necessaria.

## 21. Estado do IIS antes da futura troca para gMSA

Em 23/09/2026, uma leitura parcial no servidor `10.1.2.21` confirmou que o App Pool `CadastroColaboradores` ainda esta configurado com `IdentityType=ApplicationPoolIdentity` e `LoadUserProfile=True`. O campo residual `UserName=AUTOMIND\\CriaMovePastas` nao deve ser tratado como identidade efetiva enquanto o tipo permanecer `ApplicationPoolIdentity`.

`Test-ADServiceAccount gMSA_CadColab` retornou `True`. Nenhum worker estava ativo no instante da leitura, compativel com `StartMode=OnDemand` sem requisicao ativa.

A tentativa de `Add-Type -AssemblyName Microsoft.Web.Administration` falhou. Portanto, antes de qualquer alteracao real do App Pool, o baseline definitivo deve ser obtido novamente por `WebAdministration`/`appcmd`, sem exibir senha.

Regra de rollback para a futura troca de identidade do App Pool permanece:

1. registrar configuracao nao secreta atual e identidade efetiva antes da mudanca;
2. registrar ACL NTFS necessaria para a nova identidade;
3. preparar comando exato que restaure `ApplicationPoolIdentity` e os demais valores alterados;
4. alterar somente o pool `CadastroColaboradores`;
5. validar inicializacao do worker e funcionamento da aplicacao;
6. em anomalia, restaurar somente os valores alterados e as ACLs criadas pelo projeto;
7. nunca usar `iisreset` como mecanismo de aplicacao ou rollback.


## 22. Baseline IIS e NTFS antes da troca para gMSA

Em 23/09/2026, no servidor `10.1.2.21`, foi confirmado por leitura que o App Pool `CadastroColaboradores` continua com `IdentityType=ApplicationPoolIdentity`, `LoadUserProfile=True`, `StartMode=OnDemand` e pipeline `Integrated`. O valor residual `UserName=AUTOMIND\\CriaMovePastas` nao representa a identidade efetiva enquanto o tipo permanecer `ApplicationPoolIdentity`.

A ACL de `C:\\Automind.CadastroColaboradores` foi inventariada. Nenhuma ACE foi criada para a gMSA. Antes de qualquer troca de identidade, deve-se confirmar se a gMSA obtem leitura/execucao pelas ACLs existentes e se a aplicacao precisa de alguma pasta gravavel.

Rollback futuro do IIS deve restaurar apenas os valores alterados no App Pool e remover somente ACEs NTFS que tenham sido adicionadas pelo projeto. Nao usar `iisreset`.

## 23. Pre-validacao adicional do IIS antes da troca para gMSA

No servidor `10.1.2.21`, a leitura confirmou que o App Pool `CadastroColaboradores` continua com `ApplicationPoolIdentity`, `LogonBatch`, `LoadUserProfile=True` e `SetProfileEnvironment=True`. Nenhuma troca de identidade foi feita.

A pasta da aplicacao continua sem ACE especifica para `gMSA_CadColab$`. Nenhuma ACL NTFS deve ser adicionada preventivamente sem necessidade comprovada.

Antes da futura alteracao do App Pool, ainda devem ser confirmados por leitura:

1. valores booleanos reais de autenticacao anonima e Windows;
2. `manualGroupMembership`;
3. composicao de `IIS_IUSRS`;
4. backup local do IIS e comando exato de rollback para `ApplicationPoolIdentity`.

A troca de identidade continua bloqueada ate esses itens estarem documentados.

## 24. Pre-requisitos IIS/gMSA antes da troca de identidade

Em 23/09/2026, foram confirmados no `SV052022-6121` / `10.1.2.21`:

- App Pool `CadastroColaboradores` com `ApplicationPoolIdentity`;
- `LogonType=LogonBatch`;
- `manualGroupMembership=False`;
- `LoadUserProfile=True`;
- autenticacao anonima habilitada com `IUSR`;
- autenticacao Windows desabilitada;
- `SeBatchLogonRight` contendo o SID `S-1-5-32-568` (`IIS_IUSRS`);
- `SeDenyBatchLogonRight` nao configurado;
- `Test-ADServiceAccount gMSA_CadColab=True`.

Decisao de menor privilegio: nao adicionar a gMSA explicitamente ao grupo local `IIS_IUSRS` e nao alterar politica de direitos de usuario antes de necessidade comprovada. `manualGroupMembership=False` permite ao IIS adicionar o SID de `IIS_IUSRS` ao token do worker. O comportamento real sera validado pela inicializacao controlada do App Pool depois da futura troca.

Antes de qualquer alteracao de identidade do App Pool, criar backup local do IIS e um baseline nao secreto. O backup integral do IIS e contingencia, nao rollback primario. O rollback primario da futura troca deve restaurar somente o App Pool `CadastroColaboradores` para `ApplicationPoolIdentity` e os valores explicitamente alterados. `iisreset` permanece proibido.

## 25. Estado de seguranca antes de implementar escrita na aplicacao

Em 24/09/2026 foi confirmado que:

- App Pool usa `AUTOMIND\\gMSA_CadColab$` e esta funcional;
- `Automind:Mode=ReadOnly`;
- `07.Outros` ainda nao esta na allowlist do `appsettings.json`;
- M365 esta desabilitado;
- a linha atual do codigo nao possui servico/action de escrita AD.

Assim, nao existe neste momento rollback de usuario criado pela aplicacao, pois a aplicacao ainda nao tem caminho ativo de criacao.

Antes de publicar qualquer implementacao de escrita, registrar separadamente:

1. hash/backup do `appsettings.json` anterior;
2. arquivos de codigo alterados e commit correspondente;
3. valor anterior de `Automind:Mode`;
4. allowlist anterior e nova allowlist;
5. comando/processo de restauracao da publicacao anterior;
6. validacao HTTP e leitura AD apos rollback;
7. somente depois, roteiro de rollback de um usuario ficticio criado no piloto.

A ACL piloto de `07.Outros`, a gMSA, o grupo tecnico e o App Pool nao devem ser revertidos apenas porque a aplicacao continua `ReadOnly`; esse e o estado de seguranca esperado antes da implementacao de escrita.

## 24/09/2026 - rollback da implementacao de escrita preparada

A implementacao de escrita foi adicionada ao codigo, mas permanece bloqueada por `Automind:Mode=ReadOnly`. Portanto, nesta etapa nao existe novo objeto AD a desfazer.

Rollback de software antes do primeiro piloto:

1. manter/restaurar `Automind:Mode=ReadOnly`;
2. se o pacote novo apresentar problema, restaurar a publicacao anterior pelo fluxo Git/deploy aprovado;
3. nao remover a gMSA, o grupo tecnico ou as 16 ACEs de `07.Outros` apenas por rollback de codigo;
4. nao restaurar o backup IIS integral como primeira opcao;
5. validar HTTP e consultas AD apos qualquer rollback de publicacao.

Quando `PilotWrite` for futuramente ativado, a primeira acao de contencao da aplicacao sera voltar o modo para `ReadOnly`. Essa mudanca de configuracao deve ser tratada como alteracao real separada.

Se um usuario piloto for criado e uma etapa falhar, o codigo tenta manter a conta desabilitada. A conta nao e excluida automaticamente. Exclusao de usuario ficticio continua dependendo de autorizacao explicita; para usuario real, o procedimento e desabilitar, parar e revisar.
