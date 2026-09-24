# CHECKPOINT V021 - Replicacao AD e identidade efetiva do IIS

Data: 23/09/2026

## Resultado

- `repadmin /replsummary`: 0 falhas nos dois controladores de dominio.
- Maior atraso observado: inferior a 30 minutos.
- App Pool `CadastroColaboradores`: worker ativo confirmado.
- Identidade efetiva do worker: `IIS APPPOOL\CadastroColaboradores`.
- Portanto, o processo atual nao executa como `AUTOMIND\CriaMovePastas`.
- `CriaMovePastas` aparece como `SpecificUser` nos pools `.NET v4.5` e `.NET v4.5 Classic`.
- Nos pools `Calendario`, `AutomindTermos` e `CadastroColaboradores`, `IdentityType=ApplicationPoolIdentity`; o valor residual de `UserName` nao representa a identidade efetiva do worker.
- Nenhum servico ou tarefa agendada usando `CriaMovePastas` foi encontrado no servidor consultado.

## Decisoes

- Nao alterar, remover ou rotacionar `CriaMovePastas` nesta fase.
- Nao usar os campos residuais de `UserName` como evidencia de identidade efetiva; validar sempre pelo owner do `w3wp.exe`.
- A futura gMSA sera uma identidade nova e independente.
- Nenhuma alteracao foi executada neste checkpoint.

## Proximo passo

Continuar somente leitura: validar saude do secure channel do servidor `SV052022-6121` com o dominio e inventariar as gMSAs existentes, principalmente `PrincipalsAllowedToRetrieveManagedPassword`, antes de criar qualquer nova conta de servico.
