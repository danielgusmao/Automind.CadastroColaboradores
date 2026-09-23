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
