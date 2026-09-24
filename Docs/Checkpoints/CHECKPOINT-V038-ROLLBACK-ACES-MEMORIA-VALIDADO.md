# CHECKPOINT V038 - Rollback das ACEs validado em memoria

Data: 23/09/2026

## Objetivo

Validar o rollback exato das 16 ACEs planejadas para a OU piloto `Engenharia` sem executar nenhuma escrita no Active Directory.

## Resultado

- ACL inicial: `40` ACEs.
- ACL simulada com as 16 regras: `56` ACEs.
- ACL apos rollback em memoria: `40` ACEs.
- ACL real no AD: `40` ACEs.
- Nenhum `Set-Acl` foi executado.

## Conclusao

A inclusao e a remocao das mesmas 16 regras foram validadas em memoria. A proxima etapa permanece sem escrita no AD: gerar e validar um baseline local da ACL/SDDL da OU antes de qualquer delegacao real.

## Regra de seguranca

A primeira delegacao real da OU `Engenharia` somente pode ocorrer depois de:

1. baseline da ACL salvo;
2. baseline conferido;
3. comando de escrita revisado;
4. rollback especifico revisado;
5. autorizacao explicita do usuario.
