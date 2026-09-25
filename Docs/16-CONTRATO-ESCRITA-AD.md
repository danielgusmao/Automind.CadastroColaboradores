# ESTADO VIGENTE - 24/09/2026

Contrato validado ponta a ponta na v0.1.10: `PilotWrite` somente em `07.Outros`; membership somente em `GroupWriteAllowedDns`; `_CriaMovePastas` e o unico grupo atualmente autorizado. `lucas.costa` foi criado pelo fluxo completo com membership, readback, enable final e auditoria. Grupo fora da allowlist bloqueia a pre-validacao.

---

## v0.1.10 - contrato de membership no fluxo normal

`GroupWritesEnabled=true` não libera grupos arbitrários. O backend exige que cada DN selecionado exista, não seja protegido e esteja na `GroupWriteAllowedDns`. A membership é escrita no atributo `member` do objeto grupo antes do enable final. A releitura é obrigatória.

---

## v0.1.9 - excecao piloto controlada para membership

Existe uma unica escrita de grupo habilitavel por configuracao para teste: `teste.cadcolab` -> `_CriaMovePastas`, ambos vinculados ao laboratorio `07.Outros`. Exige `PilotWrite`, operador autorizado e gMSA esperada. O fluxo normal de criacao continua sem `GroupDns` e com `GroupWritesEnabled=false`.

# Contrato de escrita no Active Directory

## Objetivo

Definir exatamente o que o `Automind.CadastroColaboradores` podera criar ou alterar no Active Directory. Qualquer operacao fora deste contrato deve ser bloqueada ate nova aprovacao documentada.

## 1. Identidade tecnica

Identidade criada em 23/09/2026:

- Nome: `gMSA_CadColab`
- sAMAccountName: `gMSA_CadColab$`
- DNSHostName: `gMSA_CadColab.automind.com.br`
- DN: `CN=gMSA_CadColab,CN=Managed Service Accounts,DC=automind,DC=com,DC=br`
- Status: habilitada
- Host autorizado a recuperar a senha: `SV052022-6121$`

A gMSA nao recebe privilegio administrativo amplo. As permissoes serao delegadas de forma minima e somente apos testes, baseline, rollback e aprovacao.

`_informatica` continua sendo apenas autorizacao humana para usar a funcao. O operador nao empresta suas credenciais administrativas para o backend.

## 2. Operacoes permitidas no contrato inicial

O sistema podera, quando a fase de escrita for implementada e autorizada:

1. validar o chamado TOPdesk e os dados antes da escrita;
2. validar disponibilidade de `sAMAccountName`, UPN, e-mail e CN;
3. criar um objeto `User` somente em uma OU cujo DistinguishedName esteja na allowlist;
4. preencher somente os atributos aprovados abaixo;
5. definir a senha inicial sem persisti-la em banco, log ou historico;
6. definir `manager` usando o DistinguishedName de um usuario previamente localizado no AD;
7. adicionar o usuario somente a grupos explicitamente aprovados e validados;
8. habilitar a conta somente ao final, depois das validacoes da criacao.

## 3. Atributos autorizados

| Campo funcional | Atributo AD | Regra |
|---|---|---|
| Primeiro nome | `givenName` | derivado do nome completo |
| Sobrenome | `sn` | derivado do nome completo |
| Nome exibido | `displayName` | nome completo aprovado |
| Descricao | `description` | cargo/descricao aprovada |
| Escritorio/local | `physicalDeliveryOfficeName` | Local de Trabalho |
| Telefone | `telephoneNumber` | somente se a regra de divulgacao permitir |
| E-mail | `mail` | e-mail corporativo aprovado |
| Cargo | `title` | cargo em ingles aprovado |
| Departamento | `department` | departamento aprovado |
| Empresa | `company` | `Automind` |
| Superior imediato | `manager` | DN do usuario localizado no AD |
| Login legado | `sAMAccountName` | maximo 20 caracteres e disponibilidade validada |
| Login UPN | `userPrincipalName` | `usuario@automind.com.br` |

A OU nao sera inferida livremente. O `Path` de criacao deve ser o DistinguishedName de uma OU aprovada na allowlist.

## 4. Atributos fora do contrato inicial

Nao escrever automaticamente sem nova aprovacao:

- `proxyAddresses`;
- `employeeID`;
- `employeeNumber`;
- `mobile`;
- endereco postal;
- cidade/estado fora do mapeamento aprovado;
- atributos Exchange/Entra/M365;
- qualquer atributo nao listado na secao anterior.

`proxyAddresses` permanece fora da escrita inicial do CadastroColaboradores enquanto existir processo separado responsavel por esse atributo.

## 5. Grupos

A escrita de grupos e separada da criacao do usuario.

Regras:

- nunca copiar todos os grupos de um usuario de referencia;
- nunca incluir automaticamente grupos classificados como excecao;
- grupos protegidos/administrativos ficam bloqueados;
- `_informatica`, `Domain Admins`, `Enterprise Admins` e equivalentes ficam fora do contrato;
- somente grupos presentes em uma allowlist de escrita poderao receber o novo usuario;
- a alteracao ocorre no atributo `member` do grupo;
- cada grupo selecionado deve existir e ser revalidado imediatamente antes da escrita.

A allowlist final de grupos de escrita ainda precisa ser aprovada antes da implementacao desta etapa.

## 6. Fluxo de criacao seguro

Ordem obrigatoria:

1. leitura e pre-validacao;
2. registrar operador humano e chamado TOPdesk;
3. criar o usuario inicialmente desabilitado;
4. preencher atributos aprovados;
5. definir senha inicial;
6. definir `manager`;
7. adicionar somente grupos aprovados;
8. reler o objeto e validar os valores gravados;
9. habilitar a conta como ultima operacao;
10. registrar resultado final da operacao.

Se qualquer etapa falhar, parar imediatamente. Nao continuar para as etapas seguintes.

## 7. Auditoria obrigatoria

Registrar sem senha:

- usuario humano que iniciou a operacao;
- chamado TOPdesk;
- identidade tecnica executora (`gMSA_CadColab$`);
- data/hora;
- DN do usuario criado/alterado;
- OU de destino;
- atributos alterados;
- grupos adicionados/removidos;
- resultado de cada etapa;
- erro, quando houver.

## 8. Rollback

O procedimento completo e obrigatorio esta em `Docs/17-ROLLBACK-AD.md`. Toda nova escrita deve ter rollback documentado antes da execucao.

Rollback deve remover somente o que a operacao atual adicionou ou alterou.

Para testes ficticios, a exclusao do usuario de teste exige autorizacao explicita propria.

Para usuario real, o sistema nao deve excluir automaticamente uma conta ja habilitada. Em caso de falha parcial:

- manter ou colocar a conta desabilitada;
- interromper o fluxo;
- registrar exatamente o que foi aplicado;
- executar rollback somente depois de revisao e autorizacao.

Alteracoes de grupos devem ser revertidas individualmente, removendo apenas memberships adicionadas pela operacao atual.

## 9. Bloqueios obrigatorios

O backend deve bloquear:

- escrita fora das OUs permitidas;
- grupo fora da allowlist;
- grupo protegido/administrativo;
- login duplicado ou invalido;
- UPN/e-mail/CN duplicados;
- `manager` nao localizado ou ambiguo;
- operacao sem chamado TOPdesk;
- operacao sem operador autorizado por `_informatica`;
- escrita se a identidade tecnica nao estiver validada;
- qualquer atributo nao previsto neste contrato.

## 10. Estado atual

- `gMSA_CadColab$` criada, instalada no `SV052022-6121` e validada com `Test-ADServiceAccount=True`;
- grupo tecnico `SG_CadastroColaboradores_AD_Writer` criado em `06.Grupos-Gerais`;
- `gMSA_CadColab$` adicionada e validada como membro do grupo tecnico;
- App Pool ainda nao usa a gMSA;
- delegacao piloto aplicada somente em `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- a aplicacao ainda nao possui escrita de usuario habilitada.

Este documento define o limite tecnico da futura escrita; nao autoriza novas mudancas por si so.


## 11. Delegacao minima - validacao de schema

Antes da primeira delegacao de OU, os GUIDs de classe, atributos e `Reset Password` devem ser conferidos por nome.

A delegacao inicial nao deve copiar `GenericWrite` ou outras permissoes amplas observadas em contas legadas. Deve conceder somente o necessario para o contrato deste documento.

Nenhuma ACL pode ser aplicada enquanto o mapeamento nome -> GUID nao estiver validado.

## 12. GUIDs de schema aprovados para a delegacao inicial

Mapeamento validado por consulta direta ao schema do dominio:

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

Objetos/direitos relacionados:

- classe `user`: `bf967aba-0de6-11d0-a285-00aa003049e2`;
- direito `Reset Password`: `00299570-246d-11d0-a768-00aa006e0529`.

A delegacao inicial deve ser composta somente pelas ACEs necessarias para criar `User`, escrever estes atributos e redefinir a senha. `DeleteChild`, `GenericWrite`, `GenericAll`, `WriteDacl` e `WriteOwner` ficam fora do contrato inicial.

## 13. Conjunto de ACEs proposto para a OU piloto

A simulacao em memoria da OU `Engenharia` confirmou o conjunto inicial de `16` ACEs:

- 1 `CreateChild` limitada a classe `user`;
- 14 `WriteProperty`, uma para cada atributo listado na secao 12;
- 1 `ExtendedRight` de `Reset Password` para objetos `user` descendentes.

A ACL real permaneceu com `40` ACEs durante a simulacao. Nenhum `Set-Acl` foi executado.

Nao fazem parte da delegacao inicial:

- `GenericWrite`;
- `GenericAll`;
- `DeleteChild`;
- `WriteDacl`;
- `WriteOwner`;
- `pwdLastSet`.

### Troca de senha no primeiro logon

O contrato atual define senha inicial, mas nao autoriza escrita em `pwdLastSet`. Assim, a primeira delegacao nao deve implementar `ChangePasswordAtLogon`.

Se essa regra for necessaria, deve ser aprovada e documentada separadamente antes de ampliar a ACL.

## 14. Estado da ACL piloto em 07.Outros

A primeira delegacao real foi aplicada em 23/09/2026 somente na OU:

`OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

Validacao final:

- ACL total: `56` ACEs;
- ACEs do grupo tecnico: `16`;
- owner: `AUTOMIND\Domain Admins`;
- heranca ativa;
- ACEs do projeto nao herdadas;
- `ACEs=1` para cada um dos 14 atributos aprovados;
- `CreateChild` da classe `user`: `1`;
- `Reset Password` em objetos `user`: `1`.

A aparente duplicidade de `company` observada na primeira exibicao foi descartada pela consulta individual por GUID. `givenName` e `company` possuem exatamente uma ACE cada. Nenhuma correcao e nenhum rollback foram necessarios.

A delegacao piloto esta estruturalmente validada. Isso nao autoriza delegacao em outras OUs, mudanca do App Pool ou escrita de usuario sem as etapas proprias de baseline, rollback e autorizacao.

## 15. Gate de aplicacao antes do piloto de escrita

Em 24/09/2026, a configuracao implantada permanece com `Automind:Mode=ReadOnly` e `07.Outros` nao pertence a `Automind:Ad:AllowedOuDns`.

A revisao da linha de codigo atual confirmou que existem somente componentes de leitura do AD. Nao ha servico de escrita registrado no `Program.cs` nem endpoint/action de criacao de usuario ativo.

Consequencias para o piloto:

1. nao habilitar escrita apenas por alteracao de `appsettings.json`;
2. implementar uma camada de escrita explicita e separada dos servicos de leitura;
3. limitar o primeiro piloto a `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
4. manter M365 desabilitado;
5. preservar allowlist e validacoes server-side, sem confiar apenas no valor vindo da interface;
6. manter criacao inicialmente desabilitada ate senha/atributos/manager/grupos serem validados;
7. habilitar o usuario somente no final, conforme contrato ja definido;
8. nao incluir `proxyAddresses` nem `pwdLastSet` no primeiro piloto;
9. toda mudanca de codigo/configuracao deve ter rollback e autorizacao antes de publicacao.

## 24/09/2026 - implementacao do contrato no codigo, ainda inativa

O contrato acima passou a ter uma implementacao preparada no codigo por meio de `WindowsAdProvisioningWriteService`, porem o pacote continua com `Automind:Mode=ReadOnly`.

Travas adicionais implementadas:

- `WriteAllowedOuDns` separada da allowlist de leitura;
- primeiro escopo de escrita restrito a `07.Outros`;
- identidade real do processo deve ser `AUTOMIND\gMSA_CadColab$`;
- operador humano e revalidado em `_informatica` imediatamente antes da chamada de escrita;
- auditoria local precisa iniciar antes do primeiro `CreateChild`;
- qualquer grupo selecionado bloqueia a criacao piloto;
- nenhuma membership e escrita nesta fase;
- a senha temporaria nao e registrada no modelo de auditoria;
- conta e criada desabilitada e habilitada somente depois da releitura dos atributos;
- falha apos criacao nao executa exclusao automatica.

A ativacao real depende de uma futura alteracao separada de `ReadOnly` para `PilotWrite`, ainda nao autorizada/executada.
