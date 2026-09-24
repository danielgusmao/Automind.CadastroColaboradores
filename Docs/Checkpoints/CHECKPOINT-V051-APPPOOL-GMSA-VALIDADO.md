# CHECKPOINT V051 - App Pool gMSA validado ponta a ponta

Data: 24/09/2026
Servidor: `SV052022-6121` / `10.1.2.21`

## Estado validado

- App Pool: `CadastroColaboradores`;
- estado: `Started`;
- identidade: `SpecificUser`;
- usuario: `AUTOMIND\gMSA_CadColab$`;
- `Test-ADServiceAccount=True`;
- site `CadastroColaboradores`: `Started`;
- binding: `http/10.1.2.21:80:cadastro.automind.com.br`;
- aplicacao: `CadastroColaboradores/` usando o mesmo App Pool.

## Validacao HTTP

- via IP correto + Host `cadastro.automind.com.br`: HTTP `200`;
- via FQDN `http://cadastro.automind.com.br/`: HTTP `200`;
- URL final permaneceu `http://cadastro.automind.com.br/`.

## Worker

- worker criado com sucesso;
- PID observado no teste: `5820`;
- owner do processo: `AUTOMIND\gMSA_CadColab$`.

O PID e apenas evidência temporal do teste e pode mudar em recycle futuro.

## Eventos

Na janela consultada nao foram exibidos erros WAS/W3SVC. No log Application foi observado:

- provider: `IIS AspNetCore Module V2`;
- evento: `1032`;
- nivel: `Information`;
- mensagem: aplicacao `C:\Automind.CadastroColaboradores\` iniciada com sucesso.

## Conclusao

A gMSA esta efetivamente executando o worker do App Pool e a aplicacao responde HTTP 200. Nao ha indicacao de necessidade de ACL NTFS adicional, associacao manual a `IIS_IUSRS`, mudanca de direitos locais, `iisreset` ou rollback.

## Contingencia mantida

Permanece disponivel o backup pre-gMSA:

- `CadColab-Pre-gMSA-20260924-082229`.

O rollback primario permanece a restauracao exclusiva do App Pool para `ApplicationPoolIdentity`, somente se uma falha futura comprovadamente relacionada a identidade exigir isso.

## Regra operacional

Testes de leitura/simulacao no mesmo local podem ser consolidados em uma unica linha com varias validacoes. Alteracoes reais continuam uma por vez, sempre com efeito esperado, rollback e validacao preparados antes.
