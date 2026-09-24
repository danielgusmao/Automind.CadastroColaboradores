# CHECKPOINT V035 - GUIDs de schema retornados, rotulacao pendente

Data: 23/09/2026

## Resultado

Consulta somente leitura executada no AD.

Confirmado:

- classe `user`: `bf967aba-0de6-11d0-a285-00aa003049e2`;
- `Reset Password`: `00299570-246d-11d0-a768-00aa006e0529`.

A consulta tambem retornou 14 GUIDs de atributos na ordem solicitada, mas a coluna `lDAPDisplayName` ficou vazia. Para evitar qualquer delegacao baseada em inferencia, o proximo teste deve emitir cada nome de atributo junto do GUID correspondente.

## Estado

- `gMSA_CadColab$`: criada, instalada e validada;
- `SG_CadastroColaboradores_AD_Writer`: criado;
- gMSA membro do grupo tecnico;
- grupo tecnico ainda sem ACE de escrita;
- IIS ainda sem usar a gMSA;
- nenhuma ACL alterada nesta etapa.

## Regra

Nao aplicar `Set-Acl` antes de validar explicitamente o mapeamento `atributo -> GUID`.
