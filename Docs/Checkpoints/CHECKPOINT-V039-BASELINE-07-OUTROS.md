# CHECKPOINT V039 - Baseline da OU piloto 07.Outros

Data: 23/09/2026

## Objetivo

Registrar o estado da OU escolhida para o primeiro piloto de delegacao real antes de qualquer `Set-Acl`.

OU piloto:

`OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

## Baseline confirmado

- ACEs: `40`
- Owner: `AUTOMIND\Domain Admins`
- Heranca bloqueada: `False`
- Nenhuma delegacao do projeto foi aplicada nesta etapa.

Arquivos locais de recuperacao criados no servidor AD:

- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-ACL-before.xml`
  - SHA-256: `2F8C1F0AB538CAEFAD17F3CC59631D87F741860270DD1F11B113AD66EB25106D`
- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-SDDL-before.txt`
  - SHA-256: `53FB30862FDB19A069F25BC7C182949315AA00D85CAD229D0EEC61931CFC64B3`

## Regra de seguranca

O arquivo salvo e evidencia de baseline. O rollback preferencial continua sendo remover somente as ACEs adicionadas pelo projeto, e nao restaurar cegamente a ACL completa.

## Proximo passo

Repetir a simulacao das 16 ACEs exclusivamente em memoria usando `07.Outros` como alvo. Nao executar `Set-Acl` ainda.
