# CHECKPOINT V043 - ACL piloto validada com 16 ACEs exatas

Data: 23/09/2026

## Escopo

OU piloto:

`OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

Principal tecnico:

`AUTOMIND\SG_CadastroColaboradores_AD_Writer`

## Resultado da validacao detalhada

A consulta foi somente leitura e confirmou uma ACE para cada atributo aprovado:

| Atributo | GUID | ACEs |
|---|---|---:|
| `givenName` | `f0f8ff8e-1191-11d0-a060-00aa006c33ed` | 1 |
| `sn` | `bf967a41-0de6-11d0-a285-00aa003049e2` | 1 |
| `displayName` | `bf967953-0de6-11d0-a285-00aa003049e2` | 1 |
| `description` | `bf967950-0de6-11d0-a285-00aa003049e2` | 1 |
| `physicalDeliveryOfficeName` | `bf9679f7-0de6-11d0-a285-00aa003049e2` | 1 |
| `telephoneNumber` | `bf967a49-0de6-11d0-a285-00aa003049e2` | 1 |
| `mail` | `bf967961-0de6-11d0-a285-00aa003049e2` | 1 |
| `title` | `bf967a55-0de6-11d0-a285-00aa003049e2` | 1 |
| `department` | `bf96794f-0de6-11d0-a285-00aa003049e2` | 1 |
| `company` | `f0f8ff88-1191-11d0-a060-00aa006c33ed` | 1 |
| `manager` | `bf9679b5-0de6-11d0-a285-00aa003049e2` | 1 |
| `userPrincipalName` | `28630ebb-41d5-11d1-a9c1-0000f80367c1` | 1 |
| `sAMAccountName` | `3e0abfd0-126a-11d0-a060-00aa006c33ed` | 1 |
| `userAccountControl` | `bf967a68-0de6-11d0-a285-00aa003049e2` | 1 |

Direitos adicionais:

- `CreateChild User`: 1;
- `Reset Password`: 1;
- total de ACEs do grupo tecnico: 16.

## Conclusao

A aparente duplicidade de `company` observada no V042 nao existe na ACL real. `givenName` tambem esta presente exatamente uma vez. A divergencia foi somente de exibicao/copia da primeira listagem.

Nenhuma correcao foi aplicada e nenhum rollback foi executado. A ACL piloto permanece valida em `07.Outros`.

## Estado apos esta etapa

- gMSA instalada no servidor e membro do grupo tecnico;
- ACL piloto aplicada somente em `07.Outros`;
- nenhuma delegacao em outras OUs;
- nenhum `Write Members` em grupos;
- App Pool ainda em `ApplicationPoolIdentity`;
- aplicacao ainda sem escrita de usuario habilitada.

## Proximo passo

Executar somente leitura no servidor `10.1.2.21` para confirmar que o AD consultado pelo servidor ja apresenta as 16 ACEs da OU piloto. Somente depois preparar baseline e rollback da futura troca do App Pool para a gMSA.
