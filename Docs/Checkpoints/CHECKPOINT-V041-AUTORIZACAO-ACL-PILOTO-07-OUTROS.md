# CHECKPOINT V041 - Autorizacao da ACL piloto em 07.Outros

Data: 23/09/2026

## Autorizacao

O operador autorizou explicitamente a primeira delegacao real de ACL do projeto, limitada a:

`OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

Nenhuma outra OU esta autorizada nesta etapa.

## Pre-condicoes ja validadas

- baseline da OU: 40 ACEs;
- owner: `AUTOMIND\Domain Admins`;
- heranca ativa;
- SDDL salvo localmente;
- simulacao em memoria: `40 -> 56 -> 40`;
- grupo tecnico sem ACE previa na OU;
- conjunto validado: 16 ACEs.

## Delegacao autorizada

Principal:

`AUTOMIND\SG_CadastroColaboradores_AD_Writer`

Regras:

- 1 `CreateChild` somente para objetos `user`;
- 14 `WriteProperty` somente para os atributos aprovados no contrato;
- 1 `ExtendedRight` de `Reset Password` somente para objetos `user` descendentes.

Nao incluir:

- `GenericAll`;
- `GenericWrite`;
- `DeleteChild`;
- `Delete`;
- `WriteDacl`;
- `WriteOwner`;
- outras classes ou atributos.

## Protecoes da execucao

O comando de escrita deve abortar antes do `Set-Acl` se:

- o baseline SDDL salvo nao existir;
- a ACL atual for diferente do baseline;
- o grupo tecnico ja possuir ACE na OU;
- a quantidade de regras preparada for diferente de 16.

Antes do `Set-Acl`, deve ser salvo novo snapshot local da ACL/SDDL.

## Validacao esperada

Apos a escrita:

- owner continua `AUTOMIND\Domain Admins`;
- heranca continua ativa;
- ACE total esperado: 56;
- ACEs do grupo tecnico esperadas: 16.

Qualquer divergencia interrompe o procedimento.

## Rollback

Rollback preferencial: remover somente as 16 ACEs adicionadas pelo projeto com `RemoveAccessRuleSpecific` e executar um unico `Set-Acl`.

Nao restaurar cegamente a ACL completa do baseline.

O baseline completo permanece somente como evidencia e recuperacao controlada.
