# Changelog - Automind TOPdesk Bridge

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
