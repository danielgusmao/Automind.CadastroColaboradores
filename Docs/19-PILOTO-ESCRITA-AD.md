# ESTADO VIGENTE - PILOTO AD CONCLUIDO EM 24/09/2026

O objetivo deste documento foi atingido. A v0.1.10 concluiu o teste ponta a ponta com `I2609-0305` e `lucas.costa`, incluindo membership em `_CriaMovePastas`, readback, enable e auditoria. O teste negativo de allowlist tambem passou. Nao ampliar para grupos/OUs de producao sem nova aprovacao.

---

## v0.1.10 - transição do teste isolado para o fluxo completo

O teste real da v0.1.9 foi confirmado no AD e encerrou a necessidade do painel isolado. A v0.1.10 integra membership ao provisionamento normal, ainda com allowlist de escrita restrita a `_CriaMovePastas` em `07.Outros`.

---

## v0.1.9 - membership real isolada

Apos a delegacao `WriteProperty(member)` no grupo `_CriaMovePastas`, a v0.1.9 adiciona um painel especifico para validar a escrita pela propria gMSA do App Pool. Usuario e grupo sao fixados em configuracao e a operacao exige confirmacao explicita.

# Piloto de Escrita no Active Directory

## 1. Objetivo

Preparar a aplicacao `Automind.CadastroColaboradores` para um primeiro provisionamento controlado no Active Directory, sem liberar escrita no ambiente apenas pela publicacao do codigo.

O codigo desta etapa foi preparado para permitir criacao somente quando **todas** as travas abaixo estiverem satisfeitas:

- `Automind:Mode=PilotWrite`;
- processo executando como `AUTOMIND\gMSA_CadColab$`;
- operador humano ainda pertencendo ao grupo autorizado `_informatica` no momento da escrita;
- OU de destino presente na allowlist de leitura e na allowlist separada de escrita;
- no primeiro piloto, a unica OU de escrita e `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- pre-validacao completa imediatamente antes da escrita;
- nenhum grupo selecionado;
- auditoria local gravavel antes da primeira alteracao no AD.

A configuracao entregue na `v0.1.3` passa para `Automind:Mode=PilotWrite` por autorizacao explicita do responsavel. Portanto, **a publicacao desta versao habilita a criacao real somente dentro do escopo piloto e sob todas as travas descritas neste documento**.

## 2. Estado de infraestrutura ja validado

- App Pool `CadastroColaboradores` executa como `AUTOMIND\gMSA_CadColab$`;
- `Test-ADServiceAccount gMSA_CadColab=True` no servidor `10.1.2.21`;
- HTTP `200` via binding correto e FQDN;
- worker IIS validado sob a gMSA;
- `gMSA_CadColab$` e membro de `SG_CadastroColaboradores_AD_Writer`;
- a OU `07.Outros` possui exatamente 16 ACEs do grupo tecnico:
  - 1 `CreateChild` para classe `user`;
  - 14 `WriteProperty` para os atributos aprovados;
  - 1 `Reset Password`;
- nenhuma delegacao de escrita de membership de grupos foi criada.

## 3. Componentes implementados

### 3.1 `IAdProvisioningWriteService` / `WindowsAdProvisioningWriteService`

Responsavel pelo fluxo de escrita. O servico:

1. bloqueia se `Mode` nao for `PilotWrite`;
2. valida o comando recebido;
3. valida a OU contra `WriteAllowedOuDns`;
4. bloqueia qualquer tentativa de gravacao de grupos;
5. confirma que a identidade real do processo e `AUTOMIND\gMSA_CadColab$`;
6. grava auditoria de inicio **antes** da primeira escrita no AD;
7. gera senha temporaria aleatoria de pelo menos 14 caracteres;
8. cria o usuario inicialmente desabilitado;
9. grava somente os atributos aprovados;
10. define a senha inicial;
11. define `manager` usando DistinguishedName ja validado;
12. registra que memberships foram ignoradas no piloto;
13. rele o objeto e compara os atributos antes de habilitar;
14. habilita a conta somente no final;
15. rele novamente e confirma o estado habilitado;
16. devolve a senha temporaria somente na resposta de sucesso.

Se ocorrer falha depois da criacao do objeto, o servico tenta manter/confirmar a conta desabilitada e **nao exclui automaticamente o usuario**.

### 3.2 Auditoria

Foi criado `FileProvisioningAuditService`.

Caminho configurado:

`C:\Automind.CadastroColaboradores\Logs\ProvisioningAudit.jsonl`

A auditoria registra, entre outros:

- data/hora UTC;
- identificador da operacao;
- chamado TOPdesk;
- operador humano;
- identidade tecnica;
- acao/status;
- DN do usuario quando existente;
- OU;
- nomes dos atributos tratados;
- tipo/codigo tecnico da falha quando aplicavel.

**A senha nao existe no modelo de auditoria e nao e gravada no arquivo.**

Se a aplicacao nao conseguir iniciar a auditoria local, a primeira escrita no AD nao deve ocorrer.

### 3.3 Endpoint de criacao

Foi preparado `POST /Colaboradores/CriarUsuarioPiloto` com antiforgery.

Antes de chamar o servico de escrita, o endpoint:

- exige confirmacao explicita do usuario na interface;
- verifica `PilotWrite`;
- identifica o operador autenticado;
- consulta novamente o AD para confirmar que ele ainda pertence ao grupo autorizado;
- repete a pre-validacao completa;
- confirma a OU na allowlist de escrita;
- permite que os grupos comuns permaneçam marcados apenas como sugestao visual quando `GroupWritesEnabled=false`;
- envia `GroupDns=[]` ao servico de escrita, portanto nenhuma membership e alterada no piloto.

As respostas do endpoint usam `Cache-Control: no-store` e `Pragma: no-cache` porque uma resposta de sucesso pode conter a senha temporaria.

## 4. Atributos do piloto

O fluxo prepara/grava somente:

- `givenName`;
- `sn`;
- `displayName`;
- `description`;
- `physicalDeliveryOfficeName`;
- `telephoneNumber`, somente quando a opcao de divulgacao estiver ativa;
- `mail`;
- `title`;
- `department`;
- `company=Automind`;
- `manager`;
- `userPrincipalName`;
- `sAMAccountName`;
- `userAccountControl`.

`description` usa o cargo em ingles no piloto.

Continuam fora do fluxo:

- `proxyAddresses`;
- `pwdLastSet`;
- memberships de grupos;
- Microsoft 365;
- Teams;
- escrita no TOPdesk.

## 5. Configuracao ativa para o piloto controlado

O `appsettings.json` deste pacote contem:

- `Automind:Mode=PilotWrite`;
- `07.Outros` adicionada a `AllowedOuDns` para permitir leitura/pre-validacao do piloto;
- `Automind:Provisioning:TechnicalIdentity=AUTOMIND\gMSA_CadColab$`;
- `WriteAllowedOuDns` contendo **somente** `07.Outros`;
- `GroupWritesEnabled=false`;
- com `GroupWritesEnabled=false`, os grupos comuns ao cargo podem permanecer marcados na interface como sugestao; excecoes ficam desmarcadas e protegidos continuam bloqueados; essas marcacoes **nao** sao enviadas para escrita: o comando de criacao usa `GroupDns=[]`;
- senha inicial com comprimento 14;
- caminho do arquivo de auditoria;
- `Microsoft365.Enabled=false`.

A allowlist de escrita e independente da allowlist de leitura. Adicionar uma OU a `AllowedOuDns` nao autoriza escrita nela. A pre-validacao continua mostrando apenas `OU valida`; o backend de criacao revalida `WriteAllowedOuDns` e bloqueia qualquer OU fora de `07.Outros`, sem necessidade de um indicador temporario na tela.

## 6. Gates cumpridos antes da ativacao `PilotWrite`

Antes da `v0.1.3`, foram confirmados:

1. build local em .NET 10 concluido com 0 erros;
2. publicacao e testes em `ReadOnly` realizados no servidor;
3. HTTP 200 e worker IIS executando como `AUTOMIND\gMSA_CadColab$`;
4. `Test-ADServiceAccount gMSA_CadColab=True`;
5. `07.Outros` existe e e a unica OU em `WriteAllowedOuDns`;
6. `gMSA_CadColab$` continua no grupo tecnico;
7. `GroupWritesEnabled=false`;
8. `Microsoft365.Enabled=false`;
9. limite de 20 caracteres do `sAMAccountName` validado na interface e no backend;
10. autorizacao explicita recebida para gerar a `v0.1.3` em `PilotWrite`.

Observacao: o diretorio/arquivo de auditoria ainda pode nao existir antes da primeira criacao. O servico tenta cria-lo **antes da primeira escrita no AD**; se a auditoria nao puder ser iniciada, o fluxo falha antes de criar o usuario.

## 7. Rollback de codigo/configuracao

### Com `PilotWrite` ativo

O primeiro bloqueio de emergencia da aplicacao e restaurar `Automind:Mode=ReadOnly` e publicar essa configuracao. Isso bloqueia novas criacoes sem remover gMSA, ACL piloto ou grupo tecnico.

Se a `v0.1.3` apresentar problema funcional antes de qualquer usuario ser criado, restaurar a publicacao anterior `v0.1.2` pelo fluxo Git/Azure aprovado.

Se um usuario piloto ja tiver sido criado, voltar para `ReadOnly` primeiro e tratar o usuario criado separadamente conforme o rollback abaixo.

A restauracao integral do backup IIS `CadColab-Pre-gMSA-20260924-082229` nao e o rollback primario da aplicacao e nao deve ser executada automaticamente.

### Usuario ficticio criado

Se o primeiro usuario ficticio for criado:

1. em qualquer anomalia, manter/desabilitar a conta;
2. registrar DN, estado e etapas concluidas;
3. nao remover memberships genericas porque o piloto nao as grava;
4. exclusao do usuario ficticio somente com autorizacao explicita separada;
5. para usuario real, nunca usar exclusao automatica como rollback.

## 8. Validacao deste pacote no ambiente de geracao

Foi possivel validar:

- JSON do `appsettings.json`;
- sintaxe JavaScript por `node --check`;
- consistencia estrutural dos arquivos e configuracoes.

**Nao foi possivel executar `dotnet build` no ambiente de geracao porque o .NET SDK nao esta instalado.** Portanto, o build em maquina com .NET 10 SDK e obrigatorio antes de qualquer publicacao.

## Build local aprovado em 24/09/2026

O pacote preparado foi compilado com sucesso em `net10.0`, sem erros e com 15 avisos `CA1416` relacionados a APIs Windows-only de `System.DirectoryServices`.

Apos o build foi confirmado:

- `Automind:Mode=PilotWrite`;
- uma unica OU na allowlist de escrita (`07.Outros`);
- escrita de grupos desabilitada;
- Microsoft 365 desabilitado;
- servicos de escrita AD e auditoria presentes.

Esses gates foram concluídos. A `v0.1.3` e a primeira versao preparada para publicacao com `Automind:Mode=PilotWrite`, mantendo o escopo de escrita limitado a `07.Outros`.
