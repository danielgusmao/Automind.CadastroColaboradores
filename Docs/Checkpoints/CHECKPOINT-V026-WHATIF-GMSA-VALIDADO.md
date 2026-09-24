# CHECKPOINT V026 - WHATIF DA GMSA VALIDADO

Data: 23/09/2026

## Resultado

A simulacao da criacao da identidade tecnica retornou:

`What if: Performing the operation "New" on target "CN=gMSA_CadColab,CN=Managed Service Accounts,DC=automind,DC=com,DC=br".`

O alvo corresponde ao planejado e o `-WhatIf` nao alterou o AD.

## Primeira escrita proposta

Criar somente `gMSA_CadColab$` no container `Managed Service Accounts`, autorizando apenas `SV052022-6121$` a recuperar a senha gerenciada.

Nesta etapa nao:

- instalar a gMSA no servidor;
- alterar o App Pool;
- criar grupo tecnico;
- alterar ACL de OU/grupo;
- reiniciar AD/DC/IIS/servidor.

## Validacao

Apos a criacao, executar apenas `Get-ADServiceAccount` para confirmar os atributos esperados.

## Rollback preparado

Se houver desvio, remover somente `gMSA_CadColab` com `Remove-ADServiceAccount` depois de confirmar que o objeto e o criado nesta etapa.

## Gate

A criacao real depende de autorizacao explicita do operador. Nenhuma escrita foi executada neste checkpoint.
