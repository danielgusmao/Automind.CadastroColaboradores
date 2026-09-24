# Historico detalhado de checkpoints

Arquivo consolidado em 24/09/2026 para reduzir nomes e caminhos no Windows. O conteudo dos checkpoints historicos foi preservado abaixo.


---

## CHECKPOINT-V001-BASE-TOPDESK.md

# CHECKPOINT V001 - Base inicial e TOPdesk

> Arquivo historico preservado do checkpoint acumulado anterior. Nao editar retroativamente; novas evidencias vao para versoes posteriores.

# CHECKPOINT

## Estado atual
Starter MVC .NET 10 criado para testes locais.

## Implementado no starter
- identidade visual Automind inicial;
- tela de login mock;
- dashboard;
- formulário editável de colaborador;
- cenário exemplo I2609-0223;
- sugestão visual de grupos por cargo (mock);
- área administrativa inicial;
- histórico placeholder;
- documentação das decisões.

## Ainda NÃO implementado
- autenticação AD real;
- leitura AD real;
- TOPdesk real;
- SQL Server;
- Teams real;
- Microsoft Graph;
- qualquer escrita no AD/M365/TOPdesk/Teams.

## Próximo passo recomendado
Validar o projeto localmente e aprovar o fluxo/telas. Depois implementar autenticação e leitura real do AD, mantendo escrita bloqueada.

## Ajuste aprovado - telefone celular
- padrão obrigatório de exibição: `(DD) 9 XXXX-XXXX`;
- exemplo aprovado: `(71) 9 8169-6721`;
- telefone com 10 dígitos recebe automaticamente o nono dígito após o DDD;
- código de país `55`, quando presente, é removido para a apresentação interna;
- máscara aplicada no formulário e normalização aplicada no ViewModel.


## 2026-09-22 - Importação TOPdesk via extensão
- Teste confirmado: extensão + sessão SAML ativa retorna HTTP 200 para I2609-0223.
- Teste confirmado: sem sessão TOPdesk retorna HTTP 401.
- Home e tela de colaborador passam a solicitar o chamado via Automind TOPdesk Bridge.
- Manual curto publicado em `/ajuda/extensao-topdesk.html`.
- Downloads Brave/Chrome publicados em `/downloads/`.
- Pasta local padrão definida: `%USERPROFILE%\Automind\Extensoes\TopdeskBridge\`.
- Chamados de teste seguintes: I2508-0393 e I2603-0141.
- AD continua sem escrita; sugestão de acessos ainda está em mock até a etapa de leitura real do AD.

## 2026-09-22 - Regra obrigatória de chamado
- todo cadastro de colaborador deve possuir chamado TOPdesk;
- não existe cadastro "sem chamado";
- chamado estruturado: importa JSON pela extensão;
- chamado simples: número do chamado obrigatório e preenchimento manual dos dados;
- fonte permanece TOPdesk nos dois modos.

---

## CHECKPOINT-V002-AD-OU-IIS-IDENTIDADE.md

# CHECKPOINT V002 - Active Directory: OUs, IIS e identidade tecnica

> Arquivo historico preservado do checkpoint acumulado anterior. Nao editar retroativamente; novas evidencias vao para versoes posteriores.

## 2026-09-23 - Protocolo operacional para continuidade com LLM

Este projeto deve seguir obrigatoriamente estas regras em qualquer continuidade por ChatGPT, Codex, Claude ou outro agente LLM:

- não criar, editar ou alterar código, arquivos, configurações, infraestrutura ou servidores sem consentimento explícito do responsável;
- qualquer alteração em servidor deve ser informada previamente e somente executada após autorização;
- antes de propor alteração, primeiro executar testes de diagnóstico, consultar a documentação do projeto e, quando aplicável, confirmar o comportamento em documentação oficial/fontes confiáveis na internet;
- não antecipar hipóteses de causa como conclusão antes de obter evidências dos testes;
- solicitar no máximo 3 testes por rodada e aguardar os resultados antes de avançar;
- quando o teste for PowerShell, cada comando deve ser fornecido em uma única linha, sem quebra;
- priorizar testes somente de leitura enquanto a etapa estiver em validação de integrações;
- registrar continuamente: testes solicitados, resultados recebidos, decisões, alterações autorizadas, arquivos modificados, commits, deploys, pendências e itens que não podem ser alterados;
- manter checkpoints suficientes para que outro agente LLM consiga retomar o trabalho sem depender de memória externa ou inferências.

## 2026-09-23 - Estado consolidado das integrações antes dos próximos testes

### TOPdesk
- testes de integração já realizados e confirmados pelo responsável;
- Bridge 1.0.2 permanece como integração de leitura via sessão SAML do navegador;
- não há autorização para escrita no TOPdesk nesta etapa.

### Active Directory - autenticação
- testes de integração da autenticação AD já realizados e confirmados pelo responsável;
- autenticação real utiliza `WindowsAdAuthenticationService`;
- servidor AD configurado em `Automind:Ad:Server`: `10.1.2.1`;
- grupo autorizado configurado: `_informatica`;
- autenticação e verificação de associação ao grupo são operações reais no código atual;
- nenhuma escrita no AD está autorizada.

### Active Directory - consultas cadastrais
- `IAdReadOnlyService` continua registrado com `DevelopmentAdReadOnlyService`;
- portanto listagem de OUs, disponibilidade de login/e-mail e demais consultas cadastrais ainda são mock no sistema;
- a etapa seguinte definida em 23/09/2026 é validar, fora do código da aplicação e somente por leitura, se o servidor IIS consegue consultar e listar as OUs reais do AD;
- somente após obter evidência desse teste será discutida qualquer implementação no `IAdReadOnlyService`.

### Microsoft 365 / Entra / Teams
- ainda não há implementação real validada nesta sequência;
- esses testes permanecem pendentes e não devem substituir a validação de leitura das OUs do AD agora definida como próxima etapa.

## 2026-09-23 - Próximo teste autorizado: leitura de OUs do Active Directory

Objetivo: confirmar que é possível consultar as Organizational Units reais do domínio de forma somente leitura, apontando explicitamente para o servidor AD configurado (`10.1.2.1`).

Restrições:
- não criar, mover ou alterar OUs;
- não instalar módulo ou recurso no servidor sem autorização prévia;
- não alterar identidade do Application Pool, DNS, firewall, configuração LDAP ou credenciais como parte do diagnóstico;
- se o teste falhar, registrar a saída completa antes de discutir qualquer mudança.

Referência técnica confirmada: o Active Directory permite enumerar OUs por consulta LDAP usando `System.DirectoryServices.DirectorySearcher`, e o RootDSE fornece o `defaultNamingContext` do domínio. A alternativa com `Get-ADOrganizationalUnit` depende do módulo ActiveDirectory/RSAT, portanto o diagnóstico inicial deve evitar instalar dependências apenas para realizar a leitura.

## 2026-09-23 - Resultado do teste real de listagem de OUs do Active Directory

Contexto de execução:
- comando executado diretamente no servidor IIS `10.1.2.21`;
- sessão PowerShell executada com o usuário do responsável pelo projeto;
- esse usuário possui privilégios elevados no Active Directory/controlador de domínio;
- portanto este resultado comprova conectividade e permissão de leitura para essa identidade administrativa, mas não deve ser usado isoladamente como prova das permissões da identidade que executa a aplicação no IIS.

Resultado:
- consulta LDAP ao servidor AD `10.1.2.1` concluída com sucesso;
- `RootDSE/defaultNamingContext` respondeu corretamente para `DC=automind,DC=com,DC=br`;
- a enumeração de objetos `organizationalUnit` funcionou e retornou a estrutura real de OUs do domínio;
- nenhuma escrita, criação, movimentação ou alteração de objeto no Active Directory foi realizada.

Amostras confirmadas da estrutura real:
- `OU=01.Diretoria,OU=Automind,DC=automind,DC=com,DC=br`;
- `OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`;
- `OU=04.UDA,OU=Automind,DC=automind,DC=com,DC=br`;
- `OU=Operacoes,OU=Automind,DC=automind,DC=com,DC=br`;
- existem OUs hierárquicas abaixo dessas unidades, incluindo Engenharia, GEAUT, GELOG, GEMED, GETEC, Inovacao, Suporte, Tecnologia, ADM, Backoffice, Financeiro, GSTI, Pessoas e SGI, entre outras.

Decisão funcional aprovada:
- a aplicação não precisa exibir nem disponibilizar todas as OUs existentes no domínio;
- deverá existir uma configuração na área de Administração para definir explicitamente quais OUs estarão disponíveis/permitidas para uso pelo Cadastro de Colaboradores;
- a OU deve ser persistida/configurada pelo seu `DistinguishedName`, evitando ambiguidade entre OUs de mesmo nome em caminhos diferentes (exemplo observado: existem duas OUs chamadas `SSA` em ramos distintos);
- nenhuma implementação dessa configuração foi realizada nesta etapa; somente a decisão foi registrada.

Próxima validação antes de implementar leitura real de OU:
- identificar a identidade efetiva do Application Pool usado pela aplicação no IIS `10.1.2.21`;
- depois validar a leitura LDAP sob o contexto adequado dessa identidade, sem alterar configuração do servidor.

## 2026-09-23 - Resultado da identificação do Application Pool no IIS

Contexto:
- comando executado no servidor IIS `10.1.2.21`;
- levantamento realizado somente por leitura usando o módulo `WebAdministration`;
- nenhum site, Application Pool, identidade ou configuração do IIS foi alterado.

Resultado relevante para o projeto:
- site: `CadastroColaboradores`;
- caminho físico: `C:\Automind.CadastroColaboradores`;
- Application Pool: `CadastroColaboradores`;
- tipo de identidade: `ApplicationPoolIdentity`.

Interpretação operacional confirmada em documentação oficial Microsoft:
- o worker process do Application Pool executa sob a conta virtual `IIS AppPool\CadastroColaboradores`;
- quando um Application Pool configurado como `ApplicationPoolIdentity` acessa recursos de rede, o acesso é realizado usando a conta de máquina do servidor no domínio (`DOMINIO\NOMEDAMAQUINA$`);
- consequentemente, o teste anterior de LDAP executado com o usuário administrativo do responsável comprova conectividade e leitura sob aquele usuário, mas ainda não comprova que a identidade usada pela aplicação consegue realizar a mesma leitura do Active Directory.

Próxima etapa de diagnóstico:
- identificar formalmente o nome/domínio da máquina `10.1.2.21` e a respectiva conta de computador;
- verificar se existe uma forma já disponível no servidor de executar um processo de diagnóstico sob o contexto da máquina/Application Pool, sem alterar configuração do IIS;
- somente depois executar uma consulta LDAP de leitura sob esse contexto e registrar o resultado;
- nenhuma alteração de identidade do Application Pool está autorizada nesta etapa.

## 2026-09-23 - Identificação da conta de máquina e disponibilidade do PsExec

Contexto:
- testes executados no servidor IIS `10.1.2.21`;
- nenhum software foi instalado e nenhuma configuração do servidor foi alterada;
- a validação anterior já havia confirmado que o site `CadastroColaboradores` usa `ApplicationPoolIdentity`.

Resultado da associação ao domínio:
- nome da máquina: `SV052022-6121`;
- domínio: `automind.com.br`;
- `PartOfDomain`: `True`;
- conta de máquina esperada no domínio: `automind.com.br\\SV052022-6121$`;
- portanto, para acessos de rede feitos por um processo sob `ApplicationPoolIdentity`, a identidade de domínio que precisa ser considerada nos próximos diagnósticos é a conta de computador `SV052022-6121$`.

Resultado da verificação do PsExec:
- `Get-Command PsExec.exe` não retornou resultado no servidor `10.1.2.21`;
- o PsExec não deve ser instalado apenas para esta investigação sem autorização explícita do responsável;
- também não deve ser criada tarefa agendada, alterada a identidade do Application Pool ou introduzido outro mecanismo temporário de execução como SYSTEM sem autorização prévia.

Próxima etapa definida:
- realizar, diretamente no Active Directory e somente por leitura, a confirmação do objeto de computador `SV052022-6121` e de seus grupos de segurança;
- usar esses resultados para entender o contexto de autorização da conta de máquina antes de qualquer teste adicional de permissão LDAP;
- se for necessária alguma ação no AD, informar explicitamente o servidor e o comando antes da execução;
- nenhuma alteração de permissões, ACLs, grupos ou objetos do AD está autorizada nesta etapa.

## 2026-09-23 - Resultado da validação da conta de computador no Active Directory

Contexto:
- testes executados diretamente no Active Directory pelo responsável;
- comandos utilizados somente para leitura do objeto de computador e de suas associações de grupo;
- nenhuma ACL, associação de grupo, objeto, permissão ou configuração do AD foi alterada.

Objeto de computador confirmado:
- `Name`: `SV052022-6121`;
- `SamAccountName`: `SV052022-6121$`;
- `Enabled`: `True`;
- `PrimaryGroupID`: `515`;
- `DistinguishedName`: `CN=SV052022-6121,CN=Computers,DC=automind,DC=com,DC=br`.

Associação de grupos confirmada:
- a conta de computador retornou somente associação explícita a `Domain Computers`;
- `Domain Computers` é grupo `Global` da categoria `Security`;
- não foi identificada associação explícita adicional da conta de máquina a grupos administrativos ou específicos da aplicação.

Interpretação operacional para os próximos testes:
- a identidade de rede do Application Pool continua sendo considerada a conta de computador `AUTOMIND\\SV052022-6121$`, conforme o comportamento documentado de `ApplicationPoolIdentity` no IIS;
- a confirmação de existência e associação a `Domain Computers` não será tratada isoladamente como prova de acesso efetivo às OUs;
- antes de qualquer alteração de permissão, grupo ou identidade, deve ser verificado por leitura o ACL/permissões aplicáveis no AD para os contêineres/OUs que o sistema utilizará;
- como o PsExec não está disponível no servidor IIS, não será instalada ferramenta nem criado mecanismo temporário de execução como SYSTEM sem consentimento explícito.

Referência funcional mantida:
- as OUs disponíveis para o Cadastro de Colaboradores serão definidas na área de Administração;
- o sistema deverá trabalhar com o `DistinguishedName` completo das OUs autorizadas;
- nenhuma implementação dessa funcionalidade foi iniciada.

## 2026-09-23 - ACL de leitura da OU raiz Automind

Contexto:
- teste executado diretamente no Active Directory pelo responsável;
- objeto verificado: `OU=Automind,DC=automind,DC=com,DC=br`;
- consulta realizada somente por leitura via `Get-Acl` no provider `AD:`;
- nenhuma ACL, permissão, grupo ou objeto do Active Directory foi alterado.

Resultado confirmado:
- foi localizada uma ACE para `NT AUTHORITY\Authenticated Users`;
- `AccessControlType`: `Allow`;
- `ActiveDirectoryRights`: `GenericRead`;
- `IsInherited`: `False`;
- `InheritanceType`: `None`;
- `ObjectType` e `InheritedObjectType`: GUID nulo/genérico.

Interpretação operacional:
- a OU raiz `Automind` concede explicitamente leitura genérica ao grupo de segurança `Authenticated Users`;
- contas de computador autenticadas fazem parte de `Authenticated Users`, portanto a conta `SV052022-6121$` está incluída nesse principal quando autenticada no domínio;
- entretanto, como a ACE retornada possui `InheritanceType=None`, ela é efetiva para a própria OU `Automind` e não deve ser presumida como herdada automaticamente pelas OUs filhas;
- por esse motivo, a permissão de leitura das OUs filhas deve ser verificada antes de concluir que o Application Pool poderá navegar por toda a árvore configurável;
- não será concedida nova permissão nem alterada herança nesta etapa.

Próxima etapa definida:
- verificar somente por leitura as ACLs de OUs filhas representativas dentro da árvore `Automind`;
- confirmar se `Authenticated Users`, `Domain Computers` ou a conta `SV052022-6121$` possuem leitura efetiva nessas OUs;
- somente após esses resultados decidir o próximo teste; nenhuma implementação ou alteração de AD está autorizada.

## 2026-09-23 - ACLs das OUs filhas e decisão de identidade técnica para o AD

Contexto dos testes:
- os comandos de inspeção de ACL foram executados diretamente no Active Directory pelo responsável do projeto;
- a sessão no controlador de domínio usa a conta pessoal do responsável, que possui privilégios elevados;
- portanto, esses comandos servem para inspecionar as ACLs configuradas nos objetos do AD, e não como evidência de que a aplicação executa com a conta pessoal do responsável;
- ficou reafirmado que a aplicação NÃO utilizará a conta pessoal administrativa do responsável para realizar operações no Active Directory.

Resultados das ACLs verificadas:

1. `OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`
- `AreAccessRulesProtected` / herança bloqueada: `False`;
- ACE encontrada para `NT AUTHORITY\Authenticated Users`;
- `AccessControlType`: `Allow`;
- `ActiveDirectoryRights`: `GenericRead`;
- `IsInherited`: `False`;
- `InheritanceType`: `None`.

2. `OU=Engenharia,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`
- `AreAccessRulesProtected` / herança bloqueada: `False`;
- ACE encontrada para `NT AUTHORITY\Authenticated Users`;
- `AccessControlType`: `Allow`;
- `ActiveDirectoryRights`: `GenericRead`;
- `IsInherited`: `False`;
- `InheritanceType`: `None`.

Conclusão factual desta rodada:
- as três OUs já verificadas (`Automind`, `03.UDN` e `Engenharia`) possuem ACE explícita de `GenericRead` para `Authenticated Users`;
- os resultados não dependem da associação da conta de máquina a um grupo administrativo específico, pois a ACL observada é destinada a `Authenticated Users`;
- não foi concedida, removida ou alterada nenhuma permissão;
- não foi alterada herança de ACL;
- ainda não foi executada operação de escrita no AD em nome da aplicação.

Decisão arquitetural registrada para as futuras interações com o Active Directory:
- a conta pessoal administrativa do responsável não será usada pela aplicação;
- as permissões da aplicação devem ser atribuídas a um GRUPO DE SEGURANÇA dedicado no Active Directory, e não diretamente a uma conta pessoal;
- o grupo de segurança será o ponto de delegação das permissões mínimas necessárias nas OUs autorizadas pela Administração do sistema;
- uma identidade técnica da aplicação deverá ser membro desse grupo;
- o grupo de segurança não substitui a identidade que autentica no AD: grupo é mecanismo de autorização/delegação de permissões; a autenticação deve continuar sendo feita por um principal de segurança (conta de computador, conta de serviço gerenciada ou outra identidade técnica aprovada);
- nenhuma criação de grupo, conta de serviço, gMSA ou alteração de Application Pool está autorizada neste momento. Primeiro serão concluídos os testes e levantamentos necessários.

Alternativas técnicas mantidas para decisão posterior, sem implementação nesta etapa:

A. Manter `ApplicationPoolIdentity` e usar a conta de computador `AUTOMIND\SV052022-6121$` como identidade de rede.
- adicionar a conta de computador a um grupo de segurança dedicado da aplicação;
- delegar ao grupo somente as permissões necessárias nas OUs aprovadas;
- vantagem: não exige senha de serviço administrada manualmente e aproveita a identidade de máquina já usada pelo IIS para acesso de rede;
- limitação: a identidade fica vinculada ao servidor `SV052022-6121`.

B. Usar uma Group Managed Service Account (gMSA) dedicada ao CadastroColaboradores.
- configurar o Application Pool para executar com uma identidade gerenciada do domínio;
- adicionar a gMSA ao grupo de segurança dedicado da aplicação;
- delegar ao grupo somente as permissões necessárias;
- vantagem: identidade própria da aplicação, senha administrada automaticamente pelo domínio e possibilidade de uso controlado em mais de um servidor;
- requer validação prévia dos requisitos do domínio/KDS e alteração explícita da identidade do Application Pool, portanto não será feita sem autorização.

Diretriz preferida para o desenho de permissões:
- `Identidade técnica da aplicação` -> membro de -> `Grupo de segurança dedicado` -> recebe delegação nas -> `OUs autorizadas`;
- não delegar permissões diretamente à conta pessoal do responsável;
- não usar grupos administrativos amplos como `Domain Admins` para a aplicação;
- separar permissões de leitura das futuras permissões de criação/alteração de usuários e grupos;
- aplicar princípio do menor privilégio e restringir operações às OUs configuradas na área de Administração.

Referências oficiais consideradas:
- Microsoft Learn - Active Directory Security Groups: recomenda atribuir permissões a grupos de segurança em vez de usuários individuais;
- Microsoft Learn - Application Pool Identities: ApplicationPoolIdentity usa a conta de máquina no acesso a recursos de rede;
- Microsoft Learn - Service Accounts / Group Managed Service Accounts: gMSA fornece identidade de domínio gerenciada, rotação automática de senha e é suportada por Application Pools do IIS.

Próxima etapa antes de qualquer alteração:
- verificar se o domínio possui infraestrutura/requisitos para gMSA e identificar se já existe padrão corporativo de conta/grupo para aplicações IIS;
- comparar essa opção com a manutenção da conta de computador `SV052022-6121$` como identidade de rede;
- nenhuma criação ou alteração será feita até decisão e autorização explícita do responsável.

## 2026-09-23 - Resultado dos pré-requisitos de gMSA e decisão de não alterar infraestrutura nesta etapa

Contexto:
- testes executados diretamente no Active Directory pelo responsável, usando sua conta administrativa elevada;
- os comandos foram somente de leitura;
- nenhuma chave KDS, gMSA, grupo, ACL, nível funcional, Application Pool ou configuração de servidor foi criada ou alterada.

Resultados obtidos:
- domínio: `automind.com.br`;
- nível funcional do domínio: `Windows2016Domain`;
- floresta raiz: `automind.com.br`;
- nível funcional da floresta: `Windows2008R2Forest`;
- `Get-KdsRootKey` não retornou nenhuma chave existente.

Interpretação baseada na documentação oficial Microsoft consultada em 23/09/2026:
- uma KDS Root Key é necessária para o gerenciamento seguro das senhas de gMSA;
- a documentação geral atual de gerenciamento de gMSA orienta que domínio e floresta estejam em Windows Server 2012 ou posterior para suporte dos recursos gMSA em todos os dispositivos;
- o domínio atende esse requisito de nível funcional (`Windows2016Domain`), porém a floresta está em `Windows2008R2Forest`;
- além disso, não existe KDS Root Key detectada no ambiente;
- por isso, a adoção de gMSA não será tratada como mudança simples da aplicação: exigiria avaliação de infraestrutura de AD e, potencialmente, alterações de escopo corporativo;
- nenhuma criação de KDS Root Key e nenhuma elevação de nível funcional está autorizada neste momento.

Direção técnica mantida para investigação:
- continuar avaliando como opção de menor impacto `ApplicationPoolIdentity` + conta de máquina `AUTOMIND\\SV052022-6121$` + grupo de segurança dedicado para delegação de permissões;
- antes de criar qualquer grupo ou alterar associação, levantar o padrão já existente no domínio para grupos de aplicações/serviços e verificar se já existem contas de serviço gerenciadas ou convenções corporativas que devam ser reutilizadas;
- qualquer futura criação de grupo, inclusão da conta de máquina, delegação de ACL ou mudança no IIS depende de autorização explícita do responsável.

Próximos testes planejados, somente leitura:
- identificar versões dos controladores de domínio;
- verificar se existem contas de serviço gerenciadas no domínio;
- inspecionar o padrão de nomenclatura/localização dos grupos de segurança existentes destinados a aplicações/serviços.

## 2026-09-23 - DCs Windows Server 2022, contas gerenciadas existentes e correção da análise de gMSA

Contexto:
- testes executados diretamente no Active Directory pelo responsável, usando sua conta com privilégios elevados;
- todos os comandos desta rodada foram somente de leitura;
- nenhuma conta, grupo, ACL, KDS Root Key, nível funcional, configuração de IIS ou servidor foi criada ou alterada.

Resultados confirmados:

1. Controladores de domínio encontrados:
- `SV062022-6158.automind.com.br` - `10.1.2.1` - Windows Server 2022 Standard, versão `10.0 (20348)`, Site `Site-Salvador`, Global Catalog `True`;
- `SV062022-9947.automind.com.br` - `10.1.3.97` - Windows Server 2022 Standard, versão `10.0 (20348)`, Site `Site-Salvador`, Global Catalog `True`.

2. Contas de serviço gerenciadas existentes:
- `ADSyncMSA82bf3` / `ADSyncMSA82bf3$` - habilitada - `msDS-ManagedServiceAccount` (MSA);
- `provAgentgMSA` / `pGMSA_c5fa29e8$` - habilitada - `msDS-GroupManagedServiceAccount` (gMSA);
- ambas estão no contêiner padrão `CN=Managed Service Accounts,DC=automind,DC=com,DC=br`.

3. Pesquisa de grupos em `OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br`:
- não retornou nenhum grupo;
- portanto, essa OU não deve ser presumida como o local corporativo atual de grupos de segurança sem investigação adicional.

Correção de conclusão anterior:
- a conclusão anterior de que a adoção de gMSA estaria inviabilizada apenas por `ForestMode=Windows2008R2Forest` e pelo retorno vazio de `Get-KdsRootKey` NÃO deve ser usada como conclusão definitiva;
- existe uma gMSA real habilitada no domínio (`provAgentgMSA`), o que comprova que objetos gMSA já foram provisionados nesse ambiente;
- o retorno vazio de `Get-KdsRootKey` continua sendo um dado relevante, mas precisa ser investigado diretamente no Configuration Naming Context e confrontado com a configuração da gMSA existente antes de qualquer conclusão sobre a infraestrutura KDS;
- a documentação Microsoft atual orienta KDS Root Key para geração de senhas de gMSA e recomenda níveis funcionais de domínio e floresta Windows Server 2012 ou posteriores para suporte de recursos gMSA em todos os dispositivos; documentação Microsoft histórica de Windows Server 2012 registrava ausência de requisito de nível funcional para gMSA. Como o ambiente real possui uma gMSA com floresta 2008 R2, a decisão será baseada nos objetos/configuração reais e não em inferência isolada pelo nível funcional.

Decisão mantida:
- NÃO criar KDS Root Key;
- NÃO elevar nível funcional de floresta/domínio;
- NÃO criar nova gMSA;
- NÃO alterar a gMSA existente;
- NÃO criar grupo ainda;
- NÃO alterar o Application Pool;
- primeiro verificar a configuração da gMSA existente, a presença física de objetos KDS no Configuration Naming Context e a localização/padrão dos grupos de segurança usados no domínio.

Próxima rodada planejada, somente leitura e no máximo três testes:
- inspecionar propriedades relevantes de `provAgentgMSA`, especialmente os principals autorizados a recuperar sua senha;
- consultar diretamente os objetos KDS em `CN=Master Root Keys,CN=Group Key Distribution Service,CN=Services,CN=Configuration,DC=automind,DC=com,DC=br`;
- localizar em quais contêineres/OUs estão os grupos de segurança não padrão do domínio para identificar a convenção corporativa antes de propor um grupo dedicado ao CadastroColaboradores.

## 2026-09-23 - KDS Root Key confirmada diretamente no AD, gMSA existente identificada pelo sAMAccountName e localização dos grupos de segurança

Contexto desta rodada:
- testes executados diretamente no Active Directory pelo responsável, usando sua conta administrativa elevada;
- todos os comandos foram somente de leitura;
- nenhuma conta, grupo, ACL, KDS Root Key, nível funcional, Application Pool, serviço ou configuração de servidor foi criado ou alterado;
- permanece válida a regra de que a conta administrativa pessoal do responsável NÃO será utilizada pela aplicação.

Resultados recebidos:

1. Tentativa de consulta da gMSA pelo valor `provAgentgMSA`:
- comando: `Get-ADServiceAccount -Identity "provAgentgMSA" ...`;
- resultado: `ADIdentityNotFoundException`;
- o objeto não deve ser considerado ausente, pois a busca anterior com `Get-ADServiceAccount -Filter *` já retornou:
  - Name: `provAgentgMSA`;
  - SamAccountName: `pGMSA_c5fa29e8$`;
  - ObjectClass: `msDS-GroupManagedServiceAccount`;
  - DistinguishedName: `CN=provAgentgMSA,CN=Managed Service Accounts,DC=automind,DC=com,DC=br`.

Correção técnica baseada na documentação oficial Microsoft:
- o parâmetro `-Identity` de `Get-ADServiceAccount` aceita DN, GUID, SID ou `sAMAccountName`;
- `Name` não é listado como identificador aceito;
- portanto, a próxima consulta da gMSA deve usar `pGMSA_c5fa29e8$` ou o DistinguishedName completo, e não `provAgentgMSA` isoladamente;
- esse erro não representa falha da gMSA nem ausência do objeto.

2. Verificação direta da infraestrutura KDS no Configuration Naming Context:
- contêiner consultado: `CN=Master Root Keys,CN=Group Key Distribution Service,CN=Services,CN=Configuration,DC=automind,DC=com,DC=br`;
- foi encontrado um objeto KDS Root Key real:
  - Name: `74b88b84-4f70-b3b4-a399-127cf4723c34`;
  - ObjectClass: `msKds-ProvRootKey`;
  - whenCreated: `12/04/2023 19:03:39`;
  - msKds-DomainID: `CN=SV062022-9947,OU=Domain Controllers,DC=automind,DC=com,DC=br`.

Conclusão factual sobre KDS:
- existe KDS Root Key no Active Directory;
- o retorno vazio anterior de `Get-KdsRootKey` não deve ser usado como evidência de inexistência da chave;
- nenhuma nova chave deve ser criada;
- a infraestrutura já contém ao menos uma chave raiz KDS criada em 2023;
- antes de qualquer uso de gMSA pelo CadastroColaboradores, ainda será verificado quem está autorizado a recuperar a senha da gMSA existente e como esse padrão foi implementado no ambiente.

3. Distribuição dos grupos de segurança no domínio:
- `OU=06.Grupos-Gerais,OU=Automind,...` continua sem grupos;
- os grupos de segurança reais estão distribuídos por várias localizações;
- maiores concentrações encontradas:
  - 41 em `CN=Users,DC=automind,DC=com,DC=br`;
  - 28 em `CN=Builtin,DC=automind,DC=com,DC=br`;
  - 26 em `OU=Operacoes,OU=Automind,DC=automind,DC=com,DC=br`;
  - 18 em `OU=Automind,DC=automind,DC=com,DC=br`;
  - 12 em `OU=SGI,OU=04.UDA,OU=Automind,...`;
  - 11 em `OU=Backoffice,OU=04.UDA,OU=Automind,...`;
  - 9 em `OU=Acessos 2030,OU=Operacoes,OU=Automind,...`;
  - demais grupos distribuídos entre Financeiro, Engenharia, Groups/MyBusiness, Suporte, Tecnologia, Inovacao, GEMED, SGSI, GSTI, Pessoas, Desenvolvimento, ADM, PMO, Diretoria, Outros e GELOG.

Conclusão sobre grupos:
- não existe evidência de um único contêiner corporativo centralizado para todos os grupos de segurança;
- não deve ser criado grupo novo em `06.Grupos-Gerais` apenas pelo nome da OU;
- antes de propor local/nome para um grupo dedicado ao CadastroColaboradores, é necessário inspecionar grupos existentes relacionados a aplicações, serviços, IIS, desenvolvimento ou operações e identificar a convenção já adotada.

Direção arquitetural ainda em avaliação, sem implementação:
- opção A: `ApplicationPoolIdentity` -> conta de computador `AUTOMIND\\SV052022-6121$` -> grupo de segurança dedicado -> permissões mínimas nas OUs autorizadas;
- opção B: gMSA dedicada ao CadastroColaboradores -> grupo de segurança dedicado -> permissões mínimas nas OUs autorizadas;
- o grupo permanece como camada de autorização/delegação; a identidade técnica continua sendo uma conta de computador ou conta de serviço gerenciada;
- não reutilizar a gMSA `provAgentgMSA` sem conhecer sua finalidade e configuração;
- não reutilizar grupo existente sem comprovar sua finalidade;
- não conceder permissões diretamente ao usuário administrativo do responsável.

Referências oficiais verificadas nesta rodada:
- Microsoft Learn `Get-ADServiceAccount`: o parâmetro `Identity` aceita Distinguished Name, GUID, SID ou sAMAccountName;
- Microsoft Learn `Manage Group Managed Service Accounts`: `PrincipalsAllowedToRetrieveManagedPassword` controla quais computadores/grupos podem recuperar a senha da gMSA; `Test-ADServiceAccount` valida no host se a recuperação é permitida;
- Microsoft Learn `Create a Key Distribution Service (KDS) Root Key`: controladores de domínio utilizam KDS Root Key para gerar senhas de gMSA.

Próxima rodada planejada, somente leitura e no máximo três testes:
- consultar `pGMSA_c5fa29e8$` pelo sAMAccountName e inspecionar `PrincipalsAllowedToRetrieveManagedPassword`, DNSHostName, intervalo de senha, SPNs e PasswordLastSet;
- identificar grupos existentes cujo nome/descrição indiquem uso por aplicações, serviços, IIS ou automações, para entender o padrão corporativo;
- não instalar, criar ou alterar nenhum componente até a conclusão desses testes e autorização explícita do responsável.

---

## CHECKPOINT-V003-AD-USUARIOS-GRUPOS.md

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

---

## CHECKPOINT-V004-M365-DIAGNOSTICO.md

# CHECKPOINT V004 - Microsoft 365 / Entra: diagnostico inicial

> Versao corrente da investigacao Microsoft 365 / Entra. Nao contem alteracoes de codigo, IIS ou servidor; somente resultados de testes e decisoes.

## 2026-09-23 - Microsoft 365 / Entra - conectividade do servidor 10.1.2.21 e inventario do Graph PowerShell

Contexto:
- testes executados no servidor IIS/aplicacao `10.1.2.21` (`SV052022-6121`), nao no controlador de dominio;
- objetivo desta rodada: validar apenas conectividade de saida para Microsoft Graph/Entra e verificar se o Microsoft Graph PowerShell SDK ja estava instalado;
- nenhum modulo foi instalado e nenhuma configuracao do servidor foi alterada.

Resultados:
1. `Test-NetConnection graph.microsoft.com -Port 443`:
   - `RemoteAddress=40.126.45.29`;
   - `RemotePort=443`;
   - `InterfaceAlias=NIC TEAM`;
   - `SourceAddress=10.1.2.21`;
   - `TcpTestSucceeded=True`.
2. `Test-NetConnection login.microsoftonline.com -Port 443`:
   - `RemoteAddress=20.190.173.66`;
   - `RemotePort=443`;
   - `InterfaceAlias=NIC TEAM`;
   - `SourceAddress=10.1.2.21`;
   - `TcpTestSucceeded=True`.
3. inventario do PowerShell/Graph:
   - Windows PowerShell: `5.1.17763.8146`;
   - nenhum resultado para `Microsoft.Graph.Authentication`;
   - nenhum resultado para `Microsoft.Graph.Identity.DirectoryManagement`;
   - nenhum resultado para `Connect-MgGraph`;
   - nenhum resultado para `Get-MgSubscribedSku`;
   - conclusao factual: Microsoft Graph PowerShell SDK nao esta instalado/disponivel nesse servidor na consulta realizada.

Validacao em documentacao oficial Microsoft consultada apos os testes:
- PowerShell 7+ e recomendado para Microsoft Graph PowerShell SDK;
- Windows PowerShell 5.1 permanece suportado, desde que os pre-requisitos estejam presentes, incluindo .NET Framework 4.7.2+; a documentacao tambem pede PowerShellGet atualizado e Execution Policy `RemoteSigned` ou menos restritiva para uso do SDK;
- para `GET /subscribedSkus`, a permissao minima documentada para acesso Application/App-only e `LicenseAssignment.Read.All`;
- `LicenseAssignment.Read.All` em modo Application exige consentimento administrativo;
- a aplicacao futura nao deve depender da conta administrativa pessoal do responsavel; autenticacao tecnica/App-only continua sendo a direcao preferida para o bloco Microsoft 365, sujeita a validacao e aprovacao antes de criar App Registration, certificado ou segredo.

Decisao operacional mantida:
- nao instalar `Microsoft.Graph`, `PowerShellGet`, PowerShell 7 ou qualquer outro componente sem consentimento explicito;
- nao criar App Registration, segredo, certificado ou conceder permissao no Entra sem consentimento explicito;
- antes de propor instalacao, validar por leitura os pre-requisitos existentes no `10.1.2.21` e identificar o tenant via metadados publicos OIDC, sem autenticacao.

Proxima rodada de testes prevista, somente leitura e no maximo 3 comandos:
1. consultar versao do .NET Framework instalada;
2. consultar PowerShellGet/PackageManagement, repositorios e Execution Policy, sem alterar nada;
3. consultar o documento OIDC publico para `automind.com.br` e identificar `issuer`/`token_endpoint`/Tenant ID, sem login.

## 2026-09-23 - Microsoft 365 / Entra - pre-requisitos locais e falha HTTPS no Windows PowerShell 5.1

Contexto:
- testes executados no servidor IIS/aplicacao `10.1.2.21` (`SV052022-6121`);
- nenhum componente foi instalado e nenhuma configuracao persistente foi alterada;
- objetivo: validar pre-requisitos locais para acesso ao Microsoft Graph e ao tenant Entra.

Resultados:
1. .NET Framework:
   - `Version=4.7.03190`;
   - `Release=461814`;
   - conforme tabela oficial Microsoft, `Release 461814` corresponde ao .NET Framework 4.7.2 em sistemas operacionais aplicaveis;
   - portanto o requisito minimo .NET Framework 4.7.2 para Windows PowerShell 5.1 esta atendido.
2. PowerShellGet / PackageManagement / repositorios / Execution Policy:
   - `PowerShellGet 1.0.0.1` em `C:\Program Files\WindowsPowerShell\Modules\PowerShellGet\1.0.0.1\PowerShellGet.psd1`;
   - `PackageManagement 1.0.0.1` em `C:\Program Files\WindowsPowerShell\Modules\PackageManagement\1.0.0.1\PackageManagement.psd1`;
   - `Get-PSRepository` nao conseguiu obter a lista de providers e retornou `Unable to download the list of available providers` e `Unable to find module repositories`;
   - `LocalMachine=RemoteSigned`; demais escopos `Undefined`;
   - nenhuma alteracao foi feita na Execution Policy ou nos repositorios.
3. consulta OIDC publica via `Invoke-RestMethod`:
   - `https://login.microsoftonline.com/automind.com.br/v2.0/.well-known/openid-configuration` falhou com `The underlying connection was closed: An unexpected error occurred on a send`;
   - por isso `Issuer`, `AuthorizationEndpoint`, `TokenEndpoint` e `TenantId` ficaram vazios;
   - como `Test-NetConnection login.microsoftonline.com -Port 443` ja havia retornado sucesso, a falha atual esta acima da camada TCP e precisa de diagnostico de TLS/HTTPS antes de qualquer instalacao.

Validacao em documentacao oficial Microsoft apos os testes:
- a PowerShell Gallery exige TLS 1.2 ou superior;
- a Microsoft orienta explicitamente habilitar TLS 1.2 na sessao do Windows PowerShell 5.1 antes de usar a PowerShell Gallery;
- PowerShellGet v1.x nao e mais suportado; o ambiente possui `PowerShellGet 1.0.0.1`;
- nenhuma atualizacao sera feita sem autorizacao explicita.

Proxima rodada de diagnostico, somente leitura ou alteracao temporaria limitada ao processo atual do PowerShell:
1. consultar o valor atual de `[Net.ServicePointManager]::SecurityProtocol`;
2. consultar configuracao persistente de TLS 1.2 Client no SCHANNEL via registro, sem alterar o registro;
3. testar novamente o endpoint OIDC forçando TLS 1.2 apenas na sessao/processo atual e restaurando o valor original ao final; nenhuma configuracao do servidor sera persistida.

## 2026-09-23 - Microsoft 365 / Entra - TLS 1.2 presente, falha HTTPS permanece

Contexto:
- testes executados no servidor IIS/aplicacao `10.1.2.21` (`SV052022-6121`);
- nenhum componente foi instalado e nenhuma configuracao persistente foi alterada;
- objetivo: validar se a falha de `Invoke-RestMethod` era causada apenas pela ausencia de TLS 1.2 na sessao ou por configuracao persistente simples do SCHANNEL.

Resultados:
1. `[Net.ServicePointManager]::SecurityProtocol` retornou `Tls, Tls11, Tls12`;
   - portanto TLS 1.2 ja esta disponivel/anunciado no processo do Windows PowerShell 5.1 consultado.
2. a chave `HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client` nao existe no registro;
   - nenhuma chave foi criada ou alterada;
   - ausencia dessa chave nao sera interpretada isoladamente como TLS 1.2 desabilitado, pois o processo ja reporta `Tls12` e a configuracao padrao do Windows pode operar sem subchave explicita.
3. teste do endpoint OIDC Microsoft Entra forçando `[Net.SecurityProtocolType]::Tls12` apenas no processo atual continuou falhando com `The underlying connection was closed: An unexpected error occurred on a send`;
   - ao final do comando o valor original de `SecurityProtocol` foi restaurado;
   - nenhuma configuracao persistente foi alterada.

Conclusao factual desta rodada:
- a hipotese simples de que a sessao falhava apenas por nao oferecer TLS 1.2 nao foi confirmada;
- TCP 443 para `login.microsoftonline.com` e `graph.microsoft.com` continua previamente validado;
- a falha permanece na camada HTTPS/TLS ou em componentes relacionados ao cliente Windows PowerShell/.NET/rede intermediaria e precisa ser isolada por novos testes antes de qualquer alteracao.

Documentacao oficial Microsoft revisada apos os testes:
- PowerShell Gallery exige TLS 1.2 ou superior;
- PowerShellGet 1.0.0.1 do Windows PowerShell 5.1 esta fora de suporte e deve ser atualizado antes de uso produtivo, mas nenhuma atualizacao sera feita sem autorizacao;
- `netsh winhttp show proxy` e um comando oficial somente de consulta para exibir a configuracao WinHTTP;
- eventos `Schannel` do log System podem registrar falhas de handshake/certificado; qualquer habilitacao adicional de logging exigiria alteracao de registro e nao sera feita sem autorizacao.

Proxima rodada de diagnostico prevista, no maximo 3 testes e sem alteracao persistente:
1. consultar configuracao de proxy WinHTTP e WinINET do usuario atual;
2. testar o mesmo endpoint OIDC com `curl.exe`, se o executavel ja existir, para separar o comportamento do cliente curl/Schannel do Windows PowerShell/.NET;
3. capturar a cadeia completa de excecoes/InnerException do `Invoke-WebRequest` ou `Invoke-RestMethod` com TLS 1.2 forçado apenas na sessao.

Regra mantida:
- nao alterar SCHANNEL, proxy, certificados, PowerShellGet, Microsoft.Graph, PowerShell 7, App Registration ou qualquer configuracao do servidor sem consentimento explicito.

## 2026-09-23 - Microsoft 365 / Entra - curl/Schannel OK, falha restrita ao Windows PowerShell/.NET

Contexto:
- testes executados no servidor IIS/aplicacao `10.1.2.21` (`SV052022-6121`);
- nenhum componente foi instalado e nenhuma configuracao persistente foi alterada;
- objetivo: separar problema de rede/proxy/TLS do comportamento do cliente Windows PowerShell 5.1/.NET.

Resultados:
1. Proxy:
   - WinHTTP: `Direct access (no proxy server)`;
   - WinINET do usuario atual: `ProxyEnable=0`, `ProxyServer` vazio, `AutoConfigURL` vazio, `AutoDetect` vazio;
   - portanto nao foi encontrada configuracao explicita de proxy nos escopos consultados.
2. `curl.exe`:
   - o executavel ja existe no servidor;
   - consulta HTTPS a `https://login.microsoftonline.com/automind.com.br/v2.0/.well-known/openid-configuration` com `--tlsv1.2` conectou com sucesso;
   - DNS resolveu `login.microsoftonline.com`;
   - conexao TCP/443 estabelecida;
   - Schannel concluiu a sessao TLS;
   - resposta HTTP `200 OK` recebida do Microsoft Entra;
   - isso confirma que rede, DNS, porta 443 e a pilha TLS/Schannel do Windows conseguem atingir o endpoint.
3. `Invoke-WebRequest` no Windows PowerShell 5.1, com TLS 1.2 forçado apenas na sessao:
   - continuou falhando com `System.Net.WebException: The underlying connection was closed: An unexpected error occurred on a send.`;
   - `WebException.Status=SendFailure`;
   - InnerException: `System.Management.Automation.PSInvalidOperationException: There is no Runspace available to run scripts in this thread.`;
   - a mensagem interna informa que o script block tentado era `$true`.

Conclusao factual desta rodada:
- a falha nao esta em DNS, rota, porta 443 ou capacidade geral do Windows/Schannel de negociar TLS com `login.microsoftonline.com`, pois `curl.exe` obteve `HTTP 200` no mesmo servidor;
- a falha esta restrita ao caminho Windows PowerShell 5.1/.NET usado por `Invoke-WebRequest`/`Invoke-RestMethod` ou a algum estado/configuracao carregado nessa sessao/processo;
- a InnerException envolvendo um script block `$true` justifica verificar se `ServicePointManager.ServerCertificateValidationCallback` foi customizado na sessao ou por perfil/script carregado;
- nenhuma conclusao definitiva sera adotada ate validar esse callback e testar uma nova instancia `powershell.exe -NoProfile`.

Documentacao oficial Microsoft revisada:
- `System.Net.ServicePointManager.ServerCertificateValidationCallback` e o callback usado para validacao personalizada de certificados de servidor e seu valor padrao e `null`;
- `powershell.exe -NoProfile` inicia o Windows PowerShell sem carregar perfis, permitindo isolar configuracoes inseridas por perfil.

Proxima rodada de diagnostico prevista, no maximo 3 testes e sem alteracao persistente:
1. consultar o valor/metadata atual de `[Net.ServicePointManager]::ServerCertificateValidationCallback`;
2. executar o mesmo `Invoke-WebRequest` em um novo `powershell.exe -NoProfile`, sem alterar o processo atual;
3. localizar nos perfis PowerShell existentes qualquer referencia a `ServerCertificateValidationCallback`, `CertificatePolicy` ou script blocks de validacao de certificado, somente leitura.

Regra mantida:
- nao limpar callback, editar perfil, alterar certificado, instalar Microsoft.Graph/PowerShellGet/PowerShell 7 ou mudar qualquer configuracao do servidor sem consentimento explicito.

## 2026-09-23 - Callback de certificado confirmado na sessao; teste NoProfile ainda inconclusivo

### Resultado recebido

No servidor `10.1.2.21`, na sessao atual do Windows PowerShell 5.1:

- `[Net.ServicePointManager]::ServerCertificateValidationCallback` esta **CONFIGURADO**;
- metodo retornado: `Boolean lambda_method(...)`;
- `TargetType`: `System.Runtime.CompilerServices.Closure`;
- o callback portanto nao esta no valor padrao `null` nesta sessao.

O teste iniciado com `powershell.exe -NoProfile` **nao chegou a executar a chamada HTTPS**, pois ocorreu erro de parser no comando aninhado (`Missing type name after '['`). Portanto esse resultado nao pode ser usado para afirmar se uma sessao limpa funciona ou falha.

Os quatro caminhos de profile verificados nao existem:

- `C:\Windows\System32\WindowsPowerShell\v1.0\profile.ps1`;
- `C:\Windows\System32\WindowsPowerShell\v1.0\Microsoft.PowerShell_profile.ps1`;
- `C:\Users\daniel.gusmao.d\Documents\WindowsPowerShell\profile.ps1`;
- `C:\Users\daniel.gusmao.d\Documents\WindowsPowerShell\Microsoft.PowerShell_profile.ps1`.

### Interpretacao permitida neste ponto

- rede, DNS, TCP 443 e Schannel ja foram validados anteriormente pelo `curl.exe` com HTTP 200;
- a falha observada continua restrita ao caminho Windows PowerShell 5.1/.NET;
- existe um callback customizado de validacao de certificado na sessao atual;
- nao ha evidencia de que esse callback venha dos quatro profiles padrao testados;
- ainda falta repetir a verificacao em um processo `powershell.exe -NoProfile` com comando mais simples e sem erro de quoting/parser;
- nao remover, limpar ou substituir o callback sem autorizacao explicita.

### Regra operacional reforcada

Nenhuma instalacao, alteracao de SCHANNEL, limpeza de callback, edicao de profile, instalacao de Microsoft.Graph/PowerShell 7 ou criacao de App Registration deve ocorrer sem consentimento explicito do responsavel.

---

## CHECKPOINT-V005-M365-SESSAO-LIMPA.md

# CHECKPOINT V005 - Microsoft 365 / Entra: sessao PowerShell limpa validada

> Registro somente de diagnostico. Nenhum codigo, IIS, SCHANNEL, perfil, modulo PowerShell, App Registration ou configuracao de servidor foi alterado.

## 2026-09-23 - Resultado do teste em novo powershell.exe -NoProfile

Contexto:
- servidor da aplicacao/IIS: `10.1.2.21` (`SV052022-6121`);
- a sessao PowerShell original possuia `[Net.ServicePointManager]::ServerCertificateValidationCallback` customizado;
- os quatro profiles padrao pesquisados anteriormente nao existiam;
- `curl.exe` ja havia obtido HTTP 200 no endpoint OIDC do Microsoft Entra.

Resultados recebidos:
1. nova instancia `powershell.exe -NoProfile`:
   - `ServerCertificateValidationCallback: NULL`;
   - portanto a nova sessao nasce com o valor padrao, sem o callback customizado observado no processo anterior.
2. chamada HTTPS executada na mesma instancia limpa, com TLS 1.2:
   - `StatusCode: 200`;
   - `StatusDescription: OK`;
   - endpoint consultado: `https://login.microsoftonline.com/automind.com.br/v2.0/.well-known/openid-configuration`.

Conclusoes factuais:
- conectividade HTTP/TLS do servidor `10.1.2.21` com Microsoft Entra esta funcional;
- nao ha evidencia que justifique alterar SCHANNEL, TLS persistente, proxy ou certificados do servidor;
- a falha de `Invoke-WebRequest`/`Invoke-RestMethod` vista anteriormente estava associada ao estado da sessao PowerShell que possuia callback customizado de validacao de certificado;
- a origem exata de quem configurou esse callback na sessao anterior nao foi determinada e nao deve ser inferida sem teste;
- uma sessao limpa consegue consumir o endpoint OIDC normalmente;
- Microsoft Graph PowerShell continua nao instalado e nenhuma instalacao foi autorizada.

## Convencao de nome do pacote entregue

A pedido do responsavel, os ZIPs de continuidade deixam de receber nomes descritivos extensos como `...-checkpoint-M365-runspace-callback.zip`.

Padrao adotado para os pacotes entregues:

`Automind.CadastroColaboradores-AAAA-MM-DD-X.Y.Z.zip`

Versao inicial desta convencao:

`Automind.CadastroColaboradores-23-09-2026-0.0.1.zip`

Para as proximas entregas da mesma linha de trabalho, incrementar a versao do pacote (`0.0.2`, `0.0.3`, ...), preservando a data conforme aplicavel.

Observacao: o versionamento interno de checkpoints (`CHECKPOINT-V001`, `V002`, etc.) continua servindo apenas como historico documental e e independente do nome/versao do ZIP entregue.

## Estado atual do bloco Microsoft 365

Validado ate aqui:
- TCP/443 para `graph.microsoft.com` e `login.microsoftonline.com`;
- HTTPS/OIDC do Entra via `curl.exe` e Windows PowerShell 5.1 em sessao limpa;
- .NET Framework 4.7.2;
- Windows PowerShell 5.1.17763.8146;
- `PowerShellGet 1.0.0.1` e `PackageManagement 1.0.0.1` existentes;
- Microsoft Graph PowerShell SDK ausente.

Ainda nao realizado/autorizado:
- instalacao ou atualizacao de PowerShellGet/Microsoft.Graph/PowerShell 7;
- criacao de App Registration;
- criacao de secret ou certificado;
- concessao de permissao Graph;
- autenticacao App-only;
- consulta autenticada de `/subscribedSkus`.

## Proxima etapa tecnica prevista

Continuar somente com testes de leitura sem instalar componentes:
- obter/confirmar Tenant ID pelo documento OIDC publico usando sessao limpa ou `curl.exe`;
- validar resposta HTTP do endpoint Microsoft Graph (sem token, o retorno esperado e de autenticacao/autorizacao, o que confirma o caminho HTTPS da API);
- somente depois discutir, com consentimento explicito, a identidade de aplicacao necessaria para teste autenticado de licencas.

---

## CHECKPOINT-V006-M365-TENANT-GRAPH.md

# CHECKPOINT V006 - Microsoft 365 / Entra: Tenant ID e endpoint Graph validados

> Registro somente de diagnostico. Nenhum codigo, modulo, App Registration, credencial, permissao Graph, IIS ou configuracao de servidor foi criado ou alterado.

## 2026-09-23 - Resultados recebidos no servidor 10.1.2.21

### Tenant Microsoft Entra

Consulta ao documento publico OIDC executada em sessao PowerShell limpa.

Resultado:
- Issuer: `https://login.microsoftonline.com/9ab05ca8-1779-410b-ae61-82dbd20810f3/v2.0`
- Tenant ID: `9ab05ca8-1779-410b-ae61-82dbd20810f3`
- Token endpoint: `https://login.microsoftonline.com/9ab05ca8-1779-410b-ae61-82dbd20810f3/oauth2/v2.0/token`

Conclusao factual:
- o dominio `automind.com.br` resolve corretamente para o tenant Microsoft Entra acima;
- nenhuma autenticacao de usuario foi necessaria para obter esses metadados publicos.

### Endpoint Microsoft Graph - subscribedSkus

Teste executado sem token:

`GET https://graph.microsoft.com/v1.0/subscribedSkus`

Resultado:
- HTTP Status: `401`.

Interpretacao:
- a comunicacao HTTPS com o endpoint Microsoft Graph esta funcional;
- `/subscribedSkus` exige cabecalho `Authorization: Bearer {token}`;
- portanto HTTP 401 sem token e coerente com um endpoint protegido e nao indica falha de rede;
- a consulta autenticada ainda nao foi executada.

## Permissao minima documentada para a proxima etapa

Para `GET /subscribedSkus`, a permissao menos privilegiada documentada pelo Microsoft Graph e:
- `LicenseAssignment.Read.All`

Ela existe tanto no modelo Delegated quanto no modelo Application. Para o desenho futuro do CadastroColaboradores, a avaliacao continua orientada a identidade tecnica/App-only, sem uso da conta administrativa pessoal do responsavel.

Nenhuma permissao foi concedida ate este checkpoint.

## Convencao do pacote de checkpoint

A pedido do responsavel, enquanto houver apenas atualizacao documental/checkpoint e nenhuma entrega de codigo alterado, o ZIP deve ser identificado explicitamente como checkpoint:

`Automind.CadastroColaboradores-AAAA-MM-DD-checkpoint-vX.Y.Z.zip`

Primeira versao nesse padrao:

`Automind.CadastroColaboradores-23-09-2026-checkpoint-v0.0.1.zip`

Quando houver uma entrega autorizada do projeto completo, o checkpoint deve acompanhar o projeto dentro do pacote completo e o nome do projeto sera versionado separadamente.

## Estado atual do bloco Microsoft 365

Validado:
- saida TCP/443 para Entra e Microsoft Graph;
- HTTPS funcional via `curl.exe` e via Windows PowerShell 5.1 em sessao limpa;
- causa do erro da sessao original isolada a um callback customizado presente naquela sessao;
- Tenant ID confirmado: `9ab05ca8-1779-410b-ae61-82dbd20810f3`;
- endpoint `/v1.0/subscribedSkus` acessivel e retornando HTTP 401 sem token, conforme comportamento de recurso protegido;
- Microsoft Graph PowerShell SDK continua ausente.

Ainda nao realizado/autorizado:
- instalacao/atualizacao de PowerShellGet, Microsoft.Graph ou PowerShell 7;
- criacao de App Registration;
- criacao de secret ou certificado;
- concessao de `LicenseAssignment.Read.All`;
- autenticacao App-only;
- consulta autenticada das licencas/SKUs.

## Proximo passo

Antes de criar qualquer recurso no tenant, definir e validar qual mecanismo de autenticacao sera usado pela aplicacao para Microsoft Graph (preferencialmente identidade tecnica/App-only) e quais pre-requisitos ja existem no tenant. Qualquer criacao ou alteracao depende de consentimento explicito do responsavel.

---

## CHECKPOINT-V007-AD-CRIACAO-OU.md

# CHECKPOINT V007 - Active Directory: preparacao da criacao e selecao de OU

Data: 2026-09-23

## Escopo retomado

- Microsoft 365 / Entra fica pausado nesta etapa.
- O foco volta ao Active Directory, primeiro consolidando leituras e regras reais do ambiente e depois preparando um teste controlado de criacao de usuario.
- Nenhum `New-ADUser`, alteracao de atributo, movimentacao de objeto, inclusao em grupo ou delegacao de permissao esta autorizado sem consentimento explicito do responsavel.

## Politica de senha observada no dominio

Resultado de `Get-ADDefaultDomainPasswordPolicy`:

- `ComplexityEnabled`: `False`;
- `MinPasswordLength`: `8`;
- `PasswordHistoryCount`: `2`;
- `MaxPasswordAge`: `181` dias;
- `MinPasswordAge`: `0`;
- `LockoutThreshold`: `10`;
- `LockoutDuration`: `10` minutos;
- `LockoutObservationWindow`: `10` minutos;
- `ReversibleEncryptionEnabled`: `False`.

Esses valores sao evidencias do estado atual do dominio e nao devem ser alterados pelo CadastroColaboradores.

## Amostra de usuarios criados recentemente

A leitura dos 12 usuarios mais recentes sob `OU=Automind,DC=automind,DC=com,DC=br` confirmou criacoes reais em OUs distintas, incluindo:

- `OU=05.Terceiros-Ext,OU=Automind,...`;
- `OU=Engenharia,OU=03.UDN,OU=Automind,...`;
- `OU=SENSE METRICS,OU=03.UDN,OU=Automind,...`;
- `OU=GSTI,OU=04.UDA,OU=Automind,...`;
- `OU=Pessoas,OU=04.UDA,OU=Automind,...`;
- `OU=Inovacao,OU=03.UDN,OU=Automind,...`.

Evidencia funcional importante:

- OU e `Department` nao sao equivalentes e nao devem ser inferidos automaticamente um do outro;
- exemplos reais de usuarios externos possuem `Department` como `GEMED`, `ENGENHARIA` ou `T&S`, mas ficam fisicamente em `OU=05.Terceiros-Ext,OU=Automind,...`;
- portanto a unidade/OU de destino deve ser uma selecao explicita do fluxo de cadastro, sujeita a uma lista de OUs autorizadas.

## Usuario de referencia `fernando.menezes`

A leitura ampliada confirmou:

- `GivenName`: `Fernando`;
- `Surname`: `Menezes`;
- `DisplayName`: `Fernando Menezes`;
- `sAMAccountName`: `fernando.menezes`;
- `UPN`: `fernando.menezes@automind.com.br`;
- `mail`: `fernando.menezes@automind.com.br`;
- `Title`: `Network Administrator`;
- `Department`: `GSTI`;
- `Company`: `Automind`;
- `Manager`: `CN=Fabio dos Santos Daniel,OU=Suporte,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`;
- `Description`: `Network Administrator`;
- `PhysicalDeliveryOfficeName`: `Salvador`;
- `l`: `Salvador`;
- `st`: `Bahia`;
- `c`: `BR`;
- `PasswordNeverExpires`: `True`;
- `PasswordNotRequired`: `False`;
- `proxyAddresses`: primario `SMTP:fernando.menezes@automind.co`, secundario `smtp:fernando.menezes@automind.com.br`.

`employeeID`, `employeeNumber`, endereco e alguns telefones nao estao obrigatoriamente preenchidos; portanto nao devem ser tratados como obrigatorios apenas por existirem no schema do AD.

## Regra de OU para o CadastroColaboradores

Decisao funcional consolidada:

1. O formulario de cadastro deve possuir uma selecao explicita de **Unidade/OU de destino**.
2. A aplicacao nao deve expor automaticamente todas as OUs do dominio.
3. Uma area de Administracao devera manter a allowlist de OUs permitidas para o sistema.
4. Cada item configurado deve persistir no minimo:
   - nome amigavel para exibicao;
   - `DistinguishedName` completo da OU;
   - status ativo/inativo para uso no cadastro.
5. O identificador tecnico da OU e o `DistinguishedName`, nao o `Name`, pois nomes de OU podem se repetir em ramos diferentes.
6. Na futura criacao, a OU selecionada sera usada como `Path` do `New-ADUser` somente depois das validacoes de seguranca e autorizacao explicita para escrita.
7. `Department`, `Title` e `Manager` permanecem atributos separados da OU de destino.
8. A lista deve privilegiar OUs realmente destinadas a contas de colaboradores e excluir OUs tecnicas, de grupos, controladores, infraestrutura ou outras que nao facam parte do cadastro funcional.

## Proximas validacoes somente leitura

Antes de definir a allowlist inicial de OUs, levantar:

- arvore completa de OUs abaixo de `OU=Automind` com `DistinguishedName` e protecao contra exclusao acidental;
- quantidade de usuarios habilitados diretamente em cada OU, para diferenciar OUs organizacionais/folha de OUs puramente estruturais;
- com base nessas duas evidencias, propor ao responsavel a lista inicial de OUs selecionaveis, sem gravar configuracao ainda.

---

## CHECKPOINT-V008-AD-ALLOWLIST-OU.md

# CHECKPOINT V008 - Active Directory: classificacao inicial de OUs e preparo do teste ponta a ponta

Data: 2026-09-23

## Regra operacional adicional

A partir desta etapa, todo comando ou teste solicitado ao responsavel deve indicar explicitamente o local de execucao:

- **AD**: executar no controlador/ambiente de Active Directory com a identidade indicada;
- **10.1.2.21**: executar no servidor da aplicacao/IIS;
- **maquina do usuario**: executar na estacao local do responsavel.

Quando uma credencial elevada for necessaria, isso deve ser informado explicitamente. Nao assumir que o contexto administrativo usado em testes representa a identidade futura da aplicacao.

## Testes desta rodada

Os dois levantamentos foram executados **no AD, com usuario de nivel elevado**.

### Arvore de OUs abaixo de Automind

Foram identificadas 31 OUs sob `OU=Automind,DC=automind,DC=com,DC=br`, incluindo OUs de estrutura, areas funcionais, terceiros, grupos e operacoes.

O identificador tecnico que deve ser persistido pela aplicacao continua sendo o `DistinguishedName` completo.

### Usuarios habilitados diretamente por OU

Contagens observadas:

- SENSE METRICS: 42
- Engenharia: 25
- T&S: 25
- 05.Terceiros-Ext: 15
- GSTI: 12
- Financeiro: 10
- Backoffice: 9
- Pessoas: 8
- Inovacao: 7
- Suporte: 7
- SPARK LOG: 6
- SYNC AUTOMATION: 6
- Gerencia Produtos: 4
- 07.Outros: 3
- SGI: 3
- ADM: 2
- GECOM: 2
- PMO: 2
- 01.Diretoria: 2

Com zero usuarios habilitados diretamente no momento da consulta:

- Automind
- 06.Grupos-Gerais
- SGSI
- Acessos 2030
- 02.Governanca-PE
- Tecnologia
- Migracao
- Operacoes
- Desenvolvimento
- 04.UDA
- GETEC
- ListaDistribuicao
- 03.UDN

## Interpretacao funcional

A contagem de usuarios e evidência de uso, mas **nao deve ser usada sozinha para autorizar ou excluir uma OU**. Uma OU sem usuarios hoje pode ser uma unidade valida para futuros cadastros; uma OU com usuarios pode conter contas tecnicas ou outros objetos que nao devem participar do fluxo de onboarding.

### Candidatas fortes para OU selecionavel de colaborador

Com base no uso atual e nos exemplos de usuarios reais ja verificados, as seguintes OUs sao candidatas fortes para a allowlist inicial, sujeitas a confirmacao do responsavel:

- `OU=01.Diretoria,OU=Automind,DC=automind,DC=com,DC=br`
- `OU=Engenharia,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`
- `OU=SENSE METRICS,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`
- `OU=T&S,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`
- `OU=Gerencia Produtos,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`
- `OU=Inovacao,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`
- `OU=Suporte,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`
- `OU=GSTI,OU=04.UDA,OU=Automind,DC=automind,DC=com,DC=br`
- `OU=Financeiro,OU=04.UDA,OU=Automind,DC=automind,DC=com,DC=br`
- `OU=Backoffice,OU=04.UDA,OU=Automind,DC=automind,DC=com,DC=br`
- `OU=Pessoas,OU=04.UDA,OU=Automind,DC=automind,DC=com,DC=br`
- `OU=SGI,OU=04.UDA,OU=Automind,DC=automind,DC=com,DC=br`
- `OU=ADM,OU=04.UDA,OU=Automind,DC=automind,DC=com,DC=br`
- `OU=GECOM,OU=04.UDA,OU=Automind,DC=automind,DC=com,DC=br`
- `OU=PMO,OU=SGI,OU=04.UDA,OU=Automind,DC=automind,DC=com,DC=br`
- `OU=05.Terceiros-Ext,OU=Automind,DC=automind,DC=com,DC=br`

### OUs que precisam de confirmacao antes de entrar na allowlist

Nao classificar automaticamente como OU de colaborador ate confirmacao funcional:

- `OU=SPARK LOG,OU=03.UDN,OU=Automind,...`
- `OU=SYNC AUTOMATION,OU=03.UDN,OU=Automind,...`
- `OU=07.Outros,OU=Automind,...`
- `OU=02.Governanca-PE,OU=Automind,...`
- `OU=GETEC,OU=03.UDN,OU=Automind,...`
- `OU=Tecnologia,OU=03.UDN,OU=Automind,...`
- `OU=SGSI,OU=04.UDA,OU=Automind,...`
- `OU=Desenvolvimento,OU=Operacoes,OU=Automind,...`
- `OU=Migracao,OU=Automind,...`

### OUs estruturais/tecnicas que nao devem ser exibidas no combo por padrao

A menos que o responsavel determine expressamente o contrario:

- `OU=Automind,DC=automind,DC=com,DC=br` (raiz organizacional)
- `OU=03.UDN,OU=Automind,...` (container estrutural)
- `OU=04.UDA,OU=Automind,...` (container estrutural)
- `OU=06.Grupos-Gerais,OU=Automind,...` (grupos)
- `OU=ListaDistribuicao,OU=Automind,...` (listas/grupos)
- `OU=Operacoes,OU=Automind,...` (container pai; validar filhos separadamente)
- `OU=Acessos 2030,OU=Operacoes,OU=Automind,...` (nao classificada como destino de colaborador)

## Fluxo futuro de teste ponta a ponta

O responsavel autorizou como estrategia futura, mas **ainda nao autorizou a escrita nesta rodada**, um teste controlado com dados ficticios:

1. criar um chamado ficticio no TOPdesk com os dados de um colaborador de teste;
2. ler o chamado pelo fluxo real do CadastroColaboradores;
3. validar campos e identidade sugerida;
4. selecionar explicitamente a OU autorizada;
5. criar o usuario de teste no AD somente apos nova autorizacao explicita;
6. validar atributos, grupos e posicionamento do objeto;
7. documentar resultado e procedimento de limpeza/desativacao do usuario de teste, se aplicavel.

Esse teste deve ser separado em autorizacoes de escrita independentes para TOPdesk e AD.

## Proximo passo

Antes de qualquer `New-ADUser`, fechar o contrato de criacao: atributos obrigatorios, regra para `sAMAccountName`, UPN, `mail`, senha inicial, estado Enabled, expiracao/alteracao de senha, `Company`, `Description`, localidade, manager, grupos base e tratamento de terceiros. A allowlist de OUs deve ser aprovada pelo responsavel antes de ser implementada.

---

## CHECKPOINT-V009-AD-CONTRATO-USUARIO-GRUPOS.md

# CHECKPOINT V009 - Active Directory: contrato de criacao, interno x terceiro e grupos

Data: 2026-09-23

## Local dos testes desta rodada

Os comandos foram executados **no AD, com usuario de nivel elevado**.

## Objetivo

Comparar um colaborador interno recente com um terceiro recente e observar o comportamento real dos grupos da OU Engenharia antes de definir qualquer regra de criacao no sistema.

Nenhuma escrita no AD foi autorizada ou executada nesta rodada.

## Referencia interna - Gabriel Luis Lima Silva

Objeto analisado: `gabriel.silva`.

Dados observados:

- CN/DisplayName: Gabriel Luis Lima Silva;
- `givenName`: Gabriel;
- `sn`: Luis Lima Silva;
- `sAMAccountName`: `gabriel.silva`;
- UPN: `gabriel.silva@automind.com.br`;
- `mail`: `gabriel.silva@automind.com.br`;
- `proxyAddresses`: secundario `smtp:gabriel.silva@automind.com.br` e primario `SMTP:gabriel.silva@automind.co`;
- Title: Automation Systems Analyst;
- Department: ENGENHARIA;
- Company: Automind;
- Manager: Edson Neto;
- Description: Automation Systems Analyst;
- Office: Salvador;
- TelephoneNumber preenchido;
- `l`: Rio de Janeiro;
- `c`: BR;
- `PasswordNeverExpires=False`;
- `PasswordNotRequired=False`;
- `pwdLastSet` diferente de zero;
- OU: `OU=Engenharia,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`.

Grupos diretos observados no usuario:

- `_GA_E-CLIC`;
- `_Engenharia`;
- `_GAP7`;
- `Dist_Todos_SSA`;
- `Dist_Todos`;
- `Dist_Engenharia`;
- `_Todos`;
- `_Todos SSA`;
- `_Tecnica SSA`.

Conclusao: estes grupos nao devem ser copiados integralmente como padrao apenas por pertencerem ao Gabriel; parte deles pode depender de perfil, projeto ou localidade.

## Referencia terceiro - Ednan Santos Chabi de Sena

Objeto analisado: `ednan.sena.ext`.

Dados observados:

- CN/DisplayName: Ednan Santos Chabi de Sena;
- `givenName`: Ednan;
- `sn`: Santos Chabi de Sena;
- `sAMAccountName`: `ednan.sena.ext`;
- UPN: `ednan.sena.ext@automind.com.br`;
- `mail`: vazio;
- `proxyAddresses`: secundario `smtp:ednan.sena.ext@automind.com.br` e primario `SMTP:ednan.sena.ext@automind.co`;
- Title: Consultor Externo;
- Department: ENGENHARIA;
- Company: Automind;
- Manager: Edson Neto;
- Description: `I2608-0259`;
- Office/telefone/localidade vazios;
- `c`: BR;
- `PasswordNeverExpires=False`;
- `PasswordNotRequired=False`;
- `pwdLastSet=0`;
- OU: `OU=05.Terceiros-Ext,OU=Automind,DC=automind,DC=com,DC=br`;
- grupos diretos: `_Todos SSA` e `GAR - Grupo de Acesso Restrito`.

Conclusao provisoria: o terceiro tem tratamento diferente de um colaborador interno, inclusive OU, sufixo `.ext`, atributos preenchidos e grupos. Uma unica amostra nao e suficiente para transformar todos esses detalhes em regra automatica; validar a coorte de terceiros antes de implementar.

## Frequencia dos grupos na OU Engenharia

Consulta executada sobre usuarios habilitados diretamente na OU Engenharia.

Grupos com maior frequencia no resultado:

- 23: `Dist_Todos`;
- 23: `Dist_Engenharia`;
- 23: `_Todos`;
- 23: `_Todos SSA`;
- 23: `_GA_E-CLIC`;
- 23: `_Engenharia`;
- 22: `Dist_Todos_SSA`;
- 18: `_Tecnica SSA`;
- 7: `_GAP7`;
- demais grupos aparecem em subconjuntos menores.

Observacao importante: o levantamento anterior indicou 25 usuarios habilitados diretamente na OU Engenharia, enquanto a maior frequencia de grupo nesta consulta foi 23. O comando de frequencia so gera linhas para memberships existentes; portanto, antes de definir um conjunto obrigatorio, identificar os usuarios sem memberships ou com divergencias e calcular a intersecao considerando todos os usuarios habilitados.

## Regras que ja podem ser consideradas fortes

Sem implementar ainda:

### Colaborador interno

- OU deve ser selecionada explicitamente a partir da allowlist;
- `sAMAccountName` e UPN precisam ter unicidade validada antes da criacao;
- UPN usa `@automind.com.br`;
- o padrao SMTP corporativo observado e `SMTP:<alias>@automind.co` como primario e `smtp:<alias>@automind.com.br` como secundario;
- `Company=Automind` aparece nas referencias atuais;
- Title, Department e Manager sao atributos separados da OU;
- grupos devem ser derivados por regras comprovadas, nao por clonagem integral de um usuario de referencia.

### Terceiro

- `OU=05.Terceiros-Ext,OU=Automind,DC=automind,DC=com,DC=br` e uma forte referencia atual;
- varios terceiros recentes usam `.ext`, mas a regra de formacao do alias ainda precisa ser validada na coorte completa;
- grupos e atributos diferem dos internos;
- nao assumir `mail` preenchido, apesar de `proxyAddresses` estar presente na referencia;
- `Description` pode guardar identificador de chamado/processo no exemplo, mas isso ainda precisa ser validado em mais objetos.

## Pendencias antes de qualquer New-ADUser

1. Validar todos os terceiros habilitados diretamente em `05.Terceiros-Ext`: padrao `.ext`, atributos, mail/proxy, grupos, manager e `pwdLastSet`.
2. Explicar a diferenca entre os 25 usuarios habilitados na OU Engenharia e a frequencia maxima 23 dos grupos, identificando excecoes reais.
3. Somente depois fechar os grupos base automaticos e quais grupos ficam como sugestao/manual.
4. Continuar sem criar usuario ate autorizacao explicita.

## Teste ponta a ponta futuro

Permanece planejado:

1. chamado TOPdesk ficticio;
2. leitura/validacao pelo CadastroColaboradores;
3. selecao de OU permitida;
4. exibicao do objeto AD que seria criado;
5. autorizacao explicita do responsavel;
6. criacao controlada no AD;
7. validacao e documentacao.

Toda solicitacao de comando deve indicar explicitamente o local: AD, servidor 10.1.2.21 ou maquina do usuario.

---

## CHECKPOINT-V010-AD-PERFIS-GRUPOS-E-TESTE-PONTA-A-PONTA.md

# CHECKPOINT V010 - Active Directory: perfis, grupos e preparacao do teste ponta a ponta

Data: 2026-09-23

## Local dos testes desta rodada

Os comandos desta rodada foram executados **no Active Directory, com usuario de nivel elevado**.

Nenhuma escrita no AD, TOPdesk, IIS ou Microsoft 365 foi realizada.

## Resultado - OU 05.Terceiros-Ext

Foram analisados os 15 usuarios habilitados diretamente em:

`OU=05.Terceiros-Ext,OU=Automind,DC=automind,DC=com,DC=br`

### Nomenclatura `.ext`

- 11 dos 15 objetos usam `sAMAccountName` terminado em `.ext`.
- 4 objetos nao usam `.ext`: `brian.duncan`, `edwin`, `jonathas.medeiros` e `marcio.sales`.
- Os cinco objetos criados em 2026 presentes nesta amostra (`ruan.vitor.ext`, `danilo.costa.ext`, `ednan.sena.ext`, `menegasse.ext` e `paulosouza.ext`) usam `.ext`.

Conclusao:

- `.ext` e uma convencao forte para terceiros atuais, mas nao pode ser inferido apenas pela OU como regra historica absoluta.
- A futura regra do sistema deve distinguir explicitamente o tipo de colaborador, e nao assumir que todo objeto existente em `05.Terceiros-Ext` representa o padrao atual.

### Atributos

A amostra confirma heterogeneidade historica:

- Title, Department, Company, Manager e mail podem estar vazios em contas antigas/atipicas;
- os objetos mais recentes possuem mais consistencia em Title/Department/Manager;
- `proxyAddresses` aparece com o padrao corporativo ja confirmado: `SMTP:<alias>@automind.co` e `smtp:<alias>@automind.com.br`;
- Description e usada de formas diferentes: cargo, identificador de chamado/processo ou combinacao dos dois;
- nao assumir Description como campo de regra sem uma decisao funcional explicita.

### Grupos dos terceiros

Nao existe um conjunto unico de grupos comum aos 15 objetos da OU.

Grupos recorrentes observados incluem:

- `_Todos SSA`;
- `GAR - Grupo de Acesso Restrito`;
- `Dist_Todos_SSA`;
- grupos especificos como `_GAP1`, `_GAP8_Especial_EXT`, `Dist_Brigada`, `Dist_sgl` e outros.

Conclusao:

- a OU de terceiros nao deve definir sozinha os grupos automaticos;
- para terceiros, manter a mesma regra funcional do projeto: comparar usuarios ativos equivalentes por **Title + Department** e calcular a intersecao dos grupos diretos;
- grupos presentes apenas em parte da coorte ficam como excecoes/sugestoes, nunca automaticos apenas por aparecerem em um usuario de referencia.

## Resultado - divergencia dos 25 usuarios da Engenharia explicada

A OU Engenharia possui 25 usuarios habilitados diretamente.

Os dois objetos que nao possuem o nucleo de seis grupos mais frequentes sao:

- `e-clic`;
- `engeplan`.

Ambos possuem Title e Department vazios e apenas um grupo direto na consulta realizada.

Conclusao:

- a diferenca 25 x 23 observada anteriormente e explicada por contas tecnicas/atipicas dentro da mesma OU;
- a aplicacao nao deve usar apenas a OU para inferir perfil humano ou grupos;
- Title + Department continua sendo a chave funcional correta para formar a coorte de comparacao.

## Perfil validado - Automation Systems Analyst + ENGENHARIA

Foram encontrados 5 usuarios habilitados com:

- Title = `Automation Systems Analyst`;
- Department = `ENGENHARIA`.

### Grupos presentes em 5/5

Estes oito grupos formam a intersecao comprovada desta coorte:

1. `_Tecnica SSA`;
2. `Dist_Engenharia`;
3. `_Todos SSA`;
4. `_Todos`;
5. `_Engenharia`;
6. `_GA_E-CLIC`;
7. `Dist_Todos`;
8. `Dist_Todos_SSA`.

Pela regra ja aprovada em `Docs/06-GRUPOS-E-PERFIS.md`, estes sao candidatos a grupos **sugeridos e pre-selecionados** para esse perfil, desde que nao sejam identificados como privilegiados/protegidos em uma verificacao posterior.

### Grupos que nao sao comuns a todos

- `_GAP7`: 4/5;
- `_GAP2`: 1/5;
- `Dist_sustentacao_engenharia`: 1/5;
- `_Internet`: 1/5.

Esses grupos devem aparecer apenas como excecoes/sugestoes e nao ser pre-selecionados pela regra de intersecao.

## Contrato de criacao - pontos ja aprovados/documentados

Conforme `Docs/03-DECISOES.md` e evidencias reais:

- todo cadastro precisa estar vinculado a chamado TOPdesk;
- Nome de guerra contem exatamente dois nomes;
- Nome de guerra gera login e e-mail no formato `nome.sobrenome`, sem acentos;
- UPN: `<alias>@automind.com.br`;
- Company: `Automind`;
- Cargo gravado no AD deve estar em ingles;
- Department permanece atributo separado da OU;
- Manager deve ser resolvido no AD;
- OU deve ser escolhida explicitamente entre as OUs permitidas e persistida pelo DistinguishedName completo;
- telefone so sera gravado no AD quando `Deseja divulgar contato na intranet = Sim`;
- senha inicial futura tera 14 caracteres e nunca sera persistida em banco/log/historico;
- grupos sao derivados por intersecao de usuarios ativos equivalentes por Title + Department;
- grupos privilegiados/protegidos nunca devem ser copiados automaticamente;
- `proxyAddresses` observado no ambiente: primario `SMTP:<alias>@automind.co` e secundario `smtp:<alias>@automind.com.br`.

## Estrategia recomendada para o primeiro teste ponta a ponta

Para reduzir ambiguidade, o primeiro colaborador ficticio deve ser um **colaborador interno** no perfil ja validado:

- Cargo AD / Title: `Automation Systems Analyst`;
- Department: `ENGENHARIA`;
- OU de destino: `OU=Engenharia,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`;
- Manager: um manager real previamente validado, por exemplo Edson Neto, desde que o chamado ficticio seja montado com esse dado;
- grupos base: somente a intersecao 5/5, apos validar categoria/escopo/protecao dos oito grupos;
- grupos de excecao nao devem ser selecionados automaticamente.

O uso de um interno neste primeiro teste evita misturar no mesmo experimento as regras ainda historicamente heterogeneas de terceiros (`.ext`, Description e grupos).

## Antes de qualquer escrita

Ainda faltam apenas validacoes de leitura para o perfil escolhido:

1. consultar categoria/escopo e indicadores relevantes dos oito grupos comuns do perfil `Automation Systems Analyst + ENGENHARIA`;
2. escolher uma identidade ficticia claramente identificavel e validar inexistencia de sAMAccountName, UPN, mail e proxyAddresses;
3. montar a pre-visualizacao completa do objeto que seria criado.

Somente depois disso:

1. criar/usar um chamado TOPdesk ficticio, com autorizacao explicita para a escrita no TOPdesk;
2. importar/revisar os dados no CadastroColaboradores;
3. apresentar o objeto AD final e os grupos;
4. solicitar autorizacao explicita para a escrita no AD;
5. criar o usuario de teste de forma controlada;
6. validar atributos, OU, manager, grupos e senha/estado da conta;
7. documentar e, quando aprovado, remover/desabilitar o usuario de teste conforme procedimento acordado.

## Regra de local dos testes

Toda solicitacao futura deve indicar claramente um dos tres locais:

- **AD**;
- **servidor 10.1.2.21**;
- **maquina do usuario**.

---

## CHECKPOINT-V011-AD-GRUPOS-TRANSITIVOS-E-IDENTIDADE-LIVRE.md

# CHECKPOINT V011 - Active Directory: grupos transitivos e identidade ficticia livre

Data: 2026-09-23

## Local dos testes desta rodada

Os comandos desta rodada foram executados **no Active Directory, com usuario de nivel elevado**.

Nenhuma escrita no AD, TOPdesk, IIS ou Microsoft 365 foi realizada.

## Resultado - metadados dos 8 grupos comuns do perfil

Perfil analisado:

- Title = `Automation Systems Analyst`;
- Department = `ENGENHARIA`;
- coorte previamente validada = 5 usuarios;
- 8 grupos comuns = intersecao 5/5.

### Grupos de seguranca

1. `_Tecnica SSA`
   - GroupCategory: Security;
   - GroupScope: Global;
   - adminCount: vazio;
   - isCriticalSystemObject: vazio;
   - membro direto de `_GAP2` e `_Tecnica`.

2. `_Todos SSA`
   - GroupCategory: Security;
   - GroupScope: Global;
   - adminCount: vazio;
   - isCriticalSystemObject: vazio;
   - sem parent group direto retornado.

3. `_Todos`
   - GroupCategory: Security;
   - GroupScope: Global;
   - adminCount: vazio;
   - isCriticalSystemObject: vazio;
   - sem parent group direto retornado.

4. `_Engenharia`
   - GroupCategory: Security;
   - GroupScope: Global;
   - adminCount: vazio;
   - isCriticalSystemObject: vazio;
   - membro direto de `_JumpServer`.

5. `_GA_E-CLIC`
   - GroupCategory: Security;
   - GroupScope: Global;
   - adminCount: vazio;
   - isCriticalSystemObject: vazio;
   - Description: `#I2604-0053`;
   - sem parent group direto retornado.

### Grupos de distribuicao

6. `Dist_Engenharia`
   - GroupCategory: Distribution;
   - GroupScope: Global;
   - Description: `grupo de distribuicao de e-mails`;
   - membro direto de `Dist_engenharia_suporte` e `Dist_Tecnico_SSA`.

7. `Dist_Todos`
   - GroupCategory: Distribution;
   - GroupScope: Global;
   - Description: `grupo de distribuicao de e-mails`;
   - memberOf retornou `Domain Users`.

8. `Dist_Todos_SSA`
   - GroupCategory: Distribution;
   - GroupScope: Global;
   - Description: `grupo de distribuicao de e-mails`;
   - sem parent group direto retornado.

## Conclusao sobre os grupos

Nenhum dos 8 grupos retornou `adminCount=1` ou `isCriticalSystemObject=True` na consulta realizada.

Porem, a verificacao revelou associacoes indiretas que precisam ser entendidas antes de qualquer escrita:

- `_Engenharia` esta aninhado diretamente em `_JumpServer`;
- `_Tecnica SSA` esta aninhado diretamente em `_GAP2` e `_Tecnica`.

Portanto, nao e suficiente validar somente os atributos do grupo diretamente selecionado. Antes de pre-selecionar automaticamente grupos de seguranca, o sistema/procedimento deve considerar os grupos ancestrais/transitivos e evitar conceder acesso indireto nao compreendido.

Essa observacao nao significa que os grupos estejam errados. Eles aparecem em 5/5 usuarios equivalentes e podem representar a configuracao correta do perfil atual. A pendencia e apenas confirmar o impacto da cadeia de grupos antes do teste real.

Os grupos de distribuicao devem permanecer classificados separadamente dos grupos de seguranca. Eles atendem finalidade de e-mail/distribuicao e nao devem ser tratados como equivalentes a grupos de autorizacao.

## Resultado - identidade ficticia

Identidade proposta:

- CN/Nome: `Teste Provisionamento Automind`;
- alias/sAMAccountName: `teste.provisionamento`;
- UPN: `teste.provisionamento@automind.com.br`;
- SMTP primario candidato: `teste.provisionamento@automind.co`;
- SMTP secundario candidato: `teste.provisionamento@automind.com.br`;
- OU candidata: `OU=Engenharia,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`.

A consulta de colisao nao retornou nenhum objeto para:

- sAMAccountName;
- UPN;
- mail nos dois dominios verificados;
- proxyAddresses SMTP nos dois enderecos;
- CN `Teste Provisionamento Automind` diretamente na OU Engenharia.

Conclusao: **a identidade ficticia esta livre nas verificacoes executadas**.

Nenhum objeto foi criado.

## Estado do primeiro teste ponta a ponta

Ja validado:

- OU de destino;
- perfil Title + Department;
- manager de referencia;
- alias ficticio e ausencia de colisao;
- UPN e enderecos SMTP candidatos;
- 8 grupos comuns do perfil;
- categoria/escopo dos 8 grupos.

Pendencia imediata antes de montar o objeto final:

1. mapear a cadeia transitiva dos grupos de seguranca comuns, principalmente `_Engenharia` e `_Tecnica SSA`;
2. confirmar que nenhum ancestral representa grupo privilegiado/protegido ou acesso que nao deva ser concedido automaticamente;
3. depois montar a pre-visualizacao completa do chamado TOPdesk e do objeto AD, sem escrita;
4. solicitar autorizacao explicita antes de criar o chamado TOPdesk ficticio;
5. solicitar nova autorizacao explicita antes de criar o usuario no AD.

## Regra de local dos testes

Toda solicitacao futura deve indicar claramente um dos tres locais:

- **AD**;
- **servidor 10.1.2.21**;
- **maquina do usuario**.

---

## CHECKPOINT-V012-AD-CADEIA-GRUPOS-FECHADA.md

# CHECKPOINT V012 - Active Directory: cadeia transitiva dos grupos fechada

Data: 2026-09-23

## Local dos testes desta rodada

Os comandos desta rodada foram executados **no Active Directory, com usuario de nivel elevado**.

Nenhuma escrita no AD, TOPdesk, IIS ou Microsoft 365 foi realizada.

## Resultado - cadeia transitiva de `_Engenharia`

Origem:

- `CN=_Engenharia,OU=Engenharia,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`

Ancestral transitivo encontrado:

- `_JumpServer`
  - GroupCategory: Security;
  - GroupScope: Global;
  - adminCount: vazio;
  - isCriticalSystemObject: vazio;
  - Description: `Grupo de seguranca para acesso remoto usado no JumpServer`;
  - MemberOfDireto: vazio;
  - DN: `CN=_JumpServer,OU=Suporte,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`.

Conclusao:

- a cadeia identificada e `_Engenharia -> _JumpServer`;
- a consulta nao retornou novo grupo ancestral acima de `_JumpServer`;
- adicionar um usuario a `_Engenharia` implica acesso transitivo associado ao `_JumpServer`;
- isso aparece como parte do perfil real dos cinco usuarios `Automation Systems Analyst + ENGENHARIA`, portanto deve ser tratado como acesso conhecido do perfil e nao como grupo invisivel/nao documentado.

## Resultado - cadeia transitiva de `_Tecnica SSA`

Origem:

- `CN=_Tecnica SSA,OU=Operacoes,OU=Automind,DC=automind,DC=com,DC=br`

Ancestrais transitivos encontrados:

1. `_GAP2`
   - GroupCategory: Security;
   - GroupScope: Global;
   - adminCount: vazio;
   - isCriticalSystemObject: vazio;
   - Description: `Acesso pelo EM / TM / RT / RT  / CSGL / CDS / AN / PR / EST / EA / TA / CDS / CT`;
   - MemberOfDireto: vazio.

2. `_Tecnica`
   - GroupCategory: Security;
   - GroupScope: Global;
   - adminCount: vazio;
   - isCriticalSystemObject: vazio;
   - MemberOfDireto: vazio no resultado recebido.

Conclusao:

- a cadeia identificada e `_Tecnica SSA -> _GAP2` e `_Tecnica SSA -> _Tecnica`;
- nao foram retornados novos ancestrais acima de `_GAP2` ou `_Tecnica`;
- adicionar um usuario a `_Tecnica SSA` concede tambem os acessos representados por esses dois grupos ancestrais;
- esse comportamento deve ficar visivel na pre-visualizacao do cadastro antes da gravacao.

## Decisao para o primeiro teste ponta a ponta

Com os testes de leitura atuais, a etapa de levantamento do perfil `Automation Systems Analyst + ENGENHARIA` pode ser considerada suficiente para montar a pre-visualizacao final.

Para o primeiro usuario ficticio, os oito grupos comuns 5/5 permanecem candidatos ao provisionamento do perfil:

1. `_Tecnica SSA` - Security;
2. `Dist_Engenharia` - Distribution;
3. `_Todos SSA` - Security;
4. `_Todos` - Security;
5. `_Engenharia` - Security;
6. `_GA_E-CLIC` - Security;
7. `Dist_Todos` - Distribution;
8. `Dist_Todos_SSA` - Distribution.

A pre-visualizacao deve destacar explicitamente os efeitos transitivos:

- `_Engenharia` -> `_JumpServer`;
- `_Tecnica SSA` -> `_GAP2` e `_Tecnica`.

Grupos que nao aparecem em 5/5 usuarios equivalentes, como `_GAP7`, nao devem ser adicionados automaticamente no primeiro teste.

## Identidade ficticia preparada

- Nome/CN: `Teste Provisionamento Automind`;
- GivenName candidato: `Teste`;
- Surname candidato: `Provisionamento Automind`;
- DisplayName: `Teste Provisionamento Automind`;
- sAMAccountName: `teste.provisionamento`;
- UPN: `teste.provisionamento@automind.com.br`;
- mail candidato: `teste.provisionamento@automind.com.br`;
- SMTP primario: `teste.provisionamento@automind.co`;
- SMTP secundario: `teste.provisionamento@automind.com.br`;
- Title: `Automation Systems Analyst`;
- Department: `ENGENHARIA`;
- Company: `Automind`;
- Manager: `CN=Edson Neto,OU=Engenharia,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`;
- OU de destino: `OU=Engenharia,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`;
- Country: `BR`.

A identidade ja foi validada sem colisao nas consultas executadas anteriormente.

## Proximo passo

Nao ha necessidade de continuar ampliando os testes de leitura antes da simulacao do primeiro fluxo.

Sequencia sugerida:

1. montar e revisar a pre-visualizacao dos dados ficticios que irao para o chamado TOPdesk;
2. criar o chamado TOPdesk ficticio somente apos autorizacao explicita;
3. validar a leitura/importacao desses dados pelo fluxo do CadastroColaboradores;
4. montar a pre-visualizacao final do objeto AD e dos grupos;
5. criar o usuario ficticio no AD somente apos nova autorizacao explicita;
6. validar atributos, OU, grupos diretos e grupos transitivos apos a criacao;
7. Microsoft 365/licencas permanece fora desse primeiro teste.

## Regra de local dos testes

Toda solicitacao futura deve indicar claramente um dos tres locais:

- **AD**;
- **servidor 10.1.2.21**;
- **maquina do usuario**.

---

## CHECKPOINT-V013-TOPDESK-IMPORTACAO-REAL-E-PENDENCIAS-AD.md

# CHECKPOINT V013 - TOPdesk: chamado ficticio importado; pendencias antes da escrita no AD

Data: 2026-09-23

## Operacao realizada

Foi criado no TOPdesk, pela maquina do usuario/navegador, o chamado ficticio de teste:

- numero: `I2609-0295`;
- objetivo: validar o fluxo controlado TOPdesk -> CadastroColaboradores -> Active Directory;
- o chamado representa apenas um colaborador ficticio e nao corresponde a uma pessoa real.

Nenhuma escrita no Active Directory foi realizada nesta etapa.

## Resultado da importacao no CadastroColaboradores

O chamado `I2609-0295` foi carregado com sucesso pela tela `Colaboradores/ImportarTopdesk` por meio do fluxo TOPdesk/Bridge ja existente.

Os campos exibidos na aplicacao apos a importacao confirmam, entre outros:

- Nome completo: `Teste Provisionamento Automind`;
- Nome de guerra: `Teste Provisionamento`;
- Login gerado: `teste.provisionamento`;
- telefone celular: `(71) 9 9999-0000`;
- local de trabalho: `Salvador`;
- Cargo em portugues: `Analista de Sistemas de Automacao`;
- Departamento: `ENGENHARIA`;
- Empresa: `Automind`;
- Superior imediato: `Edson Neto`;
- existe campo de selecao `OU de destino`, ainda sem OU selecionada na evidencia recebida;
- `Cargo em ingles` permaneceu vazio na importacao e precisa de regra/mapeamento antes da criacao do usuario.

Conclusao: a etapa de criacao do chamado ficticio e a importacao TOPdesk -> CadastroColaboradores foram validadas no primeiro teste ponta a ponta.

## Evidencia importante - sugestao de grupos ainda nao representa o AD real

Na tela importada, a secao `Sugestao automatica pelo cargo` apresentou os grupos:

- `GG_Colaboradores` - 4/4;
- `GG_Engenharia` - 4/4;
- `VPN_Usuarios` - 4/4;
- `Projeto_Temporario` - 1/4;
- `Domain Admins` - 1/4 e classificado como protegido.

Esses nomes nao correspondem ao conjunto real levantado no Active Directory para o perfil `Automation Systems Analyst + ENGENHARIA`, cuja intersecao real 5/5 e:

1. `_Tecnica SSA`;
2. `Dist_Engenharia`;
3. `_Todos SSA`;
4. `_Todos`;
5. `_Engenharia`;
6. `_GA_E-CLIC`;
7. `Dist_Todos`;
8. `Dist_Todos_SSA`.

Portanto, a sugestao de grupos exibida nesta versao da tela nao deve ser usada como base para a primeira criacao real no AD. Antes da escrita, a aplicacao precisa consumir o servico de leitura real do AD ou a camada equivalente aprovada para retornar o perfil real.

## OU de destino

O formulario da aplicacao ja possui campo explicito `OU de destino`, com texto informando que a lista definitiva sera lida diretamente do Active Directory.

Regra mantida:

- a OU nao deve ser inferida somente por `Department`;
- a aplicacao deve exibir apenas OUs permitidas;
- internamente deve ser persistido/usado o `DistinguishedName` completo;
- para este teste, a OU candidata continua sendo `OU=Engenharia,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`;
- nenhum `New-ADUser` deve ocorrer antes de a OU ser validada e selecionada explicitamente.

## Pre-validacao exibida

A tela possui o bloco `Validar antes de criar`, com indicadores para:

- Login disponivel;
- UPN / e-mail disponivel;
- Superior localizado;
- OU valida;
- Grupos existentes;
- Licencas M365 disponiveis.

Decisao atual:

- Microsoft 365/Entra permanece pausado;
- a verificacao `Licencas M365 disponiveis` nao deve bloquear o primeiro teste de criacao no AD;
- neste momento, o proximo objetivo e tornar reais as validacoes de AD e somente depois habilitar a escrita controlada.

## Estado do primeiro teste ponta a ponta

Concluido:

1. dados ficticios definidos;
2. disponibilidade da identidade `teste.provisionamento` validada em leitura no AD;
3. perfil real de grupos `Automation Systems Analyst + ENGENHARIA` levantado;
4. cadeias transitivas de `_Engenharia` e `_Tecnica SSA` mapeadas;
5. chamado ficticio criado no TOPdesk: `I2609-0295`;
6. chamado importado com sucesso pelo CadastroColaboradores.

Pendente antes de qualquer escrita no AD:

1. substituir/ativar a leitura real de OUs no formulario;
2. substituir/ativar a sugestao real de grupos por cargo + departamento;
3. resolver `Superior imediato` para o DN real do usuario `Edson Neto`;
4. validar login, UPN/e-mail, OU e grupos usando o AD real;
5. definir/mapejar `Cargo em ingles` para `Automation Systems Analyst`;
6. manter Microsoft 365 fora do bloqueio desta fase;
7. mostrar a pre-visualizacao final do objeto AD e pedir autorizacao explicita antes de `New-ADUser`;
8. pedir autorizacao explicita separada antes de adicionar grupos.

## Proximo passo recomendado

O fluxo TOPdesk esta suficientemente validado para esta fase. O proximo trabalho deve ocorrer no projeto/aplicacao e permanecer inicialmente apenas em modo leitura:

1. ligar o formulario ao servico real de leitura de AD para OUs, superior, disponibilidade e grupos;
2. aplicar a allowlist de OUs aprovada/configuravel;
3. reproduzir na tela o perfil real 5/5 em vez dos grupos simulados;
4. repetir `Validar no AD - somente leitura` com o chamado `I2609-0295`;
5. somente depois disso preparar a rotina de criacao do usuario, ainda bloqueada ate autorizacao explicita.

Nenhum codigo deve ser alterado sem autorizacao explicita do usuario.

## Regra de local dos testes

Toda solicitacao futura deve indicar claramente um dos tres locais:

- **AD**;
- **servidor 10.1.2.21**;
- **maquina do usuario**.

---

## CHECKPOINT-V014-IMPLEMENTACAO-LEITURA-REAL-AD.md

# CHECKPOINT V014 - Implementação da leitura real do AD no CadastroColaboradores

Data: 2026-09-23

## Autorização desta etapa

Após validar o chamado fictício `I2609-0295` no TOPdesk e confirmar que a tela ainda utilizava lista de OUs, grupos e validações simuladas, o responsável autorizou modificar o projeto completo para utilizar os acessos de leitura do Active Directory já testados.

Microsoft 365/Entra continua pausado.

Nenhuma autorização foi dada para executar escrita no AD durante a preparação do pacote. Portanto esta entrega continua sem criação real de usuário.

## Alterações funcionais

### OUs

- `DevelopmentAdReadOnlyService` deixou de ser registrado e foi removido do projeto.
- novo `WindowsAdReadOnlyService` consulta OUs reais abaixo de `Automind:Ad:PeopleSearchBase`;
- a UI recebe `DisplayName + DistinguishedName` reais;
- `AllowedOuDns` filtra os DNs permitidos;
- a OU selecionada é validada novamente no AD antes da prévia.

### Disponibilidade de identidade

A pré-validação consulta o domínio inteiro para detectar colisões em:

- `sAMAccountName`;
- `userPrincipalName`;
- `mail`;
- `proxyAddresses` para `@automind.co` e `@automind.com.br`;
- CN dentro da OU selecionada, repetindo a checagem manual feita antes do teste ponta a ponta.

A pre-validacao tambem aplica a restricao de `sAMAccountName` de no maximo 20 caracteres e bloqueia caracteres invalidos antes de qualquer futura escrita.

A busca não é limitada a usuários, preservando a regra validada nos testes com grupos/endereços legados.

### Superior imediato

- resolução real entre usuários ativos;
- busca por `displayName`, `cn` ou `sAMAccountName`;
- múltiplos resultados são tratados como ambiguidade e bloqueiam a prévia.

### Grupos

- `DevelopmentAccessSuggestionService` foi removido;
- novo `WindowsAccessSuggestionService` pesquisa usuários ativos de mesmo `Title + Department`;
- grupos diretos são lidos via `memberOf`;
- grupos comuns a toda a coorte são pré-selecionados;
- exceções permanecem não selecionadas;
- grupos protegidos nunca são pré-selecionados;
- grupo primário não entra na comparação, conforme comportamento de `memberOf`;
- grupos de segurança exibem ancestrais calculados com `LDAP_MATCHING_RULE_IN_CHAIN` (`1.2.840.113556.1.4.1941`).

Para o cenário validado `Automation Systems Analyst + ENGENHARIA`, o resultado esperado do AD real continua sendo a coorte de 5 usuários e os 8 grupos comuns documentados no V010.

### Cargo em inglês

Foi criada tradução configurável por `Automind:JobTitleTranslations`.

Mapeamento inicial confirmado para o teste:

- `Analista de Sistemas de Automação` -> `Automation Systems Analyst`.

### Botões/tela

- `Buscar grupos no AD` passa a executar consulta real;
- `Validar no AD` passa a executar pré-validação real;
- Microsoft 365 foi removido dos checks da fase atual;
- a tela exibe uma prévia do objeto AD apenas quando todas as validações passam;
- a prévia inclui CN, givenName, sn, sAMAccountName, UPN, mail, SMTP primário/secundário, Title, Department, Company, Office, telefone condicional, Manager DN, OU DN e grupos;
- o botão de colaborador de referência foi desabilitado em vez de simular comportamento inexistente.

## Segurança e identidade técnica

- consultas usam a identidade do processo IIS;
- nenhuma senha administrativa foi adicionada a `appsettings.json`;
- `_Informatica` continua apenas como autorização de login e não é reutilizado como grupo técnico de escrita;
- nenhuma gMSA/grupo/ACL nova foi criada nesta etapa;
- a futura escrita continua condicionada a uma identidade técnica com permissão mínima e autorização explícita.

## OUs permitidas nesta entrega

`appsettings.json` contém uma allowlist inicial baseada nas unidades reais levantadas e na lista operacional já apresentada na interface. A aplicação só exibe DNs da allowlist que existirem no AD no momento da consulta.

A allowlist pode ser ajustada por configuração sem hardcode de rótulos na View.

## Escritas explicitamente ausentes

Não há implementação ativa de:

- `New-ADUser`;
- `Set-ADAccountPassword` / definição de senha;
- `Add-ADGroupMember`;
- alteração de manager;
- alteração de proxyAddresses/mail;
- Microsoft Graph/licenças.

## Validação técnica local do pacote

No ambiente de construção deste pacote não existe SDK/CLI `dotnet`, portanto não foi possível executar `dotnet build` aqui. Foram executadas verificações estáticas de estrutura, referências removidas, delimitadores e sintaxe JavaScript (`node --check`).

O primeiro build real deve ocorrer no ambiente do responsável/pipeline .NET 10 antes do deploy IIS.

## Git / Azure DevOps

- branch: `release`;
- sem tag;
- sem incremento de versão;
- fazer um único commit;
- push para `origin release` e `azure release`;
- não alterar remotes, pipeline ou Release.

## Primeiro teste após deploy

**LOCAL: servidor 10.1.2.21 / aplicação publicada no IIS, acessada pela máquina do usuário**

1. importar novamente `I2609-0295`;
2. confirmar que `Cargo em inglês` aparece como `Automation Systems Analyst`;
3. abrir OU e confirmar que a lista vem do AD/allowlist;
4. confirmar `03.UDN/Engenharia` e selecionar;
5. clicar `Buscar grupos no AD` e validar a coorte real;
6. clicar `Validar no AD` e verificar os seis checks;
7. conferir a prévia;
8. observar que o login ficticio `teste.provisionamento` possui 21 caracteres e, por seguranca, deve ser reprovado pela validacao de formato do sAMAccountName;
9. nao executar nenhuma criacao, pois a escrita continua ausente.

---

## CHECKPOINT-V015-CORRECAO-BUILD-SERVICOS-DEVELOPMENT.md

# CHECKPOINT V015 - Correcao de build dos servicos Development

Data: 23/09/2026

## Contexto

Ao sobrepor o pacote completo em um workspace Git existente, os arquivos antigos `Services/DevelopmentAdReadOnlyService.cs` e `Services/DevelopmentAccessSuggestionService.cs` permaneceram fisicamente no clone local. Como projetos SDK-style compilam automaticamente os arquivos `*.cs` presentes no diretorio, o `DevelopmentAdReadOnlyService` antigo continuou sendo compilado mesmo sem estar registrado no `Program.cs`.

Depois da expansao de `IAdReadOnlyService`, esse arquivo antigo deixou de cumprir o contrato da interface e gerou os erros CS0738/CS0535.

## Correcao

- `DevelopmentAdReadOnlyService.cs` volta a fazer parte do pacote completo e implementa integralmente o contrato atual de `IAdReadOnlyService`.
- o fallback Development falha de forma segura: nao retorna OU ficticia, nao afirma disponibilidade de identidade e nao valida grupos/OU como reais.
- `DevelopmentAccessSuggestionService.cs` tambem volta a fazer parte do pacote e retorna lista vazia, impedindo a reintroducao dos grupos ficticios antigos (`GG_Colaboradores`, `VPN_Usuarios`, etc.).
- `Program.cs` continua registrando exclusivamente `WindowsAdReadOnlyService` e `WindowsAccessSuggestionService`.
- nenhuma escrita no Active Directory foi adicionada.

## Motivo de compatibilidade

ZIPs sobrepostos em uma pasta existente nao removem arquivos que desapareceram da nova versao. Manter estes dois arquivos em estado compativel evita que clones antigos falhem no build por arquivos residuais.

## Validacao disponivel neste ambiente

- interface e implementacoes conferidas estaticamente;
- `git diff --check` deve permanecer limpo;
- este ambiente nao possui o SDK .NET, portanto o `dotnet build` final continua sendo executado na maquina do usuario antes do commit.

---

## CHECKPOINT-V016-GMSA-DELEGACAO-ESCRITA-AD.md

# CHECKPOINT V016 - gMSA, autorizacao humana e delegacao minima para escrita no AD

Data: 23/09/2026

## Decisao principal

Foi aprovado separar completamente:

1. quem esta autorizado a acionar o provisionamento no sistema;
2. qual identidade tecnica executa a operacao no Active Directory.

## Autorizacao humana

- operador autentica com sua propria conta;
- `_informatica` continua sendo o grupo de autorizacao administrativa do CadastroColaboradores;
- um membro de `_informatica` nao precisa ser Domain Admin, Account Operator nem possuir ACL individual nas OUs;
- `_informatica` nao deve receber permissoes de escrita no AD por causa desta aplicacao.

Motivo: se a permissao LDAP fosse concedida diretamente a `_informatica`, seus membros poderiam potencialmente utilizar ADUC/PowerShell fora do fluxo, contornando validacoes e auditoria da aplicacao. Alem disso, o diagnostico anterior mostrou que `_informatica` possui associacoes privilegiadas no dominio e nao deve ser reutilizado como grupo tecnico da aplicacao.

## Identidade tecnica

Identidade alvo: gMSA exclusiva do CadastroColaboradores.

Nome sugerido: `gMSA_CadColab$`.

O nome curto substitui a sugestao inicial longa `gmsa_CadastroColaboradores$`, pois a documentacao do `New-ADServiceAccount` recomenda `sAMAccountName` de conta de servico com 15 caracteres ou menos para compatibilidade; o simbolo `$` e acrescentado quando necessario.

A gMSA devera:

- ter senha administrada automaticamente pelo AD;
- nao possuir senha em `appsettings.json`, codigo ou pipeline;
- ser utilizavel somente pelo host IIS `SV052022-6121` / `10.1.2.21` (diretamente ou por grupo de hosts dedicado);
- executar as operacoes LDAP da aplicacao.

## Contexto atual do IIS

Hoje o App Pool esta em `ApplicationPoolIdentity`. Para acessos de rede, esse modelo usa a conta da maquina do servidor, identificada nos testes como:

`AUTOMIND\\SV052022-6121$`

Os testes manuais no AD foram executados pelo usuario elevado apenas para descoberta/validacao. Isso nao representa a identidade desejada da aplicacao em producao.

## Grupo tecnico de delegacao

Criar um grupo de seguranca dedicado, sugerido:

`SG_CadastroColaboradores_AD_Writer`

Esse grupo recebera ACLs delegadas e nao deve conter operadores humanos.

A identidade tecnica da aplicacao sera associada a esse mecanismo de delegacao. Nenhuma permissao administrativa ampla deve ser concedida.

## Delegacao nas OUs

Somente OUs aprovadas na allowlist poderao receber delegacao.

Permissoes funcionais planejadas:

- criar objeto User;
- preencher givenName;
- sn;
- displayName;
- description;
- physicalDeliveryOfficeName;
- telephoneNumber quando autorizado;
- mail;
- title;
- department;
- company;
- manager;
- userPrincipalName;
- sAMAccountName;
- definir senha;
- habilitar conta;
- forcar troca de senha no primeiro logon somente se essa politica for confirmada.

Nao conceder Domain Admin, Enterprise Admin, Account Operators ou Full Control do dominio.

## Delegacao de grupos

Grupos sao uma permissao separada da OU. Para associar um usuario, a operacao altera `member` no objeto de grupo.

Somente grupos explicitamente aprovados poderao receber permissao de alteracao de membros.

Exemplos estudados, ainda dependentes de aprovacao final de escrita:

- `_Todos`;
- `_Todos SSA`;
- `_Engenharia`;
- `_Tecnica SSA`;
- `Dist_Engenharia`.

Proibido conceder escrita automatica em grupos administrativos/protegidos, `_informatica`, `Domain Admins` ou equivalentes.

## Auditoria

O AD vera a gMSA como identidade executora. A aplicacao precisa registrar o contexto humano:

- usuario solicitante;
- chamado TOPdesk;
- identidade tecnica;
- objeto criado;
- OU;
- grupos;
- data/hora;
- resultado.

A senha inicial nunca sera persistida.

## Fluxo alvo

`membro de _informatica`

-> login no CadastroColaboradores

-> autorizacao humana validada

-> operador confirma criacao

-> backend executa como `gMSA_CadColab$`

-> AD valida apenas ACLs delegadas a identidade tecnica

-> aplicacao registra operador + chamado + resultado.

## Sequencia aprovada para implantacao

1. validar nomes/objetos tecnicos existentes;
2. criar gMSA exclusiva;
3. autorizar uso somente no `10.1.2.21`;
4. instalar/testar gMSA no servidor;
5. configurar IIS/App Pool para identidade tecnica;
6. criar grupo tecnico de delegacao;
7. delegar permissoes minimas nas OUs aprovadas;
8. delegar Write Members somente nos grupos aprovados;
9. testar leitura;
10. criar usuario ficticio;
11. validar senha;
12. validar manager;
13. validar grupos;
14. remover usuario ficticio, se decidido.

## Estado deste checkpoint

Nenhum objeto AD, grupo, ACL, gMSA ou configuracao IIS foi criado/alterado por este checkpoint. Esta versao e somente documental e prepara os testes/implantacao controlada.

---

## CHECKPOINT-V017-POLITICA-MUDANCA-AD-ROLLBACK.md

# CHECKPOINT V017 - Politica de mudanca AD/IIS, impacto zero e rollback linear

Data: 23/09/2026
Projeto: Automind.CadastroColaboradores
Status: REGRA OPERACIONAL OBRIGATORIA - nenhuma escrita autorizada neste checkpoint

## 1. Motivo

Antes de qualquer criacao de gMSA, grupo tecnico, delegacao de OU, permissao de `member`, alteracao de identidade do App Pool ou escrita de usuario no Active Directory, o projeto passa a adotar uma politica extremamente conservadora, linear, auditavel e reversivel.

O objetivo e impedir que a implantacao do provisionamento provoque indisponibilidade no Active Directory, controladores de dominio, IIS ou demais servicos corporativos.

## 2. Regra principal

Nenhuma alteracao sera executada apenas porque o comando foi preparado.

Para cada mudanca futura devem existir, ANTES da escrita:

1. teste somente leitura;
2. baseline do estado atual;
3. identificacao exata do objeto afetado;
4. comando de mudanca revisado;
5. comando/procedimento de rollback revisado;
6. criterio objetivo de sucesso;
7. criterio objetivo de parada;
8. autorizacao explicita do operador responsavel;
9. uma unica alteracao por vez;
10. validacao imediatamente apos a alteracao;
11. registro documental do resultado antes de avancar.

Ao primeiro resultado diferente do esperado, o procedimento para e nao executa a proxima etapa.

## 3. Operacoes proibidas durante esta implantacao

Sem uma necessidade tecnica excepcional, nova analise e autorizacao especifica, NAO executar:

- reinicializacao de controlador de dominio;
- `Restart-Computer` em DC;
- reinicializacao dos servicos AD DS/NTDS;
- reinicializacao de KDC/Netlogon como tentativa de correcao;
- `iisreset`;
- reinicializacao completa do servidor `10.1.2.21`;
- alteracao de Schema do AD;
- alteracao de GPO;
- alteracao de DNS do dominio;
- alteracao de configuracao KDS existente;
- `New-KdsRootKey` (o ambiente ja possui KDS root key validada);
- delegacao no dominio inteiro quando uma OU/grupo especifico e suficiente;
- inclusao da identidade tecnica em Domain Admins, Enterprise Admins, Account Operators ou grupos equivalentes;
- delegacao de escrita diretamente ao grupo `_informatica`;
- mudancas em lote de varias OUs/grupos sem validacao intermediaria.

## 4. Separacao de identidades

### Usuario humano

- autentica no CadastroColaboradores;
- deve estar autorizado pela regra da aplicacao, atualmente `_informatica`;
- pode possuir baixo privilegio administrativo no AD;
- nao fornece suas credenciais para a rotina de provisionamento;
- deve ser gravado na auditoria como operador humano.

### Identidade tecnica

Estado alvo planejado: gMSA exclusiva `gMSA_CadColab$`.

- executa as operacoes LDAP autorizadas;
- nao possui senha estatica em arquivo de configuracao;
- recebe apenas delegacao minima;
- nao recebe privilegio administrativo geral;
- AD registrara essa identidade como executor tecnico.

## 5. Politica linear por fase

### Fase 0 - diagnostico e baseline - SOMENTE LEITURA

Nenhuma alteracao.

Registrar:

- existencia/ausencia dos nomes planejados da gMSA e grupo tecnico;
- objeto do servidor `SV052022-6121$`;
- configuracao atual do App Pool `CadastroColaboradores`;
- ACL atual da pasta da aplicacao;
- ACL atual de cada OU que futuramente receber delegacao;
- ACL atual de cada grupo que futuramente receber `Write Members`;
- membros atuais do futuro grupo tecnico, caso ele ja exista;
- estado atual de replicacao/saude somente se uma verificacao adicional for necessaria.

Resultado esperado: inventario completo, nenhuma mudanca.

### Fase 1 - criar somente a gMSA

Esta fase NAO esta autorizada ainda.

Pre-condicoes:

- Fase 0 concluida;
- nome confirmado livre;
- PrincipalsAllowedToRetrieveManagedPassword revisado;
- comando de criacao apresentado;
- rollback apresentado;
- autorizacao explicita.

Mudanca unica: criar a gMSA.

Validacao imediata: objeto existe com somente os parametros aprovados.

Rollback previsto: remover somente a gMSA criada, desde que nenhuma etapa posterior dependa dela.

Nao reiniciar DC.

### Fase 2 - criar somente o grupo tecnico

Esta fase NAO esta autorizada ainda.

Mudanca unica: criar `SG_CadastroColaboradores_AD_Writer` no escopo/OU que forem aprovados.

Validar imediatamente categoria, escopo, DN e membros.

Rollback: remover somente o grupo se ainda nao houver ACLs/dependencias associadas; se houver, remover dependencias primeiro na ordem inversa.

### Fase 3 - associar identidade tecnica ao mecanismo de delegacao

Esta fase NAO esta autorizada ainda.

Uma unica associacao por vez.

Validar membro antes/depois.

Rollback: remover exatamente a associacao adicionada.

### Fase 4 - instalar/testar gMSA somente no `10.1.2.21`

Esta fase NAO esta autorizada ainda.

Antes:

- confirmar RSAT/comandos disponiveis;
- baseline local;
- nenhum reboot planejado.

Mudanca futura: instalar a service account localmente somente no servidor autorizado.

Validacao: `Test-ADServiceAccount` e leitura do estado local.

Rollback planejado: desinstalar localmente a service account, sem alterar DCs.

Se qualquer ferramenta solicitar reboot, PARAR e revisar; nao reiniciar automaticamente.

### Fase 5 - delegacao de UMA OU piloto

Esta fase NAO esta autorizada ainda.

Nao delegar todas as OUs de uma vez.

Antes de cada OU:

- capturar ACL atual;
- identificar somente ACEs que serao adicionadas;
- preparar a remocao dessas mesmas ACEs;
- obter autorizacao.

Comecar por uma unica OU piloto aprovada, preferencialmente a usada no teste controlado.

Depois da mudanca:

- conferir ACL;
- testar leitura;
- testar apenas a capacidade estritamente planejada;
- documentar;
- somente depois avaliar a proxima OU.

Rollback: remover somente as ACEs adicionadas pelo projeto. Evitar restaurar cegamente uma ACL completa, pois isso poderia apagar alteracoes concorrentes feitas por outros administradores.

### Fase 6 - delegacao de UM grupo piloto

Esta fase NAO esta autorizada ainda.

Tratar separadamente da OU.

Antes:

- capturar ACL do grupo;
- registrar membros atuais;
- preparar ACE exata de `member` e rollback exato.

Um grupo por vez.

Grupos privilegiados/protegidos permanecem fora do escopo.

### Fase 7 - preparar IIS sem impacto global

Esta fase NAO esta autorizada ainda.

Nunca usar `iisreset` como procedimento normal deste projeto.

Antes de qualquer troca de identidade:

- exportar/registrar configuracao atual do App Pool;
- registrar ACLs locais;
- garantir acesso local necessario para a identidade tecnica;
- definir rollback para `ApplicationPoolIdentity`;
- escolher janela controlada.

A ativacao de uma nova identidade no App Pool pode exigir reciclagem somente desse App Pool/processo. Isso e um impacto localizado na aplicacao e deve ser tratado como mudanca separada, anunciada e autorizada; nao deve reiniciar IIS inteiro nem o servidor.

Rollback: restaurar a configuracao anterior do App Pool e reciclar somente o pool afetado, se necessario.

### Fase 8 - leitura real usando a identidade tecnica

Antes de habilitar qualquer escrita no codigo:

- OUs;
- superior;
- grupos;
- disponibilidade de identidade;
- cadeia transitiva.

Tudo deve funcionar pela identidade tecnica.

Se leitura falhar, nenhuma escrita sera habilitada.

### Fase 9 - habilitar escrita por feature flag/configuracao controlada

A rotina de escrita deve permanecer desabilitada por padrao ate toda a infraestrutura anterior ser validada.

Antes de habilitar:

- build aprovado;
- auditoria pronta;
- OU piloto definida;
- grupos piloto definidos;
- rollback da aplicacao definido;
- autorizacao explicita.

### Fase 10 - criar UM usuario ficticio

Somente um objeto.

Sequencia:

1. pre-validacao integral;
2. exibir a previa;
3. confirmar login/UPN/CN livres;
4. confirmar OU;
5. confirmar manager;
6. confirmar atributos;
7. confirmar grupos selecionados;
8. autorizacao humana;
9. criar usuario;
10. validar objeto;
11. parar e documentar antes de senha/grupos adicionais se esses passos forem separados.

Nenhum lote de usuarios durante o piloto.

### Fase 11 - senha, manager e grupos

Cada capacidade e tratada como subetapa independente e validada imediatamente.

Ordem recomendada:

1. senha/estado da conta;
2. manager;
3. grupos, um conjunto aprovado por vez.

### Fase 12 - limpeza do usuario ficticio

Somente mediante decisao explicita.

Antes de excluir:

- confirmar que e o objeto ficticio correto;
- registrar DN/SID/grupos;
- confirmar que nao ha dependencia real.

A exclusao e uma nova escrita e tambem exige autorizacao.

## 6. Politica de rollback

Rollback segue a ordem inversa das mudancas e nunca e executado preventivamente sem necessidade.

Exemplo de dependencia:

1. escrita da aplicacao;
2. identidade do App Pool;
3. delegacoes de grupos;
4. delegacoes de OU;
5. associacao ao grupo tecnico;
6. grupo tecnico;
7. instalacao local da gMSA;
8. objeto gMSA.

Para desfazer, percorrer de cima para baixo na ordem inversa, sempre validando cada passo.

Nao remover gMSA/grupo tecnico enquanto ainda existirem ACLs, App Pools ou aplicacoes dependentes deles.

## 7. Evidencia obrigatoria por mudanca

Cada etapa futura deve registrar no checkpoint:

- data/hora;
- local da execucao: AD / `10.1.2.21` / maquina do operador;
- usuario humano que executou/autorizou;
- estado antes;
- comando executado;
- objeto alvo;
- resultado;
- estado depois;
- comando de rollback preparado;
- rollback executado ou nao;
- motivo para avancar ou parar.

## 8. Regra de local dos comandos

Todo comando passado ao operador deve vir explicitamente rotulado como um dos tres locais:

- **EXECUTAR NO AD**;
- **EXECUTAR NO SERVIDOR 10.1.2.21**;
- **EXECUTAR NA SUA MAQUINA**.

## 9. Estado ao final deste checkpoint

- nenhuma gMSA criada;
- nenhum grupo tecnico criado;
- nenhuma ACL alterada;
- nenhum usuario criado/removido;
- nenhum grupo teve membros alterados;
- App Pool continua como estava;
- nenhum DC reiniciado;
- IIS nao foi reiniciado;
- `10.1.2.21` nao foi reiniciado;
- fase atual: baseline somente leitura.

---

## CHECKPOINT-V018-BASELINE-ACL-ENGENHARIA.md

# CHECKPOINT V018 - Baseline da ACL da OU Engenharia e nomes planejados livres

Data: 23/09/2026
Projeto: Automind.CadastroColaboradores
Status: SOMENTE LEITURA - nenhuma alteracao executada

## 1. Local e identidade usada nos testes

Os comandos desta rodada foram executados no Active Directory com o usuario administrativo elevado do operador, exclusivamente para diagnostico/baseline.

Nenhuma gMSA, grupo tecnico, ACE, usuario, senha, membro de grupo, configuracao de IIS ou objeto do AD foi criado/alterado.

## 2. Objetos planejados

Consulta realizada:

- gMSA planejada: `gMSA_CadColab` / `gMSA_CadColab$`;
- grupo tecnico planejado: `SG_CadastroColaboradores_AD_Writer`;
- servidor autorizado: `SV052022-6121`.

Resultado:

- nenhuma gMSA com o nome planejado foi retornada;
- nenhum grupo com o nome planejado foi retornado;
- servidor `SV052022-6121` existe e esta habilitado;
- `sAMAccountName`: `SV052022-6121$`;
- DNS: `SV052022-6121.automind.com.br`;
- DN: `CN=SV052022-6121,CN=Computers,DC=automind,DC=com,DC=br`.

Conclusao: os nomes planejados continuam livres e o computador autorizado esta presente/ativo. Isto NAO autoriza criacao; apenas fecha esta parte do baseline.

## 3. Baseline da OU piloto Engenharia

OU:

`OU=Engenharia,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`

Resultado do descritor de seguranca:

- Owner: `AUTOMIND\\Domain Admins`;
- `AreAccessRulesProtected = False`;
- portanto a heranca de ACL nao esta bloqueada;
- total observado: 40 ACEs.

Este snapshot deve ser preservado como referencia antes de qualquer delegacao futura.

## 4. Descoberta importante: gMSA existente de Cloud Sync ja possui delegacoes na arvore

A ACL retornada contem varias ACEs para:

`AUTOMIND\\pGMSA_c5fa29e8$`

Esta conta ja havia sido identificada anteriormente como a gMSA usada pelo Azure AD / Microsoft Entra Cloud Sync e NAO deve ser reutilizada pelo CadastroColaboradores.

Entre as ACEs observadas aparecem:

- `ExtendedRight` com ObjectType `00299570-246d-11d0-a768-00aa006e0529`, GUID oficial do direito `Reset Password` / `User-Force-Change-Password`;
- `CreateChild, DeleteChild, GenericWrite` com ObjectType `bf967aba-0de6-11d0-a285-00aa003049e2`, GUID oficial da classe AD `user`;
- outras permissoes de leitura/escrita de propriedades aplicadas a classes/atributos especificos.

Conclusao segura: a ACL da OU Engenharia ja contem delegacoes tecnicas relevantes de outro produto/servico. Nenhuma delas deve ser removida, alterada, copiada ou usada como modelo cego para o CadastroColaboradores.

## 5. Implicacao para rollback

A existencia de ACLs tecnicas preexistentes reforca a politica de rollback definida no V017:

- nunca restaurar uma ACL inteira a partir de um snapshot antigo;
- nunca usar `Set-Acl` com um descritor completo como rollback generico;
- futuramente, cada ACE adicionada pelo CadastroColaboradores deve ser identificavel de forma exata;
- rollback deve remover SOMENTE a ACE criada pelo projeto;
- antes de cada mudanca, capturar novamente o estado atual para detectar alteracoes concorrentes.

## 6. Ponto ainda nao comprovado

A saida tabular desta rodada nao permite concluir com seguranca, para cada ACE da `pGMSA_c5fa29e8$`:

- se a ACE e explicita na OU Engenharia ou herdada;
- de qual OU ancestral ela se origina;
- qual `InheritanceType`/`IsInherited` exato de cada regra.

Antes de criar qualquer delegacao propria, isto sera levantado em modo somente leitura.

## 7. Proxima etapa - ainda somente leitura

### Teste A - AD

Isolar as ACEs da `pGMSA_c5fa29e8$` em `Automind`, `03.UDN` e `Engenharia`, mostrando `IsInherited`, `InheritanceType`, ObjectType e InheritedObjectType para localizar a origem da delegacao existente.

### Teste B - servidor 10.1.2.21

Capturar baseline local ainda pendente:

- disponibilidade de `Install-ADServiceAccount` / `Test-ADServiceAccount`;
- identidade atual do App Pool `CadastroColaboradores`;
- `loadUserProfile`;
- ACL atual de `C:\\Automind.CadastroColaboradores`.

Nenhuma instalacao, troca de identidade, recycle, `iisreset` ou alteracao de ACL sera feita.

## 8. Criterio de parada

Depois dos Testes A e B, parar novamente, documentar os resultados e revisar o plano. A Fase 1 (criacao da gMSA) continua NAO AUTORIZADA.

---

## CHECKPOINT-V019-HERANCA-ACL-E-BASELINE-IIS.md

# CHECKPOINT V019 — Herança de ACL e baseline IIS

Data: 23/09/2026

## Resultado dos testes

### Active Directory

A conta existente `AUTOMIND\pGMSA_c5fa29e8$` possui ACEs herdadas nas OUs:

- `OU=Automind,DC=automind,DC=com,DC=br`
- `OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`
- `OU=Engenharia,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`

Em todos os resultados analisados, `IsInherited = True`. Portanto, essas permissões não foram definidas diretamente em Engenharia nem em 03.UDN. Como também aparecem herdadas em `OU=Automind`, a origem está acima dessa OU e deve ser localizada antes de qualquer nova delegação.

Decisão: não reutilizar nem alterar `pGMSA_c5fa29e8$` ou suas ACEs.

### Servidor da aplicação

Os cmdlets `Install-ADServiceAccount` e `Test-ADServiceAccount` estão disponíveis.

O baseline de IIS não foi concluído porque:

- o módulo `WebAdministration` não foi encontrado;
- o drive `IIS:` não existe nessa sessão;
- `C:\Automind.CadastroColaboradores` não existe no host onde o comando foi executado.

Nenhuma alteração foi realizada.

## Próximos testes — somente leitura

1. localizar a origem das ACEs da `pGMSA_c5fa29e8$` no nível do domínio;
2. confirmar hostname, presença do IIS, App Pool/site e caminho físico reais no servidor `10.1.2.21`.

## Regra editorial

Documentação e interação devem usar vocabulário técnico simples, texto objetivo e sem repetição desnecessária.

---

## CHECKPOINT-V020-CONTA-LEGADA-E-WORKER-IIS.md

# CHECKPOINT V020 — Conta legada e worker IIS

Data: 23/09/2026

## Resultado dos testes

### Active Directory

A conta `AUTOMIND\CriaMovePastas` existe e está habilitada.

Estado observado:

- `SamAccountName`: `CriaMovePastas`;
- `PasswordNeverExpires`: `True`;
- membro de `_CriaMovePastas`;
- membro de `_GAP1`;
- sem SPNs configurados no resultado consultado.

Regra: não alterar senha, grupos ou objeto desta conta durante a implantação da gMSA até concluir o mapeamento de dependências. A credencial apareceu em uma saída de diagnóstico; tratar como dado sensível e planejar rotação separada, sem misturar com a implantação do CadastroColaboradores.

### Servidor 10.1.2.21

A ACL de `C:\Automind.CadastroColaboradores` foi capturada. O acesso local é herdado e inclui `SYSTEM`, `Administrators`, `Users` e a conta administrativa usada na manutenção.

No momento da consulta não havia processo `w3wp.exe` ativo para o App Pool `CadastroColaboradores`. Isso é compatível com o App Pool configurado como `OnDemand` e com encerramento por ociosidade.

Como não havia worker ativo, a identidade efetiva do processo ainda não foi comprovada em runtime.

Nenhuma alteração foi realizada.

## Próximos testes — somente leitura

1. confirmar saúde de replicação do AD antes de qualquer criação de objeto;
2. iniciar o site por acesso normal e consultar o proprietário do worker process;
3. mapear usos atuais de `CriaMovePastas` em serviços, tarefas agendadas e App Pools sem exibir senhas.

## Regra operacional

Nenhuma rotação de senha, alteração de App Pool, criação de gMSA/grupo ou ACL será executada antes desses baselines.

---

## CHECKPOINT-V021-REPLICACAO-E-IDENTIDADE-EFETIVA-IIS.md

# CHECKPOINT V021 - Replicacao AD e identidade efetiva do IIS

Data: 23/09/2026

## Resultado

- `repadmin /replsummary`: 0 falhas nos dois controladores de dominio.
- Maior atraso observado: inferior a 30 minutos.
- App Pool `CadastroColaboradores`: worker ativo confirmado.
- Identidade efetiva do worker: `IIS APPPOOL\CadastroColaboradores`.
- Portanto, o processo atual nao executa como `AUTOMIND\CriaMovePastas`.
- `CriaMovePastas` aparece como `SpecificUser` nos pools `.NET v4.5` e `.NET v4.5 Classic`.
- Nos pools `Calendario`, `AutomindTermos` e `CadastroColaboradores`, `IdentityType=ApplicationPoolIdentity`; o valor residual de `UserName` nao representa a identidade efetiva do worker.
- Nenhum servico ou tarefa agendada usando `CriaMovePastas` foi encontrado no servidor consultado.

## Decisoes

- Nao alterar, remover ou rotacionar `CriaMovePastas` nesta fase.
- Nao usar os campos residuais de `UserName` como evidencia de identidade efetiva; validar sempre pelo owner do `w3wp.exe`.
- A futura gMSA sera uma identidade nova e independente.
- Nenhuma alteracao foi executada neste checkpoint.

## Proximo passo

Continuar somente leitura: validar saude do secure channel do servidor `SV052022-6121` com o dominio e inventariar as gMSAs existentes, principalmente `PrincipalsAllowedToRetrieveManagedPassword`, antes de criar qualquer nova conta de servico.

---

## CHECKPOINT-V022-CONTEXTO-LEGADO-CRIAMOVEPASTAS.md

# CHECKPOINT V022 - Contexto legado CriaMovePastas

Data: 23/09/2026

## Objetivo

Registrar o contexto do sistema legado `CriaMovePastas` sem misturá-lo com a identidade técnica do `Automind.CadastroColaboradores`.

## Estado confirmado

- existe um projeto legado `CriaMovePastas` hospedado em IIS;
- o Application Pool `CriaMovePastas` foi configurado historicamente com a conta pessoal elevada `AUTOMIND\daniel.gusmao.d`;
- essa configuração é dívida técnica e será corrigida em trabalho separado, com baseline e rollback próprios;
- não reutilizar essa conta, o pool ou a conta AD `CriaMovePastas` no `CadastroColaboradores`;
- o worker real de `CadastroColaboradores` continua confirmado como `IIS APPPOOL\CadastroColaboradores`.

## Script legado

Arquivo informado: `C:\CriaMovePastas\Script\CriarEstrutura.ps1`.

O script:

- cria estruturas de pastas para `Projeto` ou `SAAS`;
- copia modelos com `robocopy` sem copiar ACL de origem;
- remove herança em pastas criadas;
- remove `Domain Users` e `_Informatica` das ACLs tratadas;
- concede Full Control a `Administrators` e `Domain Admins`;
- em pontos específicos concede Full Control nominal a `daniel.gusmao` e `fernando.menezes`;
- aplica permissões `RX` e `M` aos grupos `_GAP1` a `_GAP7` conforme o tipo/subpasta;
- grava log e progresso em `C:\CriaMovePastas\Logs`.

O script altera sistema de arquivos/ACL NTFS. Ele não deve ser usado como modelo de permissões do Active Directory.

## Decisão

O `CadastroColaboradores` seguirá arquitetura própria:

- `_informatica`: autorização humana dentro da aplicação;
- gMSA dedicada: identidade técnica do backend;
- grupo técnico dedicado: delegação mínima no AD;
- nenhuma conta pessoal elevada na identidade do App Pool;
- nenhuma reutilização do legado `CriaMovePastas`.

## Segurança

Credenciais vistas em telas/comandos não são registradas neste checkpoint.

## Estado da mudança

Nenhuma configuração de IIS, AD, ACL ou conta foi alterada neste checkpoint.

---

## CHECKPOINT-V023-GMSA-PREREQUISITOS-REDE.md

# CHECKPOINT V023 — gMSA: pré-requisitos de rede e AD

Data: 23/09/2026

## Resultado

Os testes continuam somente leitura.

### Contas de serviço existentes

Foram encontradas:

- `provAgentgMSA` / `pGMSA_c5fa29e8$`, habilitada, localizada em `CN=Managed Service Accounts`; somente os controladores `SV062022-9947` e `SV062022-6158` podem recuperar sua senha gerenciada.
- `ADSyncMSA82bf3$`, habilitada, sem `PrincipalsAllowedToRetrieveManagedPassword` retornado.

Conclusão: nenhuma conta existente será reutilizada pelo CadastroColaboradores.

### Servidor da aplicação

No `SV052022-6121`:

- `Test-ComputerSecureChannel` retornou `True`;
- domínio: `automind.com.br`;
- DC localizado: `SV062022-6158.automind.com.br` / `10.1.2.1`;
- site AD: `Site-Salvador`;
- DC com LDAP/KDC/DNS e escrita disponíveis;
- horário sincronizado com `SV062022-6158.automind.com.br`;
- última sincronização de horário bem-sucedida no teste.

Conclusão: canal de domínio, descoberta de DC e sincronismo de horário estão adequados para continuar os testes de prontidão da gMSA.

## Decisão

A identidade planejada continua sendo uma gMSA exclusiva do projeto, atualmente sugerida como `gMSA_CadColab$`, autorizada somente para o servidor `SV052022-6121$`.

Nenhuma gMSA, grupo, ACL ou configuração IIS foi criada/alterada nesta etapa.

## Próxima etapa

Antes da primeira escrita:

1. confirmar KDS root key e níveis funcionais por leitura;
2. registrar baseline do container `CN=Managed Service Accounts`;
3. preparar o comando exato de criação e o rollback, sem executar;
4. solicitar autorização explícita antes da criação.

---

## CHECKPOINT-V024-KDS-NIVEIS-FUNCIONAIS-E-CONTAINER-GMSA.md

# CHECKPOINT V024 — KDS, níveis funcionais e container de gMSA

Data: 23/09/2026

## Resultado

Testes somente leitura.

- `DomainMode`: `Windows2016Domain`.
- `ForestMode`: `Windows2008R2Forest`.
- `Get-KdsRootKey` voltou a não exibir dados na projeção usada. Esse retorno não será tratado como ausência da chave, porque o projeto já confirmou anteriormente um objeto `msKds-ProvRootKey` no Configuration Naming Context.
- Container de contas de serviço: `CN=Managed Service Accounts,DC=automind,DC=com,DC=br`.
- Owner do container: `AUTOMIND\Domain Admins`.
- Herança de ACL habilitada (`AreAccessRulesProtected=False`).
- 36 regras observadas no baseline.
- Contas existentes: `provAgentgMSA` / `pGMSA_c5fa29e8$` e `ADSyncMSA82bf3$`.

## Decisão

- Não executar `Add-KdsRootKey` / `New-KdsRootKey`.
- Não elevar nível funcional de domínio ou floresta como parte deste projeto.
- Antes da criação da gMSA do CadastroColaboradores, reconfirmar diretamente o objeto KDS já existente e testar sua configuração com `Test-KdsRootKey`.
- A divergência entre `Get-KdsRootKey` vazio e o objeto KDS existente será tratada como comportamento a investigar, sem alteração de infraestrutura.
- Nenhuma alteração foi executada.

## Próxima etapa

Somente leitura no AD:

1. consultar diretamente `CN=Master Root Keys,CN=Group Key Distribution Service,CN=Services,CN=Configuration,...`;
2. obter o GUID da chave encontrada;
3. executar `Test-KdsRootKey -KeyId <GUID>`;
4. parar e documentar o resultado antes de qualquer criação de gMSA.

---

## CHECKPOINT-V025-KDS-VALIDADO-GMSA.md

# CHECKPOINT V025 - KDS VALIDADO PARA GMSA

Data: 23/09/2026

## Resultado

A chave KDS existente foi confirmada diretamente no AD:

- KeyId: `74b88b84-4f70-b3b4-a399-127cf4723c34`;
- whenCreated: `12/04/2023 19:03:39`;
- `Test-KdsRootKey`: `True`.

## Decisao

O dominio esta pronto, no requisito KDS, para uma nova gMSA. Nao executar `Add-KdsRootKey`.

## Proxima etapa

Somente simulacao da criacao da `gMSA_CadColab$` com `New-ADServiceAccount -WhatIf`.

A criacao real somente podera ocorrer depois de:

1. revisar a simulacao;
2. registrar o comando definitivo;
3. registrar o rollback (`Remove-ADServiceAccount` somente para o objeto criado pelo projeto);
4. obter autorizacao explicita.

Nenhuma alteracao foi executada neste checkpoint.

---

## CHECKPOINT-V026-WHATIF-GMSA-VALIDADO.md

# CHECKPOINT V026 - WHATIF DA GMSA VALIDADO

Data: 23/09/2026

## Resultado

A simulacao da criacao da identidade tecnica retornou:

`What if: Performing the operation "New" on target "CN=gMSA_CadColab,CN=Managed Service Accounts,DC=automind,DC=com,DC=br".`

O alvo corresponde ao planejado e o `-WhatIf` nao alterou o AD.

## Primeira escrita proposta

Criar somente `gMSA_CadColab$` no container `Managed Service Accounts`, autorizando apenas `SV052022-6121$` a recuperar a senha gerenciada.

Nesta etapa nao:

- instalar a gMSA no servidor;
- alterar o App Pool;
- criar grupo tecnico;
- alterar ACL de OU/grupo;
- reiniciar AD/DC/IIS/servidor.

## Validacao

Apos a criacao, executar apenas `Get-ADServiceAccount` para confirmar os atributos esperados.

## Rollback preparado

Se houver desvio, remover somente `gMSA_CadColab` com `Remove-ADServiceAccount` depois de confirmar que o objeto e o criado nesta etapa.

## Gate

A criacao real depende de autorizacao explicita do operador. Nenhuma escrita foi executada neste checkpoint.

---

## CHECKPOINT-V027-GMSA-CRIADA-E-CONTRATO-ESCRITA-AD.md

# CHECKPOINT V027 - gMSA criada e contrato de escrita AD

Data: 23/09/2026

## Alteracao executada

Foi criada no Active Directory a identidade tecnica exclusiva do CadastroColaboradores:

- `gMSA_CadColab$`
- habilitada;
- DNS `gMSA_CadColab.automind.com.br`;
- DN `CN=gMSA_CadColab,CN=Managed Service Accounts,DC=automind,DC=com,DC=br`;
- somente `SV052022-6121$` autorizado a recuperar a senha gerenciada.

## Nao alterado

Nesta etapa nao foram executados:

- `Install-ADServiceAccount` no servidor;
- alteracao de App Pool;
- criacao do grupo tecnico de delegacao;
- delegacao de OU;
- delegacao de `Write Members` em grupos;
- criacao/modificacao de usuario de colaborador;
- restart de AD, servidor ou IIS.

## Contrato de escrita

Foi criado `Docs/16-CONTRATO-ESCRITA-AD.md` para definir de forma objetiva:

- identidade tecnica;
- atributos que o sistema podera escrever;
- atributos bloqueados;
- regra de `manager` por DN;
- OU por allowlist de DN;
- grupos por allowlist separada;
- criacao inicial desabilitada e habilitacao somente ao final;
- auditoria;
- rollback;
- bloqueios obrigatorios.

## Estado

A gMSA existe, mas ainda nao possui delegacao nem esta ativa no IIS. A escrita de usuarios continua bloqueada.

---

## CHECKPOINT-V028-GMSA-INSTALADA-NO-SERVIDOR.md

# CHECKPOINT V028 - gMSA instalada no servidor da aplicacao

Data: 23/09/2026

## Alteracao executada

No servidor `10.1.2.21` (`SV052022-6121`) foi executado:

`Install-ADServiceAccount gMSA_CadColab`

Antes da instalacao:

- `Test-ADServiceAccount -Identity gMSA_CadColab` retornou `True`;
- `Install-ADServiceAccount -Identity gMSA_CadColab -WhatIf` apontou somente para `CN=gMSA_CadColab,CN=Managed Service Accounts,DC=automind,DC=com,DC=br`.

A instalacao real terminou sem erro.

## Estado atual

- gMSA existe no AD;
- somente `SV052022-6121$` pode recuperar a senha gerenciada;
- gMSA foi instalada/localmente preparada no servidor da aplicacao;
- App Pool `CadastroColaboradores` ainda nao foi alterado;
- grupo tecnico ainda nao foi criado;
- nenhuma ACL de OU/grupo foi alterada;
- nenhuma criacao/modificacao de colaborador foi executada.

## Rollback desta etapa

Se for necessario desfazer somente a instalacao local:

`Uninstall-ADServiceAccount -Identity gMSA_CadColab`

Nao remover o objeto da gMSA do AD como rollback desta etapa, pois a criacao do objeto pertence a etapa anterior.

## Proxima etapa

Antes de criar o grupo tecnico, levantar o padrao de grupos de seguranca existente e definir onde o grupo `SG_CadastroColaboradores_AD_Writer` sera criado. Permanecer em leitura ate essa definicao.

---

## CHECKPOINT-V029-GMSA-VALIDADA-E-GRUPO-TECNICO.md

# CHECKPOINT V029 - gMSA validada e grupo tecnico

Data: 23/09/2026

## Resultado

No servidor `SV052022-6121`, `Test-ADServiceAccount -Identity gMSA_CadColab` retornou `True` apos a instalacao local.

No AD:

- `OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br` existe;
- nao existem grupos `SG_*` no levantamento atual;
- `_CriaMovePastas`: `Global / Security`;
- `_Informatica`: `Global / Security`.

## Decisao provisoria

Manter como candidato:

`SG_CadastroColaboradores_AD_Writer`

Configuracao proposta:

- `GroupCategory=Security`;
- `GroupScope=Global`;
- OU `06.Grupos-Gerais`;
- uso exclusivo para delegacao tecnica do CadastroColaboradores;
- membro inicial futuro: somente `gMSA_CadColab$`.

`_Informatica` nao recebe permissao tecnica de escrita no AD. Ela permanece somente como autorizacao do operador no sistema.

## Proxima etapa

Somente leitura/simulacao:

1. baseline da ACL da OU `06.Grupos-Gerais`;
2. confirmar que `SG_CadastroColaboradores_AD_Writer` nao existe;
3. executar `New-ADGroup -WhatIf`.

Nenhuma nova escrita foi executada neste checkpoint.

---

## CHECKPOINT-V030-GRUPO-TECNICO-WHATIF.md

# CHECKPOINT V030 - baseline e simulacao do grupo tecnico

Data: 23/09/2026

## Baseline

OU alvo:

`OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br`

Estado observado:

- owner: `AUTOMIND\\Domain Admins`;
- heranca nao bloqueada;
- 40 ACEs;
- nenhuma colisao para `SG_CadastroColaboradores_AD_Writer`.

## Simulacao

Foi executado `New-ADGroup -WhatIf` para:

- nome: `SG_CadastroColaboradores_AD_Writer`;
- `sAMAccountName`: `SG_CadastroColaboradores_AD_Writer`;
- categoria: `Security`;
- escopo: `Global`;
- OU: `06.Grupos-Gerais`;
- descricao: `Delegacao tecnica AD do Automind.CadastroColaboradores`.

O `-WhatIf` confirmou o alvo:

`CN=SG_CadastroColaboradores_AD_Writer,OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br`

Nenhuma escrita ocorreu.

## Proxima mudanca proposta

Criar somente o grupo tecnico. Nessa mesma etapa nao adicionar a gMSA ao grupo, nao delegar OUs, nao alterar grupos de acesso e nao alterar IIS.

Validacao imediata apos a criacao:

- confirmar DN, escopo, categoria e descricao;
- confirmar que o grupo esta vazio.

Rollback preparado:

`Remove-ADGroup -Identity "SG_CadastroColaboradores_AD_Writer"`

O rollback so pode ser usado apos confirmar que o objeto alvo e o grupo criado por este projeto e que ainda nao recebeu dependencias adicionais.

---

## CHECKPOINT-V031-GRUPO-TECNICO-CRIADO.md

# CHECKPOINT V031 - Grupo tecnico criado

Data: 23/09/2026

## Alteracao executada

Criado no Active Directory:

- Nome: `SG_CadastroColaboradores_AD_Writer`
- `sAMAccountName`: `SG_CadastroColaboradores_AD_Writer`
- Tipo: `Global / Security`
- OU: `OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br`
- Descricao: `Delegacao tecnica AD do Automind.CadastroColaboradores`

## Validacao

O grupo foi consultado apos a criacao e retornou com os valores esperados. O campo `Members` esta vazio.

## Estado preservado

Nesta etapa nao foram executados:

- inclusao da gMSA no grupo;
- delegacao em OU;
- permissao `Write Members` em grupos;
- alteracao de identidade do App Pool;
- restart/reboot de AD, servidor ou IIS.

## Rollback da etapa

Se for necessario desfazer esta criacao antes de qualquer delegacao ou inclusao de membros:

`Remove-ADGroup -Identity "SG_CadastroColaboradores_AD_Writer"`

Nao executar sem autorizacao explicita.

## Proxima etapa

Validar o grupo vazio, validar a `gMSA_CadColab$` e simular a inclusao dela como unico membro inicial antes de qualquer escrita.

---

## CHECKPOINT-V032-ROLLBACK-COMPLETO-AD-E-MEMBERSHIP-WHATIF.md

# CHECKPOINT V032 - Rollback completo AD e simulacao de membership

Data: 23/09/2026
Projeto: Automind.CadastroColaboradores

## Resultado da rodada

- `SG_CadastroColaboradores_AD_Writer` confirmado sem membros;
- `gMSA_CadColab$` confirmada sem `MemberOf`;
- `Add-ADGroupMember -WhatIf` confirmou o grupo tecnico como alvo;
- nenhuma membership foi criada pelo `-WhatIf`;
- criada a documentacao `Docs/17-ROLLBACK-AD.md`;
- o rollback completo passa a ser requisito obrigatorio antes de cada nova escrita.

## Estado atual

- gMSA criada no AD;
- gMSA instalada e validada no `SV052022-6121`;
- grupo tecnico criado e vazio;
- gMSA ainda nao pertence ao grupo tecnico;
- nenhuma delegacao de OU ou grupo;
- IIS ainda nao usa a gMSA;
- escrita de usuarios continua bloqueada.

## Regra para continuidade

Antes de qualquer proxima mudanca, o checkpoint deve informar comando, efeito esperado, rollback e validacao. Uma alteracao por vez.

---

## CHECKPOINT-V033-GMSA-VINCULADA-GRUPO-TECNICO.md

# CHECKPOINT V033 - gMSA vinculada ao grupo tecnico

Data: 23/09/2026

## Mudanca executada

Foi adicionada somente a `gMSA_CadColab$` ao grupo `SG_CadastroColaboradores_AD_Writer`.

## Validacao

`Get-ADGroupMember` retornou:

- `Name`: `gMSA_CadColab`
- `SamAccountName`: `gMSA_CadColab$`
- `ObjectClass`: `msDS-GroupManagedServiceAccount`

`Get-ADServiceAccount -Properties MemberOf` confirmou:

`CN=SG_CadastroColaboradores_AD_Writer,OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br`

## Impacto

A associacao so vincula a identidade tecnica ao grupo de delegacao. Como nenhuma ACE foi concedida ao grupo, esta etapa nao adiciona por si so permissoes de criacao/alteracao de usuarios.

Nenhuma alteracao foi feita no IIS, OUs, usuarios ou ACLs.

## Rollback

```powershell
$svc=Get-ADServiceAccount "gMSA_CadColab";Remove-ADGroupMember -Identity "SG_CadastroColaboradores_AD_Writer" -Members $svc -Confirm:$false
```

Validar com `Get-ADGroupMember` e `MemberOf` da gMSA.

## Proxima etapa

Somente leitura:

1. confirmar que o grupo tecnico ainda nao possui ACEs no dominio/OU piloto;
2. mapear GUIDs de classe, atributos e direitos estendidos necessarios;
3. preparar delegacao e rollback exato antes de qualquer escrita de ACL.

---

## CHECKPOINT-V034-AUSENCIA-ACL-E-CORRECAO-GUIDS.md

# CHECKPOINT V034 - Ausencia de ACL e correcao do teste de GUIDs

Data: 23/09/2026

## Resultado

Consulta somente leitura confirmou que o grupo `AUTOMIND\SG_CadastroColaboradores_AD_Writer` ainda nao possui ACE no dominio nem nas OUs avaliadas, incluindo `Engenharia`.

O teste seguinte, destinado a mapear GUIDs da classe `user`, atributos e direito estendido `Reset Password`, falhou no parser do PowerShell com `EmptyPipeElement`.

## Impacto

Nenhuma alteracao ocorreu no Active Directory. O erro aconteceu antes de qualquer operacao de escrita e nenhuma delegacao foi aplicada.

## Proximo passo

Repetir somente o teste de mapeamento de GUIDs com sintaxe corrigida. Permanecem bloqueados `Set-Acl`, delegacoes, criacao de usuarios e mudancas no IIS.

---

## CHECKPOINT-V035-GUIDS-SCHEMA-RETORNADOS-PENDENTE-ROTULACAO.md

# CHECKPOINT V035 - GUIDs de schema retornados, rotulacao pendente

Data: 23/09/2026

## Resultado

Consulta somente leitura executada no AD.

Confirmado:

- classe `user`: `bf967aba-0de6-11d0-a285-00aa003049e2`;
- `Reset Password`: `00299570-246d-11d0-a768-00aa006e0529`.

A consulta tambem retornou 14 GUIDs de atributos na ordem solicitada, mas a coluna `lDAPDisplayName` ficou vazia. Para evitar qualquer delegacao baseada em inferencia, o proximo teste deve emitir cada nome de atributo junto do GUID correspondente.

## Estado

- `gMSA_CadColab$`: criada, instalada e validada;
- `SG_CadastroColaboradores_AD_Writer`: criado;
- gMSA membro do grupo tecnico;
- grupo tecnico ainda sem ACE de escrita;
- IIS ainda sem usar a gMSA;
- nenhuma ACL alterada nesta etapa.

## Regra

Nao aplicar `Set-Acl` antes de validar explicitamente o mapeamento `atributo -> GUID`.

---

## CHECKPOINT-V036-MAPEAMENTO-GUIDS-VALIDADO.md

# CHECKPOINT V036 - Mapeamento de GUIDs validado

Data: 23/09/2026

## Resultado

Consulta somente leitura confirmou o mapeamento explicito `atributo -> GUID` para os 14 atributos do contrato de escrita.

Tambem permanecem confirmados:

- classe `user`: `bf967aba-0de6-11d0-a285-00aa003049e2`;
- `Reset Password`: `00299570-246d-11d0-a768-00aa006e0529`.

## Seguranca

Nenhuma ACL, usuario, grupo, gMSA ou configuracao IIS foi alterada nesta etapa.

## Proximo passo

Montar as ACEs propostas somente em memoria, sem chamar `Set-Acl`, e revisar o conjunto antes de qualquer delegacao real.

A proposta inicial exclui permissoes amplas como `GenericWrite`, `GenericAll`, `WriteDacl`, `WriteOwner` e `DeleteChild`.

---

## CHECKPOINT-V037-ACES-MEMORIA-VALIDADAS.md

# CHECKPOINT V037 - ACEs em memoria validadas

Data: 23/09/2026

## Resultado

A proposta de delegacao para a OU piloto `Engenharia` foi montada somente em memoria.

Resultado observado:

- ACL real antes: `40` ACEs;
- ACL real depois da simulacao: `40` ACEs;
- regras construidas somente em memoria: `16`;
- nenhuma chamada `Set-Acl` foi executada;
- nenhuma permissao foi adicionada ao Active Directory.

## Conjunto proposto

As 16 regras sao:

- 1 `CreateChild` restrita a classe `user` na OU;
- 14 `WriteProperty`, uma por atributo aprovado no contrato;
- 1 `ExtendedRight` para `Reset Password`, restrita a objetos `user` descendentes.

Permissoes amplas continuam fora da proposta: `GenericWrite`, `GenericAll`, `DeleteChild`, `WriteDacl` e `WriteOwner`.

## Regra de senha no primeiro logon

`pwdLastSet` nao esta no contrato aprovado nem nas 16 ACEs. Portanto, a delegacao atual nao inclui permissao especifica para forcar troca de senha no primeiro logon.

Se essa regra for aprovada no futuro, `pwdLastSet` deve ser tratado como alteracao separada: mapear GUID, atualizar contrato, atualizar rollback, simular novamente e somente depois delegar.

## Proximo passo

Validar em memoria tambem o rollback das 16 ACEs: adicionar as regras a uma copia logica da ACL, remover exatamente as mesmas regras e confirmar retorno ao total inicial, sem `Set-Acl`.

---

## CHECKPOINT-V038-ROLLBACK-ACES-MEMORIA-VALIDADO.md

# CHECKPOINT V038 - Rollback das ACEs validado em memoria

Data: 23/09/2026

## Objetivo

Validar o rollback exato das 16 ACEs planejadas para a OU piloto `Engenharia` sem executar nenhuma escrita no Active Directory.

## Resultado

- ACL inicial: `40` ACEs.
- ACL simulada com as 16 regras: `56` ACEs.
- ACL apos rollback em memoria: `40` ACEs.
- ACL real no AD: `40` ACEs.
- Nenhum `Set-Acl` foi executado.

## Conclusao

A inclusao e a remocao das mesmas 16 regras foram validadas em memoria. A proxima etapa permanece sem escrita no AD: gerar e validar um baseline local da ACL/SDDL da OU antes de qualquer delegacao real.

## Regra de seguranca

A primeira delegacao real da OU `Engenharia` somente pode ocorrer depois de:

1. baseline da ACL salvo;
2. baseline conferido;
3. comando de escrita revisado;
4. rollback especifico revisado;
5. autorizacao explicita do usuario.

---

## CHECKPOINT-V039-BASELINE-07-OUTROS.md

# CHECKPOINT V039 - Baseline da OU piloto 07.Outros

Data: 23/09/2026

## Objetivo

Registrar o estado da OU escolhida para o primeiro piloto de delegacao real antes de qualquer `Set-Acl`.

OU piloto:

`OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

## Baseline confirmado

- ACEs: `40`
- Owner: `AUTOMIND\Domain Admins`
- Heranca bloqueada: `False`
- Nenhuma delegacao do projeto foi aplicada nesta etapa.

Arquivos locais de recuperacao criados no servidor AD:

- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-ACL-before.xml`
  - SHA-256: `2F8C1F0AB538CAEFAD17F3CC59631D87F741860270DD1F11B113AD66EB25106D`
- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-SDDL-before.txt`
  - SHA-256: `53FB30862FDB19A069F25BC7C182949315AA00D85CAD229D0EEC61931CFC64B3`

## Regra de seguranca

O arquivo salvo e evidencia de baseline. O rollback preferencial continua sendo remover somente as ACEs adicionadas pelo projeto, e nao restaurar cegamente a ACL completa.

## Proximo passo

Repetir a simulacao das 16 ACEs exclusivamente em memoria usando `07.Outros` como alvo. Nao executar `Set-Acl` ainda.

---

## CHECKPOINT-V040-SIMULACAO-07-OUTROS-VALIDADA.md

# CHECKPOINT V040 - Simulacao de delegacao na OU 07.Outros validada

Data: 23/09/2026

## Resultado

A simulacao das 16 ACEs foi executada somente em memoria para:

`OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

Resultado:

- ACL inicial: `40` ACEs;
- ACL simulada: `56` ACEs;
- rollback em memoria: `40` ACEs;
- ACL real no AD: `40` ACEs.

Nenhum `Set-Acl` foi executado.

## Conjunto simulado

- 1 `CreateChild` para objetos da classe `user`;
- 14 `WriteProperty` para os atributos aprovados no contrato de escrita;
- 1 `ExtendedRight` para `Reset Password`;
- sem `GenericWrite`, `GenericAll`, `DeleteChild`, `WriteDacl` ou `WriteOwner`.

## Baseline relacionado

Arquivos locais ja salvos no DC:

- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-ACL-before.xml`
- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-SDDL-before.txt`

Hashes registrados no V039:

- ACL XML: `2F8C1F0AB538CAEFAD17F3CC59631D87F741860270DD1F11B113AD66EB25106D`
- SDDL: `53FB30862FDB19A069F25BC7C182949315AA00D85CAD229D0EEC61931CFC64B3`

## Proxima etapa

A proxima etapa pode ser a primeira delegacao real de ACL, somente na OU `07.Outros`.

Antes da execucao devem estar disponiveis:

1. comando de inclusao das 16 ACEs;
2. validacao imediata apos a escrita;
3. rollback granular removendo somente essas 16 ACEs;
4. baseline local ja salvo para comparacao;
5. autorizacao explicita do operador.

Nao aplicar a mesma delegacao em outras OUs nesta etapa.

---

## CHECKPOINT-V041-AUTORIZACAO-ACL-PILOTO-07-OUTROS.md

# CHECKPOINT V041 - Autorizacao da ACL piloto em 07.Outros

Data: 23/09/2026

## Autorizacao

O operador autorizou explicitamente a primeira delegacao real de ACL do projeto, limitada a:

`OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

Nenhuma outra OU esta autorizada nesta etapa.

## Pre-condicoes ja validadas

- baseline da OU: 40 ACEs;
- owner: `AUTOMIND\Domain Admins`;
- heranca ativa;
- SDDL salvo localmente;
- simulacao em memoria: `40 -> 56 -> 40`;
- grupo tecnico sem ACE previa na OU;
- conjunto validado: 16 ACEs.

## Delegacao autorizada

Principal:

`AUTOMIND\SG_CadastroColaboradores_AD_Writer`

Regras:

- 1 `CreateChild` somente para objetos `user`;
- 14 `WriteProperty` somente para os atributos aprovados no contrato;
- 1 `ExtendedRight` de `Reset Password` somente para objetos `user` descendentes.

Nao incluir:

- `GenericAll`;
- `GenericWrite`;
- `DeleteChild`;
- `Delete`;
- `WriteDacl`;
- `WriteOwner`;
- outras classes ou atributos.

## Protecoes da execucao

O comando de escrita deve abortar antes do `Set-Acl` se:

- o baseline SDDL salvo nao existir;
- a ACL atual for diferente do baseline;
- o grupo tecnico ja possuir ACE na OU;
- a quantidade de regras preparada for diferente de 16.

Antes do `Set-Acl`, deve ser salvo novo snapshot local da ACL/SDDL.

## Validacao esperada

Apos a escrita:

- owner continua `AUTOMIND\Domain Admins`;
- heranca continua ativa;
- ACE total esperado: 56;
- ACEs do grupo tecnico esperadas: 16.

Qualquer divergencia interrompe o procedimento.

## Rollback

Rollback preferencial: remover somente as 16 ACEs adicionadas pelo projeto com `RemoveAccessRuleSpecific` e executar um unico `Set-Acl`.

Nao restaurar cegamente a ACL completa do baseline.

O baseline completo permanece somente como evidencia e recuperacao controlada.

---

## CHECKPOINT-V042-ACL-PILOTO-APLICADA-VALIDACAO-DETALHADA-PENDENTE.md

# CHECKPOINT V042 - ACL piloto aplicada; validacao detalhada pendente

Data: 23/09/2026

## Escopo

OU piloto:

`OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

Principal tecnico:

`AUTOMIND\SG_CadastroColaboradores_AD_Writer`

## Resultado da escrita real

O comando protegido de `Set-Acl` foi executado e retornou:

`ACL PILOTO APLICADA`

Validacao estrutural:

- ACE total: `56`;
- ACEs do grupo tecnico: `16`;
- owner: `AUTOMIND\Domain Admins`;
- heranca bloqueada: `False`;
- todas as ACEs listadas do projeto: `IsInherited=False`.

## Evidencias pos-escrita

Arquivos locais:

- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-ACL-after-pilot.xml`
- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-SDDL-after-pilot.txt`

SHA-256:

- XML: `5B2832851DA742D9AA4B3B768214282F694B41626BFC965ABE8F4B16E02C4584`
- SDDL: `31E8B76A144EFCB7CA83C8897651FB55FC3B1FDF362CCAA09CC3CC900BF95AC3`

## Anomalia observada na listagem

O conjunto esperado possui uma ACE `WriteProperty` para `givenName` e uma para `company`.

GUIDs esperados:

- `givenName`: `f0f8ff8e-1191-11d0-a060-00aa006c33ed`
- `company`: `f0f8ff88-1191-11d0-a060-00aa006c33ed`

Na saida recebida, `f0f8ff88-1191-11d0-a060-00aa006c33ed` apareceu duas vezes e `f0f8ff8e-1191-11d0-a060-00aa006c33ed` nao apareceu.

Ainda nao e possivel concluir se existe ACE incorreta no AD ou se ocorreu problema de exibicao/copia. A proxima acao deve ser exclusivamente de leitura, contando as ACEs por cada GUID esperado.

## Regra de seguranca

- nenhuma nova escrita enquanto a divergencia nao for esclarecida;
- nao executar rollback automaticamente;
- nao usar restauracao integral do SDDL;
- se a divergencia for confirmada, preparar correcao granular com efeito esperado, rollback e validacao antes de executar.

---

## CHECKPOINT-V043-ACL-PILOTO-VALIDADA-16-ACES.md

# CHECKPOINT V043 - ACL piloto validada com 16 ACEs exatas

Data: 23/09/2026

## Escopo

OU piloto:

`OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

Principal tecnico:

`AUTOMIND\SG_CadastroColaboradores_AD_Writer`

## Resultado da validacao detalhada

A consulta foi somente leitura e confirmou uma ACE para cada atributo aprovado:

| Atributo | GUID | ACEs |
|---|---|---:|
| `givenName` | `f0f8ff8e-1191-11d0-a060-00aa006c33ed` | 1 |
| `sn` | `bf967a41-0de6-11d0-a285-00aa003049e2` | 1 |
| `displayName` | `bf967953-0de6-11d0-a285-00aa003049e2` | 1 |
| `description` | `bf967950-0de6-11d0-a285-00aa003049e2` | 1 |
| `physicalDeliveryOfficeName` | `bf9679f7-0de6-11d0-a285-00aa003049e2` | 1 |
| `telephoneNumber` | `bf967a49-0de6-11d0-a285-00aa003049e2` | 1 |
| `mail` | `bf967961-0de6-11d0-a285-00aa003049e2` | 1 |
| `title` | `bf967a55-0de6-11d0-a285-00aa003049e2` | 1 |
| `department` | `bf96794f-0de6-11d0-a285-00aa003049e2` | 1 |
| `company` | `f0f8ff88-1191-11d0-a060-00aa006c33ed` | 1 |
| `manager` | `bf9679b5-0de6-11d0-a285-00aa003049e2` | 1 |
| `userPrincipalName` | `28630ebb-41d5-11d1-a9c1-0000f80367c1` | 1 |
| `sAMAccountName` | `3e0abfd0-126a-11d0-a060-00aa006c33ed` | 1 |
| `userAccountControl` | `bf967a68-0de6-11d0-a285-00aa003049e2` | 1 |

Direitos adicionais:

- `CreateChild User`: 1;
- `Reset Password`: 1;
- total de ACEs do grupo tecnico: 16.

## Conclusao

A aparente duplicidade de `company` observada no V042 nao existe na ACL real. `givenName` tambem esta presente exatamente uma vez. A divergencia foi somente de exibicao/copia da primeira listagem.

Nenhuma correcao foi aplicada e nenhum rollback foi executado. A ACL piloto permanece valida em `07.Outros`.

## Estado apos esta etapa

- gMSA instalada no servidor e membro do grupo tecnico;
- ACL piloto aplicada somente em `07.Outros`;
- nenhuma delegacao em outras OUs;
- nenhum `Write Members` em grupos;
- App Pool ainda em `ApplicationPoolIdentity`;
- aplicacao ainda sem escrita de usuario habilitada.

## Proximo passo

Executar somente leitura no servidor `10.1.2.21` para confirmar que o AD consultado pelo servidor ja apresenta as 16 ACEs da OU piloto. Somente depois preparar baseline e rollback da futura troca do App Pool para a gMSA.

---

## CHECKPOINT-V044-IIS-GMSA-PREREQUISITOS-E-BACKUP.md

# CHECKPOINT V044 - IIS/gMSA: pre-requisitos e etapa de backup

Data: 23/09/2026.

## Validado

- `CadastroColaboradores` continua em `ApplicationPoolIdentity`.
- `LogonType=LogonBatch`.
- `manualGroupMembership=False`.
- `LoadUserProfile=True`.
- anonimo habilitado como `IUSR`.
- Windows Authentication desabilitada.
- `IIS_IUSRS` sem membros explicitamente listados.
- `SeBatchLogonRight` inclui `S-1-5-32-568` (`IIS_IUSRS`).
- `SeDenyBatchLogonRight` nao configurado.
- gMSA instalada e valida no servidor.

## Decisao

Nao adicionar a gMSA explicitamente a `IIS_IUSRS` e nao alterar user rights preventivamente. Manter o comportamento padrao do IIS com `manualGroupMembership=False` e validar no piloto real do worker.

## Proxima etapa

Criar backup local do IIS e baseline nao secreto antes da primeira troca real para `AUTOMIND\gMSA_CadColab$`.

A troca continua bloqueada ate existir autorizacao explicita e rollback preparado.

---

## CHECKPOINT-V044-VALIDACAO-SERVIDOR-FALHA-TIPO-HOSTNAME.md

# CHECKPOINT V044 - Validacao no servidor interrompida por tipo de HostName

Data: 23/09/2026

## Escopo

Servidor de aplicacao: `10.1.2.21` / `SV052022-6121`.

Objetivo: confirmar, somente por leitura, que o DC descoberto pelo servidor apresenta a ACL piloto da OU `07.Outros` e a membership da `gMSA_CadColab$` no grupo tecnico.

## Resultado

O DC descoberto foi exibido como:

`SV062022-6158.automind.com.br`

Entretanto, a propriedade `HostName` retornada por `Get-ADDomainController -Discover -Service ADWS` foi mantida no PowerShell como `Microsoft.ActiveDirectory.Management.ADPropertyValueCollection`. Ao repassa-la diretamente para `-Server`, os cmdlets `Get-ADObject`, `Get-ADGroup` e `Get-ADGroupMember` falharam no binding do parametro.

As linhas finais `ACE TOTAL: 0`, `ACE GRUPO TECNICO: 0` e `GMSA NO GRUPO: False` nao representam o estado do AD; sao consequencia das consultas anteriores terem falhado e as variaveis nao terem sido populadas.

## Impacto

- nenhuma escrita no AD;
- nenhuma alteracao no IIS;
- nenhuma alteracao no servidor;
- nenhuma evidência de problema de replicacao;
- ACL piloto da OU `07.Outros` permanece como validada no V043.

## Correcao

Repetir o teste somente leitura convertendo explicitamente o primeiro valor de `HostName` para `System.String` antes de usa-lo em `-Server`.

## Estado

Parar antes de qualquer mudanca no App Pool. Somente prosseguir depois de obter `ACE TOTAL=56`, `ACE GRUPO TECNICO=16` e `GMSA NO GRUPO=True` a partir do servidor `10.1.2.21`.

---

## CHECKPOINT-V045-ACL-E-MEMBERSHIP-VISIVEIS-NO-SERVIDOR.md

# CHECKPOINT V045 - ACL e membership visiveis no servidor da aplicacao

Data: 23/09/2026

## Escopo

Servidor: `10.1.2.21` / `SV052022-6121`.

Objetivo: confirmar por leitura que o DC usado pelo servidor apresenta a delegacao piloto da OU `07.Outros` e a membership da gMSA no grupo tecnico.

## Resultado

- DC consultado: `SV062022-6158.automind.com.br`;
- tipo da variavel usada em `-Server`: `System.String`;
- `ACE TOTAL`: `56`;
- `ACE GRUPO TECNICO`: `16`;
- `GMSA NO GRUPO`: `True`.

## Conclusao

O servidor da aplicacao enxerga o estado esperado do AD:

1. a OU piloto `07.Outros` possui as 16 ACEs tecnicas validadas;
2. `gMSA_CadColab$` e membro de `SG_CadastroColaboradores_AD_Writer`;
3. o DC consultado apresenta o mesmo estado necessario ao piloto.

Nenhuma escrita foi executada nesta validacao.

## Proximo passo

Antes de qualquer mudanca no IIS:

- reler a configuracao atual do App Pool `CadastroColaboradores`;
- confirmar estado e identidade configurada;
- confirmar a identidade efetiva do worker atual;
- validar novamente `Test-ADServiceAccount gMSA_CadColab` no servidor;
- nao exibir, copiar ou registrar senha residual do App Pool;
- preparar comando de mudanca, efeito esperado, rollback e validacao antes de qualquer escrita.

---

## CHECKPOINT-V046-BASELINE-IIS-PARCIAL-E-FALHA-ADD-TYPE.md

# CHECKPOINT V046 - Baseline IIS parcial e falha de Add-Type

Data: 23/09/2026

## Escopo

Servidor: `10.1.2.21` / `SV052022-6121`.

Objetivo: capturar o estado do App Pool `CadastroColaboradores` antes de qualquer troca de identidade para a `gMSA_CadColab$`.

## Resultado recebido

A tentativa de carregar `Microsoft.Web.Administration` pelo nome simples com `Add-Type -AssemblyName Microsoft.Web.Administration` retornou erro de assembly nao encontrado.

Apesar disso, a sequencia posterior retornou informacoes do App Pool:

- pool: `CadastroColaboradores`;
- estado: `Started`;
- `IdentityType`: `ApplicationPoolIdentity`;
- `UserName` configurado: `AUTOMIND\\CriaMovePastas`;
- `LoadUserProfile`: `True`;
- `StartMode`: `OnDemand`;
- runtime vazio, consistente com aplicacao ASP.NET Core hospedada no IIS;
- pipeline: `Integrated`;
- `Test-ADServiceAccount gMSA_CadColab`: `True`;
- nenhum worker ativo foi encontrado naquele instante.

## Interpretacao segura

1. Nenhuma alteracao foi feita no IIS.
2. O erro de `Add-Type` e de carregamento, nao de configuracao do App Pool.
3. Como a sequencia conseguiu retornar propriedades do pool mesmo apos o erro, nao sera usado esse caminho ambiguo como baseline definitivo.
4. O baseline definitivo deve ser repetido por `WebAdministration`/`appcmd`, sem ler ou exibir senha.
5. `ApplicationPoolIdentity` continua sendo a identidade efetiva configurada; o `UserName` residual `AUTOMIND\\CriaMovePastas` nao deve ser interpretado como identidade efetiva enquanto `IdentityType` permanecer `ApplicationPoolIdentity`.
6. Ausencia de worker e compativel com pool `OnDemand` sem requisicao ativa e nao deve ser tratada como falha.

## Estado de seguranca

- nenhuma troca de identidade autorizada ainda;
- nenhuma alteracao de ACL NTFS autorizada ainda;
- nao executar `iisreset`;
- nao reiniciar servidor;
- nao reciclar o pool antes de existir comando de mudanca + rollback + validacao;
- nao registrar ou exibir senha residual do App Pool.

## Proximo passo

Executar apenas leitura no servidor para:

1. confirmar o baseline do App Pool usando `WebAdministration`/`appcmd` sem `Add-Type` e sem senha;
2. identificar aplicacao/site vinculados ao pool e eventual worker ativo;
3. revisar ACL NTFS da pasta da aplicacao e diretorios potencialmente gravaveis antes de qualquer mudanca para gMSA.

---

## CHECKPOINT-V047-BASELINE-IIS-NTFS-CONFIRMADO.md

# CHECKPOINT V047 - Baseline IIS e NTFS confirmado

Data: 23/09/2026

## Escopo

Servidor: `10.1.2.21` / `SV052022-6121`.

Objetivo: fechar o baseline do App Pool `CadastroColaboradores` e da pasta publicada antes de qualquer troca de identidade para a `gMSA_CadColab$`.

## Resultado do App Pool

Leitura via `WebAdministration`/`appcmd`, sem alteracao:

- pool: `CadastroColaboradores`;
- estado: `Started`;
- `IdentityType`: `ApplicationPoolIdentity`;
- `UserName` configurado: `AUTOMIND\\CriaMovePastas`;
- `LoadUserProfile`: `True`;
- `StartMode`: `OnDemand`;
- runtime vazio;
- pipeline: `Integrated`;
- aplicacao vinculada: `CadastroColaboradores/`;
- nenhum worker ativo no instante da leitura.

Interpretacao: `ApplicationPoolIdentity` continua sendo a identidade efetiva configurada. O valor `AUTOMIND\\CriaMovePastas` permanece residual no campo `UserName` e nao deve ser tratado como identidade efetiva enquanto `IdentityType=ApplicationPoolIdentity`.

## ACL NTFS da publicacao

Pasta: `C:\\Automind.CadastroColaboradores`.

ACEs observadas:

- `NT AUTHORITY\\SYSTEM`: `FullControl` herdado;
- `BUILTIN\\Administrators`: `FullControl` herdado;
- `BUILTIN\\Users`: `ReadAndExecute, Synchronize` herdado;
- `BUILTIN\\Users`: `AppendData` herdado em containers;
- `BUILTIN\\Users`: `CreateFiles` herdado em containers;
- `AUTOMIND\\daniel.gusmao.d`: `FullControl` herdado;
- `CREATOR OWNER`: ACE herdada `InheritOnly`.

Diretorios de primeiro nivel:

- `Extensions`;
- `runtimes`;
- `wwwroot`.

## Conclusoes de seguranca

1. Nenhuma alteracao foi feita em IIS, AD ou NTFS.
2. Ainda nao assumir que a gMSA recebe `BUILTIN\\Users`; isso deve ser confirmado no servidor antes de depender dessas ACEs.
3. Ainda nao conceder nova ACL NTFS a `gMSA_CadColab$` sem comprovar necessidade de leitura/escrita da aplicacao.
4. Antes da troca do App Pool, capturar configuracao de autenticacao e um baseline exportavel do pool/site, sem senha em texto claro.
5. Manter rollback preparado para restaurar `ApplicationPoolIdentity` e remover somente ACLs NTFS adicionadas especificamente para a gMSA, caso alguma seja necessaria.
6. Nao executar `iisreset`.

## Proximos testes permitidos

Somente leitura no servidor `10.1.2.21`:

1. confirmar os membros do grupo local `BUILTIN\\Users` e verificar se `Authenticated Users`/outro principal aplicavel explica o acesso da gMSA;
2. ler configuracao de autenticacao da aplicacao e exportar configuracao nao secreta do pool/site;
3. somente depois definir se a gMSA precisa de ACE NTFS propria.

---

## CHECKPOINT-V048-GRUPO-LOCAL-E-PROPRIEDADES-IIS-PARCIAIS.md

# CHECKPOINT V048 - Grupo local e propriedades IIS parciais

Data: 23/09/2026

## Objetivo

Confirmar, somente por leitura, a composicao do grupo local `Users`, propriedades da `gMSA_CadColab` e parametros do App Pool `CadastroColaboradores` antes de qualquer troca de identidade no IIS.

## Resultado - servidor 10.1.2.21

### Grupo local BUILTIN\\Users

Membros retornados:

- `AUTOMIND\\Domain Users`;
- `NT AUTHORITY\\Authenticated Users`;
- `NT AUTHORITY\\INTERACTIVE`.

### gMSA

- Name: `gMSA_CadColab`;
- SamAccountName: `gMSA_CadColab$`;
- Enabled: `True`;
- PrimaryGroupID: `515`.

### App Pool CadastroColaboradores

- `IdentityType=ApplicationPoolIdentity`;
- `LogonType=LogonBatch`;
- `LoadUserProfile=True`;
- `SetProfileEnvironment=True`;
- binding HTTP: `10.1.2.21:80:cadastro.automind.com.br`.

A consulta de `anonymousAuthentication` e `windowsAuthentication` retornou objetos `ConfigurationAttribute` em vez dos valores booleanos. Portanto, esses tres campos ainda nao estao validados e devem ser repetidos acessando explicitamente `.Value`.

## NTFS

Nenhuma ACE NTFS foi criada para a gMSA. Como `BUILTIN\\Users` contem `Authenticated Users`, o acesso existente pode ser suficiente para leitura/execucao apos um logon autenticado da gMSA, mas isso nao sera assumido como concluido antes da validacao funcional do worker.

## Estado de seguranca

- nenhuma alteracao no IIS;
- nenhuma alteracao no AD;
- nenhuma alteracao NTFS;
- identidade do pool permanece `ApplicationPoolIdentity`;
- troca para gMSA continua nao executada.

## Proximo passo

1. repetir leitura das configuracoes de autenticacao usando `.Value`;
2. ler `manualGroupMembership` do App Pool e a composicao de `IIS_IUSRS`;
3. somente depois preparar backup local do IIS, comando de troca de identidade e rollback exato.

---

## CHECKPOINT-V049-BACKUP-IIS-E-REGRA-TESTES-CONSOLIDADOS.md

# CHECKPOINT V049 - Backup IIS e regra de testes consolidados

Data: 24/09/2026

## Backup IIS confirmado

No servidor `10.1.2.21`, foi criado o backup:

`CadColab-Pre-gMSA-20260924-082229`

Arquivos e hashes:

- `C:\Windows\System32\inetsrv\backup\CadColab-Pre-gMSA-20260924-082229\applicationHost.config`
  - SHA-256: `B46EE9FEA44440598CB5B3014A631E3FD89E78D4358FCF22DA90385853511840`
- `C:\Temp\CadastroColaboradores-Rollback\IIS-CadastroColaboradores-before-20260924-082229.txt`
  - SHA-256: `CD51C31F6CE9B87B1E09B00C5B5D48BAE2756CFB07946777FC3DE0755F8EE45B`

Nenhuma troca de identidade, recycle, restart, `iisreset`, alteracao de AD ou alteracao NTFS ocorreu durante o backup.

## Regra operacional nova

Para economizar tempo, varios testes podem ser consolidados em uma unica linha de PowerShell quando:

1. forem somente leitura ou simulacao;
2. forem executados no mesmo local;
3. nao introduzirem escrita real;
4. cada resultado puder ser distinguido na saida;
5. a linha impedir que uma falha intermediaria gere conclusao enganadora.

Pode haver mais de 3 validacoes na mesma linha.

A regra de seguranca para escrita continua inalterada: `ALTERACAO REAL` deve ocorrer uma mudanca por vez, com comando, efeito esperado, rollback e validacao preparados antes da execucao.

## Proximo passo

Preparar a troca controlada do App Pool `CadastroColaboradores` para `AUTOMIND\gMSA_CadColab$`, com:

- comando de alteracao real isolado;
- rollback restrito ao App Pool;
- validacoes consolidadas em uma unica linha de leitura apos a mudanca;
- sem `iisreset`;
- sem alterar outros pools, sites, AD ou NTFS.

---

## CHECKPOINT-V050-APPPOOL-GMSA-VALIDACAO-HTTP-INCONCLUSIVA.md

# CHECKPOINT V050 - App Pool gMSA / validacao HTTP inicial inconclusiva

Data: 24/09/2026
Servidor: `SV052022-6121` / `10.1.2.21`

## Alteracao real executada

O App Pool `CadastroColaboradores` foi configurado para:

- `IdentityType=SpecificUser`;
- `UserName=AUTOMIND\gMSA_CadColab$`;
- `LogonType=LogonBatch`.

A gMSA continuou valida no servidor: `Test-ADServiceAccount=True`.

## Resultado da primeira validacao

- pool: `Started`;
- requisicao enviada para `http://127.0.0.1/` com cabecalho `Host=cadastro.automind.com.br`;
- retorno: HTTP 404;
- worker do pool nao encontrado apos a requisicao;
- sem eventos WAS/W3SVC exibidos nos cinco minutos consultados.

## Interpretacao

O binding confirmado anteriormente e `10.1.2.21:80:cadastro.automind.com.br`. Portanto, testar contra `127.0.0.1` nao prova que a aplicacao vinculada a `10.1.2.21` foi acionada. O 404 nao deve ser usado isoladamente para concluir falha da gMSA nem para disparar rollback.

## Bloqueios

Ate a proxima validacao:

- nao mudar ACL NTFS;
- nao conceder direitos locais extras;
- nao executar `iisreset`;
- nao fazer nova alteracao no AD;
- nao restaurar backup integral do IIS.

## Rollback mantido preparado

Se a ativacao real do pool falhar quando o site correto for acionado, o rollback primario continua restrito ao pool:

`ApplicationPoolIdentity`

O backup `CadColab-Pre-gMSA-20260924-082229` permanece apenas como contingencia integral.

---

## CHECKPOINT-V051-APPPOOL-GMSA-VALIDADO.md

# CHECKPOINT V051 - App Pool gMSA validado ponta a ponta

Data: 24/09/2026
Servidor: `SV052022-6121` / `10.1.2.21`

## Estado validado

- App Pool: `CadastroColaboradores`;
- estado: `Started`;
- identidade: `SpecificUser`;
- usuario: `AUTOMIND\gMSA_CadColab$`;
- `Test-ADServiceAccount=True`;
- site `CadastroColaboradores`: `Started`;
- binding: `http/10.1.2.21:80:cadastro.automind.com.br`;
- aplicacao: `CadastroColaboradores/` usando o mesmo App Pool.

## Validacao HTTP

- via IP correto + Host `cadastro.automind.com.br`: HTTP `200`;
- via FQDN `http://cadastro.automind.com.br/`: HTTP `200`;
- URL final permaneceu `http://cadastro.automind.com.br/`.

## Worker

- worker criado com sucesso;
- PID observado no teste: `5820`;
- owner do processo: `AUTOMIND\gMSA_CadColab$`.

O PID e apenas evidência temporal do teste e pode mudar em recycle futuro.

## Eventos

Na janela consultada nao foram exibidos erros WAS/W3SVC. No log Application foi observado:

- provider: `IIS AspNetCore Module V2`;
- evento: `1032`;
- nivel: `Information`;
- mensagem: aplicacao `C:\Automind.CadastroColaboradores\` iniciada com sucesso.

## Conclusao

A gMSA esta efetivamente executando o worker do App Pool e a aplicacao responde HTTP 200. Nao ha indicacao de necessidade de ACL NTFS adicional, associacao manual a `IIS_IUSRS`, mudanca de direitos locais, `iisreset` ou rollback.

## Contingencia mantida

Permanece disponivel o backup pre-gMSA:

- `CadColab-Pre-gMSA-20260924-082229`.

O rollback primario permanece a restauracao exclusiva do App Pool para `ApplicationPoolIdentity`, somente se uma falha futura comprovadamente relacionada a identidade exigir isso.

## Regra operacional

Testes de leitura/simulacao no mesmo local podem ser consolidados em uma unica linha com varias validacoes. Alteracoes reais continuam uma por vez, sempre com efeito esperado, rollback e validacao preparados antes.

---

## CHECKPOINT-V052-CONFIG-READONLY-POS-GMSA.md

# CHECKPOINT V052 - Configuracao ReadOnly apos gMSA

Data: 24/09/2026

## Validacao executada

Local: servidor `10.1.2.21`.
Tipo: somente leitura.

Resultado:

- `MODE: ReadOnly`;
- `AD SERVER: 10.1.2.1`;
- `PEOPLE SEARCH BASE: OU=Automind,DC=automind,DC=com,DC=br`;
- `AUTHORIZED GROUP: _informatica`;
- `OU ALLOWLIST TOTAL: 22`;
- `07.OUTROS NA ALLOWLIST: False`;
- `M365 ENABLED: False`;
- `APPSETTINGS SHA256: 9C6DB1F3CE04EB96C99157DA8E2A99CAC0808B83448F32D933507E315DAB441A`;
- pool `Started`;
- identidade `SpecificUser`;
- usuario `AUTOMIND\\gMSA_CadColab$`;
- `Test-ADServiceAccount=True`;
- HTTP `200`.

## Conclusao

A infraestrutura IIS/gMSA esta operacional, mas a aplicacao continua explicitamente bloqueada para escrita. A OU piloto `07.Outros` ainda nao esta disponivel na allowlist da aplicacao.

Revisao da linha de codigo atual mostrou apenas interfaces/servicos de leitura do AD. Nao existe servico de escrita registrado nem endpoint/action de criacao de usuario. Logo, mudar somente o modo/configuracao nao deve ser usado como atalho para habilitar provisionamento.

## Proximo gate

Antes do piloto real de criacao de usuario:

1. implementar escrita AD explicita e separada;
2. restringir server-side o primeiro destino a `07.Outros`;
3. manter M365 desabilitado;
4. preparar rollback de codigo/configuracao;
5. buildar e revisar;
6. publicar somente apos autorizacao;
7. criar somente usuario ficticio de teste no primeiro write end-to-end;
8. uma alteracao real por vez.

## Regra operacional de testes

Testes somente leitura/simulacao no mesmo local podem ser consolidados em uma unica linha de PowerShell, inclusive mais de tres verificacoes. Escritas reais permanecem isoladas, cada uma com efeito esperado, rollback e validacao preparados.

---

## CHECKPOINT-V053-IMPLEMENTACAO-ESCRITA-PILOTO-PREPARADA.md

# CHECKPOINT V053 - Implementacao de escrita piloto preparada

Data: 24/09/2026

## Estado

Foi preparada no codigo a primeira implementacao de provisionamento AD, mas ela permanece **inativa** por configuracao.

`Automind:Mode=ReadOnly` continua sendo o valor entregue no `appsettings.json`.

Nenhuma publicacao, mudanca de configuracao no servidor ou criacao de usuario foi executada nesta etapa.

## Escopo preparado

- unica OU autorizada em `WriteAllowedOuDns`: `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- `07.Outros` adicionada tambem a allowlist de leitura do codigo para permitir pre-validacao;
- identidade tecnica obrigatoria: `AUTOMIND\gMSA_CadColab$`;
- operador humano revalidado em `_informatica` imediatamente antes da escrita;
- grupos continuam bloqueados;
- M365 continua desabilitado;
- `proxyAddresses` e `pwdLastSet` continuam fora do contrato.

## Fluxo implementado

`pre-validacao -> auditoria -> CreateChild desabilitado -> atributos -> senha -> manager -> releitura -> habilitar por ultimo -> releitura final -> auditoria`

Em falha depois do CreateChild:

- interromper imediatamente;
- tentar confirmar conta desabilitada;
- nao excluir automaticamente;
- exigir revisao manual.

## Auditoria

Arquivo configurado:

`C:\Automind.CadastroColaboradores\Logs\ProvisioningAudit.jsonl`

A senha temporaria nao integra o modelo de auditoria e nao e persistida pela aplicacao. Ela somente e retornada ao operador na resposta bem-sucedida da criacao.

## Arquivos principais novos/alterados

- `Services/IAdProvisioningWriteService.cs`;
- `Services/WindowsAdProvisioningWriteService.cs`;
- `Services/IProvisioningAuditService.cs`;
- `Services/FileProvisioningAuditService.cs`;
- `Models/AdProvisioningWriteModels.cs`;
- `Controllers/ColaboradoresController.cs`;
- `Services/IAdAuthenticationService.cs`;
- `Services/WindowsAdAuthenticationService.cs`;
- `Services/AdConnectionFactory.cs`;
- `Models/AdDirectoryModels.cs`;
- `Program.cs`;
- `Views/Colaboradores/Novo.cshtml`;
- `Views/Home/Index.cshtml`;
- `wwwroot/js/site.js`;
- `wwwroot/css/automind.css`;
- `appsettings.json`;
- documentacao do projeto.

## Validacoes realizadas no ambiente de geracao

- `appsettings.json` parseado com sucesso;
- `node --check wwwroot/js/site.js` sem erro;
- `Automind:Mode` confirmado como `ReadOnly`;
- `WriteAllowedOuDns` confirmado contendo somente `07.Outros`;
- `GroupWritesEnabled=false`;
- `Microsoft365.Enabled=false`.

Nao foi executado `dotnet build`: o ambiente de geracao nao possui .NET SDK.

## Regra operacional mantida

Testes somente leitura/simulacao no mesmo local podem ser consolidados em uma unica linha e conter mais de 3 verificacoes. Alteracoes reais continuam uma por vez, com efeito esperado, rollback e validacao preparados antes.

## Proximo passo

1. entregar o projeto completo com a implementacao preparada;
2. executar build em maquina com .NET 10 SDK;
3. corrigir qualquer erro de compilacao antes de publicacao;
4. publicar inicialmente ainda em `ReadOnly`;
5. validar site, leitura AD, OU piloto e caminho de auditoria;
6. somente depois preparar e autorizar a mudanca real para `PilotWrite`.

---

## CHECKPOINT-V054-BUILD-PILOTO-READONLY-APROVADO.md

# CHECKPOINT V054 - Build do piloto ReadOnly aprovado

Data: 24/09/2026

## Resultado do build

Build executado na maquina de desenvolvimento com .NET 10 SDK:

- `Automind.CadastroColaboradores net10.0`: sucesso;
- 0 erros;
- 15 avisos `CA1416` em `Services/AdConnectionFactory.cs`;
- os avisos correspondem ao uso de APIs Windows-only de `System.DirectoryServices`.

O projeto permanece destinado ao servidor Windows/IIS, portanto os avisos nao impediram a continuidade do piloto.

## Validacoes de seguranca apos o build

- `Automind:Mode=ReadOnly`;
- `AllowedOuDns`: 23 entradas;
- `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br` presente na allowlist de leitura;
- `WriteAllowedOuDns`: 1 entrada;
- unica OU de escrita: `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- `GroupWritesEnabled=False`;
- `Microsoft365.Enabled=False`;
- servico de escrita AD presente;
- servico de auditoria presente.

## Estado operacional

Nenhuma alteracao foi feita no servidor nesta etapa.

O App Pool de producao continua executando como `AUTOMIND\gMSA_CadColab$`, e a versao atualmente implantada permanece separada deste build local ate nova autorizacao de deploy.

## Regra para a proxima mudanca

A publicacao da nova versao sera uma **ALTERACAO REAL** no servidor e deve ocorrer mantendo `Automind:Mode=ReadOnly`.

Antes da publicacao devem estar preparados:

1. comando/processo de deploy;
2. efeito esperado;
3. rollback para a versao anterior;
4. validacao pos-deploy;
5. confirmacao de que `PilotWrite` continua desabilitado.

Nenhuma ativacao de escrita AD deve ocorrer junto com o deploy.

---

## CHECKPOINT-V055-PUBLISH-READONLY-E-PACOTE-RELEASE.md

# CHECKPOINT V055 - Publish ReadOnly validado e regra de pacote de release

Data: 24/09/2026

## Resultado do publish local

O publish foi executado na maquina de desenvolvimento com .NET 10 e concluido com sucesso.

Resultado informado:

- projeto `Automind.CadastroColaboradores` em `net10.0`;
- build/publish concluido com sucesso;
- 15 avisos `CA1416`, todos relacionados a APIs `System.DirectoryServices` suportadas apenas em Windows;
- nenhum erro de compilacao informado;
- pasta gerada: `artifacts\\CadastroColaboradores-ReadOnly-20260924-091040`.

## Validacoes de seguranca do artefato publicado

A validacao do conteudo publicado confirmou:

- `Automind:Mode=ReadOnly`;
- allowlist de leitura com 23 OUs;
- `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br` presente na allowlist de leitura;
- allowlist de escrita com exatamente 1 OU;
- unico alvo de escrita configurado: `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- `GroupWritesEnabled=False`;
- `Microsoft365.Enabled=False`;
- `Automind.CadastroColaboradores.dll` presente;
- `web.config` presente;
- hashes SHA-256 foram gerados para `appsettings.json`, DLL e ZIP do publish.

Os hashes completos nao foram registrados neste checkpoint porque a saida copiada na conversa foi apresentada truncada. Nao inferir ou completar valores de hash.

## Estado de seguranca

O pacote permanece bloqueado para escrita real porque `Automind:Mode=ReadOnly`.

Portanto, esta etapa nao habilitou:

- criacao de usuarios no AD;
- escrita de grupos;
- `proxyAddresses`;
- `pwdLastSet`;
- Microsoft 365;
- Teams;
- escrita no TOPdesk.

## Nova regra operacional de checkpoint/pacote

Durante a preparacao de um pacote completo para release/teste:

- nao gerar um ZIP de checkpoint separado a cada etapa;
- manter `Docs/CHECKPOINT.md`, `Docs/CP-HIST.md` e demais documentos atualizados dentro do proprio pacote completo do projeto;
- entregar o checkpoint junto com o pacote completo;
- voltar a gerar checkpoint separado apenas ao entrar em uma nova fase de testes, quando isso ajudar a continuidade, ou mediante pedido explicito;
- a regra anterior de consolidar varios testes somente leitura/simulacao em uma unica linha de PowerShell continua valida;
- alteracoes reais continuam uma por vez, com efeito esperado, rollback e validacao preparados previamente.

## Proximo marco

O proximo pacote pode seguir para commit/publicacao em `ReadOnly` para validacao do deploy. A ativacao `PilotWrite` continua sendo uma etapa separada e nao deve ocorrer implicitamente durante o deploy.

---

## CHECKPOINT-V056-FLUXO-VS-GIT-AZURE-DEVOPS.md

# CHECKPOINT V056 - Fluxo Visual Studio, Git e Azure DevOps

Data: 24/09/2026.

## Objetivo

Registrar o fluxo real usado pelo responsavel enquanto o CadastroColaboradores esta em fase de ajustes iniciais e testes frequentes no servidor.

## Fluxo confirmado

1. O projeto e aberto e ajustado no Visual Studio.
2. O pacote completo entregue pelo assistente e usado para atualizar a copia local do projeto.
3. Branch operacional atual: `release`.
4. Executar build local antes do envio.
5. Revisar `git status`/`git status --short`.
6. Registrar a rodada com commit descritivo.
7. Nao criar tag, versao numerica ou bump de versao nesta fase.
8. `origin` aponta para GitHub e atua como backup/espelho do codigo.
9. `azure` aponta para Azure DevOps Repos e alimenta o fluxo de CI/CD existente.
10. Enviar `release` aos dois remotes.
11. O Azure DevOps Pipeline/Release publica a nova rodada no servidor para teste visual e funcional.
12. O responsavel valida o comportamento no ambiente real e a proxima rodada segue o mesmo ciclo.

## Sequencia de referencia

```powershell
git branch --show-current
dotnet build -c Release
git status --short
git add .
git commit -m "<descricao objetiva da alteracao>"
git remote -v
git push origin release
git push azure release
```

## Regras

- Git e o historico/backup de codigo desta fase.
- Rollbacks de AD e IIS continuam independentes e seguem seus documentos especificos.
- O pacote completo contem `Docs/` e checkpoint.
- Nao gerar checkpoint ZIP separado enquanto esta fase estiver ativa.
- Nao copiar o ZIP de codigo-fonte diretamente para a pasta publicada do IIS como substituto do publish.
- Antes de `git add .`, revisar arquivos gerados. A pasta `artifacts/` observada no projeto pode conter publish/ZIP local e nao deve entrar em commit por acidente.
- O `.gitignore` atual ainda nao exclui `artifacts/`; qualquer alteracao nele deve ser deliberada e revisada antes de aplicar.

---

## V058 - CadColab v0.1.1 - sugestoes de grupos informativas no piloto

Data: 24/09/2026

- teste ReadOnly com `teste.cadcolab` concluiu a pre-validacao AD sem pendencias;
- a OU `05.Terceiros-Ext` observada no teste foi selecao acidental e nao altera o escopo do piloto;
- grupos comuns continuam marcados automaticamente, excecoes desmarcadas e protegidos bloqueados;
- com `GroupWritesEnabled=false`, as marcacoes passam a ser explicitamente informativas e nao bloqueiam a criacao piloto;
- o backend de criacao continua enviando `GroupDns=[]`, logo nenhuma membership e gravada;
- removida a exigencia de desmarcar grupos antes da criacao;
- nao foi criado indicador temporario de OU de escrita na pre-validacao;
- `WriteAllowedOuDns` continua sendo a trava real e permanece restrita a `07.Outros`;
- `Mode=ReadOnly`, M365, proxyAddresses e pwdLastSet permanecem bloqueados;
- pacote completo versionado como `CadColab-v0.1.1.zip`.



## V059 - CadColab v0.1.2 - limite visual do login sAMAccountName

Data: 24/09/2026

- mantido o limite tecnico ja existente no backend: `sAMAccountName` com no maximo 20 caracteres;
- campo **Login** passa a exibir permanentemente `max. 20 caracteres no AD`;
- adicionado contador visual `N / 20`;
- ao receber valor acima de 20 caracteres (por exemplo via importacao TOPdesk), a tela destaca o campo e informa quantos caracteres excederam o limite;
- edicao manual do campo usa `maxlength=20`, evitando novos caracteres acima do limite;
- a regra nao trunca silenciosamente um login ja importado: o usuario consegue identificar o excesso e ajustar conscientemente;
- validacao backend continua sendo a autoridade e retorna mensagem explicita com o tamanho recebido e o limite 20;
- motivacao registrada: `teste.provisionamento` excedeu o limite e precisou ser reduzido para `teste.cadcolab`;
- nenhuma regra de OU, grupo, M365, `proxyAddresses`, `pwdLastSet` ou escrita AD foi alterada;
- `Automind:Mode` permanece `ReadOnly`;
- pacote completo versionado como `CadColab-v0.1.2.zip`.


## V060 - CadColab v0.1.3 - ativacao controlada do PilotWrite

Data: 24/09/2026.

Autorizacao explicita do responsavel recebida para avancar de `ReadOnly` para `PilotWrite`.

Estado preparado no pacote:

- `Automind:Mode=PilotWrite`;
- `WriteAllowedOuDns` = somente `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- `GroupWritesEnabled=false`;
- grupos comuns continuam selecionados visualmente como sugestao, mas nenhuma membership e enviada ao writer (`GroupDns=[]`);
- `Microsoft365.Enabled=false`;
- `proxyAddresses` e `pwdLastSet` permanecem fora da escrita;
- `VERSION.txt` = `CadColab 0.1.3`;
- `Version` do projeto = `0.1.3`;
- `VERSION.txt` configurado para `CopyToOutputDirectory` e `CopyToPublishDirectory`, corrigindo a ausencia constatada no servidor;
- rollback de contencao: voltar `Mode=ReadOnly` e publicar;
- pacote: `CadColab-v0.1.3.zip`;
- sem tag Git automatica.

O pre-check imediatamente anterior confirmou no servidor: `Mode=ReadOnly`, unica OU de escrita `07.Outros`, App Pool Started, identidade `AUTOMIND\gMSA_CadColab$`, `Test-ADServiceAccount=True`, OU existente, gMSA no grupo tecnico e HTTP 200. O diretorio de auditoria ainda nao existia; isso e fail-safe porque o writer tenta iniciar a auditoria antes da primeira escrita AD e deve abortar se nao conseguir.


## 24/09/2026 - V061 / pacote v0.1.4: seleção de grupos ampliada sem escrita

Mudança de prioridade autorizada durante os testes: antes de habilitar escrita de grupos, completar a experiência de seleção e validação.

- pesquisa de referência continua usando `Title + Department` em todas as OUs abaixo de `PeopleSearchBase`;
- o login do colaborador em edição é excluído da coorte, evitando o caso observado em que `teste.cadcolab` transformou uma referência de 5/5 em 5/6 depois de ser criado;
- interface separa visualmente **Grupos comuns ao cargo**, **Exceções encontradas** e **Grupos protegidos**;
- incluído campo **Outros grupos** com pesquisa por nome/sAMAccountName diretamente no AD;
- grupos manuais podem ser selecionados quando não aparecem na coorte;
- o backend revalida todos os DNs selecionados e rejeita inexistentes ou protegidos;
- grupos protegidos continuam bloqueados também na busca manual;
- a prévia passa a incluir grupos selecionados manualmente;
- `Automind:Mode=PilotWrite` permanece;
- `WriteAllowedOuDns` continua somente `07.Outros`;
- `GroupWritesEnabled=false` permanece e o writer continua recebendo `GroupDns=[]`;
- nenhuma associação de usuário a grupo é habilitada nesta versão;
- nenhuma alteração de ACL/AD faz parte deste pacote;
- `VERSION.txt` e `.csproj` atualizados para `0.1.4`;
- a antiga previsão de usar `v0.1.4` para habilitar escrita de grupos fica superada por esta decisão; a escrita de grupos será versionada somente depois de ACL/allowlist e teste controlado aprovados.

Validação executável neste ambiente: `site.js` passou em `node --check`. O ambiente de geração não possui .NET SDK; build .NET 10 continua obrigatório na máquina do Visual Studio antes de commit/push.
