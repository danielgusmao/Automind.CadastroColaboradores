# CHECKPOINT - INDICE ATUAL

## Politica de versionamento

A partir de 2026-09-23, este arquivo deixa de acumular todo o historico em um unico documento.

- `Docs/CHECKPOINT.md` e apenas o indice/resumo atual.
- O historico fica em `Docs/CP-HIST.md`
- Versoes anteriores nao devem ser reescritas retroativamente; novas evidencias entram em uma nova versao.
- Isso reduz o tamanho de cada arquivo e facilita reutilizacao por outros agentes LLM.

## Regras operacionais obrigatorias

- Nao criar, editar ou alterar codigo, configuracoes, AD, TOPdesk, Microsoft 365, Teams, IIS ou servidores sem consentimento explicito do responsavel.
- Antes de qualquer alteracao: testar, consultar a documentacao do projeto e fontes oficiais, e documentar as evidencias.
- Nao especular causa antes dos testes.
- Solicitar no maximo 3 testes por rodada.
- Comandos PowerShell enviados ao responsavel devem ficar em uma unica linha, sem quebras.
- Nenhuma alteracao em servidor deve ser feita sem aviso previo.
- Manter contexto suficiente para continuidade por qualquer agente LLM.

## Indice de versoes

1. `Checkpoints/CHECKPOINT-V001-BASE-TOPDESK.md`
   - estado inicial do projeto;
   - starter existente;
   - integracao TOPdesk/Bridge;
   - regras iniciais e protocolo operacional.

2. `Checkpoints/CHECKPOINT-V002-AD-OU-IIS-IDENTIDADE.md`
   - listagem real de OUs;
   - servidor IIS `10.1.2.21` / `SV052022-6121`;
   - ApplicationPoolIdentity;
   - conta de maquina e ACLs;
   - investigacao KDS/gMSA e identidade tecnica.

3. `Checkpoints/CHECKPOINT-V003-AD-USUARIOS-GRUPOS.md`
   - leitura de usuarios/cargo/departamento/manager;
   - comparacao de grupos por cargo/departamento;
   - grupos diretos via `memberOf`;
   - investigacao do grupo `_Informatica`;
   - disponibilidade de sAMAccountName/UPN/mail/proxyAddresses;
   - regra SMTP `@automind.co` e script diario;
   - encerramento da Fase 1 de leitura do AD.

4. `Checkpoints/CHECKPOINT-V004-M365-DIAGNOSTICO.md`
   - conectividade Graph/Entra a partir de `10.1.2.21`;
   - inventario de PowerShell/.NET/PowerShellGet;
   - diagnostico HTTPS/TLS;
   - `curl.exe`/Schannel com HTTP 200;
   - falha restrita ao Windows PowerShell 5.1/.NET;
   - callback customizado de certificado detectado na sessao atual;
   - teste `-NoProfile` ainda inconclusivo por erro de parser.

5. `Checkpoints/CHECKPOINT-V005-M365-SESSAO-LIMPA.md`
   - nova sessao `powershell.exe -NoProfile` inicia com callback de certificado `NULL`;
   - endpoint OIDC Microsoft Entra retorna HTTP 200 na sessao limpa;
   - descartada necessidade de alterar TLS/SCHANNEL com as evidencias atuais;
   - convencao de nome dos ZIPs adotada: `Automind.CadastroColaboradores-AAAA-MM-DD-X.Y.Z.zip`;
   - primeiro pacote nessa convencao: `0.0.1`.


6. `Checkpoints/CHECKPOINT-V006-M365-TENANT-GRAPH.md`
   - Tenant ID confirmado via OIDC publico;
   - endpoint Graph `/subscribedSkus` respondeu HTTP 401 sem token, validando o caminho HTTPS para recurso protegido;
   - permissao minima documentada para futura consulta autenticada: `LicenseAssignment.Read.All`;
   - novo padrao para ZIP somente de checkpoint: `Automind.CadastroColaboradores-AAAA-MM-DD-checkpoint-vX.Y.Z.zip`.


7. `Checkpoints/CHECKPOINT-V007-AD-CRIACAO-OU.md`
   - Microsoft 365/Entra pausado;
   - politica de senha atual do dominio registrada;
   - amostra de usuarios recentes e atributos reais de referencia;
   - OU de destino passa a ser selecao explicita e separada de Department;
   - allowlist de OUs sera administravel e persistida por DistinguishedName;
   - proxima etapa: mapear OUs e uso real antes de qualquer escrita no AD.


8. `Checkpoints/CHECKPOINT-V008-AD-ALLOWLIST-OU.md`
   - levantamentos executados no AD com usuario elevado;
   - arvore real de OUs e quantidade de usuarios habilitados diretamente por OU registradas;
   - classificacao inicial entre candidatas, OUs a confirmar e OUs estruturais/tecnicas;
   - regra obrigatoria: todo teste deve indicar AD, 10.1.2.21 ou maquina do usuario;
   - estrategia futura aprovada para teste ponta a ponta com chamado TOPdesk ficticio + usuario AD ficticio, mantendo autorizacao explicita antes de cada escrita.


9. `Checkpoints/CHECKPOINT-V009-AD-CONTRATO-USUARIO-GRUPOS.md`
   - comparacao real entre colaborador interno e terceiro;
   - diferencas de OU, `.ext`, atributos, SMTP, senha e grupos registradas;
   - frequencia real de grupos da Engenharia registrada;
   - proibido clonar grupos integrais de um usuario como regra automatica;
   - pendencias: validar coorte de terceiros e excecoes dos 25 usuarios da Engenharia antes de fechar grupos base.


10. `Checkpoints/CHECKPOINT-V010-AD-PERFIS-GRUPOS-E-TESTE-PONTA-A-PONTA.md`
   - coorte completa de 15 usuarios habilitados de `05.Terceiros-Ext` analisada;
   - `.ext` confirmado como convencao forte atual, mas nao regra historica absoluta por OU;
   - diferenca 25 x 23 da Engenharia explicada pelas contas atipicas `e-clic` e `engeplan`;
   - perfil `Automation Systems Analyst + ENGENHARIA` validado com 5 usuarios e intersecao de 8 grupos;
   - primeiro teste ponta a ponta recomendado com colaborador interno ficticio desse perfil;
   - antes de qualquer escrita, validar metadados dos 8 grupos e disponibilidade da identidade ficticia.


11. `Checkpoints/CHECKPOINT-V011-AD-GRUPOS-TRANSITIVOS-E-IDENTIDADE-LIVRE.md`
   - metadados dos 8 grupos comuns do perfil validados;
   - nenhum dos 8 retornou `adminCount=1` ou `isCriticalSystemObject=True`;
   - `_Engenharia` esta aninhado em `_JumpServer`;
   - `_Tecnica SSA` esta aninhado em `_GAP2` e `_Tecnica`;
   - identidade ficticia `teste.provisionamento` validada sem colisoes;
   - antes da escrita, falta apenas mapear a cadeia transitiva dos grupos de seguranca e montar a pre-visualizacao final.


12. `Checkpoints/CHECKPOINT-V012-AD-CADEIA-GRUPOS-FECHADA.md`
   - cadeia transitiva de `_Engenharia` fechada em `_JumpServer`;
   - cadeia transitiva de `_Tecnica SSA` fechada em `_GAP2` e `_Tecnica`;
   - nenhum novo ancestral foi retornado acima desses grupos;
   - efeitos transitivos devem aparecer na pre-visualizacao do provisionamento;
   - leitura do perfil `Automation Systems Analyst + ENGENHARIA` considerada suficiente para montar o primeiro teste ponta a ponta.


13. `Checkpoints/CHECKPOINT-V013-TOPDESK-IMPORTACAO-REAL-E-PENDENCIAS-AD.md`
   - chamado ficticio `I2609-0295` criado no TOPdesk e importado com sucesso pelo CadastroColaboradores;
   - confirmou que OU, grupos e validacao da tela ainda estavam simulados antes desta implementacao.

14. `Checkpoints/CHECKPOINT-V014-IMPLEMENTACAO-LEITURA-REAL-AD.md` **(corrente)**
   - mocks de OU/grupos substituidos por consultas reais ao AD;
   - OU lida do AD e filtrada por allowlist de DistinguishedName;
   - disponibilidade de sAMAccountName/UPN/mail/proxyAddresses e CN na OU ligada ao botao de validacao;
   - limite de 20 caracteres e caracteres invalidos de sAMAccountName validados antes da pre-visualizacao;
   - superior imediato resolvido no AD;
   - sugestao de grupos por Title + Department implementada com `memberOf` e cadeia transitiva;
   - cargo aprovado PT-BR -> EN configuravel;
   - pre-visualizacao do objeto AD adicionada;
   - Microsoft 365 permanece pausado;
   - escrita no AD continua ausente/bloqueada.

## Estado atual resumido

### TOPdesk

- testes de integracao ja realizados;
- Bridge 1.0.2 em leitura;
- nenhuma escrita TOPdesk autorizada.

### Active Directory

- Fase 1 de leitura validada e ligada ao codigo da aplicacao;
- `WindowsAdReadOnlyService` lista OUs, valida identidade, superior, OU e grupos usando a identidade do processo IIS;
- `WindowsAccessSuggestionService` calcula grupos reais por `Title + Department` e mostra efeitos indiretos;
- OU de destino continua explicita e filtrada por allowlist de DistinguishedName;
- `_Informatica` permanece apenas na autorizacao humana e nao e usada como principal tecnico de escrita;
- gMSA `gMSA_CadColab$` criada, instalada no `SV052022-6121` e validada;
- grupo tecnico `SG_CadastroColaboradores_AD_Writer` criado e contendo somente a gMSA do projeto;
- ACL piloto aplicada somente em `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- ACL piloto validada com 56 ACEs totais, 16 ACEs exatas do grupo tecnico, owner preservado e heranca ativa;
- as 16 ACEs sao: 1 `CreateChild` para `user`, 14 `WriteProperty` aprovadas e 1 `Reset Password`;
- nenhuma outra OU recebeu delegacao do projeto;
- App Pool ainda usa `ApplicationPoolIdentity`;
- a aplicacao ainda nao possui `New-ADUser`, definicao de senha ou inclusao em grupos habilitados.

### Microsoft 365 / Entra

- `graph.microsoft.com:443` e `login.microsoftonline.com:443` acessiveis a partir de `10.1.2.21`;
- Microsoft Graph PowerShell nao esta instalado;
- Windows PowerShell: `5.1.17763.8146`;
- .NET Framework: 4.7.2 (`Release 461814`);
- PowerShellGet/PackageManagement: 1.0.0.1;
- PSGallery nao foi resolvida pelo PowerShellGet;
- `curl.exe` acessa o endpoint OIDC do Entra com HTTP 200 usando Schannel;
- `Invoke-WebRequest`/`Invoke-RestMethod` falharam na sessao PowerShell original que possuia callback customizado;
- a sessao PowerShell original tinha `ServerCertificateValidationCallback` customizado;
- os profiles padrao pesquisados nao existem;
- uma nova instancia `powershell.exe -NoProfile` inicia com callback `NULL` e obtem HTTP 200 no endpoint OIDC;
- com as evidencias atuais, nao ha motivo para alterar TLS/SCHANNEL do servidor;
- Tenant ID confirmado: `9ab05ca8-1779-410b-ae61-82dbd20810f3`;
- `/v1.0/subscribedSkus` responde HTTP 401 sem token, confirmando acesso ao endpoint protegido do Graph.

## Proximo passo pendente

Confirmar, a partir do servidor `10.1.2.21`, que a delegacao piloto de `07.Outros` ja esta visivel no AD consultado pelo servidor. Essa verificacao e somente leitura e precede qualquer alteracao da identidade do App Pool. Depois disso, capturar novamente o baseline atual do App Pool e preparar a mudanca para `gMSA_CadColab$` com rollback exato antes de qualquer escrita no IIS.

## Nome dos pacotes entregues

Enquanto a entrega for somente documental/checkpoint, usar `Automind.CadastroColaboradores-AAAA-MM-DD-checkpoint-vX.Y.Z.zip`. Para esta entrega de projeto completo, nao criar tag nem versionamento numerico de Git: o checkpoint acompanha o projeto e a branch `release` recebe apenas um commit. O versionamento interno `CHECKPOINT-VNNN` permanece para o historico documental.

15. `Checkpoints/CHECKPOINT-V015-CORRECAO-BUILD-SERVICOS-DEVELOPMENT.md` **(corrente)**
   - corrigidos os arquivos residuais `DevelopmentAdReadOnlyService.cs` e `DevelopmentAccessSuggestionService.cs` para clones/workspaces onde eles ainda existem;
   - `DevelopmentAdReadOnlyService` agora implementa todo o contrato atual de `IAdReadOnlyService`;
   - fallbacks de desenvolvimento retornam estado seguro/vazio e nao reintroduzem OU/grupos ficticios;
   - `Program.cs` continua usando somente os servicos Windows reais;
   - nenhuma escrita no AD foi adicionada.

16. `Checkpoints/CHECKPOINT-V016-GMSA-DELEGACAO-ESCRITA-AD.md` **(corrente)**
   - separacao formal entre autorizacao humana (`_informatica`) e identidade tecnica de escrita;
   - arquitetura alvo com gMSA exclusiva `gMSA_CadColab$`;
   - servidor autorizado: `SV052022-6121` / `10.1.2.21`;
   - grupo tecnico de delegacao sugerido `SG_CadastroColaboradores_AD_Writer`;
   - delegacao minima por OU e por grupo, sem Domain Admin/Full Control;
   - auditoria deve preservar operador humano + chamado TOPdesk mesmo quando o AD registrar a gMSA;
   - sequencia de implantacao e teste aprovada;
   - nenhuma escrita/configuracao foi executada neste checkpoint.

## Proximo passo apos V016

Executar somente consultas de pre-criacao no AD para confirmar que o nome da gMSA e o grupo tecnico sugeridos nao existem e confirmar o objeto do servidor `SV052022-6121$`. Depois desses resultados, apresentar os comandos exatos de criacao antes da primeira alteracao.

17. `Checkpoints/CHECKPOINT-V017-POLITICA-MUDANCA-AD-ROLLBACK.md` **(corrente)**
   - politica obrigatoria de impacto minimo/zero para AD e IIS;
   - nenhum reboot de DC, nenhum `iisreset` e nenhum reboot do `10.1.2.21` como parte normal da implantacao;
   - toda escrita exige baseline, rollback preparado e autorizacao explicita;
   - uma unica mudanca por vez, com validacao e documentacao antes de avancar;
   - delegacoes iniciam por uma unica OU/grupo piloto, nunca em lote;
   - rollback remove apenas ACEs/objetos adicionados pelo projeto, evitando restauracao cega de ACL completa;
   - eventual recycle futuro do App Pool e impacto localizado e deve ter janela/autorizacao propria;
   - fase atual permanece SOMENTE LEITURA.

## Proximo passo apos V017

Executar somente os testes de baseline/readiness explicitamente classificados como leitura. Nenhum `New-ADServiceAccount`, `New-ADGroup`, `Add-ADGroupMember`, alteracao de ACL, `Install-ADServiceAccount`, troca de identidade do App Pool, `Set-AD*`, `New-ADUser` ou comando equivalente sera executado antes da documentacao e aprovacao da etapa correspondente.

18. `Checkpoints/CHECKPOINT-V018-BASELINE-ACL-ENGENHARIA.md` **(corrente)**
   - nomes `gMSA_CadColab` e `SG_CadastroColaboradores_AD_Writer` confirmados livres por consulta;
   - servidor `SV052022-6121$` confirmado existente e habilitado;
   - baseline da OU `Engenharia`: owner `Domain Admins`, heranca habilitada, 40 ACEs;
   - ACL contem delegacoes preexistentes da gMSA de Entra Cloud Sync `pGMSA_c5fa29e8$`, inclusive direitos relacionados a usuario/reset de senha;
   - estas ACLs preexistentes nao serao tocadas nem usadas como rollback/modelo cego;
   - proxima rodada continua SOMENTE LEITURA: localizar origem/heranca dessas ACEs e capturar baseline do servidor `10.1.2.21`.

## Proximo passo apos V018

Nao criar gMSA, grupo tecnico ou delegacoes ainda. Primeiro concluir dois baselines somente leitura: (1) origem/heranca das ACEs existentes da `pGMSA_c5fa29e8$` na arvore `Automind -> 03.UDN -> Engenharia`; (2) estado atual do App Pool/ferramentas/ACL local no servidor `10.1.2.21`. Depois documentar e parar novamente para revisao.

---

## 23/09/2026 — V019: herança de ACL e baseline IIS

- As ACEs de `pGMSA_c5fa29e8$` observadas em `Automind`, `03.UDN` e `Engenharia` são herdadas.
- A origem da delegação está acima de `OU=Automind` e ainda será localizada por leitura.
- `Install-ADServiceAccount` e `Test-ADServiceAccount` estão disponíveis no host testado.
- O baseline de IIS não fechou: `WebAdministration` ausente, drive `IIS:` inexistente e caminho `C:\Automind.CadastroColaboradores` não encontrado.
- Nenhuma alteração foi executada.
- Regra editorial: vocabulário técnico simples, objetivo e sem redundância.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 — V020: conta legada e worker IIS

- `CriaMovePastas` existe e está habilitada;
- `PasswordNeverExpires=True`;
- associações observadas: `_CriaMovePastas` e `_GAP1`;
- não alterar essa conta até mapear dependências;
- ACL da pasta da aplicação capturada no `10.1.2.21`;
- não havia worker ativo no momento da consulta, então a identidade efetiva do processo ainda será confirmada;
- credencial exibida por diagnóstico não será registrada/repetida; rotação será tratada separadamente;
- nenhuma alteração foi executada.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 — V021: replicacao AD e identidade efetiva do IIS

- replicacao AD sem falhas (`0` falhas nos dois DCs);
- worker do `CadastroColaboradores` ativo e executando como `IIS APPPOOL\CadastroColaboradores`;
- `CriaMovePastas` nao e a identidade efetiva desse worker;
- `CriaMovePastas` aparece como `SpecificUser` apenas nos pools `.NET v4.5` e `.NET v4.5 Classic`;
- nao foram encontrados servicos ou tarefas agendadas usando essa conta no servidor consultado;
- nenhuma alteracao foi executada.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 — V022: contexto legado CriaMovePastas

- `CriaMovePastas` é um sistema legado separado do `CadastroColaboradores`;
- seu script PowerShell cria estruturas de Projeto/SAAS e aplica ACLs NTFS com `icacls`;
- o pool `CriaMovePastas` foi historicamente configurado com a conta pessoal elevada `AUTOMIND\daniel.gusmao.d`;
- essa configuração será corrigida separadamente e não será reutilizada no `CadastroColaboradores`;
- a conta/pool legado não serve como modelo para delegação no AD;
- `CadastroColaboradores` mantém a arquitetura planejada com gMSA própria e delegação mínima;
- nenhuma alteração foi executada.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 — V023: pré-requisitos de gMSA

- `provAgentgMSA` continua reservada ao Cloud Sync e não será reutilizada;
- `ADSyncMSA82bf3$` também não será reutilizada;
- `SV052022-6121` possui canal seguro válido com `automind.com.br`;
- DC localizado: `SV062022-6158` / `10.1.2.1`;
- horário sincronizado com o domínio;
- nenhuma alteração foi executada.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 — V024: KDS, níveis funcionais e container de gMSA

- `DomainMode=Windows2016Domain`;
- `ForestMode=Windows2008R2Forest`;
- container `CN=Managed Service Accounts` confirmado, owner `Domain Admins`, herança ativa e 36 regras no baseline;
- `Get-KdsRootKey` voltou a não exibir dados, mas o projeto já confirmou anteriormente um objeto `msKds-ProvRootKey` real no Configuration Naming Context;
- não executar `Add-KdsRootKey` e não alterar níveis funcionais;
- próxima etapa continua somente leitura: consultar diretamente a chave KDS existente e executar `Test-KdsRootKey` com seu GUID;
- nenhuma alteração foi executada.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 — V025: KDS validado para gMSA

- objeto KDS confirmado diretamente no AD: `74b88b84-4f70-b3b4-a399-127cf4723c34`;
- criado em `12/04/2023 19:03:39`;
- `Test-KdsRootKey` retornou `True`;
- nao criar nova KDS Root Key;
- prerequisito KDS encerrado;
- proxima etapa: simular (`-WhatIf`) a criacao da `gMSA_CadColab$`, sem escrita, com `SV052022-6121$` como unico host autorizado;
- antes da criacao real, registrar comando, efeito esperado e rollback exato.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 — V026: simulacao de criacao da gMSA validada

- `New-ADServiceAccount -WhatIf` apontou exclusivamente para `CN=gMSA_CadColab,CN=Managed Service Accounts,DC=automind,DC=com,DC=br`;
- nenhuma alteracao foi executada pelo `-WhatIf`;
- o nome/objeto de destino corresponde ao planejado;
- primeira escrita proposta: criar somente a `gMSA_CadColab$`, sem instalar no servidor, sem trocar identidade do App Pool, sem criar grupo tecnico e sem delegar ACL;
- validacao pos-mudanca sera somente leitura com `Get-ADServiceAccount`;
- rollback preparado: remover somente `gMSA_CadColab` se a validacao falhar ou se a mudanca precisar ser desfeita;
- a escrita depende de autorizacao explicita do operador.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V027: gMSA criada e contrato de escrita AD

- criada e validada `gMSA_CadColab$`;
- somente `SV052022-6121$` autorizado a recuperar a senha gerenciada;
- nenhuma delegacao, instalacao no servidor ou alteracao no IIS executada nesta etapa;
- criado `Docs/16-CONTRATO-ESCRITA-AD.md` como contrato tecnico da futura escrita de usuarios;
- fluxo futuro definido para criar conta inicialmente desabilitada e habilitar somente ao final;
- `manager` deve receber DN do superior localizado no AD;
- grupos terao allowlist de escrita separada;
- escrita de usuarios permanece bloqueada ate concluir delegacoes e testes.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V028: gMSA instalada no servidor

- `Test-ADServiceAccount gMSA_CadColab` retornou `True` no `SV052022-6121`;
- `Install-ADServiceAccount -WhatIf` confirmou o alvo esperado;
- `Install-ADServiceAccount gMSA_CadColab` foi executado sem erro;
- a gMSA esta preparada localmente no servidor;
- App Pool, grupo tecnico e ACLs continuam inalterados;
- rollback desta etapa: `Uninstall-ADServiceAccount -Identity gMSA_CadColab`;
- proxima etapa permanece em leitura para definir o grupo tecnico e seu local de criacao.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V029: gMSA validada e padrao do grupo tecnico

- `Test-ADServiceAccount gMSA_CadColab` retornou `True` no `SV052022-6121` apos a instalacao local;
- `OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br` existe e e candidata para o grupo tecnico;
- nao existem grupos com prefixo `SG_*` no dominio consultado;
- `_CriaMovePastas` e `_Informatica` sao grupos `Global / Security`;
- nao reutilizar `_CriaMovePastas` nem `_Informatica` como principal tecnico do CadastroColaboradores;
- candidato mantido: `SG_CadastroColaboradores_AD_Writer`, tipo `Global / Security`, em `06.Grupos-Gerais`;
- antes da criacao real: capturar ACL da OU, confirmar ausencia do nome e executar `New-ADGroup -WhatIf`;
- nenhuma nova escrita no AD foi executada nesta etapa.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V030: baseline e simulacao do grupo tecnico

- OU `06.Grupos-Gerais`: owner `Domain Admins`, heranca ativa e 40 ACEs no baseline;
- `SG_CadastroColaboradores_AD_Writer` nao existe no AD;
- `New-ADGroup -WhatIf` confirmou exatamente o DN planejado;
- nenhuma alteracao foi executada pelo `-WhatIf`;
- proxima escrita proposta: criar somente o grupo tecnico `Global / Security`, sem adicionar membros e sem delegar ACL;
- validacao pos-criacao: `Get-ADGroup` e confirmacao de grupo vazio;
- rollback preparado: remover somente esse grupo se a validacao falhar ou se a mudanca precisar ser desfeita;
- a criacao real depende de autorizacao explicita do operador.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V031: grupo tecnico criado

- criado `SG_CadastroColaboradores_AD_Writer`;
- tipo `Global / Security`;
- DN: `CN=SG_CadastroColaboradores_AD_Writer,OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br`;
- descricao: `Delegacao tecnica AD do Automind.CadastroColaboradores`;
- grupo criado vazio;
- nenhuma gMSA adicionada ao grupo nesta etapa;
- nenhuma delegacao de OU/grupo e nenhuma alteracao no IIS executada;
- proxima etapa: validar e simular a inclusao da `gMSA_CadColab$` como unico membro inicial.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V032: rollback completo AD e simulacao de membership

- grupo tecnico confirmado vazio;
- `gMSA_CadColab$` confirmada sem `MemberOf`;
- `Add-ADGroupMember -WhatIf` confirmou o alvo esperado sem alterar o AD;
- criado `Docs/17-ROLLBACK-AD.md` com rollback completo das alteracoes atuais e futuras do provisionamento;
- toda nova escrita passa a exigir: comando, efeito esperado, rollback e validacao antes da execucao;
- nenhuma nova escrita foi executada nesta rodada.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V033: gMSA vinculada ao grupo tecnico

- `gMSA_CadColab$` adicionada ao `SG_CadastroColaboradores_AD_Writer`;
- `Get-ADGroupMember` confirmou a gMSA como membro;
- `MemberOf` da gMSA confirmou o grupo tecnico;
- nenhuma ACL de OU/grupo foi delegada nesta etapa;
- IIS permanece sem alteracao de identidade;
- rollback especifico da associacao registrado em `Docs/17-ROLLBACK-AD.md`;
- proxima etapa permanece somente leitura: confirmar ausencia de ACEs do grupo tecnico e mapear GUIDs exatos antes de qualquer delegacao.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V034: ausencia de ACL confirmada e correcao do teste de GUIDs

- confirmado que `AUTOMIND\SG_CadastroColaboradores_AD_Writer` nao possui ACE no dominio, `OU=Automind`, `OU=03.UDN` nem `OU=Engenharia`;
- portanto, a gMSA vinculada ao grupo tecnico ainda nao possui delegacao de escrita nessas estruturas;
- o comando de mapeamento de GUIDs nao foi executado por erro de sintaxe PowerShell (`EmptyPipeElement`);
- o erro ocorreu antes de qualquer consulta de schema ser concluida e nao alterou o AD;
- o teste sera repetido com a coleta do `foreach` em variavel antes do `Format-Table`;
- nenhuma ACL, usuario, grupo, gMSA ou configuracao IIS foi alterada nesta rodada.

Detalhes: `Docs/CP-HIST.md`


## 23/09/2026 - V035: GUIDs de schema retornados, rotulacao pendente

- grupo tecnico continua sem ACE no dominio e nas OUs verificadas;
- classe `user`: `bf967aba-0de6-11d0-a285-00aa003049e2`;
- `Reset Password`: `00299570-246d-11d0-a768-00aa006e0529`;
- 14 GUIDs de atributos foram retornados, mas sem `lDAPDisplayName` visivel na tabela;
- antes de qualquer delegacao, repetir consulta somente leitura exibindo explicitamente `atributo -> GUID`;
- nenhuma ACL ou configuracao IIS foi alterada nesta rodada.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V036: mapeamento de GUIDs validado

- confirmado o mapeamento explicito dos 14 atributos do contrato para seus GUIDs de schema;
- classe `user` e direito `Reset Password` permanecem confirmados;
- nenhuma ACL foi alterada;
- proxima etapa: construir as ACEs propostas somente em memoria, sem `Set-Acl`;
- permissoes amplas permanecem fora da proposta inicial.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V037: ACEs em memoria validadas

- simulacao da delegacao da OU `Engenharia` concluida sem `Set-Acl`;
- ACL real permaneceu em `40` ACEs antes e depois;
- foram construidas `16` regras somente em memoria;
- conjunto: 1 `CreateChild` para `user`, 14 `WriteProperty` de atributos aprovados e 1 `Reset Password`;
- `GenericWrite`, `GenericAll`, `DeleteChild`, `WriteDacl` e `WriteOwner` continuam excluidos;
- `pwdLastSet` nao faz parte do contrato atual e nao sera delegado sem aprovacao separada;
- proximo passo: simular tambem o rollback exato das 16 ACEs em memoria, sem alterar o AD.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V038: rollback das ACEs validado em memoria

- simulacao de inclusao das 16 ACEs na OU `Engenharia` permaneceu somente em memoria;
- ACL inicial: `40` ACEs;
- ACL simulada com as 16 regras: `56` ACEs;
- rollback em memoria retornou para `40` ACEs;
- ACL real no AD permaneceu em `40` ACEs durante todo o teste;
- nenhum `Set-Acl` foi executado;
- antes de qualquer delegacao real, sera salvo um baseline local da ACL/SDDL para permitir comparacao e recuperacao controlada;
- proxima etapa continua sem escrita no AD.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V039: baseline da OU piloto 07.Outros

- OU piloto definida: `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- baseline: `40` ACEs, owner `AUTOMIND\Domain Admins`, heranca ativa;
- ACL e SDDL salvos localmente em `C:\Temp\CadastroColaboradores-Rollback`;
- hashes SHA-256 registrados no checkpoint V039;
- nenhuma ACL do AD foi alterada;
- proximo passo: simular as 16 ACEs somente em memoria na OU `07.Outros`.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V040: simulacao da delegacao em 07.Outros validada

- OU piloto: `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- ACL inicial: `40` ACEs;
- apos adicionar as 16 regras ao objeto ACL em memoria: `56` ACEs;
- rollback em memoria: `40` ACEs;
- ACL real no AD permaneceu em `40` ACEs;
- nenhum `Set-Acl` foi executado;
- baseline local da ACL/SDDL ja existe e os hashes foram registrados no V039;
- proxima escrita possivel: aplicar as 16 ACEs somente em `07.Outros`, com autorizacao explicita e rollback granular preparado.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V041: autorizacao da ACL piloto em 07.Outros

- operador autorizou explicitamente a primeira delegacao real de ACL;
- escopo exclusivo: `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- aplicar somente as 16 ACEs ja validadas em memoria;
- comando de escrita deve validar o SDDL atual contra o baseline e abortar em qualquer divergencia;
- salvar snapshot local imediatamente antes do `Set-Acl`;
- validacao esperada apos escrita: 56 ACEs totais, 16 pertencentes ao grupo tecnico, owner e heranca preservados;
- rollback granular permanece obrigatorio e remove somente as 16 ACEs do projeto;
- nenhuma outra OU esta autorizada nesta etapa.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V042: ACL piloto aplicada em 07.Outros; validacao detalhada pendente

- primeira delegacao real aplicada somente em `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- ACL total passou de `40` para `56` ACEs;
- foram encontradas `16` ACEs do grupo `AUTOMIND\SG_CadastroColaboradores_AD_Writer`;
- owner permaneceu `AUTOMIND\Domain Admins`;
- heranca permaneceu ativa (`AreAccessRulesProtected=False`);
- todas as 16 ACEs retornaram `IsInherited=False`;
- snapshot pos-escrita salvo em `C:\Temp\CadastroColaboradores-Rollback`;
- hashes pos-escrita: XML `5B2832851DA742D9AA4B3B768214282F694B41626BFC965ABE8F4B16E02C4584`; SDDL `31E8B76A144EFCB7CA83C8897651FB55FC3B1FDF362CCAA09CC3CC900BF95AC3`;
- a listagem recebida exibiu o GUID `f0f8ff88-1191-11d0-a060-00aa006c33ed` duas vezes e nao exibiu `f0f8ff8e-1191-11d0-a060-00aa006c33ed`;
- por politica de parada na primeira anomalia, nenhuma nova escrita deve ocorrer ate confirmar por leitura se ha duplicidade real ou apenas problema de exibicao/copia;
- rollback granular continua preparado e NAO deve ser executado sem autorizacao ou validacao de necessidade.

Detalhes: `Docs/CP-HIST.md`


## 23/09/2026 - V043: ACL piloto validada com 16 ACEs exatas

- consulta somente leitura confirmou `ACEs=1` para cada um dos 14 atributos aprovados;
- `givenName` e `company` estao corretos, sem duplicidade real;
- `CreateChild User=1`;
- `Reset Password=1`;
- total de ACEs do grupo tecnico: `16`;
- a divergencia visual observada no V042 foi descartada como problema de exibicao/copia, nao como erro da ACL;
- nenhuma correcao ou rollback foi necessario;
- ACL piloto de `07.Outros` permanece aplicada e validada;
- nenhuma nova escrita foi executada nesta validacao;
- proximo passo: confirmar a visibilidade da ACL a partir do servidor `10.1.2.21`, somente leitura, antes de preparar a troca de identidade do App Pool.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V044: validacao no servidor interrompida por tipo de HostName

- teste executado no servidor `10.1.2.21` foi somente leitura;
- DC descoberto: `SV062022-6158.automind.com.br`;
- `HostName` foi retornado como `ADPropertyValueCollection` e nao foi convertido para `System.String` antes de ser usado no parametro `-Server`;
- `Get-ADObject`, `Get-ADGroup` e `Get-ADGroupMember` falharam por binding de parametro;
- os valores finais `0/0/False` sao invalidos para diagnostico porque as consultas falharam antes;
- nenhuma alteracao ocorreu em AD, IIS ou servidor;
- nao ha evidência, por este resultado, de falha de replicacao;
- proximo passo: repetir a mesma verificacao convertendo explicitamente o primeiro `HostName` para string.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V045: ACL e membership confirmadas a partir do servidor 10.1.2.21

- teste repetido no servidor `10.1.2.21` com o HostName do DC convertido explicitamente para `System.String`;
- DC consultado: `SV062022-6158.automind.com.br`;
- ACL total da OU `07.Outros`: `56` ACEs;
- ACEs do grupo `SG_CadastroColaboradores_AD_Writer`: `16`;
- membership da `gMSA_CadColab$` no grupo tecnico: `True`;
- a delegacao piloto e a membership estao visiveis a partir do servidor da aplicacao;
- nenhuma escrita ocorreu nesta validacao;
- proximo passo: capturar o baseline atual do App Pool `CadastroColaboradores` e da identidade efetiva do worker antes de qualquer mudanca de identidade no IIS.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V046: baseline IIS parcial; Add-Type nao sera usado como referencia definitiva

- no servidor `10.1.2.21`, `Add-Type -AssemblyName Microsoft.Web.Administration` falhou por assembly nao encontrado;
- a sequencia posterior ainda retornou o App Pool `CadastroColaboradores` como `Started`, `ApplicationPoolIdentity`, `LoadUserProfile=True`, `StartMode=OnDemand` e pipeline `Integrated`;
- o campo `UserName` configurado permanece `AUTOMIND\\CriaMovePastas`, mas nao e a identidade efetiva enquanto `IdentityType=ApplicationPoolIdentity`;
- `Test-ADServiceAccount gMSA_CadColab=True`;
- nenhum worker estava ativo naquele instante;
- nenhuma alteracao ocorreu no IIS;
- por seguranca, o baseline definitivo sera repetido via `WebAdministration`/`appcmd`, sem `Add-Type` e sem leitura de senha;
- antes de trocar a identidade do pool, revisar tambem ACL NTFS da pasta da aplicacao.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V047: baseline IIS e NTFS confirmado

- `CadastroColaboradores` permanece `Started`, `ApplicationPoolIdentity`, `LoadUserProfile=True`, `StartMode=OnDemand`, pipeline `Integrated`;
- `AUTOMIND\\CriaMovePastas` permanece somente como valor residual em `UserName` enquanto `IdentityType=ApplicationPoolIdentity`;
- aplicacao vinculada: `CadastroColaboradores/`;
- nenhum worker ativo no instante da leitura;
- ACL da publicacao `C:\\Automind.CadastroColaboradores` foi inventariada sem alteracao;
- `BUILTIN\\Users` possui leitura/execucao e ACEs herdadas de criacao/anexacao em containers, mas ainda nao sera assumido que a gMSA recebe esse acesso sem confirmar a composicao do grupo local;
- nenhum direito NTFS novo sera concedido antes de comprovar necessidade;
- proximo passo: somente leitura para confirmar grupo local `Users`, autenticacao IIS e baseline exportavel antes de preparar a troca de identidade.

Detalhes: `Docs/CP-HIST.md`

## 23/09/2026 - V048: grupo local e propriedades IIS parciais

- `BUILTIN\\Users` contem `AUTOMIND\\Domain Users`, `NT AUTHORITY\\Authenticated Users` e `NT AUTHORITY\\INTERACTIVE`;
- `gMSA_CadColab$` permanece habilitada e retornou `PrimaryGroupID=515`;
- App Pool `CadastroColaboradores`: `ApplicationPoolIdentity`, `LogonBatch`, `LoadUserProfile=True`, `SetProfileEnvironment=True`;
- binding confirmado: `http 10.1.2.21:80:cadastro.automind.com.br`;
- propriedades de autenticacao foram retornadas como objetos `ConfigurationAttribute`, portanto ainda precisam ser repetidas com `.Value`;
- nenhuma ACL NTFS foi adicionada para a gMSA;
- nenhuma alteracao ocorreu em IIS, AD ou NTFS;
- proximo passo: leitura de autenticacao corrigida e `manualGroupMembership`/`IIS_IUSRS` antes de qualquer troca de identidade.

Detalhes: `Docs/CP-HIST.md`

## Atualizacao 23/09/2026 - pre-requisitos IIS/gMSA confirmados

No servidor `10.1.2.21`, o App Pool `CadastroColaboradores` permanece sem alteracao com:

- `IdentityType=ApplicationPoolIdentity`;
- `LogonType=LogonBatch`;
- `manualGroupMembership=False`;
- `LoadUserProfile=True`;
- autenticacao anonima habilitada com `IUSR`;
- autenticacao Windows desabilitada;
- binding HTTP `10.1.2.21:80:cadastro.automind.com.br`.

O grupo local `IIS_IUSRS` nao apresentou membros explicitamente cadastrados. A politica local exportada mostrou `SeBatchLogonRight` contendo `S-1-5-32-568` (`IIS_IUSRS`) e `SeDenyBatchLogonRight` nao configurado.

Como `manualGroupMembership=False`, a estrategia aprovada para o piloto e nao adicionar preventivamente a gMSA a `IIS_IUSRS` nem alterar direitos locais. O IIS sera deixado usar seu mecanismo padrao de SID `IIS_IUSRS` no token do worker e isso sera validado no momento controlado da troca do App Pool.

A gMSA continua instalada e valida no servidor (`Test-ADServiceAccount=True`). Nenhuma ACL NTFS adicional foi criada para ela.

### Proximo passo

Criar backup local do IIS e registrar um baseline nao secreto do App Pool antes da primeira troca real de identidade. O backup nao autoriza a troca; a alteracao para `gMSA_CadColab$` continua bloqueada ate haver comando, efeito esperado, rollback e validacao preparados e autorizacao explicita.

## 24/09/2026 - V049: backup IIS confirmado e nova regra operacional de testes

Backup local do IIS criado no servidor `10.1.2.21` antes de qualquer troca de identidade do App Pool:

- backup: `CadColab-Pre-gMSA-20260924-082229`;
- baseline: `C:\Temp\CadastroColaboradores-Rollback\IIS-CadastroColaboradores-before-20260924-082229.txt`;
- SHA-256 `applicationHost.config`: `B46EE9FEA44440598CB5B3014A631E3FD89E78D4358FCF22DA90385853511840`;
- SHA-256 baseline textual: `CD51C31F6CE9B87B1E09B00C5B5D48BAE2756CFB07946777FC3DE0755F8EE45B`;
- nenhuma alteracao de identidade, recycle, restart, IISReset, AD ou NTFS ocorreu nesta etapa.

### Nova regra operacional para testes

A partir deste checkpoint, a regra anterior de limitar a rodada a poucos testes fica substituida para verificacoes de baixo risco:

- quando os testes forem `SOMENTE LEITURA` ou `SIMULACAO`, estiverem no mesmo local de execucao e puderem ser executados com seguranca em sequencia, agrupar varios testes em uma unica linha de PowerShell;
- e permitido executar mais de 3 validacoes/testes na mesma linha para reduzir tempo operacional;
- a linha deve continuar informando claramente o local de execucao e o nivel de risco;
- falhas intermediarias devem ser tratadas para evitar que resultados finais enganadores sejam apresentados como validos;
- `ALTERACAO REAL` continua com uma mudanca por vez, sempre com efeito esperado, rollback e validacao preparados antes da execucao;
- nao concatenar varias escritas reais independentes apenas para ganhar velocidade.

Detalhes: `Docs/CP-HIST.md`

## 24/09/2026 - V050: App Pool alterado para gMSA; validacao HTTP inicial inconclusiva

Alteracao real executada no servidor `10.1.2.21`:

- App Pool `CadastroColaboradores` passou para `IdentityType=SpecificUser`;
- identidade configurada: `AUTOMIND\gMSA_CadColab$`;
- pool permaneceu `Started`;
- `LogonType=LogonBatch`;
- `Test-ADServiceAccount=True`.

A primeira requisicao HTTP de validacao usou `127.0.0.1` com `Host=cadastro.automind.com.br` e retornou 404. Nao houve worker ativo apos essa tentativa. Como o binding inventariado e `10.1.2.21:80:cadastro.automind.com.br`, o teste via loopback nao e conclusivo para o site alvo. Nao executar rollback ainda apenas por esse resultado.

Proximo passo: validacao somente leitura consolidada usando o IP real `10.1.2.21`/FQDN, seguida de checagem do worker, owner do processo, estado do pool e eventos WAS/W3SVC/IIS/AspNetCore. Nenhuma nova escrita ate concluir essa validacao.

Detalhes: `Docs/CP-HIST.md`

## 24/09/2026 - V051: App Pool gMSA validado ponta a ponta

A validacao consolidada no servidor `10.1.2.21` confirmou a troca do App Pool `CadastroColaboradores` para a gMSA:

- pool `Started`;
- `IdentityType=SpecificUser`;
- `UserName=AUTOMIND\gMSA_CadColab$`;
- `Test-ADServiceAccount=True`;
- site e aplicacao vinculados corretamente ao pool;
- HTTP via IP/binding correto: `200`;
- HTTP via FQDN: `200`;
- worker ativo: PID `5820` no momento do teste;
- owner do worker: `AUTOMIND\gMSA_CadColab$`;
- evento IIS AspNetCore Module V2 ID `1032`: aplicacao iniciada com sucesso;
- sem erro WAS/W3SVC/IIS/.NET na janela consultada.

A validacao anterior por `127.0.0.1` foi definitivamente classificada como teste inadequado ao binding e nao como falha da gMSA.

Estado atual: nao executar rollback, nao conceder ACL NTFS/direitos locais adicionais e nao usar `iisreset`. A proxima etapa deve validar as operacoes reais da aplicacao sob a identidade gMSA e, para qualquer escrita AD, manter uma alteracao real por vez com rollback preparado.

Detalhes: `Docs/CP-HIST.md`

## 24/09/2026 - V052: configuracao da aplicacao permanece bloqueada para escrita

Validacao somente leitura executada no servidor `10.1.2.21` apos a troca bem-sucedida do App Pool para `AUTOMIND\\gMSA_CadColab$`:

- `Automind:Mode=ReadOnly`;
- AD Server: `10.1.2.1`;
- PeopleSearchBase: `OU=Automind,DC=automind,DC=com,DC=br`;
- AuthorizedGroup: `_informatica`;
- allowlist atual de OUs: `22` entradas;
- `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br` **nao** esta na allowlist;
- Microsoft 365 continua desabilitado;
- SHA-256 do `appsettings.json` implantado: `9C6DB1F3CE04EB96C99157DA8E2A99CAC0808B83448F32D933507E315DAB441A`;
- App Pool: `Started`, `SpecificUser`, `AUTOMIND\\gMSA_CadColab$`;
- `Test-ADServiceAccount=True`;
- HTTP do site: `200`.

Conclusao: a infraestrutura tecnica da gMSA esta operacional, mas a aplicacao permanece em modo de leitura e `07.Outros` ainda nao e um destino permitido pela configuracao. Nenhuma escrita de usuario esta liberada.

Revisao do codigo atual confirmou ainda que a linha implantada possui apenas servicos de leitura (`IAdReadOnlyService`/`WindowsAdReadOnlyService`) e nao possui servico de escrita registrado nem action de criacao de usuario. Portanto, alterar somente `Automind:Mode` ou a allowlist nao e suficiente para habilitar provisionamento; a escrita tera de ser implementada explicitamente e revisada antes de qualquer piloto.

Proximo passo recomendado: preparar a implementacao de escrita piloto limitada a `07.Outros`, mantendo `Microsoft365.Enabled=false`, sem alterar outras OUs e sem habilitar escrita antes de build, revisao, rollback de codigo/configuracao e autorizacao explicita.

Detalhes: `Docs/CP-HIST.md`

## 24/09/2026 - V053: implementacao de escrita piloto preparada, ainda ReadOnly

- implementado servico de escrita AD controlada, mas `Automind:Mode=ReadOnly` permanece no pacote;
- unica OU da allowlist de escrita: `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- `07.Outros` foi adicionada a allowlist de leitura do codigo para permitir pre-validacao;
- backend exige identidade tecnica `AUTOMIND\gMSA_CadColab$` e revalida o operador humano em `_informatica` antes de escrever;
- fluxo implementado: criar desabilitado -> atributos -> senha -> manager -> releitura -> habilitar somente no final -> releitura final;
- auditoria JSONL deve estar gravavel antes da primeira escrita AD;
- senha temporaria nao e persistida pela aplicacao e so e retornada na resposta de sucesso;
- grupos continuam sem escrita; `proxyAddresses`, `pwdLastSet` e M365 continuam bloqueados;
- em falha apos CreateChild, a aplicacao tenta manter a conta desabilitada e nao exclui automaticamente;
- `appsettings.json` e JavaScript foram validados estaticamente;
- `dotnet build` nao foi executado no ambiente de geracao por ausencia do .NET SDK;
- nenhuma publicacao, alteracao de servidor ou criacao de usuario foi feita nesta etapa;
- proximo passo obrigatorio: build em maquina com .NET 10 SDK e revisao do resultado antes de publicar.

Detalhes: `Docs/CP-HIST.md` e `Docs/19-PILOTO-ESCRITA-AD.md`.)

## 24/09/2026 - V054: build local do piloto aprovado, ainda ReadOnly

Build executado na maquina de desenvolvimento com .NET 10 SDK:

- projeto: `Automind.CadastroColaboradores`;
- target: `net10.0`;
- resultado: **sucesso**, 0 erros;
- avisos: 15 `CA1416`, todos relacionados ao uso de APIs `System.DirectoryServices` suportadas apenas em Windows;
- como a aplicacao e destinada a Windows/IIS, esses avisos nao bloquearam o piloto e nao representam falha de compilacao.

Validacoes de seguranca realizadas na mesma rodada:

- `Automind:Mode=ReadOnly`;
- allowlist de leitura: 23 OUs;
- `07.Outros` presente na allowlist de leitura;
- allowlist de escrita: exatamente 1 OU;
- alvo de escrita: `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- `GroupWritesEnabled=False`;
- `Microsoft365.Enabled=False`;
- `WindowsAdProvisioningWriteService.cs` presente;
- `FileProvisioningAuditService.cs` presente.

Conclusao: o codigo do piloto compila e mantem as travas previstas. Nenhuma publicacao/deploy, alteracao de servidor ou escrita em AD ocorreu nesta etapa.

Proximo passo: preparar publicacao da nova versao **ainda em ReadOnly**, com rollback do deploy definido antes de qualquer mudanca no servidor. Somente depois da validacao do deploy em ReadOnly sera preparada a ativacao `PilotWrite`.

Detalhes: `Docs/CP-HIST.md`

## 24/09/2026 - V055: publish ReadOnly validado e checkpoint incorporado ao pacote completo

Publish local da linha piloto concluido com sucesso:

- `net10.0`;
- 0 erros informados;
- 15 avisos `CA1416` de compatibilidade Windows para `System.DirectoryServices`;
- artefato publicado permaneceu com `Automind:Mode=ReadOnly`;
- allowlist de leitura: 23 OUs, incluindo `07.Outros`;
- allowlist de escrita: exatamente 1 OU, `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- `GroupWritesEnabled=False`;
- `Microsoft365.Enabled=False`;
- DLL e `web.config` presentes;
- hashes SHA-256 foram gerados para os principais artefatos do publish; nao registrar valores completos que nao estejam disponiveis integralmente.

Nova regra operacional: durante a preparacao do pacote completo para release/teste, manter o checkpoint dentro do proprio pacote completo e nao gerar ZIP de checkpoint separado a cada etapa. Checkpoints separados voltam a ser considerados quando entrar uma nova fase de testes ou por solicitacao explicita.

O estado continua seguro para deploy de validacao porque o pacote permanece `ReadOnly`. `PilotWrite` devera ser ativado somente em etapa separada, com autorizacao e rollback preparados.

Detalhes: `Docs/CP-HIST.md`

## 24/09/2026 - V056: fluxo Visual Studio + Git + Azure DevOps formalizado

Fluxo operacional informado e aprovado para a fase atual de ajustes iniciais:

- desenvolvimento e revisao local no Visual Studio;
- branch atual `release`;
- pacote completo do assistente atualiza a copia local do projeto;
- build local antes de commit/push;
- `origin` = GitHub para backup/espelho;
- `azure` = Azure DevOps Repos para o fluxo de publicacao;
- alteracoes sao registradas por commit, sem tag e sem bump/versionamento numerico nesta fase;
- apos commit, push de `release` para `origin` e `azure`;
- Azure Pipeline/Release existente publica automaticamente/no fluxo ja configurado para o servidor, onde o responsavel valida visual e funcionalmente cada rodada;
- Git representa o backup/historico de codigo durante os ajustes, sem substituir rollback de AD/IIS;
- checkpoint continua incorporado ao pacote completo; nao gerar ZIP separado durante esta fase;
- revisar `git status --short` antes de `git add .`, principalmente por causa da pasta local `artifacts/` gerada por publishes de teste.

Referencia detalhada: `Docs/10-FLUXO-GIT-E-PUBLICACAO.md` e `Docs/CP-HIST.md`

## 24/09/2026 - V057: pacote curto/versionado e revisao da tela ReadOnly

Nova regra de distribuicao:

- pacote completo atual passa a `CadColab-v0.1.0.zip`;
- proximos ajustes desta linha: `v0.1.1`, `v0.1.2`, ...;
- `VERSION.txt` registra a versao;
- nao criar tag Git automaticamente; manter commits na branch `release`;
- historico detalhado dos checkpoints foi consolidado em `Docs/CP-HIST.md` para reduzir caminhos longos;
- codigo funcional nao foi alterado nesta rodada de reorganizacao/nomenclatura.

Revisao da tela publicada em 24/09/2026:

- banner confirma `AD REAL - SOMENTE LEITURA` e `Automind:Mode=ReadOnly`;
- chamado `I2609-0295` carregado;
- OU selecionada `07.Outros`;
- sugestao de grupos retornou 8 comuns, 4 excecoes e 0 protegidos;
- o piloto continua sem gravacao de memberships;
- pre-validacao apresentou uma unica pendencia: `Login valido e disponivel`;
- o login de teste exibido e `teste.provisionamento`, que ultrapassa o limite de 20 caracteres do `sAMAccountName` ja validado pelo projeto;
- nenhuma escrita foi executada.

Antes de ativar `PilotWrite`, fechar a pre-validacao com login de ate 20 caracteres e revisar o comportamento dos checkboxes de grupos, pois a tela mostra os grupos comuns selecionados mesmo com escrita de memberships desabilitada.

## 24/09/2026 - V058 / pacote v0.1.1: sugestoes de grupos mantidas e escrita continua bloqueada

A revisao do teste ReadOnly confirmou que a pre-validacao ficou integralmente verde com o login curto `teste.cadcolab`. A OU exibida no PDF era `05.Terceiros-Ext` por selecao acidental do operador; nao foi tratada como falha de regra de tela.

Decisoes aplicadas no codigo da `v0.1.1`:

- grupos comuns ao cargo continuam marcados automaticamente;
- excecoes continuam desmarcadas;
- protegidos continuam bloqueados;
- `GroupWritesEnabled=false` passa a significar somente **sem escrita de memberships**, sem obrigar desmarcar as sugestoes;
- o endpoint de criacao nao bloqueia mais apenas porque existem grupos comuns marcados;
- o comando enviado ao servico de escrita continua com `GroupDns=[]`, portanto nenhuma membership e alterada;
- a interface deixa explicito que as marcacoes sao somente sugestoes enquanto a escrita de grupos estiver desabilitada;
- nao foi adicionado check temporario de `OU autorizada para escrita piloto`; a pre-validacao continua com `OU valida`;
- a protecao real de OU permanece no backend: `WriteAllowedOuDns` segue contendo apenas `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- `Automind:Mode` continua `ReadOnly`;
- `Microsoft365.Enabled=false`;
- `proxyAddresses` e `pwdLastSet` continuam fora da escrita;
- pacote completo passa a `CadColab-v0.1.1.zip`.

Proximo gate: build local .NET 10 -> commit/push na branch `release` -> deploy Azure ainda `ReadOnly` -> repetir o teste com `07.Outros` selecionada. Somente depois disso preparar a ativacao real de `PilotWrite`.



## 24/09/2026 - V059 / pacote v0.1.2: limite de 20 caracteres do login visivel na tela

A tela de Novo colaborador passa a tornar explicita a restricao do Active Directory para `sAMAccountName`:

- limite maximo: 20 caracteres;
- contador visual `N / 20`;
- aviso em destaque quando um valor importado ultrapassar o limite;
- `maxlength=20` para edicao manual;
- backend continua validando o limite independentemente da interface;
- nao truncar automaticamente valores importados: o operador deve decidir o login curto correto.

Caso que motivou o ajuste: `teste.provisionamento` ultrapassou 20 caracteres e foi ajustado manualmente para `teste.cadcolab`.

Nenhuma escrita AD foi habilitada; `Mode=ReadOnly` permanece.
