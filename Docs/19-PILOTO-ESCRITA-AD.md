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

A configuracao entregue neste pacote continua com `Automind:Mode=ReadOnly`. Portanto, **o pacote nao ativa escrita apenas por ser compilado ou publicado**.

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
- bloqueia se existir qualquer grupo selecionado.

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

## 5. Configuracao preparada, mas inativa

O `appsettings.json` deste pacote contem:

- `Automind:Mode=ReadOnly`;
- `07.Outros` adicionada a `AllowedOuDns` para permitir leitura/pre-validacao do piloto;
- `Automind:Provisioning:TechnicalIdentity=AUTOMIND\gMSA_CadColab$`;
- `WriteAllowedOuDns` contendo **somente** `07.Outros`;
- `GroupWritesEnabled=false`;
- com `GroupWritesEnabled=false`, os grupos comuns ao cargo podem permanecer marcados na interface como sugestao; excecoes ficam desmarcadas e protegidos continuam bloqueados; essas marcacoes **nao** sao enviadas para escrita: o comando de criacao usa `GroupDns=[]`;
- senha inicial com comprimento 14;
- caminho do arquivo de auditoria;
- `Microsoft365.Enabled=false`.

A allowlist de escrita e independente da allowlist de leitura. Adicionar uma OU a `AllowedOuDns` nao autoriza escrita nela. A pre-validacao continua mostrando apenas `OU valida`; o backend de criacao revalida `WriteAllowedOuDns` e bloqueia qualquer OU fora de `07.Outros`, sem necessidade de um indicador temporario na tela.

## 6. Travas que permanecem antes do primeiro usuario

Nao ativar `PilotWrite` ainda. Antes disso:

1. compilar o pacote em maquina com .NET 10 SDK;
2. revisar o resultado do build;
3. publicar mantendo `Mode=ReadOnly`;
4. validar HTTP e leituras AD apos a publicacao;
5. validar que a gMSA consegue criar/anexar o arquivo de auditoria local, sem escrever no AD;
6. confirmar que `07.Outros` aparece na tela e continua sendo a unica OU em `WriteAllowedOuDns`;
7. preparar backup/hash do `appsettings.json` implantado;
8. preparar alteracao real de `ReadOnly` para `PilotWrite`, efeito esperado, rollback e validacao;
9. obter autorizacao explicita;
10. somente depois executar um usuario ficticio em `07.Outros`.

## 7. Rollback de codigo/configuracao

### Antes de ativar `PilotWrite`

O estado seguro e `Mode=ReadOnly`. Se o pacote novo apresentar problema funcional, restaurar a publicacao anterior pelo processo de deploy/Git aprovado, sem alterar gMSA, ACL piloto ou grupo tecnico automaticamente.

### Depois de uma futura ativacao de `PilotWrite`

O primeiro bloqueio de emergencia da aplicacao sera restaurar `Automind:Mode=ReadOnly`. Esse rollback de configuracao deve ser tratado como uma alteracao real separada e validada.

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

- `Automind:Mode=ReadOnly`;
- uma unica OU na allowlist de escrita (`07.Outros`);
- escrita de grupos desabilitada;
- Microsoft 365 desabilitado;
- servicos de escrita AD e auditoria presentes.

Portanto, a proxima etapa permitida e publicar a nova versao **ainda em ReadOnly** e validar o comportamento no servidor antes de qualquer ativacao `PilotWrite`.
