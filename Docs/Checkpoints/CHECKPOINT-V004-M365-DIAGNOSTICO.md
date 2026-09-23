# CHECKPOINT V004 - Microsoft 365 / Entra: diagnostico inicial

> Versao corrente da investigacao Microsoft 365 / Entra. Nao contem alteracoes de codigo, IIS ou servidor; somente resultados de testes e decisoes.

## 2026-09-23 - Microsoft 365 / Entra - conectividade do servidor 10.1.2.21 e inventario do Graph PowerShell

Contexto:
- testes executados no servidor IIS/aplicacao `10.1.2.21` (`SV052022-6121`), nao no controlador de dominio;
- objetivo desta rodada: validar apenas conectividade de saida para Microsoft Graph/Entra e verificar se o Microsoft Graph PowerShell SDK ja estava instalado;
- nenhum modulo foi instalado e nenhuma configuracao do servidor foi alterada.

Resultados:
1. `Test-NetConnection graph.microsoft.com -Port 443`:
   - `RemoteAddress=40.126.45.29`;
   - `RemotePort=443`;
   - `InterfaceAlias=NIC TEAM`;
   - `SourceAddress=10.1.2.21`;
   - `TcpTestSucceeded=True`.
2. `Test-NetConnection login.microsoftonline.com -Port 443`:
   - `RemoteAddress=20.190.173.66`;
   - `RemotePort=443`;
   - `InterfaceAlias=NIC TEAM`;
   - `SourceAddress=10.1.2.21`;
   - `TcpTestSucceeded=True`.
3. inventario do PowerShell/Graph:
   - Windows PowerShell: `5.1.17763.8146`;
   - nenhum resultado para `Microsoft.Graph.Authentication`;
   - nenhum resultado para `Microsoft.Graph.Identity.DirectoryManagement`;
   - nenhum resultado para `Connect-MgGraph`;
   - nenhum resultado para `Get-MgSubscribedSku`;
   - conclusao factual: Microsoft Graph PowerShell SDK nao esta instalado/disponivel nesse servidor na consulta realizada.

Validacao em documentacao oficial Microsoft consultada apos os testes:
- PowerShell 7+ e recomendado para Microsoft Graph PowerShell SDK;
- Windows PowerShell 5.1 permanece suportado, desde que os pre-requisitos estejam presentes, incluindo .NET Framework 4.7.2+; a documentacao tambem pede PowerShellGet atualizado e Execution Policy `RemoteSigned` ou menos restritiva para uso do SDK;
- para `GET /subscribedSkus`, a permissao minima documentada para acesso Application/App-only e `LicenseAssignment.Read.All`;
- `LicenseAssignment.Read.All` em modo Application exige consentimento administrativo;
- a aplicacao futura nao deve depender da conta administrativa pessoal do responsavel; autenticacao tecnica/App-only continua sendo a direcao preferida para o bloco Microsoft 365, sujeita a validacao e aprovacao antes de criar App Registration, certificado ou segredo.

Decisao operacional mantida:
- nao instalar `Microsoft.Graph`, `PowerShellGet`, PowerShell 7 ou qualquer outro componente sem consentimento explicito;
- nao criar App Registration, segredo, certificado ou conceder permissao no Entra sem consentimento explicito;
- antes de propor instalacao, validar por leitura os pre-requisitos existentes no `10.1.2.21` e identificar o tenant via metadados publicos OIDC, sem autenticacao.

Proxima rodada de testes prevista, somente leitura e no maximo 3 comandos:
1. consultar versao do .NET Framework instalada;
2. consultar PowerShellGet/PackageManagement, repositorios e Execution Policy, sem alterar nada;
3. consultar o documento OIDC publico para `automind.com.br` e identificar `issuer`/`token_endpoint`/Tenant ID, sem login.

## 2026-09-23 - Microsoft 365 / Entra - pre-requisitos locais e falha HTTPS no Windows PowerShell 5.1

Contexto:
- testes executados no servidor IIS/aplicacao `10.1.2.21` (`SV052022-6121`);
- nenhum componente foi instalado e nenhuma configuracao persistente foi alterada;
- objetivo: validar pre-requisitos locais para acesso ao Microsoft Graph e ao tenant Entra.

Resultados:
1. .NET Framework:
   - `Version=4.7.03190`;
   - `Release=461814`;
   - conforme tabela oficial Microsoft, `Release 461814` corresponde ao .NET Framework 4.7.2 em sistemas operacionais aplicaveis;
   - portanto o requisito minimo .NET Framework 4.7.2 para Windows PowerShell 5.1 esta atendido.
2. PowerShellGet / PackageManagement / repositorios / Execution Policy:
   - `PowerShellGet 1.0.0.1` em `C:\Program Files\WindowsPowerShell\Modules\PowerShellGet\1.0.0.1\PowerShellGet.psd1`;
   - `PackageManagement 1.0.0.1` em `C:\Program Files\WindowsPowerShell\Modules\PackageManagement\1.0.0.1\PackageManagement.psd1`;
   - `Get-PSRepository` nao conseguiu obter a lista de providers e retornou `Unable to download the list of available providers` e `Unable to find module repositories`;
   - `LocalMachine=RemoteSigned`; demais escopos `Undefined`;
   - nenhuma alteracao foi feita na Execution Policy ou nos repositorios.
3. consulta OIDC publica via `Invoke-RestMethod`:
   - `https://login.microsoftonline.com/automind.com.br/v2.0/.well-known/openid-configuration` falhou com `The underlying connection was closed: An unexpected error occurred on a send`;
   - por isso `Issuer`, `AuthorizationEndpoint`, `TokenEndpoint` e `TenantId` ficaram vazios;
   - como `Test-NetConnection login.microsoftonline.com -Port 443` ja havia retornado sucesso, a falha atual esta acima da camada TCP e precisa de diagnostico de TLS/HTTPS antes de qualquer instalacao.

Validacao em documentacao oficial Microsoft apos os testes:
- a PowerShell Gallery exige TLS 1.2 ou superior;
- a Microsoft orienta explicitamente habilitar TLS 1.2 na sessao do Windows PowerShell 5.1 antes de usar a PowerShell Gallery;
- PowerShellGet v1.x nao e mais suportado; o ambiente possui `PowerShellGet 1.0.0.1`;
- nenhuma atualizacao sera feita sem autorizacao explicita.

Proxima rodada de diagnostico, somente leitura ou alteracao temporaria limitada ao processo atual do PowerShell:
1. consultar o valor atual de `[Net.ServicePointManager]::SecurityProtocol`;
2. consultar configuracao persistente de TLS 1.2 Client no SCHANNEL via registro, sem alterar o registro;
3. testar novamente o endpoint OIDC forçando TLS 1.2 apenas na sessao/processo atual e restaurando o valor original ao final; nenhuma configuracao do servidor sera persistida.

## 2026-09-23 - Microsoft 365 / Entra - TLS 1.2 presente, falha HTTPS permanece

Contexto:
- testes executados no servidor IIS/aplicacao `10.1.2.21` (`SV052022-6121`);
- nenhum componente foi instalado e nenhuma configuracao persistente foi alterada;
- objetivo: validar se a falha de `Invoke-RestMethod` era causada apenas pela ausencia de TLS 1.2 na sessao ou por configuracao persistente simples do SCHANNEL.

Resultados:
1. `[Net.ServicePointManager]::SecurityProtocol` retornou `Tls, Tls11, Tls12`;
   - portanto TLS 1.2 ja esta disponivel/anunciado no processo do Windows PowerShell 5.1 consultado.
2. a chave `HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client` nao existe no registro;
   - nenhuma chave foi criada ou alterada;
   - ausencia dessa chave nao sera interpretada isoladamente como TLS 1.2 desabilitado, pois o processo ja reporta `Tls12` e a configuracao padrao do Windows pode operar sem subchave explicita.
3. teste do endpoint OIDC Microsoft Entra forçando `[Net.SecurityProtocolType]::Tls12` apenas no processo atual continuou falhando com `The underlying connection was closed: An unexpected error occurred on a send`;
   - ao final do comando o valor original de `SecurityProtocol` foi restaurado;
   - nenhuma configuracao persistente foi alterada.

Conclusao factual desta rodada:
- a hipotese simples de que a sessao falhava apenas por nao oferecer TLS 1.2 nao foi confirmada;
- TCP 443 para `login.microsoftonline.com` e `graph.microsoft.com` continua previamente validado;
- a falha permanece na camada HTTPS/TLS ou em componentes relacionados ao cliente Windows PowerShell/.NET/rede intermediaria e precisa ser isolada por novos testes antes de qualquer alteracao.

Documentacao oficial Microsoft revisada apos os testes:
- PowerShell Gallery exige TLS 1.2 ou superior;
- PowerShellGet 1.0.0.1 do Windows PowerShell 5.1 esta fora de suporte e deve ser atualizado antes de uso produtivo, mas nenhuma atualizacao sera feita sem autorizacao;
- `netsh winhttp show proxy` e um comando oficial somente de consulta para exibir a configuracao WinHTTP;
- eventos `Schannel` do log System podem registrar falhas de handshake/certificado; qualquer habilitacao adicional de logging exigiria alteracao de registro e nao sera feita sem autorizacao.

Proxima rodada de diagnostico prevista, no maximo 3 testes e sem alteracao persistente:
1. consultar configuracao de proxy WinHTTP e WinINET do usuario atual;
2. testar o mesmo endpoint OIDC com `curl.exe`, se o executavel ja existir, para separar o comportamento do cliente curl/Schannel do Windows PowerShell/.NET;
3. capturar a cadeia completa de excecoes/InnerException do `Invoke-WebRequest` ou `Invoke-RestMethod` com TLS 1.2 forçado apenas na sessao.

Regra mantida:
- nao alterar SCHANNEL, proxy, certificados, PowerShellGet, Microsoft.Graph, PowerShell 7, App Registration ou qualquer configuracao do servidor sem consentimento explicito.

## 2026-09-23 - Microsoft 365 / Entra - curl/Schannel OK, falha restrita ao Windows PowerShell/.NET

Contexto:
- testes executados no servidor IIS/aplicacao `10.1.2.21` (`SV052022-6121`);
- nenhum componente foi instalado e nenhuma configuracao persistente foi alterada;
- objetivo: separar problema de rede/proxy/TLS do comportamento do cliente Windows PowerShell 5.1/.NET.

Resultados:
1. Proxy:
   - WinHTTP: `Direct access (no proxy server)`;
   - WinINET do usuario atual: `ProxyEnable=0`, `ProxyServer` vazio, `AutoConfigURL` vazio, `AutoDetect` vazio;
   - portanto nao foi encontrada configuracao explicita de proxy nos escopos consultados.
2. `curl.exe`:
   - o executavel ja existe no servidor;
   - consulta HTTPS a `https://login.microsoftonline.com/automind.com.br/v2.0/.well-known/openid-configuration` com `--tlsv1.2` conectou com sucesso;
   - DNS resolveu `login.microsoftonline.com`;
   - conexao TCP/443 estabelecida;
   - Schannel concluiu a sessao TLS;
   - resposta HTTP `200 OK` recebida do Microsoft Entra;
   - isso confirma que rede, DNS, porta 443 e a pilha TLS/Schannel do Windows conseguem atingir o endpoint.
3. `Invoke-WebRequest` no Windows PowerShell 5.1, com TLS 1.2 forçado apenas na sessao:
   - continuou falhando com `System.Net.WebException: The underlying connection was closed: An unexpected error occurred on a send.`;
   - `WebException.Status=SendFailure`;
   - InnerException: `System.Management.Automation.PSInvalidOperationException: There is no Runspace available to run scripts in this thread.`;
   - a mensagem interna informa que o script block tentado era `$true`.

Conclusao factual desta rodada:
- a falha nao esta em DNS, rota, porta 443 ou capacidade geral do Windows/Schannel de negociar TLS com `login.microsoftonline.com`, pois `curl.exe` obteve `HTTP 200` no mesmo servidor;
- a falha esta restrita ao caminho Windows PowerShell 5.1/.NET usado por `Invoke-WebRequest`/`Invoke-RestMethod` ou a algum estado/configuracao carregado nessa sessao/processo;
- a InnerException envolvendo um script block `$true` justifica verificar se `ServicePointManager.ServerCertificateValidationCallback` foi customizado na sessao ou por perfil/script carregado;
- nenhuma conclusao definitiva sera adotada ate validar esse callback e testar uma nova instancia `powershell.exe -NoProfile`.

Documentacao oficial Microsoft revisada:
- `System.Net.ServicePointManager.ServerCertificateValidationCallback` e o callback usado para validacao personalizada de certificados de servidor e seu valor padrao e `null`;
- `powershell.exe -NoProfile` inicia o Windows PowerShell sem carregar perfis, permitindo isolar configuracoes inseridas por perfil.

Proxima rodada de diagnostico prevista, no maximo 3 testes e sem alteracao persistente:
1. consultar o valor/metadata atual de `[Net.ServicePointManager]::ServerCertificateValidationCallback`;
2. executar o mesmo `Invoke-WebRequest` em um novo `powershell.exe -NoProfile`, sem alterar o processo atual;
3. localizar nos perfis PowerShell existentes qualquer referencia a `ServerCertificateValidationCallback`, `CertificatePolicy` ou script blocks de validacao de certificado, somente leitura.

Regra mantida:
- nao limpar callback, editar perfil, alterar certificado, instalar Microsoft.Graph/PowerShellGet/PowerShell 7 ou mudar qualquer configuracao do servidor sem consentimento explicito.

## 2026-09-23 - Callback de certificado confirmado na sessao; teste NoProfile ainda inconclusivo

### Resultado recebido

No servidor `10.1.2.21`, na sessao atual do Windows PowerShell 5.1:

- `[Net.ServicePointManager]::ServerCertificateValidationCallback` esta **CONFIGURADO**;
- metodo retornado: `Boolean lambda_method(...)`;
- `TargetType`: `System.Runtime.CompilerServices.Closure`;
- o callback portanto nao esta no valor padrao `null` nesta sessao.

O teste iniciado com `powershell.exe -NoProfile` **nao chegou a executar a chamada HTTPS**, pois ocorreu erro de parser no comando aninhado (`Missing type name after '['`). Portanto esse resultado nao pode ser usado para afirmar se uma sessao limpa funciona ou falha.

Os quatro caminhos de profile verificados nao existem:

- `C:\Windows\System32\WindowsPowerShell\v1.0\profile.ps1`;
- `C:\Windows\System32\WindowsPowerShell\v1.0\Microsoft.PowerShell_profile.ps1`;
- `C:\Users\daniel.gusmao.d\Documents\WindowsPowerShell\profile.ps1`;
- `C:\Users\daniel.gusmao.d\Documents\WindowsPowerShell\Microsoft.PowerShell_profile.ps1`.

### Interpretacao permitida neste ponto

- rede, DNS, TCP 443 e Schannel ja foram validados anteriormente pelo `curl.exe` com HTTP 200;
- a falha observada continua restrita ao caminho Windows PowerShell 5.1/.NET;
- existe um callback customizado de validacao de certificado na sessao atual;
- nao ha evidencia de que esse callback venha dos quatro profiles padrao testados;
- ainda falta repetir a verificacao em um processo `powershell.exe -NoProfile` com comando mais simples e sem erro de quoting/parser;
- nao remover, limpar ou substituir o callback sem autorizacao explicita.

### Regra operacional reforcada

Nenhuma instalacao, alteracao de SCHANNEL, limpeza de callback, edicao de profile, instalacao de Microsoft.Graph/PowerShell 7 ou criacao de App Registration deve ocorrer sem consentimento explicito do responsavel.
