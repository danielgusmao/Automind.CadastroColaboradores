# CHECKPOINT V037 - ACEs em memoria validadas

Data: 23/09/2026

## Resultado

A proposta de delegacao para a OU piloto `Engenharia` foi montada somente em memoria.

Resultado observado:

- ACL real antes: `40` ACEs;
- ACL real depois da simulacao: `40` ACEs;
- regras construidas somente em memoria: `16`;
- nenhuma chamada `Set-Acl` foi executada;
- nenhuma permissao foi adicionada ao Active Directory.

## Conjunto proposto

As 16 regras sao:

- 1 `CreateChild` restrita a classe `user` na OU;
- 14 `WriteProperty`, uma por atributo aprovado no contrato;
- 1 `ExtendedRight` para `Reset Password`, restrita a objetos `user` descendentes.

Permissoes amplas continuam fora da proposta: `GenericWrite`, `GenericAll`, `DeleteChild`, `WriteDacl` e `WriteOwner`.

## Regra de senha no primeiro logon

`pwdLastSet` nao esta no contrato aprovado nem nas 16 ACEs. Portanto, a delegacao atual nao inclui permissao especifica para forcar troca de senha no primeiro logon.

Se essa regra for aprovada no futuro, `pwdLastSet` deve ser tratado como alteracao separada: mapear GUID, atualizar contrato, atualizar rollback, simular novamente e somente depois delegar.

## Proximo passo

Validar em memoria tambem o rollback das 16 ACEs: adicionar as regras a uma copia logica da ACL, remover exatamente as mesmas regras e confirmar retorno ao total inicial, sem `Set-Acl`.
