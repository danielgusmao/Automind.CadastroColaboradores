# CHECKPOINT - INDICE ATUAL

## Politica de versionamento

A partir de 2026-09-23, este arquivo deixa de acumular todo o historico em um unico documento.

- `Docs/CHECKPOINT.md` e apenas o indice/resumo atual.
- O historico fica em `Docs/Checkpoints/CHECKPOINT-VNNN-*.md`.
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

- Fase 1 de leitura validada e agora ligada ao codigo da aplicacao;
- `WindowsAdReadOnlyService` lista OUs, valida identidade, superior, OU e grupos usando a identidade do processo IIS;
- `WindowsAccessSuggestionService` calcula grupos reais por `Title + Department` e mostra efeitos indiretos;
- OU de destino continua explicita e e filtrada por allowlist de DistinguishedName;
- a conta administrativa usada nos testes nao e armazenada nem usada pela aplicacao;
- `_Informatica` permanece apenas na autorizacao de login e nao deve ser reutilizado como grupo tecnico de escrita;
- nenhuma conta/grupo/ACL tecnica nova foi criada;
- nao existe `New-ADUser`, senha ou inclusao em grupos nesta entrega.

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

Publicar esta entrega na branch `release` e repetir o chamado `I2609-0295` contra a aplicacao atualizada. O teste deve confirmar OU real, cargo em ingles, coorte/grupos reais, superior, disponibilidade de identidade e a pre-visualizacao final. Somente depois desse teste deve ser discutida a implementacao/ativacao de `New-ADUser` e inclusao em grupos, sempre com autorizacao explicita e identidade tecnica de escrita definida.

## Nome dos pacotes entregues

Enquanto a entrega for somente documental/checkpoint, usar `Automind.CadastroColaboradores-AAAA-MM-DD-checkpoint-vX.Y.Z.zip`. Para esta entrega de projeto completo, nao criar tag nem versionamento numerico de Git: o checkpoint acompanha o projeto e a branch `release` recebe apenas um commit. O versionamento interno `CHECKPOINT-VNNN` permanece para o historico documental.

15. `Checkpoints/CHECKPOINT-V015-CORRECAO-BUILD-SERVICOS-DEVELOPMENT.md` **(corrente)**
   - corrigidos os arquivos residuais `DevelopmentAdReadOnlyService.cs` e `DevelopmentAccessSuggestionService.cs` para clones/workspaces onde eles ainda existem;
   - `DevelopmentAdReadOnlyService` agora implementa todo o contrato atual de `IAdReadOnlyService`;
   - fallbacks de desenvolvimento retornam estado seguro/vazio e nao reintroduzem OU/grupos ficticios;
   - `Program.cs` continua usando somente os servicos Windows reais;
   - nenhuma escrita no AD foi adicionada.
