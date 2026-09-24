# CadColab - Contexto de continuidade para novo chat

**Versao do contexto:** 0.1.3-continuity  
**Data:** 24/09/2026  
**Projeto:** `Automind.CadastroColaboradores` / pacote curto `CadColab`  
**Objetivo deste arquivo:** permitir abrir um novo chat e continuar exatamente do ponto atual, sem repetir investigacoes ja encerradas e sem perder as regras de seguranca.

> Ao iniciar um novo chat, anexe este arquivo e diga: **"Continue o projeto CadColab a partir deste contexto. Nao repita etapas ja validadas. Siga as regras operacionais deste documento."**

---

## 1. Regra de precedencia deste documento

Este arquivo descreve o **estado real mais recente** do projeto, depois do primeiro teste `PilotWrite` com criacao real de usuario no AD.

Alguns documentos antigos dentro do pacote `CadColab-v0.1.3.zip` ainda descrevem estados anteriores, por exemplo App Pool em `ApplicationPoolIdentity`, escrita ausente ou usuario piloto ainda nao criado. **Quando houver conflito, este arquivo deve prevalecer ate o checkpoint interno do projeto ser atualizado.**

---

## 2. Regras operacionais obrigatorias

1. Nao alterar codigo, AD, IIS, servidor, TOPdesk ou configuracoes sem autorizacao explicita do usuario.
2. Testes somente leitura/simulacao executados no mesmo local devem ser **concatenados em uma unica linha PowerShell**, podendo conter mais de 3 validacoes para economizar tempo.
3. Para **ALTERACAO REAL**, manter **uma mudanca por vez**.
4. Antes de cada alteracao real precisam existir:
   - comando da mudanca;
   - efeito esperado;
   - rollback;
   - validacao do rollback.
5. Todo teste deve dizer claramente onde executar:
   - **AD**;
   - **servidor 10.1.2.21**;
   - **maquina do usuario / Visual Studio**.
6. Comandos PowerShell enviados ao usuario devem ficar em **uma unica linha**.
7. Nao usar `iisreset`.
8. Nao reiniciar DC/AD, KDC, Netlogon ou servidor por causa deste projeto.
9. Nao restaurar ACL inteira cegamente; rollback de ACL deve remover somente ACE criada pelo projeto.
10. Parar no primeiro resultado inesperado antes de qualquer nova escrita.
11. Nao apagar usuario de teste automaticamente. Primeiro conter/desabilitar. Exclusao exige autorizacao separada.
12. `Microsoft 365` continua pausado.
13. `proxyAddresses` continua fora da escrita do aplicativo; existe automacao separada para SMTP.
14. `pwdLastSet` continua fora do contrato atual.
15. `_informatica` serve apenas para autorizacao humana da aplicacao; nao usar como principal tecnico de escrita no AD.

---

## 3. Regras de empacotamento, Git e release

### Pacotes

- Usar nomes curtos para evitar erro de caminho longo no Windows.
- Convencao atual:
  - `CadColab-v0.1.0.zip`
  - `CadColab-v0.1.1.zip`
  - `CadColab-v0.1.2.zip`
  - `CadColab-v0.1.3.zip`
- Proxima atualizacao prevista: **`CadColab-v0.1.4.zip`**.
- Durante a preparacao de uma release/teste, **nao gerar ZIP de checkpoint separado a cada etapa**. O checkpoint deve ficar dentro do pacote completo.
- Checkpoint separado volta a ser considerado apenas numa nova fase de testes ou se o usuario pedir.

### Git / Visual Studio

O usuario trabalha no Visual Studio em:

`C:\Users\daniel.gusmao\source\repos\Automind.CadastroColaboradores`

Branch usada: `release`.

Remotes:

- `origin` = GitHub / backup-espelho;
- `azure` = Azure DevOps / fluxo de build-release.

Fluxo normal durante ajustes iniciais:

```powershell
git branch --show-current;dotnet build -c Release;git status --short
```

Depois, se tudo estiver correto:

```powershell
git add .;git commit -m "<descricao da alteracao>";git push origin release;git push azure release
```

- Nao criar tag Git automaticamente.
- Nao fazer bump/tag adicional fora da versao curta do pacote, salvo pedido explicito.
- O Azure publica no servidor e o usuario valida visual/funcionalmente.
- Build atual gera cerca de **15 warnings CA1416** por `System.DirectoryServices` Windows-only; isso e esperado no destino Windows/IIS e nao representa erro de build.
- Evitar commitar `artifacts/` de publish local por engano.

---

## 4. Versao atual e pacote

Ultimo pacote entregue e publicado:

`CadColab-v0.1.3.zip`

SHA-256 do pacote entregue:

`163F6C6FEED556F611AC21C1CD830F247890C4CCFD740E3CE3E852DDB4A25791`

No servidor apos deploy:

- `VERSION: CadColab 0.1.3`
- `Automind:Mode = PilotWrite`
- `GroupWritesEnabled = False`
- `Microsoft365.Enabled = False`
- somente uma OU permitida para escrita:
  `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`
- SHA-256 do `appsettings.json` implantado:
  `CB7AED132E617DB6B170673CEC13A40C0F45F342D102AC4948AE1CB05251607A`

O `VERSION.txt` passou a ser copiado para o publish na v0.1.3.

---

## 5. Infraestrutura AD/IIS validada

### Dominio

- Dominio: `automind.com.br`
- DC principal usado pelo aplicativo: `10.1.2.1`
- DC identificado: `SV062022-6158.automind.com.br`
- Segundo DC conhecido: `SV062022-9947`
- replicacao ja foi validada sem falhas no levantamento anterior.

### Servidor da aplicacao

- IP: `10.1.2.21`
- hostname: `SV052022-6121`
- caminho da aplicacao:
  `C:\Automind.CadastroColaboradores`
- site:
  `http://cadastro.automind.com.br/`
- binding:
  `10.1.2.21:80:cadastro.automind.com.br`
- HTTP validado com `200`.

### gMSA

Conta tecnica criada para o projeto:

- nome: `gMSA_CadColab`
- sAMAccountName: `gMSA_CadColab$`
- DN:
  `CN=gMSA_CadColab,CN=Managed Service Accounts,DC=automind,DC=com,DC=br`
- host autorizado a recuperar senha gerenciada:
  `SV052022-6121$`
- `Test-ADServiceAccount gMSA_CadColab = True`
- instalada localmente no servidor `10.1.2.21`.

KDS Root Key ja existe e foi validada. **Nao criar outra KDS Root Key.**

### Grupo tecnico de delegacao

- nome: `SG_CadastroColaboradores_AD_Writer`
- DN:
  `CN=SG_CadastroColaboradores_AD_Writer,OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br`
- tipo: `Global / Security`
- membro atual: somente `gMSA_CadColab$` para este projeto.

### App Pool

App Pool:

`CadastroColaboradores`

Estado atual validado:

- `Started`
- `IdentityType = SpecificUser`
- `UserName = AUTOMIND\gMSA_CadColab$`
- `LogonType = LogonBatch`
- `LoadUserProfile = True`
- `manualGroupMembership = False`
- worker real ja foi validado com owner `AUTOMIND\gMSA_CadColab$`
- ASP.NET Core Module registrou inicializacao com sucesso.

Backup IIS antes da troca para gMSA:

`CadColab-Pre-gMSA-20260924-082229`

Hashes registrados:

- `applicationHost.config`: `B46EE9FEA44440598CB5B3014A631E3FD89E78D4358FCF22DA90385853511840`
- baseline textual do pool: `CD51C31F6CE9B87B1E09B00C5B5D48BAE2756CFB07946777FC3DE0755F8EE45B`

Nao usar `CriaMovePastas` nem contas pessoais como identidade tecnica do CadastroColaboradores.

---

## 6. Delegacao atual na OU piloto 07.Outros

OU piloto:

`OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

Antes da delegacao:

- 40 ACEs;
- owner `AUTOMIND\Domain Admins`;
- heranca ativa.

Baseline local salvo no DC:

- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-ACL-before.xml`
- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-SDDL-before.txt`

Hashes baseline:

- ACL XML: `2F8C1F0AB538CAEFAD17F3CC59631D87F741860270DD1F11B113AD66EB25106D`
- SDDL: `53FB30862FDB19A069F25BC7C182949315AA00D85CAD229D0EEC61931CFC64B3`

Delegacao aplicada e validada:

- total atual: 56 ACEs;
- 16 ACEs explicitas do grupo tecnico;
- owner preservado;
- heranca continua ativa.

As 16 ACEs sao:

- 1 `CreateChild` para classe `user`;
- 14 `WriteProperty` nos atributos:
  - `givenName`
  - `sn`
  - `displayName`
  - `description`
  - `physicalDeliveryOfficeName`
  - `telephoneNumber`
  - `mail`
  - `title`
  - `department`
  - `company`
  - `manager`
  - `userPrincipalName`
  - `sAMAccountName`
  - `userAccountControl`
- 1 `ExtendedRight` de `Reset Password`.

Hashes depois da ACL piloto:

- ACL XML: `5B2832851DA742D9AA4B3B768214282F694B41626BFC965ABE8F4B16E02C4584`
- SDDL: `31E8B76A144EFCB7CA83C8897651FB55FC3B1FDF362CCAA09CC3CC900BF95AC3`

Nenhuma outra OU recebeu delegacao de escrita deste projeto.

---

## 7. Contrato de escrita atual da v0.1.3

Modo atual:

`PilotWrite`

Allowlist de escrita:

`OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

Fluxo implementado no backend:

1. exige confirmacao explicita do operador;
2. confirma operador humano autorizado pelo grupo `_informatica`;
3. repete toda pre-validacao no backend;
4. confirma que a OU esta na `WriteAllowedOuDns`;
5. confirma que o processo esta executando como `AUTOMIND\gMSA_CadColab$`;
6. grava auditoria antes da primeira escrita no AD;
7. gera senha temporaria;
8. cria o usuario **desabilitado**;
9. grava atributos aprovados;
10. define senha;
11. grava `manager` pelo DN validado;
12. grupos sao atualmente **skipped**;
13. releitura confirma usuario desabilitado e atributos;
14. habilita a conta por ultimo;
15. faz releitura final e auditoria de conclusao.

Se ocorrer erro depois da criacao, o fluxo tenta manter/desabilitar a conta para revisao manual. Nao existe exclusao automatica.

### Atributos permitidos hoje

- `givenName`
- `sn`
- `displayName`
- `description`
- `physicalDeliveryOfficeName`
- `telephoneNumber` somente quando divulgacao estiver habilitada
- `mail`
- `title`
- `department`
- `company`
- `manager`
- `userPrincipalName`
- `sAMAccountName`
- `userAccountControl`
- senha via `SetPassword`

### Fora do contrato atual

- `proxyAddresses`
- `pwdLastSet`
- memberships de grupos
- Microsoft 365
- Teams
- escrita de volta no TOPdesk

---

## 8. TOPdesk / formulario de teste

Chamado ficticio utilizado:

`I2609-0295`

Dados principais do piloto:

- nome: `Teste Provisionamento Automind`
- nome de guerra: `Teste Provisionamento`
- login final usado: `teste.cadcolab`
- email: `teste.cadcolab@automind.com.br`
- local: `Salvador`
- cargo PT: `Analista de Sistemas de Automacao`
- cargo EN: `Automation Systems Analyst`
- departamento: `ENGENHARIA`
- empresa: `Automind`
- superior: `Edson Neto`
- OU piloto: `07.Outros`

A interface passou a exibir limite de `sAMAccountName` de 20 caracteres.

Exemplo que falhou corretamente:

- `teste.provisionamento` = 21 caracteres;
- tela mostrou `21 / 20` e pediu reducao.

Nao truncar login automaticamente; o operador deve escolher conscientemente o login final.

---

## 9. Primeiro PilotWrite REAL ja executado

**IMPORTANTE: o usuario ficticio ja foi criado no AD. Nao tentar criar outro objeto com o mesmo CN/login.**

Objeto criado:

`CN=Teste Provisionamento Automind,OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

Identidade:

- `sAMAccountName = teste.cadcolab`
- `UPN = teste.cadcolab@automind.com.br`
- `mail = teste.cadcolab@automind.com.br`

Pelas telas do ADUC, foram confirmados visualmente:

- objeto criado em `07.Outros`;
- `First name = Teste`;
- `Last name = Provisionamento Automind`;
- `Display name = Teste Provisionamento Automind`;
- `Description = Automation Systems Analyst`;
- `Office = Salvador`;
- `E-mail = teste.cadcolab@automind.com.br`;
- logon UPN e pre-Windows 2000 = `teste.cadcolab`;
- `Job Title = Automation Systems Analyst`;
- `Department = ENGENHARIA`;
- `Company = Automind`;
- `Manager = Edson Neto`.

### Grupos do usuario apos a criacao

Na aba **Member Of** aparece somente:

`Domain Users`

Isso e **esperado na v0.1.3**, porque:

- `GroupWritesEnabled = false`;
- o controller envia `GroupDns = []` para o servico de escrita;
- o servico rejeita `GroupDns.Count > 0` enquanto a escrita de grupos estiver desabilitada;
- portanto a criacao do usuario funcionou, mas memberships nao foram gravadas.

`Domain Users` e grupo primario; nao tratar sua aparicao como validacao dos 8 grupos sugeridos.

### O que ainda precisa ser confirmado por leitura depois da criacao

O comando consolidado que deveria verificar usuario, auditoria e ACL de grupos falhou por **erro de parser `EmptyPipeElement` antes de executar**. Portanto:

- nao houve alteracao causada por esse comando;
- o estado `Enabled` do usuario deve ser confirmado via PowerShell;
- `PasswordLastSet` deve ser confirmado via leitura;
- o arquivo de auditoria pos-criacao deve ser confirmado;
- a lista direta `memberOf` deve ser confirmada;
- nao assumir que a auditoria existe ate ler o arquivo.

---

## 10. Auditoria

Caminho configurado:

`C:\Automind.CadastroColaboradores\Logs\ProvisioningAudit.jsonl`

Antes do primeiro PilotWrite real:

- pasta `Logs` nao existia;
- arquivo nao existia.

Isso nao era bloqueio por design, porque `FileProvisioningAuditService` executa:

`Directory.CreateDirectory(folder)`

antes de `File.AppendAllTextAsync`.

O write service tenta gravar `provisioning-start` **antes da primeira escrita no AD**. Se essa gravacao falhar, a criacao deve falhar antes de `Children.Add(...)`.

**Estado pos-criacao ainda precisa ser confirmado por leitura.**

---

## 11. Sugestao de grupos do perfil do teste

Perfil usado:

`Title = Automation Systems Analyst` + `Department = ENGENHARIA`

Coorte real encontrada: 5 usuarios.

### 8 grupos comuns (5/5)

1. `Dist_Engenharia` - Distribution / Global
2. `Dist_Todos` - Distribution / Global
3. `Dist_Todos_SSA` - Distribution / Global
4. `_Engenharia` - Security / Global
5. `_GA_E-CLIC` - Security / Global
6. `_Todos` - Security / Global
7. `_Todos SSA` - Security / Global
8. `_Técnica SSA` - Security / Global

A interface deve continuar marcando automaticamente os grupos **comuns ao cargo** e deixando excecoes desmarcadas.

### Excecoes observadas

- `_GAP7` - 4/5
- `Dist_sustentacao_engenharia` - 1/5
- `_GAP2` - 1/5
- `_Internet` - 1/5

### Efeitos indiretos conhecidos

- `_Engenharia` -> `_JumpServer`
- `_Técnica SSA` -> `_GAP2`, `_Tecnica`

Esses efeitos transitivos devem continuar visiveis na UI/auditoria.

### Estado de escrita de grupos

Ainda **nao existe delegacao `Write member`** para o grupo tecnico nesses 8 grupos.

Ainda **nao habilitar `GroupWritesEnabled=true`**.

---

## 12. Proxima fase exata: memberships de grupos

Objetivo da proxima versao prevista:

`CadColab-v0.1.4.zip`

Mas **nao gerar v0.1.4 antes de concluir o levantamento e a delegacao controlada**.

Ordem obrigatoria:

1. confirmar por leitura o usuario criado, estado Enabled, PasswordLastSet, memberOf e auditoria;
2. localizar os 8 grupos e seus DNs exatos;
3. obter o GUID do atributo `member` no schema;
4. verificar se o grupo tecnico ja possui alguma ACE nos 8 grupos - esperado atualmente: zero;
5. montar as ACEs `WriteProperty` somente para atributo `member`, uma por grupo, **somente em memoria**;
6. preparar rollback exato com `RemoveAccessRuleSpecific` para cada ACE;
7. obter autorizacao explicita antes de qualquer `Set-Acl` real;
8. aplicar delegacao real somente nos 8 grupos aprovados;
9. validar ACL depois da escrita;
10. somente depois gerar codigo v0.1.4 com:
    - `GroupWritesEnabled=true`;
    - allowlist explicita dos 8 DNs;
    - nunca aceitar grupo arbitrario vindo da tela;
    - bloquear grupo protegido;
    - adicionar memberships depois dos atributos/senha/manager e antes do enable final;
    - releitura confirmar memberships;
    - auditoria registrar grupos adicionados;
    - rollback remover apenas memberships adicionadas pelo projeto.

Decisao ainda pendente para o teste de v0.1.4:

- adicionar grupos ao usuario ficticio ja existente por um fluxo especifico; **ou**
- excluir/recriar o usuario ficticio para testar o provisionamento completo.

**Nao excluir o usuario atual sem autorizacao explicita.**

---

## 13. Proximo comando correto - SOMENTE LEITURA

O comando anterior falhou com:

`An empty pipe element is not allowed.`

Causa: tentativa de colocar `| Format-Table` diretamente depois de um `foreach` sem armazenar o resultado.

Nenhuma alteracao ocorreu.

### Executar no servidor `10.1.2.21` - SOMENTE LEITURA

O proximo chat deve enviar este comando corrigido em uma unica linha:

```powershell
Import-Module ActiveDirectory -ErrorAction Stop;$userDn="CN=Teste Provisionamento Automind,OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br";$tech="SG_CadastroColaboradores_AD_Writer";$names=@("Dist_Engenharia","Dist_Todos","Dist_Todos_SSA","_Engenharia","_GA_E-CLIC","_Todos","_Todos SSA","_Técnica SSA");Write-Host "=== USUARIO CRIADO ===";Get-ADUser -Identity $userDn -Properties Enabled,userPrincipalName,mail,title,department,company,manager,physicalDeliveryOfficeName,telephoneNumber,memberOf,PasswordLastSet,whenCreated|Select-Object Name,SamAccountName,Enabled,userPrincipalName,mail,title,department,company,manager,physicalDeliveryOfficeName,telephoneNumber,PasswordLastSet,whenCreated,DistinguishedName,@{N="GruposDiretos";E={@($_.memberOf).Count}},@{N="MemberOf";E={$_.memberOf -join "; "}}|Format-List;Write-Host "=== AUDITORIA ===";$audit="C:\Automind.CadastroColaboradores\Logs\ProvisioningAudit.jsonl";Write-Host "ARQUIVO EXISTE:" (Test-Path $audit);if(Test-Path $audit){Get-Content $audit -Tail 20|ForEach-Object {$o=$_|ConvertFrom-Json;[pscustomobject]@{Action=$o.action;Status=$o.status;Chamado=$o.chamado;Operator=$o.operator;TechnicalIdentity=$o.technicalIdentity;DN=$o.distinguishedName;Groups=(@($o.groups)-join "; ")}}|Format-Table -AutoSize};Write-Host "=== GUID MEMBER ===";$schema=(Get-ADRootDSE).schemaNamingContext;$m=Get-ADObject -SearchBase $schema -LDAPFilter "(lDAPDisplayName=member)" -Properties schemaIDGUID,lDAPDisplayName;[pscustomobject]@{Atributo=$m.lDAPDisplayName;GUID=(New-Object Guid (,$m.schemaIDGUID))}|Format-Table -AutoSize;Write-Host "=== 8 GRUPOS ===";$groups=Get-ADGroup -Filter * -SearchBase "OU=Automind,DC=automind,DC=com,DC=br" -Properties GroupCategory,GroupScope|Where-Object {$names -contains $_.Name};$sid=(Get-ADGroup $tech).SID.Value;$rows=foreach($g in $groups){$acl=Get-Acl ("AD:\"+$g.DistinguishedName);$aces=@($acl.Access|Where-Object {try{$_.IdentityReference.Translate([System.Security.Principal.SecurityIdentifier]).Value -eq $sid}catch{$false}});[pscustomobject]@{Name=$g.Name;Category=$g.GroupCategory;Scope=$g.GroupScope;DN=$g.DistinguishedName;ACEGrupoTecnico=$aces.Count}};$rows|Sort-Object Name|Format-Table -AutoSize;Write-Host "GRUPOS ENCONTRADOS:" @($groups).Count
```

Esperado, mas deve ser confirmado:

- usuario encontrado;
- `Enabled=True` se o fluxo terminou normalmente;
- `PasswordLastSet` preenchido;
- grupos diretos de negocio = 0 neste momento;
- auditoria existe e contem etapas do provisionamento;
- GUID de `member` retornado;
- 8 grupos encontrados;
- `ACEGrupoTecnico=0` em todos os 8 antes da delegacao.

Se qualquer item divergir, parar antes de escrita.

---

## 14. Rollback / contencao atuais

### Bloquear novas criacoes do aplicativo

Rollback funcional mais rapido:

`Automind:Mode = ReadOnly`

Depois publicar/recarregar apenas o necessario pelo fluxo normal de release. Isso bloqueia novos `PilotWrite` sem apagar objetos existentes.

### Conter o usuario ficticio atual

Somente se houver anomalia e com autorizacao para a alteracao real:

```powershell
Disable-ADAccount -Identity "CN=Teste Provisionamento Automind,OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br"
```

Validacao:

```powershell
Get-ADUser -Identity "CN=Teste Provisionamento Automind,OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br" -Properties Enabled|Select-Object Name,SamAccountName,Enabled,DistinguishedName|Format-List
```

Exclusao do usuario ficticio somente com autorizacao separada.

### Rollback da delegacao da OU

Remover somente as 16 ACEs criadas pelo projeto, usando as regras exatas e `RemoveAccessRuleSpecific`. Nunca aplicar o SDDL antigo cegamente sobre a ACL atual.

### Rollback da associacao gMSA -> grupo tecnico

Somente depois de remover delegacoes que dependam dela:

```powershell
$svc=Get-ADServiceAccount "gMSA_CadColab";Remove-ADGroupMember -Identity "SG_CadastroColaboradores_AD_Writer" -Members $svc -Confirm:$false
```

### Rollback local da gMSA

Somente quando nenhum App Pool/servico/tarefa estiver usando:

```powershell
Uninstall-ADServiceAccount -Identity "gMSA_CadColab"
```

Nao remover KDS Root Key.

---

## 15. SMTP / e-mail

Regra existente fora deste aplicativo:

- UPN/mail corporativo: `@automind.com.br`
- SMTP principal futuro: `SMTP:<login>@automind.co`
- SMTP secundario futuro: `smtp:<login>@automind.com.br`

Existe script diario separado que administra `proxyAddresses`.

Por isso o aplicativo **nao deve competir com essa automacao neste momento**.

---

## 16. Microsoft 365

Pausado por decisao do usuario.

Ja foi validado anteriormente:

- conectividade 443 com Microsoft;
- tenant ID: `9ab05ca8-1779-410b-ae61-82dbd20810f3`;
- Graph `/subscribedSkus` respondeu 401 sem token, indicando endpoint acessivel;
- nao retomar licenciamento/Graph sem pedido do usuario.

---

## 17. CriaMovePastas - NAO misturar com CadColab

Existe sistema legado separado `CriaMovePastas` com contas/permissoes antigas.

Regras:

- nao reutilizar `CriaMovePastas` para CadColab;
- nao copiar ACLs/delegacoes antigas desse projeto;
- nao usar conta pessoal elevada no App Pool do CadColab;
- a divida tecnica de `CriaMovePastas` deve ser tratada em fase separada.

---

## 18. Resumo curtissimo para o proximo agente

Estado neste instante:

- `CadColab v0.1.3` implantado;
- `PilotWrite` ativo;
- App Pool roda como `AUTOMIND\gMSA_CadColab$`;
- OU de escrita = somente `07.Outros`;
- usuario ficticio `teste.cadcolab` **ja foi criado com sucesso** em `07.Outros`;
- cargo/department/company/manager foram gravados;
- grupos de negocio **nao foram gravados**, por design, porque `GroupWritesEnabled=false`;
- tela continua sugerindo/marcando os 8 grupos comuns ao cargo;
- proxima fase = delegar `Write member` somente nos 8 grupos aprovados e preparar `v0.1.4`;
- antes de qualquer nova escrita, executar o comando corrigido da Secao 13;
- o comando anterior falhou apenas por parser e nao alterou nada;
- nao apagar nem recriar o usuario ficticio ate decisao explicita;
- nao habilitar escrita de grupos antes da ACL e rollback estarem testados em memoria.

---

## 19. Frase recomendada para abrir o novo chat

**"Continue o projeto CadColab usando o arquivo de contexto anexado como fonte principal. Estamos depois do primeiro PilotWrite real: o usuario `teste.cadcolab` ja existe em `07.Outros`, mas os 8 grupos ainda nao foram escritos. Comece pelo comando somente-leitura corrigido da Secao 13 e nao repita etapas anteriores."**
