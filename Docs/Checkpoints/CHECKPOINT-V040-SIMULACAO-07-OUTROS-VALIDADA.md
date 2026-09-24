# CHECKPOINT V040 - Simulacao de delegacao na OU 07.Outros validada

Data: 23/09/2026

## Resultado

A simulacao das 16 ACEs foi executada somente em memoria para:

`OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

Resultado:

- ACL inicial: `40` ACEs;
- ACL simulada: `56` ACEs;
- rollback em memoria: `40` ACEs;
- ACL real no AD: `40` ACEs.

Nenhum `Set-Acl` foi executado.

## Conjunto simulado

- 1 `CreateChild` para objetos da classe `user`;
- 14 `WriteProperty` para os atributos aprovados no contrato de escrita;
- 1 `ExtendedRight` para `Reset Password`;
- sem `GenericWrite`, `GenericAll`, `DeleteChild`, `WriteDacl` ou `WriteOwner`.

## Baseline relacionado

Arquivos locais ja salvos no DC:

- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-ACL-before.xml`
- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-SDDL-before.txt`

Hashes registrados no V039:

- ACL XML: `2F8C1F0AB538CAEFAD17F3CC59631D87F741860270DD1F11B113AD66EB25106D`
- SDDL: `53FB30862FDB19A069F25BC7C182949315AA00D85CAD229D0EEC61931CFC64B3`

## Proxima etapa

A proxima etapa pode ser a primeira delegacao real de ACL, somente na OU `07.Outros`.

Antes da execucao devem estar disponiveis:

1. comando de inclusao das 16 ACEs;
2. validacao imediata apos a escrita;
3. rollback granular removendo somente essas 16 ACEs;
4. baseline local ja salvo para comparacao;
5. autorizacao explicita do operador.

Nao aplicar a mesma delegacao em outras OUs nesta etapa.
