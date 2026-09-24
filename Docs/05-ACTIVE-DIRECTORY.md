# Active Directory

## Estado atual - leitura real habilitada

A aplicação usa o Active Directory real para:

- autenticar usuário/senha no login;
- restringir o acesso inicial ao grupo configurado em `Automind:Ad:AuthorizedGroup`;
- listar OUs existentes diretamente no AD;
- filtrar OUs por `Automind:Ad:AllowedOuDns`, usando o DistinguishedName completo;
- localizar superior imediato entre usuários ativos;
- validar disponibilidade de `sAMAccountName`, UPN, `mail` e `proxyAddresses`;
- consultar usuários ativos por `Title + Department`;
- ler grupos diretos por `memberOf`;
- validar existência dos grupos selecionados;
- consultar grupos ancestrais para exibir efeitos indiretos de grupos de segurança.

As consultas de cadastro utilizam a identidade do processo IIS, sem senha administrativa armazenada na aplicação. Em `ApplicationPoolIdentity`, o acesso de rede deve ser validado com as permissões efetivas da identidade do servidor/aplicação.

## OUs

- Base de pesquisa: `Automind:Ad:PeopleSearchBase`.
- A lista é lida do AD em tempo real.
- Quando `AllowedOuDns` possui valores, somente DNs existentes na allowlist são exibidos.
- O valor submetido/validado é o DistinguishedName completo, nunca apenas o nome amigável.
- A aplicação valida novamente a OU antes de montar a prévia.

## Disponibilidade de identidade

Antes de qualquer futura criação, a pré-validação pesquisa no domínio inteiro colisões em:

- `sAMAccountName`;
- `userPrincipalName`;
- `mail` para os domínios corporativos envolvidos;
- `proxyAddresses` para o SMTP primário `@automind.co` e o alias `@automind.com.br`.

A busca não é limitada a objetos `user`, pois grupos/contatos também podem possuir atributos de e-mail conflitantes.

## SMTP

Regra confirmada no ambiente:

- UPN/mail normalmente: `usuario@automind.com.br`;
- `SMTP:` primário: `usuario@automind.co`;
- `smtp:` secundário: `usuario@automind.com.br`.

A prévia mostra ambos, mas nenhuma escrita é feita nesta entrega.

## Escrita continua proibida nesta entrega

Não existe rotina ativa para:

- `New-ADUser`;
- definir/redefinir senha;
- `Add-ADGroupMember` / remover membros;
- alterar `manager`;
- mover objetos;
- alterar `mail` ou `proxyAddresses`.

A próxima fase de escrita deverá ser apresentada separadamente e só poderá ser implementada/ativada após autorização explícita e definição da identidade técnica com permissão mínima necessária.

## Arquitetura planejada para escrita - gMSA + delegação mínima

### Situação atual

Os testes manuais de administração foram executados com usuário elevado somente para validar comportamento e permissões. Isso não deve se tornar dependência da aplicação.

No estado atual do IIS, o pool usa `ApplicationPoolIdentity`. Para acesso de rede, identidades de Application Pool usam a conta da máquina; no servidor atual isso corresponde a `AUTOMIND\\SV052022-6121$`. Esse comportamento já foi validado no diagnóstico e é documentado pela Microsoft.

### Estado alvo

A criação futura deve ser executada por uma gMSA dedicada do CadastroColaboradores, sugerida como `gMSA_CadColab$`, sem senha estática armazenada na aplicação.

A gMSA deve ser autorizada a recuperar sua senha gerenciada somente no servidor `SV052022-6121` / `10.1.2.21` (ou por grupo de hosts dedicado caso a arquitetura evolua para múltiplos nós).

A aplicação deverá manter `_informatica` apenas para autorização humana. Ser membro de `_informatica` não implica receber permissões LDAP fora do sistema.

### Delegação de criação/alteração de usuários

A delegação deverá ser concedida a um grupo técnico dedicado, sugerido `SG_CadastroColaboradores_AD_Writer`, e limitada às OUs aprovadas na allowlist.

Escopo funcional planejado:

- criar objetos `User`;
- definir `givenName`;
- definir `sn`;
- definir `displayName`;
- definir `description`;
- definir `physicalDeliveryOfficeName`;
- definir `telephoneNumber` quando permitido pela regra de divulgação;
- definir `mail`;
- definir `title`;
- definir `department`;
- definir `company`;
- definir `manager` usando o DistinguishedName do superior resolvido no AD;
- definir `userPrincipalName`;
- definir `sAMAccountName`;
- definir senha inicial;
- opcionalmente forçar troca de senha no primeiro logon, se esta regra for confirmada;
- habilitar a conta.

Não conceder permissão no domínio inteiro quando a operação pode ser limitada às OUs da allowlist.

### Delegação de grupos

A associação do usuário a grupos será tratada separadamente. A aplicação deverá modificar o atributo `member` somente de grupos explicitamente aprovados para gerenciamento.

Exemplos já estudados, ainda sujeitos à aprovação final de escrita:

- `_Todos`;
- `_Todos SSA`;
- `_Engenharia`;
- `_Técnica SSA`;
- `Dist_Engenharia`.

Não autorizar automaticamente alteração em:

- `Domain Admins`;
- `Enterprise Admins`;
- `_informatica`;
- grupos administrativos/protegidos;
- qualquer grupo não aprovado na allowlist de escrita.

### Auditoria

Mesmo quando o AD registrar `gMSA_CadColab$` como executor, o histórico do CadastroColaboradores deve preservar:

- operador humano;
- chamado TOPdesk;
- gMSA executora;
- CN/DN criado;
- OU de destino;
- atributos relevantes;
- grupos efetivamente adicionados;
- data/hora;
- sucesso/falha.

Senha inicial continua proibida em log, banco e histórico.

### Sequência operacional aprovada

1. criar gMSA exclusiva;
2. autorizar a gMSA somente no `10.1.2.21`;
3. configurar o App Pool IIS para usar a identidade técnica;
4. criar grupo técnico de delegação;
5. colocar somente a identidade técnica no mecanismo de delegação;
6. delegar permissões mínimas nas OUs aprovadas;
7. delegar alteração de membros apenas nos grupos aprovados;
8. testar leitura;
9. testar criação de usuário fictício;
10. testar senha;
11. testar `manager`;
12. testar grupos;
13. remover o usuário fictício ao final, se decidido.

### Fontes Microsoft

- https://learn.microsoft.com/en-us/iis/manage/configuring-security/application-pool-identities
- https://learn.microsoft.com/windows/security/identity-protection/access-control/service-accounts
- https://learn.microsoft.com/en-us/windows-server/identity/ad-ds/manage/group-managed-service-accounts/group-managed-service-accounts/manage-group-managed-service-accounts
- https://learn.microsoft.com/en-us/windows-server/identity/ad-ds/manage/delegation-control-wizard
- https://learn.microsoft.com/en-us/windows-server/identity/ad-ds/plan/delegating-administration-by-using-ou-objects

## Politica de seguranca operacional para a implantacao de escrita

A implantacao de gMSA/delegacao/escrita nao pode causar reinicio de controlador de dominio nem alteracao global desnecessaria.

Regras obrigatorias:

- primeiro somente leitura e baseline;
- uma mudanca por vez;
- rollback escrito/revisado antes da mudanca;
- validacao imediatamente depois;
- parada ao primeiro desvio;
- sem delegacao no dominio inteiro;
- sem Domain Admin/Enterprise Admin/Account Operators;
- sem permissao de escrita concedida a `_informatica`;
- sem `New-KdsRootKey`;
- sem reboot de DC;
- sem `iisreset`;
- sem reboot do `10.1.2.21` como procedimento de implantacao;
- delegacao de OU e delegacao de grupos tratadas como mudancas diferentes;
- piloto em uma OU antes de ampliar a allowlist de escrita;
- grupos recebem permissoes individualmente e somente se aprovados;
- qualquer eventual recycle do App Pool e considerado impacto de aplicacao e precisa de etapa/janela/autorizacao propria.

O rollback deve preferir a remocao das ACEs/associacoes criadas pelo projeto, em vez da restauracao integral e cega de ACLs.

A especificacao completa esta em `Docs/CP-HIST.md`

## Baseline de seguranca antes da identidade tecnica - 23/09/2026

Antes de qualquer escrita foi capturado o baseline da OU piloto `OU=Engenharia,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`.

- owner: `AUTOMIND\\Domain Admins`;
- heranca de ACL nao bloqueada (`AreAccessRulesProtected=False`);
- 40 ACEs observadas no snapshot;
- existem delegacoes preexistentes para `AUTOMIND\\pGMSA_c5fa29e8$`, identidade associada ao Entra/Azure AD Cloud Sync;
- entre elas aparecem direito de Reset Password e direitos de Create/Delete/GenericWrite para a classe `user`.

Regra: o CadastroColaboradores nao reutiliza essa gMSA e nao altera suas ACEs. Toda delegacao propria devera ser adicionada separadamente e ter rollback que remova apenas as ACEs do projeto, sem restauracao completa/cega da ACL.

## Baseline de herança — 23/09/2026

As ACEs observadas para `AUTOMIND\pGMSA_c5fa29e8$` em `OU=Automind`, `OU=03.UDN` e `OU=Engenharia` estão marcadas como herdadas. A origem está acima de `OU=Automind` e deve ser localizada antes de qualquer nova delegação. Não reutilizar nem alterar essa gMSA ou suas ACEs.

## Baseline da conta legada e do worker IIS — 23/09/2026

A conta `AUTOMIND\CriaMovePastas` existe, está habilitada, possui `PasswordNeverExpires=True` e participa dos grupos `_CriaMovePastas` e `_GAP1`. Ela não será alterada durante a implantação da identidade técnica até que suas dependências sejam mapeadas.

No servidor `10.1.2.21`, a pasta `C:\Automind.CadastroColaboradores` possui ACL herdada. No instante do teste não havia worker ativo do App Pool, portanto a identidade efetiva do `w3wp.exe` ainda precisa ser confirmada após um acesso normal ao site.

A credencial da conta legada apareceu em saída de diagnóstico. Ela não deve ser repetida nem registrada na documentação. Qualquer rotação será tratada como mudança separada, com análise de dependências, impacto e rollback próprios.

## Replicacao e identidade efetiva do IIS - 23/09/2026

- `repadmin /replsummary`: 0 falhas nos DCs `SV062022-6158` e `SV062022-9947`.
- O worker do App Pool `CadastroColaboradores` executa como `IIS APPPOOL\CadastroColaboradores`.
- A conta `CriaMovePastas` nao e a identidade efetiva do processo atual.
- Qualquer valor residual de `UserName` em App Pool com `IdentityType=ApplicationPoolIdentity` nao deve ser tratado como identidade operacional sem validacao do `w3wp.exe`.
- A futura gMSA do projeto sera criada separadamente e somente apos concluir os testes de prontidao.

## Pré-requisitos de gMSA confirmados — 23/09/2026

O servidor `SV052022-6121` possui canal seguro válido com o domínio, localiza o DC `SV062022-6158` (`10.1.2.1`) e está sincronizado com o horário do domínio.

As contas `pGMSA_c5fa29e8$` e `ADSyncMSA82bf3$` são preexistentes e não serão reutilizadas pelo CadastroColaboradores. A futura identidade técnica continua isolada em uma gMSA própria, autorizada somente no servidor da aplicação.

## KDS e níveis funcionais — validação adicional de 23/09/2026

- domínio: `Windows2016Domain`;
- floresta: `Windows2008R2Forest`;
- container `CN=Managed Service Accounts` confirmado, com owner `Domain Admins`, herança ativa e 36 ACEs no baseline;
- `Get-KdsRootKey` voltou a não exibir dados na projeção usada;
- esse retorno não invalida a confirmação anterior do objeto `msKds-ProvRootKey` existente no Configuration Naming Context;
- não criar nova KDS Root Key e não alterar níveis funcionais;
- próximo passo: reconfirmar o objeto KDS diretamente e testar sua configuração com `Test-KdsRootKey`, somente leitura.

## KDS validado para gMSA - 23/09/2026

A chave KDS existente foi confirmada diretamente no Configuration Naming Context:

- KeyId: `74b88b84-4f70-b3b4-a399-127cf4723c34`;
- criada em `12/04/2023 19:03:39`;
- `Test-KdsRootKey` retornou `True`.

Conclusao: o dominio ja possui KDS funcional para uso de gMSA. Nao executar `Add-KdsRootKey`.

A proxima etapa permanece controlada: preparar a criacao da `gMSA_CadColab$` com o servidor `SV052022-6121$` como unico principal autorizado a recuperar a senha gerenciada. Antes da escrita, usar `-WhatIf`, registrar o comando definitivo e o rollback exato.

## Simulacao da primeira escrita de gMSA - 23/09/2026

A simulacao com `New-ADServiceAccount -WhatIf` confirmou o alvo esperado:

`CN=gMSA_CadColab,CN=Managed Service Accounts,DC=automind,DC=com,DC=br`

Nenhuma escrita ocorreu.

A primeira mudanca, quando autorizada, sera limitada a criacao desse unico objeto. Nessa etapa nao sera executado `Install-ADServiceAccount`, nao sera alterado IIS/App Pool, nao sera criado grupo tecnico e nao sera modificada ACL de OU ou grupo.

Validacao pos-mudanca: consultar o objeto criado e confirmar nome, `sAMAccountName`, `Enabled`, `DNSHostName`, DN e `PrincipalsAllowedToRetrieveManagedPassword`.

Rollback: remover somente a gMSA criada pelo projeto. O rollback so sera executado se necessario e apos confirmacao do objeto alvo.

## gMSA do CadastroColaboradores criada - 23/09/2026

Objeto criado e validado:

- `Name`: `gMSA_CadColab`
- `sAMAccountName`: `gMSA_CadColab$`
- `Enabled`: `True`
- `DNSHostName`: `gMSA_CadColab.automind.com.br`
- `DistinguishedName`: `CN=gMSA_CadColab,CN=Managed Service Accounts,DC=automind,DC=com,DC=br`
- `PrincipalsAllowedToRetrieveManagedPassword`: somente `CN=SV052022-6121,CN=Computers,DC=automind,DC=com,DC=br`

Esta criacao foi a primeira escrita real do plano de identidade tecnica. Nenhuma outra mudanca foi feita nesta etapa: a gMSA ainda nao foi instalada no servidor, nao foi vinculada ao App Pool e nao recebeu ACL de OU/grupo.

O contrato de criacao/alteracao de usuarios esta em `Docs/16-CONTRATO-ESCRITA-AD.md` e deve ser tratado como limite tecnico da futura implementacao de escrita.

## gMSA instalada no servidor da aplicacao - 23/09/2026

No servidor `SV052022-6121`:

- `Test-ADServiceAccount gMSA_CadColab` retornou `True`;
- o `-WhatIf` de instalacao confirmou o objeto esperado;
- `Install-ADServiceAccount gMSA_CadColab` foi executado sem erro.

Estado: a gMSA esta preparada no host, mas ainda nao foi associada ao App Pool e ainda nao possui delegacao de escrita no AD.

Rollback local desta etapa: `Uninstall-ADServiceAccount -Identity gMSA_CadColab`.

## Preparacao do grupo tecnico de delegacao - 23/09/2026

Validacoes concluidas:

- `gMSA_CadColab$` esta instalada no `SV052022-6121` e `Test-ADServiceAccount` retorna `True`;
- a OU `06.Grupos-Gerais` existe;
- nao foi encontrado padrao preexistente `SG_*`;
- grupos de referencia `_CriaMovePastas` e `_Informatica` sao `Global / Security`.

Candidato para a proxima etapa:

- nome: `SG_CadastroColaboradores_AD_Writer`;
- categoria: `Security`;
- escopo: `Global`;
- destino: `OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br`.

Esse grupo sera exclusivo para delegacao tecnica. `_Informatica` continua apenas como autorizacao humana de uso do sistema. A gMSA sera o unico membro inicial do grupo tecnico.

Antes da criacao real, executar baseline da ACL da OU, consulta de colisao de nome e simulacao `-WhatIf`. Rollback da criacao, se necessario: remover somente o grupo criado pelo projeto.

## Grupo tecnico - baseline e simulacao validados - 23/09/2026

Na OU `OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br`:

- owner: `AUTOMIND\\Domain Admins`;
- heranca ativa (`AreAccessRulesProtected=False`);
- 40 ACEs observadas no baseline;
- nao existe grupo com o nome `SG_CadastroColaboradores_AD_Writer`.

A simulacao abaixo foi executada com `-WhatIf` e nao alterou o AD:

`New-ADGroup -Name "SG_CadastroColaboradores_AD_Writer" -SamAccountName "SG_CadastroColaboradores_AD_Writer" -GroupCategory Security -GroupScope Global -Path "OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br" -Description "Delegacao tecnica AD do Automind.CadastroColaboradores" -WhatIf`

Alvo confirmado pela simulacao:

`CN=SG_CadastroColaboradores_AD_Writer,OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br`

Nenhuma delegacao foi aplicada e nenhum membro foi incluido. A criacao real do grupo depende de autorizacao explicita e sera uma mudanca isolada. A validacao pos-criacao sera somente leitura. Rollback preparado: remover somente o grupo criado pelo projeto, desde que ainda esteja vazio e sem delegacoes adicionais.

## Grupo tecnico criado - 23/09/2026

Objeto criado e validado:

- `Name`: `SG_CadastroColaboradores_AD_Writer`
- `sAMAccountName`: `SG_CadastroColaboradores_AD_Writer`
- `GroupScope`: `Global`
- `GroupCategory`: `Security`
- `Description`: `Delegacao tecnica AD do Automind.CadastroColaboradores`
- `DistinguishedName`: `CN=SG_CadastroColaboradores_AD_Writer,OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br`
- membros: nenhum

O grupo sera usado somente para receber as delegacoes tecnicas do CadastroColaboradores. `_Informatica` continua separado e serve apenas para autorizacao humana no sistema.

Nenhuma ACL foi aplicada nesta etapa e a `gMSA_CadColab$` ainda nao foi adicionada ao grupo.

## Rollback do provisionamento

O rollback operacional do projeto esta centralizado em `Docs/17-ROLLBACK-AD.md`. Nenhuma nova escrita no AD deve ser executada sem comando de rollback e validacao documentados antes da mudanca.

## gMSA adicionada ao grupo tecnico - 23/09/2026

Associacao criada e validada:

- membro: `gMSA_CadColab$`;
- grupo: `SG_CadastroColaboradores_AD_Writer`;
- DN do grupo: `CN=SG_CadastroColaboradores_AD_Writer,OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br`;
- `Get-ADGroupMember` confirmou a gMSA como membro;
- `MemberOf` da gMSA confirmou o grupo tecnico.

Esta associacao nao concede permissao de escrita por si so. Nenhuma ACL de OU ou de grupo foi delegada nesta etapa e o IIS continua sem usar a gMSA.

Rollback desta etapa:

```powershell
$svc=Get-ADServiceAccount "gMSA_CadColab";Remove-ADGroupMember -Identity "SG_CadastroColaboradores_AD_Writer" -Members $svc -Confirm:$false
```

Validacao do rollback: `Get-ADGroupMember` deve voltar vazio e `MemberOf` da gMSA nao deve conter o grupo tecnico.

## Grupo tecnico ainda sem delegacao - 23/09/2026

Consulta somente leitura confirmou ausencia de ACE para `AUTOMIND\SG_CadastroColaboradores_AD_Writer` nos seguintes niveis:

- `DC=automind,DC=com,DC=br`;
- `OU=Automind,DC=automind,DC=com,DC=br`;
- `OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`;
- `OU=Engenharia,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`.

Conclusao: a associacao da `gMSA_CadColab$` ao grupo tecnico ainda nao concede permissao de escrita nessas estruturas.

O primeiro comando de mapeamento dos GUIDs do schema falhou com `EmptyPipeElement` por sintaxe PowerShell. Nao houve alteracao no AD. O teste deve ser repetido antes de qualquer delegacao.


## Mapeamento de schema para delegacao minima - 23/09/2026

Consulta somente leitura retornou:

- classe `user`: `bf967aba-0de6-11d0-a285-00aa003049e2`;
- `Reset Password`: `00299570-246d-11d0-a768-00aa006e0529`.

Os 14 GUIDs de atributos foram retornados na mesma ordem da lista consultada, mas o campo `lDAPDisplayName` nao apareceu na saida. Antes de qualquer ACL, o mapeamento nome -> GUID sera repetido com rotulo explicito.

Nenhuma ACL foi alterada nesta etapa.

## Mapeamento validado de atributos para delegacao minima - 23/09/2026

Consulta somente leitura confirmou os GUIDs exatos do schema usados pelo contrato de escrita:

| Atributo | GUID |
|---|---|
| `givenName` | `f0f8ff8e-1191-11d0-a060-00aa006c33ed` |
| `sn` | `bf967a41-0de6-11d0-a285-00aa003049e2` |
| `displayName` | `bf967953-0de6-11d0-a285-00aa003049e2` |
| `description` | `bf967950-0de6-11d0-a285-00aa003049e2` |
| `physicalDeliveryOfficeName` | `bf9679f7-0de6-11d0-a285-00aa003049e2` |
| `telephoneNumber` | `bf967a49-0de6-11d0-a285-00aa003049e2` |
| `mail` | `bf967961-0de6-11d0-a285-00aa003049e2` |
| `title` | `bf967a55-0de6-11d0-a285-00aa003049e2` |
| `department` | `bf96794f-0de6-11d0-a285-00aa003049e2` |
| `company` | `f0f8ff88-1191-11d0-a060-00aa006c33ed` |
| `manager` | `bf9679b5-0de6-11d0-a285-00aa003049e2` |
| `userPrincipalName` | `28630ebb-41d5-11d1-a9c1-0000f80367c1` |
| `sAMAccountName` | `3e0abfd0-126a-11d0-a060-00aa006c33ed` |
| `userAccountControl` | `bf967a68-0de6-11d0-a285-00aa003049e2` |

Tambem ja confirmados:

- classe `user`: `bf967aba-0de6-11d0-a285-00aa003049e2`;
- `Reset Password`: `00299570-246d-11d0-a768-00aa006e0529`.

Nenhuma ACL foi alterada nesta etapa. O proximo passo e montar as ACEs propostas apenas em memoria e revisar o conjunto antes de qualquer `Set-Acl`.

## OU piloto para delegacao controlada - 07.Outros

A primeira validacao de delegacao real sera feita em:

`OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

Baseline antes de qualquer escrita:

- ACEs: `40`;
- owner: `AUTOMIND\Domain Admins`;
- heranca ativa;
- ACL e SDDL salvos localmente para comparacao e recuperacao.

As simulacoes anteriores em `Engenharia` permaneceram somente em memoria. Nenhum `Set-Acl` foi executado naquela OU.

## Simulacao final da OU piloto 07.Outros - 23/09/2026

A delegacao proposta foi validada somente em memoria na OU `07.Outros`:

- ACL inicial: `40` ACEs;
- simulacao com 16 ACEs: `56`;
- rollback em memoria: `40`;
- ACL real: `40`.

Nenhum `Set-Acl` foi executado. A OU `07.Outros` permanece sem ACE do grupo `SG_CadastroColaboradores_AD_Writer` ate autorizacao explicita para a escrita piloto.

## Visibilidade da delegacao a partir do servidor da aplicacao - 23/09/2026

No servidor `10.1.2.21` / `SV052022-6121`, uma consulta somente leitura foi repetida usando explicitamente o DC como `System.String`.

Resultado:

- DC consultado: `SV062022-6158.automind.com.br`;
- ACL total em `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`: `56`;
- ACEs do principal `AUTOMIND\\SG_CadastroColaboradores_AD_Writer`: `16`;
- `gMSA_CadColab$` encontrada como membro do grupo tecnico: `True`.

A consulta confirma que a ACL piloto e a membership necessarias estao visiveis a partir do servidor da aplicacao. Nenhuma alteracao foi feita nesta etapa. A troca de identidade do App Pool continua nao autorizada e deve ser precedida por baseline e rollback especificos do IIS.

## 24/09/2026 - piloto de escrita preparado no codigo

Foi adicionada uma camada de escrita controlada, ainda inativa por `Automind:Mode=ReadOnly`.

- allowlist de escrita separada da allowlist de leitura;
- unica OU de escrita configurada: `07.Outros`;
- grupos nao sao escritos;
- `proxyAddresses` e `pwdLastSet` nao sao escritos;
- M365 permanece fora do fluxo;
- conta e criada desabilitada e so e habilitada apos releitura dos atributos;
- a identidade tecnica e o operador humano sao revalidados antes da escrita;
- auditoria local precisa estar funcional antes do primeiro CreateChild.

Consultar `Docs/19-PILOTO-ESCRITA-AD.md` para o fluxo e as travas completas.
