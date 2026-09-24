# Registro de interacoes e decisoes - 24/09/2026

Este documento registra as interacoes recentes relevantes para que o contexto nao dependa do chat.

## Primeiro PilotWrite real

O usuario ficticio `teste.cadcolab` foi criado em:

`CN=Teste Provisionamento Automind,OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

Leitura posterior confirmou:

- `Enabled=True`;
- `PasswordLastSet=24/09/2026 11:41:19`;
- UPN/mail `teste.cadcolab@automind.com.br`;
- `title=Automation Systems Analyst`;
- `department=ENGENHARIA`;
- `company=Automind`;
- manager `Edson Neto` pelo DN esperado;
- office `Salvador`;
- `memberOf` direto vazio, portanto nenhum grupo de negocio escrito;
- auditoria presente com `provisioning-start`, `create-disabled`, `attributes`, `password`, `manager`, `groups=skipped`, `readback-disabled`, `enable` e `provisioning-complete`;
- identidade tecnica registrada: `AUTOMIND\gMSA_CadColab$`.

## Levantamento para grupos

Foi obtido o GUID do atributo `member`:

`bf9679c0-0de6-11d0-a285-00aa003049e2`

Os 8 grupos comuns ao perfil foram localizados e o grupo tecnico nao tinha ACE neles no momento da leitura.

A ACE `WriteProperty` para `member` foi testada somente em memoria em 8 grupos: em todos os casos o ciclo foi `0 -> 1 -> 0` usando `AddAccessRule` e `RemoveAccessRuleSpecific`, sem `Set-Acl`.

Nenhuma delegacao real de `member` foi aplicada nessa rodada.

## Correcao de escopo: 07.Outros x pesquisa de referencia

O responsavel corrigiu a interpretacao do escopo:

- alteracoes reais de teste permanecem restritas a `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- grupos/usuarios de outras OUs podem e devem ser consultados em modo somente leitura para inferir o perfil de acesso pelo cargo;
- nao usar `_Engenharia` ou outro grupo de producao como laboratorio de escrita durante esta fase;
- nao criar grupo ficticio apenas para testar a logica de sugestao.

A leitura em `07.Outros` encontrou um unico grupo: `_CriaMovePastas`. Esse objeto pertence ao sistema legado e nao deve ser reutilizado pelo CadColab.

## Coorte real do cargo

Pesquisa global para `Automation Systems Analyst + ENGENHARIA`, excluindo o proprio `teste.cadcolab`, encontrou 5 usuarios ativos de referencia.

Grupos 5/5, comuns ao cargo:

- `Dist_Engenharia`;
- `Dist_Todos`;
- `Dist_Todos_SSA`;
- `_Engenharia`;
- `_GA_E-CLIC`;
- `_Todos`;
- `_Todos SSA`;
- `_Técnica SSA`.

Excecoes observadas:

- `_GAP7` = 4/5;
- `Dist_sustentacao_engenharia` = 1/5;
- `_GAP2` = 1/5;
- `_Internet` = 1/5.

Efeitos indiretos conhecidos:

- `_Engenharia` -> `_JumpServer`;
- `_Técnica SSA` -> `_GAP2`, `_Tecnica`.

## Erro de autoinclusao da coorte

Depois que `teste.cadcolab` foi criado ativo com o mesmo cargo/departamento e ainda sem grupos, uma consulta que nao o excluia passou a mostrar `5/6`, classificando incorretamente os 8 grupos comuns como excecoes.

Decisao: o colaborador em cadastro deve ser excluido da propria coorte por `sAMAccountName`.

## Modelo de selecao de grupos aprovado

A tela deve trabalhar com tres origens:

1. **Comuns ao cargo** - presentes em 100% da coorte, marcados automaticamente.
2. **Excecoes encontradas** - presentes somente em parte da coorte, exibidas desmarcadas para escolha do operador.
3. **Outros grupos** - busca manual/autocomplete no AD; o operador digita o nome, escolhe um grupo real e ele entra na selecao.

Todos os grupos selecionados manualmente devem ser revalidados pelo backend e grupos protegidos devem permanecer bloqueados.

Enquanto `GroupWritesEnabled=false`, toda selecao e somente informativa/preparatoria. O writer continua recebendo `GroupDns=[]` e nao grava associacoes de grupo.

## Terminologia acordada

Neste projeto, **membership** significa a associacao efetiva do usuario a um grupo do Active Directory.

- sugestao de grupo: somente recomendacao na tela;
- grupo selecionado: escolha do operador ainda sem escrita;
- membership gravada: usuario efetivamente adicionado ao grupo no AD;
- membership direta: o usuario esta diretamente no grupo;
- acesso indireto/transitivo: decorre de grupos aninhados, sem membership direta naquele grupo ancestral.

## Politica primordial de documentacao

O responsavel determinou que documentacao e checkpoint nunca mais sejam reduzidos ou removidos entre versoes. Toda nova versao deve apenas acrescentar historico e preservar os documentos anteriores, permitindo continuidade integral por humanos ou LLMs.
