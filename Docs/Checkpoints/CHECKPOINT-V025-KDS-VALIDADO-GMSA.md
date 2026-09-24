# CHECKPOINT V025 - KDS VALIDADO PARA GMSA

Data: 23/09/2026

## Resultado

A chave KDS existente foi confirmada diretamente no AD:

- KeyId: `74b88b84-4f70-b3b4-a399-127cf4723c34`;
- whenCreated: `12/04/2023 19:03:39`;
- `Test-KdsRootKey`: `True`.

## Decisao

O dominio esta pronto, no requisito KDS, para uma nova gMSA. Nao executar `Add-KdsRootKey`.

## Proxima etapa

Somente simulacao da criacao da `gMSA_CadColab$` com `New-ADServiceAccount -WhatIf`.

A criacao real somente podera ocorrer depois de:

1. revisar a simulacao;
2. registrar o comando definitivo;
3. registrar o rollback (`Remove-ADServiceAccount` somente para o objeto criado pelo projeto);
4. obter autorizacao explicita.

Nenhuma alteracao foi executada neste checkpoint.
