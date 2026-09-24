# CHECKPOINT V036 - Mapeamento de GUIDs validado

Data: 23/09/2026

## Resultado

Consulta somente leitura confirmou o mapeamento explicito `atributo -> GUID` para os 14 atributos do contrato de escrita.

Tambem permanecem confirmados:

- classe `user`: `bf967aba-0de6-11d0-a285-00aa003049e2`;
- `Reset Password`: `00299570-246d-11d0-a768-00aa006e0529`.

## Seguranca

Nenhuma ACL, usuario, grupo, gMSA ou configuracao IIS foi alterada nesta etapa.

## Proximo passo

Montar as ACEs propostas somente em memoria, sem chamar `Set-Acl`, e revisar o conjunto antes de qualquer delegacao real.

A proposta inicial exclui permissoes amplas como `GenericWrite`, `GenericAll`, `WriteDacl`, `WriteOwner` e `DeleteChild`.
