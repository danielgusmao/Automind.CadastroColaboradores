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

