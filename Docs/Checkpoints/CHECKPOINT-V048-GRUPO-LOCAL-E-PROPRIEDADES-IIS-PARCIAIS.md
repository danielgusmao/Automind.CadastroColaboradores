# CHECKPOINT V048 - Grupo local e propriedades IIS parciais

Data: 23/09/2026

## Objetivo

Confirmar, somente por leitura, a composicao do grupo local `Users`, propriedades da `gMSA_CadColab` e parametros do App Pool `CadastroColaboradores` antes de qualquer troca de identidade no IIS.

## Resultado - servidor 10.1.2.21

### Grupo local BUILTIN\\Users

Membros retornados:

- `AUTOMIND\\Domain Users`;
- `NT AUTHORITY\\Authenticated Users`;
- `NT AUTHORITY\\INTERACTIVE`.

### gMSA

- Name: `gMSA_CadColab`;
- SamAccountName: `gMSA_CadColab$`;
- Enabled: `True`;
- PrimaryGroupID: `515`.

### App Pool CadastroColaboradores

- `IdentityType=ApplicationPoolIdentity`;
- `LogonType=LogonBatch`;
- `LoadUserProfile=True`;
- `SetProfileEnvironment=True`;
- binding HTTP: `10.1.2.21:80:cadastro.automind.com.br`.

A consulta de `anonymousAuthentication` e `windowsAuthentication` retornou objetos `ConfigurationAttribute` em vez dos valores booleanos. Portanto, esses tres campos ainda nao estao validados e devem ser repetidos acessando explicitamente `.Value`.

## NTFS

Nenhuma ACE NTFS foi criada para a gMSA. Como `BUILTIN\\Users` contem `Authenticated Users`, o acesso existente pode ser suficiente para leitura/execucao apos um logon autenticado da gMSA, mas isso nao sera assumido como concluido antes da validacao funcional do worker.

## Estado de seguranca

- nenhuma alteracao no IIS;
- nenhuma alteracao no AD;
- nenhuma alteracao NTFS;
- identidade do pool permanece `ApplicationPoolIdentity`;
- troca para gMSA continua nao executada.

## Proximo passo

1. repetir leitura das configuracoes de autenticacao usando `.Value`;
2. ler `manualGroupMembership` do App Pool e a composicao de `IIS_IUSRS`;
3. somente depois preparar backup local do IIS, comando de troca de identidade e rollback exato.
