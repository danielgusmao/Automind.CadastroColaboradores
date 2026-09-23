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
