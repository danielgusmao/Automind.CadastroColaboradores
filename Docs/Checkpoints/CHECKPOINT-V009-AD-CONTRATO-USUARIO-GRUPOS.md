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
