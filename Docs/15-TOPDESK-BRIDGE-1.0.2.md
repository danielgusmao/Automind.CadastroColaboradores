# TOPdesk Bridge 1.0.2

## Motivo da alteração

A extensão conseguia consultar o TOPdesk pelo popup e retornar HTTP 200, porém a aplicação web não recebia a resposta da extensão.

## Decisão técnica

A versão 1.0.2 elimina a dependência de comunicação página -> extensão por `window.postMessage` ou `CustomEvent` para a importação principal.

O `content.js` da extensão agora captura diretamente o `submit` do formulário marcado com `data-topdesk-import`, consulta o TOPdesk através do `background.js` e, em caso de sucesso, envia o JSON do incidente ao endpoint ASP.NET Core já existente.

Fluxo:

Cadastro -> submit -> content.js -> background.js -> TOPdesk -> content.js -> POST ImportarTopdesk

## Segurança

- Somente GET no TOPdesk.
- Usa a sessão SAML já aberta no navegador.
- Não armazena senha.
- Não utiliza Application Token.
- Não altera TOPdesk, AD, M365 ou Teams.

## Versão

Brave: 1.0.2
Chrome: 1.0.2
