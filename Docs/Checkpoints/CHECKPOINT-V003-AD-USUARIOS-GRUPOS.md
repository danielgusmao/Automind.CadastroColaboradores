# CHECKPOINT V003 - Active Directory: usuarios, grupos e identidade

> Arquivo historico preservado do checkpoint acumulado anterior. Nao editar retroativamente; novas evidencias vao para versoes posteriores.

## 2026-09-23 - gMSA existente identificada como Azure AD Cloud Sync e padrão de grupos de aplicação confirmado

Contexto desta rodada:
- testes executados diretamente no Active Directory pelo responsável, usando sua conta administrativa elevada;
- todos os comandos foram somente de leitura;
- nenhuma conta, gMSA, grupo, ACL, KDS, Application Pool, serviço ou configuração de servidor foi criada ou alterada;
- permanece válido que a conta administrativa pessoal do responsável não será utilizada pela aplicação.

Resultado da consulta correta da gMSA existente `pGMSA_c5fa29e8$`:
- Name: `provAgentgMSA`;
- SamAccountName: `pGMSA_c5fa29e8$`;
- Enabled: `True`;
- DNSHostName: `automind.com.br`;
- ManagedPasswordIntervalInDays: `30`;
- PasswordLastSet: `24/08/2026 15:02:30`;
- Description: `Azure AD cloud sync service account`;
- ServicePrincipalNames: nenhum valor apresentado;
- `PrincipalsAllowedToRetrieveManagedPassword`: nenhum principal foi retornado pelo comando executado.

Conclusão sobre essa gMSA:
- a própria descrição identifica `provAgentgMSA` como conta do Azure AD Cloud Sync;
- ela não deve ser reutilizada pelo CadastroColaboradores;
- nenhuma propriedade da conta será modificada;
- eventual adoção de gMSA para o CadastroColaboradores deve usar identidade dedicada, e somente após decisão e autorização explícita.

Resultado da pesquisa por grupos de segurança relacionados a aplicações/serviços:
- foram localizados grupos globais de segurança com nomenclatura/finalidade específica de sistemas, incluindo:
  - `_Contatos_Automind_G1` - descrição: `Grupo de permissão no sistema de Contatos - EDICAO`;
  - `_Contatos_Automind_G2` - descrição: `Grupo de permissão no sistema de Contatos - LEITURA`;
  - `_Coord_Sistemas`;
  - `_GAC1` - descrição: `GAC (Grupo de Acesso a Cadastro)`;
  - `_GAC2` - descrição: `GAC (Grupo de Acesso à Cadastro)`;
  - `CAD_SGI` - descrição: `Permissão de CADASTROS do SGI`;
- a mesma pesquisa também retornou grupos internos/padrão do Windows/IIS/RDS, que não devem ser interpretados como grupos funcionais da aplicação.

Conclusão sobre o padrão de autorização:
- existe evidência de uso corporativo de grupos de segurança Global dedicados a sistemas e níveis/finalidades de acesso;
- isso reforça o desenho de delegar permissões do CadastroColaboradores a grupo de segurança dedicado, em vez de conta pessoal;
- ainda não está autorizado criar esse grupo, definir seu nome/localização, adicionar a conta `SV052022-6121$`, criar gMSA ou delegar ACLs;
- o desenho de identidade técnica permanece em avaliação entre `ApplicationPoolIdentity`/conta de máquina + grupo dedicado e uma futura gMSA dedicada + grupo dedicado.

Retorno ao roteiro funcional documentado do Active Directory - Fase 1:
- autenticação e grupo `_informatica`: já validados anteriormente;
- listagem real de OUs: validada;
- próximas capacidades de leitura previstas em `Docs/05-ACTIVE-DIRECTORY.md`: pesquisar usuários, consultar cargo/departamento, pesquisar superior imediato, listar/comparar grupos e verificar disponibilidade de login/UPN/e-mail;
- o próximo teste deve começar pela leitura de usuários e dos atributos `title`, `department`, `manager`, `mail` e `userPrincipalName`, sem qualquer escrita;
- somente depois de validar os atributos reais do domínio serão testadas resolução do manager, associação de grupos e disponibilidade de identificadores.

Referências oficiais verificadas:
- Microsoft Learn `Get-ADUser`: permite pesquisar usuários e recuperar propriedades adicionais usando `-Properties`;
- Microsoft Learn Active Directory object model: `Department`, `Manager`, `EmailAddress/mail` e demais atributos são propriedades do objeto de usuário;
- Microsoft Learn `Get-ADPrincipalGroupMembership`: retorna grupos dos quais um usuário/computador/grupo/conta de serviço é membro;
- Microsoft Learn Service Accounts/gMSA: contas gerenciadas são apropriadas para isolar identidades de serviços, inclusive IIS, mas uma gMSA existente de outro serviço não deve ser presumida reutilizável.

Próxima rodada planejada:
- executar somente consultas de leitura para verificar como os usuários reais estão preenchendo `Title`, `Department`, `Manager`, `mail` e `userPrincipalName`;
- nenhum teste de escrita será executado;
- no máximo três testes por rodada e cada comando PowerShell continuará em uma única linha.

## 2026-09-23 - Leitura de usuários, cargo/departamento e superior validada no Active Directory

Contexto desta rodada:
- testes executados diretamente no Active Directory pelo responsável, usando sua conta administrativa elevada;
- todos os comandos foram somente de leitura;
- nenhuma conta, atributo, grupo, OU, ACL, gMSA, Application Pool ou configuração de servidor foi criada ou alterada;
- os resultados desta rodada validam a estrutura e os dados disponíveis no AD, mas não substituem a validação futura da identidade técnica da aplicação para operações efetivas.

Resultado da amostragem de usuários ativos sob `OU=Automind,DC=automind,DC=com,DC=br`:
- usuários ativos encontrados: `190`;
- com `Title` preenchido: `153`;
- com `Department` preenchido: `152`;
- com `Manager` preenchido: `150`;
- com `mail` preenchido: `161`;
- com `userPrincipalName` preenchido: `190`.

Conclusões factuais sobre os atributos:
- `userPrincipalName` está presente em todos os 190 usuários ativos consultados;
- `mail`, `Title`, `Department` e `Manager` não estão presentes em 100% dos usuários ativos;
- a implementação futura não pode assumir preenchimento obrigatório desses atributos sem regra/validação explícita;
- a amostra retornou valores reais de cargo e departamento, por exemplo `Network Administrator`/`GSTI`, `Bussiner Director`/`DIRETORIA`, `Commercial Consultant`/`SBM`, `IT Analyst`/`GSTI`, `Administrative & Finance Leader`/`ADM_FIN`, entre outros;
- o conteúdo atual do AD deve ser preservado como fonte de verdade para os testes, inclusive eventuais grafias existentes; nenhuma normalização será aplicada sem decisão funcional posterior.

Validação do atributo `manager`:
- o atributo `Manager` retorna o Distinguished Name de outro objeto de usuário;
- a resolução desse DN via `Get-ADUser` funcionou corretamente;
- exemplos confirmados:
  - `fernando.menezes` -> `fabio.daniel`;
  - `macario` -> `paulo.frank`;
  - `manuel` -> `gualbert.silva`;
  - `daniel.gusmao` -> `fabio.daniel`;
  - `millena` -> `renato.penna`;
- ao resolver o manager, também foi possível obter nome, cargo e e-mail do superior.

Conclusão desta etapa funcional:
- capacidade de pesquisar usuários: validada por leitura;
- capacidade de consultar cargo/departamento: validada por leitura;
- capacidade de pesquisar/resolver superior imediato: validada por leitura;
- permanece pendente implementar essas consultas no `IAdReadOnlyService` real; esta rodada apenas confirmou o comportamento e os dados do domínio;
- nenhuma alteração de código está autorizada nesta etapa.

Retorno ao roteiro de `Docs/05-ACTIVE-DIRECTORY.md` e `Docs/03-DECISOES.md`:
- a próxima capacidade de Fase 1 a validar é `listar grupos` e `comparar associação de grupos`;
- a regra funcional aprovada para sugestão de grupos é comparar usuários ativos com o mesmo cargo/departamento e sugerir os grupos comuns a todos;
- usuário de referência é opcional e não deve ser a regra principal;
- grupos privilegiados/protegidos nunca serão copiados automaticamente;
- após a validação de grupos, permanece pendente verificar disponibilidade de login/UPN/e-mail.

Próxima rodada planejada:
- primeiro identificar combinações reais de `Title + Department` que possuam mais de um usuário ativo, para que a comparação de grupos seja testada com uma população válida;
- depois consultar os grupos dos usuários equivalentes e calcular a interseção observada;
- no máximo três testes por rodada, todos somente leitura e em uma linha quando PowerShell.

## 2026-09-23 - Comparacao de grupos por cargo/departamento validada em amostra real; pendente confirmacao estrita de grupos diretos

Contexto da rodada:
- testes executados no Active Directory com a conta administrativa do operador, somente para leitura/inspecao;
- nenhuma conta, grupo, associacao de grupo, OU, ACL, gMSA, Application Pool ou configuracao de servidor foi criada ou alterada;
- regra funcional em avaliacao: usuarios ativos com o mesmo `Title + Department` devem ter seus grupos comparados e somente os grupos comuns a todos devem ser sugeridos; grupos presentes apenas em parte dos usuarios devem aparecer como excecoes e nao ser pre-selecionados.

Resultado da identificacao de perfis equivalentes:
- maior combinacao encontrada: `Instrumentation Technician` + `SENSE METRICS`, com 12 usuarios ativos;
- outras combinacoes reais com mais de um usuario tambem foram encontradas, incluindo `Systems Analyst` + `T&S` (10), `Automation Technician` + `ENGENHARIA` (7), `Automation Systems Analyst` + `ENGENHARIA` (5), entre outras.

Resultado do primeiro levantamento de associacoes usando `Get-ADPrincipalGroupMembership` para os 12 usuarios `Instrumentation Technician` / `SENSE METRICS`:
- `_Tecnica SSA` (nome real retornado com acento: `_Técnica SSA`) presente em 12/12;
- `_Todos` presente em 12/12;
- `_Todos SSA` presente em 12/12;
- `Domain Users` presente em 12/12;
- `_GAP6` apareceu apenas para `bruna.mota`, portanto representa uma associacao parcial/excecao na amostra e nao deve ser sugerida pela regra de intersecao.

Interpretacao funcional provisoria:
- a amostra real confirma que a estrategia de intersecao por `Title + Department` consegue distinguir grupos comuns de grupos individuais/excepcionais;
- `_GAP6` e um exemplo concreto do comportamento esperado para grupo presente em parte dos usuarios: deve ser exibido como excecao, nao pre-selecionado;
- ainda nao considerar a etapa de grupos totalmente concluida porque `Docs/06-GRUPOS-E-PERFIS.md` exige explicitamente `grupos diretos`.

Correcao metodologica apos consulta a documentacao oficial Microsoft:
- o atributo `memberOf` do objeto de usuario contem os grupos dos quais o usuario e membro DIRETO;
- `memberOf` nao inclui o grupo primario, que e representado por `primaryGroupId`;
- `Domain Users` e normalmente o grupo primario de um usuario e, por isso, nao aparece em `memberOf`;
- como o teste anterior utilizou `Get-ADPrincipalGroupMembership` e retornou `Domain Users`, sera feita uma rodada final baseada em `memberOf` para alinhar o teste exatamente a regra documentada de grupos diretos e evitar que o grupo primario seja tratado como sugestao funcional.

Proxima validacao autorizada apenas para leitura:
1. recalcular a intersecao dos grupos DIRETOS dos 12 usuarios do perfil mais frequente usando `memberOf`;
2. listar os grupos diretos parciais/excecoes desse mesmo perfil e a quantidade de usuarios em que aparecem;
3. inspecionar metadados de seguranca dos grupos resultantes (`adminCount`, `isCriticalSystemObject`, escopo, categoria, SID e descricao) antes de definir a regra para grupos privilegiados/protegidos.

Fontes consultadas:
- Microsoft Learn, `User Security Attributes`: `memberOf` contem grupos de associacao direta, exceto o grupo primario;
- Microsoft Open Specifications, atributo `memberOf`: associa os DNs dos grupos aos quais o objeto pertence, exceto o grupo primario;
- Microsoft Open Specifications, `primaryGroupID`: por padrao aponta para `Domain Users`, que nao aparece no atributo `memberOf`.

## 2026-09-23 - Grupos diretos confirmados; excecao real identificada; grupo _Informatica requer verificacao de protecao

Contexto da rodada:
- resultados recebidos dos tres testes de leitura executados no Active Directory pelo responsavel com conta administrativa elevada;
- nenhuma conta, grupo, associacao, OU, ACL, gMSA, Application Pool ou configuracao foi alterada;
- o objetivo foi alinhar a regra de sugestao de grupos ao requisito documentado de usar somente associacoes DIRETAS (`memberOf`) e identificar grupos potencialmente protegidos antes de qualquer futura automacao.

Resultado da intersecao estrita por `memberOf` para o perfil `Instrumentation Technician` + `SENSE METRICS`:
- total de usuarios: `12`;
- `_Técnica SSA`: `12/12`;
- `_Todos`: `12/12`;
- `_Todos SSA`: `12/12`;
- `Domain Users` NAO apareceu na intersecao baseada em `memberOf`.

Conclusao sobre grupos diretos:
- o resultado confirma empiricamente no dominio `automind.com.br` que a regra de negocio deve usar associacao direta (`memberOf`) para a sugestao funcional;
- o grupo primario (`Domain Users`) nao deve ser tratado como sugestao funcional por esta regra;
- a documentacao oficial Microsoft confirma que `memberOf` nao inclui o grupo primario, representado separadamente por `primaryGroupID`;
- a estrategia aprovada permanece: intersecao dos grupos diretos dos usuarios equivalentes por `Title + Department`.

Resultado das excecoes do mesmo perfil:
- `_GAP6`: presente em `1/12`, somente no usuario `bruna.mota`;
- a amostra confirma a regra prevista em `Docs/06-GRUPOS-E-PERFIS.md`: grupo presente em parte dos usuarios deve ser exibido como excecao e NAO pre-selecionado.

Resultado do inventario de grupos com `adminCount=1` e/ou `isCriticalSystemObject=True`:
- foram retornados diversos grupos internos/protegidos do Active Directory/Windows, incluindo `Domain Admins`, `Enterprise Admins`, `Schema Admins`, `Administrators`, `Account Operators`, `Backup Operators`, `Server Operators`, `Print Operators`, `Domain Controllers` e outros;
- o grupo funcional `_Informatica`, utilizado atualmente na autorizacao de entrada da aplicacao, apareceu com `adminCount=1`;
- `BackOffice Folder Operators` e `Exchange Domain Servers` tambem apareceram com `adminCount=1`;
- `_Informatica` nao apareceu na lista oficial padrao de grupos protegidos da Microsoft consultada nesta rodada.

Cuidado metodologico sobre `adminCount`:
- `adminCount=1` e evidencia que exige investigacao, mas NAO sera usado isoladamente para classificar um grupo como atualmente protegido;
- segundo a documentacao Microsoft, objetos removidos de grupos protegidos podem permanecer com `adminCount=1`;
- antes de criar qualquer regra automatica de exclusao de grupos, deve-se verificar associacao direta/transitiva a grupos protegidos e o estado de heranca do descritor de seguranca;
- nenhum atributo `adminCount`, ACL ou membership sera alterado durante esta investigacao.

Estado da etapa de grupos:
- comparacao por cargo/departamento: validada em amostra real;
- intersecao de grupos diretos: validada;
- identificacao de grupos parciais/excecoes: validada;
- pendencia de seguranca: esclarecer o estado atual de `_Informatica` porque esse mesmo grupo e usado na autorizacao da aplicacao e retornou `adminCount=1`;
- somente depois dessa verificacao a sequencia retorna ao teste de disponibilidade de `sAMAccountName`, UPN e e-mail.

Proxima rodada de leitura planejada:
1. consultar propriedades do grupo `_Informatica`, principalmente DN, `adminCount`, `memberOf`, descricao e datas de alteracao;
2. verificar se a heranca de ACL do objeto `_Informatica` esta desabilitada e inspecionar seu owner, sem alterar ACL;
3. consultar a cadeia transitiva de grupos acima de `_Informatica` via LDAP_MATCHING_RULE_IN_CHAIN para saber se ele esta atualmente aninhado em algum grupo protegido conhecido.

Referencias oficiais verificadas:
- Microsoft Learn `[MS-ADA2] Attribute memberOf`: o atributo lista os grupos aos quais o objeto pertence, exceto o grupo primario;
- Microsoft Learn `User Security Attributes`: `primaryGroupId` nao aparece em `memberOf`; por padrao `Domain Users` e o grupo primario dos usuarios;
- Microsoft Learn `Appendix C - Protected Accounts and Groups in Active Directory`: lista os grupos padrao protegidos e descreve AdminSDHolder/SDProp;
- Microsoft Learn `Reducing the Active Directory Attack Surface`: `adminCount=1` pode permanecer depois que um objeto deixa de pertencer a grupo protegido;
- Microsoft Learn `LDAP Matching Rules`: `1.2.840.113556.1.4.1941` (`LDAP_MATCHING_RULE_TRANSITIVE_EVAL/IN_CHAIN`) permite percorrer a cadeia de associacao por DN.

## 2026-09-23 - Grupo _Informatica confirmado como aninhado em grupos privilegiados; seguir para disponibilidade de identidade

Contexto da rodada:
- resultados recebidos dos tres testes de leitura executados no Active Directory pelo responsavel com conta administrativa elevada;
- nenhuma conta, grupo, associacao, ACL, OU, gMSA, Application Pool ou configuracao foi alterada;
- objetivo: esclarecer o estado real do grupo `_Informatica`, atualmente usado para autorizar o acesso humano ao CadastroColaboradores, antes de seguir para os testes de disponibilidade de identidade.

Resultado do objeto `_Informatica`:
- `Name`: `_Informatica`;
- `SamAccountName`: `_Informatica`;
- `GroupScope`: `Global`;
- `GroupCategory`: `Security`;
- `adminCount`: `1`;
- `isCriticalSystemObject`: vazio/False;
- `whenCreated`: `24/09/2004 18:28:14`;
- `whenChanged`: `21/09/2026 11:55:56`;
- DN: `CN=_Informatica,OU=GSTI,OU=04.UDA,OU=Automind,DC=automind,DC=com,DC=br`.

Associacoes diretas relevantes de `_Informatica` observadas em `memberOf`:
- `_JumpServer`;
- `Performance Log Users`;
- `Print Operators`;
- `Account Operators`;
- `Backup Operators`;
- `Server Operators`;
- `Remote Desktop Users`;
- `Performance Monitor Users`;
- `Network Configuration Operators`.

Resultado da ACL do objeto `_Informatica`:
- `HerancaBloqueada`: `True`;
- owner: `AUTOMIND\Domain Admins`;
- regras explicitas: `21`;
- regras herdadas: `0`.

Resultado da cadeia transitiva de grupos acima de `_Informatica`:
- a consulta LDAP `1.2.840.113556.1.4.1941` confirmou na cadeia, entre outros, `_JumpServer`, `Account Operators`, `Backup Operators`, `Network Configuration Operators`, `Performance Log Users`, `Performance Monitor Users`, `Print Operators`, `Remote Desktop Users` e `Server Operators`;
- `Account Operators`, `Backup Operators`, `Print Operators` e `Server Operators` constam na lista oficial Microsoft de grupos protegidos por AdminSDHolder;
- portanto, neste ambiente, nao e mais apenas uma inferencia baseada em `adminCount`: `_Informatica` esta efetivamente aninhado em grupos privilegiados/protegidos do dominio.

Implicacao para o projeto, sem alteracao nesta fase:
- `_Informatica` continua sendo o grupo de autorizacao atual da aplicacao enquanto nao houver decisao explicita para mudar isso;
- NAO reutilizar `_Informatica` como grupo tecnico de permissao para as operacoes automatizadas do CadastroColaboradores;
- qualquer futura revisao do grupo de login/autorizacao da aplicacao deve ser tratada como melhoria separada, apresentada previamente e aprovada pelo responsavel;
- nenhuma associacao, ACL ou atributo do grupo sera alterado durante a fase de testes.

Fontes oficiais verificadas:
- Microsoft Learn `Appendix C - Protected Accounts and Groups in Active Directory`: `Account Operators`, `Backup Operators`, `Print Operators` e `Server Operators` estao entre os grupos protegidos por AdminSDHolder;
- Microsoft Learn `Active Directory Security Groups`: documenta os privilegios e a protecao AdminSDHolder desses grupos;
- Microsoft Learn `Reducing the Active Directory Attack Surface`: membros de grupos protegidos deixam de herdar permissoes normais; `adminCount=1` isoladamente poderia permanecer apos remocao, mas a associacao atual observada nesta rodada elimina essa ambiguidade para `_Informatica`.

Estado atual dos testes AD de Fase 1:
- autenticacao AD: testada;
- autorizacao atual por `_Informatica`: testada; grupo identificado como privilegiado e registrado como item futuro de melhoria, sem mudanca agora;
- listagem de OUs: testada;
- leitura de usuarios: testada;
- leitura de `Title`, `Department`, `Manager`, `mail` e `userPrincipalName`: testada;
- resolucao de `Manager`: testada;
- comparacao de grupos diretos por `Title + Department`: testada;
- intersecao de grupos comuns: testada;
- excecoes de grupo: testadas;
- proxima capacidade prevista em `Docs/05-ACTIVE-DIRECTORY.md`: verificar disponibilidade de `sAMAccountName`, UPN e e-mail.

Proxima rodada planejada, somente leitura:
1. validar uma identidade EXISTENTE para provar que a consulta detecta ocupacao de `sAMAccountName`, `userPrincipalName`, `mail` e `proxyAddresses`;
2. validar uma identidade sintetica NAO existente para provar o caminho de disponibilidade sem criar objeto algum;
3. verificar se existem duplicidades atuais nos atributos que precisam ser unicos para sincronizacao Microsoft 365 (`userPrincipalName`, `mail` e `proxyAddresses`) e em `sAMAccountName`.

Cuidado adicional para Microsoft 365:
- a documentacao Microsoft exige unicidade de `mail`, `proxyAddresses` e `userPrincipalName` para sincronizacao confiavel;
- `sAMAccountName` tem limite de 20 caracteres e deve ser unico no diretorio;
- portanto a futura verificacao de disponibilidade nao deve olhar apenas `mail`; deve considerar tambem UPN, `sAMAccountName` e enderecos SMTP existentes em `proxyAddresses`.

## 2026-09-23 - Disponibilidade de identidade validada; encontrada duplicidade real em mail

Contexto da rodada:
- tres testes de leitura executados no Active Directory pelo responsavel com conta administrativa elevada;
- nenhuma conta, atributo, proxy address, grupo, ACL, OU, gMSA, Application Pool ou configuracao foi alterada;
- objetivo: validar deteccao de ocupacao/disponibilidade de `sAMAccountName`, `userPrincipalName`, `mail` e `proxyAddresses`, conforme proxima capacidade prevista na Fase 1.

Teste de identidade existente (`fernando.menezes`):
- objeto encontrado corretamente;
- `sAMAccountName`: `fernando.menezes`;
- `userPrincipalName`: `fernando.menezes@automind.com.br`;
- `mail`: `fernando.menezes@automind.com.br`;
- `proxyAddresses` retornou pelo menos `smtp:fernando.menezes@automind.com.br` e um endereco primario prefixado por `SMTP:` cujo valor exibido na saida foi `fernando.menezes@automind.co`;
- como essa divergencia pode ser relevante para Exchange/Microsoft 365, ela deve ser confirmada por leitura exata do atributo antes de qualquer conclusao ou ajuste.

Teste de identidade sintetica (`teste.cadastro260923`):
- nenhuma colisao encontrada em `sAMAccountName`, UPN, `mail` ou `proxyAddresses`;
- isso valida o caminho de consulta para indicar disponibilidade sem criar ou reservar qualquer objeto.

Inventario de duplicidades no AD:
- `DuplicidadesSamAccountName`: 0;
- `DuplicidadesUPN`: 0;
- `DuplicidadesMail`: 2;
- `DuplicidadesSMTPProxy`: 0;
- valor duplicado em `mail`: `paex@automind.com.br`, presente em 2 objetos;
- a existencia da duplicidade em `mail` precisa ser investigada por objeto antes de definir a regra final do sistema; nao corrigir ou alterar nenhum objeto nesta fase.

Referencias oficiais verificadas:
- Microsoft Learn `Prepare for directory synchronization to Microsoft 365`: `mail`, `proxyAddresses`, `userPrincipalName` e `sAMAccountName` devem ser unicos para sincronizacao confiavel; duplicidade de `mail` pode impedir que todos os objetos aparecam corretamente no Microsoft 365;
- Microsoft Learn `How the proxyAddresses attribute is populated in Microsoft Entra ID`: `SMTP:` em maiusculas identifica o endereco SMTP primario e `smtp:` em minusculas identifica endereco secundario;
- Microsoft Learn `Troubleshoot errors during synchronization`: duplicidades em atributos de identidade podem gerar `AttributeValueMustBeUnique`/quarentena na sincronizacao Entra.

Decisao provisoria para a futura validacao do CadastroColaboradores, ainda sem implementacao:
- a disponibilidade de identidade devera considerar pelo menos `sAMAccountName`, `userPrincipalName`, `mail` e todos os SMTP em `proxyAddresses`;
- a regra final so sera fechada depois de identificar os dois objetos que usam `paex@automind.com.br` e confirmar o `proxyAddresses` exato de `fernando.menezes`;
- nao assumir que a duplicidade atual e erro operacional sem antes identificar a natureza e o estado dos dois objetos.

Proxima rodada planejada, somente leitura:
1. identificar os dois objetos que possuem `mail=paex@automind.com.br`, incluindo tipo, estado, UPN e `proxyAddresses`;
2. ler `proxyAddresses` de `fernando.menezes` sem truncamento para confirmar qual e o `SMTP:` primario real;
3. somente se necessario, verificar se algum dos dois objetos `paex` esta sincronizado/ativo e qual deveria ser considerado pela futura regra de disponibilidade.

## 2026-09-23 - Duplicidade de mail PAEX explicada; divergencia de SMTP primario de Fernando confirmada

Contexto da rodada:
- dois testes adicionais de leitura executados no Active Directory pelo responsavel com conta administrativa elevada;
- nenhuma conta, grupo, endereco, proxy address, ACL, OU, Application Pool ou configuracao foi alterada;
- objetivo: identificar a duplicidade real de `mail=paex@automind.com.br` e confirmar sem truncamento os `proxyAddresses` de `fernando.menezes` antes de fechar a regra futura de disponibilidade de identidade.

Resultado da duplicidade `paex@automind.com.br`:
1. objeto `_Paex`:
   - `ObjectClass`: `group`;
   - `sAMAccountName`: `_Paex`;
   - `mail`: `Paex@automind.com.br`;
   - `proxyAddresses`: inclui `SMTP:Paex@automind.com.br` como SMTP primario e `smtp:Paex@autosoftbr.com.br` como secundario, alem de enderecos X400;
   - DN: `CN=_Paex,OU=Automind,DC=automind,DC=com,DC=br`.
2. objeto `Dist_Paex`:
   - `ObjectClass`: `group`;
   - `sAMAccountName`: `Dist_Paex`;
   - `mail`: `paex@automind.com.br`;
   - `proxyAddresses`: vazio;
   - DN: `CN=Dist_Paex,OU=ListaDistribuicao,OU=Automind,DC=automind,DC=com,DC=br`.

Interpretacao comprovada desta rodada:
- a duplicidade encontrada no atributo `mail` nao e entre usuarios; ocorre entre dois grupos;
- `_Paex` possui o endereco em `proxyAddresses` como SMTP primario;
- `Dist_Paex` possui o mesmo valor apenas em `mail`;
- portanto, para a futura verificacao de disponibilidade de um novo usuario, a pesquisa nao deve ser limitada a objetos `user`; deve verificar conflitos de identidade/endereco em objetos relevantes do diretorio, incluindo grupos/contatos quando aplicavel;
- nao corrigir ou alterar nenhum dos dois grupos PAEX nesta fase.

Resultado exato de `fernando.menezes`:
- `userPrincipalName`: `fernando.menezes@automind.com.br`;
- `mail`: `fernando.menezes@automind.com.br`;
- `proxyAddresses`:
  - `SMTP:fernando.menezes@automind.co`;
  - `smtp:fernando.menezes@automind.com.br`;
- portanto a divergencia observada anteriormente foi confirmada: o SMTP primario registrado em `proxyAddresses` e `@automind.co`, enquanto UPN e `mail` usam `@automind.com.br`.

Referencias oficiais verificadas nesta rodada:
- Microsoft Learn `Identity synchronization and duplicate attribute resiliency`: UPN e SMTP ProxyAddress devem ser exclusivos entre objetos User, Group e Contact no tenant Entra; conflitos podem ser colocados em quarentena pela resiliencia de atributo duplicado;
- Microsoft Learn `Troubleshoot errors during synchronization` / `Prepare for directory synchronization to Microsoft 365`: `mail`, `proxyAddresses` e UPN podem causar conflitos de unicidade na sincronizacao;
- Microsoft Learn `Procedures for email address policies in Exchange Server`: prefixo `SMTP:` em maiusculas representa o endereco SMTP primario e `smtp:` em minusculas representa endereco secundario.

Decisao de teste, ainda sem implementacao:
- nao classificar `SMTP:fernando.menezes@automind.co` como erro ou typo antes de verificar se `automind.co` e dominio/endereco utilizado em outros objetos do ambiente;
- manter como regra candidata, ainda nao implementada, que a verificacao de disponibilidade deve pesquisar `sAMAccountName`, UPN, `mail` e SMTP em `proxyAddresses` em todo o escopo relevante do diretorio;
- duplicidades legadas existentes devem ser relatadas, mas nao corrigidas automaticamente pela aplicacao.

Proxima rodada planejada, somente leitura:
1. localizar objetos que usam `@automind.co` como SMTP primario;
2. levantar a distribuicao dos dominios usados como SMTP primario no AD para saber se `automind.co` e padrao ou caso isolado;
3. se necessario, ler atributos Exchange relacionados de `fernando.menezes` (por exemplo `targetAddress`) para entender a origem da divergencia, sem alterar qualquer atributo.

## 2026-09-23 - Dominio SMTP primario @automind.co confirmado como regra intencional; script diario documentado

Contexto informado pelo responsavel:
- `automind.co` e um alias do dominio principal `automind.com.br`;
- existe um script diario no Active Directory que adiciona automaticamente `proxyAddresses` para usuarios cujo `SamAccountName` contem ponto (`*.*`) e que ainda nao possuem `proxyAddresses`;
- regra implementada pelo script:
  - `SMTP:<alias>@automind.co` = endereco SMTP primario;
  - `smtp:<alias>@automind.com.br` = endereco SMTP secundario/alias;
- o script usa `Set-ADUser -Add @{ proxyAddresses = $emails }` e, portanto, e um processo externo ja existente no ambiente; o CadastroColaboradores nao deve alterar esse script nem substituir sua responsabilidade sem autorizacao explicita.

Script informado pelo responsavel, mantido aqui como referencia operacional:
```powershell
Import-Module ActiveDirectory

Get-ADUser -Filter { SamAccountName -like "*.*" -and -not (proxyAddresses -like "*") } -Properties proxyAddresses |
ForEach-Object {

    $alias = $_.SamAccountName

    $emails = @(
        "SMTP:$alias@automind.co",
        "smtp:$alias@automind.com.br"
    )

    Set-ADUser $_ -Add @{ proxyAddresses = $emails }

    Write-Host "[OK] Atualizado:" $alias
}
```

Consequencia para a investigacao:
- `SMTP:fernando.menezes@automind.co` nao e anomalia; esta aderente a regra operacional existente;
- `mail` e `userPrincipalName` podem permanecer em `@automind.com.br` enquanto o SMTP primario em `proxyAddresses` usa `@automind.co`;
- a futura logica de criacao/validacao do CadastroColaboradores deve respeitar esta convencao e nao tentar "corrigir" o SMTP primario para `@automind.com.br` sem decisao explicita.

Resultados da rodada de leitura subsequente:
- inventario de SMTP primario encontrou:
  - 221 objetos com dominio `automind.co`;
  - 26 objetos com dominio `automind.com.br`;
  - 1 objeto com dominio `aurtomind.co`;
  - 1 objeto com dominio `utomind.co`;
- isso confirma que `automind.co` e o padrao dominante para SMTP primario, mas deixa dois valores com grafia diferente que precisam ser identificados antes de qualquer conclusao/correcao;
- consulta de propriedades Exchange adicionais em `fernando.menezes` falhou porque `Get-ADUser -Properties msExchRecipientTypeDetails` retornou `One or more properties are invalid`; nenhuma alteracao foi feita.

Validacao em documentacao oficial Microsoft:
- em `proxyAddresses`, `SMTP:` maiusculo representa o endereco SMTP primario e `smtp:` minusculo representa endereco secundario;
- a documentacao Microsoft mostra que atributos Exchange como `msExchRecipientTypeDetails` podem precisar ser lidos via `Get-ADObject -Properties *` quando nao sao aceitos diretamente pela consulta feita com `Get-ADUser` no ambiente/modulo atual;
- nenhuma conclusao deve ser tirada do erro de propriedade sem primeiro verificar quais atributos Exchange existem de fato no schema/objeto do ambiente.

Proxima rodada autorizada de diagnostico, somente leitura:
1. identificar o objeto cujo SMTP primario termina em `@aurtomind.co`;
2. identificar o objeto cujo SMTP primario termina em `@utomind.co`;
3. verificar em `fernando.menezes` quais atributos Exchange relacionados existem de fato, usando leitura generica do objeto, sem alterar nada.

## 2026-09-23 - AD Fase 1 encerrada; regra SMTP confirmada e proximo bloco Microsoft 365 / Entra

Contexto da rodada:
- o responsavel informou que `automind.co` e um alias intencional do dominio principal `automind.com.br`;
- existe processo diario externo no AD que adiciona `proxyAddresses` para usuarios elegiveis, usando `SMTP:<alias>@automind.co` como primario e `smtp:<alias>@automind.com.br` como secundario;
- nenhuma conta, atributo, grupo, ACL, OU, gMSA, Application Pool, script, servidor ou configuracao foi alterada nesta rodada.

Resultados dos dois testes finais de leitura:
1. objetos com SMTP primario fora dos dominios esperados:
   - `dev automind` (`sAMAccountName=dev.automind`) possui `SMTP:dev.autominda@utomind.co` e secundario `smtp:dev.autominda@utomind.com.br`;
   - `Jeliel Santos Godinho de Jesus` (`sAMAccountName=jeliel.santos`) possui `SMTP:jeliel.santos@aurtomind.co` e secundario `smtp:jeliel.santos@aurtomind.com.br`;
   - esses dois registros ficam documentados como divergencias legadas/isoladas; nao corrigir automaticamente e nao usar como padrao do CadastroColaboradores.
2. leitura generica de atributos Exchange de `fernando.menezes`:
   - unico atributo correspondente retornado com valor foi `msExchRequireAuthToSendTo=False`;
   - nao foram retornados `targetAddress`, `mailNickname` ou outros atributos `msExch*` com valor na consulta realizada;
   - portanto nao ha evidencia, nessa leitura, de que `targetAddress` explique o SMTP primario de Fernando; o SMTP `@automind.co` permanece explicado pela regra operacional/script informado pelo responsavel.

Regra operacional consolidada para identidade/e-mail, ainda sem implementacao:
- nao tratar `@automind.co` como erro;
- considerar como convencao esperada do ambiente, quando aplicavel:
  - UPN: `@automind.com.br`;
  - atributo `mail`: normalmente `@automind.com.br`, respeitando excecoes legadas existentes;
  - SMTP primario em `proxyAddresses`: `SMTP:<alias>@automind.co`;
  - SMTP secundario em `proxyAddresses`: `smtp:<alias>@automind.com.br`;
- a futura verificacao de disponibilidade deve pesquisar conflitos em `sAMAccountName`, `userPrincipalName`, `mail` e todos os SMTP de `proxyAddresses`, incluindo objetos nao-user quando aplicavel;
- divergencias legadas devem ser sinalizadas/registradas, nunca corrigidas automaticamente sem autorizacao.

Fechamento dos testes Active Directory - Fase 1 (`Docs/05-ACTIVE-DIRECTORY.md`):
- autenticar usuario/senha: TESTADO;
- verificar grupo `_informatica`: TESTADO;
- pesquisar usuarios: TESTADO;
- pesquisar cargos/departamentos: TESTADO;
- pesquisar superior imediato: TESTADO;
- listar OUs: TESTADO;
- listar grupos: TESTADO;
- comparar associacao de grupos: TESTADO;
- verificar disponibilidade de login/UPN/e-mail: TESTADO;
- operacoes de escrita do AD continuam PROIBIDAS nesta fase e nao foram executadas.

Achados que ficam para melhorias futuras, sem alteracao agora:
- `_Informatica` e grupo privilegiado/aninhado em grupos protegidos e nao deve ser reutilizado como grupo tecnico da aplicacao;
- a aplicacao deve usar OUs permitidas/configuradas pela administracao em vez de expor toda a arvore do dominio;
- identidade tecnica futura para escrita no AD deve ser separada da conta pessoal do responsavel e receber permissoes por grupo dedicado;
- avaliar posteriormente gMSA dedicada versus identidade atual do Application Pool, somente com aprovacao explicita;
- os objetos `dev automind` e `jeliel.santos` possuem dominios SMTP divergentes (`utomind.co` / `aurtomind.co`) e ficam apenas registrados para eventual saneamento separado.

Proximo bloco formal conforme documentacao do projeto:
- `Docs/07-MICROSOFT-365.md` define a primeira etapa de Microsoft 365 / Entra como somente consulta;
- objetivos: listar SKUs existentes, quantidade habilitada, quantidade consumida e quantidade disponivel;
- atribuicao de licenca permanece fora de escopo e so podera ser implementada apos aprovacao explicita;
- antes de qualquer autenticacao ou criacao de App Registration/segredo/certificado, validar conectividade do servidor e ferramentas ja existentes, sem instalar ou alterar nada.

