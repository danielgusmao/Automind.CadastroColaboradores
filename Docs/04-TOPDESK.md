# TOPdesk

Endpoint utilizado:
`GET /tas/api/incidents/number/{numero}`

O chamado `CRIAÇÃO DE USUÁRIO` entrega o formulário principalmente no campo `request`.

Chamados reais utilizados no desenho e nos testes:
- `I2609-0223`
- `I2508-0393`
- `I2603-0141`

## Autenticação aprovada para esta fase

A integração de leitura do TOPdesk utiliza a extensão `Automind TOPdesk Bridge`, instalada no Brave ou Chrome do operador.

A extensão reaproveita a sessão SAML já autenticada do próprio operador em `automind.topdesk.net`.

Não serão utilizados nesta fase:
- senha TOPdesk armazenada pelo Cadastro;
- Application Token;
- conta técnica TOPdesk.

Comportamento validado:
- sessão SAML ativa -> endpoint retorna HTTP 200 e JSON do incidente;
- sessão ausente/expirada -> endpoint retorna HTTP 401;
- o site `cadastro.automind.com.br` não consegue ler diretamente o endpoint por CORS;
- a extensão Chromium consegue fazer a leitura com a sessão do operador.

A integração permanece somente leitura. Nenhuma escrita no TOPdesk foi autorizada.
