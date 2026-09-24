# CHECKPOINT V034 - Ausencia de ACL e correcao do teste de GUIDs

Data: 23/09/2026

## Resultado

Consulta somente leitura confirmou que o grupo `AUTOMIND\SG_CadastroColaboradores_AD_Writer` ainda nao possui ACE no dominio nem nas OUs avaliadas, incluindo `Engenharia`.

O teste seguinte, destinado a mapear GUIDs da classe `user`, atributos e direito estendido `Reset Password`, falhou no parser do PowerShell com `EmptyPipeElement`.

## Impacto

Nenhuma alteracao ocorreu no Active Directory. O erro aconteceu antes de qualquer operacao de escrita e nenhuma delegacao foi aplicada.

## Proximo passo

Repetir somente o teste de mapeamento de GUIDs com sintaxe corrigida. Permanecem bloqueados `Set-Acl`, delegacoes, criacao de usuarios e mudancas no IIS.
