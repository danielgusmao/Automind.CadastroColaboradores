# CHECKPOINT V044 - IIS/gMSA: pre-requisitos e etapa de backup

Data: 23/09/2026.

## Validado

- `CadastroColaboradores` continua em `ApplicationPoolIdentity`.
- `LogonType=LogonBatch`.
- `manualGroupMembership=False`.
- `LoadUserProfile=True`.
- anonimo habilitado como `IUSR`.
- Windows Authentication desabilitada.
- `IIS_IUSRS` sem membros explicitamente listados.
- `SeBatchLogonRight` inclui `S-1-5-32-568` (`IIS_IUSRS`).
- `SeDenyBatchLogonRight` nao configurado.
- gMSA instalada e valida no servidor.

## Decisao

Nao adicionar a gMSA explicitamente a `IIS_IUSRS` e nao alterar user rights preventivamente. Manter o comportamento padrao do IIS com `manualGroupMembership=False` e validar no piloto real do worker.

## Proxima etapa

Criar backup local do IIS e baseline nao secreto antes da primeira troca real para `AUTOMIND\gMSA_CadColab$`.

A troca continua bloqueada ate existir autorizacao explicita e rollback preparado.
