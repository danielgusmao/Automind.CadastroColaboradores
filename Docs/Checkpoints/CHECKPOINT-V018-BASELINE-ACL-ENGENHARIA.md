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
