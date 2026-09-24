# CHECKPOINT V020 — Conta legada e worker IIS

Data: 23/09/2026

## Resultado dos testes

### Active Directory

A conta `AUTOMIND\CriaMovePastas` existe e está habilitada.

Estado observado:

- `SamAccountName`: `CriaMovePastas`;
- `PasswordNeverExpires`: `True`;
- membro de `_CriaMovePastas`;
- membro de `_GAP1`;
- sem SPNs configurados no resultado consultado.

Regra: não alterar senha, grupos ou objeto desta conta durante a implantação da gMSA até concluir o mapeamento de dependências. A credencial apareceu em uma saída de diagnóstico; tratar como dado sensível e planejar rotação separada, sem misturar com a implantação do CadastroColaboradores.

### Servidor 10.1.2.21

A ACL de `C:\Automind.CadastroColaboradores` foi capturada. O acesso local é herdado e inclui `SYSTEM`, `Administrators`, `Users` e a conta administrativa usada na manutenção.

No momento da consulta não havia processo `w3wp.exe` ativo para o App Pool `CadastroColaboradores`. Isso é compatível com o App Pool configurado como `OnDemand` e com encerramento por ociosidade.

Como não havia worker ativo, a identidade efetiva do processo ainda não foi comprovada em runtime.

Nenhuma alteração foi realizada.

## Próximos testes — somente leitura

1. confirmar saúde de replicação do AD antes de qualquer criação de objeto;
2. iniciar o site por acesso normal e consultar o proprietário do worker process;
3. mapear usos atuais de `CriaMovePastas` em serviços, tarefas agendadas e App Pools sem exibir senhas.

## Regra operacional

Nenhuma rotação de senha, alteração de App Pool, criação de gMSA/grupo ou ACL será executada antes desses baselines.
