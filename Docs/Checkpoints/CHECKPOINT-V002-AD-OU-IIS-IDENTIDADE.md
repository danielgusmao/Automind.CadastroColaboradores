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

