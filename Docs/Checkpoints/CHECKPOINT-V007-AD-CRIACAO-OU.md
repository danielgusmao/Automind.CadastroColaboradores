# CHECKPOINT V007 - Active Directory: preparacao da criacao e selecao de OU

Data: 2026-09-23

## Escopo retomado

- Microsoft 365 / Entra fica pausado nesta etapa.
- O foco volta ao Active Directory, primeiro consolidando leituras e regras reais do ambiente e depois preparando um teste controlado de criacao de usuario.
- Nenhum `New-ADUser`, alteracao de atributo, movimentacao de objeto, inclusao em grupo ou delegacao de permissao esta autorizado sem consentimento explicito do responsavel.

## Politica de senha observada no dominio

Resultado de `Get-ADDefaultDomainPasswordPolicy`:

- `ComplexityEnabled`: `False`;
- `MinPasswordLength`: `8`;
- `PasswordHistoryCount`: `2`;
- `MaxPasswordAge`: `181` dias;
- `MinPasswordAge`: `0`;
- `LockoutThreshold`: `10`;
- `LockoutDuration`: `10` minutos;
- `LockoutObservationWindow`: `10` minutos;
- `ReversibleEncryptionEnabled`: `False`.

Esses valores sao evidencias do estado atual do dominio e nao devem ser alterados pelo CadastroColaboradores.

## Amostra de usuarios criados recentemente

A leitura dos 12 usuarios mais recentes sob `OU=Automind,DC=automind,DC=com,DC=br` confirmou criacoes reais em OUs distintas, incluindo:

- `OU=05.Terceiros-Ext,OU=Automind,...`;
- `OU=Engenharia,OU=03.UDN,OU=Automind,...`;
- `OU=SENSE METRICS,OU=03.UDN,OU=Automind,...`;
- `OU=GSTI,OU=04.UDA,OU=Automind,...`;
- `OU=Pessoas,OU=04.UDA,OU=Automind,...`;
- `OU=Inovacao,OU=03.UDN,OU=Automind,...`.

Evidencia funcional importante:

- OU e `Department` nao sao equivalentes e nao devem ser inferidos automaticamente um do outro;
- exemplos reais de usuarios externos possuem `Department` como `GEMED`, `ENGENHARIA` ou `T&S`, mas ficam fisicamente em `OU=05.Terceiros-Ext,OU=Automind,...`;
- portanto a unidade/OU de destino deve ser uma selecao explicita do fluxo de cadastro, sujeita a uma lista de OUs autorizadas.

## Usuario de referencia `fernando.menezes`

A leitura ampliada confirmou:

- `GivenName`: `Fernando`;
- `Surname`: `Menezes`;
- `DisplayName`: `Fernando Menezes`;
- `sAMAccountName`: `fernando.menezes`;
- `UPN`: `fernando.menezes@automind.com.br`;
- `mail`: `fernando.menezes@automind.com.br`;
- `Title`: `Network Administrator`;
- `Department`: `GSTI`;
- `Company`: `Automind`;
- `Manager`: `CN=Fabio dos Santos Daniel,OU=Suporte,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`;
- `Description`: `Network Administrator`;
- `PhysicalDeliveryOfficeName`: `Salvador`;
- `l`: `Salvador`;
- `st`: `Bahia`;
- `c`: `BR`;
- `PasswordNeverExpires`: `True`;
- `PasswordNotRequired`: `False`;
- `proxyAddresses`: primario `SMTP:fernando.menezes@automind.co`, secundario `smtp:fernando.menezes@automind.com.br`.

`employeeID`, `employeeNumber`, endereco e alguns telefones nao estao obrigatoriamente preenchidos; portanto nao devem ser tratados como obrigatorios apenas por existirem no schema do AD.

## Regra de OU para o CadastroColaboradores

Decisao funcional consolidada:

1. O formulario de cadastro deve possuir uma selecao explicita de **Unidade/OU de destino**.
2. A aplicacao nao deve expor automaticamente todas as OUs do dominio.
3. Uma area de Administracao devera manter a allowlist de OUs permitidas para o sistema.
4. Cada item configurado deve persistir no minimo:
   - nome amigavel para exibicao;
   - `DistinguishedName` completo da OU;
   - status ativo/inativo para uso no cadastro.
5. O identificador tecnico da OU e o `DistinguishedName`, nao o `Name`, pois nomes de OU podem se repetir em ramos diferentes.
6. Na futura criacao, a OU selecionada sera usada como `Path` do `New-ADUser` somente depois das validacoes de seguranca e autorizacao explicita para escrita.
7. `Department`, `Title` e `Manager` permanecem atributos separados da OU de destino.
8. A lista deve privilegiar OUs realmente destinadas a contas de colaboradores e excluir OUs tecnicas, de grupos, controladores, infraestrutura ou outras que nao facam parte do cadastro funcional.

## Proximas validacoes somente leitura

Antes de definir a allowlist inicial de OUs, levantar:

- arvore completa de OUs abaixo de `OU=Automind` com `DistinguishedName` e protecao contra exclusao acidental;
- quantidade de usuarios habilitados diretamente em cada OU, para diferenciar OUs organizacionais/folha de OUs puramente estruturais;
- com base nessas duas evidencias, propor ao responsavel a lista inicial de OUs selecionaveis, sem gravar configuracao ainda.

