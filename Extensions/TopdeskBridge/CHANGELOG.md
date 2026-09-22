# Changelog - Automind TOPdesk Bridge

## 1.0.1 - 22/09/2026

Correcao da ponte entre o site e a extensao.

- Substituido o canal principal baseado apenas em `window.postMessage` por eventos DOM dedicados com `requestId`.
- Removida a dependencia da verificacao `event.source === window`, que podia impedir a comunicacao entre a pagina e o content script Chromium em mundo isolado.
- Adicionado marcador DOM `data-automind-topdesk-bridge-version` para diagnostico.
- Mantida compatibilidade com a implementacao anterior via `postMessage`.
- Nenhuma mudanca no acesso ao TOPdesk: continua somente leitura e usando a sessao SAML do operador.

## 1.0.0 - 22/09/2026

Primeira versao aprovada para testes.

- Brave e Chrome.
- Manifest V3.
- Consulta de incidente TOPdesk via sessao SAML existente no navegador.
- Retorno de JSON em `HTTP 200`.
- Deteccao de sessao ausente/expirada por `HTTP 401`.
- Botao para abrir login SAML do TOPdesk.
- Ponte preparada para `cadastro.automind.com.br`.
- Nenhuma escrita no TOPdesk.
- Nenhuma senha ou Application Token armazenado.

### Testes confirmados

- TOPdesk logado -> `I2609-0223` -> HTTP 200.
- TOPdesk deslogado -> `I2609-0223` -> HTTP 401.
- Login SAML realizado -> nova consulta -> HTTP 200.
