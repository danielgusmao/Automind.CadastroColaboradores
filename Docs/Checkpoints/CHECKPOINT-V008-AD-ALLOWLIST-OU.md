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
