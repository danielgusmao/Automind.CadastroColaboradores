# CHECKPOINT V047 - Baseline IIS e NTFS confirmado

Data: 23/09/2026

## Escopo

Servidor: `10.1.2.21` / `SV052022-6121`.

Objetivo: fechar o baseline do App Pool `CadastroColaboradores` e da pasta publicada antes de qualquer troca de identidade para a `gMSA_CadColab$`.

## Resultado do App Pool

Leitura via `WebAdministration`/`appcmd`, sem alteracao:

- pool: `CadastroColaboradores`;
- estado: `Started`;
- `IdentityType`: `ApplicationPoolIdentity`;
- `UserName` configurado: `AUTOMIND\\CriaMovePastas`;
- `LoadUserProfile`: `True`;
- `StartMode`: `OnDemand`;
- runtime vazio;
- pipeline: `Integrated`;
- aplicacao vinculada: `CadastroColaboradores/`;
- nenhum worker ativo no instante da leitura.

Interpretacao: `ApplicationPoolIdentity` continua sendo a identidade efetiva configurada. O valor `AUTOMIND\\CriaMovePastas` permanece residual no campo `UserName` e nao deve ser tratado como identidade efetiva enquanto `IdentityType=ApplicationPoolIdentity`.

## ACL NTFS da publicacao

Pasta: `C:\\Automind.CadastroColaboradores`.

ACEs observadas:

- `NT AUTHORITY\\SYSTEM`: `FullControl` herdado;
- `BUILTIN\\Administrators`: `FullControl` herdado;
- `BUILTIN\\Users`: `ReadAndExecute, Synchronize` herdado;
- `BUILTIN\\Users`: `AppendData` herdado em containers;
- `BUILTIN\\Users`: `CreateFiles` herdado em containers;
- `AUTOMIND\\daniel.gusmao.d`: `FullControl` herdado;
- `CREATOR OWNER`: ACE herdada `InheritOnly`.

Diretorios de primeiro nivel:

- `Extensions`;
- `runtimes`;
- `wwwroot`.

## Conclusoes de seguranca

1. Nenhuma alteracao foi feita em IIS, AD ou NTFS.
2. Ainda nao assumir que a gMSA recebe `BUILTIN\\Users`; isso deve ser confirmado no servidor antes de depender dessas ACEs.
3. Ainda nao conceder nova ACL NTFS a `gMSA_CadColab$` sem comprovar necessidade de leitura/escrita da aplicacao.
4. Antes da troca do App Pool, capturar configuracao de autenticacao e um baseline exportavel do pool/site, sem senha em texto claro.
5. Manter rollback preparado para restaurar `ApplicationPoolIdentity` e remover somente ACLs NTFS adicionadas especificamente para a gMSA, caso alguma seja necessaria.
6. Nao executar `iisreset`.

## Proximos testes permitidos

Somente leitura no servidor `10.1.2.21`:

1. confirmar os membros do grupo local `BUILTIN\\Users` e verificar se `Authenticated Users`/outro principal aplicavel explica o acesso da gMSA;
2. ler configuracao de autenticacao da aplicacao e exportar configuracao nao secreta do pool/site;
3. somente depois definir se a gMSA precisa de ACE NTFS propria.
