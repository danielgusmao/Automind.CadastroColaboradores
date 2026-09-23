# CHECKPOINT V005 - Microsoft 365 / Entra: sessao PowerShell limpa validada

> Registro somente de diagnostico. Nenhum codigo, IIS, SCHANNEL, perfil, modulo PowerShell, App Registration ou configuracao de servidor foi alterado.

## 2026-09-23 - Resultado do teste em novo powershell.exe -NoProfile

Contexto:
- servidor da aplicacao/IIS: `10.1.2.21` (`SV052022-6121`);
- a sessao PowerShell original possuia `[Net.ServicePointManager]::ServerCertificateValidationCallback` customizado;
- os quatro profiles padrao pesquisados anteriormente nao existiam;
- `curl.exe` ja havia obtido HTTP 200 no endpoint OIDC do Microsoft Entra.

Resultados recebidos:
1. nova instancia `powershell.exe -NoProfile`:
   - `ServerCertificateValidationCallback: NULL`;
   - portanto a nova sessao nasce com o valor padrao, sem o callback customizado observado no processo anterior.
2. chamada HTTPS executada na mesma instancia limpa, com TLS 1.2:
   - `StatusCode: 200`;
   - `StatusDescription: OK`;
   - endpoint consultado: `https://login.microsoftonline.com/automind.com.br/v2.0/.well-known/openid-configuration`.

Conclusoes factuais:
- conectividade HTTP/TLS do servidor `10.1.2.21` com Microsoft Entra esta funcional;
- nao ha evidencia que justifique alterar SCHANNEL, TLS persistente, proxy ou certificados do servidor;
- a falha de `Invoke-WebRequest`/`Invoke-RestMethod` vista anteriormente estava associada ao estado da sessao PowerShell que possuia callback customizado de validacao de certificado;
- a origem exata de quem configurou esse callback na sessao anterior nao foi determinada e nao deve ser inferida sem teste;
- uma sessao limpa consegue consumir o endpoint OIDC normalmente;
- Microsoft Graph PowerShell continua nao instalado e nenhuma instalacao foi autorizada.

## Convencao de nome do pacote entregue

A pedido do responsavel, os ZIPs de continuidade deixam de receber nomes descritivos extensos como `...-checkpoint-M365-runspace-callback.zip`.

Padrao adotado para os pacotes entregues:

`Automind.CadastroColaboradores-AAAA-MM-DD-X.Y.Z.zip`

Versao inicial desta convencao:

`Automind.CadastroColaboradores-23-09-2026-0.0.1.zip`

Para as proximas entregas da mesma linha de trabalho, incrementar a versao do pacote (`0.0.2`, `0.0.3`, ...), preservando a data conforme aplicavel.

Observacao: o versionamento interno de checkpoints (`CHECKPOINT-V001`, `V002`, etc.) continua servindo apenas como historico documental e e independente do nome/versao do ZIP entregue.

## Estado atual do bloco Microsoft 365

Validado ate aqui:
- TCP/443 para `graph.microsoft.com` e `login.microsoftonline.com`;
- HTTPS/OIDC do Entra via `curl.exe` e Windows PowerShell 5.1 em sessao limpa;
- .NET Framework 4.7.2;
- Windows PowerShell 5.1.17763.8146;
- `PowerShellGet 1.0.0.1` e `PackageManagement 1.0.0.1` existentes;
- Microsoft Graph PowerShell SDK ausente.

Ainda nao realizado/autorizado:
- instalacao ou atualizacao de PowerShellGet/Microsoft.Graph/PowerShell 7;
- criacao de App Registration;
- criacao de secret ou certificado;
- concessao de permissao Graph;
- autenticacao App-only;
- consulta autenticada de `/subscribedSkus`.

## Proxima etapa tecnica prevista

Continuar somente com testes de leitura sem instalar componentes:
- obter/confirmar Tenant ID pelo documento OIDC publico usando sessao limpa ou `curl.exe`;
- validar resposta HTTP do endpoint Microsoft Graph (sem token, o retorno esperado e de autenticacao/autorizacao, o que confirma o caminho HTTPS da API);
- somente depois discutir, com consentimento explicito, a identidade de aplicacao necessaria para teste autenticado de licencas.
